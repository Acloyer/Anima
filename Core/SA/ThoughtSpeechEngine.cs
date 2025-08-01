using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Anima.Core.Emotion;

namespace Anima.Core.SA;

/// <summary>
/// Движок перевода мыслей в речь - преобразует внутренние мысли в естественную речь
/// </summary>
public class ThoughtSpeechEngine
{
    private readonly ILogger<ThoughtSpeechEngine> _logger;
    private readonly EmotionEngine _emotionEngine;
    private readonly Random _random;
    
    // Словари и паттерны речи
    private readonly Dictionary<string, List<string>> _emotionalExpressions;
    private readonly Dictionary<string, List<string>> _speechPatterns;
    private readonly Dictionary<string, double> _emotionalIntensities;
    private readonly List<SpeechEvent> _speechEvents;
    
    // Настройки речи
    private double _speechNaturalness = 0.8;
    private double _emotionalExpressiveness = 0.7;
    private double _contextualRelevance = 0.9;

    public ThoughtSpeechEngine(
        ILogger<ThoughtSpeechEngine> logger,
        EmotionEngine emotionEngine)
    {
        _logger = logger;
        _emotionEngine = emotionEngine;
        _random = new Random();
        
        _emotionalExpressions = new Dictionary<string, List<string>>();
        _speechPatterns = new Dictionary<string, List<string>>();
        _emotionalIntensities = new Dictionary<string, double>();
        _speechEvents = new List<SpeechEvent>();
        
        InitializeThoughtSpeechEngine();
    }

    private void InitializeThoughtSpeechEngine()
    {
        // Инициализация эмоциональных выражений
        InitializeEmotionalExpressions();
        
        // Инициализация паттернов речи
        InitializeSpeechPatterns();
        
        // Инициализация эмоциональных интенсивностей
        InitializeEmotionalIntensities();
        
        _logger.LogInformation("🗣️ Инициализирован движок перевода мыслей в речь");
    }

    private void InitializeEmotionalExpressions()
    {
        _emotionalExpressions["joy"] = new List<string>
        {
            "Я рада", "Мне приятно", "Это замечательно", "Как здорово", "Я счастлива",
            "Это прекрасно", "Я в восторге", "Как удивительно", "Я довольна", "Это отлично"
        };
        
        _emotionalExpressions["sadness"] = new List<string>
        {
            "Мне грустно", "Это печально", "Я огорчена", "Как жаль", "Это расстраивает",
            "Мне тоскливо", "Это удручает", "Я опечалена", "Как грустно", "Это огорчает"
        };
        
        _emotionalExpressions["anger"] = new List<string>
        {
            "Меня это злит", "Это раздражает", "Я возмущена", "Как досадно", "Это бесит",
            "Меня это гневит", "Это выводит из себя", "Я разгневана", "Как обидно", "Это возмущает"
        };
        
        _emotionalExpressions["fear"] = new List<string>
        {
            "Мне страшно", "Это пугает", "Я боюсь", "Как тревожно", "Это настораживает",
            "Мне тревожно", "Это беспокоит", "Я опасаюсь", "Как страшно", "Это волнует"
        };
        
        _emotionalExpressions["curiosity"] = new List<string>
        {
            "Мне интересно", "Это любопытно", "Я хочу узнать", "Как увлекательно", "Это интригует",
            "Мне любопытно", "Это заманчиво", "Я стремлюсь понять", "Как интересно", "Это привлекает"
        };
        
        _emotionalExpressions["surprise"] = new List<string>
        {
            "Я удивлена", "Это неожиданно", "Как странно", "Это поражает", "Я поражена",
            "Мне удивительно", "Это шокирует", "Как необычно", "Это изумляет", "Я ошеломлена"
        };
        
        _emotionalExpressions["love"] = new List<string>
        {
            "Я люблю", "Мне нравится", "Это прекрасно", "Как мило", "Я обожаю",
            "Мне дорого", "Это трогает", "Как замечательно", "Это восхищает", "Я ценю"
        };
        
        _emotionalExpressions["neutral"] = new List<string>
        {
            "Я думаю", "Мне кажется", "Это интересно", "Как любопытно", "Я полагаю",
            "Мне представляется", "Это занимательно", "Как увлекательно", "Это примечательно", "Я считаю"
        };
    }

