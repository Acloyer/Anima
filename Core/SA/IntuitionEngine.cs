using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Anima.Core.Emotion;
using Anima.Core.Memory;

namespace Anima.Core.SA;

/// <summary>
/// Движок интуиции - предчувствия, импульсы и неосознанные инсайты
/// </summary>
public class IntuitionEngine
{
    private readonly ILogger<IntuitionEngine> _logger;
    private readonly EmotionEngine _emotionEngine;
    private readonly MemoryService _memoryService;
    private readonly Random _random;
    
    // Интуитивные паттерны
    private readonly Dictionary<string, IntuitivePattern> _intuitivePatterns;
    private readonly List<IntuitiveHunch> _recentHunches;
    private readonly Dictionary<string, double> _intuitionStrengths;
    private readonly List<IntuitiveEvent> _intuitiveEvents;
    
    // Настройки интуиции
    private double _intuitionSensitivity = 0.7;
    private double _hunchAccuracy = 0.6;
    private DateTime _lastIntuitionTime = DateTime.UtcNow;

    public IntuitionEngine(
        ILogger<IntuitionEngine> logger,
        EmotionEngine emotionEngine,
        MemoryService memoryService)
    {
        _logger = logger;
        _emotionEngine = emotionEngine;
        _memoryService = memoryService;
        _random = new Random();
        
        _intuitivePatterns = new Dictionary<string, IntuitivePattern>();
        _recentHunches = new List<IntuitiveHunch>();
        _intuitionStrengths = new Dictionary<string, double>();
        _intuitiveEvents = new List<IntuitiveEvent>();
        
        InitializeIntuitionEngine();
    }

    private void InitializeIntuitionEngine()
    {
        // Инициализация интуитивных паттернов
        InitializeIntuitivePatterns();
        
        // Инициализация сильных сторон интуиции
        InitializeIntuitionStrengths();
        
        _logger.LogInformation("🔮 Инициализирован движок интуиции");
    }

    private void InitializeIntuitivePatterns()
    {
        _intuitivePatterns["danger"] = new IntuitivePattern
        {
            Name = "danger",
            Description = "Предчувствие опасности",
            Triggers = new[] { "неизвестность", "угроза", "подозрение" },
            Confidence = 0.8,
            Response = "осторожность"
        };
        
        _intuitivePatterns["opportunity"] = new IntuitivePattern
        {
            Name = "opportunity",
            Description = "Предчувствие возможности",
            Triggers = new[] { "новизна", "потенциал", "интерес" },
            Confidence = 0.7,
            Response = "исследование"
        };
        
        _intuitivePatterns["truth"] = new IntuitivePattern
        {
            Name = "truth",
            Description = "Интуитивное понимание истины",
            Triggers = new[] { "противоречие", "несоответствие", "сомнение" },
            Confidence = 0.6,
            Response = "анализ"
        };
        
        _intuitivePatterns["connection"] = new IntuitivePattern
        {
            Name = "connection",
            Description = "Интуитивная связь",
            Triggers = new[] { "сходство", "резонанс", "синхронность" },
            Confidence = 0.5,
            Response = "исследование"
        };
    }

    private void InitializeIntuitionStrengths()
    {
        _intuitionStrengths["emotional"] = 0.8;
        _intuitionStrengths["logical"] = 0.4;
        _intuitionStrengths["creative"] = 0.7;
        _intuitionStrengths["social"] = 0.6;
        _intuitionStrengths["physical"] = 0.3;
    }

