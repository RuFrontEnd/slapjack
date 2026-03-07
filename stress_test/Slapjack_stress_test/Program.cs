using Microsoft.AspNetCore.SignalR.Client;
using NBomber.CSharp;
using NBomber.Contracts;
using System.Net.Http; // 記得加上這一行

// 1. 定義一個簡單的玩家客戶端
var scenario = Scenario.Create("matchmaking_scenario", async context =>
{
    // 為了模擬真實性，我們可以從 Step 的 context 取得 logger
    var logger = context.Logger;

    // A. 建立連線 (此部分通常在 Init 階段做更好，但這裡先示範單一 Step 流程)
    var connection = new HubConnectionBuilder()
        .WithUrl("http://localhost:5000/gameHub", options =>
        {
            // 核心修正：加入此 Callback 來忽略憑證錯誤
            options.HttpMessageHandlerFactory = (handler) =>
            {
                if (handler is HttpClientHandler clientHandler)
                {
                    clientHandler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }
                return handler;
            };
        })
        .Build();

    try
    {
        // B. 紀錄連線動作
        var connectPart = await Step.Run("connect", context, async () =>
        {
            await connection.StartAsync();
            return Response.Ok();
        });

        // C. 紀錄發起配對動作
        var matchPart = await Step.Run("start_matching", context, async () =>
        {
            // 呼叫你的 Hub 方法
            await connection.InvokeAsync("StartMatching", $"User_{context.InvocationNumber}");
            return Response.Ok();
        });

        // D. 模擬玩家在線等待一段時間 (例如等待 5 秒看會不會收到結果)
        await Task.Delay(5000);

        await connection.StopAsync();
        return Response.Ok();
    }
    catch (Exception ex)
    {
        return Response.Fail(message: ex.Message);
    }
})
.WithLoadSimulations(
    Simulation.KeepConstant(copies: 100, during: TimeSpan.FromSeconds(30))
);

// 3. 啟動 NBomber
NBomberRunner
    .RegisterScenarios(scenario)
    .Run();