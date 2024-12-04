using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NBomber.Contracts;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace LBank.Tests.Loadtest.Cli
{
    class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            string jwt = await Login("admin", "adminpass");

            var initialLedgers = await GetAllLedgers(jwt);
            if (initialLedgers == null || initialLedgers.Count == 0)
            {
                Console.WriteLine("No initial ledgers found.");
            }
            else
            {
                Console.WriteLine("Initial ledgers found.");
            }

            decimal initialBalance = CalculateTotalBalance(initialLedgers);
            

            Console.WriteLine("Starting NBomber load test...");
            var scenario = CreateScenario(httpClient);
            NBomberRunner
                .RegisterScenarios(scenario)
                .WithReportFileName("reports")
                .WithReportFolder("reports")
                .WithReportFormats(ReportFormat.Html)
                .Run();

            var finalLedgers = await GetAllLedgers(jwt);
            decimal finalBalance = CalculateTotalBalance(finalLedgers);
            Console.WriteLine($"Starting money: {initialBalance}");
            Console.WriteLine($"Ending money: {finalBalance}");

            decimal difference = finalBalance - initialBalance;
            Console.WriteLine($"Difference: {difference}");

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
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

        static async Task<string> Login(string username, string password)
        {
            var loginData = new { Username = username, Password = password };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("http://localhost:5000/api/v1/login", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(responseContent);
            return jsonDocument.RootElement.GetProperty("token").GetString();
        }

        static async Task<List<Ledger>> GetAllLedgers(string jwt)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var response = await httpClient.GetAsync("http://localhost:5000/api/v1/ledgers");
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Content: {responseContent}");

            return JsonSerializer.Deserialize<List<Ledger>>(responseContent);
        }

        static decimal CalculateTotalBalance(List<Ledger> ledgers)
        {
            decimal totalBalance = 0;
            foreach (var ledger in ledgers)
            {
                totalBalance += ledger.balance;
            }
            return totalBalance;
        }
    }

    public class Ledger
    {
        public int id { get; set; }
        public string name { get; set; }
        public decimal balance { get; set; }
    }
}