using ApplicationManagementService.Context;
using ApplicationManagementService.DependencyRegister;
using ApplicationManagementService.Extensions;
using ApplicationManagementService.Middleware;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace ApplicationManagementService;

public class Startup
{
    private IConfiguration Configuration { get; }

    public Startup(IHostEnvironment environment)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(environment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

        Configuration = builder.Build();
    }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection");

        serviceCollection.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        serviceCollection.AddControllersWithViews();

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        RegisterDependencies.Register(serviceCollection, Configuration);
    }

    public async Task Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
        }

        await app.DatabaseIsConnected();

        app.UseHsts();

        app.UseRouting();

        app.UseAuthorization();

        app.UseMiddleware<ConcurrencyMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/health");
            endpoints.MapControllers();
        });

        await app.Services.RegisterToServiceDiscovery(Configuration);

        app.Services.ApplyMigrations();

        await app.RunAsync();
    }
}