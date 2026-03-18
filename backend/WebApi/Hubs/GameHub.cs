using Application.Services;
using Microsoft.AspNetCore.SignalR;
namespace WebAPi.Hubs;

public class GameHub(GameService gameService, ILogger<GameHub> logger) : Hub
{
    //private readonly MatchmakingService _matchmaking;
    public async Task<bool> StartMatching(string playerName)
    {
        var added = await gameService.AddPlayerAsync(Context.ConnectionId, playerName, null);

        if (!added)
        {
            await Clients.Caller.SendAsync("StartMatchingRejected", new
            {
                Code = "DUPLICATED_NAME",
                Message = "Player name already exists"
            });
            return false;
        }

        await gameService.AddPlayerToQueue(Context.ConnectionId, playerName);
        return true;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"disconnect: {Context.ConnectionId}");

        await gameService.RemovePlayerFromQueue(Context.ConnectionId);

        await gameService.CloseRoomAsync(Context.ConnectionId);

        await gameService.RemovePlayerAsync(Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }
};

