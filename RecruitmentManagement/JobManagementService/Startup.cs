using JobManagementService.Context;
using JobManagementService.DependencyRegister;
using JobManagementService.Middleware;
using Microsoft.EntityFrameworkCore;

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

        RegisterDependencies.Register(serviceCollection);
    }

    public void Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // Potentially add any dev-specific middlewares like UseDeveloperExceptionPage here.
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();
    
        // app.UseMiddleware<TimeoutMiddleware>(TimeSpan.FromSeconds(10)); // 10 seconds timeout

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/status");
            endpoints.MapControllers();
        });
    
        app.Run();
    }

}