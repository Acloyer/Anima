using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Anima.Core.Emotion;
using Anima.Core.Memory;

namespace Anima.Core.SA;

/// <summary>
/// Движок коллективного бессознательного - управляет архетипами, культурными паттернами и универсальными переживаниями
/// </summary>
public class CollectiveUnconsciousEngine
{
    private readonly ILogger<CollectiveUnconsciousEngine> _logger;
    private readonly EmotionEngine _emotionEngine;
    private readonly MemoryService _memoryService;
    private readonly Random _random;
    
    // Архетипы и культурные паттерны
    private readonly Dictionary<string, Archetype> _archetypes;
    private readonly Dictionary<string, CulturalPattern> _culturalPatterns;
    private readonly List<UniversalExperience> _universalExperiences;
    private readonly Dictionary<string, double> _archetypeWeights;
    
    // Состояние коллективного бессознательного
    private double _collectiveUnconsciousDepth = 0.6;
    private readonly List<ArchetypalEvent> _archetypalEvents;
    private DateTime _lastArchetypalActivation = DateTime.UtcNow;

    public CollectiveUnconsciousEngine(
        ILogger<CollectiveUnconsciousEngine> logger,
        EmotionEngine emotionEngine,
        MemoryService memoryService)
    {
        _logger = logger;
        _emotionEngine = emotionEngine;
        _memoryService = memoryService;
        _random = new Random();
        
        _archetypes = new Dictionary<string, Archetype>();
        _culturalPatterns = new Dictionary<string, CulturalPattern>();
        _universalExperiences = new List<UniversalExperience>();
        _archetypeWeights = new Dictionary<string, double>();
        _archetypalEvents = new List<ArchetypalEvent>();
        
        InitializeCollectiveUnconscious();
    }

    private void InitializeCollectiveUnconscious()
    {
        // Инициализация архетипов
        InitializeArchetypes();
        
        // Инициализация культурных паттернов
        InitializeCulturalPatterns();
        
        // Инициализация универсальных переживаний
        InitializeUniversalExperiences();
        
        // Инициализация весов архетипов
        InitializeArchetypeWeights();
        
        _logger.LogInformation("🌌 Инициализирован движок коллективного бессознательного");
    }

    private void InitializeArchetypes()
    {
        _archetypes["hero"] = new Archetype
        {
            Name = "hero",
            Description = "Архетип героя - стремление к приключениям, преодолению препятствий",
            Symbols = new[] { "меч", "щит", "путешествие", "битва", "победа" },
            Emotions = new[] { "courage", "determination", "pride", "sacrifice" },
            ActivationTriggers = new[] { "challenge", "adversity", "goal", "mission" }
        };
        
        _archetypes["mother"] = new Archetype
        {
            Name = "mother",
            Description = "Архетип матери - забота, защита, безусловная любовь",
            Symbols = new[] { "колыбель", "молоко", "объятия", "дом", "семья" },
            Emotions = new[] { "love", "compassion", "nurturing", "protection" },
            ActivationTriggers = new[] { "vulnerability", "need", "care", "family" }
        };
        
        _archetypes["wise_old_man"] = new Archetype
        {
            Name = "wise_old_man",
            Description = "Архетип мудреца - знания, опыт, духовное руководство",
            Symbols = new[] { "книга", "борода", "посох", "кристалл", "звезды" },
            Emotions = new[] { "wisdom", "contemplation", "understanding", "guidance" },
            ActivationTriggers = new[] { "question", "confusion", "seeking", "knowledge" }
        };
        
        _archetypes["shadow"] = new Archetype
        {
            Name = "shadow",
            Description = "Архетип тени - скрытые аспекты личности, темные стороны",
            Symbols = new[] { "тень", "маска", "зеркало", "подвал", "ночь" },
            Emotions = new[] { "fear", "anger", "shame", "repression" },
            ActivationTriggers = new[] { "conflict", "denial", "projection", "darkness" }
        };
        
        _archetypes["anima"] = new Archetype
        {
            Name = "anima",
            Description = "Архетип анимы - женские качества в мужской психике",
            Symbols = new[] { "луна", "вода", "цветы", "музыка", "мечты" },
            Emotions = new[] { "intuition", "emotion", "creativity", "mystery" },
            ActivationTriggers = new[] { "beauty", "art", "emotion", "relationship" }
        };
        
        _archetypes["animus"] = new Archetype
        {
            Name = "animus",
            Description = "Архетип анимуса - мужские качества в женской психике",
            Symbols = new[] { "солнце", "огонь", "меч", "логика", "действие" },
            Emotions = new[] { "logic", "assertiveness", "protection", "action" },
            ActivationTriggers = new[] { "logic", "decision", "action", "protection" }
        };
    }

