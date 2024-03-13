using CardShop;
using CardShop.Interfaces;
using CardShop.Logic;
using CardShop.Repositories;
using Serilog.Events;
using Serilog;
using Serilog.Hosting;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// This specifies which implementation to inject into classes whose constructor requires any of the following Implementations.
builder.Services.AddSingleton<ICardProductBuilder, CardProductBuilder>();
builder.Services.AddSingleton<IShopManager, ShopManager>();
builder.Services.AddSingleton<IInventoryManager, InventoryManager>();
builder.Services.AddSingleton<IInventoryRepository, InventoryRepository>();


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
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var cardProductBuilder = app.Services.GetRequiredService<ICardProductBuilder>();
var shopManager = app.Services.GetRequiredService<IShopManager>();

var timer = new Stopwatch();
timer.Start();

await cardProductBuilder.InitializeCardSets();
shopManager.Initialize();

timer.Stop();

var logThing = app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ShopManager>>();

StaticHelpers.Logger = logThing;

logThing.LogInformation($">>>>>>>>>>>>>> Initialization lasted {timer.ElapsedMilliseconds} ms.");

app.Run();


