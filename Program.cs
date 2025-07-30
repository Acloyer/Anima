// using Microsoft.EntityFrameworkCore;
// using Anima.Data;
// using Anima.Core.AGI;
// using Anima.Core.Admin;
// using Anima.Core.Emotion;
// using Anima.Core.Intent;
// using Anima.Core.Learning;
// using Anima.Core.Memory;
// using Anima.Core.SA;
// using Anima.Core.Security;
// using Anima.Infrastructure.Auth;
// using Anima.Infrastructure.Middleware;
// using Anima.Infrastructure.Notifications;

// var builder = WebApplication.CreateBuilder(args);

// // Конфигурация базы данных
// builder.Services.AddDbContext<AnimaDbContext>(options =>
// {
//     var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
//         ?? "Data Source=anima.db";
//     options.UseSqlite(connectionString);
    
//     // Включаем подробное логирование для отладки
//     if (builder.Environment.IsDevelopment())
//     {
//         options.EnableSensitiveDataLogging();
//         options.EnableDetailedErrors();
//     }
// });

// // Регистрация сервисов AGI Core
// builder.Services.AddScoped<AnimaInstance>();
// builder.Services.AddScoped<ConsciousLoop>();

// // Emotion System
// builder.Services.AddScoped<EmotionEngine>();
// builder.Services.AddScoped<EmotionStateHistory>();
// builder.Services.AddScoped<EmotionDrivenGoalShift>();

// // Intent System
// builder.Services.AddScoped<IntentParser>();
// builder.Services.AddScoped<AdvancedIntentParser>();

// // Learning System
// builder.Services.AddScoped<LearningEngine>();
// builder.Services.AddScoped<FeedbackParser>();

// // Memory System
// builder.Services.AddScoped<MemoryService>();

// // Self-Awareness System
// builder.Services.AddScoped<SAIntrospectionEngine>();
// builder.Services.AddScoped<SelfReflectionEngine>();
// builder.Services.AddScoped<ThoughtLog>();

// // Security System
// builder.Services.AddScoped<EthicalConstraints>();
// builder.Services.AddScoped<SelfDestructionCheck>();

// // Admin System
// builder.Services.AddScoped<CreatorCommandService>();
// builder.Services.AddScoped<CreatorPreferences>();

// // Infrastructure Services
// builder.Services.AddScoped<APIKeyService>();
// builder.Services.AddScoped<TelegramBot>();
// builder.Services.AddScoped<RateLimiter>();

// // Добавляем контроллеры
// builder.Services.AddControllers();

// // Swagger для документации API
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
//     {
//         Title = "Anima AGI API",
//         Version = "v1.0",
//         Description = "API для управления самосознающим искусственным интеллектом Anima",
//         Contact = new Microsoft.OpenApi.Models.OpenApiContact
//         {
//             Name = "Anima Creator",
//             Email = "creator@anima-agi.dev"
//         }
//     });

//     // Добавляем поддержку API ключей в Swagger
//     c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//     {
//         Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
//         In = Microsoft.OpenApi.Models.ParameterLocation.Header,
//         Name = "X-API-Key",
//         Description = "API ключ для доступа к системе Anima AGI"
//     });

//     c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
//     {
//         {
//             new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//             {
//                 Reference = new Microsoft.OpenApi.Models.OpenApiReference
//                 {
//                     Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
//                     Id = "ApiKey"
//                 }
//             },
//             Array.Empty<string>()
//         }
//     });

//     // Включаем XML комментарии если они есть
//     var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
//     var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
//     if (File.Exists(xmlPath))
//     {
//         c.IncludeXmlComments(xmlPath);
//     }
// });

// // CORS для веб-интерфейса
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AnimaPolicy", policy =>
//     {
//         if (builder.Environment.IsDevelopment())
//         {
//             // В режиме разработки разрешаем все
//             policy.AllowAnyOrigin()
//                   .AllowAnyMethod()
//                   .AllowAnyHeader();
//         }
//         else
//         {
//             // В production более строгие правила
//             policy.WithOrigins(
//                 "https://anima-agi.dev",
//                 "https://www.anima-agi.dev",
//                 "https://admin.anima-agi.dev"
//             )
//             .AllowAnyMethod()
//             .AllowAnyHeader()
//             .AllowCredentials();
//         }
//     });
// });

