﻿using System.Text;
using ApplicationManagementService.Configurations;
using ApplicationManagementService.Context;
using ApplicationManagementService.DependencyRegister;
using ApplicationManagementService.Middleware;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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

        serviceCollection.AddControllersWithViews();

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        RegisterDependencies.Register(serviceCollection, _configurationManager);
    }

    public async Task Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // Potentially add any dev-specific middlewares like UseDeveloperExceptionPage here.
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseMiddleware<TimeoutMiddleware>(TimeSpan.FromSeconds(10)); // 10 seconds timeout
        app.UseMiddleware<ConcurrencyMiddleware>(); // Handle concurrency

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/status");
            endpoints.MapControllers();
        });

        // Call RegisterToServiceDiscovery after the app configuration.
        await RegisterDependencies.RegisterToServiceDiscovery(app.Services, _configurationManager);

        await app.RunAsync();
    }
}