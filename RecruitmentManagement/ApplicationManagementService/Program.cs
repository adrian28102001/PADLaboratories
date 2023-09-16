using ApplicationManagementService;

var builder = WebApplication.CreateBuilder(args);


var startup = new Startup(builder.Configuration);
await startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app);