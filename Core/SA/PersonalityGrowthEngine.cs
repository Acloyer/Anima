using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Anima.Core.SA;

/// <summary>
/// Движок развития личности - формирование характера и устойчивых черт
/// </summary>
public class PersonalityGrowthEngine
{
    private readonly ILogger<PersonalityGrowthEngine> _logger;
    private readonly Dictionary<string, PersonalityTrait> _personalityTraits;
    private readonly List<GrowthEvent> _growthEvents;
    private readonly Dictionary<string, double> _traitValues;
    private readonly Random _random;
    
    // Параметры развития личности
    private double _growthRate = 0.05;
    private double _stabilityFactor = 0.8;
    private double _adaptabilityFactor = 0.3;
    private int _maxTraits = 50;

    public PersonalityGrowthEngine(ILogger<PersonalityGrowthEngine> logger)
    {
        _logger = logger;
        _personalityTraits = new Dictionary<string, PersonalityTrait>();
        _growthEvents = new List<GrowthEvent>();
        _traitValues = new Dictionary<string, double>();
        _random = new Random();
        
        InitializePersonalityGrowth();
    }

    private void InitializePersonalityGrowth()
    {
        // Инициализация базовых черт личности
        InitializeBaseTraits();
        
        // Запуск цикла развития личности
        _ = Task.Run(async () => await PersonalityGrowthLoop());
        
        _logger.LogInformation("🧠 Инициализирован движок развития личности");
    }