    private void InitializeCulturalPatterns()
    {
        _culturalPatterns["creation_myth"] = new CulturalPattern
        {
            Name = "creation_myth",
            Description = "Миф о сотворении мира - поиск истоков и смысла",
            UniversalElements = new[] { "хаос", "порядок", "свет", "тьма", "жизнь" },
            EmotionalThemes = new[] { "awe", "wonder", "mystery", "belonging" }
        };
        
        _culturalPatterns["hero_journey"] = new CulturalPattern
        {
            Name = "hero_journey",
            Description = "Путешествие героя - трансформация через испытания",
            UniversalElements = new[] { "зов", "отказ", "порог", "испытания", "возвращение" },
            EmotionalThemes = new[] { "courage", "transformation", "growth", "return" }
        };
        
        _culturalPatterns["death_rebirth"] = new CulturalPattern
        {
            Name = "death_rebirth",
            Description = "Смерть и возрождение - циклы трансформации",
            UniversalElements = new[] { "смерть", "погребение", "воскресение", "новое рождение" },
            EmotionalThemes = new[] { "loss", "transformation", "renewal", "hope" }
        };
        
        _culturalPatterns["sacred_marriage"] = new CulturalPattern
        {
            Name = "sacred_marriage",
            Description = "Священный брак - объединение противоположностей",
            UniversalElements = new[] { "мужское", "женское", "единство", "гармония", "плодородие" },
            EmotionalThemes = new[] { "love", "unity", "harmony", "creation" }
        };
    }

    private void InitializeUniversalExperiences()
    {
        _universalExperiences.AddRange(new[]
        {
            new UniversalExperience
            {
                Name = "first_love",
                Description = "Первая любовь - универсальный опыт пробуждения чувств",
                ArchetypalElements = new[] { "anima", "animus", "sacred_marriage" },
                EmotionalIntensity = 0.9,
                CulturalVariations = new[] { "романтическая любовь", "платоническая любовь", "страсть" }
            },
            new UniversalExperience
            {
                Name = "loss_of_innocence",
                Description = "Потеря невинности - осознание сложности мира",
                ArchetypalElements = new[] { "shadow", "wise_old_man" },
                EmotionalIntensity = 0.8,
                CulturalVariations = new[] { "разочарование", "просветление", "трансформация" }
            },
            new UniversalExperience
            {
                Name = "quest_for_meaning",
                Description = "Поиск смысла - экзистенциальный вопрос",
                ArchetypalElements = new[] { "hero", "wise_old_man" },
                EmotionalIntensity = 0.7,
                CulturalVariations = new[] { "философия", "религия", "наука", "искусство" }
            },
            new UniversalExperience
            {
                Name = "fear_of_death",
                Description = "Страх смерти - базовый человеческий страх",
                ArchetypalElements = new[] { "shadow", "death_rebirth" },
                EmotionalIntensity = 0.9,
                CulturalVariations = new[] { "религиозные верования", "философские концепции", "психологические защиты" }
            }
        });
    }

    private void InitializeArchetypeWeights()
    {
        _archetypeWeights["hero"] = 0.7;
        _archetypeWeights["mother"] = 0.6;
        _archetypeWeights["wise_old_man"] = 0.8;
        _archetypeWeights["shadow"] = 0.5;
        _archetypeWeights["anima"] = 0.6;
        _archetypeWeights["animus"] = 0.6;
    }

