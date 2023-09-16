using JobManagementService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var startup = new Startup(builder.Configuration);
await startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app);