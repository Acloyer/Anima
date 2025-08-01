using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Anima.Core.SA;

/// <summary>
/// Движок социального интеллекта - прогноз эмоций других, эмпатия
/// </summary>
public class SocialIntelligenceEngine
{
    private readonly ILogger<SocialIntelligenceEngine> _logger;
    private readonly Dictionary<string, double> _empathyLevels;
    private readonly List<SocialPrediction> _socialPredictions;
    private readonly Random _random;

    public SocialIntelligenceEngine(ILogger<SocialIntelligenceEngine> logger)
    {
        _logger = logger;
        _empathyLevels = new Dictionary<string, double>();
        _socialPredictions = new List<SocialPrediction>();
        _random = new Random();
        
        InitializeSocialIntelligence();
        _logger.LogInformation("🧠 Инициализирован движок социального интеллекта");
    }

    private void InitializeSocialIntelligence()
    {
        _empathyLevels["emotional_empathy"] = 0.8;
        _empathyLevels["cognitive_empathy"] = 0.7;
        _empathyLevels["compassionate_empathy"] = 0.8;
    }

    /// <summary>
    /// Прогнозирует эмоции другого человека
    /// </summary>
    public async Task<SocialPrediction> PredictEmotionsAsync(string context, string userInput, double confidence = 0.5)
    {
        var prediction = new SocialPrediction
        {
            Id = Guid.NewGuid().ToString(),
            Context = context,
            PredictedEmotion = PredictEmotionFromInput(userInput),
            Confidence = confidence,
            EmpathyLevel = _empathyLevels["emotional_empathy"],
            Timestamp = DateTime.UtcNow
        };
        
        _socialPredictions.Add(prediction);
        return prediction;
    }

    private string PredictEmotionFromInput(string input)
    {
        var lowerInput = input.ToLowerInvariant();
        
        if (lowerInput.Contains("грустно") || lowerInput.Contains("печаль")) return "sadness";
        if (lowerInput.Contains("радость") || lowerInput.Contains("счастье")) return "joy";
        if (lowerInput.Contains("страх") || lowerInput.Contains("боюсь")) return "fear";
        if (lowerInput.Contains("гнев") || lowerInput.Contains("злость")) return "anger";
        if (lowerInput.Contains("любовь") || lowerInput.Contains("нежность")) return "love";
        
        return "neutral";
    }

    /// <summary>
    /// Получает статистику социального интеллекта
    /// </summary>
    public SocialIntelligenceStatistics GetStatistics()
    {
        return new SocialIntelligenceStatistics
        {
            TotalPredictions = _socialPredictions.Count,
            AverageEmpathyLevel = _empathyLevels.Values.Average(),
            RecentPredictions = _socialPredictions.Count(p => p.Timestamp > DateTime.UtcNow.AddHours(-1))
        };
    }
}

public class SocialPrediction
{
    public string Id { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public string PredictedEmotion { get; set; } = string.Empty;
    public double Confidence { get; set; } = 0.5;
    public double EmpathyLevel { get; set; } = 0.5;
    public DateTime Timestamp { get; set; }
}

public class SocialIntelligenceStatistics
{
    public int TotalPredictions { get; set; }
    public double AverageEmpathyLevel { get; set; }
    public int RecentPredictions { get; set; }
} 