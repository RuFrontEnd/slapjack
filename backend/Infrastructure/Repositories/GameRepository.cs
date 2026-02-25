using Domain.Interfaces.Repositories;
using Infrastructure.Persistence;
using StackExchange.Redis;

namespace Infrastructure.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly IDatabase _db;
        private const string RoomListKey = "active_rooms";

        // constructor
        public GameRepository(ApplicationDbContext context, IConnectionMultiplexer redis) => _db = redis.GetDatabase();

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
