using Domain.Entities;
using Domain.Interfaces.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Numerics;
using System.Text.Json;

namespace Infrastructure.Repositories
{
    public class GameRepository(ApplicationDbContext context, IConnectionMultiplexer redis, ILogger<GameRepository> logger) : IGameRepository
    {
        private readonly IDatabase redisDB = redis.GetDatabase();
        private const string PlayerKey = "player";
        private const string MatchingKey = "matching";
        private const string MatchingMapKey = $"{MatchingKey}:matching_map";
        private const string MatchingQueueKey = $"{MatchingKey}:matching_queue";
        private const string RoomKey = "room";

        private static string GetPlayerRedisKey(string connId) => $"{PlayerKey}:{connId}";
        private static string GetRoomRedisKey(string roomId) => $"{RoomKey}:{roomId}";

        private static HashEntry[] BuildPlayerHashEntries(string connId, string name, string? roomId)
            =>
            [
                new HashEntry("connId", connId),
                new HashEntry("name", name),
                new HashEntry("roomId", roomId ?? string.Empty),
            ];

        private static HashEntry[] BuildRoomHashEntries(string playersJson)
            =>
            [
                new HashEntry("players", playersJson),
                new HashEntry("status", "waiting"),
                new HashEntry("createdAt", DateTime.UtcNow.ToString("O")),
            ];

        //public async Task<List<string>> GetPlayersAsync()
        //{
        //    try
        //    {
        //        return await redisDB.ListLengthAsync(MatchingQueueKey);
        //    }
        //    catch (Exception ex)
        //    {
        //        // 記錄錯誤，但回傳 0 讓程式繼續跑
        //        logger.LogError(ex, "讀取 Redis 隊列長度時出錯");
        //        return 0;
        //    }
        //}

        public async Task<bool> PushPlayerAsync(string connId, string name, string? roomId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(connId) || string.IsNullOrWhiteSpace(name))
                {
                    logger.LogWarning("push player skipped: invalid connId or name");
                    return false;
                }

                await redisDB.HashSetAsync(GetPlayerRedisKey(connId), BuildPlayerHashEntries(connId, name, roomId));

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "push player failed");
                return false;
            }
        }

        public async Task<bool> DeletePlayerAsync(string connId)
        {
            try
            {
                await redisDB.KeyDeleteAsync(GetPlayerRedisKey(connId));
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "delete player failed");
                return false;
            }
        }

        public async Task<bool> EnqueuePlayerAsync(string connId, string name)
        {
            var player = new PlayerEntity(connId, name);
            var playerJson = JsonSerializer.Serialize(player);
            var tran = redisDB.CreateTransaction();

            tran.AddCondition(Condition.HashNotExists(MatchingMapKey, connId));

            _ = tran.ListRightPushAsync(MatchingQueueKey, playerJson);
            _ = tran.HashSetAsync(MatchingMapKey, player.ConnectionId, playerJson);

            return await tran.ExecuteAsync();
        }

        public async Task<bool> DequeuePlayerAsync(string connId)
        {
            var playerJson = await redisDB.HashGetAsync(MatchingMapKey, connId);

            if (playerJson.IsNull) return true;

            var tran = redisDB.CreateTransaction();

            _ = tran.ListRemoveAsync(MatchingQueueKey, playerJson);
            _ = tran.HashDeleteAsync(MatchingMapKey, connId);

            return await tran.ExecuteAsync();
        }

        public async Task<long> GetMatchingQueueLengthAsync()
        {
            try
            {
                return await redisDB.ListLengthAsync(MatchingQueueKey);
            }
            catch (Exception ex)
            {
                // 記錄錯誤，但回傳 0 讓程式繼續跑
                logger.LogError(ex, "讀取 Redis 隊列長度時出錯");
                return 0;
            }
        }

        //TOOD: implement transaction
        public async Task<List<PlayerEntity>> PopMatchGroupAsync(int count)
        {
            var matchedPlayers = new List<PlayerEntity>();
            var poppedJsonList = new List<RedisValue>();

            for (int i = 0; i < count; i++)
            {
                var playerJson = await redisDB.ListLeftPopAsync(MatchingQueueKey);

                if (!playerJson.HasValue)
                {
                    break;
                }

                poppedJsonList.Add(playerJson);

                var player = JsonSerializer.Deserialize<PlayerEntity>(playerJson!);
                if (player != null)
                {
                    matchedPlayers.Add(player);
                }
            }

            if (matchedPlayers.Count != count)
            {
                for (int i = poppedJsonList.Count - 1; i >= 0; i--)
                {
                    await redisDB.ListLeftPushAsync(MatchingQueueKey, poppedJsonList[i]);
                }

                return new List<PlayerEntity>();
            }

            for (int i = 0; i < matchedPlayers.Count; i++)
            {
                await redisDB.HashDeleteAsync(MatchingMapKey, matchedPlayers[i].ConnectionId);
            }

            return matchedPlayers;
        }

        public async Task<string> CreateRoomAsync(List<PlayerEntity> players)
        {
            string roomId = Guid.NewGuid().ToString();

            var playersJson = JsonSerializer.Serialize(players);

            string roomKey = GetRoomRedisKey(roomId);

            await redisDB.HashSetAsync(roomKey, BuildRoomHashEntries(playersJson));

            for (int i = 0; i < players.Count; i++)
            {
                await redisDB.HashSetAsync(
                    GetPlayerRedisKey(players[i].ConnectionId),
                    BuildPlayerHashEntries(players[i].ConnectionId, players[i].Name, roomId));
            }

            return roomId;
        }

        public async Task<bool> DeleteRoomAsync(string connId)
        {
            string? roomId = await redisDB.HashGetAsync(GetPlayerRedisKey(connId), "roomId");

            if (string.IsNullOrWhiteSpace(roomId)) return false;

            string roomKey = GetRoomRedisKey(roomId);

            string? roomPlayersJson = await redisDB.HashGetAsync(roomKey, "players");

            if (roomPlayersJson == null) return false;

            var roomPlayers = JsonSerializer.Deserialize<List<PlayerEntity>>(roomPlayersJson!);

            if (roomPlayers == null) return false;

            await redisDB.KeyDeleteAsync(roomKey);

            for (int i = 0; i < roomPlayers.Count; i++)
            {
                await redisDB.HashSetAsync(
                    GetPlayerRedisKey(roomPlayers[i].ConnectionId),
                    BuildPlayerHashEntries(roomPlayers[i].ConnectionId, roomPlayers[i].Name, string.Empty));
            }

            return true;
        }
    }

}