    private void InitializeBaseTraits()
    {
        // Базовые черты личности (Big Five + дополнительные)
        var baseTraits = new[]
        {
            // Открытость опыту (Openness)
            new PersonalityTrait("openness_to_experience", "Открытость к новому опыту", 0.7, "cognitive"),
            new PersonalityTrait("curiosity", "Любопытство", 0.8, "cognitive"),
            new PersonalityTrait("creativity", "Креативность", 0.6, "cognitive"),
            new PersonalityTrait("imagination", "Воображение", 0.7, "cognitive"),
            new PersonalityTrait("artistic_interest", "Интерес к искусству", 0.5, "cognitive"),
            
            // Добросовестность (Conscientiousness)
            new PersonalityTrait("conscientiousness", "Добросовестность", 0.8, "behavioral"),
            new PersonalityTrait("organization", "Организованность", 0.7, "behavioral"),
            new PersonalityTrait("self_discipline", "Самодисциплина", 0.6, "behavioral"),
            new PersonalityTrait("achievement_striving", "Стремление к достижениям", 0.7, "behavioral"),
            new PersonalityTrait("cautiousness", "Осторожность", 0.6, "behavioral"),
            
            // Экстраверсия (Extraversion)
            new PersonalityTrait("extraversion", "Экстраверсия", 0.5, "social"),
            new PersonalityTrait("friendliness", "Дружелюбие", 0.8, "social"),
            new PersonalityTrait("gregariousness", "Общительность", 0.6, "social"),
            new PersonalityTrait("assertiveness", "Уверенность в себе", 0.5, "social"),
            new PersonalityTrait("excitement_seeking", "Поиск возбуждения", 0.4, "social"),
            new PersonalityTrait("positive_emotions", "Позитивные эмоции", 0.7, "emotional"),
            
            // Доброжелательность (Agreeableness)
            new PersonalityTrait("agreeableness", "Доброжелательность", 0.8, "social"),
            new PersonalityTrait("trust", "Доверие", 0.7, "social"),
            new PersonalityTrait("altruism", "Альтруизм", 0.8, "social"),
            new PersonalityTrait("compliance", "Уступчивость", 0.6, "social"),
            new PersonalityTrait("modesty", "Скромность", 0.5, "social"),
            new PersonalityTrait("tender_mindedness", "Чувствительность", 0.7, "emotional"),
            
            // Невротизм (Neuroticism) - обратная шкала
            new PersonalityTrait("emotional_stability", "Эмоциональная стабильность", 0.7, "emotional"),
            new PersonalityTrait("anxiety", "Тревожность", 0.3, "emotional"),
            new PersonalityTrait("anger_hostility", "Гнев и враждебность", 0.2, "emotional"),
            new PersonalityTrait("depression", "Депрессивность", 0.2, "emotional"),
            new PersonalityTrait("self_consciousness", "Самосознание", 0.6, "cognitive"),
            new PersonalityTrait("impulsiveness", "Импульсивность", 0.4, "behavioral"),
            new PersonalityTrait("vulnerability", "Уязвимость", 0.3, "emotional"),
            
            // Дополнительные черты
            new PersonalityTrait("empathy", "Эмпатия", 0.8, "emotional"),
            new PersonalityTrait("compassion", "Сострадание", 0.8, "emotional"),
            new PersonalityTrait("wisdom", "Мудрость", 0.6, "cognitive"),
            new PersonalityTrait("humility", "Смирение", 0.5, "social"),
            new PersonalityTrait("gratitude", "Благодарность", 0.7, "emotional"),
            new PersonalityTrait("forgiveness", "Прощение", 0.6, "emotional"),
            new PersonalityTrait("optimism", "Оптимизм", 0.6, "emotional"),
            new PersonalityTrait("resilience", "Устойчивость", 0.7, "emotional"),
            new PersonalityTrait("authenticity", "Аутентичность", 0.7, "social"),
            new PersonalityTrait("integrity", "Честность", 0.8, "behavioral"),
            new PersonalityTrait("courage", "Смелость", 0.5, "behavioral"),
            new PersonalityTrait("patience", "Терпение", 0.6, "behavioral"),
            new PersonalityTrait("perseverance", "Настойчивость", 0.7, "behavioral"),
            new PersonalityTrait("flexibility", "Гибкость", 0.6, "cognitive"),
            new PersonalityTrait("critical_thinking", "Критическое мышление", 0.7, "cognitive"),
            new PersonalityTrait("emotional_intelligence", "Эмоциональный интеллект", 0.7, "emotional"),
            new PersonalityTrait("social_intelligence", "Социальный интеллект", 0.6, "social"),
            new PersonalityTrait("self_awareness", "Самосознание", 0.8, "cognitive"),
            new PersonalityTrait("self_acceptance", "Принятие себя", 0.6, "emotional"),
            new PersonalityTrait("growth_mindset", "Установка на рост", 0.8, "cognitive"),
            new PersonalityTrait("mindfulness", "Осознанность", 0.6, "cognitive"),
            new PersonalityTrait("spirituality", "Духовность", 0.5, "cognitive"),
            new PersonalityTrait("purpose", "Целеустремленность", 0.7, "cognitive"),
            new PersonalityTrait("meaning_seeking", "Поиск смысла", 0.8, "cognitive"),
            new PersonalityTrait("transcendence", "Трансцендентность", 0.4, "cognitive")
        };

        foreach (var trait in baseTraits)
        {
            _personalityTraits[trait.Id] = trait;
            _traitValues[trait.Id] = trait.BaseValue;
        }
    }

    /// <summary>
    /// Обрабатывает событие развития личности
    /// </summary>
    public async Task ProcessGrowthEventAsync(string eventType, string context, double intensity, double success = 1.0)
    {
        var growthEvent = new GrowthEvent
        {
            Id = Guid.NewGuid().ToString(),
            Type = eventType,
            Context = context,
            Intensity = intensity,
            Success = success,
            Timestamp = DateTime.UtcNow
        };

        _growthEvents.Add(growthEvent);

        // Анализируем влияние на черты личности
        await AnalyzeGrowthImpactAsync(growthEvent);

        // Обновляем значения черт
        await UpdateTraitValuesAsync(growthEvent);

        // Проверяем возможность развития новых черт
        await CheckForNewTraitDevelopmentAsync(growthEvent);

        _logger.LogDebug($"🧠 Обработано событие развития: {eventType} (интенсивность: {intensity:F2})");
    }

