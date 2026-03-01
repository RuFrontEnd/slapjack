using Microsoft.Extensions.Hosting;
using Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace WebAPi.Workers;

public class MatchmakingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MatchmakingWorker> _logger;

    public MatchmakingWorker(IServiceProvider serviceProvider, ILogger<MatchmakingWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("配對服務已啟動...");

        // 當服務沒被關閉時，持續迴圈
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IGameRepository>();

                    // 1. 檢查目前排隊人數
                    long count = await repo.GetQueueLengthAsync();

                    if (count >= 4)
                    {
                        _logger.LogInformation($"偵測到 {count} 人在排隊，開始配對...");

                        // 2. 抓出 4 個人 (使用我們先前寫的 PopMatchGroupAsync)
                        var matchedPlayers = await repo.PopMatchGroupAsync(4);

                        if (matchedPlayers.Count == 4)
                        {
                            // 3. 執行開房邏輯 (例如呼叫外部 API 或通知 SignalR)
                            string roomCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                            _logger.LogInformation($"[配對成功] 房間號碼: {roomCode}");

                            // TODO: 這裡可以透過 IHubContext 通知這 4 位玩家連線進入房間
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "配對服務發生錯誤");
            }

            // 每隔 2 秒檢查一次，避免過度消耗 CPU
            await Task.Delay(2000, stoppingToken);
        }
    }
}
