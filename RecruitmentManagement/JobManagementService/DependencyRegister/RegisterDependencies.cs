using System.Text;
using JobManagementService.Configurations;
using JobManagementService.Context;
using JobManagementService.Repositories;
using JobManagementService.Services.JobOffer;
using Newtonsoft.Json;
using Polly;

namespace JobManagementService.DependencyRegister;

public static class RegisterDependencies
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IJobOfferService, JobOfferService>();

        services.AddHttpClient("APIGateway",
            client => { client.BaseAddress = new Uri("http://localhost:3000/jobmanagement"); });

        services.AddHealthChecks();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();
    }

    public static async Task RegisterToServiceDiscovery(ConfigurationManager configurationManager)
    {
        var serviceConfig = configurationManager.GetSection("ServiceConfig").Get<ServiceConfiguration>();

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
            using var client = new HttpClient();

            var response = await client.PostAsync($"{serviceConfig.DiscoveryUrl}/register",
                new StringContent(JsonConvert.SerializeObject(new
                {
                    name = serviceConfig.ServiceName,
                    url = serviceConfig.ServiceUrl,
                    load = GetCurrentServiceLoad()
                }), Encoding.UTF8, "application/json"));

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