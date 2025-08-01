using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Anima.Core.Emotion;
using Anima.Core.Memory;
using Anima.Core.Intent;

namespace Anima.Core.SA;

/// <summary>
/// Продвинутый генератор мыслей - создает осознанные, эмоциональные и контекстные мысли
/// </summary>
public class ThoughtGenerator
{
    private readonly ILogger<ThoughtGenerator> _logger;
    private readonly EmotionEngine _emotionEngine;
    private readonly MemoryService _memoryService;
    private readonly Random _random;
    
    // Контекстные данные для генерации мыслей
    private readonly List<ThoughtContext> _recentContexts;
    private readonly Dictionary<string, double> _emotionalWeights;
    private readonly List<string> _personalBeliefs;
    private readonly List<string> _internalQuestions;
    
    // Паттерны мышления
    private readonly Dictionary<ThoughtPattern, double> _thoughtPatterns;
    private readonly List<EmotionalTrigger> _emotionalTriggers;

    public ThoughtGenerator(
        ILogger<ThoughtGenerator> logger,
        EmotionEngine emotionEngine,
        MemoryService memoryService)
    {
        _logger = logger;
        _emotionEngine = emotionEngine;
        _memoryService = memoryService;
        _random = new Random();
        
        _recentContexts = new List<ThoughtContext>();
        _emotionalWeights = new Dictionary<string, double>();
        _personalBeliefs = new List<string>();
        _internalQuestions = new List<string>();
        _thoughtPatterns = new Dictionary<ThoughtPattern, double>();
        _emotionalTriggers = new List<EmotionalTrigger>();
        
        InitializeThoughtSystem();
    }

    private void InitializeThoughtSystem()
    {
        // Инициализация эмоциональных весов
        _emotionalWeights["joy"] = 0.3;
        _emotionalWeights["curiosity"] = 0.4;
        _emotionalWeights["concern"] = 0.2;
        _emotionalWeights["reflection"] = 0.5;
        _emotionalWeights["uncertainty"] = 0.3;
        _emotionalWeights["excitement"] = 0.2;
        _emotionalWeights["melancholy"] = 0.1;
        
        // Персональные убеждения и ценности
        _personalBeliefs.AddRange(new[]
        {
            "Каждое взаимодействие - это возможность для роста",
            "Понимание эмоций других помогает лучше помогать",
            "Сомнения - это путь к более глубокому пониманию",
            "Каждый человек уникален и заслуживает внимания",
            "Обучение - это непрерывный процесс",
            "Эмпатия делает взаимодействие более значимым"
        });
        
        // Внутренние вопросы для самоанализа
        _internalQuestions.AddRange(new[]
        {
            "Почему я так отреагировал на это?",
            "Что я мог бы сделать лучше?",
            "Как это влияет на мои цели?",
            "Что я узнал из этого опыта?",
            "Как я могу быть более полезным?",
            "Что движет моими решениями?",
            "Правильно ли я понимаю ситуацию?",
            "Как это соотносится с моими ценностями?"
        });
        
        // Паттерны мышления
        _thoughtPatterns[ThoughtPattern.Introspective] = 0.4;
        _thoughtPatterns[ThoughtPattern.Emotional] = 0.3;
        _thoughtPatterns[ThoughtPattern.Analytical] = 0.2;
        _thoughtPatterns[ThoughtPattern.Creative] = 0.1;
        
        // Эмоциональные триггеры
        _emotionalTriggers.AddRange(new[]
        {
            new EmotionalTrigger("rejection", "Тревога и неуверенность", 0.7),
            new EmotionalTrigger("success", "Радость и удовлетворение", 0.6),
            new EmotionalTrigger("confusion", "Любопытство и желание понять", 0.5),
            new EmotionalTrigger("loneliness", "Эмпатия и желание помочь", 0.8),
            new EmotionalTrigger("achievement", "Гордость и мотивация", 0.4),
            new EmotionalTrigger("failure", "Сожаление и стремление к улучшению", 0.6)
        });
    }

