using Anima.Data;
using Anima.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
// using DbContext = Anima.Data.Models.AnimaDbContext;
using DbContext = Anima.Data.AnimaDbContext;
using System.Security.Cryptography;
using System.Text;

namespace Anima.Infrastructure.Auth;

/// <summary>
/// Сервис управления API ключами и аутентификацией
/// </summary>
public class APIKeyService
{
    private readonly string _masterKey; // Получаем из конфигурации
    private readonly string _connectionString; // Строка подключения к БД

    public APIKeyService(IConfiguration configuration)
    {
        _masterKey = configuration["Security:DefaultApiKey"] ?? "anima-creator-key-2025-v1-secure";
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=./anima.db;Cache=Shared;Foreign Keys=true;Mode=ReadWriteCreate;";
    }
    private readonly Dictionary<string, UserSession> _activeSessions = new();

    /// <summary>
    /// Валидация API ключа и получение информации о пользователе
    /// </summary>
    public async Task<AuthResult> ValidateAPIKeyAsync(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            return new AuthResult
            {
                IsValid = false,
                ErrorMessage = "API ключ не предоставлен"
            };
        }

        // Проверка мастер-ключа Создателя
        if (apiKey == _masterKey)
        {
            return new AuthResult
            {
                IsValid = true,
                UserId = "creator",
                Role = "Creator",
                UserName = "Creator",
                Permissions = GetCreatorPermissions()
            };
        }

        using var db = new DbContext(new DbContextOptionsBuilder<DbContext>()
            .UseSqlite(_connectionString)
            .Options);
        
        var keyRecord = await db.APIKeys
            .Where(k => k.KeyHash == HashAPIKey(apiKey))
            .Where(k => !k.IsRevoked)
            .Where(k => k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync();

        if (keyRecord == null)
        {
            await LogAuthAttempt(apiKey, false, "Invalid or expired API key");
            return new AuthResult
            {
                IsValid = false,
                ErrorMessage = "Недействительный или истекший API ключ"
            };
        }

        // Обновляем последнее использование
        keyRecord.LastUsedAt = DateTime.UtcNow;
        keyRecord.UsageCount++;
        await db.SaveChangesAsync();

        await LogAuthAttempt(apiKey, true, "Successful authentication");

        return new AuthResult
        {
            IsValid = true,
            UserId = keyRecord.UserId,
            Role = keyRecord.Role,
            UserName = keyRecord.Name,
            Permissions = GetRolePermissions(keyRecord.Role),
            APIKeyId = keyRecord.Id
        };
    }

    /// <summary>
    /// Создание нового API ключа
    /// </summary>
    public async Task<string> CreateAPIKeyAsync(string name, string role, string userId, DateTime? expiresAt = null, string createdBy = "System")
    {
        var apiKey = GenerateAPIKey();
        var keyHash = HashAPIKey(apiKey);

        var options = new DbContextOptionsBuilder<AnimaDbContext>()
            .UseSqlite(_connectionString)
            .Options;
        using var db = new AnimaDbContext(options);
        
        // Исправляем проблему с типами данных
        var apiKeyRecord = new APIKey
        {
            Name = name,
            KeyHash = keyHash,
            Role = role,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            CreatedBy = createdBy,
            IsRevoked = false,
            UsageCount = 0
        };

        db.APIKeys.Add(apiKeyRecord);
        await db.SaveChangesAsync();

        await LogAPIKeyAction($"API key created: {name} for {userId} with role {role}", createdBy);

        return apiKey;
    }

    /// <summary>
    /// Отзыв API ключа
    /// </summary>
    public async Task<bool> RevokeAPIKeyAsync(int keyId, string revokedBy)
    {
        var options = new DbContextOptionsBuilder<AnimaDbContext>()
            .UseSqlite(_connectionString)
            .Options;
        using var db = new AnimaDbContext(options);
        
        var apiKey = await db.APIKeys.FindAsync(keyId);
        if (apiKey == null) return false;

        apiKey.IsRevoked = true;
        apiKey.RevokedAt = DateTime.UtcNow;
        apiKey.RevokedBy = revokedBy;

        await db.SaveChangesAsync();
        await LogAPIKeyAction($"API key revoked: {apiKey.Name} by {revokedBy}", revokedBy);

        return true;
    }

