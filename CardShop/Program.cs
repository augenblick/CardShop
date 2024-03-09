using CardShop.Interfaces;
using CardShop.Logic;
using CardShop.Repositories;
using Newtonsoft.Json;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// This specifies which implementation to inject into classes whose constructor requires any of the following Implementations.
builder.Services.AddSingleton<ICardTestRepository, CardTestRepository>();
builder.Services.AddSingleton<ICardProductBuilder, CardProductBuilder>();
builder.Services.AddSingleton<IShopManager, ShopManager>();


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
Console.WriteLine($">>>>>>>>>>>>>> Initialization lasted {timer.ElapsedMilliseconds} ms.");

app.Run();


