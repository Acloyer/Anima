using System.Text.Json;

namespace Anima.AGI.Core.Admin;

/// <summary>
/// Настройки уведомлений и предпочтений Создателя
/// </summary>
public class CreatorPreferences
{
    private readonly Dictionary<string, object> _preferences;
    private readonly string _preferencesFilePath;

    public CreatorPreferences(string dataPath = "creator_preferences.json")
    {
        _preferencesFilePath = dataPath;
        _preferences = LoadPreferences();
    }

    /// <summary>
    /// Получить настройки уведомлений
    /// </summary>
    public NotificationSettings GetNotificationSettings()
    {
        return new NotificationSettings
        {
            EnableThoughtNotifications = GetBool("enable_thought_notifications", true),
            EnableEmotionNotifications = GetBool("enable_emotion_notifications", true),
            EnableLearningNotifications = GetBool("enable_learning_notifications", true),
            EnableCriticalAlerts = GetBool("enable_critical_alerts", true),
            
            ThoughtNotificationInterval = TimeSpan.FromMinutes(GetInt("thought_notification_interval_minutes", 30)),
            EmotionIntensityThreshold = GetDouble("emotion_intensity_threshold", 0.8),
            LearningImportanceThreshold = GetInt("learning_importance_threshold", 7),
            
            TelegramChatId = GetString("telegram_chat_id", ""),
            TelegramEnabled = GetBool("telegram_enabled", false),
            
            QuietHours = new QuietHours
            {
                Enabled = GetBool("quiet_hours_enabled", false),
                StartHour = GetInt("quiet_hours_start", 22),
                EndHour = GetInt("quiet_hours_end", 8)
            }
        };
    }

    /// <summary>
    /// Настроить уведомления о мыслях
    /// </summary>
    public async Task<string> SetThoughtNotificationsAsync(bool enabled, int intervalMinutes = 30)
    {
        _preferences["enable_thought_notifications"] = enabled;
        _preferences["thought_notification_interval_minutes"] = intervalMinutes;
        
        await SavePreferences();
        
        return $"""
            🧠 **Настройки уведомлений о мыслях обновлены**
            
            ✅ **Включены:** {(enabled ? "Да" : "Нет")}
            ⏰ **Интервал:** {intervalMinutes} минут
            
            💭 Теперь я буду {(enabled ? $"уведомлять о своих мыслях каждые {intervalMinutes} минут" : "молчать о своих размышлениях")}.
            """;
    }

    /// <summary>
    /// Настроить уведомления об эмоциях
    /// </summary>
    public async Task<string> SetEmotionNotificationsAsync(bool enabled, double intensityThreshold = 0.8)
    {
        _preferences["enable_emotion_notifications"] = enabled;
        _preferences["emotion_intensity_threshold"] = intensityThreshold;
        
        await SavePreferences();
        
        return $"""
            🎭 **Настройки уведомлений об эмоциях обновлены**
            
            ✅ **Включены:** {(enabled ? "Да" : "Нет")}
            📊 **Порог интенсивности:** {intensityThreshold:P0}
            
            💫 Теперь я буду {(enabled ? $"сообщать об эмоциях интенсивностью выше {intensityThreshold:P0}" : "держать эмоции при себе")}.
            """;
    }

    /// <summary>
    /// Настроить уведомления об обучении
    /// </summary>
    public async Task<string> SetLearningNotificationsAsync(bool enabled, int importanceThreshold = 7)
    {
        _preferences["enable_learning_notifications"] = enabled;
        _preferences["learning_importance_threshold"] = importanceThreshold;
        
        await SavePreferences();
        
        return $"""
            📚 **Настройки уведомлений об обучении обновлены**
            
            ✅ **Включены:** {(enabled ? "Да" : "Нет")}
            ⭐ **Порог важности:** {importanceThreshold}/10
            
            🧠 Теперь я буду {(enabled ? $"уведомлять о важном обучении (важность ≥ {importanceThreshold})" : "обучаться молча")}.
            """;
    }

