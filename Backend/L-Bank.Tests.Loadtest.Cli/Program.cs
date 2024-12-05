using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace L_Bank.Tests.Loadtest.Cli;

class Program
{
    private static async Task Main()
    {
        using var httpClient = new HttpClient();

        string? jwt;
        try
        {
            jwt = await Login(httpClient, "admin", "adminpass");
            Console.WriteLine("Login successful.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login failed: {ex.Message}");
            return;
        }

        var initialLedgers = await GetAllLedgers(httpClient, jwt);
        PrintAllLedgers(initialLedgers ?? []);

        Console.WriteLine(initialLedgers?.Count == 0 ? "No initial ledgers found." : "Initial ledgers found.");

        var initialBalance = CalculateTotalBalance(initialLedgers);

        Console.WriteLine("Starting NBomber load test...");
        var httpScenario = CreateHttpScenario();
        var bookingScenario = CreateBookingScenario(jwt);

        NBomberRunner
            .RegisterScenarios(httpScenario, bookingScenario)
            .WithReportFileName("reports")
            .WithReportFolder("reports")
            .WithReportFormats(ReportFormat.Html)
            .Run();

        var finalLedgers = await GetAllLedgers(httpClient, jwt);
        var finalBalance = CalculateTotalBalance(finalLedgers);
        Console.WriteLine($"Starting money: {initialBalance}");
        Console.WriteLine($"Ending money: {finalBalance}");

        PrintAllLedgers(finalLedgers ?? []);

        var difference = finalBalance - initialBalance;
        Console.WriteLine($"Difference: {difference}");

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
    }

    private static void PrintAllLedgers(List<Ledger> ledgers)
    {
        Console.WriteLine("Ledgers:");

        foreach (var ledger in ledgers)
        {
            Console.WriteLine($"ID: {ledger.Id}, Amount: {ledger.Balance}");
        }
    }

    static ScenarioProps CreateHttpScenario()
    {
        return Scenario.Create("http_scenario", async context =>
            {
                using var httpClient = new HttpClient();
                var request = Http.CreateRequest("GET", "http://localhost:5000/api/v1/lbankinfo")
                    .WithHeader("Accept", "application/json");

                var response = await Http.Send(httpClient, request);
                return response;
            })
            .WithoutWarmUp()
            .WithLoadSimulations(
                Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
            );
    }

    private static ScenarioProps CreateBookingScenario(string? jwt)
    {
        return Scenario.Create("booking_scenario", async context =>
            {
                using var httpClient = new HttpClient();
                var booking = new
                {
                    SourceId = 3,
                    DestinationId = 2,
                    Amount = 1
                };

                var request = Http.CreateRequest("POST", "http://localhost:5000/api/v1/bookings")
                    .WithHeader("Authorization", $"Bearer {jwt}")
                    .WithHeader("Accept", "application/json")
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(
                        new StringContent(JsonConvert.SerializeObject(booking), Encoding.UTF8, "application/json"));

                var response = await Http.Send(httpClient, request);
                return response;
            })
            .WithoutWarmUp()
            .WithLoadSimulations(
                Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
            );
    }

    static async Task<string?> Login(HttpClient httpClient, string username, string password)
    {
        var loginData = new { Username = username, Password = password };
        var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("http://localhost:5000/api/v1/login", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonDocument = JsonDocument.Parse(responseContent);

        if (jsonDocument.RootElement.TryGetProperty("token", out var tokenElement))
        {
            return tokenElement.GetString();
        }

        throw new Exception("Token not found in response.");
    }

    private static async Task<List<Ledger>?> GetAllLedgers(HttpClient httpClient, string? jwt)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var response = await httpClient.GetAsync("http://localhost:5000/api/v1/ledgers");
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<List<Ledger>>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static decimal CalculateTotalBalance(List<Ledger>? ledgers)
    {
        return ledgers?.Sum(ledger => ledger.Balance) ?? 0;
    }

    public class Ledger
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }
}