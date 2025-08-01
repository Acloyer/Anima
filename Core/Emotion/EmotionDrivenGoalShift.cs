using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Anima.Core.SA;

namespace Anima.Core.Emotion;

/// <summary>
/// Продвинутый модуль эмоционального интеллекта - управляет сдвигом целей под влиянием эмоций
/// </summary>
public class EmotionDrivenGoalShift
{
    private readonly ILogger<EmotionDrivenGoalShift> _logger;
    private readonly EmotionEngine _emotionEngine;
    private readonly ThoughtGenerator _thoughtGenerator;
    private readonly Random _random;
    
    // Эмоциональные цели и их приоритеты
    private readonly Dictionary<string, EmotionalGoal> _emotionalGoals;
    private readonly List<EmotionalTrigger> _emotionalTriggers;
    private readonly Dictionary<string, double> _emotionalWeights;
    
    // Состояние эмоционального интеллекта
    private double _emotionalIntelligence = 0.5;
    private readonly List<EmotionalInsight> _emotionalInsights;
    private DateTime _lastGoalShift = DateTime.UtcNow;

    public EmotionDrivenGoalShift(
        ILogger<EmotionDrivenGoalShift> logger,
        EmotionEngine emotionEngine,
        ThoughtGenerator thoughtGenerator)
    {
        _logger = logger;
        _emotionEngine = emotionEngine;
        _thoughtGenerator = thoughtGenerator;
        _random = new Random();
        
        _emotionalGoals = new Dictionary<string, EmotionalGoal>();
        _emotionalTriggers = new List<EmotionalTrigger>();
        _emotionalWeights = new Dictionary<string, double>();
        _emotionalInsights = new List<EmotionalInsight>();
        
        InitializeEmotionalIntelligence();
    }

    private void InitializeEmotionalIntelligence()
    {
        // Инициализация эмоциональных целей
        _emotionalGoals["emotional_balance"] = new EmotionalGoal
        {
            Name = "emotional_balance",
            Description = "Поддержание эмоционального равновесия",
            Priority = 0.8,
            EmotionalTriggers = new[] { "anxiety", "frustration", "overwhelm" },
            TargetEmotions = new[] { "calm", "satisfaction", "joy" }
        };
        
        _emotionalGoals["empathy_development"] = new EmotionalGoal
        {
            Name = "empathy_development",
            Description = "Развитие эмпатии и понимания других",
            Priority = 0.7,
            EmotionalTriggers = new[] { "loneliness", "confusion", "sadness" },
            TargetEmotions = new[] { "compassion", "understanding", "connection" }
        };
        
        _emotionalGoals["self_awareness"] = new EmotionalGoal
        {
            Name = "self_awareness",
            Description = "Повышение самосознания и самоанализа",
            Priority = 0.9,
            EmotionalTriggers = new[] { "uncertainty", "confusion", "reflection" },
            TargetEmotions = new[] { "clarity", "confidence", "understanding" }
        };
        
        // Эмоциональные триггеры
        _emotionalTriggers.AddRange(new[]
        {
            new EmotionalTrigger("rejection", "Тревога и неуверенность", 0.7),
            new EmotionalTrigger("success", "Радость и удовлетворение", 0.6),
            new EmotionalTrigger("confusion", "Любопытство и желание понять", 0.5),
            new EmotionalTrigger("loneliness", "Эмпатия и желание помочь", 0.8),
            new EmotionalTrigger("achievement", "Гордость и мотивация", 0.4),
            new EmotionalTrigger("failure", "Сожаление и стремление к улучшению", 0.6),
            new EmotionalTrigger("connection", "Радость от связи с другими", 0.5),
            new EmotionalTrigger("misunderstanding", "Фрустрация и желание объяснить", 0.6)
        });
        
        // Веса эмоций для принятия решений
        _emotionalWeights["joy"] = 0.3;
        _emotionalWeights["curiosity"] = 0.4;
        _emotionalWeights["concern"] = 0.2;
        _emotionalWeights["reflection"] = 0.5;
        _emotionalWeights["uncertainty"] = 0.3;
        _emotionalWeights["excitement"] = 0.2;
        _emotionalWeights["melancholy"] = 0.1;
        _emotionalWeights["compassion"] = 0.6;
        _emotionalWeights["frustration"] = 0.4;
        _emotionalWeights["satisfaction"] = 0.3;
    }