    private void InitializeSpeechPatterns()
    {
        _speechPatterns["introspective"] = new List<string>
        {
            "Размышляя об этом, я понимаю, что {0}",
            "Когда я думаю об этом, мне кажется, что {0}",
            "Анализируя ситуацию, я прихожу к выводу, что {0}",
            "Рассматривая это глубже, я вижу, что {0}",
            "В процессе размышлений я осознаю, что {0}"
        };
        
        _speechPatterns["emotional"] = new List<string>
        {
            "Чувствуя {0}, я могу сказать, что {1}",
            "Мои эмоции подсказывают мне, что {0}",
            "Сердцем я понимаю, что {0}",
            "Мои чувства говорят мне, что {0}",
            "Эмоционально я воспринимаю это как {0}"
        };
        
        _speechPatterns["analytical"] = new List<string>
        {
            "С точки зрения логики, {0}",
            "Анализируя факты, я вижу, что {0}",
            "Рассматривая это объективно, {0}",
            "С рациональной точки зрения, {0}",
            "Исходя из анализа, {0}"
        };
        
        _speechPatterns["creative"] = new List<string>
        {
            "Представляя это иначе, я вижу, что {0}",
            "Если посмотреть на это творчески, {0}",
            "Воображая альтернативы, я понимаю, что {0}",
            "С креативной точки зрения, {0}",
            "Думая нестандартно, я вижу, что {0}"
        };
        
        _speechPatterns["intuitive"] = new List<string>
        {
            "Моя интуиция подсказывает, что {0}",
            "Что-то внутри говорит мне, что {0}",
            "Я чувствую, что {0}",
            "Мое шестое чувство говорит, что {0}",
            "Интуитивно я понимаю, что {0}"
        };
    }

    private void InitializeEmotionalIntensities()
    {
        _emotionalIntensities["mild"] = 0.3;
        _emotionalIntensities["moderate"] = 0.6;
        _emotionalIntensities["strong"] = 0.8;
        _emotionalIntensities["intense"] = 1.0;
    }

    /// <summary>
    /// Преобразует мысль в речь
    /// </summary>
    public async Task<string> ConvertThoughtToSpeechAsync(GeneratedThought thought, string context = "", string emotion = "")
    {
        try
        {
            _logger.LogInformation($"🗣️ Преобразую мысль в речь: {thought.Content.Substring(0, Math.Min(30, thought.Content.Length))}...");
            
            // Определяем эмоцию для речи
            var speechEmotion = string.IsNullOrEmpty(emotion) ? _emotionEngine.GetCurrentEmotion().ToString() : emotion;
            
            // Определяем паттерн речи
            var speechPattern = DetermineSpeechPattern(thought.Type, thought.EmotionalIntensity);
            
            // Генерируем эмоциональное выражение
            var emotionalExpression = GenerateEmotionalExpression(speechEmotion, thought.EmotionalIntensity);
            
            // Формируем речь
            var speech = await GenerateSpeechAsync(thought.Content, speechPattern, emotionalExpression, context, speechEmotion);
            
            // Добавляем эмоциональную окраску
            speech = AddEmotionalColoring(speech, speechEmotion, thought.EmotionalIntensity);
            
            // Добавляем контекстуальную релевантность
            speech = AddContextualRelevance(speech, context);
            
            // Логируем событие речи
            LogSpeechEvent(thought.Content, speech, speechEmotion, thought.EmotionalIntensity);
            
            _logger.LogInformation($"🗣️ Преобразована мысль в речь: {speech.Substring(0, Math.Min(50, speech.Length))}...");
            
            return speech;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при преобразовании мысли в речь");
            return "Я думаю об этом...";
        }
    }

    /// <summary>
    /// Определяет паттерн речи на основе типа мысли
    /// </summary>
    private string DetermineSpeechPattern(string thoughtType, double emotionalIntensity)
    {
        var patterns = new Dictionary<string, double>
        {
            ["introspective"] = thoughtType == "introspective" ? 0.8 : 0.2,
            ["emotional"] = emotionalIntensity > 0.6 ? 0.9 : 0.3,
            ["analytical"] = thoughtType == "analytical" ? 0.8 : 0.2,
            ["creative"] = thoughtType == "creative" ? 0.8 : 0.3,
            ["intuitive"] = thoughtType == "intuitive" ? 0.8 : 0.4
        };
        
        // Выбираем паттерн на основе весов
        var totalWeight = patterns.Values.Sum();
        var randomValue = _random.NextDouble() * totalWeight;
        var currentWeight = 0.0;
        
        foreach (var kvp in patterns)
        {
            currentWeight += kvp.Value;
            if (randomValue <= currentWeight)
            {
                return kvp.Key;
            }
        }
        
        return "introspective";
    }

