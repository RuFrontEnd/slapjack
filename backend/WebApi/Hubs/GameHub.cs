using Microsoft.AspNetCore.SignalR;
namespace WebAPi.Hubs;

public class GameHub : Hub
{
    //private readonly MatchmakingService _matchmaking;
    public async Task StartMatching(string playerName)
    {
        //await _matchService.AddPlayerToQueue(Context.ConnectionId, playerName);
    }
};

