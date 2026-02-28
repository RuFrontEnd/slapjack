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

    public async Task<string> JoinOrCreateRoomAsync(string connectionId, string playerName)
    {
        // 1. 找空房
        string roomCode = await gameRepository.GetAvailableRoomAsync();

        // 2. 沒空房就創一個
        if (roomCode == null)
        {
            roomCode = new Random().Next(1000, 9999).ToString();
            await gameRepository.CreateRoomAsync(roomCode);
        }

        // 3. 加入玩家
        await gameRepository.AddPlayerToRoomAsync(roomCode, connectionId, playerName);
        return roomCode;
    }
};

