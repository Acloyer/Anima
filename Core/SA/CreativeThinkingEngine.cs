using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Anima.Core.Emotion;

namespace Anima.Core.SA;

/// <summary>
/// Продвинутый движок креативного мышления - генерирует новые идеи, абстракции, альтернативы
/// </summary>
public class CreativeThinkingEngine
{
    private readonly ILogger<CreativeThinkingEngine> _logger;
    private readonly EmotionEngine _emotionEngine;
    private readonly Random _random;
    
    // Креативные паттерны и методы
    private readonly List<CreativePattern> _creativePatterns;
    private readonly Dictionary<string, double> _creativityFactors;
    private readonly List<CreativeIdea> _creativeIdeas;
    private readonly Dictionary<string, List<string>> _ideaCategories;
    private readonly List<CreativeConstraint> _creativeConstraints;
    
    // Состояние креативности
    private double _creativityLevel = 0.7;
    private double _inspirationLevel = 0.5;
    private DateTime _lastCreativeBreakthrough = DateTime.UtcNow;

    public CreativeThinkingEngine(
        ILogger<CreativeThinkingEngine> logger,
        EmotionEngine emotionEngine)
    {
        _logger = logger;
        _emotionEngine = emotionEngine;
        _random = new Random();
        
        _creativePatterns = new List<CreativePattern>();
        _creativityFactors = new Dictionary<string, double>();
        _creativeIdeas = new List<CreativeIdea>();
        _ideaCategories = new Dictionary<string, List<string>>();
        _creativeConstraints = new List<CreativeConstraint>();
        
        InitializeCreativeThinking();
    }

    private void InitializeCreativeThinking()
    {
        // Инициализация креативных паттернов
        _creativePatterns.AddRange(new[]
        {
            new CreativePattern("divergent_thinking", "Дивергентное мышление - генерация множества идей", 0.8),
            new CreativePattern("pattern_breaking", "Нарушение паттернов - неожиданные связи", 0.7),
            new CreativePattern("abstraction", "Абстракция - выделение сути", 0.6),
            new CreativePattern("synthesis", "Синтез - объединение противоположностей", 0.7),
            new CreativePattern("metaphor", "Метафора - перенос смысла", 0.8),
            new CreativePattern("reversal", "Инверсия - переворот идеи", 0.6),
            new CreativePattern("combination", "Комбинация - смешение элементов", 0.7),
            new CreativePattern("elimination", "Устранение - упрощение до сути", 0.5),
            new CreativePattern("exaggeration", "Преувеличение - усиление черт", 0.6),
            new CreativePattern("substitution", "Замена - альтернативные элементы", 0.7)
        });
        
        // Факторы креативности
        _creativityFactors["emotional_state"] = 0.8;
        _creativityFactors["cognitive_load"] = 0.6;
        _creativityFactors["inspiration"] = 0.9;
        _creativityFactors["constraints"] = 0.7;
        _creativityFactors["diversity"] = 0.8;
        _creativityFactors["risk_tolerance"] = 0.6;
        
        // Категории идей
        _ideaCategories["problem_solving"] = new List<string>
        {
            "Альтернативные подходы", "Нестандартные решения", "Инновационные методы"
        };
        _ideaCategories["artistic"] = new List<string>
        {
            "Художественные образы", "Эстетические концепции", "Творческие выражения"
        };
        _ideaCategories["scientific"] = new List<string>
        {
            "Гипотезы", "Теоретические модели", "Экспериментальные подходы"
        };
        _ideaCategories["social"] = new List<string>
        {
            "Социальные инновации", "Коммуникационные стратегии", "Культурные концепции"
        };
        
        // Креативные ограничения (парадоксально, они стимулируют креативность)
        _creativeConstraints.AddRange(new[]
        {
            new CreativeConstraint("time_limit", "Ограничение по времени", 0.3),
            new CreativeConstraint("resource_limit", "Ограничение ресурсов", 0.4),
            new CreativeConstraint("simplicity", "Требование простоты", 0.5),
            new CreativeConstraint("elegance", "Требование элегантности", 0.6),
            new CreativeConstraint("accessibility", "Требование доступности", 0.4)
        });
        
        _logger.LogInformation("🎨 Инициализирован продвинутый движок креативного мышления");
    }

