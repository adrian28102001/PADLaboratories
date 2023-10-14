using System.Text;
using ApplicationManagementService.Configurations;
using ApplicationManagementService.Context;
using ApplicationManagementService.Factories;
using ApplicationManagementService.Models;
using ApplicationManagementService.Repositories;
using ApplicationManagementService.Services;
using Newtonsoft.Json;
using Polly;

namespace ApplicationManagementService.DependencyRegister;

public static class RegisterDependencies
{
    public static void Register(IServiceCollection services, ConfigurationManager configurationManager)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddHttpClient("APIGateway",
            client => { client.BaseAddress = new Uri("https://localhost:3000/jobmanagement"); });

        services.AddHealthChecks();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<IFileStorageService, FileStorageService>();

        services.AddSingleton<IHttpClientFactory, CustomHttpClientFactory>();

        services.Configure<EmailSettings>(configurationManager.GetSection("EmailSettings"));
    }

    public static async Task RegisterToServiceDiscovery(IServiceProvider serviceProvider,
        ConfigurationManager configurationManager)
    {
        var serviceConfig = configurationManager.GetSection("ServiceConfig").Get<ServiceConfiguration>();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(serviceConfig.DiscoveryUrl);

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
            var response = await client.PostAsync("register",
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