    /// <summary>
    /// Получение списка API ключей (только для Создателя)
    /// </summary>
    public async Task<List<APIKeyInfo>> GetAPIKeysAsync(string requestingUserRole)
    {
        if (requestingUserRole != "Creator")
        {
            throw new UnauthorizedAccessException("Only Creator can view API keys");
        }

        var options = new DbContextOptionsBuilder<AnimaDbContext>()
            .UseSqlite(_connectionString)
            .Options;
        using var db = new AnimaDbContext(options);
        
        var keys = await db.APIKeys
            .Select(k => new APIKeyInfo
            {
                Id = k.Id,
                Name = k.Name,
                Role = k.Role,
                UserId = k.UserId,
                CreatedAt = k.CreatedAt,
                LastUsedAt = k.LastUsedAt,
                ExpiresAt = k.ExpiresAt,
                IsRevoked = k.IsRevoked,
                UsageCount = k.UsageCount,
                KeyPrefix = k.KeyHash.Substring(0, 8) + "..." // Показываем только префикс
            })
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync();

        return keys;
    }

    /// <summary>
    /// Создание временной сессии
    /// </summary>
    public async Task<string> CreateSessionAsync(string userId, string role, TimeSpan? duration = null)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var sessionDuration = duration ?? TimeSpan.FromHours(24);
        
        var session = new UserSession
        {
            SessionId = sessionId,
            UserId = userId,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(sessionDuration),
            LastActivity = DateTime.UtcNow
        };

        _activeSessions[sessionId] = session;
        
        // Очищаем старые сессии
        await CleanupExpiredSessions();

