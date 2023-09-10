﻿using System.Text;
using ApplicationManagementService.Configurations;
using ApplicationManagementService.Repositories;
using ApplicationManagementService.Services;
using Newtonsoft.Json;

namespace ApplicationManagementService.DependencyRegister;

public static class RegisterDependencies
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddHttpClient<JobManagementServiceProxy>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5062");
        });
        
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