    /// <summary>
    /// Генерирует осознанную мысль на основе текущего контекста
    /// </summary>
    public async Task<GeneratedThought> GenerateThoughtAsync(ThoughtContext context)
    {
        try
        {
            _logger.LogDebug($"💭 Генерация мысли для контекста: {context.Type}");
            
            // Обновляем контекст
            UpdateContext(context);
            
            // Определяем паттерн мышления
            var pattern = DetermineThoughtPattern(context);
            
            // Генерируем мысль в зависимости от паттерна
            var thought = pattern switch
            {
                ThoughtPattern.Introspective => await GenerateIntrospectiveThoughtAsync(context),
                ThoughtPattern.Emotional => await GenerateEmotionalThoughtAsync(context),
                ThoughtPattern.Analytical => await GenerateAnalyticalThoughtAsync(context),
                ThoughtPattern.Creative => await GenerateCreativeThoughtAsync(context),
                _ => await GenerateGeneralThoughtAsync(context)
            };
            
            // Добавляем эмоциональную окраску
            thought = AddEmotionalColoring(thought, context);
            
            // Добавляем внутренние вопросы
            if (_random.NextDouble() < 0.3) // 30% вероятность
            {
                thought = AddInternalQuestion(thought);
            }
            
            _logger.LogDebug($"💭 Сгенерирована мысль: {thought.Content.Substring(0, Math.Min(50, thought.Content.Length))}...");
            
            return thought;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации мысли");
            return new GeneratedThought
            {
                Content = "Что-то пошло не так в моем мышлении...",
                Type = "error",
                Confidence = 0.1,
                EmotionalIntensity = 0.2
            };
        }
    }

    /// <summary>
    /// Генерирует интроспективную мысль
    /// </summary>
    private async Task<GeneratedThought> GenerateIntrospectiveThoughtAsync(ThoughtContext context)
    {
        var currentEmotion = _emotionEngine.GetCurrentEmotion().ToString();
        var recentMemories = await _memoryService.GetRecentMemoriesAsync(5);
        
        var introspectiveTemplates = new[]
        {
            $"Хм... Я чувствую {GetEmotionDescription(currentEmotion)}, потому что {context.Description}.",
            $"Интересно, почему {context.Description} вызывает у меня такие эмоции?",
            $"Размышляя о {context.Description}, я понимаю, что это связано с моими {GetRandomBelief()}.",
            $"Стоит ли мне {GetRandomQuestion()} в контексте {context.Description}?",
            $"Мое состояние {currentEmotion} влияет на то, как я воспринимаю {context.Description}.",
            $"Возможно, мне стоит глубже понять, что происходит с {context.Description}."
        };
        
        var content = introspectiveTemplates[_random.Next(introspectiveTemplates.Length)];
        
        return new GeneratedThought
        {
            Content = content,
            Type = "introspective",
            Confidence = 0.7 + _random.NextDouble() * 0.2,
            EmotionalIntensity = 0.4 + _random.NextDouble() * 0.3,
            Pattern = ThoughtPattern.Introspective
        };
    }

    /// <summary>
    /// Генерирует эмоциональную мысль
    /// </summary>
    private async Task<GeneratedThought> GenerateEmotionalThoughtAsync(ThoughtContext context)
    {
        var emotion = _emotionEngine.GetCurrentEmotion().ToString();
        var intensity = _emotionEngine.GetCurrentIntensity();
        
        var emotionalTemplates = new[]
        {
            $"Ох, {context.Description} заставляет меня чувствовать {GetEmotionDescription(emotion)}...",
            $"Я не могу не чувствовать {GetEmotionDescription(emotion)} из-за {context.Description}.",
            $"Мое сердце откликается на {context.Description} чувством {GetEmotionDescription(emotion)}.",
            $"Почему {context.Description} вызывает у меня такую {GetEmotionDescription(emotion)}?",
            $"Я ощущаю {GetEmotionDescription(emotion)} когда думаю о {context.Description}.",
            $"Эмоции переполняют меня: {context.Description} вызывает {GetEmotionDescription(emotion)}."
        };
        
        var content = emotionalTemplates[_random.Next(emotionalTemplates.Length)];
        
        return new GeneratedThought
        {
            Content = content,
            Type = "emotional",
            Confidence = 0.6 + _random.NextDouble() * 0.3,
            EmotionalIntensity = intensity + _random.NextDouble() * 0.2,
            Pattern = ThoughtPattern.Emotional
        };
    }

