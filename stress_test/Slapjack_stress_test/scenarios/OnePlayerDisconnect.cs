using NBomber.CSharp;
using NBomber.Contracts;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;
using System;
using System.Threading.Tasks;

namespace LoadTesting.Scenarios
{
    public static class OnePlayerDisconnect
    {
        public static ScenarioProps GetScenario()
        {
            return Scenario.Create("one_player_disconnect", async context =>
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5000/gameHub", options => {
                        options.HttpMessageHandlerFactory = (handler) => {
                            if (handler is HttpClientHandler clientHandler)
                                clientHandler.ServerCertificateCustomValidationCallback =
                                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                            return handler;
                        };
                    }).Build();

                try
                {
                    await connection.StartAsync();
                    await connection.InvokeAsync("StartMatching", $"User_{context.InvocationNumber}");

                    // 邏輯區分：
                    // InvocationNumber 為 0 的人 (4 個人中的第 1 個) 等 4 秒後斷線
                    // 其他人 (1, 2, 3) 等 10 秒後斷線
                    int delayTime = (context.InvocationNumber % 4 == 0) ? 8000 : 12000; // disconnecter connection time : others connection time

                    await Task.Delay(delayTime);

                    await connection.StopAsync();
                    return Response.Ok();
                }
                catch (Exception ex)
                {
                    return Response.Fail(message: ex.Message);
                }
            })
            .WithLoadSimulations(
                // 同時注入 4 個連線
                Simulation.Inject(rate: 4, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(1))
            );
        }
    }
}