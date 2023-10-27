using JobManagementService;
using JobManagementService.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

var env = builder.Environment;
env.ConfigureEnvironment(builder);

var startup = new Startup(env);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
await startup.Configure(app);