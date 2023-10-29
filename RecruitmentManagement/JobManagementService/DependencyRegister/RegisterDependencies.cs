using System.Text;
using JobManagementService.Configurations;
using JobManagementService.Context;
using JobManagementService.Repositories;
using JobManagementService.Services.JobOffer;
using Newtonsoft.Json;
using Polly;
using JobManagementService.Factories;

namespace JobManagementService.DependencyRegister;

public static class RegisterDependencies
{
    public static void Register(IServiceCollection services)
    {
        services.AddSwaggerGen();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IJobOfferService, JobOfferService>();

        // Registering CustomHttpClientFactory for dependency injection
        services.AddSingleton<IHttpClientFactory, CustomHttpClientFactory>();

        services.AddHttpClient("APIGateway",
            client => { client.BaseAddress = new Uri("http://jobmanagementservice:5062/jobmanagement"); });

        services.AddHealthChecks();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();
    }
}