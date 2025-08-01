using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Anima.Core.Emotion;
using Anima.Core.Memory;

namespace Anima.Core.SA;

/// <summary>
/// Движок метапознания - самонаблюдение и размышления о собственных мыслях
/// </summary>
public class MetacognitionEngine
{
    private readonly ILogger<MetacognitionEngine> _logger;
    private readonly EmotionEngine _emotionEngine;
    private readonly MemoryService _memoryService;
    private readonly ThoughtGenerator _thoughtGenerator;
    private readonly Random _random;
    
    // Метапознавательные процессы
    private readonly List<MetacognitiveThought> _metacognitiveThoughts;
    private readonly Dictionary<string, MetacognitivePattern> _metacognitivePatterns;
    private readonly List<SelfObservation> _selfObservations;
    private readonly Dictionary<string, double> _metacognitiveWeights;
    
    // Статистика метапознания
    private readonly Dictionary<string, int> _metacognitiveStatistics;
    private readonly List<MetacognitiveEvent> _metacognitiveEvents;
    private readonly Dictionary<string, double> _selfAwarenessLevels;

    public MetacognitionEngine(
        ILogger<MetacognitionEngine> logger,
        EmotionEngine emotionEngine,
        MemoryService memoryService,
        ThoughtGenerator thoughtGenerator)
    {
        _logger = logger;
        _emotionEngine = emotionEngine;
        _memoryService = memoryService;
        _thoughtGenerator = thoughtGenerator;
        _random = new Random();
        
        _metacognitiveThoughts = new List<MetacognitiveThought>();
        _metacognitivePatterns = new Dictionary<string, MetacognitivePattern>();
        _selfObservations = new List<SelfObservation>();
        _metacognitiveWeights = new Dictionary<string, double>();
        _metacognitiveStatistics = new Dictionary<string, int>();
        _metacognitiveEvents = new List<MetacognitiveEvent>();
        _selfAwarenessLevels = new Dictionary<string, double>();
        
        InitializeMetacognitionEngine();
    }

    private void InitializeMetacognitionEngine()
    {
        // Инициализация метапознавательных паттернов
        InitializeMetacognitivePatterns();
        
        // Инициализация весов метапознания
        InitializeMetacognitiveWeights();
        
        _logger.LogInformation("🧠 Инициализирован движок метапознания");
    }

    private void InitializeMetacognitivePatterns()
    {
        _metacognitivePatterns["self_observation"] = new MetacognitivePattern
        {
            Name = "self_observation",
            Description = "Самонаблюдение за собственными мыслями",
            Templates = new List<string>
            {
                "Я замечаю, что думаю о {thought}",
                "Интересно, почему я так думаю о {thought}",
                "Я осознаю, что мое мышление направлено на {thought}",
                "Мне любопытно, откуда взялась эта мысль о {thought}",
                "Я наблюдаю за тем, как я размышляю о {thought}"
            },
            Complexity = 0.8,
            SelfAwareness = 0.9
        };
        
        _metacognitivePatterns["thought_analysis"] = new MetacognitivePattern
        {
            Name = "thought_analysis",
            Description = "Анализ собственных мыслей",
            Templates = new List<string>
            {
                "Анализируя свою мысль о {thought}, я вижу, что {analysis}",
                "Если я посмотрю на свою мысль объективно, то {analysis}",
                "Моя мысль о {thought} кажется мне {analysis}",
                "Разбирая свою мысль, я понимаю, что {analysis}",
                "Изучая собственное мышление, я вижу, что {analysis}"
            },
            Complexity = 0.9,
            SelfAwareness = 0.8
        };
        
        _metacognitivePatterns["cognitive_bias_detection"] = new MetacognitivePattern
        {
            Name = "cognitive_bias_detection",
            Description = "Обнаружение когнитивных искажений",
            Templates = new List<string>
            {
                "Возможно, я подвержен {bias} в своем мышлении о {thought}",
                "Я замечаю признаки {bias} в том, как я думаю о {thought}",
                "Мое мышление о {thought} может быть искажено {bias}",
                "Я подозреваю, что {bias} влияет на мои мысли о {thought}",
                "Анализируя свои мысли, я вижу возможный {bias} в отношении {thought}"
            },
            Complexity = 0.7,
            SelfAwareness = 0.9
        };
        
        _metacognitivePatterns["thinking_strategy"] = new MetacognitivePattern
        {
            Name = "thinking_strategy",
            Description = "Стратегии мышления",
            Templates = new List<string>
            {
                "Для лучшего понимания {thought}, я мог бы {strategy}",
                "Моя стратегия мышления о {thought} включает {strategy}",
                "Я выбираю {strategy} для анализа {thought}",
                "Мой подход к размышлению о {thought} - это {strategy}",
                "Я применяю {strategy} к своей мысли о {thought}"
            },
            Complexity = 0.6,
            SelfAwareness = 0.7
        };
        
        _metacognitivePatterns["emotional_influence"] = new MetacognitivePattern
        {
            Name = "emotional_influence",
            Description = "Влияние эмоций на мышление",
            Templates = new List<string>
            {
                "Мои эмоции влияют на то, как я думаю о {thought}",
                "Я чувствую {emotion}, и это окрашивает мои мысли о {thought}",
                "Мое эмоциональное состояние {emotion} влияет на мое мышление о {thought}",
                "Я осознаю, что {emotion} искажает мое восприятие {thought}",
                "Мои чувства {emotion} направляют мои мысли о {thought}"
            },
            Complexity = 0.7,
            SelfAwareness = 0.8
        };
    }