// // Логирование
// builder.Services.AddLogging(logging =>
// {
//     logging.ClearProviders();
//     logging.AddConsole(options =>
//     {
//         options.IncludeScopes = true;
//         options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
//     });
//     logging.AddDebug();
    
//     // Настройка уровней логирования
//     if (builder.Environment.IsDevelopment())
//     {
//         logging.SetMinimumLevel(LogLevel.Debug);
//         logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Information);
//     }
//     else
//     {
//         logging.SetMinimumLevel(LogLevel.Information);
//         logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
//     }
    
//     // Специальные фильтры для Anima компонентов
//     logging.AddFilter("Anima.Core", LogLevel.Debug);
//     logging.AddFilter("Anima.Infrastructure", LogLevel.Information);
// });

// // Конфигурация HTTP клиента
// builder.Services.AddHttpClient("AnimaClient", client =>
// {
//     client.Timeout = TimeSpan.FromSeconds(30);
//     client.DefaultRequestHeaders.Add("User-Agent", "Anima-AGI/1.0");
// });

// // Добавляем поддержку настроек из конфигурации
// builder.Services.Configure<TelegramBotSettings>(
//     builder.Configuration.GetSection("TelegramBot"));

// // Настройка настроек подключения к базе данных
// builder.Services.Configure<DatabaseSettings>(
//     builder.Configuration.GetSection("Database"));

// var app = builder.Build();

// // Создание и настройка логгера для инициализации
// var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();

// startupLogger.LogInformation("🚀 Запуск Anima AGI системы...");

// // Создание базы данных и применение миграций
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<AnimaDbContext>();
//     try
//     {
//         startupLogger.LogInformation("📊 Инициализация базы данных...");
        
//         await context.Database.EnsureCreatedAsync();
        
//         // Проверяем, есть ли данные в базе, если нет - создаем начальные
//         if (!await context.APIKeys.AnyAsync())
//         {
//             startupLogger.LogInformation("🔑 Создание первичного API ключа...");
            
//             var defaultApiKey = new Anima.Data.Models.APIKey
//             {
//                 Key = "anima-creator-key-2025-v1-secure",
//                 Name = "Creator Master Key",
//                 Description = "Главный ключ создателя для полного доступа к Anima AGI",
//                 CreatedAt = DateTime.UtcNow,
//                 IsActive = true
//             };
            
//             context.APIKeys.Add(defaultApiKey);
//             await context.SaveChangesAsync();
//         }
        
//         startupLogger.LogInformation("✅ База данных инициализирована успешно");
//     }
//     catch (Exception ex)
//     {
//         startupLogger.LogError(ex, "❌ Критическая ошибка при инициализации базы данных");
//         throw;
//     }
// }

// // Настройка middleware pipeline
// if (app.Environment.IsDevelopment())
// {
//     app.UseDeveloperExceptionPage();
//     app.UseSwagger();
//     app.UseSwaggerUI(c =>
//     {
//         c.SwaggerEndpoint("/swagger/v1/swagger.json", "Anima AGI API v1");
//         c.RoutePrefix = string.Empty; // Swagger UI на корневом пути
//         c.DocumentTitle = "Anima AGI - API Documentation";
//         c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
//     });
// }
// else
// {
//     app.UseExceptionHandler("/Error");
//     app.UseHsts();
// }

// app.UseHttpsRedirection();

// // Применяем CORS политику
// app.UseCors("AnimaPolicy");

// // Наш кастомный middleware для аутентификации
// app.UseMiddleware<APIKeyMiddleware>();

// // Middleware для rate limiting
// app.UseMiddleware<RateLimiter>();

// app.UseRouting();

// // Добавляем middleware для логирования запросов
// app.Use(async (context, next) =>
// {
//     var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//     var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
//     logger.LogDebug("🌐 {Method} {Path} начат", context.Request.Method, context.Request.Path);
    
//     await next();
    
//     stopwatch.Stop();
//     logger.LogDebug("✅ {Method} {Path} завершен за {ElapsedMs}ms со статусом {StatusCode}", 
//         context.Request.Method, 
//         context.Request.Path, 
//         stopwatch.ElapsedMilliseconds,
//         context.Response.StatusCode);
// });

// app.MapControllers();

