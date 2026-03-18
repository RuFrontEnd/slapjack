using Domain.Entities;

namespace Domain.Interfaces.Repositories
{
    public interface IGameRepository
    {
        //Task<bool> GetPlayersAsync(string connId, string name);
        Task<bool> PushPlayerAsync(string connId, string name, string? roomId);
        Task<bool> DeletePlayerAsync(string connId);
        Task<bool> EnqueuePlayerAsync(string connId, string name);
        Task<bool> DequeuePlayerAsync(string connId);
        Task<long> GetMatchingQueueLengthAsync();
        Task<List<PlayerEntity>> PopMatchGroupAsync(int count);
        Task<string> CreateRoomAsync(List<PlayerEntity> players);
        Task<bool> DeleteRoomAsync(string connId);
    }

}
