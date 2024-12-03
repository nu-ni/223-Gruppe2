using NBomber.Contracts;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;

Console.WriteLine("Calling LBank Info API...");

try
{
    using var httpClient = new HttpClient();
    var response = await CallLBankInfoApi(httpClient);
    Console.WriteLine("API Response:");
    Console.WriteLine(response);
    
    Console.WriteLine("Starting NBomber load test...");
    var scenario = CreateScenario(httpClient);
    NBomberRunner
        .RegisterScenarios(scenario)
        .WithReportFileName("reports")
        .WithReportFolder("reports")
        .WithReportFormats(ReportFormat.Html)
        .Run();

}
catch (Exception ex)
{
    Console.WriteLine("An error occurred while calling the API:");
    Console.WriteLine(ex.Message);
}

Console.WriteLine("Press any key to exit");
Console.ReadKey();
return;

static async Task<string> CallLBankInfoApi(HttpClient httpClient)
{
    httpClient.BaseAddress = new Uri("http://localhost:5000");
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

    var response = await httpClient.GetAsync("/api/v1/lbankinfo");
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsStringAsync();
}

static ScenarioProps CreateScenario(HttpClient httpClient)
{
    return Scenario.Create("http_scenario", async _ =>
        {
            var request =
                Http.CreateRequest("GET", "http://localhost:5000/api/v1/lbankinfo")
                    .WithHeader("Accept", "application/json");
            
            var response = await Http.Send(httpClient, request);

            return response;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 100, 
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromSeconds(30))
        );
}