namespace JobManagementService.Extensions;

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
}