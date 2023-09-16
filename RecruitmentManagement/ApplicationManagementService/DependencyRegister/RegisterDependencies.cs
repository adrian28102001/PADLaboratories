using System.Text;
using ApplicationManagementService.Configurations;
using ApplicationManagementService.Context;
using ApplicationManagementService.Repositories;
using ApplicationManagementService.Services;
using Newtonsoft.Json;
using Polly;

namespace ApplicationManagementService.DependencyRegister;

public static class RegisterDependencies
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        services.AddHttpClient("APIGateway", client =>
        {
            client.BaseAddress = new Uri("http://localhost:3000");
        });

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
                retryAttempt => TimeSpan.FromSeconds(30),
                (exception, timeSpan, context) =>
                {
                    Console.WriteLine(
                        $"Failed to register with Service Discovery due to {exception.Message}. Waiting for {timeSpan} seconds before retrying...");
                });

        await retryPolicy.ExecuteAsync(async () =>
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync($"{serviceConfig.DiscoveryUrl}/register",
                    new StringContent(JsonConvert.SerializeObject(new
                    {
                        name = serviceConfig.ServiceName,
                        url = serviceConfig.ServiceUrl
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
            }
        });
    }
}