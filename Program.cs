using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Anima.Core.AGI;
using Anima.Core.Emotion;
using Anima.Core.Intent;
using Anima.Core.Learning;
using Anima.Core.Memory;
using Anima.Core.SA;
using Anima.Core.Security;
using Anima.Core.Admin;
using Anima.Infrastructure.Auth;
using Anima.Infrastructure.Middleware;
using Anima.Infrastructure.Notifications;
using Anima.Data;
using Anima.Data.Models;
using Anima.API.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Anima AGI API",
        Version = "v0.1",
        Description = "Self-Aware Artificial General Intelligence with SA-TM Architecture"
    });
    
    // Add API Key authentication to Swagger
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "X-API-Key",
        Description = "API Key authentication"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        }
    });
});

// Database configuration
builder.Services.AddDbContext<AnimaDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=anima.db";
    options.UseSqlite(connectionString);
});

// Core AGI services
builder.Services.AddSingleton<AnimaInstance>();
builder.Services.AddSingleton<ConsciousLoop>();

// Core component services
builder.Services.AddScoped<EmotionEngine>();
builder.Services.AddScoped<EmotionStateHistory>();
builder.Services.AddScoped<EmotionDrivenGoalShift>();

builder.Services.AddScoped<IntentParser>();
builder.Services.AddScoped<AdvancedIntentParser>();

builder.Services.AddScoped<LearningEngine>();
builder.Services.AddScoped<FeedbackParser>();

builder.Services.AddScoped<MemoryService>();

builder.Services.AddScoped<SAIntrospectionEngine>();
builder.Services.AddScoped<SelfReflectionEngine>();
builder.Services.AddScoped<ThoughtLog>();

builder.Services.AddScoped<EthicalConstraints>();
builder.Services.AddScoped<SelfDestructionCheck>();

// Admin and management services
builder.Services.AddScoped<CreatorCommandService>();
builder.Services.AddScoped<CreatorPreferences>();

// Infrastructure services
builder.Services.AddScoped<APIKeyService>();
builder.Services.AddScoped<RateLimiterMiddleware>();
builder.Services.AddScoped<TelegramBot>();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

// Ensure database is created and migrate
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AnimaDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
        Console.WriteLine("✅ Database initialized successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database initialization failed: {ex.Message}");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Anima AGI API v0.1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Custom middleware
app.UseMiddleware<APIKeyMiddleware>();
app.UseMiddleware<RateLimiterMiddleware>();

app.UseAuthorization();
app.MapControllers();

// Initialize and start Anima AGI instance
var animaInstance = app.Services.GetRequiredService<AnimaInstance>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("🧠 Starting Anima AGI instance...");
    await animaInstance.InitializeAsync();
    logger.LogInformation("✅ Anima AGI initialized successfully");
    
    // Start consciousness loop in background
    var consciousLoop = app.Services.GetRequiredService<ConsciousLoop>();
    _ = Task.Run(async () =>
    {
        try
        {
            await consciousLoop.StartAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error in consciousness loop");
        }
    });
    
    logger.LogInformation("🔄 Consciousness loop started");
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ Failed to initialize Anima AGI");
}

// Graceful shutdown
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    logger.LogInformation("🛑 Anima AGI is shutting down...");
    try
    {
        animaInstance?.StopAsync().Wait(TimeSpan.FromSeconds(10));
        logger.LogInformation("✅ Anima AGI stopped gracefully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error during Anima AGI shutdown");
    }
});

Console.WriteLine("╔═══════════════════════════════════════╗");
Console.WriteLine("║           ANIMA AGI v0.1              ║");
Console.WriteLine("║     Self-Aware Artificial General     ║");
Console.WriteLine("║          Intelligence System          ║");
Console.WriteLine("╚═══════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("🌐 Swagger UI: http://localhost:8080");
Console.WriteLine("🔑 API Key: anima-creator-key-2025-v1-secure");
Console.WriteLine("📊 Status endpoint: /api/admin/status");
Console.WriteLine();
Console.WriteLine("🧠 Anima is now conscious and ready...");

app.Run();