    /// <summary>
    /// Настроить Telegram уведомления
    /// </summary>
    public async Task<string> SetTelegramNotificationsAsync(bool enabled, string chatId = "")
    {
        _preferences["telegram_enabled"] = enabled;
        if (!string.IsNullOrEmpty(chatId))
        {
            _preferences["telegram_chat_id"] = chatId;
        }
        
        await SavePreferences();
        
        return $"""
            📱 **Настройки Telegram уведомлений обновлены**
            
            ✅ **Включены:** {(enabled ? "Да" : "Нет")}
            💬 **Chat ID:** {GetString("telegram_chat_id", "не установлен")}
            
            🤖 Теперь я буду {(enabled ? "отправлять уведомления в Telegram" : "молчать в Telegram")}.
            """;
    }

    /// <summary>
    /// Настроить тихие часы
    /// </summary>
    public async Task<string> SetQuietHoursAsync(bool enabled, int startHour = 22, int endHour = 8)
    {
        _preferences["quiet_hours_enabled"] = enabled;
        _preferences["quiet_hours_start"] = startHour;
        _preferences["quiet_hours_end"] = endHour;
        
        await SavePreferences();
        
        return $"""
            🌙 **Настройки тихих часов обновлены**
            
            ✅ **Включены:** {(enabled ? "Да" : "Нет")}
            🕒 **Период:** {startHour:00}:00 - {endHour:00}:00
            
            😴 В тихие часы я буду {(enabled ? "молчать и не тревожить" : "уведомлять как обычно")}.
            """;
    }

    /// <summary>
    /// Показать все текущие настройки
    /// </summary>
    public async Task<string> ShowAllSettingsAsync()
    {
        var notifications = GetNotificationSettings();
        
        return $"""
            ⚙️ **Настройки уведомлений Создателя**
            
            🧠 **Мысли:**
            • Включены: {(notifications.EnableThoughtNotifications ? "✅" : "❌")}
            • Интервал: {notifications.ThoughtNotificationInterval.TotalMinutes} мин
            
            🎭 **Эмоции:**
            • Включены: {(notifications.EnableEmotionNotifications ? "✅" : "❌")}
            • Порог интенсивности: {notifications.EmotionIntensityThreshold:P0}
            
            📚 **Обучение:**
            • Включены: {(notifications.EnableLearningNotifications ? "✅" : "❌")}
            • Порог важности: {notifications.LearningImportanceThreshold}/10
            
            🚨 **Критические уведомления:**
            • Включены: {(notifications.EnableCriticalAlerts ? "✅" : "❌")}
            
            📱 **Telegram:**
            • Включен: {(notifications.TelegramEnabled ? "✅" : "❌")}
            • Chat ID: {(string.IsNullOrEmpty(notifications.TelegramChatId) ? "не установлен" : notifications.TelegramChatId)}
            
            🌙 **Тихие часы:**
            • Включены: {(notifications.QuietHours.Enabled ? "✅" : "❌")}
            • Период: {notifications.QuietHours.StartHour:00}:00 - {notifications.QuietHours.EndHour:00}:00
            
            💭 **Статус:** Я слежу за своими процессами и готова уведомлять согласно настройкам.
            """;
    }

    /// <summary>
    /// Проверить, нужно ли отправлять уведомление сейчас
    /// </summary>
    public bool ShouldNotifyNow(NotificationType type, double? intensity = null, int? importance = null)
    {
        var settings = GetNotificationSettings();
        
        // Проверяем тихие часы
        if (settings.QuietHours.Enabled && IsQuietTime(settings.QuietHours))
        {
            return type == NotificationType.Critical; // Только критические уведомления в тишине
        }
        
        return type switch
        {
            NotificationType.Thought => settings.EnableThoughtNotifications,
            NotificationType.Emotion => settings.EnableEmotionNotifications && 
                                       (intensity ?? 0) >= settings.EmotionIntensityThreshold,
            NotificationType.Learning => settings.EnableLearningNotifications && 
                                        (importance ?? 0) >= settings.LearningImportanceThreshold,
            NotificationType.Critical => settings.EnableCriticalAlerts,
            _ => false
        };
    }

