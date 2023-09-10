using ApplicationManagementService.Services;
using SharedLibrary.Repositories;

namespace ApplicationManagementService.DependencyRegister;

public static class RegisterDependencies
{
    public static void Register(IServiceCollection services, ConfigurationManager configurationManager)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddHttpClient<JobManagementServiceProxy>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5062");
        });
        
        services.AddHealthChecks();
    }
}