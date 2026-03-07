using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;
using System.Xml.Linq;
namespace Application.Services;

public class GameService(IGameRepository gameRepository, IHubContext<Hub<IGameService>> hubContext)
{
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

