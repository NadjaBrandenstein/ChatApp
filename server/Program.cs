using System.Text.Json;
using api;
using api.Service;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using StateleSSE.AspNetCore.Extensions;

public class Program
{
    public static void ConfigurationServices(IServiceCollection services, IConfiguration configuration,
        WebApplicationBuilder builder)
    {
        // --------------------------
        // Database (PostgreSQL)
        // --------------------------
        var appOptions = services.AddAppOptions(configuration);

        var connectionString = appOptions.DbConnectionString;
        
        builder.Services.AddDbContext<MyDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        );
        
        // --------------------------
        // Redis (Render)
        // --------------------------
        var redis = builder.Configuration.GetSection("Redis").Value;
        
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = ConfigurationOptions.Parse( redis    );
            config.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(config);
        });
        
        builder.Services.AddRedisSseBackplane();
        
        // --------------------------
        // Controllers
        // --------------------------
        builder.Services.AddControllers()
            .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
        builder.Services.AddOpenApiDocument();
        
        // --------------------------
        // Service
        // --------------------------
        builder.Services.AddScoped<MessageService>();
        
        // --------------------------
        // Cors
        // --------------------------
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("FrontendPolicy", policy =>
            {
                policy
                    .WithOrigins("http://localhost:5174") // your React dev server
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
        
        // Prevent shutdown delay (for SSE)
        builder.Services.Configure<HostOptions>(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(0); 
        });
        
        
        //builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    }

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configure services
        ConfigurationServices(builder.Services, builder.Configuration, builder);
        
        var app = builder.Build();
        
        app.UseRouting();
        //app.UseExceptionHandler();

        app.UseCors(conf => conf.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(_ => true));
        
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.MapControllers();
        app.UseOpenApi();
        app.UseSwaggerUi();
        
        if (app.Environment.IsDevelopment())
        {
            await app.GenerateApiClientsFromOpenApi("/../../client/src/generated-ts-client.ts");
        }
        
        app.MapControllers();
        
        await app.RunAsync();
        
    }
    
}


//builder.Services.AddInMemorySseBackplane();