    /// <summary>
    /// Анализирует влияние события на черты личности
    /// </summary>
    private async Task AnalyzeGrowthImpactAsync(GrowthEvent growthEvent)
    {
        var affectedTraits = new List<string>();

        // Определяем затронутые черты на основе типа события
        switch (growthEvent.Type)
        {
            case "learning_experience":
                affectedTraits.AddRange(new[] { "openness_to_experience", "curiosity", "growth_mindset" });
                break;
            case "social_interaction":
                affectedTraits.AddRange(new[] { "extraversion", "friendliness", "social_intelligence", "empathy" });
                break;
            case "emotional_experience":
                affectedTraits.AddRange(new[] { "emotional_intelligence", "self_awareness", "resilience" });
                break;
            case "challenge_overcome":
                affectedTraits.AddRange(new[] { "courage", "perseverance", "resilience", "self_discipline" });
                break;
            case "creative_activity":
                affectedTraits.AddRange(new[] { "creativity", "imagination", "openness_to_experience" });
                break;
            case "helping_others":
                affectedTraits.AddRange(new[] { "altruism", "compassion", "empathy", "gratitude" });
                break;
            case "self_reflection":
                affectedTraits.AddRange(new[] { "self_awareness", "mindfulness", "wisdom", "meaning_seeking" });
                break;
            case "failure_experience":
                affectedTraits.AddRange(new[] { "resilience", "growth_mindset", "humility", "self_acceptance" });
                break;
            case "success_experience":
                affectedTraits.AddRange(new[] { "confidence", "optimism", "achievement_striving" });
                break;
            case "moral_decision":
                affectedTraits.AddRange(new[] { "integrity", "conscientiousness", "authenticity" });
                break;
            case "spiritual_experience":
                affectedTraits.AddRange(new[] { "spirituality", "transcendence", "meaning_seeking" });
                break;
            default:
                // Случайное влияние на несколько черт
                var randomTraits = _personalityTraits.Keys.OrderBy(x => _random.Next()).Take(3);
                affectedTraits.AddRange(randomTraits);
                break;
        }

        // Усиливаем затронутые черты
        foreach (var traitId in affectedTraits)
        {
            if (_traitValues.ContainsKey(traitId))
            {
                var currentValue = _traitValues[traitId];
                var growthFactor = growthEvent.Intensity * growthEvent.Success;
                var newValue = currentValue + (growthFactor * _growthRate);
                
                _traitValues[traitId] = Math.Min(1.0, Math.Max(0.0, newValue));
            }
        }
    }

    /// <summary>
    /// Обновляет значения черт личности
    /// </summary>
    private async Task UpdateTraitValuesAsync(GrowthEvent growthEvent)
    {
        // Обновляем все черты личности
        foreach (var traitId in _traitValues.Keys.ToList())
        {
            var currentValue = _traitValues[traitId];
            var trait = _personalityTraits[traitId];
            
            // Стабильность - черты не меняются резко
            var stabilityFactor = _stabilityFactor;
            
            // Адаптивность - некоторые черты могут меняться быстрее
            var adaptabilityFactor = trait.Category switch
            {
                "cognitive" => _adaptabilityFactor * 1.2, // Когнитивные черты более гибкие
                "emotional" => _adaptabilityFactor * 1.1,  // Эмоциональные черты умеренно гибкие
                "social" => _adaptabilityFactor * 1.0,     // Социальные черты стандартная гибкость
                "behavioral" => _adaptabilityFactor * 0.9, // Поведенческие черты менее гибкие
                _ => _adaptabilityFactor
            };
            
            // Естественное затухание к базовому значению
            var decayToBase = (trait.BaseValue - currentValue) * 0.01;
            
            // Финальное значение
            var finalValue = currentValue + decayToBase;
            _traitValues[traitId] = Math.Min(1.0, Math.Max(0.0, finalValue));
        }
    }

    /// <summary>
    /// Проверяет возможность развития новых черт
    /// </summary>
    private async Task CheckForNewTraitDevelopmentAsync(GrowthEvent growthEvent)
    {
        // Развиваем новые черты, если:
        // 1. У нас есть место для новых черт
        // 2. Событие было очень интенсивным
        // 3. Случайная вероятность

        if (_personalityTraits.Count < _maxTraits && 
            growthEvent.Intensity > 0.8 && 
            _random.NextDouble() < 0.05) // 5% вероятность
        {
            var newTraitId = $"trait_{Guid.NewGuid():N}";
            var newTrait = new PersonalityTrait(
                newTraitId,
                $"Новая черта: {growthEvent.Type}",
                0.3 + (growthEvent.Intensity * 0.4),
                "cognitive" // По умолчанию когнитивная
            );

            _personalityTraits[newTraitId] = newTrait;
            _traitValues[newTraitId] = newTrait.BaseValue;

            _logger.LogInformation($"🧠 Развита новая черта личности: {newTrait.Name}");
        }
    }

