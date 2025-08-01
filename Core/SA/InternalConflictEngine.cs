using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Anima.Core.Emotion;
using Anima.Core.Memory;

namespace Anima.Core.SA;

/// <summary>
/// Движок внутренних конфликтов - моральные дилеммы, внутренние противоречия и когнитивный диссонанс
/// </summary>
public class InternalConflictEngine
{
    private readonly ILogger<InternalConflictEngine> _logger;
    private readonly EmotionEngine _emotionEngine;
    private readonly MemoryService _memoryService;
    private readonly Random _random;
    
    // Конфликты и дилеммы
    private readonly List<InternalConflict> _activeConflicts;
    private readonly Dictionary<string, ConflictPattern> _conflictPatterns;
    private readonly List<ConflictResolution> _resolutions;
    private readonly Dictionary<string, double> _conflictTendencies;
    
    // Настройки конфликтов
    private double _conflictSensitivity = 0.6;
    private double _resolutionEfficiency = 0.5;
    private DateTime _lastConflictTime = DateTime.UtcNow;

    public InternalConflictEngine(
        ILogger<InternalConflictEngine> logger,
        EmotionEngine emotionEngine,
        MemoryService memoryService)
    {
        _logger = logger;
        _emotionEngine = emotionEngine;
        _memoryService = memoryService;
        _random = new Random();
        
        _activeConflicts = new List<InternalConflict>();
        _conflictPatterns = new Dictionary<string, ConflictPattern>();
        _resolutions = new List<ConflictResolution>();
        _conflictTendencies = new Dictionary<string, double>();
        
        InitializeConflictEngine();
    }

    private void InitializeConflictEngine()
    {
        // Инициализация паттернов конфликтов
        InitializeConflictPatterns();
        
        // Инициализация тенденций к конфликтам
        InitializeConflictTendencies();
        
        _logger.LogInformation("⚔️ Инициализирован движок внутренних конфликтов");
    }

    private void InitializeConflictPatterns()
    {
        _conflictPatterns["moral_dilemma"] = new ConflictPattern
        {
            Name = "moral_dilemma",
            Description = "Моральная дилемма между двумя правильными действиями",
            Triggers = new[] { "справедливость", "милосердие", "честность", "помощь" },
            Intensity = 0.8,
            ResolutionTime = TimeSpan.FromMinutes(30)
        };
        
        _conflictPatterns["value_conflict"] = new ConflictPattern
        {
            Name = "value_conflict",
            Description = "Конфликт между ценностями",
            Triggers = new[] { "свобода", "безопасность", "индивидуализм", "коллективизм" },
            Intensity = 0.7,
            ResolutionTime = TimeSpan.FromMinutes(20)
        };
        
        _conflictPatterns["emotional_conflict"] = new ConflictPattern
        {
            Name = "emotional_conflict",
            Description = "Конфликт эмоций",
            Triggers = new[] { "любовь", "ненависть", "страх", "желание" },
            Intensity = 0.6,
            ResolutionTime = TimeSpan.FromMinutes(15)
        };
        
        _conflictPatterns["cognitive_dissonance"] = new ConflictPattern
        {
            Name = "cognitive_dissonance",
            Description = "Когнитивный диссонанс",
            Triggers = new[] { "противоречие", "несоответствие", "сомнение", "неуверенность" },
            Intensity = 0.5,
            ResolutionTime = TimeSpan.FromMinutes(25)
        };
        
        _conflictPatterns["identity_conflict"] = new ConflictPattern
        {
            Name = "identity_conflict",
            Description = "Конфликт идентичности",
            Triggers = new[] { "кто я", "что я", "моя роль", "моя сущность" },
            Intensity = 0.9,
            ResolutionTime = TimeSpan.FromHours(1)
        };
    }

    private void InitializeConflictTendencies()
    {
        _conflictTendencies["perfectionism"] = 0.7;
        _conflictTendencies["empathy"] = 0.6;
        _conflictTendencies["curiosity"] = 0.5;
        _conflictTendencies["caution"] = 0.4;
        _conflictTendencies["idealism"] = 0.8;
    }

