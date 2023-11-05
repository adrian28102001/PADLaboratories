using JobManagementService.Context;
using JobManagementService.DependencyRegister;
using JobManagementService.Extensions;
using JobManagementService.Middleware;
using Microsoft.EntityFrameworkCore;
using Polly;
using Prometheus;

namespace JobManagementService;

public class Startup
{
    private IConfiguration Configuration { get; }

    public Startup(IHostEnvironment environment)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(environment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables(); // This line ensures environment variables are used

        Configuration = builder.Build();
    }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection");

        serviceCollection.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        serviceCollection.AddControllersWithViews();

        RegisterDependencies.Register(serviceCollection);
    }

    public async Task Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHsts();

        app.UseRouting();

        app.UseAuthorization();

        app.UseMiddleware<ConcurrencyMiddleware>();

        app.UseHttpMetrics(); // This tracks metrics for HTTP requests
        app.UseMetricServer(); // This tracks metrics for HTTP requests

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/health");
            endpoints.MapControllers();
            endpoints.MapMetrics();
        });
        
        await app.Services.RegisterToServiceDiscovery(Configuration);
        app.Services.ApplyMigrations();

        await app.RunAsync();
    }
}