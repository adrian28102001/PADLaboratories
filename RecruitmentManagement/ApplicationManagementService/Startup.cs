using ApplicationManagementService.Context;
using ApplicationManagementService.DependencyRegister;
using ApplicationManagementService.Middleware;
using Microsoft.EntityFrameworkCore;

namespace ApplicationManagementService;

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

        serviceCollection.AddControllersWithViews();
        
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        RegisterDependencies.Register(serviceCollection, _configurationManager);
    }

    public void Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
        }

        app.UseMiddleware<TimeoutMiddleware>(TimeSpan.FromSeconds(10)); // 10 seconds timeout
        
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}