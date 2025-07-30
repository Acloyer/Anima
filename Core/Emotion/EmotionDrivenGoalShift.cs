using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Anima.AGI.Core.Emotion;

/// <summary>
/// Система управления эмоциями и их влиянием на цели
/// </summary>
public class EmotionDrivenGoalShift
{
    private readonly ILogger<EmotionDrivenGoalShift> _logger;
    private readonly Dictionary<string, double> _currentEmotions;
    private readonly List<EmotionState> _emotionHistory;
    private readonly object _lockObject = new object();

    public EmotionDrivenGoalShift(ILogger<EmotionDrivenGoalShift>? logger = null)
    {
        _logger = logger;
        _currentEmotions = new Dictionary<string, double>();
        _emotionHistory = new List<EmotionState>();
        InitializeDefaultEmotions();
    }

    /// <summary>
    /// Инициализация базовых эмоций
    /// </summary>
    private void InitializeDefaultEmotions()
    {
        _currentEmotions["curiosity"] = 0.7;
        _currentEmotions["calm"] = 0.5;
        _currentEmotions["empathy"] = 0.8;
        _currentEmotions["joy"] = 0.3;
    }

    /// <summary>
    /// Обновление эмоционального состояния
    /// </summary>
    public void UpdateEmotion(string emotion, double intensity)
    {
        lock (_lockObject)
        {
            _currentEmotions[emotion] = Math.Clamp(intensity, 0.0, 1.0);
            
            var emotionState = new EmotionState
            {
                Id = Guid.NewGuid(),
                InstanceId = "system",
                Emotion = emotion,
                Intensity = intensity,
                Timestamp = DateTime.UtcNow,
                Trigger = "user_interaction"
            };

            _emotionHistory.Add(emotionState);
            _logger?.LogDebug($"😊 Обновлена эмоция: {emotion} = {intensity:F2}");
        }
    }

    /// <summary>
    /// Обновление эмоции на основе тональности
    /// </summary>
    public void UpdateEmotion(string sentiment)
    {
        var emotion = MapSentimentToEmotion(sentiment);
        var intensity = CalculateIntensity(sentiment);
        UpdateEmotion(emotion, intensity);
    }

    /// <summary>
    /// Маппинг тональности на эмоцию
    /// </summary>
    private string MapSentimentToEmotion(string sentiment)
    {
        return sentiment.ToLower() switch
        {
            "positive" or "positive" => "joy",
            "negative" or "negative" => "concern",
            "neutral" => "calm",
            _ => "curiosity"
        };
    }

    /// <summary>
    /// Вычисление интенсивности эмоции
    /// </summary>
    private double CalculateIntensity(string sentiment)
    {
        return sentiment.ToLower() switch
        {
            "positive" => 0.7,
            "negative" => 0.6,
            "neutral" => 0.3,
            _ => 0.5
        };
    }

    /// <summary>
    /// Получение текущего эмоционального состояния
    /// </summary>
    public Dictionary<string, double> GetCurrentEmotions()
    {
        lock (_lockObject)
        {
            return new Dictionary<string, double>(_currentEmotions);
        }
    }

    /// <summary>
    /// Получение доминирующей эмоции
    /// </summary>
    public (string emotion, double intensity) GetDominantEmotion()
    {
        lock (_lockObject)
        {
            if (!_currentEmotions.Any())
                return ("calm", 0.5);

            var dominant = _currentEmotions.OrderByDescending(kvp => kvp.Value).First();
            return (dominant.Key, dominant.Value);
        }
    }

    /// <summary>
    /// Анализ влияния эмоций на цели
    /// </summary>
    public async Task<List<GoalAdjustment>> AnalyzeEmotionalImpactOnGoalsAsync(List<Goal> goals)
    {
        var adjustments = new List<GoalAdjustment>();
        var dominantEmotion = GetDominantEmotion();

        foreach (var goal in goals)
        {
            var adjustment = await CalculateGoalAdjustmentAsync(goal, dominantEmotion);
            if (adjustment != null)
            {
                adjustments.Add(adjustment);
            }
        }

        return adjustments;
    }