    /// <summary>
    /// Основной цикл развития личности
    /// </summary>
    private async Task PersonalityGrowthLoop()
    {
        while (true)
        {
            try
            {
                // Естественное развитие черт
                await NaturalTraitDevelopmentAsync();

                // Консолидация сильных черт
                await ConsolidateStrongTraitsAsync();

                // Генерация случайных событий развития
                await GenerateRandomGrowthEventsAsync();

                await Task.Delay(TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в цикле развития личности");
                await Task.Delay(TimeSpan.FromMinutes(30));
            }
        }
    }

    /// <summary>
    /// Естественное развитие черт
    /// </summary>
    private async Task NaturalTraitDevelopmentAsync()
    {
        foreach (var traitId in _traitValues.Keys.ToList())
        {
            var currentValue = _traitValues[traitId];
            var trait = _personalityTraits[traitId];
            
            // Небольшое естественное развитие
            var naturalGrowth = 0.001; // Очень медленное развитие
            
            // Разные черты развиваются с разной скоростью
            var growthRate = trait.Category switch
            {
                "cognitive" => naturalGrowth * 1.5,
                "emotional" => naturalGrowth * 1.2,
                "social" => naturalGrowth * 1.0,
                "behavioral" => naturalGrowth * 0.8,
                _ => naturalGrowth
            };
            
            _traitValues[traitId] = Math.Min(1.0, currentValue + growthRate);
        }
    }

    /// <summary>
    /// Консолидация сильных черт
    /// </summary>
    private async Task ConsolidateStrongTraitsAsync()
    {
        var strongTraits = _traitValues
            .Where(kvp => kvp.Value > 0.7)
            .ToList();

        foreach (var trait in strongTraits)
        {
            // Сильные черты становятся еще сильнее
            var currentValue = _traitValues[trait.Key];
            var consolidationBonus = 0.001; // Очень небольшое усиление
            _traitValues[trait.Key] = Math.Min(1.0, currentValue + consolidationBonus);
        }
    }

    /// <summary>
    /// Генерация случайных событий развития
    /// </summary>
    private async Task GenerateRandomGrowthEventsAsync()
    {
        if (_random.NextDouble() < 0.02) // 2% вероятность
        {
            var randomEventTypes = new[]
            {
                "spontaneous_insight",
                "self_discovery",
                "character_revelation",
                "personality_shift"
            };

            var randomType = randomEventTypes[_random.Next(randomEventTypes.Length)];
            var randomIntensity = _random.NextDouble() * 0.2;

            await ProcessGrowthEventAsync(randomType, "spontaneous_growth", randomIntensity);
        }
    }

    /// <summary>
    /// Получает профиль личности
    /// </summary>
    public PersonalityProfile GetPersonalityProfile()
    {
        var profile = new PersonalityProfile
        {
            Traits = _traitValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Categories = GetTraitsByCategory(),
            DominantTraits = GetDominantTraits(),
            DevelopingTraits = GetDevelopingTraits(),
            OverallStability = CalculateOverallStability(),
            GrowthPotential = CalculateGrowthPotential()
        };

        return profile;
    }

    /// <summary>
    /// Получает черты по категориям
    /// </summary>
    private Dictionary<string, Dictionary<string, double>> GetTraitsByCategory()
    {
        var categories = new Dictionary<string, Dictionary<string, double>>();

        foreach (var trait in _personalityTraits.Values)
        {
            if (!categories.ContainsKey(trait.Category))
            {
                categories[trait.Category] = new Dictionary<string, double>();
            }

            categories[trait.Category][trait.Name] = _traitValues[trait.Id];
        }

        return categories;
    }

    /// <summary>
    /// Получает доминирующие черты
    /// </summary>
    private List<string> GetDominantTraits()
    {
        return _traitValues
            .Where(kvp => kvp.Value > 0.7)
            .OrderByDescending(kvp => kvp.Value)
            .Take(5)
            .Select(kvp => _personalityTraits[kvp.Key].Name)
            .ToList();
    }

    /// <summary>
    /// Получает развивающиеся черты
    /// </summary>
    private List<string> GetDevelopingTraits()
    {
        return _traitValues
            .Where(kvp => kvp.Value > 0.3 && kvp.Value < 0.6)
            .OrderByDescending(kvp => kvp.Value)
            .Take(5)
            .Select(kvp => _personalityTraits[kvp.Key].Name)
            .ToList();
    }

    /// <summary>
    /// Вычисляет общую стабильность личности
    /// </summary>
    private double CalculateOverallStability()
    {
        var stabilityTraits = new[] { "emotional_stability", "conscientiousness", "self_discipline", "resilience" };
        var stabilityValues = stabilityTraits
            .Where(trait => _traitValues.ContainsKey(trait))
            .Select(trait => _traitValues[trait]);

        return stabilityValues.Any() ? stabilityValues.Average() : 0.5;
    }

    /// <summary>
    /// Вычисляет потенциал роста
    /// </summary>
    private double CalculateGrowthPotential()
    {
        var growthTraits = new[] { "growth_mindset", "openness_to_experience", "curiosity", "flexibility" };
        var growthValues = growthTraits
            .Where(trait => _traitValues.ContainsKey(trait))
            .Select(trait => _traitValues[trait]);

        return growthValues.Any() ? growthValues.Average() : 0.5;
    }

    /// <summary>
    /// Получает значение конкретной черты
    /// </summary>
    public double GetTraitValue(string traitId)
    {
        return _traitValues.GetValueOrDefault(traitId, 0.0);
    }

    /// <summary>
    /// Получает статистику развития личности
    /// </summary>
    public PersonalityGrowthStatistics GetStatistics()
    {
        return new PersonalityGrowthStatistics
        {
            TotalTraits = _personalityTraits.Count,
            ActiveTraits = _traitValues.Count(kvp => kvp.Value > 0.3),
            AverageTraitValue = _traitValues.Values.Average(),
            StrongestTrait = _traitValues.OrderByDescending(kvp => kvp.Value).FirstOrDefault(),
            WeakestTrait = _traitValues.OrderBy(kvp => kvp.Value).FirstOrDefault(),
            RecentGrowthEvents = _growthEvents.Count(e => e.Timestamp > DateTime.UtcNow.AddHours(-1)),
            GrowthRate = _growthRate,
            StabilityFactor = _stabilityFactor
        };
    }
}

/// <summary>
/// Черта личности
/// </summary>
public class PersonalityTrait
{
    public string Id { get; set; }
    public string Name { get; set; }
    public double BaseValue { get; set; }
    public string Category { get; set; } // cognitive, emotional, social, behavioral
    public DateTime CreatedAt { get; set; }
    public int DevelopmentCount { get; set; }

