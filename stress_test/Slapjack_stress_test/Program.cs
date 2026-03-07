// 1. turn on termainal
// 2. cd Slapjack_stress_test
// 3. input following command:
// dotnet run -- createOneRoom
// dotnet run -- createMatchingQueue

using NBomber.CSharp;
// 確保你有 using 你的 Scenarios 命名空間
using LoadTesting.Scenarios;

var scenarioType = args.Length > 0 ? args[0] : "match";

var scenarios = scenarioType switch
{
    "createOneRoom" => new[] { CreateOneRoom.GetScenario() }, // 對應上面定義的類別與方法
    "createMatchingQueue" => new[] { CreateMatchingQueue.GetScenario() },
    //"all" => new[] { CreateOneRoom.GetScenario(), DisconnectTest.GetScenario() },
    _ => throw new Exception("未知的情境")
};

NBomberRunner
    .RegisterScenarios(scenarios)
    .Run();