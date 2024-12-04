using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LBank.Tests.Loadtest.Cli
{
    class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            string jwt = await Login("admin", "adminpass");
            Console.WriteLine($"JWT: {jwt}");

            var ledgers = await GetAllLedgers(jwt);
            if (ledgers == null || ledgers.Count == 0)
            {
                Console.WriteLine("No ledgers found.");
            }
            else
            {
                foreach (var ledger in ledgers)
                {
                    Console.WriteLine(ledger.name);
                }
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
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
    }

    public class Ledger
    {
        public int id { get; set; }
        public string name { get; set; }
        public decimal balance { get; set; }
    }
}