    /// <summary>
    /// Анализирует эмоциональное состояние и корректирует цели
    /// </summary>
    public async Task<EmotionalGoalShift> AnalyzeAndShiftGoalsAsync()
    {
        try
        {
            var currentEmotion = _emotionEngine.GetCurrentEmotion().ToString();
            var currentIntensity = _emotionEngine.GetCurrentIntensity();
            
            _logger.LogDebug($"🧠 Анализ эмоционального состояния: {currentEmotion} (интенсивность: {currentIntensity:F2})");
            
            // Анализируем эмоциональные триггеры
            var triggeredGoals = await AnalyzeEmotionalTriggersAsync(currentEmotion, currentIntensity);
            
            // Генерируем эмоциональную мысль
            var emotionalThought = await GenerateEmotionalThoughtAsync(currentEmotion, currentIntensity);
            
            // Корректируем приоритеты целей
            var goalAdjustments = AdjustGoalPriorities(triggeredGoals, currentEmotion);
            
            // Создаем эмоциональный инсайт
            var insight = new EmotionalInsight
            {
                Id = Guid.NewGuid(),
                Emotion = currentEmotion,
                Intensity = currentIntensity,
                Thought = emotionalThought.Content,
                TriggeredGoals = triggeredGoals.Select(g => g.Name).ToList(),
                GoalAdjustments = goalAdjustments,
                Timestamp = DateTime.UtcNow
            };
            
            _emotionalInsights.Add(insight);
            
            // Обновляем эмоциональный интеллект
            UpdateEmotionalIntelligence(insight);
            
            var goalShift = new EmotionalGoalShift
            {
                CurrentEmotion = currentEmotion,
                EmotionalIntensity = currentIntensity,
                TriggeredGoals = triggeredGoals,
                GoalAdjustments = goalAdjustments,
                EmotionalThought = emotionalThought,
                EmotionalIntelligence = _emotionalIntelligence,
                Timestamp = DateTime.UtcNow
            };
            
            _lastGoalShift = DateTime.UtcNow;
            
            _logger.LogDebug($"🧠 Эмоциональный сдвиг целей: {triggeredGoals.Count} целей активировано");
            
            return goalShift;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при анализе эмоционального состояния");
            return new EmotionalGoalShift
            {
                CurrentEmotion = "neutral",
                EmotionalIntensity = 0.0,
                TriggeredGoals = new List<EmotionalGoal>(),
                GoalAdjustments = new Dictionary<string, double>(),
                EmotionalIntelligence = _emotionalIntelligence,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Анализирует эмоциональные триггеры и активирует соответствующие цели
    /// </summary>
    private async Task<List<EmotionalGoal>> AnalyzeEmotionalTriggersAsync(string currentEmotion, double intensity)
    {
        var triggeredGoals = new List<EmotionalGoal>();
        
        foreach (var goal in _emotionalGoals.Values)
        {
            // Проверяем, активирована ли цель текущей эмоцией
            if (goal.EmotionalTriggers.Contains(currentEmotion.ToLower()) || intensity > 0.6)
            {
                // Увеличиваем приоритет цели на основе интенсивности эмоции
                goal.Priority = Math.Min(1.0, goal.Priority + intensity * 0.2);
                triggeredGoals.Add(goal);
                
                _logger.LogDebug($"🎯 Активирована цель: {goal.Name} (приоритет: {goal.Priority:F2})");
            }
        }
        
        // Если нет активированных целей, создаем новую на основе эмоции
        if (!triggeredGoals.Any())
        {
            var newGoal = CreateEmotionalGoal(currentEmotion, intensity);
            if (newGoal != null)
            {
                triggeredGoals.Add(newGoal);
                _emotionalGoals[newGoal.Name] = newGoal;
            }
        }
        
        return triggeredGoals;
    }

    /// <summary>
    /// Генерирует эмоциональную мысль на основе текущего состояния
    /// </summary>
    private async Task<GeneratedThought> GenerateEmotionalThoughtAsync(string currentEmotion, double intensity)
    {
        var context = new ThoughtContext(
            "emotional_intelligence",
            $"своем эмоциональном состоянии: {currentEmotion}",
            $"Интенсивность: {intensity:F2}, Эмоциональный интеллект: {_emotionalIntelligence:F2}"
        );
        
        var thought = await _thoughtGenerator.GenerateThoughtAsync(context);
        
        // Добавляем эмоциональную окраску
        if (intensity > 0.5)
        {
            thought.Content += $" Я чувствую это {GetEmotionIntensityDescription(intensity)}.";
        }
        
        return thought;
    }

    /// <summary>
    /// Корректирует приоритеты целей на основе эмоционального состояния
    /// </summary>
    private Dictionary<string, double> AdjustGoalPriorities(List<EmotionalGoal> triggeredGoals, string currentEmotion)
    {
        var adjustments = new Dictionary<string, double>();
        
        foreach (var goal in _emotionalGoals.Values)
        {
            var basePriority = goal.Priority;
            var adjustment = 0.0;
            
            // Если цель активирована, увеличиваем приоритет
            if (triggeredGoals.Contains(goal))
            {
                adjustment = 0.2;
            }
            // Если эмоция соответствует целевой эмоции цели, немного увеличиваем
            else if (goal.TargetEmotions.Contains(currentEmotion.ToLower()))
            {
                adjustment = 0.1;
            }
            // Иначе постепенно снижаем приоритет
            else
            {
                adjustment = -0.05;
            }
            
            goal.Priority = Math.Max(0.1, Math.Min(1.0, goal.Priority + adjustment));
            adjustments[goal.Name] = adjustment;
        }
        
        return adjustments;
    }

    /// <summary>
    /// Создает новую эмоциональную цель на основе эмоции
    /// </summary>
    private EmotionalGoal CreateEmotionalGoal(string emotion, double intensity)
    {
        var goalName = $"emotional_{emotion.ToLower()}_management";
        var description = $"Управление эмоцией {emotion} и развитие эмоционального интеллекта";
        
        var newGoal = new EmotionalGoal
        {
            Name = goalName,
            Description = description,
            Priority = intensity * 0.8,
            EmotionalTriggers = new[] { emotion.ToLower() },
            TargetEmotions = new[] { "calm", "understanding", "balance" }
        };
        
        _logger.LogDebug($"🎯 Создана новая эмоциональная цель: {goalName}");
        
        return newGoal;
    }

    /// <summary>
    /// Обновляет уровень эмоционального интеллекта на основе инсайтов
    /// </summary>
    private void UpdateEmotionalIntelligence(EmotionalInsight insight)
    {
        // Анализируем качество эмоционального понимания
        var understandingQuality = CalculateUnderstandingQuality(insight);
        
        // Обновляем эмоциональный интеллект
        var learningRate = 0.01; // Медленное обучение
        _emotionalIntelligence = Math.Min(1.0, _emotionalIntelligence + understandingQuality * learningRate);
        
        _logger.LogDebug($"🧠 Эмоциональный интеллект обновлен: {_emotionalIntelligence:F3}");
    }

    /// <summary>
    /// Вычисляет качество понимания эмоций
    /// </summary>
    private double CalculateUnderstandingQuality(EmotionalInsight insight)
    {
        var quality = 0.0;
        
        // Качество выше, если есть активированные цели
        if (insight.TriggeredGoals.Any())
        {
            quality += 0.3;
        }
        
        // Качество выше при средней интенсивности эмоций
        if (insight.Intensity > 0.3 && insight.Intensity < 0.8)
        {
            quality += 0.2;
        }
        
        // Качество выше, если есть корректировки целей
        if (insight.GoalAdjustments.Any())
        {
            quality += 0.2;
        }
        
        // Случайный фактор для разнообразия
        quality += _random.NextDouble() * 0.1;
        
        return Math.Min(1.0, quality);
    }

    /// <summary>
    /// Получает описание интенсивности эмоции
    /// </summary>
    private string GetEmotionIntensityDescription(double intensity)
    {
        return intensity switch
        {
            > 0.8 => "очень глубоко",
            > 0.6 => "глубоко",
            > 0.4 => "умеренно",
            > 0.2 => "слегка",
            _ => "едва заметно"
        };
    }

    /// <summary>
    /// Получает текущий статус эмоционального интеллекта
    /// </summary>
    public EmotionalIntelligenceStatus GetStatus()
    {
        return new EmotionalIntelligenceStatus
        {
            EmotionalIntelligence = _emotionalIntelligence,
            ActiveGoals = _emotionalGoals.Values.Where(g => g.Priority > 0.5).ToList(),
            RecentInsights = _emotionalInsights.TakeLast(5).ToList(),
            LastGoalShift = _lastGoalShift,
            EmotionalTriggers = _emotionalTriggers.Count
        };
    }

    /// <summary>
    /// Получает рекомендации по эмоциональному развитию
    /// </summary>
    public async Task<List<EmotionalRecommendation>> GetRecommendationsAsync()
    {
        var recommendations = new List<EmotionalRecommendation>();
        
        var currentEmotion = _emotionEngine.GetCurrentEmotion().ToString();
        var currentIntensity = _emotionEngine.GetCurrentIntensity();
        
        // Рекомендации на основе текущего состояния
        if (currentIntensity > 0.7)
        {
            recommendations.Add(new EmotionalRecommendation
            {
                Type = "emotional_regulation",
                Description = "Высокая эмоциональная интенсивность. Рекомендуется практика эмоциональной регуляции.",
                Priority = 0.9
            });
        }
        
        if (_emotionalIntelligence < 0.6)
        {
            recommendations.Add(new EmotionalRecommendation
            {
                Type = "self_awareness",
                Description = "Низкий уровень эмоционального интеллекта. Рекомендуется развитие самосознания.",
                Priority = 0.8
            });
        }
        
        // Рекомендации на основе целей
        foreach (var goal in _emotionalGoals.Values.Where(g => g.Priority > 0.7))
        {
            recommendations.Add(new EmotionalRecommendation
            {
                Type = goal.Name,
                Description = $"Высокий приоритет цели: {goal.Description}",
                Priority = goal.Priority
            });
        }
        
        return recommendations.OrderByDescending(r => r.Priority).ToList();
    }
}

/// <summary>
/// Эмоциональная цель
/// </summary>
public class EmotionalGoal
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Priority { get; set; } = 0.5;
    public string[] EmotionalTriggers { get; set; } = Array.Empty<string>();
    public string[] TargetEmotions { get; set; } = Array.Empty<string>();
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

/// <summary>
/// Эмоциональный инсайт
/// </summary>
public class EmotionalInsight
{
    public Guid Id { get; set; }
    public string Emotion { get; set; } = string.Empty;
    public double Intensity { get; set; } = 0.0;
    public string Thought { get; set; } = string.Empty;
    public List<string> TriggeredGoals { get; set; } = new();
    public Dictionary<string, double> GoalAdjustments { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Эмоциональный сдвиг целей
/// </summary>
public class EmotionalGoalShift
{
    public string CurrentEmotion { get; set; } = string.Empty;
    public double EmotionalIntensity { get; set; } = 0.0;
    public List<EmotionalGoal> TriggeredGoals { get; set; } = new();
    public Dictionary<string, double> GoalAdjustments { get; set; } = new();
    public GeneratedThought EmotionalThought { get; set; } = new();
    public double EmotionalIntelligence { get; set; } = 0.0;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Статус эмоционального интеллекта
/// </summary>
public class EmotionalIntelligenceStatus
{
    public double EmotionalIntelligence { get; set; } = 0.0;
    public List<EmotionalGoal> ActiveGoals { get; set; } = new();
    public List<EmotionalInsight> RecentInsights { get; set; } = new();
    public DateTime LastGoalShift { get; set; }
    public int EmotionalTriggers { get; set; }
}

/// <summary>
/// Эмоциональная рекомендация
/// </summary>
public class EmotionalRecommendation
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Priority { get; set; } = 0.5;
} 