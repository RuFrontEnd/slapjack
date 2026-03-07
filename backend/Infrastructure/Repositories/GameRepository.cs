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
        private const string MatchingKey = "matching";
        private const string MatchingMapKey = $"{MatchingKey}:matching_map";
        private const string MatchingQueueKey = $"{MatchingKey}:matching_queue";
        private const string RoomKey = "room";
        private const string RoomPlayerMapKey = $"{RoomKey}:room_player_map";

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
            for (int i = 0; i < count; i++)
            {
                var playerJson = await redisDB.ListLeftPopAsync(MatchingQueueKey);

                if (playerJson.HasValue)
                {
                    var player = JsonSerializer.Deserialize<PlayerEntity>(playerJson!);

                    if (player != null)
                    {
                        matchedPlayers.Add(player);

                        await redisDB.HashDeleteAsync(MatchingMapKey, player.ConnectionId);
                    }
                }
            }

            return matchedPlayers;
        }

        public async Task<string> CreateRoomAsync(List<PlayerEntity> players)
        {
            string roomId = Guid.NewGuid().ToString();

            var playersJson = JsonSerializer.Serialize(players);

            string roomKey = $"room:{roomId}";

            await redisDB.HashSetAsync(roomKey, new HashEntry[] {
                new HashEntry("players", playersJson),
                new HashEntry("status", "waiting"),
                new HashEntry("createdAt", DateTime.UtcNow.ToString("O"))
            });

            for (int i = 0; i < players.Count; i++)
            {
                await redisDB.HashSetAsync(RoomPlayerMapKey, new HashEntry[] {
                    new HashEntry(players[i].ConnectionId, roomId),
                });
            }

            return roomKey;
        }

        public async Task<bool> DeleteRoomAsync(string connId)
        {
            string? roomId = await redisDB.HashGetAsync(RoomPlayerMapKey, connId);
            string roomKey = $"room:{roomId}";

            string? roomPlayersJson = await redisDB.HashGetAsync(roomKey, "players");

            if (roomPlayersJson == null) return false;

            var roomPlayers = JsonSerializer.Deserialize<List<PlayerEntity>>(roomPlayersJson!);

            if (roomPlayers == null) return false;

            await redisDB.KeyDeleteAsync(roomKey);

            for (int i = 0; i < roomPlayers.Count; i++)
            {
                await redisDB.HashDeleteAsync(RoomPlayerMapKey, roomPlayers[i].ConnectionId);
            }

            return true;
        }
    }

}


