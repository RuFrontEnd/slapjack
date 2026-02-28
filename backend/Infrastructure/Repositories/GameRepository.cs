using Domain.Entities;
using Domain.Interfaces.Repositories;
using Infrastructure.Persistence;
using StackExchange.Redis;
using System.Numerics;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Infrastructure.Repositories
{
    public class GameRepository(ApplicationDbContext context, IConnectionMultiplexer redis) : IGameRepository
    {
        private readonly IDatabase redisDB = redis.GetDatabase();
        private const string MatchingMapKey = "matching_map";
        private const string MatchingQueueKey = "matching_queue";

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

        public async Task<string> GetAvailableRoomAsync()
        {
            return "";
        }

        public async Task<string> CreateRoomAsync(string roomCode)
        {
            return "";
        }

        public async Task AddPlayerToRoomAsync(string roomCode, string connectionId, string player)
        {
            
        }

        public async Task<long> GetPlayerCountAsync(string roomCode)
        {
            return 123;
        }
    }

}
