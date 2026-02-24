using Application.Services;
using Domain.Provider;
using Domain.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Provider;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 1. get connection string & register DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(dataSource));

// register Repository (interface to instance)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IShapeRepository, ShapeRepository>();

// register Provider
builder.Services.AddScoped<IJwtProvider, JwtProvider>();

// register Service
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ShapeService>();

// connect to Redis
var redisConnectionString = builder.Configuration.GetSection("Redis:ConnectionString").Value;
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
    Console.WriteLine("Redis 連線服務已註冊。");
}

// add authentication middleware
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
        };
    });

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:4000") // 可以鏈式呼叫
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// test Postgres DB connection
app.MapGet("/test-db", async (ApplicationDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect
            ? Results.Ok(new { Message = "PostgreSQL 連線成功！" })
            : Results.Problem("無法連線到資料庫");
    }
    catch (Exception ex)
    {
        return Results.Problem($"連線失敗: {ex.Message}");
    }
});

// test Redis DB connection
app.MapGet("/test-redis", async (IConnectionMultiplexer redis) =>
{
    try
    {
        IDatabase db = redis.GetDatabase();
        var key = "test-key";
        var value = $"Hello from Redis at {DateTime.Now}";

        await db.StringSetAsync(key, value, TimeSpan.FromMinutes(1));
        var retrievedValue = await db.StringGetAsync(key);

        return Results.Ok(new
        {
            Message = "Redis 連線成功！",
            WroteValue = value,
            ReadValue = retrievedValue.ToString()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Redis 連線失敗: {ex.Message}");
    }
});

app.UseCors("AllowLocalhost");
app.MapControllers();
app.Run();
