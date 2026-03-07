using NBomber.CSharp;
using NBomber.Contracts;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;

namespace LoadTesting.Scenarios // 建議加上命名空間
{
    public static class CreateMatchingQueue
    {
        public static ScenarioProps GetScenario()
        {
            return Scenario.Create("create_matching_queue", async context =>
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5000/gameHub", options => {
                        options.HttpMessageHandlerFactory = (handler) => {
                            if (handler is HttpClientHandler clientHandler)
                                clientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                            return handler;
                        };
                    }).Build();

                try
                {
                    await connection.StartAsync();
                    await connection.InvokeAsync("StartMatching", $"User_{context.InvocationNumber}");
                    await Task.Delay(100000); // connection time
                    await connection.StopAsync();
                    return Response.Ok();
                }
                catch (Exception ex) { return Response.Fail(message: ex.Message); }
            })
            .WithLoadSimulations(
                Simulation.Inject(rate: 3, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(1))
            );
        }
    }
}