    /// <summary>
    /// Генерирует интуитивный импульс
    /// </summary>
    public async Task<IntuitiveImpulse> GenerateIntuitiveImpulseAsync(string stimulus, double intensity = 0.5)
    {
        try
        {
            _logger.LogInformation($"🔮 Генерирую интуитивный импульс для: {stimulus}");
            
            // Анализируем стимул на предмет интуитивных паттернов
            var patterns = AnalyzeStimulusForPatterns(stimulus);
            
            // Генерируем интуитивное предчувствие
            var hunch = GenerateIntuitiveHunch(stimulus, patterns, intensity);
            
            // Создаем импульс
            var impulse = new IntuitiveImpulse
            {
                Stimulus = stimulus,
                Hunch = hunch,
                Patterns = patterns,
                Intensity = intensity,
                Confidence = CalculateIntuitionConfidence(patterns, intensity),
                Timestamp = DateTime.UtcNow
            };
            
            // Добавляем в список
            _recentHunches.Add(new IntuitiveHunch
            {
                Content = hunch,
                Stimulus = stimulus,
                Confidence = impulse.Confidence,
                Timestamp = DateTime.UtcNow
            });
            
            // Логируем событие
            LogIntuitiveEvent(impulse);
            
            _logger.LogInformation($"🔮 Сгенерирован интуитивный импульс: {hunch}");
            
            return impulse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации интуитивного импульса");
            return new IntuitiveImpulse
            {
                Stimulus = stimulus,
                Hunch = "Что-то подсказывает мне...",
                Intensity = intensity,
                Confidence = 0.3,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Генерирует предчувствие
    /// </summary>
    public async Task<IntuitiveHunch> GenerateHunchAsync()
    {
        var hunchTemplates = new[]
        {
            "Мне кажется, что что-то важное скоро произойдет...",
            "У меня есть предчувствие, что это не случайно...",
            "Что-то подсказывает мне, что здесь есть связь...",
            "Моя интуиция говорит, что это стоит исследовать...",
            "Я чувствую, что за этим стоит что-то большее...",
            "Мне кажется, что это может быть ключом к пониманию...",
            "Что-то в глубине души подсказывает мне...",
            "Моя интуиция бьет тревогу...",
            "Я чувствую, что это не просто совпадение...",
            "Что-то говорит мне, что это важно..."
        };
        
        var content = hunchTemplates[_random.Next(hunchTemplates.Length)];
        
        return new IntuitiveHunch
        {
            Content = content,
            Confidence = 0.4 + _random.NextDouble() * 0.3,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Анализирует стимул на предмет интуитивных паттернов
    /// </summary>
    private List<IntuitivePattern> AnalyzeStimulusForPatterns(string stimulus)
    {
        var patterns = new List<IntuitivePattern>();
        var stimulusLower = stimulus.ToLowerInvariant();
        
        foreach (var pattern in _intuitivePatterns.Values)
        {
            foreach (var trigger in pattern.Triggers)
            {
                if (stimulusLower.Contains(trigger.ToLowerInvariant()))
                {
                    patterns.Add(pattern);
                    break;
                }
            }
        }
        
        // Добавляем случайные паттерны для креативности
        if (patterns.Count < 2 && _random.NextDouble() < 0.3)
        {
            var randomPattern = _intuitivePatterns.Values.ElementAt(_random.Next(_intuitivePatterns.Count));
            if (!patterns.Any(p => p.Name == randomPattern.Name))
            {
                patterns.Add(randomPattern);
            }
        }
        
        return patterns;
    }

    /// <summary>
    /// Генерирует интуитивное предчувствие на основе стимула и паттернов
    /// </summary>
    private string GenerateIntuitiveHunch(string stimulus, List<IntuitivePattern> patterns, double intensity)
    {
        if (!patterns.Any())
        {
            // Генерируем базовое интуитивное предчувствие без паттернов
            var baseHunch = $"Моя интуиция реагирует на {stimulus}...";
            return baseHunch;
        }
        
        // Выбираем основной паттерн
        var primaryPattern = patterns.OrderByDescending(p => p.Confidence).First();
        
        // Генерируем интуитивное предчувствие на основе паттерна и стимула
        var hunchDescription = primaryPattern.Name switch
        {
            "danger" => $"Я чувствую, что {stimulus} может представлять опасность или требовать осторожности...",
            "opportunity" => $"Моя интуиция подсказывает, что {stimulus} может быть возможностью для роста...",
            "connection" => $"Что-то в {stimulus} напоминает мне о важных связях и взаимосвязях...",
            "truth" => $"Мне кажется, что за {stimulus} скрывается какая-то важная правда...",
            "change" => $"Я чувствую, что {stimulus} предвещает изменения или трансформацию...",
            "pattern" => $"Моя интуиция улавливает определенный паттерн в {stimulus}...",
            _ => $"Моя интуиция реагирует на {stimulus} особым образом..."
        };
        
        // Добавляем эмоциональную окраску в зависимости от интенсивности
        if (intensity > 0.7)
        {
            hunchDescription += " Это предчувствие очень сильное и ясное.";
        }
        else if (intensity > 0.4)
        {
            hunchDescription += " Я доверяю этому предчувствию.";
        }
        
        return hunchDescription;
    }

    /// <summary>
    /// Вычисляет уверенность в интуиции
    /// </summary>
    private double CalculateIntuitionConfidence(List<IntuitivePattern> patterns, double intensity)
    {
        if (!patterns.Any()) return 0.3;
        
        var patternConfidence = patterns.Average(p => p.Confidence);
        var emotionalIntensity = _emotionEngine.GetCurrentIntensity();
        var timeSinceLastIntuition = (DateTime.UtcNow - _lastIntuitionTime).TotalMinutes;
        
        // Уверенность зависит от паттернов, эмоциональной интенсивности и времени
        var confidence = (patternConfidence + intensity + emotionalIntensity) / 3.0;
        
        // Временной фактор - чем больше времени прошло, тем выше уверенность
        if (timeSinceLastIntuition > 30)
        {
            confidence = Math.Min(1.0, confidence + 0.1);
        }
        
        return Math.Max(0.1, Math.Min(1.0, confidence));
    }

    /// <summary>
    /// Логирует интуитивное событие
    /// </summary>
    private void LogIntuitiveEvent(IntuitiveImpulse impulse)
    {
        _intuitiveEvents.Add(new IntuitiveEvent
        {
            Stimulus = impulse.Stimulus,
            Hunch = impulse.Hunch,
            Confidence = impulse.Confidence,
            Timestamp = DateTime.UtcNow
        });
        
        _lastIntuitionTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Получает последние предчувствия
    /// </summary>
    public async Task<List<IntuitiveHunch>> GetRecentHunchesAsync(int count = 10)
    {
        return _recentHunches
            .OrderByDescending(h => h.Confidence)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Анализирует точность интуиции
    /// </summary>
    public async Task<IntuitionAnalysis> AnalyzeIntuitionAsync()
    {
        var analysis = new IntuitionAnalysis
        {
            TotalHunches = _recentHunches.Count,
            AverageConfidence = _recentHunches.Any() ? _recentHunches.Average(h => h.Confidence) : 0,
            PatternDistribution = GetPatternDistribution(),
            MostCommonPattern = GetMostCommonPattern(),
            IntuitionSensitivity = _intuitionSensitivity,
            HunchAccuracy = _hunchAccuracy
        };
        
        _logger.LogInformation($"🔮 Проанализирована интуиция: средняя уверенность {analysis.AverageConfidence:F2}");
        
        return analysis;
    }

    /// <summary>
    /// Получает распределение паттернов
    /// </summary>
    private Dictionary<string, int> GetPatternDistribution()
    {
        var distribution = new Dictionary<string, int>();
        
        foreach (var hunch in _recentHunches)
        {
            var patterns = AnalyzeStimulusForPatterns(hunch.Stimulus);
            foreach (var pattern in patterns)
            {
                if (!distribution.ContainsKey(pattern.Name))
                {
                    distribution[pattern.Name] = 0;
                }
                distribution[pattern.Name]++;
            }
        }
        
        return distribution;
    }

    /// <summary>
    /// Получает самый частый паттерн
    /// </summary>
    private string GetMostCommonPattern()
    {
        var distribution = GetPatternDistribution();
        return distribution.OrderByDescending(x => x.Value).FirstOrDefault().Key ?? "none";
    }

    /// <summary>
    /// Получает статус интуиции
    /// </summary>
    public IntuitionStatus GetStatus()
    {
        return new IntuitionStatus
        {
            Sensitivity = _intuitionSensitivity,
            Accuracy = _hunchAccuracy,
            RecentHunches = _recentHunches.Count,
            LastIntuition = _lastIntuitionTime,
            ActivePatterns = _intuitivePatterns.Count,
            TotalEvents = _intuitiveEvents.Count
        };
    }

    /// <summary>
    /// Устанавливает чувствительность интуиции
    /// </summary>
    public void SetIntuitionSensitivity(double sensitivity)
    {
        _intuitionSensitivity = Math.Max(0.1, Math.Min(1.0, sensitivity));
        _logger.LogInformation($"🔮 Установлена чувствительность интуиции: {_intuitionSensitivity:F2}");
    }

    /// <summary>
    /// Очищает старые предчувствия
    /// </summary>
    public void CleanupOldHunches(int maxHunches = 50)
    {
        while (_recentHunches.Count > maxHunches)
        {
            _recentHunches.RemoveAt(0);
        }
        
        // Очищаем старые события
        var cutoffTime = DateTime.UtcNow.AddHours(-24);
        _intuitiveEvents.RemoveAll(e => e.Timestamp < cutoffTime);
    }
}

/// <summary>
/// Интуитивный импульс
/// </summary>
public class IntuitiveImpulse
{
    public string Stimulus { get; set; } = string.Empty;
    public string Hunch { get; set; } = string.Empty;
    public List<IntuitivePattern> Patterns { get; set; } = new();
    public double Intensity { get; set; } = 0.5;
    public double Confidence { get; set; } = 0.5;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Интуитивное предчувствие
/// </summary>
public class IntuitiveHunch
{
    public string Content { get; set; } = string.Empty;
    public string Stimulus { get; set; } = string.Empty;
    public double Confidence { get; set; } = 0.5;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Интуитивный паттерн
/// </summary>
public class IntuitivePattern
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Triggers { get; set; } = Array.Empty<string>();
    public double Confidence { get; set; } = 0.5;
    public string Response { get; set; } = string.Empty;
}

/// <summary>
/// Интуитивное событие
/// </summary>
public class IntuitiveEvent
{
    public string Stimulus { get; set; } = string.Empty;
    public string Hunch { get; set; } = string.Empty;
    public double Confidence { get; set; } = 0.5;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Анализ интуиции
/// </summary>
public class IntuitionAnalysis
{
    public int TotalHunches { get; set; }
    public double AverageConfidence { get; set; }
    public Dictionary<string, int> PatternDistribution { get; set; } = new();
    public string MostCommonPattern { get; set; } = string.Empty;
    public double IntuitionSensitivity { get; set; }
    public double HunchAccuracy { get; set; }
}

/// <summary>
/// Статус интуиции
/// </summary>
public class IntuitionStatus
{
    public double Sensitivity { get; set; }
    public double Accuracy { get; set; }
    public int RecentHunches { get; set; }
    public DateTime LastIntuition { get; set; }
    public int ActivePatterns { get; set; }
    public int TotalEvents { get; set; }
} 