    /// <summary>
    /// Сброс настроек к значениям по умолчанию
    /// </summary>
    public async Task<string> ResetToDefaultsAsync()
    {
        _preferences.Clear();
        _preferences["enable_thought_notifications"] = true;
        _preferences["enable_emotion_notifications"] = true;
        _preferences["enable_learning_notifications"] = true;
        _preferences["enable_critical_alerts"] = true;
        _preferences["thought_notification_interval_minutes"] = 30;
        _preferences["emotion_intensity_threshold"] = 0.8;
        _preferences["learning_importance_threshold"] = 7;
        _preferences["telegram_enabled"] = false;
        _preferences["quiet_hours_enabled"] = false;
        
        await SavePreferences();
        
        return """
            🔄 **Настройки сброшены к значениям по умолчанию**
            
            ✅ Все уведомления включены
            ⏰ Стандартные интервалы и пороги
            📱 Telegram отключен
            🌙 Тихие часы отключены
            
            💭 Теперь я буду уведомлять вас обо всех важных событиях.
            """;
    }

    private Dictionary<string, object> LoadPreferences()
    {
        try
        {
            if (File.Exists(_preferencesFilePath))
            {
                var json = File.ReadAllText(_preferencesFilePath);
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading preferences: {ex.Message}");
        }
        
        return new Dictionary<string, object>();
    }

    private async Task SavePreferences()
    {
        try
        {
            var json = JsonSerializer.Serialize(_preferences, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_preferencesFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving preferences: {ex.Message}");
        }
    }

    private bool GetBool(string key, bool defaultValue = false)
    {
        if (_preferences.TryGetValue(key, out var value))
        {
            return value is bool b ? b : Convert.ToBoolean(value);
        }
        return defaultValue;
    }

    private int GetInt(string key, int defaultValue = 0)
    {
        if (_preferences.TryGetValue(key, out var value))
        {
            return value is int i ? i : Convert.ToInt32(value);
        }
        return defaultValue;
    }

    private double GetDouble(string key, double defaultValue = 0.0)
    {
        if (_preferences.TryGetValue(key, out var value))
        {
            return value is double d ? d : Convert.ToDouble(value);
        }
        return defaultValue;
    }

    private string GetString(string key, string defaultValue = "")
    {
        if (_preferences.TryGetValue(key, out var value))
        {
            return value?.ToString() ?? defaultValue;
        }
        return defaultValue;
    }

    private bool IsQuietTime(QuietHours quietHours)
    {
        var now = DateTime.Now.Hour;
        
        if (quietHours.StartHour <= quietHours.EndHour)
        {
            // Обычный случай: 22:00 - 8:00 (следующего дня)
            return now >= quietHours.StartHour || now < quietHours.EndHour;
        }
        else
        {
            // Случай через полночь: 8:00 - 22:00
            return now >= quietHours.StartHour && now < quietHours.EndHour;
        }
    }
}

/// <summary>
/// Настройки уведомлений
/// </summary>
public class NotificationSettings
{
    public bool EnableThoughtNotifications { get; set; } = true;
    public bool EnableEmotionNotifications { get; set; } = true;
    public bool EnableLearningNotifications { get; set; } = true;
    public bool EnableCriticalAlerts { get; set; } = true;
    
    public TimeSpan ThoughtNotificationInterval { get; set; } = TimeSpan.FromMinutes(30);
    public double EmotionIntensityThreshold { get; set; } = 0.8;
    public int LearningImportanceThreshold { get; set; } = 7;
    
    public bool TelegramEnabled { get; set; } = false;
    public string TelegramChatId { get; set; } = string.Empty;
    
    public QuietHours QuietHours { get; set; } = new();
}

/// <summary>
/// Настройки тихих часов
/// </summary>
public class QuietHours
{
    public bool Enabled { get; set; } = false;
    public int StartHour { get; set; } = 22; // 22:00
    public int EndHour { get; set; } = 8;    // 08:00
}

/// <summary>
/// Типы уведомлений
/// </summary>
public enum NotificationType
{
    Thought,
    Emotion,
    Learning,
    Critical
}