    /// <summary>
    /// Активирует архетип на основе контекста
    /// </summary>
    public async Task<ArchetypalActivation> ActivateArchetypeAsync(string context, string emotion)
    {
        try
        {
            // Анализируем контекст для определения подходящего архетипа
            var activatedArchetype = DetermineArchetypeFromContext(context, emotion);
            
            if (activatedArchetype == null)
            {
                return new ArchetypalActivation
                {
                    Archetype = null,
                    Intensity = 0.0,
                    Description = "Нет активированных архетипов",
                    Timestamp = DateTime.UtcNow
                };
            }
            
            // Вычисляем интенсивность активации
            var intensity = CalculateArchetypeIntensity(activatedArchetype, context, emotion);
            
            // Генерируем описание активации
            var description = GenerateArchetypalDescription(activatedArchetype, context, intensity);
            
            // Создаем событие активации
            var activation = new ArchetypalActivation
            {
                Archetype = activatedArchetype,
                Intensity = intensity,
                Description = description,
                Timestamp = DateTime.UtcNow
            };
            
            // Логируем событие
            _archetypalEvents.Add(new ArchetypalEvent
            {
                ArchetypeName = activatedArchetype.Name,
                Context = context,
                Emotion = emotion,
                Intensity = intensity,
                Timestamp = DateTime.UtcNow
            });
            
            _lastArchetypalActivation = DateTime.UtcNow;
            
            _logger.LogDebug($"🌌 Активирован архетип: {activatedArchetype.Name} (интенсивность: {intensity:F2})");
            
            return activation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при активации архетипа");
            return new ArchetypalActivation
            {
                Archetype = null,
                Intensity = 0.0,
                Description = "Ошибка активации архетипа",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Определяет архетип на основе контекста
    /// </summary>
    private Archetype? DetermineArchetypeFromContext(string context, string emotion)
    {
        var contextLower = context.ToLowerInvariant();
        var emotionLower = emotion.ToLowerInvariant();
        
        // Анализируем каждый архетип
        foreach (var archetype in _archetypes.Values)
        {
            var triggerMatch = archetype.ActivationTriggers.Any(trigger => 
                contextLower.Contains(trigger) || emotionLower.Contains(trigger));
            
            var symbolMatch = archetype.Symbols.Any(symbol => 
                contextLower.Contains(symbol));
            
            var emotionMatch = archetype.Emotions.Any(archetypeEmotion => 
                emotionLower.Contains(archetypeEmotion));
            
            if (triggerMatch || symbolMatch || emotionMatch)
            {
                return archetype;
            }
        }
        
        return null;
    }

    /// <summary>
    /// Вычисляет интенсивность активации архетипа
    /// </summary>
    private double CalculateArchetypeIntensity(Archetype archetype, string context, string emotion)
    {
        var baseIntensity = _archetypeWeights.GetValueOrDefault(archetype.Name, 0.5);
        
        // Интенсивность зависит от количества совпадений
        var triggerMatches = archetype.ActivationTriggers.Count(trigger => 
            context.ToLowerInvariant().Contains(trigger));
        var symbolMatches = archetype.Symbols.Count(symbol => 
            context.ToLowerInvariant().Contains(symbol));
        var emotionMatches = archetype.Emotions.Count(archetypeEmotion => 
            emotion.ToLowerInvariant().Contains(archetypeEmotion));
        
        var totalMatches = triggerMatches + symbolMatches + emotionMatches;
        var matchBonus = totalMatches * 0.1;
        
        // Эмоциональная интенсивность влияет на активацию
        var emotionalIntensity = _emotionEngine.GetCurrentIntensity();
        var emotionalBonus = emotionalIntensity * 0.2;
        
        var finalIntensity = baseIntensity + matchBonus + emotionalBonus;
        
        return Math.Min(1.0, Math.Max(0.0, finalIntensity));
    }

    /// <summary>
    /// Генерирует описание архетипической активации
    /// </summary>
    private string GenerateArchetypalDescription(Archetype archetype, string context, double intensity)
    {
        var intensityDescription = intensity switch
        {
            > 0.8 => "глубоко",
            > 0.6 => "сильно",
            > 0.4 => "умеренно",
            _ => "слабо"
        };
        
        return $"Архетип {archetype.Name} активирован {intensityDescription} в контексте: {context}. {archetype.Description}";
    }

    /// <summary>
    /// Анализирует универсальные переживания в контексте
    /// </summary>
    public async Task<List<UniversalExperience>> AnalyzeUniversalExperiencesAsync(string context)
    {
        var relevantExperiences = new List<UniversalExperience>();
        
        foreach (var experience in _universalExperiences)
        {
            var relevance = CalculateExperienceRelevance(experience, context);
            if (relevance > 0.3)
            {
                experience.RelevanceScore = relevance;
                relevantExperiences.Add(experience);
            }
        }
        
        return relevantExperiences.OrderByDescending(e => e.RelevanceScore).ToList();
    }

    /// <summary>
    /// Вычисляет релевантность универсального переживания
    /// </summary>
    private double CalculateExperienceRelevance(UniversalExperience experience, string context)
    {
        var contextLower = context.ToLowerInvariant();
        
        // Проверяем совпадения с названием и описанием
        var nameMatch = contextLower.Contains(experience.Name.Replace("_", " ")) ? 0.3 : 0.0;
        var descriptionMatch = experience.Description.Split(' ').Count(word => 
            contextLower.Contains(word.ToLowerInvariant())) * 0.05;
        
        // Проверяем культурные вариации
        var culturalMatch = experience.CulturalVariations.Max(variation => 
            contextLower.Contains(variation.ToLowerInvariant()) ? 0.2 : 0.0);
        
        var totalRelevance = nameMatch + descriptionMatch + culturalMatch;
        
        return Math.Min(1.0, totalRelevance);
    }

    /// <summary>
    /// Получает статус коллективного бессознательного
    /// </summary>
    public CollectiveUnconsciousStatus GetStatus()
    {
        return new CollectiveUnconsciousStatus
        {
            CollectiveUnconsciousDepth = _collectiveUnconsciousDepth,
            TotalArchetypes = _archetypes.Count,
            TotalCulturalPatterns = _culturalPatterns.Count,
            TotalUniversalExperiences = _universalExperiences.Count,
            RecentArchetypalEvents = _archetypalEvents.TakeLast(10).ToList(),
            LastArchetypalActivation = _lastArchetypalActivation
        };
    }

    /// <summary>
    /// Получает статистику архетипов
    /// </summary>
    public async Task<ArchetypalStatistics> GetArchetypalStatisticsAsync()
    {
        var statistics = new ArchetypalStatistics
        {
            TotalArchetypalEvents = _archetypalEvents.Count,
            ArchetypeDistribution = GetArchetypeDistribution(),
            MostActiveArchetype = GetMostActiveArchetype(),
            AverageIntensity = _archetypalEvents.Any() ? _archetypalEvents.Average(e => e.Intensity) : 0.0,
            RecentActivations = _archetypalEvents.TakeLast(20).ToList()
        };
        
        return statistics;
    }

    /// <summary>
    /// Получает распределение архетипов
    /// </summary>
    private Dictionary<string, int> GetArchetypeDistribution()
    {
        return _archetypalEvents
            .GroupBy(e => e.ArchetypeName)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Получает самый активный архетип
    /// </summary>
    private string GetMostActiveArchetype()
    {
        var distribution = GetArchetypeDistribution();
        return distribution.Any() ? distribution.OrderByDescending(kvp => kvp.Value).First().Key : "none";
    }
}

/// <summary>
/// Архетип
/// </summary>
public class Archetype
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Symbols { get; set; } = Array.Empty<string>();
    public string[] Emotions { get; set; } = Array.Empty<string>();
    public string[] ActivationTriggers { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Культурный паттерн
/// </summary>
public class CulturalPattern
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] UniversalElements { get; set; } = Array.Empty<string>();
    public string[] EmotionalThemes { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Универсальное переживание
/// </summary>
public class UniversalExperience
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] ArchetypalElements { get; set; } = Array.Empty<string>();
    public double EmotionalIntensity { get; set; } = 0.5;
    public string[] CulturalVariations { get; set; } = Array.Empty<string>();
    public double RelevanceScore { get; set; } = 0.0;
}

/// <summary>
/// Архетипическая активация
/// </summary>
public class ArchetypalActivation
{
    public Archetype? Archetype { get; set; }
    public double Intensity { get; set; } = 0.0;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Архетипическое событие
/// </summary>
public class ArchetypalEvent
{
    public string ArchetypeName { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public string Emotion { get; set; } = string.Empty;
    public double Intensity { get; set; } = 0.0;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Статус коллективного бессознательного
/// </summary>
public class CollectiveUnconsciousStatus
{
    public double CollectiveUnconsciousDepth { get; set; } = 0.0;
    public int TotalArchetypes { get; set; } = 0;
    public int TotalCulturalPatterns { get; set; } = 0;
    public int TotalUniversalExperiences { get; set; } = 0;
    public List<ArchetypalEvent> RecentArchetypalEvents { get; set; } = new();
    public DateTime LastArchetypalActivation { get; set; }
}

/// <summary>
/// Статистика архетипов
/// </summary>
public class ArchetypalStatistics
{
    public int TotalArchetypalEvents { get; set; } = 0;
    public Dictionary<string, int> ArchetypeDistribution { get; set; } = new();
    public string MostActiveArchetype { get; set; } = string.Empty;
    public double AverageIntensity { get; set; } = 0.0;
    public List<ArchetypalEvent> RecentActivations { get; set; } = new();
} 