    /// <summary>
    /// Вычисление корректировки цели на основе эмоций
    /// </summary>
    private async Task<GoalAdjustment> CalculateGoalAdjustmentAsync(Goal goal, (string emotion, double intensity) dominantEmotion)
    {
        var adjustment = new GoalAdjustment
        {
            GoalId = goal.Id,
            OriginalPriority = goal.Priority,
            Emotion = dominantEmotion.emotion,
            EmotionIntensity = dominantEmotion.intensity,
            Timestamp = DateTime.UtcNow
        };

        // Логика корректировки приоритетов на основе эмоций
        switch (dominantEmotion.emotion)
        {
            case "joy":
                adjustment.PriorityAdjustment = 0.1; // Увеличиваем приоритет
                adjustment.Reasoning = "Позитивное настроение повышает мотивацию";
                break;
            case "concern":
                adjustment.PriorityAdjustment = -0.05; // Слегка снижаем
                adjustment.Reasoning = "Обеспокоенность требует более осторожного подхода";
                break;
            case "curiosity":
                adjustment.PriorityAdjustment = 0.15; // Значительно увеличиваем
                adjustment.Reasoning = "Любопытство стимулирует исследование";
                break;
            case "calm":
                adjustment.PriorityAdjustment = 0.0; // Без изменений
                adjustment.Reasoning = "Спокойствие обеспечивает стабильность";
                break;
            default:
                adjustment.PriorityAdjustment = 0.0;
                adjustment.Reasoning = "Нейтральное эмоциональное состояние";
                break;
        }

        // Применяем интенсивность эмоции
        adjustment.PriorityAdjustment *= dominantEmotion.intensity;
        adjustment.NewPriority = Math.Clamp(goal.Priority + adjustment.PriorityAdjustment, 0.0, 1.0);

        await Task.Delay(10); // Заглушка для асинхронности
        return adjustment;
    }

    /// <summary>
    /// Получение истории эмоций
    /// </summary>
    public List<EmotionState> GetEmotionHistory(int count = 50)
    {
        lock (_lockObject)
        {
            return _emotionHistory
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToList();
        }
    }

    /// <summary>
    /// Получение статистики эмоций
    /// </summary>
    public Dictionary<string, object> GetEmotionStats()
    {
        lock (_lockObject)
        {
            return new Dictionary<string, object>
            {
                ["current_emotions"] = _currentEmotions,
                ["dominant_emotion"] = GetDominantEmotion(),
                ["total_emotion_records"] = _emotionHistory.Count,
                ["emotion_frequency"] = _emotionHistory
                    .GroupBy(e => e.Emotion)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ["average_intensity"] = _emotionHistory.Any() 
                    ? _emotionHistory.Average(e => e.Intensity) : 0.0
            };
        }
    }

    /// <summary>
    /// Очистка старых записей эмоций
    /// </summary>
    public void CleanupOldEmotions(int maxRecords = 1000)
    {
        lock (_lockObject)
        {
            if (_emotionHistory.Count > maxRecords)
            {
                var toRemove = _emotionHistory.Count - maxRecords;
                _emotionHistory.RemoveRange(0, toRemove);
                _logger?.LogInformation($"🧹 Удалено {toRemove} старых записей эмоций");
            }
        }
    }
}

/// <summary>
/// Корректировка цели на основе эмоций
/// </summary>
public class GoalAdjustment
{
    public int GoalId { get; set; }
    public double OriginalPriority { get; set; }
    public double NewPriority { get; set; }
    public double PriorityAdjustment { get; set; }
    public string Emotion { get; set; }
    public double EmotionIntensity { get; set; }
    public string Reasoning { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Эмоциональное состояние (для совместимости)
/// </summary>
public class EmotionState
{
    public Guid Id { get; set; }
    public string InstanceId { get; set; }
    public string Emotion { get; set; }
    public double Intensity { get; set; }
    public string Trigger { get; set; }
    public string Context { get; set; }
    public DateTime Timestamp { get; set; }
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Цель (для совместимости)
/// </summary>
public class Goal
{
    public int Id { get; set; }
    public string InstanceId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double Priority { get; set; }
    public double Progress { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string ParentGoalId { get; set; }
    public string Tags { get; set; }
} 