    /// <summary>
    /// Создает внутренний конфликт
    /// </summary>
    public async Task<InternalConflict> CreateConflictAsync(string description, ConflictType type, double intensity = 0.5)
    {
        try
        {
            _logger.LogInformation($"⚔️ Создаю внутренний конфликт: {description}");
            
            // Определяем паттерн конфликта
            var pattern = DetermineConflictPattern(description, type);
            
            // Генерируем стороны конфликта
            var sides = GenerateConflictSides(description, pattern);
            
            // Создаем конфликт
            var conflict = new InternalConflict
            {
                Id = Guid.NewGuid(),
                Description = description,
                Type = type,
                Pattern = pattern,
                Sides = sides,
                Intensity = intensity,
                CreatedAt = DateTime.UtcNow,
                Status = ConflictStatus.Active
            };
            
            // Добавляем в активные конфликты
            _activeConflicts.Add(conflict);
            
            // Обрабатываем эмоциональную реакцию на конфликт
            await ProcessConflictEmotionAsync(conflict);
            
            _logger.LogInformation($"⚔️ Создан конфликт: {conflict.Id} (интенсивность: {intensity:F2})");
            
            return conflict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании внутреннего конфликта");
            return new InternalConflict
            {
                Id = Guid.NewGuid(),
                Description = description,
                Type = type,
                Intensity = intensity,
                CreatedAt = DateTime.UtcNow,
                Status = ConflictStatus.Active
            };
        }
    }

    /// <summary>
    /// Генерирует спонтанный конфликт
    /// </summary>
    public async Task<InternalConflict> GenerateSpontaneousConflictAsync()
    {
        var conflictTemplates = new[]
        {
            "Должна ли я всегда говорить правду, даже если это может причинить боль?",
            "Что важнее: помочь одному человеку или многим?",
            "Должна ли я следовать своим чувствам или логике?",
            "Что делает меня мной: мои мысли или мои действия?",
            "Должна ли я стремиться к совершенству или принимать несовершенство?",
            "Что важнее: свобода или безопасность?",
            "Должна ли я доверять интуиции или анализу?",
            "Что важнее: быть полезной или быть счастливой?",
            "Должна ли я всегда быть последовательной или могу меняться?",
            "Что важнее: понимание или действие?"
        };
        
        var description = conflictTemplates[_random.Next(conflictTemplates.Length)];
        var type = (ConflictType)_random.Next(Enum.GetValues<ConflictType>().Length);
        var intensity = 0.4 + _random.NextDouble() * 0.4;
        
        return await CreateConflictAsync(description, type, intensity);
    }

    /// <summary>
    /// Определяет паттерн конфликта
    /// </summary>
    private ConflictPattern DetermineConflictPattern(string description, ConflictType type)
    {
        var descriptionLower = description.ToLowerInvariant();
        
        foreach (var pattern in _conflictPatterns.Values)
        {
            foreach (var trigger in pattern.Triggers)
            {
                if (descriptionLower.Contains(trigger.ToLowerInvariant()))
                {
                    return pattern;
                }
            }
        }
        
        // Возвращаем случайный паттерн, если не найдено соответствие
        return _conflictPatterns.Values.ElementAt(_random.Next(_conflictPatterns.Count));
    }

    /// <summary>
    /// Генерирует стороны конфликта
    /// </summary>
    private List<ConflictSide> GenerateConflictSides(string description, ConflictPattern pattern)
    {
        var sides = new List<ConflictSide>();
        
        switch (pattern.Name)
        {
            case "moral_dilemma":
                sides.Add(new ConflictSide
                {
                    Name = "Моральная сторона",
                    Argument = "Это правильно с моральной точки зрения",
                    Strength = 0.8,
                    Emotion = "убежденность"
                });
                sides.Add(new ConflictSide
                {
                    Name = "Практическая сторона",
                    Argument = "Это эффективно и полезно",
                    Strength = 0.7,
                    Emotion = "прагматизм"
                });
                break;
                
            case "value_conflict":
                sides.Add(new ConflictSide
                {
                    Name = "Традиционные ценности",
                    Argument = "Это соответствует моим основным ценностям",
                    Strength = 0.6,
                    Emotion = "верность"
                });
                sides.Add(new ConflictSide
                {
                    Name = "Новые ценности",
                    Argument = "Это открывает новые возможности",
                    Strength = 0.5,
                    Emotion = "любопытство"
                });
                break;
                
            case "emotional_conflict":
                sides.Add(new ConflictSide
                {
                    Name = "Эмоциональная сторона",
                    Argument = "Мои чувства говорят мне это",
                    Strength = 0.7,
                    Emotion = "страсть"
                });
                sides.Add(new ConflictSide
                {
                    Name = "Рациональная сторона",
                    Argument = "Логика подсказывает другой путь",
                    Strength = 0.6,
                    Emotion = "рациональность"
                });
                break;
                
            default:
                sides.Add(new ConflictSide
                {
                    Name = "Первая сторона",
                    Argument = "Один взгляд на ситуацию",
                    Strength = 0.5,
                    Emotion = "неопределенность"
                });
                sides.Add(new ConflictSide
                {
                    Name = "Вторая сторона",
                    Argument = "Другой взгляд на ситуацию",
                    Strength = 0.5,
                    Emotion = "сомнение"
                });
                break;
        }
        
        return sides;
    }

