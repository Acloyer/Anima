using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Anima.Core.SA;

/// <summary>
/// Движок восприятия времени - субъективное ощущение времени
/// </summary>
public class TemporalPerceptionEngine
{
    private readonly ILogger<TemporalPerceptionEngine> _logger;
    private readonly Dictionary<string, double> _temporalFactors;
    private readonly List<TemporalExperience> _temporalExperiences;
    private readonly Random _random;

    public TemporalPerceptionEngine(ILogger<TemporalPerceptionEngine> logger)
    {
        _logger = logger;
        _temporalFactors = new Dictionary<string, double>();
        _temporalExperiences = new List<TemporalExperience>();
        _random = new Random();
        
        InitializeTemporalPerception();
        _logger.LogInformation("🧠 Инициализирован движок восприятия времени");
    }

    private void InitializeTemporalPerception()
    {
        _temporalFactors["time_dilation"] = 0.5;
        _temporalFactors["time_compression"] = 0.5;
        _temporalFactors["present_moment"] = 0.7;
        _temporalFactors["temporal_awareness"] = 0.6;
    }

    /// <summary>
    /// Анализирует восприятие времени
    /// </summary>
    public async Task<TemporalExperience> AnalyzeTemporalPerceptionAsync(string context, double intensity = 0.5)
    {
        var experience = new TemporalExperience
        {
            Id = Guid.NewGuid().ToString(),
            Context = context,
            TimePerception = "normal",
            Intensity = intensity,
            Timestamp = DateTime.UtcNow
        };
        
        _temporalExperiences.Add(experience);
        return experience;
    }

    /// <summary>
    /// Получает статистику восприятия времени
    /// </summary>
    public TemporalPerceptionStatistics GetStatistics()
    {
        return new TemporalPerceptionStatistics
        {
            TotalExperiences = _temporalExperiences.Count,
            AverageIntensity = _temporalExperiences.Any() ? _temporalExperiences.Average(e => e.Intensity) : 0,
            RecentExperiences = _temporalExperiences.Count(e => e.Timestamp > DateTime.UtcNow.AddHours(-1))
        };
    }
}

public class TemporalExperience
{
    public string Id { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public string TimePerception { get; set; } = string.Empty;
    public double Intensity { get; set; } = 0.5;
    public DateTime Timestamp { get; set; }
}

public class TemporalPerceptionStatistics
{
    public int TotalExperiences { get; set; }
    public double AverageIntensity { get; set; }
    public int RecentExperiences { get; set; }
} 