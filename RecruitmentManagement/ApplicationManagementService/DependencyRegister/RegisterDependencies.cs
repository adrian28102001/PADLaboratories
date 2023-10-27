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
    public static void Register(IServiceCollection services, IConfiguration configurationManager)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddHealthChecks();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<IFileStorageService, FileStorageService>();

        services.AddSingleton<IHttpClientFactory, CustomHttpClientFactory>();

        services.Configure<EmailSettings>(configurationManager.GetSection("EmailSettings"));
    }
}