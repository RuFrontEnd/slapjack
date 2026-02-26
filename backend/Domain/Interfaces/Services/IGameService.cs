namespace Domain.Interfaces.Services
{
    public interface IGameService
    {
        Task AddPlayerToQueue(string connId, string name);
    }

}
