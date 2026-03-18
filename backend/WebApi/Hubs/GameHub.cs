using Application.Services;
using Microsoft.AspNetCore.SignalR;
namespace WebAPi.Hubs;

public class GameHub(GameService gameService, ILogger<GameHub> logger) : Hub
{
    //private readonly MatchmakingService _matchmaking;
    public async Task StartMatching(string playerName)
    {
        await gameService.AddPlayerAsync(Context.ConnectionId, playerName, null);

        await gameService.AddPlayerToQueue(Context.ConnectionId, playerName);
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

