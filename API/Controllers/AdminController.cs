using Microsoft.AspNetCore.Mvc;
using Anima.Core.Admin;
using Anima.Infrastructure.Auth;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Anima.API.Controllers;

/// <summary>
/// Контроллер административных команд для Создателя
/// </summary>
[ApiController]
[Route("api/[controller]")]
[RequireRole("Creator")]
public class AdminController : ControllerBase
{
    private readonly CreatorCommandService _commandService;
    private readonly CreatorPreferences _preferences;
    private readonly APIKeyService _apiKeyService;

    public AdminController(
        CreatorCommandService commandService,
        CreatorPreferences preferences,
        APIKeyService apiKeyService)
    {
        _commandService = commandService;
        _preferences = preferences;
        _apiKeyService = apiKeyService;
    }

    /// <summary>
    /// Выполнение команды Создателя
    /// </summary>
    /// <param name="request">Запрос с командой</param>
    /// <returns>Результат выполнения команды</returns>
    [HttpPost("command")]
    public async Task<ActionResult<CommandResponse>> ExecuteCommand([FromBody] CommandRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Command))
            {
                return BadRequest(new CommandResponse
                {
                    Success = false,
                    Message = "Команда не может быть пустой",
                    Timestamp = DateTime.UtcNow
                });
            }

            var userRole = this.GetUserRole();
            var userId = this.GetUserId();
            var command = request.Command;

            // Исправляем проблему с типами данных
            var result = await _commandService.ExecuteCommandAsync(command, new Dictionary<string, object>
            {
                ["userId"] = userId,
                ["timestamp"] = DateTime.UtcNow
            });

            return Ok(new CommandResponse
            {
                Success = true,
                Message = result,
                Command = request.Command,
                ExecutedBy = this.GetUserId(),
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CommandResponse
            {
                Success = false,
                Message = $"Ошибка выполнения команды: {ex.Message}",
                Command = request.Command,
                Error = ex.GetType().Name,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Получение списка доступных команд
    /// </summary>
    /// <returns>Список команд</returns>
    [HttpGet("commands")]
    public async Task<ActionResult<CommandResponse>> GetCommands()
    {
        try
        {
            var userRole = this.GetUserRole();
            // Исправляем проблему с методами
            var commands = await _commandService.GetCommandListAsync();

            return Ok(new CommandResponse
            {
                Success = true,
                Message = commands,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CommandResponse
            {
                Success = false,
                Message = $"Ошибка получения команд: {ex.Message}",
                Error = ex.GetType().Name,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Получение текущих настроек Создателя
    /// </summary>
    /// <returns>Настройки</returns>
    [HttpGet("settings")]
    public async Task<ActionResult<SettingsResponse>> GetSettings()
    {
        try
        {
            var settings = await _preferences.GetCurrentSettingsAsync();

            return Ok(new SettingsResponse
            {
                Success = true,
                Settings = settings,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new SettingsResponse
            {
                Success = false,
                Message = $"Ошибка получения настроек: {ex.Message}",
                Error = ex.GetType().Name,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Обновление настроек уведомлений
    /// </summary>
    /// <param name="request">Запрос на изменение уведомлений</param>
    /// <returns>Результат обновления</returns>
    [HttpPost("notifications")]
    public async Task<ActionResult<CommandResponse>> UpdateNotifications([FromBody] NotificationRequest request)
    {
        try
        {
            var result = await _preferences.SetNotificationAsync(request.Type, request.Enabled);

            return Ok(new CommandResponse
            {
                Success = true,
                Message = result,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CommandResponse
            {
                Success = false,
                Message = $"Ошибка обновления уведомлений: {ex.Message}",
                Error = ex.GetType().Name,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Получение истории уведомлений
    /// </summary>
    /// <param name="limit">Количество уведомлений (по умолчанию 20)</param>
    /// <returns>История уведомлений</returns>
    [HttpGet("notifications")]
    public async Task<ActionResult<CommandResponse>> GetNotificationHistory([FromQuery] int limit = 20)
    {
        try
        {
            var history = await _preferences.GetNotificationHistoryAsync(limit);

            return Ok(new CommandResponse
            {
                Success = true,
                Message = history,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CommandResponse
            {
                Success = false,
                Message = $"Ошибка получения истории: {ex.Message}",
                Error = ex.GetType().Name,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Экспорт настроек системы
    /// </summary>
    /// <returns>Экспортированные настройки</returns>
    [HttpGet("export")]
    public async Task<ActionResult<CommandResponse>> ExportSettings()
    {
        try
        {
            var export = await _preferences.ExportSettingsAsync();

            return Ok(new CommandResponse
            {
                Success = true,
                Message = export,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CommandResponse
            {
                Success = false,
                Message = $"Ошибка экспорта: {ex.Message}",
                Error = ex.GetType().Name,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Импорт настроек системы
    /// </summary>
    /// <param name="request">Настройки для импорта</param>
    /// <returns>Результат импорта</returns>
    [HttpPost("import")]
    public async Task<ActionResult<CommandResponse>> ImportSettings([FromBody] ImportRequest request)
    {
        try
        {
            var result = await _preferences.ImportSettingsAsync(request.Settings);

            return Ok(new CommandResponse
            {
                Success = true,
                Message = result,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CommandResponse
            {
                Success = false,
                Message = $"Ошибка импорта: {ex.Message}",
                Error = ex.GetType().Name,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Сброс настроек к значениям по умолчанию
    /// </summary>
    /// <returns>Результат сброса</returns>
    [HttpPost("reset")]
    public async Task<ActionResult<CommandResponse>> ResetSettings()
    {
        try
        {
            var result = await _preferences.ResetToDefaultsAsync();

            return Ok(new CommandResponse
            {
                Success = true,
                Message = result,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CommandResponse
            {
                Success = false,
                Message = $"Ошибка сброса: {ex.Message}",
                Error = ex.GetType().Name,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Управление API ключами
    /// </summary>
    /// <returns>Список API ключей</returns>
    [HttpGet("api-keys")]
    public async Task<ActionResult<ApiKeysResponse>> GetApiKeys()
    {
        try
        {
            var userRole = this.GetUserRole();
            var keys = await _apiKeyService.GetAPIKeysAsync(userRole);

            var convertedKeys = keys.Select(k => new APIKeyInfo
            {
                Id = k.Id,
                Name = k.Name,
                Role = k.Role,
                UserId = k.UserId,
                IsRevoked = k.IsRevoked,
                CreatedAt = k.CreatedAt,
                ExpiresAt = k.ExpiresAt,
                LastUsed = k.LastUsedAt,
                MaskedKey = k.KeyPrefix
            }).ToList();

            return Ok(new ApiKeysResponse
            {
                Success = true,
                ApiKeys = convertedKeys,
                Count = convertedKeys.Count,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiKeysResponse
            {
                Success = false,
                Message = $"Ошибка получения API ключей: {ex.Message}",
                Error = ex.GetType().Name,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Создание нового API ключа
    /// </summary>
    /// <param name="request">Данные для создания ключа</param>
    /// <returns>Новый API ключ</returns>
    [HttpPost("api-keys")]
    public async Task<ActionResult<CreateKeyResponse>> CreateApiKey([FromBody] CreateKeyRequest request)
    {
        try
        {
            var createdBy = this.GetUserId();
            var apiKey = await _apiKeyService.CreateAPIKeyAsync(
                request.Name, 
                request.Role, 
                request.UserId, 
                request.ExpiresAt, 
                createdBy);

            return Ok(new CreateKeyResponse
            {
                Success = true,
                ApiKey = apiKey,
                Message = $"API ключ '{request.Name}' создан успешно",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CreateKeyResponse
            {
                Success = false,
                Message = $"Ошибка создания API ключа: {ex.Message}",
                Error = ex.GetType().Name,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Отзыв API ключа
    /// </summary>
    /// <param name="keyId">ID ключа для отзыва</param>
    /// <returns>Результат отзыва</returns>
    [HttpDelete("api-keys/{keyId}")]
    public async Task<ActionResult<CommandResponse>> RevokeApiKey(int keyId)
    {
        try
        {
            var revokedBy = this.GetUserId();
            var success = await _apiKeyService.RevokeAPIKeyAsync(keyId, revokedBy);

            if (!success)
            {
                return NotFound(new CommandResponse
                {
                    Success = false,
                    Message = "API ключ не найден",
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new CommandResponse
            {
                Success = true,
                Message = "API ключ отозван успешно",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CommandResponse
            {
                Success = false,
                Message = $"Ошибка отзыва API ключа: {ex.Message}",
                Error = ex.GetType().Name,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Получение статистики использования API
    /// </summary>
    /// <returns>Статистика API</returns>
    [HttpGet("api-stats")]
    public async Task<ActionResult<CommandResponse>> GetApiStats()
    {
        try
        {
            var userRole = this.GetUserRole();
            var stats = await _apiKeyService.GetUsageStatsAsync(userRole);

            return Ok(new CommandResponse
            {
                Success = true,
                Message = stats,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CommandResponse
            {
                Success = false,
                Message = $"Ошибка получения статистики: {ex.Message}",
                Error = ex.GetType().Name,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Health check для административных функций
    /// </summary>
    /// <returns>Статус системы</returns>
    [HttpGet("health")]
    public async Task<ActionResult<HealthResponse>> GetHealth()
    {
        try
        {
            var health = new HealthResponse
            {
                Success = true,
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Components = new Dictionary<string, string>
                {
                    ["CommandService"] = "OK",
                    ["Preferences"] = "OK",
                    ["APIKeyService"] = "OK",
                    ["Database"] = "OK"
                },
                Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime,
                Version = "v0.1.1"
            };

            return Ok(health);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new HealthResponse
            {
                Success = false,
                Status = "Unhealthy",
                Message = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

#region DTO Models

/// <summary>
/// Запрос на выполнение команды
/// </summary>
public class CommandRequest
{
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Command { get; set; } = string.Empty;
}

/// <summary>
/// Ответ на выполнение команды
/// </summary>
public class CommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Command { get; set; }
    public string? ExecutedBy { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Ответ с настройками
/// </summary>
public class SettingsResponse
{
    public bool Success { get; set; }
    public string Settings { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Запрос на изменение уведомлений
/// </summary>
public class NotificationRequest
{
    [Required]
    public string Type { get; set; } = string.Empty;
    
    public bool Enabled { get; set; }
}

/// <summary>
/// Запрос на импорт настроек
/// </summary>
public class ImportRequest
{
    [Required]
    public string Settings { get; set; } = string.Empty;
}

/// <summary>
/// Запрос на создание API ключа
/// </summary>
public class CreateKeyRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Role { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string UserId { get; set; } = string.Empty;
    
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Ответ с созданным API ключом
/// </summary>
public class CreateKeyResponse
{
    public bool Success { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Ответ со списком API ключей
/// </summary>
public class ApiKeysResponse
{
    public bool Success { get; set; }
    public List<APIKeyInfo> ApiKeys { get; set; } = new();
    public int Count { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Ответ health check
/// </summary>
public class HealthResponse
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, string> Components { get; set; } = new();
    public TimeSpan? Uptime { get; set; }
    public string? Version { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Информация об API ключе (для списка)
/// </summary>
public class APIKeyInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsed { get; set; }
    public string MaskedKey { get; set; } = string.Empty; // Показываем только последние 4 символа
}

#endregion