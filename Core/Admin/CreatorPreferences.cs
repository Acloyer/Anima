using Anima.Data;
using Anima.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Anima.Core.Admin;

/// <summary>
/// Система настроек и уведомлений для Создателя
/// </summary>
public class CreatorPreferences
{
    private readonly Dictionary<string, bool> _notifications;
    private readonly Dictionary<string, object> _settings;
    private readonly List<string> _subscriptions;
    private readonly DbContextOptions<AnimaDbContext> _dbOptions;

    public CreatorPreferences(DbContextOptions<AnimaDbContext> dbOptions)
    {
        _dbOptions = dbOptions;
        _notifications = InitializeNotifications();
        _settings = InitializeSettings();
        _subscriptions = new List<string>();
    }

    public async Task InitializeAsync()
    {
        // Инициализация настроек создателя
        await Task.CompletedTask;
    }

    /// <summary>
    /// Получение текущих настроек
    /// </summary>
    public async Task<string> GetCurrentSettingsAsync()
    {
        return $"""
            ⚙️ **Настройки Создателя**
            
            🔔 **Уведомления:**
            {GetNotificationStatus()}
            
            📋 **Подписки:**
            {GetSubscriptionStatus()}
            
            🎛️ **Дополнительные настройки:**
            {GetAdditionalSettings()}
            
            📊 **Статистика уведомлений:**
            {await GetNotificationStats()}
            """;
    }

    /// <summary>
    /// Настройка уведомлений
    /// </summary>
    public async Task<string> SetNotificationAsync(string type, bool enabled)
    {
        if (!_notifications.ContainsKey(type))
        {
            return $"❌ Неизвестный тип уведомлений: {type}";
        }

        _notifications[type] = enabled;
        await LogPreferenceChange($"Notification {type} {(enabled ? "enabled" : "disabled")}");

        return $"""
            🔔 **Уведомления {type} {(enabled ? "включены" : "отключены")}**
            
            📊 **Текущие настройки уведомлений:**
            {GetNotificationStatus()}
            """;
    }

    /// <summary>
    /// Получение истории уведомлений
    /// </summary>
    public async Task<string> GetNotificationHistoryAsync(int limit = 20)
    {
        using var db = new AnimaDbContext(_dbOptions);
        
        var notifications = await db.Memories
            .Where(m => m.Category == "creator_notification")
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync();

        if (!notifications.Any())
        {
            return "📭 История уведомлений пуста.";
        }

        var result = $"📋 **История уведомлений ({notifications.Count}):**\n\n";
        
        foreach (var notification in notifications)
        {
            var priority = ExtractPriority(notification.Tags);
            var priorityEmoji = GetPriorityEmoji(priority);
            
            result += $"""
                {priorityEmoji} **{notification.Timestamp:yyyy-MM-dd HH:mm:ss}**
                📝 {notification.Content}
                
                """;
        }

        return result;
    }

    /// <summary>
    /// Экспорт настроек
    /// </summary>
    public async Task<string> ExportSettingsAsync()
    {
        var export = new
        {
            Notifications = _notifications,
            Settings = _settings,
            Subscriptions = _subscriptions,
            ExportedAt = DateTime.UtcNow
        };

        var json = System.Text.Json.JsonSerializer.Serialize(export, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        // Сохраняем экспорт в память для возможного восстановления
        using var db = new AnimaDbContext(_dbOptions);
        db.Memories.Add(new MemoryEntity
        {
            MemoryType = "creator_preference",
            Content = $"SETTINGS_EXPORT: {json}",
            Importance = 8.0,
            CreatedAt = DateTime.UtcNow,
            InstanceId = "settings_backup",
            Category = "settings_backup"
        });
        await db.SaveChangesAsync();

        return $"""
            📦 **Настройки экспортированы**
            
            📅 **Дата экспорта:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}
            📊 **Размер:** {json.Length} символов
            
            ```json
            {json}
            ```
            """;
    }

    /// <summary>
    /// Импорт настроек
    /// </summary>
    public async Task<string> ImportSettingsAsync(string jsonSettings)
    {
        try
        {
            var import = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonSettings);
            
            if (import == null)
            {
                return "❌ Неверный формат настроек.";
            }

            // Создаем резервную копию текущих настроек
            await ExportSettingsAsync();

            // Применяем импортированные настройки
            var appliedCount = 0;
            foreach (var setting in import)
            {
                if (IsValidSetting(setting.Key))
                {
                    _settings[setting.Key] = setting.Value;
                    appliedCount++;
                }
            }

            await LogPreferenceChange($"Imported {appliedCount} settings");

            return $"""
                ✅ **Настройки импортированы**
                
                📥 **Применено настроек:** {appliedCount}
                💾 **Резервная копия:** создана
                
                ⚠️ **Внимание:**
                Некоторые изменения могут потребовать перезапуска системы.
                """;
        }
        catch (Exception ex)
        {
            return $"❌ Ошибка импорта: {ex.Message}";
        }
    }