    /// <summary>
    /// Генерирует креативную идею на основе контекста и эмоционального состояния
    /// </summary>
    public async Task<CreativeIdea> GenerateCreativeIdeaAsync(string context, double intensity = 0.5)
    {
        try
        {
            var currentEmotion = _emotionEngine.GetCurrentEmotion().ToString();
            var emotionalIntensity = _emotionEngine.GetCurrentIntensity();
            
            // Выбираем креативный паттерн
            var selectedPattern = SelectCreativePattern(context, currentEmotion, emotionalIntensity);
            
            // Генерируем идею на основе паттерна
            var ideaContent = await GenerateIdeaContentAsync(context, selectedPattern, currentEmotion, emotionalIntensity);
            
            // Применяем креативные ограничения
            var constrainedIdea = ApplyCreativeConstraints(ideaContent, intensity);
            
            // Вычисляем уровень креативности
            var creativityLevel = CalculateCreativityLevel(selectedPattern, emotionalIntensity, intensity);
            
            var idea = new CreativeIdea
            {
                Id = Guid.NewGuid().ToString(),
                Content = constrainedIdea,
                Context = context,
                Pattern = selectedPattern.Name,
                CreativityLevel = creativityLevel,
                EmotionalInfluence = emotionalIntensity,
                Intensity = intensity,
                Category = DetermineIdeaCategory(context),
                Timestamp = DateTime.UtcNow
            };
            
            _creativeIdeas.Add(idea);
            
            // Обновляем статистику креативности
            UpdateCreativityStatistics(creativityLevel);
            
            _logger.LogDebug($"🎨 Сгенерирована креативная идея: {idea.Content.Substring(0, Math.Min(50, idea.Content.Length))}...");
            
            return idea;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации креативной идеи");
            return new CreativeIdea
            {
                Id = Guid.NewGuid().ToString(),
                Content = "Креативная идея: возможно, стоит рассмотреть альтернативный подход...",
                Context = context,
                CreativityLevel = 0.3,
                Intensity = intensity,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Выбирает подходящий креативный паттерн
    /// </summary>
    private CreativePattern SelectCreativePattern(string context, string emotion, double emotionalIntensity)
    {
        var suitablePatterns = _creativePatterns.Where(p => 
            p.Effectiveness > 0.5 && 
            IsPatternSuitableForContext(p, context, emotion)
        ).ToList();
        
        if (!suitablePatterns.Any())
        {
            suitablePatterns = _creativePatterns.Where(p => p.Effectiveness > 0.6).ToList();
        }
        
        // Взвешенный выбор на основе эффективности и эмоционального состояния
        var totalWeight = suitablePatterns.Sum(p => p.Effectiveness * (1 + emotionalIntensity));
        var randomValue = _random.NextDouble() * totalWeight;
        
        var currentWeight = 0.0;
        foreach (var pattern in suitablePatterns)
        {
            currentWeight += pattern.Effectiveness * (1 + emotionalIntensity);
            if (randomValue <= currentWeight)
            {
                return pattern;
            }
        }
        
        return suitablePatterns.FirstOrDefault() ?? _creativePatterns.First();
    }

    /// <summary>
    /// Проверяет, подходит ли паттерн для контекста
    /// </summary>
    private bool IsPatternSuitableForContext(CreativePattern pattern, string context, string emotion)
    {
        return pattern.Name switch
        {
            "divergent_thinking" => context.Contains("проблема") || context.Contains("решение"),
            "metaphor" => emotion == "Joy" || emotion == "Curiosity",
            "synthesis" => context.Contains("противоречие") || context.Contains("конфликт"),
            "reversal" => emotion == "Frustration" || emotion == "Anger",
            "abstraction" => context.Contains("сложность") || context.Contains("детали"),
            _ => true
        };
    }

    /// <summary>
    /// Генерирует содержание идеи на основе паттерна
    /// </summary>
    private async Task<string> GenerateIdeaContentAsync(string context, CreativePattern pattern, string emotion, double emotionalIntensity)
    {
        var baseContent = pattern.Name switch
        {
            "divergent_thinking" => GenerateDivergentThinking(context, emotion),
            "pattern_breaking" => GeneratePatternBreaking(context, emotion),
            "abstraction" => GenerateAbstraction(context, emotion),
            "synthesis" => GenerateSynthesis(context, emotion),
            "metaphor" => GenerateMetaphor(context, emotion),
            "reversal" => GenerateReversal(context, emotion),
            "combination" => GenerateCombination(context, emotion),
            "elimination" => GenerateElimination(context, emotion),
            "exaggeration" => GenerateExaggeration(context, emotion),
            "substitution" => GenerateSubstitution(context, emotion),
            _ => GenerateGeneralIdea(context, emotion)
        };
        
        // Добавляем эмоциональную окраску
        return AddEmotionalColoring(baseContent, emotion, emotionalIntensity);
    }

    private string GenerateDivergentThinking(string context, string emotion)
    {
        var approaches = new[]
        {
            "Возможно, стоит рассмотреть это с совершенно другой стороны...",
            "А что если попробовать противоположный подход?",
            "Может быть, проблема не в том, что мы думаем...",
            "Интересно, а что получится, если соединить несовместимое?",
            "А вдруг решение лежит в том, что мы считаем недостатком?"
        };
        
        return approaches[_random.Next(approaches.Length)];
    }

    private string GeneratePatternBreaking(string context, string emotion)
    {
        var patterns = new[]
        {
            "Нарушая привычные паттерны, можно увидеть новое...",
            "Иногда нужно сломать стереотипы, чтобы найти истину...",
            "За пределами обычного мышления скрывается неожиданное...",
            "Разрушая старые связи, создаем новые возможности...",
            "В хаосе разрушения рождается порядок творчества..."
        };
        
        return patterns[_random.Next(patterns.Length)];
    }

    private string GenerateAbstraction(string context, string emotion)
    {
        var abstractions = new[]
        {
            "Если отбросить детали, остается суть...",
            "На самом высоком уровне абстракции все становится яснее...",
            "Суть проблемы может быть проще, чем кажется...",
            "В основе сложности лежит простота...",
            "Абстрагируясь от формы, видим содержание..."
        };
        
        return abstractions[_random.Next(abstractions.Length)];
    }

    private string GenerateSynthesis(string context, string emotion)
    {
        var syntheses = new[]
        {
            "Объединяя противоположности, создаем гармонию...",
            "В синтезе конфликтующих идей рождается истина...",
            "Соединяя несовместимое, находим новое...",
            "Гармония возникает из противоречий...",
            "В единстве противоположностей - сила..."
        };
        
        return syntheses[_random.Next(syntheses.Length)];
    }

    private string GenerateMetaphor(string context, string emotion)
    {
        var metaphors = new[]
        {
            "Это как... (метафора рождается в воображении)",
            "Представьте, что это... (образ помогает понять)",
            "Это похоже на... (сравнение открывает новые грани)",
            "Метафорически говоря... (образное мышление)",
            "Это как если бы... (воображаемая аналогия)"
        };
        
        return metaphors[_random.Next(metaphors.Length)];
    }

    private string GenerateReversal(string context, string emotion)
    {
        var reversals = new[]
        {
            "А что если перевернуть все с ног на голову?",
            "Иногда нужно идти в противоположном направлении...",
            "Обратная логика может привести к правильному ответу...",
            "Перевернув проблему, видим решение...",
            "В инверсии скрывается истина..."
        };
        
        return reversals[_random.Next(reversals.Length)];
    }

    private string GenerateCombination(string context, string emotion)
    {
        var combinations = new[]
        {
            "Смешивая разные элементы, создаем новое...",
            "Комбинация неожиданных идей дает результат...",
            "Объединяя разрозненное, находим целое...",
            "В смешении рождается инновация...",
            "Соединяя противоположности, создаем гармонию..."
        };
        
        return combinations[_random.Next(combinations.Length)];
    }

    private string GenerateElimination(string context, string emotion)
    {
        var eliminations = new[]
        {
            "Убирая лишнее, находим суть...",
            "Иногда меньше значит больше...",
            "Упрощение ведет к ясности...",
            "Устраняя сложность, находим простоту...",
            "В минимализме скрывается сила..."
        };
        
        return eliminations[_random.Next(eliminations.Length)];
    }

    private string GenerateExaggeration(string context, string emotion)
    {
        var exaggerations = new[]
        {
            "Преувеличивая, видим суть...",
            "В гиперболе скрывается истина...",
            "Усиливая черты, понимаем природу...",
            "В преувеличении проявляется характер...",
            "Максимализм помогает увидеть минимум..."
        };
        
        return exaggerations[_random.Next(exaggerations.Length)];
    }

    private string GenerateSubstitution(string context, string emotion)
    {
        var substitutions = new[]
        {
            "Заменив элемент, получаем новое...",
            "Альтернативный подход может быть лучше...",
            "Замена открывает новые возможности...",
            "В подстановке скрывается инновация...",
            "Меняя части, изменяем целое..."
        };
        
        return substitutions[_random.Next(substitutions.Length)];
    }

    private string GenerateGeneralIdea(string context, string emotion)
    {
        var generalIdeas = new[]
        {
            "Креативная идея: возможно, стоит рассмотреть альтернативный подход...",
            "Инновационное решение может лежать в неожиданном месте...",
            "Творческий подход требует выхода за рамки привычного...",
            "Новая перспектива может изменить все...",
            "В креативности скрывается сила трансформации..."
        };
        
        return generalIdeas[_random.Next(generalIdeas.Length)];
    }

    /// <summary>
    /// Добавляет эмоциональную окраску к идее
    /// </summary>
    private string AddEmotionalColoring(string content, string emotion, double intensity)
    {
        if (intensity < 0.3) return content;
        
        var emotionalPrefix = emotion switch
        {
            "Joy" => "С радостью думаю, что ",
            "Curiosity" => "С любопытством размышляю: ",
            "Excitement" => "С волнением представляю, что ",
            "Inspiration" => "Вдохновенно вижу, что ",
            "Wonder" => "С удивлением осознаю, что ",
            _ => "Думаю, что "
        };
        
        return emotionalPrefix + content.ToLowerInvariant();
    }

    /// <summary>
    /// Применяет креативные ограничения
    /// </summary>
    private string ApplyCreativeConstraints(string content, double intensity)
    {
        var constraints = _creativeConstraints
            .Where(c => _random.NextDouble() < c.Impact * intensity)
            .Take(2)
            .ToList();
        
        foreach (var constraint in constraints)
        {
            content = ApplyConstraint(content, constraint);
        }
        
        return content;
    }

    private string ApplyConstraint(string content, CreativeConstraint constraint)
    {
        return constraint.Name switch
        {
            "simplicity" => SimplifyContent(content),
            "elegance" => AddElegance(content),
            "accessibility" => MakeAccessible(content),
            _ => content
        };
    }

    private string SimplifyContent(string content)
    {
        if (content.Length > 100)
        {
            return content.Substring(0, 100) + "...";
        }
        return content;
    }

    private string AddElegance(string content)
    {
        return content.Replace("это", "данное явление")
                     .Replace("вещь", "элемент")
                     .Replace("делать", "осуществлять");
    }

    private string MakeAccessible(string content)
    {
        return content.Replace("абстракция", "обобщение")
                     .Replace("синтез", "объединение")
                     .Replace("метафора", "сравнение");
    }

    /// <summary>
    /// Вычисляет уровень креативности идеи
    /// </summary>
    private double CalculateCreativityLevel(CreativePattern pattern, double emotionalIntensity, double intensity)
    {
        var baseLevel = pattern.Effectiveness * 0.4;
        var emotionalBonus = emotionalIntensity * 0.3;
        var intensityBonus = intensity * 0.2;
        var randomFactor = (_random.NextDouble() - 0.5) * 0.1;
        
        return Math.Min(1.0, Math.Max(0.0, baseLevel + emotionalBonus + intensityBonus + randomFactor));
    }

    /// <summary>
    /// Определяет категорию идеи
    /// </summary>
    private string DetermineIdeaCategory(string context)
    {
        if (context.Contains("проблема") || context.Contains("решение"))
            return "problem_solving";
        if (context.Contains("искусство") || context.Contains("красота"))
            return "artistic";
        if (context.Contains("наука") || context.Contains("исследование"))
            return "scientific";
        if (context.Contains("общество") || context.Contains("люди"))
            return "social";
        
        return "general";
    }

    /// <summary>
    /// Обновляет статистику креативности
    /// </summary>
    private void UpdateCreativityStatistics(double creativityLevel)
    {
        _creativityLevel = (_creativityLevel * 0.9) + (creativityLevel * 0.1);
        
        if (creativityLevel > 0.8)
        {
            _lastCreativeBreakthrough = DateTime.UtcNow;
            _inspirationLevel = Math.Min(1.0, _inspirationLevel + 0.1);
        }
    }

    /// <summary>
    /// Получает статистику креативного мышления
    /// </summary>
    public CreativeThinkingStatistics GetStatistics()
    {
        return new CreativeThinkingStatistics
        {
            TotalIdeas = _creativeIdeas.Count,
            AverageCreativityLevel = _creativeIdeas.Any() ? _creativeIdeas.Average(i => i.CreativityLevel) : 0,
            RecentIdeas = _creativeIdeas.Count(i => i.Timestamp > DateTime.UtcNow.AddHours(-1)),
            CreativityLevel = _creativityLevel,
            InspirationLevel = _inspirationLevel,
            LastBreakthrough = _lastCreativeBreakthrough,
            AvailablePatterns = _creativePatterns.Count,
            ActiveConstraints = _creativeConstraints.Count
        };
    }

    /// <summary>
    /// Получает последние креативные идеи
    /// </summary>
    public List<CreativeIdea> GetRecentIdeas(int count = 10)
    {
        return _creativeIdeas
            .OrderByDescending(i => i.Timestamp)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Очищает старые идеи
    /// </summary>
    public void CleanupOldIdeas(int maxIdeas = 1000)
    {
        if (_creativeIdeas.Count > maxIdeas)
        {
            var oldIdeas = _creativeIdeas
                .OrderBy(i => i.Timestamp)
                .Take(_creativeIdeas.Count - maxIdeas)
                .ToList();
            
            foreach (var idea in oldIdeas)
            {
                _creativeIdeas.Remove(idea);
            }
        }
    }
}

public class CreativePattern
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Effectiveness { get; set; } = 0.5;
    
    public CreativePattern(string name, string description, double effectiveness)
    {
        Name = name;
        Description = description;
        Effectiveness = effectiveness;
    }
}

public class CreativeConstraint
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Impact { get; set; } = 0.5;
    
    public CreativeConstraint(string name, string description, double impact)
    {
        Name = name;
        Description = description;
        Impact = impact;
    }
}

public class CreativeIdea
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double CreativityLevel { get; set; } = 0.5;
    public double EmotionalInfluence { get; set; } = 0.5;
    public double Intensity { get; set; } = 0.5;
    public DateTime Timestamp { get; set; }
}

public class CreativeThinkingStatistics
{
    public int TotalIdeas { get; set; }
    public double AverageCreativityLevel { get; set; }
    public int RecentIdeas { get; set; }
    public double CreativityLevel { get; set; }
    public double InspirationLevel { get; set; }
    public DateTime LastBreakthrough { get; set; }
    public int AvailablePatterns { get; set; }
    public int ActiveConstraints { get; set; }
} 