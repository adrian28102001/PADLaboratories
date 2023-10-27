using System.Text;
using JobManagementService.Configurations;
using JobManagementService.Context;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Polly;

namespace JobManagementService.Extensions;

public static class ServiceProviderExtensions
{
    public static async Task RegisterToServiceDiscovery(this IServiceProvider serviceProvider,
        IConfiguration configurationManager)
    {
        var serviceConfig = configurationManager.GetSection("ServiceConfig").Get<ServiceConfiguration>();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(serviceConfig.DiscoveryUrl);

        Console.WriteLine($"Attempting to register with Service Discovery at URL: {client.BaseAddress}");

        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryForeverAsync(
                retryAttempt => TimeSpan.FromSeconds(10),
                (exception, timeSpan, context) =>
                {
                    Console.WriteLine(
                        $"Failed to register with Service Discovery due to {exception.Message}. Waiting for 10 seconds before retrying...");
                });

        await retryPolicy.ExecuteAsync(async () =>
        {
            var payload = new
            {
                name = serviceConfig.ServiceName,
                url = serviceConfig.ServiceUrl,
                load = GetCurrentServiceLoad()
            };

            // Logging the Service Name, Service URL, and Load
            Console.WriteLine($"Service Name: {payload.name}");
            Console.WriteLine($"Service URL: {payload.url}");
            Console.WriteLine($"Service Load: {payload.load}");

            var response = await client.PostAsync("register",
                new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Successfully registered with Service Discovery!");
            }
            else
            {
                throw new HttpRequestException(
                    $"Failed to register with Service Discovery. StatusCode: {response.StatusCode}");
            }
        });
    }

    public static void ApplyMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var policy = Policy.Handle<Exception>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(15),
                });

            policy.Execute(() =>
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
            });

            Console.WriteLine("Migrated inventory db");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while migrating the inventory database {ex}");
        }
    }

    private static int GetCurrentServiceLoad()
    {
        return 100;
    }
}