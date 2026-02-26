using Domain.Interfaces.Repositories;
using Infrastructure.Persistence;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Repositories
{
    public class GameRepository(ApplicationDbContext context, IConnectionMultiplexer redis) : IGameRepository
    {
        private readonly IDatabase redisDB = redis.GetDatabase();
        private const string MatchmakingQueueKey = "match_queue";

        // constructor removed because primary-constructor parameters are used above

        public async Task EnqueuePlayerAsync(string connectionId, string playerName)
        {
            // 將玩家資訊序列化後存入 List 尾端
            var playerInfo = JsonSerializer.Serialize(new { connectionId, playerName });
            Console.WriteLine(playerInfo);
            await redisDB.ListRightPushAsync(MatchmakingQueueKey, playerInfo);
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
