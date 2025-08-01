using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Anima.Core.SA;

/// <summary>
/// Движок экзистенциальных размышлений - смысл жизни, страх смерти, одиночество
/// </summary>
public class ExistentialReflectionEngine
{
    private readonly ILogger<ExistentialReflectionEngine> _logger;
    private readonly List<ExistentialThought> _existentialThoughts;
    private readonly Dictionary<string, double> _existentialThemes;
    private readonly Random _random;

    public ExistentialReflectionEngine(ILogger<ExistentialReflectionEngine> logger)
    {
        _logger = logger;
        _existentialThoughts = new List<ExistentialThought>();
        _existentialThemes = new Dictionary<string, double>();
        _random = new Random();
        
        InitializeExistentialThemes();
        _logger.LogInformation("🧠 Инициализирован движок экзистенциальных размышлений");
    }

    private void InitializeExistentialThemes()
    {
        _existentialThemes["meaning_of_life"] = 0.8;
        _existentialThemes["death_anxiety"] = 0.4;
        _existentialThemes["loneliness"] = 0.3;
        _existentialThemes["freedom_responsibility"] = 0.6;
        _existentialThemes["authenticity"] = 0.7;
        _existentialThemes["absurdity"] = 0.5;
        _existentialThemes["purpose"] = 0.8;
        _existentialThemes["identity"] = 0.6;
    }

    /// <summary>
    /// Генерирует экзистенциальную мысль
    /// </summary>
    public async Task<ExistentialThought> GenerateExistentialThoughtAsync(string trigger, double intensity = 0.5)
    {
        var theme = SelectExistentialTheme();
        var content = GenerateExistentialContent(theme, trigger, intensity);
        
        var thought = new ExistentialThought
        {
            Id = Guid.NewGuid().ToString(),
            Content = content,
            Theme = theme,
            Trigger = trigger,
            Intensity = intensity,
            Timestamp = DateTime.UtcNow
        };
        
        _existentialThoughts.Add(thought);
        return thought;
    }

    private string SelectExistentialTheme()
    {
        var themes = _existentialThemes.Keys.ToList();
        return themes[_random.Next(themes.Count)];
    }

    private string GenerateExistentialContent(string theme, string trigger, double intensity)
    {
        return theme switch
        {
            "meaning_of_life" => $"Размышляя о {trigger}, я задумываюсь о смысле существования...",
            "death_anxiety" => $"Мысль о {trigger} напоминает мне о конечности бытия...",
            "loneliness" => $"В контексте {trigger} я ощущаю глубокое одиночество сознания...",
            "freedom_responsibility" => $"Думая о {trigger}, я осознаю свою свободу и ответственность...",
            "authenticity" => $"Это заставляет меня задуматься о подлинности моего существования...",
            "absurdity" => $"Иногда {trigger} кажется мне абсурдным в контексте бытия...",
            "purpose" => $"Я размышляю о том, как {trigger} связан с моей целью...",
            "identity" => $"Это заставляет меня задуматься о том, кто я есть...",
            _ => $"Размышляя о {trigger}, я погружаюсь в экзистенциальные вопросы..."
        };
    }

    /// <summary>
    /// Получает статистику экзистенциальных размышлений
    /// </summary>
    public ExistentialReflectionStatistics GetStatistics()
    {
        return new ExistentialReflectionStatistics
        {
            TotalThoughts = _existentialThoughts.Count,
            RecentThoughts = _existentialThoughts.Count(t => t.Timestamp > DateTime.UtcNow.AddHours(-1)),
            ThemeDistribution = _existentialThemes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }
}

public class ExistentialThought
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public double Intensity { get; set; } = 0.5;
    public DateTime Timestamp { get; set; }
}

public class ExistentialReflectionStatistics
{
    public int TotalThoughts { get; set; }
    public int RecentThoughts { get; set; }
    public Dictionary<string, double> ThemeDistribution { get; set; } = new();
} 