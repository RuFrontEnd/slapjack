using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;
using System.Xml.Linq;
namespace Application.Services;

public class GameService(IGameRepository gameRepository, IHubContext<Hub<IGameService>> hubContext)
{
    public async Task ValidateName(string name)
    {
        //await gameRepository.GetPlayersAsync(name);
    }

    public async Task<bool> AddPlayerAsync(string connId, string name, string? roomId)
    {
        return await gameRepository.PushPlayerAsync(connId, name, roomId);
    }

    public async Task<bool> RemovePlayerAsync(string connId)
    {
        return await gameRepository.DeletePlayerAsync(connId);
    }

    public async Task AddPlayerToQueue(string connId, string name)
    {
        await gameRepository.EnqueuePlayerAsync(connId, name);
    }

    public async Task RemovePlayerFromQueue(string connId)
    {
        await gameRepository.DequeuePlayerAsync(connId);
    }

    public async Task CloseRoomAsync(string connId)
    {
        await gameRepository.DeleteRoomAsync(connId);
    }
};

