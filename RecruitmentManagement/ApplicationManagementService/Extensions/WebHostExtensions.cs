using ApplicationManagementService.Context;

namespace ApplicationManagementService.Extensions;

public static class WebHostExtensions
{
    public static void ConfigureEnvironment(this IWebHostEnvironment env, WebApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

        Console.WriteLine("Configuring for environment: {env.EnvironmentName}");
        Console.WriteLine($"Added appsettings.json and appsettings.{env.EnvironmentName}.json");
        
        if (env.IsProduction())
        {
            builder.Configuration.AddJsonFile("dockersettings.json", optional: true, reloadOnChange: true);
            Console.WriteLine("Added dockersettings.json for production environment");
        }
    }

    public static async Task DatabaseIsConnected(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        Console.WriteLine(await dbContext.Database.CanConnectAsync()
            ? "Successfully connected to the database."
            : "Unable to connect to the database.");
    }
}