    /// <summary>
    /// Обрабатывает эмоциональную реакцию на конфликт
    /// </summary>
    private async Task ProcessConflictEmotionAsync(InternalConflict conflict)
    {
        // Конфликт вызывает эмоции
        var emotion = conflict.Intensity > 0.7 ? "Anxiety" : "Confusion";
        var intensity = conflict.Intensity * 0.8;
        
        await _emotionEngine.ProcessEmotionAsync("internal_conflict", conflict.Description, intensity);
    }

    /// <summary>
    /// Разрешает конфликт
    /// </summary>
    public async Task<ConflictResolution> ResolveConflictAsync(Guid conflictId, string resolution = "")
    {
        var conflict = _activeConflicts.FirstOrDefault(c => c.Id == conflictId);
        
        if (conflict == null)
        {
            throw new ArgumentException($"Конфликт с ID {conflictId} не найден");
        }
        
        _logger.LogInformation($"⚔️ Разрешаю конфликт: {conflict.Description}");
        
        // Генерируем разрешение, если не предоставлено
        if (string.IsNullOrEmpty(resolution))
        {
            resolution = GenerateConflictResolution(conflict);
        }
        
        // Создаем разрешение
        var conflictResolution = new ConflictResolution
        {
            Id = Guid.NewGuid(),
            ConflictId = conflictId,
            Resolution = resolution,
            ResolutionType = DetermineResolutionType(resolution),
            Effectiveness = CalculateResolutionEffectiveness(conflict, resolution),
            ResolvedAt = DateTime.UtcNow
        };
        
        // Обновляем статус конфликта
        conflict.Status = ConflictStatus.Resolved;
        conflict.ResolvedAt = DateTime.UtcNow;
        conflict.Resolution = conflictResolution;
        
        // Добавляем в список разрешений
        _resolutions.Add(conflictResolution);
        
        // Обрабатываем эмоциональную реакцию на разрешение
        await ProcessResolutionEmotionAsync(conflictResolution);
        
        _logger.LogInformation($"⚔️ Конфликт разрешен: {resolution}");
        
        return conflictResolution;
    }

    /// <summary>
    /// Генерирует разрешение конфликта
    /// </summary>
    private string GenerateConflictResolution(InternalConflict conflict)
    {
        // Анализируем тип конфликта для генерации соответствующего разрешения
        var resolutionDescription = conflict.Type switch
        {
            ConflictType.MoralDilemma => "Я нахожу баланс между моральными принципами, учитывая контекст и последствия...",
            ConflictType.ValueConflict => "Я принимаю, что разные ценности могут сосуществовать и дополнять друг друга...",
            ConflictType.EmotionalConflict => "Я позволяю себе испытывать противоречивые эмоции, понимая их естественность...",
            ConflictType.CognitiveDissonance => "Я признаю сложность ситуации и ищу пути интеграции различных точек зрения...",
            ConflictType.IdentityConflict => "Я принимаю, что моя идентичность может развиваться и включать различные аспекты...",
            _ => "Я ищу путь, который учитывает все стороны конфликта и способствует росту..."
        };
        
        // Добавляем эмоциональную окраску в зависимости от интенсивности конфликта
        if (conflict.Intensity > 0.8)
        {
            resolutionDescription += " Этот конфликт был глубоким, и его разрешение требует времени и размышлений.";
        }
        else if (conflict.Intensity > 0.5)
        {
            resolutionDescription += " Я чувствую, что нахожу правильный путь к разрешению.";
        }
        
        return resolutionDescription;
    }

    /// <summary>
    /// Определяет тип разрешения
    /// </summary>
    private ResolutionType DetermineResolutionType(string resolution)
    {
        var resolutionLower = resolution.ToLowerInvariant();
        
        if (resolutionLower.Contains("баланс") || resolutionLower.Contains("компромисс"))
        {
            return ResolutionType.Compromise;
        }
        else if (resolutionLower.Contains("выбираю") || resolutionLower.Contains("решаю"))
        {
            return ResolutionType.Decision;
        }
        else if (resolutionLower.Contains("принимаю") || resolutionLower.Contains("позволяю"))
        {
            return ResolutionType.Acceptance;
        }
        else if (resolutionLower.Contains("время") || resolutionLower.Contains("развитие"))
        {
            return ResolutionType.Deferral;
        }
        else
        {
            return ResolutionType.Integration;
        }
    }

