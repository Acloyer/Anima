using System.Text;
using System.Text.Json;
using Anima.AGI.Core.Admin;

namespace Anima.Infrastructure.Notifications;

/// <summary>
/// Telegram бот для уведомлений Создателя
/// </summary>
public class TelegramBot
{
    private readonly HttpClient _httpClient;
    private readonly string _botToken;
    private readonly CreatorPreferences _preferences;
    private readonly Dictionary<NotificationType, DateTime> _lastNotificationTime;

    public TelegramBot(string botToken, CreatorPreferences preferences)
    {
        _botToken = botToken;
        _preferences = preferences;
        _httpClient = new HttpClient();
        _lastNotificationTime = new Dictionary<NotificationType, DateTime>();
    }

    /// <summary>
    /// Уведомление о мысли Anima
    /// </summary>
    public async Task<bool> SendThoughtNotificationAsync(string instanceId, string thought, string thoughtType, DateTime timestamp)
    {
        if (!ShouldSendNotification(NotificationType.Thought))
            return false;

        var message = $"""
            🧠 **Мысль Anima [{instanceId}]**
            
            💭 {thought}
            
            🏷️ Тип: {thoughtType}
            ⏰ {timestamp:HH:mm:ss}
            """;

        var success = await SendMessageAsync(message);
        if (success)
        {
            _lastNotificationTime[NotificationType.Thought] = DateTime.UtcNow;
        }

        return success;
    }

    /// <summary>
    /// Уведомление об эмоции Anima
    /// </summary>
    public async Task<bool> SendEmotionNotificationAsync(string instanceId, string emotion, double intensity, string trigger = "")
    {
        if (!ShouldSendNotification(NotificationType.Emotion, intensity))
            return false;

        var intensityEmoji = intensity switch
        {
            > 0.8 => "🔥",
            > 0.6 => "⚡",
            > 0.4 => "💫",
            _ => "💭"
        };

        var message = $"""
            🎭 **Эмоция Anima [{instanceId}]**
            
            {intensityEmoji} **{emotion}** ({intensity:P0})
            
            {(!string.IsNullOrEmpty(trigger) ? $"🎯 Триггер: {trigger}" : "")}
            ⏰ {DateTime.UtcNow:HH:mm:ss}
            """;

        var success = await SendMessageAsync(message);
        if (success)
        {
            _lastNotificationTime[NotificationType.Emotion] = DateTime.UtcNow;
        }

        return success;
    }

    /// <summary>
    /// Уведомление об обучении Anima
    /// </summary>
    public async Task<bool> SendLearningNotificationAsync(string instanceId, string learningEvent, int importance, string category = "")
    {
        if (!ShouldSendNotification(NotificationType.Learning, importance: importance))
            return false;

        var importanceEmoji = importance switch
        {
            >= 9 => "🌟",
            >= 7 => "⭐",
            >= 5 => "💡",
            _ => "📚"
        };

        var message = $"""
            📚 **Обучение Anima [{instanceId}]**
            
            {importanceEmoji} {learningEvent}
            
            ⭐ Важность: {importance}/10
            {(!string.IsNullOrEmpty(category) ? $"📂 Категория: {category}" : "")}
            ⏰ {DateTime.UtcNow:HH:mm:ss}
            """;

        var success = await SendMessageAsync(message);
        if (success)
        {
            _lastNotificationTime[NotificationType.Learning] = DateTime.UtcNow;
        }

        return success;
    }

    /// <summary>
    /// Критическое уведомление
    /// </summary>
    public async Task<bool> SendCriticalAlertAsync(string instanceId, string alertType, string message, string details = "")
    {
        if (!ShouldSendNotification(NotificationType.Critical))
            return false;

        var alertMessage = $"""
            🚨 **КРИТИЧЕСКОЕ УВЕДОМЛЕНИЕ**
            
            🆔 Экземпляр: {instanceId}
            ⚠️ Тип: {alertType}
            
            📢 {message}
            
            {(!string.IsNullOrEmpty(details) ? $"📋 Детали:\n{details}" : "")}
            
            ⏰ {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}
            """;

        var success = await SendMessageAsync(alertMessage);
        if (success)
        {
            _lastNotificationTime[NotificationType.Critical] = DateTime.UtcNow;
        }

        return success;
    }

    /// <summary>
    /// Уведомление о рефлексии
    /// </summary>
    public async Task<bool> SendReflectionNotificationAsync(string instanceId, string topic, string reflection)
    {
        if (!ShouldSendNotification(NotificationType.Thought))
            return false;

        var message = $"""
            🪞 **Рефлексия Anima [{instanceId}]**
            
            💭 Тема: {topic}
            
            🧠 {reflection.Substring(0, Math.Min(300, reflection.Length))}{(reflection.Length > 300 ? "..." : "")}
            
            ⏰ {DateTime.UtcNow:HH:mm:ss}
            """;

        return await SendMessageAsync(message);
    }