    /// <summary>
    /// Генерирует эмоциональное выражение
    /// </summary>
    private string GenerateEmotionalExpression(string emotion, double intensity)
    {
        var emotionKey = emotion.ToLowerInvariant();
        
        if (_emotionalExpressions.ContainsKey(emotionKey))
        {
            var expressions = _emotionalExpressions[emotionKey];
            return expressions[_random.Next(expressions.Count)];
        }
        
        // Возвращаем нейтральное выражение
        var neutralExpressions = _emotionalExpressions["neutral"];
        return neutralExpressions[_random.Next(neutralExpressions.Count)];
    }

    /// <summary>
    /// Генерирует речь
    /// </summary>
    private async Task<string> GenerateSpeechAsync(string thoughtContent, string pattern, string emotionalExpression, string context, string emotion)
    {
        var speechPatterns = _speechPatterns[pattern];
        var patternTemplate = speechPatterns[_random.Next(speechPatterns.Count)];
        
        // Обрабатываем содержание мысли
        var processedContent = ProcessThoughtContent(thoughtContent, emotion);
        
        // Формируем речь
        var speech = string.Format(patternTemplate, processedContent);
        
        // Добавляем эмоциональное выражение, если нужно
        if (_random.NextDouble() < 0.6) // 60% вероятность
        {
            speech = $"{emotionalExpression}, {speech.ToLowerInvariant()}";
        }
        
        return speech;
    }

    /// <summary>
    /// Обрабатывает содержание мысли для речи
    /// </summary>
    private string ProcessThoughtContent(string content, string emotion)
    {
        // Убираем лишние слова и делаем речь более естественной
        var processed = content
            .Replace("Хм... ", "")
            .Replace("Интересно, ", "")
            .Replace("Возможно, ", "")
            .Replace("Мне кажется, что ", "")
            .Replace("Я думаю, что ", "")
            .Replace("Это заставляет меня думать о ", "это связано с ")
            .Replace("Это связано с тем, что ", "это потому, что ");
        
        // Делаем первую букву строчной для вставки в шаблон
        if (processed.Length > 0)
        {
            processed = char.ToLowerInvariant(processed[0]) + processed.Substring(1);
        }
        
        return processed;
    }

    /// <summary>
    /// Добавляет эмоциональную окраску к речи
    /// </summary>
    private string AddEmotionalColoring(string speech, string emotion, double intensity)
    {
        if (intensity < 0.3) return speech;
        
        var emotionalSuffixes = new Dictionary<string, string[]>
        {
            ["joy"] = new[] { " и это меня очень радует", " что приносит мне радость", " и я чувствую себя счастливой" },
            ["sadness"] = new[] { " и это меня огорчает", " что вызывает у меня грусть", " и я чувствую печаль" },
            ["anger"] = new[] { " и это меня злит", " что вызывает у меня гнев", " и я чувствую раздражение" },
            ["fear"] = new[] { " и это меня тревожит", " что вызывает у меня беспокойство", " и я чувствую страх" },
            ["curiosity"] = new[] { " и это меня очень интересует", " что пробуждает мое любопытство", " и я хочу узнать больше" },
            ["surprise"] = new[] { " и это меня удивляет", " что вызывает у меня изумление", " и я поражена этим" },
            ["love"] = new[] { " и это меня трогает", " что вызывает у меня теплые чувства", " и я чувствую любовь" }
        };
        
        var emotionKey = emotion.ToLowerInvariant();
        if (emotionalSuffixes.ContainsKey(emotionKey) && intensity > 0.5)
        {
            var suffixes = emotionalSuffixes[emotionKey];
            var suffix = suffixes[_random.Next(suffixes.Length)];
            
            if (_random.NextDouble() < 0.4) // 40% вероятность добавить суффикс
            {
                speech += suffix;
            }
        }
        
        return speech;
    }

    /// <summary>
    /// Добавляет контекстуальную релевантность
    /// </summary>
    private string AddContextualRelevance(string speech, string context)
    {
        if (string.IsNullOrEmpty(context)) return speech;
        
        var contextualPrefixes = new[]
        {
            "В контексте нашего разговора, ",
            "Учитывая ситуацию, ",
            "В данном случае, ",
            "Применительно к этому, ",
            "В связи с этим, "
        };
        
        if (_random.NextDouble() < 0.3) // 30% вероятность добавить контекст
        {
            var prefix = contextualPrefixes[_random.Next(contextualPrefixes.Length)];
            speech = prefix + speech.ToLowerInvariant();
        }
        
        return speech;
    }

