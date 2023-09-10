using ApplicationManagementService.DependencyRegister;
using ApplicationManagementService.Middleware;
using JobManagementService.Context;
using Microsoft.EntityFrameworkCore;
using TimeoutMiddleware = JobManagementService.Middleware.TimeoutMiddleware;

namespace JobManagementService;

public class Startup
{
    private readonly ConfigurationManager _configurationManager;

    public Startup(ConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;
    }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        // Add services to the container.
        var connectionString = _configurationManager.GetConnectionString("DefaultConnection");

        serviceCollection.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // serviceCollection.AddDatabaseDeveloperPageExceptionFilter();
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        serviceCollection.AddControllersWithViews();

        RegisterDependencies.Register(serviceCollection, _configurationManager);
    }

    public void Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        
        app.UseMiddleware<TimeoutMiddleware>(TimeSpan.FromSeconds(10)); // 10 seconds timeout

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/status");
            endpoints.MapControllers();
        });
        
        app.MapControllers();

        app.Run();
    }
}