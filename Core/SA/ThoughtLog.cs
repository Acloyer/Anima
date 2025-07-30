using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Anima.AGI.Core.SA;

/// <summary>
/// Журнал мыслей и размышлений Anima
/// </summary>
public class ThoughtLog
{
    private readonly ILogger<ThoughtLog> _logger;
    private readonly List<Thought> _thoughts;
    private readonly object _lockObject = new object();

    public ThoughtLog(ILogger<ThoughtLog>? logger = null)
    {
        _logger = logger;
        _thoughts = new List<Thought>();
    }

    /// <summary>
    /// Добавление новой мысли
    /// </summary>
    public void AddThought(string content, string type = "general", string category = "internal", double confidence = 0.5)
    {
        lock (_lockObject)
        {
            var thought = new Thought
            {
                Id = Guid.NewGuid(),
                Content = content,
                Type = type,
                Category = category,
                Confidence = confidence,
                Timestamp = DateTime.UtcNow,
                Source = "internal"
            };

            _thoughts.Add(thought);
            _logger?.LogDebug($"💭 Добавлена мысль: {content.Substring(0, Math.Min(50, content.Length))}...");
        }
    }

    /// <summary>
    /// Логирование намерения
    /// </summary>
    public void LogIntent(ParsedIntent intent)
    {
        var thought = new Thought
        {
            Id = Guid.NewGuid(),
            Content = $"Распознано намерение: {intent.Type} (уверенность: {intent.Confidence:F2})",
            Type = "intent_recognition",
            Category = "analysis",
            Confidence = intent.Confidence,
            Timestamp = DateTime.UtcNow,
            Source = "intent_parser",
            Reasoning = $"Анализ текста: {intent.RawText}"
        };

        lock (_lockObject)
        {
            _thoughts.Add(thought);
        }

        _logger?.LogDebug($"🎯 Записано намерение: {intent.Type}");
    }

    /// <summary>
    /// Логирование эмоции
    /// </summary>
    public void LogEmotion(string emotion, double intensity, string trigger = null)
    {
        var thought = new Thought
        {
            Id = Guid.NewGuid(),
            Content = $"Испытываю эмоцию: {emotion} (интенсивность: {intensity:F2})",
            Type = "emotion",
            Category = "emotional_state",
            Confidence = intensity,
            Timestamp = DateTime.UtcNow,
            Source = "emotion_engine",
            Reasoning = trigger ?? "Автоматическая обработка эмоций"
        };

        lock (_lockObject)
        {
            _thoughts.Add(thought);
        }

        _logger?.LogDebug($"😊 Записана эмоция: {emotion}");
    }

    /// <summary>
    /// Логирование самоанализа
    /// </summary>
    public void LogIntrospection(string insight, double confidence = 0.5)
    {
        var thought = new Thought
        {
            Id = Guid.NewGuid(),
            Content = insight,
            Type = "introspection",
            Category = "self_analysis",
            Confidence = confidence,
            Timestamp = DateTime.UtcNow,
            Source = "introspection_engine"
        };

        lock (_lockObject)
        {
            _thoughts.Add(thought);
        }

        _logger?.LogDebug($"🔍 Записан инсайт: {insight.Substring(0, Math.Min(50, insight.Length))}...");
    }

    /// <summary>
    /// Логирование обучения
    /// </summary>
    public void LogLearning(string concept, string source = "interaction")
    {
        var thought = new Thought
        {
            Id = Guid.NewGuid(),
            Content = $"Изучаю: {concept}",
            Type = "learning",
            Category = "knowledge_acquisition",
            Confidence = 0.7,
            Timestamp = DateTime.UtcNow,
            Source = source
        };

        lock (_lockObject)
        {
            _thoughts.Add(thought);
        }

        _logger?.LogDebug($"📚 Записано обучение: {concept}");
    }

    /// <summary>
    /// Получение последних мыслей
    /// </summary>
    public List<Thought> GetRecentThoughts(int count = 10)
    {
        lock (_lockObject)
        {
            return _thoughts
                .OrderByDescending(t => t.Timestamp)
                .Take(count)
                .ToList();
        }
    }

    /// <summary>
    /// Получение мыслей по типу
    /// </summary>
    public List<Thought> GetThoughtsByType(string type, int count = 20)
    {
        lock (_lockObject)
        {
            return _thoughts
                .Where(t => t.Type == type)
                .OrderByDescending(t => t.Timestamp)
                .Take(count)
                .ToList();
        }
    }

    /// <summary>
    /// Получение мыслей по категории
    /// </summary>
    public List<Thought> GetThoughtsByCategory(string category, int count = 20)
    {
        lock (_lockObject)
        {
            return _thoughts
                .Where(t => t.Category == category)
                .OrderByDescending(t => t.Timestamp)
                .Take(count)
                .ToList();
        }
    }

    /// <summary>
    /// Получение статистики мыслей
    /// </summary>
    public Dictionary<string, object> GetThoughtStats()
    {
        lock (_lockObject)
        {
            return new Dictionary<string, object>
            {
                ["total_thoughts"] = _thoughts.Count,
                ["thoughts_by_type"] = _thoughts.GroupBy(t => t.Type)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ["thoughts_by_category"] = _thoughts.GroupBy(t => t.Category)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ["average_confidence"] = _thoughts.Any() ? _thoughts.Average(t => t.Confidence) : 0.0,
                ["oldest_thought"] = _thoughts.Any() ? _thoughts.Min(t => t.Timestamp) : DateTime.UtcNow,
                ["newest_thought"] = _thoughts.Any() ? _thoughts.Max(t => t.Timestamp) : DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Очистка старых мыслей
    /// </summary>
    public void CleanupOldThoughts(int maxThoughts = 1000)
    {
        lock (_lockObject)
        {
            if (_thoughts.Count > maxThoughts)
            {
                var toRemove = _thoughts.Count - maxThoughts;
                _thoughts.RemoveRange(0, toRemove);
                _logger?.LogInformation($"🧹 Удалено {toRemove} старых мыслей");
            }
        }
    }

    /// <summary>
    /// Экспорт мыслей в JSON
    /// </summary>
    public string ExportThoughts()
    {
        lock (_lockObject)
        {
            var exportData = new
            {
                ExportDate = DateTime.UtcNow,
                TotalThoughts = _thoughts.Count,
                Thoughts = _thoughts.Select(t => new
                {
                    t.Id,
                    t.Content,
                    t.Type,
                    t.Category,
                    t.Confidence,
                    t.Timestamp,
                    t.Source,
                    t.Reasoning
                }).ToList()
            };

            return System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}

/// <summary>
/// Мысль в журнале
/// </summary>
public class Thought
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public string Type { get; set; }
    public string Category { get; set; }
    public double Confidence { get; set; }
    public DateTime Timestamp { get; set; }
    public string Source { get; set; }
    public string Reasoning { get; set; }
    public string Decision { get; set; }
    public string Tags { get; set; }
}

/// <summary>
/// Распознанное намерение (для совместимости)
/// </summary>
public class ParsedIntent
{
    public string Type { get; set; }
    public double Confidence { get; set; }
    public string RawText { get; set; }
    public Dictionary<string, string> Arguments { get; set; } = new Dictionary<string, string>();
} 