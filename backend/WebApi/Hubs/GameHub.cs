using Application.Services;
using Microsoft.AspNetCore.SignalR;
namespace WebAPi.Hubs;

public class GameHub(GameService gameService, ILogger<GameHub> logger) : Hub
{
    //private readonly MatchmakingService _matchmaking;
    public async Task StartMatching(string playerName)
    {
        await gameService.AddPlayerToQueue(Context.ConnectionId, playerName);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // 呼叫你的 Service 把他從 Redis 隊列中踢掉，避免配對到空殼
        await gameService.RemovePlayerFromQueue(Context.ConnectionId);

        // TODO: close room

        //await gameService.closeRoomAsync()

        await base.OnDisconnectedAsync(exception);
    }
};

