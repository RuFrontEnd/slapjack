using Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.SignalR;
using WebAPi.Hubs;

namespace Application.Workers;

public class MatchmakingWorker(IServiceProvider serviceProvider, ILogger<MatchmakingWorker> logger, IHubContext<GameHub> hubContext) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var gameRepository = scope.ServiceProvider.GetRequiredService<IGameRepository>();

                    // 1. 先看總共有多少人在排隊
                    long totalWaiting = await gameRepository.GetQueueLengthAsync();

                    // 2. 計算可以組成幾組 (例如 100 人可以組成 25 組)
                    int groupsToCreate = (int)(totalWaiting / 4);

                    if (groupsToCreate > 0)
                    {
                        logger.LogInformation($"發現 {totalWaiting} 人在排隊，準備一口氣建立 {groupsToCreate} 個房間...");

                        // 3. 用迴圈把這 25 組人通通處理掉，不需要等待
                        for (int i = 0; i < groupsToCreate; i++)
                        {
                            var matchedPlayers = await gameRepository.PopMatchGroupAsync(4);

                            if (matchedPlayers.Count == 4)
                            {
                                var roomId = await gameRepository.CreateRoomAsync(matchedPlayers);

                                var connectionIds = matchedPlayers.Select(matchedPlayer => matchedPlayer.ConnectionId);
                                var playerNames = matchedPlayers.Select(matchedPlayer => matchedPlayer.Name);

                                await hubContext.Clients.Clients(connectionIds).SendAsync("ReceiveMatchResult", new
                                {
                                    RoomId = roomId,
                                    Players = playerNames
                                });
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "配對批次處理發生錯誤");
            }

            // 處理完「當前所有」可能的組別後，才休息 2 秒等下一波人進來
            await Task.Delay(2000, stoppingToken);
        }
    }
}