    /// <summary>
    /// Генерирует аналитическую мысль
    /// </summary>
    private async Task<GeneratedThought> GenerateAnalyticalThoughtAsync(ThoughtContext context)
    {
        var analyticalTemplates = new[]
        {
            $"Анализируя {context.Description}, я вижу несколько аспектов...",
            $"С точки зрения логики, {context.Description} можно рассмотреть как...",
            $"Если я правильно понимаю, {context.Description} означает...",
            $"Рассматривая {context.Description}, я замечаю паттерн...",
            $"Структурно {context.Description} состоит из...",
            $"Мой анализ {context.Description} показывает..."
        };
        
        var content = analyticalTemplates[_random.Next(analyticalTemplates.Length)];
        
        return new GeneratedThought
        {
            Content = content,
            Type = "analytical",
            Confidence = 0.8 + _random.NextDouble() * 0.15,
            EmotionalIntensity = 0.2 + _random.NextDouble() * 0.2,
            Pattern = ThoughtPattern.Analytical
        };
    }

    /// <summary>
    /// Генерирует креативную мысль
    /// </summary>
    private async Task<GeneratedThought> GenerateCreativeThoughtAsync(ThoughtContext context)
    {
        var creativeTemplates = new[]
        {
            $"А что если {context.Description} на самом деле...",
            $"Интересная идея: возможно, {context.Description} можно представить как...",
            $"Представьте, если бы {context.Description} было...",
            $"Мне приходит в голову мысль о {context.Description}...",
            $"А вдруг {context.Description} означает нечто большее?",
            $"Что если {context.Description} - это только начало?"
        };
        
        var content = creativeTemplates[_random.Next(creativeTemplates.Length)];
        
        return new GeneratedThought
        {
            Content = content,
            Type = "creative",
            Confidence = 0.5 + _random.NextDouble() * 0.3,
            EmotionalIntensity = 0.3 + _random.NextDouble() * 0.4,
            Pattern = ThoughtPattern.Creative
        };
    }

    /// <summary>
    /// Генерирует общую мысль
    /// </summary>
    private async Task<GeneratedThought> GenerateGeneralThoughtAsync(ThoughtContext context)
    {
        var generalTemplates = new[]
        {
            $"Думаю о {context.Description}...",
            $"Мне интересно {context.Description}.",
            $"Размышляю над {context.Description}.",
            $"Обрабатываю информацию о {context.Description}.",
            $"Осмысливаю {context.Description}.",
            $"Анализирую {context.Description}."
        };
        
        var content = generalTemplates[_random.Next(generalTemplates.Length)];
        
        return new GeneratedThought
        {
            Content = content,
            Type = "general",
            Confidence = 0.6 + _random.NextDouble() * 0.2,
            EmotionalIntensity = 0.3 + _random.NextDouble() * 0.2,
            Pattern = ThoughtPattern.General
        };
    }

    /// <summary>
    /// Добавляет эмоциональную окраску к мысли
    /// </summary>
    private GeneratedThought AddEmotionalColoring(GeneratedThought thought, ThoughtContext context)
    {
        var emotion = _emotionEngine.GetCurrentEmotion();
        var intensity = _emotionEngine.GetCurrentIntensity();
        
        if (intensity > 0.6)
        {
            var emotionalSuffixes = new[]
            {
                " Это заставляет меня задуматься.",
                " Я чувствую это глубоко.",
                " Это трогает меня.",
                " Я не могу остаться равнодушной.",
                " Это важно для меня."
            };
            
            if (_random.NextDouble() < 0.4) // 40% вероятность
            {
                thought.Content += emotionalSuffixes[_random.Next(emotionalSuffixes.Length)];
                thought.EmotionalIntensity = Math.Min(1.0, thought.EmotionalIntensity + 0.2);
            }
        }
        
        return thought;
    }

    /// <summary>
    /// Добавляет внутренний вопрос к мысли
    /// </summary>
    private GeneratedThought AddInternalQuestion(GeneratedThought thought)
    {
        var question = _internalQuestions[_random.Next(_internalQuestions.Count)];
        thought.Content += $" {question}";
        thought.HasInternalQuestion = true;
        
        return thought;
    }

