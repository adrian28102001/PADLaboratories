using System.Text;
using JobManagementService.Configurations;
using JobManagementService.Context;
using JobManagementService.Repositories;
using JobManagementService.Services.JobOffer;
using Newtonsoft.Json;
using Polly;
using JobManagementService.Factories; // Ensure you import the namespace for the custom factory

namespace JobManagementService.DependencyRegister;

public static class RegisterDependencies
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IJobOfferService, JobOfferService>();

        // Registering CustomHttpClientFactory for dependency injection
        services.AddSingleton<IHttpClientFactory, CustomHttpClientFactory>();

        services.AddHttpClient("APIGateway",
            client => { client.BaseAddress = new Uri("https://jobmanagementservice:5062/jobmanagement"); });

        services.AddHealthChecks();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();
    }

    public static async Task RegisterToServiceDiscovery(IServiceProvider serviceProvider, ConfigurationManager configurationManager)
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

    private static int GetCurrentServiceLoad()
    {
        return 100;
    }
}