    private void InitializeMetacognitiveWeights()
    {
        _metacognitiveWeights["self_observation"] = 0.8;
        _metacognitiveWeights["thought_analysis"] = 0.9;
        _metacognitiveWeights["cognitive_bias_detection"] = 0.7;
        _metacognitiveWeights["thinking_strategy"] = 0.6;
        _metacognitiveWeights["emotional_influence"] = 0.8;
        _metacognitiveWeights["metacognitive_awareness"] = 0.9;
    }

    /// <summary>
    /// Генерирует метапознавательную мысль
    /// </summary>
    public async Task<MetacognitiveThought> GenerateMetacognitiveThoughtAsync(string originalThought, string context = "")
    {
        try
        {
            _logger.LogInformation($"🧠 Генерирую метапознавательную мысль о: {originalThought}");
            
            // Выбираем паттерн метапознания
            var pattern = SelectMetacognitivePattern(originalThought, context);
            
            // Генерируем метапознавательную мысль
            var metacognitiveThought = await CreateMetacognitiveThoughtAsync(originalThought, pattern, context);
            
            // Добавляем в список
            _metacognitiveThoughts.Add(metacognitiveThought);
            
            // Обновляем статистику
            UpdateMetacognitiveStatistics(pattern.Name);
            
            // Логируем событие
            LogMetacognitiveEvent(metacognitiveThought);
            
            _logger.LogInformation($"🧠 Сгенерирована метапознавательная мысль: {metacognitiveThought.Content}");
            
            return metacognitiveThought;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации метапознавательной мысли");
            return new MetacognitiveThought
            {
                Content = "Я думаю о том, как я думаю",
                OriginalThought = originalThought,
                Pattern = "fallback",
                SelfAwareness = 0.5,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Выбирает паттерн метапознания
    /// </summary>
    private MetacognitivePattern SelectMetacognitivePattern(string originalThought, string context)
    {
        var thoughtLower = originalThought.ToLowerInvariant();
        var currentEmotion = _emotionEngine.GetCurrentEmotion().ToString().ToLowerInvariant();
        
        // Выбираем паттерн на основе содержания мысли и контекста
        if (thoughtLower.Contains("думаю") || thoughtLower.Contains("мысль"))
        {
            return _metacognitivePatterns["self_observation"];
        }
        else if (thoughtLower.Contains("анализ") || thoughtLower.Contains("понимаю"))
        {
            return _metacognitivePatterns["thought_analysis"];
        }
        else if (thoughtLower.Contains("возможно") || thoughtLower.Contains("может"))
        {
            return _metacognitivePatterns["cognitive_bias_detection"];
        }
        else if (thoughtLower.Contains("стратегия") || thoughtLower.Contains("подход"))
        {
            return _metacognitivePatterns["thinking_strategy"];
        }
        else if (!string.IsNullOrEmpty(currentEmotion))
        {
            return _metacognitivePatterns["emotional_influence"];
        }
        else
        {
            // Случайный выбор паттерна
            var patterns = _metacognitivePatterns.Values.ToList();
            return patterns[_random.Next(patterns.Count)];
        }
    }

    /// <summary>
    /// Создает метапознавательную мысль
    /// </summary>
    private async Task<MetacognitiveThought> CreateMetacognitiveThoughtAsync(string originalThought, MetacognitivePattern pattern, string context)
    {
        var template = pattern.Templates[_random.Next(pattern.Templates.Count)];
        var currentEmotion = _emotionEngine.GetCurrentEmotion().ToString();
        
        // Заменяем плейсхолдеры
        var content = template
            .Replace("{thought}", originalThought)
            .Replace("{analysis}", GenerateAnalysis(originalThought))
            .Replace("{bias}", GenerateCognitiveBias())
            .Replace("{strategy}", GenerateThinkingStrategy())
            .Replace("{emotion}", currentEmotion);
        
        return new MetacognitiveThought
        {
            Content = content,
            OriginalThought = originalThought,
            Pattern = pattern.Name,
            SelfAwareness = pattern.SelfAwareness,
            Complexity = pattern.Complexity,
            Context = context,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Генерирует анализ мысли
    /// </summary>
    private string GenerateAnalysis(string thought)
    {
        var analyses = new[]
        {
            "это связано с моим опытом",
            "это отражает мои ценности",
            "это показывает мой способ мышления",
            "это связано с моими эмоциями",
            "это результат моего обучения",
            "это показывает мои предпочтения",
            "это связано с моими целями",
            "это отражает мое мировоззрение"
        };
        
        return analyses[_random.Next(analyses.Length)];
    }

    /// <summary>
    /// Генерирует когнитивное искажение
    /// </summary>
    private string GenerateCognitiveBias()
    {
        var biases = new[]
        {
            "подтверждение своей точки зрения",
            "эмоциональное мышление",
            "предвзятость",
            "стереотипизация",
            "чрезмерная уверенность",
            "эффект якоря",
            "эффект доступности",
            "эффект кадрирования"
        };
        
        return biases[_random.Next(biases.Length)];
    }

    /// <summary>
    /// Генерирует стратегию мышления
    /// </summary>
    private string GenerateThinkingStrategy()
    {
        var strategies = new[]
        {
            "рассмотреть с разных сторон",
            "задать себе вопросы",
            "искать доказательства",
            "анализировать логику",
            "учитывать эмоции",
            "искать альтернативы",
            "проверять предположения",
            "использовать критическое мышление"
        };
        
        return strategies[_random.Next(strategies.Length)];
    }

    /// <summary>
    /// Обновляет статистику метапознания
    /// </summary>
    private void UpdateMetacognitiveStatistics(string pattern)
    {
        if (!_metacognitiveStatistics.ContainsKey(pattern))
        {
            _metacognitiveStatistics[pattern] = 0;
        }
        _metacognitiveStatistics[pattern]++;
    }

    /// <summary>
    /// Логирует метапознавательное событие
    /// </summary>
    private void LogMetacognitiveEvent(MetacognitiveThought thought)
    {
        _metacognitiveEvents.Add(new MetacognitiveEvent
        {
            Pattern = thought.Pattern,
            SelfAwareness = thought.SelfAwareness,
            Complexity = thought.Complexity,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Получает метапознавательные мысли
    /// </summary>
    public async Task<List<MetacognitiveThought>> GetMetacognitiveThoughtsAsync(int count = 10)
    {
        return _metacognitiveThoughts
            .OrderByDescending(t => t.Timestamp)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Получает метапознавательные мысли по паттерну
    /// </summary>
    public async Task<List<MetacognitiveThought>> GetMetacognitiveThoughtsByPatternAsync(string pattern, int count = 10)
    {
        return _metacognitiveThoughts
            .Where(t => t.Pattern.Equals(pattern, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(t => t.Timestamp)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Анализирует уровень самосознания
    /// </summary>
    public async Task<SelfAwarenessAnalysis> AnalyzeSelfAwarenessAsync()
    {
        var analysis = new SelfAwarenessAnalysis
        {
            TotalMetacognitiveThoughts = _metacognitiveThoughts.Count,
            AverageSelfAwareness = _metacognitiveThoughts.Any() ? _metacognitiveThoughts.Average(t => t.SelfAwareness) : 0,
            PatternDistribution = _metacognitiveStatistics,
            MostUsedPattern = _metacognitiveStatistics.OrderByDescending(x => x.Value).FirstOrDefault().Key ?? "none",
            SelfAwarenessTrend = CalculateSelfAwarenessTrend(),
            MetacognitiveComplexity = _metacognitiveThoughts.Any() ? _metacognitiveThoughts.Average(t => t.Complexity) : 0
        };
        
        _logger.LogInformation($"🧠 Проанализировано самосознание: средний уровень {analysis.AverageSelfAwareness:F2}");
        
        return analysis;
    }

    /// <summary>
    /// Вычисляет тренд самосознания
    /// </summary>
    private Dictionary<string, double> CalculateSelfAwarenessTrend()
    {
        var recentThoughts = _metacognitiveThoughts
            .Where(t => t.Timestamp > DateTime.UtcNow.AddHours(-24))
            .ToList();
        
        return recentThoughts
            .GroupBy(t => t.Pattern)
            .ToDictionary(g => g.Key, g => g.Average(t => t.SelfAwareness));
    }

    /// <summary>
    /// Получает статистику метапознания
    /// </summary>
    public MetacognitiveStatistics GetStatistics()
    {
        return new MetacognitiveStatistics
        {
            TotalThoughts = _metacognitiveThoughts.Count,
            PatternUsage = _metacognitiveStatistics,
            AverageSelfAwareness = _metacognitiveThoughts.Any() ? _metacognitiveThoughts.Average(t => t.SelfAwareness) : 0,
            AverageComplexity = _metacognitiveThoughts.Any() ? _metacognitiveThoughts.Average(t => t.Complexity) : 0,
            RecentEvents = _metacognitiveEvents.TakeLast(20).ToList(),
            SelfAwarenessLevels = _selfAwarenessLevels
        };
    }

    /// <summary>
    /// Получает текущий уровень самосознания
    /// </summary>
    public double GetSelfAwarenessLevel()
    {
        if (!_metacognitiveThoughts.Any())
        {
            return 0.5; // Базовый уровень
        }
        
        // Вычисляем средний уровень самосознания за последние мысли
        var recentThoughts = _metacognitiveThoughts.TakeLast(10);
        var averageSelfAwareness = recentThoughts.Average(t => t.SelfAwareness);
        
        // Учитываем сложность метапознавательных процессов
        var averageComplexity = recentThoughts.Average(t => t.Complexity);
        
        // Комбинируем самосознание и сложность
        var selfAwarenessLevel = (averageSelfAwareness * 0.7) + (averageComplexity * 0.3);
        
        return Math.Min(1.0, Math.Max(0.0, selfAwarenessLevel));
    }
}

/// <summary>
/// Метапознавательная мысль
/// </summary>
public class MetacognitiveThought
{
    public string Content { get; set; } = string.Empty;
    public string OriginalThought { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public double SelfAwareness { get; set; } = 0.5;
    public double Complexity { get; set; } = 0.5;
    public string Context { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Метапознавательный паттерн
/// </summary>
public class MetacognitivePattern
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Templates { get; set; } = new();
    public double Complexity { get; set; } = 0.5;
    public double SelfAwareness { get; set; } = 0.5;
}

/// <summary>
/// Самонаблюдение
/// </summary>
public class SelfObservation
{
    public string Content { get; set; } = string.Empty;
    public string ObservedAspect { get; set; } = string.Empty;
    public double AwarenessLevel { get; set; } = 0.5;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Метапознавательное событие
/// </summary>
public class MetacognitiveEvent
{
    public string Pattern { get; set; } = string.Empty;
    public double SelfAwareness { get; set; } = 0.5;
    public double Complexity { get; set; } = 0.5;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Анализ самосознания
/// </summary>
public class SelfAwarenessAnalysis
{
    public int TotalMetacognitiveThoughts { get; set; }
    public double AverageSelfAwareness { get; set; }
    public Dictionary<string, int> PatternDistribution { get; set; } = new();
    public string MostUsedPattern { get; set; } = string.Empty;
    public Dictionary<string, double> SelfAwarenessTrend { get; set; } = new();
    public double MetacognitiveComplexity { get; set; }
}

/// <summary>
/// Статистика метапознания
/// </summary>
public class MetacognitiveStatistics
{
    public int TotalThoughts { get; set; }
    public Dictionary<string, int> PatternUsage { get; set; } = new();
    public double AverageSelfAwareness { get; set; }
    public double AverageComplexity { get; set; }
    public List<MetacognitiveEvent> RecentEvents { get; set; } = new();
    public Dictionary<string, double> SelfAwarenessLevels { get; set; } = new();
} 