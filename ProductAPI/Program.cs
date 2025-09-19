using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using ProductAPI.Data;
using ProductAPI.Filters;
using ProductAPI.Helpers;
using ProductAPI.Repositories;
using StackExchange.Redis;
namespace ProductAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        // Configure Entity Framework and SQL Server
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("SQLConnection")));


        // Global exception filter
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<GlobalExceptionFilter>();
        });

        // Configure Redis

        var redis = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("RedisConnection")!);
        builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

        // Dependency injection
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IUniqueIdGenerator, UniqueIdGenerator>();

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
    }
}
