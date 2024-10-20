using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Instantiate the Startup class
var startup = new Startup(builder.Configuration);

// Call the ConfigureServices method from Startup
startup.ConfigureServices(builder.Services);

var app = builder.Build();

// Call the Configure method from Startup to set up middleware
startup.Configure(app);

app.Run();
