namespace Domain.Interfaces.Services
{
    public interface IGameService
    {
        Task<bool> ValidateName(string name);
        Task AddPlayerToQueue(string connId, string name);
    }

}
