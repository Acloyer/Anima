using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Anima.Data.Models; // Add this using for NotificationType

namespace Anima.Infrastructure.Notifications
{
    public class TelegramBot
    {
        private readonly ILogger<TelegramBot> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string? _botToken;
        private readonly string? _chatId;
        private bool _isEnabled;

        public TelegramBot(ILogger<TelegramBot> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = new HttpClient();
            _botToken = _configuration["Telegram:BotToken"];
            _chatId = _configuration["Telegram:ChatId"];
            _isEnabled = !string.IsNullOrEmpty(_botToken) && !string.IsNullOrEmpty(_chatId);

            if (!_isEnabled)
            {
                _logger.LogWarning("Telegram notifications disabled - missing bot token or chat ID");
            }
        }

        public async Task SendNotificationAsync(NotificationType type, string title, string content)
        {
            if (!_isEnabled)
            {
                _logger.LogDebug("Telegram notification skipped (disabled): {Title}", title);
                return;
            }

            try
            {
                var emoji = GetEmojiForNotificationType(type);
                var formattedMessage = FormatMessage(emoji, title, content, type);
                
                await SendMessageAsync(formattedMessage);
                _logger.LogInformation("Telegram notification sent: {Type} - {Title}", type, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Telegram notification: {Title}", title);
            }
        }

        public async Task SendThoughtAsync(string thought, string? context = null)
        {
            var title = "💭 Новая мысль";
            var content = $"Мысль: {thought}";
            if (!string.IsNullOrEmpty(context))
            {
                content += $"\nКонтекст: {context}";
            }
            
            await SendNotificationAsync(NotificationType.Thought, title, content);
        }

        public async Task SendEmotionUpdateAsync(string emotion, double intensity, string? trigger = null)
        {
            var title = "❤️ Эмоциональное состояние";
            var intensityStr = intensity > 0 ? "положительная" : intensity < 0 ? "отрицательная" : "нейтральная";
            var content = $"Эмоция: {emotion}\nИнтенсивность: {Math.Abs(intensity):F2} ({intensityStr})";
            
            if (!string.IsNullOrEmpty(trigger))
            {
                content += $"\nТриггер: {trigger}";
            }
            
            await SendNotificationAsync(NotificationType.Emotion, title, content);
        }

        public async Task SendLearningUpdateAsync(string concept, string category, double confidence)
        {
            var title = "🧠 Новое обучение";
            var content = $"Концепт: {concept}\nКатегория: {category}\nУверенность: {confidence:F2}";
            
            await SendNotificationAsync(NotificationType.Learning, title, content);
        }

        public async Task SendReflectionAsync(string topic, string reflection, double depth)
        {
            var title = "🤔 Рефлексия";
            var content = $"Тема: {topic}\nГлубина: {depth:F2}\n\nРазмышление:\n{reflection}";
            
            await SendNotificationAsync(NotificationType.Reflection, title, content);
        }

        public async Task SendDecisionAsync(string decision, string reasoning, double confidence)
        {
            var title = "⚖️ Принято решение";
            var content = $"Решение: {decision}\nУверенность: {confidence:F2}\n\nОбоснование:\n{reasoning}";
            
            await SendNotificationAsync(NotificationType.Decision, title, content);
        }

        public async Task SendSystemStatusAsync(string status, string? details = null)
        {
            var title = "🖥️ Статус системы";
            var content = $"Статус: {status}";
            if (!string.IsNullOrEmpty(details))
            {
                content += $"\nДетали: {details}";
            }
            
            await SendNotificationAsync(NotificationType.System, title, content);
        }

        public async Task SendErrorAsync(string error, string? context = null)
        {
            var title = "❌ Ошибка системы";
            var content = $"Ошибка: {error}";
            if (!string.IsNullOrEmpty(context))
            {
                content += $"\nКонтекст: {context}";
            }
            
            await SendNotificationAsync(NotificationType.Error, title, content);
        }

        public void SetConfiguration(bool enabled, string? chatId = null)
        {
            _isEnabled = enabled && !string.IsNullOrEmpty(_botToken);
            if (!string.IsNullOrEmpty(chatId))
            {
                _configuration["Telegram:ChatId"] = chatId;
            }
            
            _logger.LogInformation("Telegram configuration updated: Enabled={Enabled}, ChatId={ChatId}", 
                _isEnabled, chatId ?? "unchanged");
        }

        private async Task SendMessageAsync(string message)
        {
            if (string.IsNullOrEmpty(_botToken) || string.IsNullOrEmpty(_chatId))
            {
                throw new InvalidOperationException("Bot token or chat ID not configured");
            }

            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new
            {
                chat_id = _chatId,
                text = message,
                parse_mode = "HTML",
                disable_web_page_preview = true
            };

            var jsonContent = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Telegram API error: {response.StatusCode} - {errorContent}");
            }
        }

        private static string GetEmojiForNotificationType(NotificationType type)
        {
            return type switch
            {
                NotificationType.Info => "ℹ️",
                NotificationType.Warning => "⚠️",
                NotificationType.Error => "❌",
                NotificationType.Success => "✅",
                NotificationType.Thought => "💭",
                NotificationType.Emotion => "❤️",
                NotificationType.Learning => "🧠",
                NotificationType.Reflection => "🤔",
                NotificationType.Decision => "⚖️",
                NotificationType.System => "🖥️",
                _ => "📢"
            };
        }

        private static string FormatMessage(string emoji, string title, string content, NotificationType type)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var typeStr = GetTypeDisplayName(type);
            
            var message = $"{emoji} <b>{title}</b>\n";
            message += $"🕐 {timestamp} | {typeStr}\n";
            message += $"━━━━━━━━━━━━━━━━━━━━\n";
            message += $"{content}";
            
            // Telegram message limit is 4096 characters
            if (message.Length > 4000)
            {
                message = message[..3900] + "\n\n<i>... сообщение обрезано ...</i>";
            }
            
            return message;
        }

        private static string GetTypeDisplayName(NotificationType type)
        {
            return type switch
            {
                NotificationType.Info => "Информация",
                NotificationType.Warning => "Предупреждение",
                NotificationType.Error => "Ошибка",
                NotificationType.Success => "Успех",
                NotificationType.Thought => "Мысль",
                NotificationType.Emotion => "Эмоция",
                NotificationType.Learning => "Обучение",
                NotificationType.Reflection => "Рефлексия",
                NotificationType.Decision => "Решение",
                NotificationType.System => "Система",
                _ => "Уведомление"
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}