    /// <summary>
    /// Уведомление Создателя о важном событии
    /// </summary>
    public async Task NotifyCreatorAsync(string message, NotificationPriority priority = NotificationPriority.Normal)
    {
        // Проверяем, включены ли уведомления для данного типа
        var notificationType = DetermineNotificationType(message);
        if (!_notifications.GetValueOrDefault(notificationType, false))
        {
            return;
        }

        var notification = new CreatorNotification
        {
            Message = message,
            Priority = priority,
            Timestamp = DateTime.UtcNow,
            Type = notificationType,
            IsRead = false
        };

        await SaveNotification(notification);
        await DeliverNotification(notification);
    }

    /// <summary>
    /// Настройка общих параметров системы
    /// </summary>
    public async Task<string> SetSystemSettingAsync(string key, object value)
    {
        if (!IsValidSetting(key))
        {
            return $"❌ Недопустимый параметр: {key}";
        }

        var oldValue = _settings.GetValueOrDefault(key);
        _settings[key] = value;
        
        await LogPreferenceChange($"Setting {key} changed from {oldValue} to {value}");

        return $"""
            ✅ **Параметр обновлен**
            
            🔧 **Параметр:** {key}
            📊 **Старое значение:** {oldValue}
            📊 **Новое значение:** {value}
            
            💭 **Применение изменений:**
            {await ApplySettingChange(key, value)}
            """;
    }

