using JobManagementService.Repositories;
using JobManagementService.Services.JobOffer;

namespace JobManagementService.DependencyRegister;

public static class RegisterDependencies
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IJobOfferService, JobOfferService>();

        services.AddHealthChecks();
    }
}