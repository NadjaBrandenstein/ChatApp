using System.Security.Claims;
using System.Text.Json;
using api;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using StateleSSE.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using StateleSSE.AspNetCore;

public class Program
{
    public static void ConfigurationServices(IServiceCollection services, IConfiguration configuration,
        WebApplicationBuilder builder)
    {
        // --------------------------
        // Database (PostgreSQL)
        // --------------------------
        var appOptions = services.AddAppOptions(configuration);
        var redis = appOptions.Redis;

        var connectionString = appOptions.DbConnectionString;
        
        builder.Services.AddDbContext<MyDbContext>(options =>
            options.UseNpgsql(connectionString)
                // .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        );
        
        // --------------------------
        // Redis (Render)
        // --------------------------
        // var redis = builder.Configuration.GetSection("Redis").Value;
        
        // builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        // {
        //     var config = ConfigurationOptions.Parse( sp.GetRequiredService<AppOptions>().Redis    );
        //     config.AbortOnConnectFail = false;
        //     return ConnectionMultiplexer.Connect(config);
        // });

        if (!string.IsNullOrEmpty(redis))
        {
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var config = ConfigurationOptions.Parse(redis);
                config.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(config);
            });
            builder.Services.AddRedisSseBackplane();
        }
        else
        {
            builder.Services.AddInMemorySseBackplane();
        }
        
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
        // Cors
        // --------------------------
        builder.Services.AddCors();
        
        // Prevent shutdown delay (for SSE)
        builder.Services.Configure<HostOptions>(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(0); 
        });
        
        // --------------------------
        // JWT
        // --------------------------
        var jwtKey = builder.Configuration["Jwt:Key"];

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),
                    NameClaimType = ClaimTypes.Name
                };
            });
            
        builder.Services.AddAuthorization();
        builder.Services.AddScoped<JwtService>();
    }

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configure services
        ConfigurationServices(builder.Services, builder.Configuration, builder);
        
        var app = builder.Build();
        
        app.UseRouting();
        
        app.UseCors(_ => _.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(_ => true));
        
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseOpenApi();
        app.UseSwaggerUi();
        
        if (app.Environment.IsDevelopment())
        {
            await app.GenerateApiClientsFromOpenApi("/../../client/src/generated-ts-client.ts");
        }
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();
        
        await app.RunAsync();
    }
}
