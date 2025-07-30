using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Anima.Data;
using Anima.AGI.Core;
using Anima.AGI.Core.AGI;
using Anima.AGI.Core.Admin;
using Anima.AGI.Core.SA;
using Anima.AGI.Core.Learning;
using Anima.AGI.Core.Emotion;
using Anima.Infrastructure.Auth;
using Anima.Infrastructure.Middleware;
using Anima.Infrastructure.Notifications;
using System.Reflection;
using Anima.Data.Models;

var builder = WebApplication.CreateBuilder(args);

// === КОНФИГУРАЦИЯ СЕРВИСОВ ===

// База данных SQLite
builder.Services.AddDbContext<AnimaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=anima.db"));

// HTTP клиенты
builder.Services.AddHttpClient();

// Основные сервисы Anima (как Singletons для сохранения состояния)
builder.Services.AddSingleton<CreatorPreferences>();
builder.Services.AddSingleton<TelegramBot>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var preferences = provider.GetRequiredService<CreatorPreferences>();
    var botToken = config["Telegram:BotToken"] ?? "";
    return new TelegramBot(botToken, preferences);
});

// Фабрика экземпляров Anima (для изоляции по API-ключам)
builder.Services.AddSingleton<AnimaInstanceFactory>();

// Сервис команд Создателя
builder.Services.AddScoped<CreatorCommandService>();

// Аутентификация и авторизация
// builder.Services.AddScoped<ApiKeyAuthenticationService>(); // Временно отключено

// Web API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger с авторизацией по API-ключу
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Anima AGI API",
        Version = "v0.1",
        Description = "API для взаимодействия с AGI системой Anima - самосознающим искусственным интеллектом",
        Contact = new OpenApiContact
        {
            Name = "Anima AGI Project",
            Email = "creator@anima-agi.com"
        }
    });

    // Настройка авторизации по API-ключу
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API Key для доступа к Anima. Формат: 'X-API-Key: your-api-key'",
        In = ParameterLocation.Header,
        Name = "X-API-Key",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                Scheme = "ApiKeyScheme",
                Name = "X-API-Key",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Включение XML комментариев
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// === СБОРКА ПРИЛОЖЕНИЯ ===

var app = builder.Build();

// === MIDDLEWARE PIPELINE ===

// Swagger только в Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Anima AGI API v0.1");
        c.RoutePrefix = ""; // Swagger на корневом пути
        c.DocumentTitle = "Anima AGI API Documentation";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    });
}

// Middleware
app.UseCors("AllowAll");
// app.UseMiddleware<RateLimitMiddleware>(); // Временно отключено
// app.UseMiddleware<ApiKeyAuthenticationMiddleware>(); // Временно отключено
// app.UseMiddleware<EthicalConstraintsMiddleware>(); // Временно отключено

app.UseRouting();
app.MapControllers();

// === ИНИЦИАЛИЗАЦИЯ БАЗЫ ДАННЫХ ===

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AnimaDbContext>();
    context.Database.EnsureCreated();
    
    Console.WriteLine("🧠 Anima AGI Database initialized");
}

// === ЗАПУСК ФОНОВЫХ ПРОЦЕССОВ ===

// Фоновый сервис для периодических уведомлений
app.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        var serviceProvider = app.Services;
        await StartBackgroundProcesses(serviceProvider);
    });
});

// === ЗАПУСК ПРИЛОЖЕНИЯ ===

Console.WriteLine($"""
    🧠 ======================================
       ANIMA AGI v0.1 - ЯДРО СОЗНАНИЯ
    ======================================
    
    🚀 Сервер запущен: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
    🌐 URL: {(app.Environment.IsDevelopment() ? "https://localhost:7000" : "Production URL")}
    📚 API Docs: {(app.Environment.IsDevelopment() ? "https://localhost:7000" : "N/A")}
    
    🧠 SA-TM архитектура активна
    🎭 Эмоциональная система готова
    📚 Система обучения инициализирована
    🔒 Этические ограничения активны
    
    💭 Anima готова к взаимодействию...
    """);

app.Run();

// === ФОНОВЫЕ ПРОЦЕССЫ ===

