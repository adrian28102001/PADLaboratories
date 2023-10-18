using JobManagementService.Context;
using JobManagementService.DependencyRegister;
using JobManagementService.Middleware;
using Microsoft.EntityFrameworkCore;
using Polly;

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

    private void ApplyMigrations(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var policy = Policy.Handle<Exception>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(15),
                });

            policy.Execute(() =>
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();
            });

            Console.WriteLine("Migrated inventory db");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while migrating the inventory database {ex}");
        }
    }

    public async Task Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // Potentially add any dev-specific middlewares like UseDeveloperExceptionPage here.
        }

        app.UseHsts();

        app.UseRouting();

        app.UseAuthorization();

        app.UseMiddleware<TimeoutMiddleware>(TimeSpan.FromSeconds(10)); // 10 seconds timeout
        app.UseMiddleware<ConcurrencyMiddleware>(); // Handle concurrency

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/health");
            endpoints.MapControllers();
        });

        await RegisterDependencies.RegisterToServiceDiscovery(app.Services, _configurationManager);

        // ApplyMigrations(app.Services);

        await app.RunAsync();
    }
}