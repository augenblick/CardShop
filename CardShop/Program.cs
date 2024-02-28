using CardShop.Interfaces;
using CardShop.Logic;
using CardShop.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// This specifies which implementation to inject into classes whose constructor includes an "ICardTestRepository".
builder.Services.AddSingleton<ICardTestRepository, CardTestRepository>();
builder.Services.AddSingleton<ICardProductBuilder, CardProductBuilder>();

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

app.Run();