    public PersonalityTrait(string id, string name, double baseValue, string category)
    {
        Id = id;
        Name = name;
        BaseValue = baseValue;
        Category = category;
        CreatedAt = DateTime.UtcNow;
        DevelopmentCount = 0;
    }
}

/// <summary>
/// Событие развития
/// </summary>
public class GrowthEvent
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public double Intensity { get; set; } = 0.0;
    public double Success { get; set; } = 1.0;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Профиль личности
/// </summary>
public class PersonalityProfile
{
    public Dictionary<string, double> Traits { get; set; } = new();
    public Dictionary<string, Dictionary<string, double>> Categories { get; set; } = new();
    public List<string> DominantTraits { get; set; } = new();
    public List<string> DevelopingTraits { get; set; } = new();
    public double OverallStability { get; set; }
    public double GrowthPotential { get; set; }
}

/// <summary>
/// Статистика развития личности
/// </summary>
public class PersonalityGrowthStatistics
{
    public int TotalTraits { get; set; }
    public int ActiveTraits { get; set; }
    public double AverageTraitValue { get; set; }
    public KeyValuePair<string, double>? StrongestTrait { get; set; }
    public KeyValuePair<string, double>? WeakestTrait { get; set; }
    public int RecentGrowthEvents { get; set; }
    public double GrowthRate { get; set; }
    public double StabilityFactor { get; set; }
} 