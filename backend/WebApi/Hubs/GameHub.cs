using Application.Services;
using Microsoft.AspNetCore.SignalR;
namespace WebAPi.Hubs;

public class GameHub(GameService gameService) : Hub
{
    //private readonly MatchmakingService _matchmaking;
    public async Task StartMatching(string playerName)
    {
        Console.WriteLine(playerName);
        await gameService.AddPlayerToQueue(Context.ConnectionId, playerName);
    }
};

