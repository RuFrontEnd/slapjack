namespace Domain.Interfaces.Repositories
{
    public interface IGameRepository
    {
        Task<string> GetAvailableRoomAsync();
        Task<string> CreateRoomAsync(string roomCode);
        Task AddPlayerToRoomAsync(string roomCode, string connectionId, string player);
        Task<long> GetPlayerCountAsync(string roomCode);
    }

}
