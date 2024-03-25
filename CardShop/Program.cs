using CardShop;
using CardShop.Interfaces;
using CardShop.Logic;
using CardShop.Repositories;
using Serilog.Events;
using Serilog;
using System.Diagnostics;
using CardShop.Models;
using Microsoft.OpenApi.Models;
using CardShop.ConfigurationClasses;

// CORS
var MyAllowAnyOrigins = "_myAllowAnyOrigins";

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowAnyOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                                .AllowAnyMethod()
                                .AllowAnyHeader(); 

                      });
});

// Add services to the container.
// This specifies which implementation to inject into classes whose constructor requires any of the following Implementations.
builder.Services.AddSingleton<ICardProductBuilder, CardProductBuilder>();
builder.Services.AddSingleton<IShopManager, ShopManager>();
builder.Services.AddSingleton<IInventoryManager, InventoryManager>();
builder.Services.AddSingleton<IInventoryRepository, InventoryRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IUserManager, UserManager>();
builder.Services.AddSingleton<DbBuilder>();


var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.File("Logs/DailyLog.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

builder.Services.AddLogging(builder => { 
        builder.AddConsole();
        builder.AddSerilog(logger);
    });

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<Card>(() => new OpenApiSchema { Type = "object" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS
app.UseCors(MyAllowAnyOrigins);

app.UseAuthorization();

app.MapControllers();

var cardProductBuilder = app.Services.GetRequiredService<ICardProductBuilder>();
var shopManager = app.Services.GetRequiredService<IShopManager>();
var dbBuilder = app.Services.GetRequiredService<DbBuilder>();

var timer = new Stopwatch();
timer.Start();

var logThing = app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ShopManager>>();

StaticHelpers.Logger = logThing;

await dbBuilder.InitializeDb();
await cardProductBuilder.InitializeCardSets();
shopManager.Initialize();

timer.Stop();

logThing.LogInformation($">>>>>>>>>>>>>> Initialization lasted {timer.ElapsedMilliseconds} ms.");

app.Run();