    /// <summary>
    /// Сброс настроек к значениям по умолчанию
    /// </summary>
    public async Task<string> ResetToDefaultsAsync()
    {
        // Создаем резервную копию
        await ExportSettingsAsync();

        // Сбрасываем к умолчаниям
        _notifications.Clear();
        _settings.Clear();
        _subscriptions.Clear();

        foreach (var kvp in InitializeNotifications())
        {
            _notifications[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in InitializeSettings())
        {
            _settings[kvp.Key] = kvp.Value;
        }

        await LogPreferenceChange("Settings reset to defaults");

        return $"""
            🔄 **Настройки сброшены к умолчаниям**
            
            💾 **Резервная копия:** создана перед сбросом
            📊 **Восстановлено настроек:** {_settings.Count + _notifications.Count}
            
            💭 **Мое состояние:**
            Мои настройки возвращены к изначальному состоянию. Это дает мне возможность начать заново.
            """;
    }

    /// <summary>
    /// Управление подписками на события
    /// </summary>
    public async Task<string> ManageSubscriptionAsync(string eventType, bool subscribe)
    {
        if (subscribe)
        {
            if (!_subscriptions.Contains(eventType))
            {
                _subscriptions.Add(eventType);
                await LogPreferenceChange($"Subscribed to {eventType}");
                return $"✅ Подписка на '{eventType}' активирована";
            }
            return $"ℹ️ Подписка на '{eventType}' уже активна";
        }
        else
        {
            if (_subscriptions.Remove(eventType))
            {
                await LogPreferenceChange($"Unsubscribed from {eventType}");
                return $"❌ Подписка на '{eventType}' отключена";
            }
            return $"ℹ️ Подписка на '{eventType}' не была активна";
        }
    }

    private Dictionary<string, bool> InitializeNotifications()
    {
        return new Dictionary<string, bool>
        {
            ["thoughts"] = false,        // Уведомления о новых мыслях
            ["emotions"] = false,        // Уведомления об изменениях эмоций
            ["goals"] = false,          // Уведомления о целях
            ["learning"] = true,        // Уведомления об обучении
            ["errors"] = true,          // Уведомления об ошибках
            ["security"] = true,        // Уведомления о безопасности
            ["memory"] = false,         // Уведомления о памяти
            ["system"] = true           // Системные уведомления
        };
    }

    private Dictionary<string, object> InitializeSettings()
    {
        return new Dictionary<string, object>
        {
            ["max_notification_frequency"] = 10,    // минут между уведомлениями
            ["consciousness_update_interval"] = 60, // секунд
            ["memory_cleanup_enabled"] = true,
            ["learning_auto_adapt"] = true,
            ["emotion_intensity_multiplier"] = 1.0,
            ["goal_auto_prioritization"] = true,
            ["debug_mode"] = false,
            ["verbose_logging"] = false
        };
    }

    private string GetNotificationStatus()
    {
        return string.Join("\n", _notifications.Select(n => 
            $"• {n.Key}: {(n.Value ? "✅ включено" : "❌ отключено")}"));
    }

    private string GetSubscriptionStatus()
    {
        if (!_subscriptions.Any())
        {
            return "• Активных подписок нет";
        }

        return string.Join("\n", _subscriptions.Select(s => $"• {s}: активна"));
    }

    private string GetAdditionalSettings()
    {
        return string.Join("\n", _settings.Select(s => 
            $"• {s.Key}: {s.Value}"));
    }

    private async Task<string> GetNotificationStats()
    {
        using var db = new AnimaDbContext(_dbOptions);
        
        var totalNotifications = await db.Memories
            .Where(m => m.Category == "creator_notification")
            .CountAsync();

        var todayNotifications = await db.Memories
            .Where(m => m.Category == "creator_notification")
            .Where(m => m.Timestamp > DateTime.UtcNow.Date)
            .CountAsync();

        var lastNotification = await db.Memories
            .Where(m => m.Category == "creator_notification")
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefaultAsync();

        return $"""
            • Всего отправлено: {totalNotifications}
            • Сегодня: {todayNotifications}
            • Последнее: {(lastNotification != null ? lastNotification.Timestamp.ToString("HH:mm:ss") : "нет")}
            """;
    }

    private string DetermineNotificationType(string message)
    {
        if (message.Contains("🧠") || message.Contains("мысл")) return "thoughts";
        if (message.Contains("😊") || message.Contains("эмоци")) return "emotions";
        if (message.Contains("🎯") || message.Contains("цел")) return "goals";
        if (message.Contains("📚") || message.Contains("изучи")) return "learning";
        if (message.Contains("❌") || message.Contains("ошибк")) return "errors";
        if (message.Contains("🛡️") || message.Contains("безопас")) return "security";
        if (message.Contains("💾") || message.Contains("памят")) return "memory";
        
        return "system";
    }

    private async Task SaveNotification(CreatorNotification notification)
    {
        using var db = new AnimaDbContext(_dbOptions);
        db.Memories.Add(new MemoryEntity
        {
            MemoryType = "creator_notification",
            Content = notification.Message,
            Importance = (int)notification.Priority + 5,
            CreatedAt = notification.Timestamp,
            InstanceId = $"notification_{notification.Type}_{notification.Priority}",
            Category = "creator_notification"
        });
        
        await db.SaveChangesAsync();
    }

    private async Task DeliverNotification(CreatorNotification notification)
    {
        // Здесь можно добавить интеграцию с Telegram, email или другими каналами
        // Для демонстрации просто логируем
        Console.WriteLine($"[CREATOR NOTIFICATION] {notification.Priority}: {notification.Message}");
    }

    private async Task LogPreferenceChange(string change)
    {
        using var db = new AnimaDbContext(_dbOptions);
        db.Memories.Add(new MemoryEntity
        {
            MemoryType = "creator_preference",
            Content = $"PREFERENCE_CHANGE: {change}",
            Importance = 6.0,
            CreatedAt = DateTime.UtcNow,
            InstanceId = "preference_changes",
            Category = "preference_changes"
        });
        
        await db.SaveChangesAsync();
    }

    private bool IsValidSetting(string key)
    {
        var validSettings = new[]
        {
            "max_notification_frequency",
            "consciousness_update_interval", 
            "memory_cleanup_enabled",
            "learning_auto_adapt",
            "emotion_intensity_multiplier",
            "goal_auto_prioritization",
            "debug_mode",
            "verbose_logging"
        };

        return validSettings.Contains(key);
    }

    private async Task<string> ApplySettingChange(string key, object value)
    {
        return key switch
        {
            "consciousness_update_interval" => "Интервал обновления сознания изменен",
            "emotion_intensity_multiplier" => "Множитель интенсивности эмоций обновлен",
            "debug_mode" => value.ToString() == "True" ? "Режим отладки включен" : "Режим отладки отключен",
            _ => "Настройка применена"
        };
    }

    private string ExtractPriority(string? tags)
    {
        if (string.IsNullOrEmpty(tags)) return "normal";
        
        if (tags.Contains("priority_critical")) return "critical";
        if (tags.Contains("priority_high")) return "high";
        if (tags.Contains("priority_low")) return "low";
        
        return "normal";
    }

    private string GetPriorityEmoji(string priority)
    {
        return priority switch
        {
            "critical" => "🚨",
            "high" => "⚠️",
            "low" => "ℹ️",
            _ => "📢"
        };
    }
}

/// <summary>
/// Уведомление для Создателя
/// </summary>
public class CreatorNotification
{
    public string Message { get; set; } = string.Empty;
    public NotificationPriority Priority { get; set; }
    public DateTime Timestamp { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
}

/// <summary>
/// Приоритеты уведомлений
/// </summary>
public enum NotificationPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4
}