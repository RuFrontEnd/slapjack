using Domain.Entities;

namespace Domain.Interfaces.Repositories
{
    public interface IGameRepository
    {
        Task<bool> EnqueuePlayerAsync(string connId, string name);
        Task<bool> DequeuePlayerAsync(string connId);
        Task<long> GetQueueLengthAsync();
        Task<List<PlayerEntity>> PopMatchGroupAsync(int count);
        Task<string> CreateRoomAsync(List<PlayerEntity> players);
    }

}