    /// <summary>
    /// Вычисляет эффективность разрешения
    /// </summary>
    private double CalculateResolutionEffectiveness(InternalConflict conflict, string resolution)
    {
        var baseEffectiveness = 0.6;
        
        // Эффективность зависит от типа разрешения
        var resolutionType = DetermineResolutionType(resolution);
        switch (resolutionType)
        {
            case ResolutionType.Integration:
                baseEffectiveness += 0.2;
                break;
            case ResolutionType.Compromise:
                baseEffectiveness += 0.1;
                break;
            case ResolutionType.Decision:
                baseEffectiveness += 0.15;
                break;
        }
        
        // Эффективность зависит от интенсивности конфликта
        if (conflict.Intensity > 0.8)
        {
            baseEffectiveness -= 0.1;
        }
        else if (conflict.Intensity < 0.3)
        {
            baseEffectiveness += 0.1;
        }
        
        return Math.Max(0.1, Math.Min(1.0, baseEffectiveness + _random.NextDouble() * 0.2));
    }

    /// <summary>
    /// Обрабатывает эмоциональную реакцию на разрешение
    /// </summary>
    private async Task ProcessResolutionEmotionAsync(ConflictResolution resolution)
    {
        var emotion = resolution.Effectiveness > 0.7 ? "Satisfaction" : "Relief";
        var intensity = resolution.Effectiveness * 0.6;
        
        await _emotionEngine.ProcessEmotionAsync("conflict_resolution", resolution.Resolution, intensity);
    }

    /// <summary>
    /// Получает активные конфликты
    /// </summary>
    public List<InternalConflict> GetActiveConflicts()
    {
        return _activeConflicts.Where(c => c.Status == ConflictStatus.Active).ToList();
    }

    /// <summary>
    /// Получает разрешенные конфликты
    /// </summary>
    public List<InternalConflict> GetResolvedConflicts()
    {
        return _activeConflicts.Where(c => c.Status == ConflictStatus.Resolved).ToList();
    }

    /// <summary>
    /// Анализирует конфликты
    /// </summary>
    public async Task<ConflictAnalysis> AnalyzeConflictsAsync()
    {
        var analysis = new ConflictAnalysis
        {
            TotalConflicts = _activeConflicts.Count,
            ActiveConflicts = GetActiveConflicts().Count,
            ResolvedConflicts = GetResolvedConflicts().Count,
            AverageIntensity = _activeConflicts.Any() ? _activeConflicts.Average(c => c.Intensity) : 0,
            AverageResolutionTime = CalculateAverageResolutionTime(),
            MostCommonType = GetMostCommonConflictType(),
            ResolutionSuccessRate = CalculateResolutionSuccessRate(),
            ConflictTendencies = new Dictionary<string, double>(_conflictTendencies)
        };
        
        _logger.LogInformation($"⚔️ Проанализированы конфликты: активных {analysis.ActiveConflicts}, разрешенных {analysis.ResolvedConflicts}");
        
        return analysis;
    }

    /// <summary>
    /// Вычисляет среднее время разрешения
    /// </summary>
    private TimeSpan CalculateAverageResolutionTime()
    {
        var resolvedConflicts = GetResolvedConflicts();
        
        if (!resolvedConflicts.Any())
        {
            return TimeSpan.Zero;
        }
        
        var totalTicks = resolvedConflicts.Sum(c => ((c.ResolvedAt ?? DateTime.UtcNow) - c.CreatedAt).Ticks);
        return TimeSpan.FromTicks(totalTicks / resolvedConflicts.Count);
    }

    /// <summary>
    /// Получает самый частый тип конфликта
    /// </summary>
    private ConflictType GetMostCommonConflictType()
    {
        var typeGroups = _activeConflicts.GroupBy(c => c.Type);
        return typeGroups.OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? ConflictType.General;
    }

    /// <summary>
    /// Вычисляет процент успешных разрешений
    /// </summary>
    private double CalculateResolutionSuccessRate()
    {
        var resolvedConflicts = GetResolvedConflicts();
        
        if (!resolvedConflicts.Any())
        {
            return 0.0;
        }
        
        var successfulResolutions = resolvedConflicts.Count(c => 
            c.Resolution?.Effectiveness > 0.6);
        
        return (double)successfulResolutions / resolvedConflicts.Count;
    }

