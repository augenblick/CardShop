using CardShop;
using CardShop.Interfaces;
using CardShop.Logic;
using CardShop.Repositories;
using Serilog.Events;
using Serilog;
using System.Diagnostics;
using CardShop.Models;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using CardShop.Repositories.Models;

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
builder.Services.AddSingleton<TokenManager, TokenManager>();


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
builder.Services.AddHttpContextAccessor();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});



var secret = "TODO: store this some place secure!";
var key = Encoding.ASCII.GetBytes(secret);

//builder.Services
//    .AddIdentity<SecureUser, IdentityRole>(options =>
//    {
//        options.SignIn.RequireConfirmedAccount = false;
//        options.User.RequireUniqueEmail = true;
//        options.Password.RequireDigit = false;
//        options.Password.RequiredLength = 6;
//        options.Password.RequireNonAlphanumeric = false;
//        options.Password.RequireUppercase = false;
//    })
//    .AddRoles<IdentityRole>();

var validIssuer = builder.Configuration.GetValue<string>("JwtTokenSettings:ValidIssuer");
var validAudience = builder.Configuration.GetValue<string>("JwtTokenSettings:ValidAudience");
var symmetricSecurityKey = builder.Configuration.GetValue<string>("JwtTokenSettings:SymmetricSecurityKey");

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = true;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = validIssuer,
            ValidAudience = validAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(symmetricSecurityKey)
            ),
        };
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

if (!Debugger.IsAttached || app.Services.GetRequiredService<IConfiguration>().GetValue<bool>("UseAuthDuringDev"))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapControllers();

var cardProductBuilder = app.Services.GetRequiredService<ICardProductBuilder>();
var shopManager = app.Services.GetRequiredService<IShopManager>();

var timer = new Stopwatch();
timer.Start();

var logThing = app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ShopManager>>();

StaticHelpers.Logger = logThing;

await cardProductBuilder.InitializeCardSets();
shopManager.Initialize();

timer.Stop();

logThing.LogInformation($">>>>>>>>>>>>>> Initialization lasted {timer.ElapsedMilliseconds} ms.");

app.Run();