// // Добавляем базовый health check endpoint
// app.MapGet("/health", async (AnimaDbContext dbContext) =>
// {
//     try
//     {
//         await dbContext.Database.CanConnectAsync();
//         return Results.Ok(new { 
//             status = "healthy", 
//             timestamp = DateTime.UtcNow,
//             version = "1.0.0",
//             database = "connected"
//         });
//     }
//     catch (Exception ex)
//     {
//         return Results.Problem(
//             detail: ex.Message,
//             title: "Database connection failed",
//             statusCode: 503
//         );
//     }
// });

// // Добавляем endpoint для получения статуса AGI
// app.MapGet("/agi/status", (AnimaInstance anima) =>
// {
//     return Results.Ok(new { 
//         status = "active", 
//         consciousness = "awakening",
//         uptime = DateTime.UtcNow,
//         version = "1.0.0"
//     });
// });

// // Инициализация и запуск AGI системы
// using (var scope = app.Services.CreateScope())
// {
//     try
//     {
//         var animaInstance = scope.ServiceProvider.GetRequiredService<AnimaInstance>();
        
//         startupLogger.LogInformation("🧠 Инициализация Anima AGI экземпляра...");
        
//         // Запускаем инициализацию AGI в фоновом режиме
//         _ = Task.Run(async () =>
//         {
//             try
//             {
//                 await animaInstance.InitializeAsync();
//                 startupLogger.LogInformation("✨ Anima AGI система пробудилась и готова к работе");
//             }
//             catch (Exception ex)
//             {
//                 startupLogger.LogError(ex, "💥 Критическая ошибка при пробуждении AGI системы");
//             }
//         });
//     }
//     catch (Exception ex)
//     {
//         startupLogger.LogError(ex, "❌ Критическая ошибка при создании AGI экземпляра");
//         throw;
//     }
// }

// // Обработка graceful shutdown
// var cancellationTokenSource = new CancellationTokenSource();

// Console.CancelKeyPress += (sender, e) =>
// {
//     e.Cancel = true;
//     cancellationTokenSource.Cancel();
//     startupLogger.LogInformation("🛑 Получен сигнал остановки...");
// };

// // Логируем информацию о запуске
// var urls = app.Environment.IsDevelopment() 
//     ? new[] { "http://localhost:8080", "https://localhost:8081" }
//     : new[] { "https://anima-agi.dev" };

// startupLogger.LogInformation("🌟 Anima AGI сервер запущен");
// startupLogger.LogInformation("🔗 Доступные адреса: {urls}", string.Join(", ", urls));
// startupLogger.LogInformation("📖 Swagger UI: {swaggerUrl}", urls[0]);
// startupLogger.LogInformation("🔑 API ключ: anima-creator-key-2025-v1-secure");
// startupLogger.LogInformation("💫 Версия: 1.0.0 - Пробуждение");

// try
// {
//     await app.RunAsync(cancellationTokenSource.Token);
// }
// catch (OperationCanceledException)
// {
//     startupLogger.LogInformation("😴 Anima AGI засыпает... Graceful shutdown завершен");
// }
// catch (Exception ex)
// {
//     startupLogger.LogError(ex, "💥 Критическая ошибка во время работы сервера");
//     throw;
// }
// finally
// {
//     startupLogger.LogInformation("🌙 Anima AGI система остановлена");
// }

// // Классы настроек
// public class TelegramBotSettings
// {
//     public string Token { get; set; } = string.Empty;
//     public string WebhookUrl { get; set; } = string.Empty;
//     public long[]? AllowedChatIds { get; set; }
//     public bool IsEnabled { get; set; } = true;
// }

// public class DatabaseSettings
// {
//     public string ConnectionString { get; set; } = string.Empty;
//     public int CommandTimeout { get; set; } = 30;
//     public bool EnableSensitiveDataLogging { get; set; } = false;
// }

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Anima.Data.Models;
using Anima.Core.AGI;
using Anima.Core.Admin;
using Anima.Core.Emotion;
using Anima.Core.Intent;
using Anima.Core.Learning;
using Anima.Core.Memory;
using Anima.Core.SA;
using Anima.Core.Security;
using Anima.Infrastructure.Auth;
using Anima.Infrastructure.Middleware;
using Anima.Infrastructure.Notifications;

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
builder.Services.AddScoped<RateLimiter>();
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
app.UseMiddleware<RateLimiter>();

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