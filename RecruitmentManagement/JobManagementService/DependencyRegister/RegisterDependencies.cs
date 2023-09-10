using System.Text;
using JobManagementService.Configurations;
using JobManagementService.Repositories;
using JobManagementService.Services.JobOffer;
using Newtonsoft.Json;

namespace JobManagementService.DependencyRegister;

public static class RegisterDependencies
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IJobOfferService, JobOfferService>();

        services.AddHealthChecks();
    }
    
    public static void RegisterToServiceDiscovery(ConfigurationManager configurationManager)
    {
        var serviceConfig = configurationManager.GetSection("ServiceConfig").Get<ServiceConfiguration>();

        using (var client = new HttpClient())
        {
            client.PostAsync($"{serviceConfig.DiscoveryUrl}/register", 
                new StringContent(JsonConvert.SerializeObject(new { 
                    name = serviceConfig.ServiceName, 
                    url = serviceConfig.ServiceUrl 
                }), Encoding.UTF8, "application/json")).Wait();
        }
    }
}