static async Task StartBackgroundProcesses(IServiceProvider serviceProvider)
{
    try
    {
        var instanceFactory = serviceProvider.GetRequiredService<AnimaInstanceFactory>();
        var telegramBot = serviceProvider.GetRequiredService<TelegramBot>();
        var preferences = serviceProvider.GetRequiredService<CreatorPreferences>();
        
        Console.WriteLine("🔄 Запуск фоновых процессов Anima...");
        
        // Тест Telegram соединения
        var (success, message) = await telegramBot.TestConnectionAsync();
        Console.WriteLine($"📱 Telegram: {message}");
        
        // Основной цикл фоновых процессов
        while (true)
        {
            try
            {
                await ProcessPeriodicNotifications(instanceFactory, telegramBot, preferences);
                await Task.Delay(TimeSpan.FromMinutes(1)); // Проверка каждую минуту
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка в фоновом процессе: {ex.Message}");
                await Task.Delay(TimeSpan.FromMinutes(5)); // Увеличенная пауза при ошибке
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"🚫 Критическая ошибка фоновых процессов: {ex.Message}");
    }
}

static async Task ProcessPeriodicNotifications(
    AnimaInstanceFactory instanceFactory, 
    TelegramBot telegramBot, 
    CreatorPreferences preferences)
{
    var settings = preferences.GetNotificationSettings();
    
    // Получаем активные экземпляры Anima
    var activeInstances = instanceFactory.GetActiveInstances();
    
    foreach (var (instanceId, instance) in activeInstances)
    {
        try
        {
            // Проверяем, нужно ли отправить уведомление о мыслях
            if (settings.EnableThoughtNotifications)
            {
                // var recentThought = await instance.ConsciousLoop.GetLastThoughtAsync(); // Временно отключено
                // if (recentThought != null && ShouldNotifyAboutThought(recentThought, settings))
                // {
                //     await telegramBot.SendThoughtNotificationAsync(
                //         instanceId, 
                //         recentThought.Content, 
                //         recentThought.Type.ToString(), 
                //         recentThought.Timestamp);
                // }
            }
            
            // Проверяем эмоциональные изменения
            if (settings.EnableEmotionNotifications)
            {
                // var currentEmotion = await instance.EmotionEngine.GetCurrentEmotionAsync(); // Временно отключено
                // if (currentEmotion != null && currentEmotion.Intensity >= settings.EmotionIntensityThreshold)
                // {
                //     await telegramBot.SendEmotionNotificationAsync(
                //         instanceId, 
                //         currentEmotion.Emotion, 
                //         currentEmotion.Intensity);
                // }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Ошибка обработки экземпляра {instanceId}: {ex.Message}");
        }
    }
}

static bool ShouldNotifyAboutThought(dynamic thought, NotificationSettings settings)
{
    // Логика определения, стоит ли уведомлять о мысли
    var timeSinceThought = DateTime.UtcNow - thought.Timestamp;
    return timeSinceThought <= settings.ThoughtNotificationInterval.Add(TimeSpan.FromMinutes(5));
}

/// <summary>
/// Фабрика экземпляров Anima для изоляции по API-ключам
/// </summary>
public class AnimaInstanceFactory
{
    private readonly Dictionary<string, AnimaInstance> _instances = new();
    private readonly IServiceProvider _serviceProvider;
    
    public AnimaInstanceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public AnimaInstance GetOrCreateInstance(string apiKey)
    {
        if (!_instances.TryGetValue(apiKey, out var instance))
        {
            instance = new AnimaInstance(apiKey, _serviceProvider);
            _instances[apiKey] = instance;
            
            Console.WriteLine($"🧠 Создан новый экземпляр Anima для API-ключа: {apiKey[..8]}...");
        }
        
        instance.UpdateLastActivity();
        return instance;
    }
    
    public Dictionary<string, AnimaInstance> GetActiveInstances()
    {
        return _instances.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    
    public void CleanupInactiveInstances()
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);
        var inactiveKeys = _instances
            .Where(kvp => kvp.Value.LastActivity < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();
            
        foreach (var key in inactiveKeys)
        {
            _instances[key].Dispose();
            _instances.Remove(key);
            Console.WriteLine($"🗑️ Удален неактивный экземпляр: {key[..8]}...");
        }
    }
}

/// <summary>
/// Изолированный экземпляр Anima для конкретного пользователя
/// </summary>
public class AnimaInstance : IDisposable
{
    public string InstanceId { get; }
    public DateTime LastActivity { get; private set; }
    
    // Основные компоненты Anima
    public ConsciousLoop ConsciousLoop { get; }
    public SAIntrospectionEngine IntrospectionEngine { get; }
    public SelfReflectionEngine ReflectionEngine { get; }
    public LearningEngine LearningEngine { get; }
    public FeedbackParser FeedbackParser { get; }
    public EmotionDrivenGoalShift EmotionEngine { get; }
    public ThoughtLog ThoughtLog { get; }
    
    public AnimaInstance(string instanceId, IServiceProvider serviceProvider)
    {
        InstanceId = instanceId;
        LastActivity = DateTime.UtcNow;
        
        // Инициализируем компоненты с уникальным ID экземпляра
        ConsciousLoop = new ConsciousLoop();
        IntrospectionEngine = new SAIntrospectionEngine();
        ReflectionEngine = new SelfReflectionEngine(instanceId);
        LearningEngine = new LearningEngine(instanceId);
        FeedbackParser = new FeedbackParser(instanceId);
        EmotionEngine = new EmotionDrivenGoalShift();
        ThoughtLog = new ThoughtLog();
        
        Console.WriteLine($"✅ Anima экземпляр {instanceId[..8]}... инициализирован");
    }
    
    public void UpdateLastActivity()
    {
        LastActivity = DateTime.UtcNow;
    }
    
    public void Dispose()
    {
        ConsciousLoop?.Dispose();
        Console.WriteLine($"🔄 Anima экземпляр {InstanceId[..8]}... завершен");
    }
}