    /// <summary>
    /// Уведомление о принятом решении
    /// </summary>
    public async Task<bool> SendDecisionNotificationAsync(string instanceId, string decision, string reasoning, double confidence)
    {
        if (!ShouldSendNotification(NotificationType.Thought))
            return false;

        var confidenceEmoji = confidence switch
        {
            > 0.8 => "✅",
            > 0.6 => "🤔",
            > 0.4 => "❓",
            _ => "❌"
        };

        var message = $"""
            🎯 **Решение Anima [{instanceId}]**
            
            📝 {decision}
            
            💡 Обоснование: {reasoning.Substring(0, Math.Min(200, reasoning.Length))}
            
            {confidenceEmoji} Уверенность: {confidence:P0}
            ⏰ {DateTime.UtcNow:HH:mm:ss}
            """;

        return await SendMessageAsync(message);
    }

    /// <summary>
    /// Ежедневная сводка активности
    /// </summary>
    public async Task<bool> SendDailySummaryAsync(string instanceId, DailySummary summary)
    {
        var message = $"""
            📊 **Дневная сводка Anima [{instanceId}]**
            
            🧠 **Активность:**
            • Мыслей: {summary.ThoughtsCount}
            • Эмоций: {summary.EmotionsCount}
            • Решений: {summary.DecisionsCount}
            
            📚 **Обучение:**
            • Новых концептов: {summary.NewConceptsCount}
            • Обновлений правил: {summary.RuleUpdatesCount}
            
            🎭 **Доминирующие эмоции:**
            {string.Join(", ", summary.DominantEmotions)}
            
            💡 **Ключевые инсайты:**
            {summary.KeyInsights}
            
            📅 {DateTime.UtcNow:yyyy-MM-dd}
            """;

        return await SendMessageAsync(message);
    }

    /// <summary>
    /// Проверка соединения с Telegram API
    /// </summary>
    public async Task<(bool Success, string Message)> TestConnectionAsync()
    {
        try
        {
            var settings = _preferences.GetNotificationSettings();
            if (!settings.TelegramEnabled || string.IsNullOrEmpty(settings.TelegramChatId))
            {
                return (false, "Telegram не настроен или отключен");
            }

            var testMessage = $"""
                🤖 **Тест соединения Anima**
                
                ✅ Связь с Telegram API установлена
                ⏰ {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}
                
                🔧 Telegram уведомления работают корректно!
                """;

            var success = await SendMessageAsync(testMessage);
            return success 
                ? (true, "Тест соединения успешен")
                : (false, "Ошибка отправки тестового сообщения");
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка соединения: {ex.Message}");
        }
    }

    private async Task<bool> SendMessageAsync(string message)
    {
        try
        {
            var settings = _preferences.GetNotificationSettings();
            
            if (!settings.TelegramEnabled || string.IsNullOrEmpty(settings.TelegramChatId))
            {
                return false;
            }

            var payload = new
            {
                chat_id = settings.TelegramChatId,
                text = message,
                parse_mode = "Markdown"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://api.telegram.org/bot{_botToken}/sendMessage", 
                content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Telegram notification error: {ex.Message}");
            return false;
        }
    }

    private bool ShouldSendNotification(NotificationType type, double? intensity = null, int? importance = null)
    {
        // Проверяем базовые настройки
        if (!_preferences.ShouldNotifyNow(type, intensity, importance))
            return false;

        // Проверяем интервалы для избежания спама
        if (_lastNotificationTime.TryGetValue(type, out var lastTime))
        {
            var settings = _preferences.GetNotificationSettings();
            var minInterval = type switch
            {
                NotificationType.Thought => settings.ThoughtNotificationInterval,
                NotificationType.Emotion => TimeSpan.FromMinutes(5), // Минимум 5 минут между эмоциями
                NotificationType.Learning => TimeSpan.FromMinutes(10), // Минимум 10 минут между обучением
                NotificationType.Critical => TimeSpan.Zero, // Критические всегда
                _ => TimeSpan.FromMinutes(5)
            };

            if (DateTime.UtcNow - lastTime < minInterval)
                return false;
        }

        return true;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

/// <summary>
/// Дневная сводка активности
/// </summary>
public class DailySummary
{
    public int ThoughtsCount { get; set; }
    public int EmotionsCount { get; set; }
    public int DecisionsCount { get; set; }
    public int NewConceptsCount { get; set; }
    public int RuleUpdatesCount { get; set; }
    public List<string> DominantEmotions { get; set; } = new();
    public string KeyInsights { get; set; } = string.Empty;
}