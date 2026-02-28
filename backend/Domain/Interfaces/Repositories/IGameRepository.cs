namespace Domain.Interfaces.Repositories
{
    public interface IGameRepository
    {
        Task<bool> EnqueuePlayerAsync(string connId, string name);
        Task<bool> DequeuePlayerAsync(string connId);
        Task<string> GetAvailableRoomAsync();
        Task<string> CreateRoomAsync(string roomCode);
        Task AddPlayerToRoomAsync(string roomCode, string connectionId, string player);
        Task<long> GetPlayerCountAsync(string roomCode);
    }

}