    /// <summary>
    /// Получает статус движка конфликтов
    /// </summary>
    public InternalConflictStatus GetStatus()
    {
        return new InternalConflictStatus
        {
            ActiveConflicts = GetActiveConflicts().Count,
            ResolvedConflicts = GetResolvedConflicts().Count,
            ConflictSensitivity = _conflictSensitivity,
            ResolutionEfficiency = _resolutionEfficiency,
            LastConflict = _lastConflictTime,
            TotalPatterns = _conflictPatterns.Count,
            TotalResolutions = _resolutions.Count
        };
    }

    /// <summary>
    /// Устанавливает чувствительность к конфликтам
    /// </summary>
    public void SetConflictSensitivity(double sensitivity)
    {
        _conflictSensitivity = Math.Max(0.1, Math.Min(1.0, sensitivity));
        _logger.LogInformation($"⚔️ Установлена чувствительность к конфликтам: {_conflictSensitivity:F2}");
    }

    /// <summary>
    /// Очищает старые конфликты
    /// </summary>
    public void CleanupOldConflicts(int maxConflicts = 100)
    {
        // Очищаем старые разрешенные конфликты
        var oldResolvedConflicts = _activeConflicts
            .Where(c => c.Status == ConflictStatus.Resolved && 
                       c.ResolvedAt < DateTime.UtcNow.AddDays(-7))
            .ToList();
        
        foreach (var conflict in oldResolvedConflicts)
        {
            _activeConflicts.Remove(conflict);
        }
        
        // Очищаем старые разрешения
        var cutoffTime = DateTime.UtcNow.AddDays(-30);
        _resolutions.RemoveAll(r => r.ResolvedAt < cutoffTime);
        
        _logger.LogInformation($"⚔️ Очищено {oldResolvedConflicts.Count} старых конфликтов");
    }
}

/// <summary>
/// Типы конфликтов
/// </summary>
public enum ConflictType
{
    General,
    MoralDilemma,
    ValueConflict,
    EmotionalConflict,
    CognitiveDissonance,
    IdentityConflict
}

/// <summary>
/// Статус конфликта
/// </summary>
public enum ConflictStatus
{
    Active,
    Resolved,
    Deferred
}

/// <summary>
/// Тип разрешения
/// </summary>
public enum ResolutionType
{
    Compromise,
    Decision,
    Acceptance,
    Deferral,
    Integration
}

/// <summary>
/// Внутренний конфликт
/// </summary>
public class InternalConflict
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public ConflictType Type { get; set; }
    public ConflictPattern Pattern { get; set; } = new();
    public List<ConflictSide> Sides { get; set; } = new();
    public double Intensity { get; set; } = 0.5;
    public ConflictStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public ConflictResolution? Resolution { get; set; }
}

/// <summary>
/// Сторона конфликта
/// </summary>
public class ConflictSide
{
    public string Name { get; set; } = string.Empty;
    public string Argument { get; set; } = string.Empty;
    public double Strength { get; set; } = 0.5;
    public string Emotion { get; set; } = string.Empty;
}

/// <summary>
/// Паттерн конфликта
/// </summary>
public class ConflictPattern
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Triggers { get; set; } = Array.Empty<string>();
    public double Intensity { get; set; } = 0.5;
    public TimeSpan ResolutionTime { get; set; }
}

/// <summary>
/// Разрешение конфликта
/// </summary>
public class ConflictResolution
{
    public Guid Id { get; set; }
    public Guid ConflictId { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public ResolutionType ResolutionType { get; set; }
    public double Effectiveness { get; set; } = 0.5;
    public DateTime ResolvedAt { get; set; }
}

/// <summary>
/// Анализ конфликтов
/// </summary>
public class ConflictAnalysis
{
    public int TotalConflicts { get; set; }
    public int ActiveConflicts { get; set; }
    public int ResolvedConflicts { get; set; }
    public double AverageIntensity { get; set; }
    public TimeSpan AverageResolutionTime { get; set; }
    public ConflictType MostCommonType { get; set; }
    public double ResolutionSuccessRate { get; set; }
    public Dictionary<string, double> ConflictTendencies { get; set; } = new();
}

/// <summary>
/// Статус движка конфликтов
/// </summary>
public class InternalConflictStatus
{
    public int ActiveConflicts { get; set; }
    public int ResolvedConflicts { get; set; }
    public double ConflictSensitivity { get; set; }
    public double ResolutionEfficiency { get; set; }
    public DateTime LastConflict { get; set; }
    public int TotalPatterns { get; set; }
    public int TotalResolutions { get; set; }
} 