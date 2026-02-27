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

    //public override async Task OnDisconnectedAsync(Exception? exception)
    //{
    //    // 這裡的 Context.ConnectionId 就是斷掉的那個人
    //    Console.WriteLine($"diconnect id: {Context.ConnectionId}");

    //    // 呼叫你的 Service 把他從 Redis 隊列中踢掉，避免配對到空殼
    //    await gameService.RemovePlayerFromQueue(Context.ConnectionId);

    //    await base.OnDisconnectedAsync(exception);
    //}
};