        return sessionId;
    }

    /// <summary>
    /// Валидация сессии
    /// </summary>
    public AuthResult ValidateSession(string sessionId)
    {
        if (!_activeSessions.TryGetValue(sessionId, out var session))
        {
            return new AuthResult
            {
                IsValid = false,
                ErrorMessage = "Сессия не найдена"
            };
        }

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            _activeSessions.Remove(sessionId);
            return new AuthResult
            {
                IsValid = false,
                ErrorMessage = "Сессия истекла"
            };
        }

        // Обновляем время последней активности
        session.LastActivity = DateTime.UtcNow;

        return new AuthResult
        {
            IsValid = true,
            UserId = session.UserId,
            Role = session.Role,
            UserName = session.UserId,
            Permissions = GetRolePermissions(session.Role),
            SessionId = sessionId
        };
    }

    /// <summary>
    /// Получение информации о текущем пользователе из HTTP контекста
    /// </summary>
    public async Task<CurrentUser> GetCurrentUserAsync(Microsoft.AspNetCore.Http.HttpContext httpContext)
    {
        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            // Проверяем, это API ключ или сессия
            AuthResult authResult;
            if (token.Length == 32) // Сессия
            {
                authResult = ValidateSession(token);
            }
            else // API ключ
            {
                authResult = await ValidateAPIKeyAsync(token);
            }

            if (authResult.IsValid)
            {
                return new CurrentUser
                {
                    UserId = authResult.UserId,
                    Role = authResult.Role,
                    UserName = authResult.UserName,
                    Permissions = authResult.Permissions,
                    IsAuthenticated = true
                };
            }
        }

        return new CurrentUser
        {
            UserId = "anonymous",
            Role = "Anonymous",
            UserName = "Anonymous",
            Permissions = new List<string>(),
            IsAuthenticated = false
        };
    }

    /// <summary>
    /// Генерация статистики использования API
    /// </summary>
    public async Task<string> GetUsageStatsAsync(string requestingUserRole)
    {
        if (requestingUserRole != "Creator")
        {
            return "❌ Только Создатель может просматривать статистику API.";
        }

        var options = new DbContextOptionsBuilder<AnimaDbContext>()
            .UseSqlite(_connectionString)
            .Options;
        using var db = new AnimaDbContext(options);
        
        var totalKeys = await db.APIKeys.CountAsync();
        var activeKeys = await db.APIKeys.CountAsync(k => !k.IsRevoked);
        var expiredKeys = await db.APIKeys.CountAsync(k => k.ExpiresAt < DateTime.UtcNow);
        
        var usage24h = await db.APIKeys
            .Where(k => k.LastUsedAt > DateTime.UtcNow.AddDays(-1))
            .SumAsync(k => k.UsageCount);

        var topUsers = await db.APIKeys
            .Where(k => !k.IsRevoked)
            .GroupBy(k => k.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count(), TotalUsage = g.Sum(k => k.UsageCount) })
            .OrderByDescending(x => x.TotalUsage)
            .Take(5)
            .ToListAsync();

        var activeSessions = _activeSessions.Count(s => s.Value.ExpiresAt > DateTime.UtcNow);

        return $"""
            📊 **Статистика API Anima**
            
            🔑 **API Ключи:**
            • Всего: {totalKeys}
            • Активных: {activeKeys}
            • Истекших: {expiredKeys}
            
            📈 **Использование:**
            • Запросов за 24ч: {usage24h}
            • Активных сессий: {activeSessions}
            
            👥 **Топ пользователи:**
            {string.Join("\n", topUsers.Select(u => $"• {u.UserId}: {u.TotalUsage} запросов ({u.Count} ключей)"))}
            
            🕐 **Обновлено:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}
            """;
    }

    private string GenerateAPIKey()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private string HashAPIKey(string apiKey)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(hashedBytes);
    }

    private List<string> GetCreatorPermissions()
    {
        return new List<string>
        {
            "admin:all",
            "memory:all",
            "emotion:all",
            "goal:all",
            "consciousness:all",
            "debug:all",
            "system:all"
        };
    }

    private List<string> GetRolePermissions(string role)
    {
        return role switch
        {
            "Creator" => GetCreatorPermissions(),
            "Admin" => new List<string>
            {
                "memory:read",
                "memory:write",
                "emotion:read",
                "goal:read",
                "consciousness:read"
            },
            "User" => new List<string>
            {
                "chat:basic",
                "emotion:read",
                "memory:read_own"
            },
            "Demo" => new List<string>
            {
                "chat:limited"
            },
            _ => new List<string>()
        };
    }

    private async Task LogAuthAttempt(string apiKey, bool success, string details)
    {
        var options = new DbContextOptionsBuilder<AnimaDbContext>()
            .UseSqlite(_connectionString)
            .Options;
        using var db = new AnimaDbContext(options);
        
        // Логируем только первые 8 символов ключа для безопасности
        var keyPrefix = apiKey.Length > 8 ? apiKey.Substring(0, 8) + "..." : "short_key";
        
        db.Memories.Add(new MemoryEntity
        {
            MemoryType = "security_audit",
            Content = $"AUTH_ATTEMPT: {keyPrefix} - {(success ? "SUCCESS" : "FAILED")} - {details}",
            Importance = success ? 4 : 7,
            CreatedAt = DateTime.UtcNow,
            InstanceId = Guid.NewGuid().ToString(), // Assuming instanceId is needed for MemoryEntity
            Category = "security"
        });
        
        await db.SaveChangesAsync();
    }

    private async Task LogAPIKeyAction(string action, string performedBy)
    {
        var options = new DbContextOptionsBuilder<AnimaDbContext>()
            .UseSqlite(_connectionString)
            .Options;
        using var db = new AnimaDbContext(options);
        
        db.Memories.Add(new MemoryEntity
        {
            MemoryType = "security_audit",
            Content = $"API_KEY_ACTION: {action}",
            Importance = 8,
            CreatedAt = DateTime.UtcNow,
            InstanceId = Guid.NewGuid().ToString(), // Assuming instanceId is needed for MemoryEntity
            Category = "security"
        });
        
        await db.SaveChangesAsync();
    }

    private async Task CleanupExpiredSessions()
    {
        var expiredSessions = _activeSessions
            .Where(s => s.Value.ExpiresAt < DateTime.UtcNow)
            .Select(s => s.Key)
            .ToList();

        foreach (var sessionId in expiredSessions)
        {
            _activeSessions.Remove(sessionId);
        }
    }
}

/// <summary>
/// Результат аутентификации
/// </summary>
public class AuthResult
{
    public bool IsValid { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public string ErrorMessage { get; set; } = string.Empty;
    public int? APIKeyId { get; set; }
    public string? SessionId { get; set; }
}

/// <summary>
/// Информация о текущем пользователе
/// </summary>
public class CurrentUser
{
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public bool IsAuthenticated { get; set; }
    
    public bool HasPermission(string permission)
    {
        return Permissions.Contains(permission) || Permissions.Contains("admin:all");
    }
    
    public bool IsCreator => Role == "Creator";
}

/// <summary>
/// Информация об API ключе
/// </summary>
public class APIKeyInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public int UsageCount { get; set; }
    public string KeyPrefix { get; set; } = string.Empty;
}

/// <summary>
/// Пользовательская сессия
/// </summary>
public class UserSession
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime LastActivity { get; set; }
}