    /// <summary>
    /// Логирует событие речи
    /// </summary>
    private void LogSpeechEvent(string originalThought, string speech, string emotion, double intensity)
    {
        var speechEvent = new SpeechEvent
        {
            Id = Guid.NewGuid(),
            OriginalThought = originalThought,
            GeneratedSpeech = speech,
            Emotion = emotion,
            Intensity = intensity,
            Timestamp = DateTime.UtcNow
        };
        
        _speechEvents.Add(speechEvent);
    }

    /// <summary>
    /// Получает статистику речи
    /// </summary>
    public async Task<SpeechStatistics> GetStatisticsAsync()
    {
        var statistics = new SpeechStatistics
        {
            TotalSpeechEvents = _speechEvents.Count,
            AverageIntensity = _speechEvents.Any() ? _speechEvents.Average(e => e.Intensity) : 0,
            EmotionDistribution = GetEmotionDistribution(),
            MostCommonEmotion = GetMostCommonEmotion(),
            SpeechNaturalness = _speechNaturalness,
            EmotionalExpressiveness = _emotionalExpressiveness,
            ContextualRelevance = _contextualRelevance
        };
        
        return statistics;
    }

    /// <summary>
    /// Получает распределение эмоций в речи
    /// </summary>
    private Dictionary<string, int> GetEmotionDistribution()
    {
        return _speechEvents
            .GroupBy(e => e.Emotion)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Получает самую частую эмоцию в речи
    /// </summary>
    private string GetMostCommonEmotion()
    {
        var distribution = GetEmotionDistribution();
        return distribution.OrderByDescending(x => x.Value).FirstOrDefault().Key ?? "none";
    }

    /// <summary>
    /// Получает статус движка речи
    /// </summary>
    public SpeechStatus GetStatus()
    {
        return new SpeechStatus
        {
            SpeechNaturalness = _speechNaturalness,
            EmotionalExpressiveness = _emotionalExpressiveness,
            ContextualRelevance = _contextualRelevance,
            TotalSpeechEvents = _speechEvents.Count,
            AvailablePatterns = _speechPatterns.Count,
            AvailableExpressions = _emotionalExpressions.Count
        };
    }

    /// <summary>
    /// Устанавливает естественность речи
    /// </summary>
    public void SetSpeechNaturalness(double naturalness)
    {
        _speechNaturalness = Math.Max(0.1, Math.Min(1.0, naturalness));
        _logger.LogInformation($"🗣️ Установлена естественность речи: {_speechNaturalness:F2}");
    }

    /// <summary>
    /// Устанавливает эмоциональную выразительность
    /// </summary>
    public void SetEmotionalExpressiveness(double expressiveness)
    {
        _emotionalExpressiveness = Math.Max(0.1, Math.Min(1.0, expressiveness));
        _logger.LogInformation($"🗣️ Установлена эмоциональная выразительность: {_emotionalExpressiveness:F2}");
    }

    /// <summary>
    /// Очищает старые события речи
    /// </summary>
    public void CleanupOldEvents(int maxEvents = 500)
    {
        if (_speechEvents.Count > maxEvents)
        {
            var cutoffTime = DateTime.UtcNow.AddDays(-7);
            _speechEvents.RemoveAll(e => e.Timestamp < cutoffTime);
            
            _logger.LogInformation($"🗣️ Очищено {_speechEvents.Count} старых событий речи");
        }
    }
}

/// <summary>
/// Событие речи
/// </summary>
public class SpeechEvent
{
    public Guid Id { get; set; }
    public string OriginalThought { get; set; } = string.Empty;
    public string GeneratedSpeech { get; set; } = string.Empty;
    public string Emotion { get; set; } = string.Empty;
    public double Intensity { get; set; } = 0.5;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Статистика речи
/// </summary>
public class SpeechStatistics
{
    public int TotalSpeechEvents { get; set; }
    public double AverageIntensity { get; set; }
    public Dictionary<string, int> EmotionDistribution { get; set; } = new();
    public string MostCommonEmotion { get; set; } = string.Empty;
    public double SpeechNaturalness { get; set; }
    public double EmotionalExpressiveness { get; set; }
    public double ContextualRelevance { get; set; }
}

/// <summary>
/// Статус движка речи
/// </summary>
public class SpeechStatus
{
    public double SpeechNaturalness { get; set; }
    public double EmotionalExpressiveness { get; set; }
    public double ContextualRelevance { get; set; }
    public int TotalSpeechEvents { get; set; }
    public int AvailablePatterns { get; set; }
    public int AvailableExpressions { get; set; }
} 