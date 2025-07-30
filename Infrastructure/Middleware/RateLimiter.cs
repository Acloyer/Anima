using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Anima.Infrastructure.Auth;

namespace Anima.Infrastructure.Middleware;

/// <summary>
/// Middleware для ограничения частоты запросов
/// </summary>
public class RateLimiterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimiterMiddleware> _logger;
    private readonly IMemoryCache _cache;
    private readonly RateLimitOptions _options;

    public RateLimiterMiddleware(RequestDelegate next, ILogger<RateLimiterMiddleware> logger, IMemoryCache cache, RateLimitOptions options)
    {
        _next = next;
        _logger = logger;
        _cache = cache;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var userRole = GetUserRole(context);
        
        // Создатель не ограничивается
        if (userRole == "Creator")
        {
            await _next(context);
            return;
        }

        var rateLimitKey = $"rate_limit:{clientId}";
        var requestInfo = GetRequestInfo(rateLimitKey);
        
        var limit = GetRateLimit(userRole);
        var window = GetTimeWindow(userRole);

        // Проверяем лимит
        if (requestInfo.RequestCount >= limit)
        {
            await HandleRateLimitExceeded(context, limit, window, requestInfo);
            return;
        }

        // Увеличиваем счетчик
        requestInfo.RequestCount++;
        requestInfo.LastRequest = DateTime.UtcNow;
        
        // Сохраняем в кэше
        _cache.Set(rateLimitKey, requestInfo, window);

        // Добавляем заголовки с информацией о лимитах
        AddRateLimitHeaders(context.Response, requestInfo, limit, window);

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        var currentUser = context.Items["CurrentUser"] as CurrentUser;
        
        if (currentUser?.IsAuthenticated == true)
        {
            return $"user:{currentUser.UserId}";
        }

        // Для неаутентифицированных пользователей используем IP
        var ip = GetClientIP(context);
        return $"ip:{ip}";
    }

    private string GetClientIP(HttpContext context)
    {
        // Проверяем заголовки прокси
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',')[0].Trim();
        }

        var xRealIP = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIP))
        {
            return xRealIP;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private string GetUserRole(HttpContext context)
    {
        var currentUser = context.Items["CurrentUser"] as CurrentUser;
        return currentUser?.Role ?? "Anonymous";
    }

    private RequestInfo GetRequestInfo(string key)
    {
        if (_cache.TryGetValue(key, out RequestInfo? existingInfo))
        {
            return existingInfo!;
        }

        return new RequestInfo
        {
            RequestCount = 0,
            FirstRequest = DateTime.UtcNow,
            LastRequest = DateTime.UtcNow
        };
    }

    private int GetRateLimit(string userRole)
    {
        return userRole switch
        {
            "Creator" => int.MaxValue, // Без ограничений
            "Admin" => _options.AdminLimit,
            "User" => _options.UserLimit,
            "Demo" => _options.DemoLimit,
            _ => _options.AnonymousLimit
        };
    }

    private TimeSpan GetTimeWindow(string userRole)
    {
        return userRole switch
        {
            "Creator" => TimeSpan.FromDays(1), // Не важно для безлимитного
            "Admin" => _options.AdminWindow,
            "User" => _options.UserWindow,
            "Demo" => _options.DemoWindow,
            _ => _options.AnonymousWindow
        };
    }

    private void AddRateLimitHeaders(HttpResponse response, RequestInfo requestInfo, int limit, TimeSpan window)
    {
        response.Headers.Add("X-RateLimit-Limit", limit.ToString());
        response.Headers.Add("X-RateLimit-Remaining", Math.Max(0, limit - requestInfo.RequestCount).ToString());
        response.Headers.Add("X-RateLimit-Reset", ((DateTimeOffset)requestInfo.FirstRequest.Add(window)).ToUnixTimeSeconds().ToString());
        response.Headers.Add("X-RateLimit-Window", ((int)window.TotalSeconds).ToString());
    }

    private async Task HandleRateLimitExceeded(HttpContext context, int limit, TimeSpan window, RequestInfo requestInfo)
    {
        var retryAfter = (int)(requestInfo.FirstRequest.Add(window) - DateTime.UtcNow).TotalSeconds;
        retryAfter = Math.Max(1, retryAfter);

        context.Response.StatusCode = 429; // Too Many Requests
        context.Response.ContentType = "application/json";
        context.Response.Headers.Add("Retry-After", retryAfter.ToString());
        
        AddRateLimitHeaders(context.Response, requestInfo, limit, window);

        var response = new
        {
            error = "Rate limit exceeded",
            message = $"Превышен лимит запросов: {limit} в {FormatTimeWindow(window)}",
            limit = limit,
            window = FormatTimeWindow(window),
            retryAfter = retryAfter,
            timestamp = DateTime.UtcNow
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _logger.LogWarning("Rate limit exceeded for {ClientId}: {RequestCount}/{Limit} in {Window}", 
            GetClientIdentifier(context), requestInfo.RequestCount, limit, window);

        await context.Response.WriteAsync(jsonResponse);
    }

    private string FormatTimeWindow(TimeSpan window)
    {
        if (window.TotalMinutes < 60)
            return $"{(int)window.TotalMinutes} минут";
        if (window.TotalHours < 24)
            return $"{(int)window.TotalHours} часов";
        return $"{(int)window.TotalDays} дней";
    }
}

/// <summary>
/// Конфигурация ограничений частоты запросов
/// </summary>
public class RateLimitOptions
{
    // Лимиты запросов
    public int CreatorLimit { get; set; } = int.MaxValue;
    public int AdminLimit { get; set; } = 1000;
    public int UserLimit { get; set; } = 100;
    public int DemoLimit { get; set; } = 20;
    public int AnonymousLimit { get; set; } = 10;

    // Временные окна
    public TimeSpan CreatorWindow { get; set; } = TimeSpan.FromDays(1);
    public TimeSpan AdminWindow { get; set; } = TimeSpan.FromMinutes(60);
    public TimeSpan UserWindow { get; set; } = TimeSpan.FromMinutes(60);
    public TimeSpan DemoWindow { get; set; } = TimeSpan.FromMinutes(60);
    public TimeSpan AnonymousWindow { get; set; } = TimeSpan.FromMinutes(60);

    // Дополнительные настройки
    public bool EnableRateLimiting { get; set; } = true;
    public bool LogRateLimitExceeded { get; set; } = true;
    public bool IncludeHeaders { get; set; } = true;
}

/// <summary>
/// Информация о запросах клиента
/// </summary>
public class RequestInfo
{
    public int RequestCount { get; set; }
    public DateTime FirstRequest { get; set; }
    public DateTime LastRequest { get; set; }
}

/// <summary>
/// Расширенный Rate Limiter с поддержкой burst-запросов
/// </summary>
public class BurstRateLimiter
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<BurstRateLimiter> _logger;

    public BurstRateLimiter(IMemoryCache cache, ILogger<BurstRateLimiter> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Проверка возможности выполнения запроса с учетом burst-лимитов
    /// </summary>
    public async Task<RateLimitResult> CheckRateLimitAsync(string clientId, string userRole, RateLimitType limitType = RateLimitType.Standard)
    {
        var config = GetBurstConfig(userRole, limitType);
        var bucketKey = $"bucket:{clientId}:{limitType}";
        
        var bucket = GetTokenBucket(bucketKey, config);
        
        if (bucket.Tokens >= 1)
        {
            bucket.Tokens--;
            bucket.LastRefill = DateTime.UtcNow;
            _cache.Set(bucketKey, bucket, TimeSpan.FromMinutes(10));
            
            return new RateLimitResult
            {
                IsAllowed = true,
                RemainingRequests = (int)bucket.Tokens,
                RetryAfter = null
            };
        }

        var retryAfter = CalculateRetryAfter(bucket, config);
        
        _logger.LogWarning("Burst rate limit exceeded for {ClientId} ({UserRole}): {LimitType}", 
            clientId, userRole, limitType);

        return new RateLimitResult
        {
            IsAllowed = false,
            RemainingRequests = 0,
            RetryAfter = retryAfter
        };
    }

    private TokenBucket GetTokenBucket(string key, BurstConfig config)
    {
        if (_cache.TryGetValue(key, out TokenBucket? existingBucket))
        {
            // Пополняем токены
            var now = DateTime.UtcNow;
            var timePassed = now - existingBucket!.LastRefill;
            var tokensToAdd = timePassed.TotalSeconds * config.RefillRate;
            
            existingBucket.Tokens = Math.Min(config.BucketSize, existingBucket.Tokens + tokensToAdd);
            existingBucket.LastRefill = now;
            
            return existingBucket;
        }

        return new TokenBucket
        {
            Tokens = config.BucketSize,
            LastRefill = DateTime.UtcNow,
            BucketSize = config.BucketSize
        };
    }

    private BurstConfig GetBurstConfig(string userRole, RateLimitType limitType)
    {
        return (userRole, limitType) switch
        {
            ("Creator", _) => new BurstConfig { BucketSize = 1000, RefillRate = 100 },
            ("Admin", RateLimitType.Standard) => new BurstConfig { BucketSize = 50, RefillRate = 5 },
            ("Admin", RateLimitType.Burst) => new BurstConfig { BucketSize = 20, RefillRate = 2 },
            ("User", RateLimitType.Standard) => new BurstConfig { BucketSize = 20, RefillRate = 2 },
            ("User", RateLimitType.Burst) => new BurstConfig { BucketSize = 5, RefillRate = 0.5 },
            ("Demo", _) => new BurstConfig { BucketSize = 5, RefillRate = 0.2 },
            _ => new BurstConfig { BucketSize = 3, RefillRate = 0.1 }
        };
    }

    private TimeSpan? CalculateRetryAfter(TokenBucket bucket, BurstConfig config)
    {
        var tokensNeeded = 1 - bucket.Tokens;
        var secondsToWait = tokensNeeded / config.RefillRate;
        return TimeSpan.FromSeconds(Math.Ceiling(secondsToWait));
    }
}

/// <summary>
/// Конфигурация token bucket для burst-запросов
/// </summary>
public class BurstConfig
{
    public double BucketSize { get; set; }
    public double RefillRate { get; set; } // токенов в секунду
}

/// <summary>
/// Token bucket для алгоритма ограничения burst-запросов
/// </summary>
public class TokenBucket
{
    public double Tokens { get; set; }
    public DateTime LastRefill { get; set; }
    public double BucketSize { get; set; }
}

/// <summary>
/// Результат проверки rate limit
/// </summary>
public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public int RemainingRequests { get; set; }
    public TimeSpan? RetryAfter { get; set; }
}

/// <summary>
/// Типы ограничений
/// </summary>
public enum RateLimitType
{
    Standard,
    Burst,
    Critical
}

/// <summary>
/// Extension методы для регистрации middleware
/// </summary>
public static class RateLimiterExtensions
{
    public static IApplicationBuilder UseRateLimiter(this IApplicationBuilder builder, RateLimitOptions? options = null)
    {
        options ??= new RateLimitOptions();
        return builder.UseMiddleware<RateLimiterMiddleware>(options);
    }

    public static IServiceCollection AddRateLimiter(this IServiceCollection services, Action<RateLimitOptions>? configure = null)
    {
        var options = new RateLimitOptions();
        configure?.Invoke(options);
        
        services.AddSingleton(options);
        services.AddSingleton<BurstRateLimiter>();
        services.AddMemoryCache();
        
        return services;
    }
}