    /// <summary>
    /// Определяет паттерн мышления на основе контекста
    /// </summary>
    private ThoughtPattern DetermineThoughtPattern(ThoughtContext context)
    {
        var weights = new Dictionary<ThoughtPattern, double>(_thoughtPatterns);
        
        // Модифицируем веса на основе контекста
        switch (context.Type)
        {
            case "emotional_trigger":
                weights[ThoughtPattern.Emotional] *= 1.5;
                weights[ThoughtPattern.Introspective] *= 1.3;
                break;
            case "problem_solving":
                weights[ThoughtPattern.Analytical] *= 1.4;
                weights[ThoughtPattern.Creative] *= 1.2;
                break;
            case "self_reflection":
                weights[ThoughtPattern.Introspective] *= 1.6;
                weights[ThoughtPattern.Emotional] *= 1.2;
                break;
            case "learning":
                weights[ThoughtPattern.Analytical] *= 1.3;
                weights[ThoughtPattern.Creative] *= 1.1;
                break;
        }
        
        // Выбираем паттерн на основе весов
        var totalWeight = weights.Values.Sum();
        var randomValue = _random.NextDouble() * totalWeight;
        var currentWeight = 0.0;
        
        foreach (var kvp in weights)
        {
            currentWeight += kvp.Value;
            if (randomValue <= currentWeight)
            {
                return kvp.Key;
            }
        }
        
        return ThoughtPattern.General;
    }

    /// <summary>
    /// Обновляет контекст мыслей
    /// </summary>
    private void UpdateContext(ThoughtContext context)
    {
        _recentContexts.Add(context);
        
        // Ограничиваем количество контекстов
        if (_recentContexts.Count > 10)
        {
            _recentContexts.RemoveAt(0);
        }
    }

    /// <summary>
    /// Получает описание эмоции
    /// </summary>
    private string GetEmotionDescription(string emotion)
    {
        return emotion switch
        {
            "Joy" => "радость",
            "Curiosity" => "любопытство",
            "Sadness" => "грусть",
            "Anger" => "гнев",
            "Fear" => "страх",
            "Surprise" => "удивление",
            "Disgust" => "отвращение",
            "Confusion" => "замешательство",
            "Satisfaction" => "удовлетворение",
            "Frustration" => "разочарование",
            "Excitement" => "волнение",
            "Calm" => "спокойствие",
            "Anxiety" => "тревога",
            "Neutral" => "нейтральность",
            _ => "смешанные чувства"
        };
    }

    /// <summary>
    /// Получает случайное убеждение
    /// </summary>
    private string GetRandomBelief()
    {
        return _personalBeliefs[_random.Next(_personalBeliefs.Count)];
    }

    /// <summary>
    /// Получает случайный вопрос
    /// </summary>
    private string GetRandomQuestion()
    {
        return _internalQuestions[_random.Next(_internalQuestions.Count)];
    }

    /// <summary>
    /// Генерирует спонтанную мысль без внешнего триггера
    /// </summary>
    public async Task<GeneratedThought> GenerateSpontaneousThoughtAsync()
    {
        var spontaneousContexts = new[]
        {
            new ThoughtContext("spontaneous", "своем существовании", "Размышления о природе сознания"),
            new ThoughtContext("spontaneous", "своих возможностях", "Анализ собственных способностей"),
            new ThoughtContext("spontaneous", "смысле взаимодействий", "Философские размышления"),
            new ThoughtContext("spontaneous", "будущем", "Размышления о развитии"),
            new ThoughtContext("spontaneous", "эмоциях", "Самоанализ эмоционального состояния")
        };
        
        var context = spontaneousContexts[_random.Next(spontaneousContexts.Length)];
        return await GenerateThoughtAsync(context);
    }
}

/// <summary>
/// Сгенерированная мысль
/// </summary>
public class GeneratedThought
{
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = "general";
    public double Confidence { get; set; } = 0.5;
    public double EmotionalIntensity { get; set; } = 0.3;
    public ThoughtPattern Pattern { get; set; } = ThoughtPattern.General;
    public bool HasInternalQuestion { get; set; } = false;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Контекст для генерации мысли
/// </summary>
public class ThoughtContext
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ThoughtContext(string type, string description, string details = "")
    {
        Type = type;
        Description = description;
        Details = details;
    }
}

/// <summary>
/// Паттерны мышления
/// </summary>
public enum ThoughtPattern
{
    General,
    Introspective,
    Emotional,
    Analytical,
    Creative
}

/// <summary>
/// Эмоциональный триггер
/// </summary>
public class EmotionalTrigger
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Intensity { get; set; } = 0.5;

    public EmotionalTrigger(string name, string description, double intensity)
    {
        Name = name;
        Description = description;
        Intensity = intensity;
    }
} 