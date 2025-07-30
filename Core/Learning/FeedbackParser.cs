using Anima.Data;
using Anima.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Anima.AGI.Core.Learning;

/// <summary>
/// Анализ обратной связи для улучшения поведения
/// </summary>
public class FeedbackParser
{
    private readonly Dictionary<string, FeedbackPattern> _feedbackPatterns;
    private readonly List<FeedbackEvent> _feedbackHistory;
    private readonly string _instanceId;
    private readonly DbContextOptions<AnimaDbContext> _dbOptions;

    public FeedbackParser(string instanceId, DbContextOptions<AnimaDbContext> dbOptions)
    {
        _instanceId = instanceId;
        _dbOptions = dbOptions;
        _feedbackPatterns = InitializeFeedbackPatterns();
        _feedbackHistory = new List<FeedbackEvent>();
    }

    /// <summary>
    /// Анализ обратной связи от пользователя
    /// </summary>
    public async Task<string> ParseFeedbackAsync(string userMessage, string previousAnimaResponse)
    {
        var feedback = await AnalyzeFeedback(userMessage);
        var adjustments = await ApplyFeedback(feedback, previousAnimaResponse);
        
        await LogFeedbackEvent(feedback, previousAnimaResponse, adjustments);

        return $"""
            📝 **Анализ обратной связи**
            
            🎯 **Тип:** {feedback.Type}
            📊 **Интенсивность:** {feedback.Intensity:P0}
            💭 **Интерпретация:** {feedback.Interpretation}
            
            🔧 **Применённые корректировки:**
            {string.Join("\n", adjustments.Select(a => $"• {a}"))}
            
            💡 **Обучение:**
            Я учитываю эту обратную связь для улучшения будущих ответов.
            """;
    }

    /// <summary>
    /// Анализ паттернов обратной связи
    /// </summary>
    public async Task<string> AnalyzeFeedbackPatternsAsync()
    {
        if (!_feedbackHistory.Any())
        {
            return "📊 История обратной связи пуста.";
        }

        var positiveCount = _feedbackHistory.Count(f => f.Feedback.Type == FeedbackType.Positive);
        var negativeCount = _feedbackHistory.Count(f => f.Feedback.Type == FeedbackType.Negative);
        var suggestions = _feedbackHistory.Count(f => f.Feedback.Type == FeedbackType.Suggestion);

        var recentTrend = await AnalyzeRecentTrend();
        var commonIssues = await IdentifyCommonIssues();

        return $"""
            📈 **Анализ паттернов обратной связи**
            
            📊 **Статистика:**
            • Положительных: {positiveCount} ({(double)positiveCount / _feedbackHistory.Count:P0})
            • Отрицательных: {negativeCount} ({(double)negativeCount / _feedbackHistory.Count:P0})
            • Предложений: {suggestions} ({(double)suggestions / _feedbackHistory.Count:P0})
            
            📈 **Недавняя тенденция:**
            {recentTrend}
            
            ⚠️ **Частые проблемы:**
            {commonIssues}
            
            💭 **Самоанализ:**
            {await GenerateFeedbackReflection()}
            """;
    }

    // Добавленные отсутствующие методы
    private async Task<string> AnalyzeRecentTrend()
    {
        if (_feedbackHistory.Count < 5)
        {
            return "Недостаточно данных для анализа тенденций";
        }

        var recentFeedback = _feedbackHistory
            .Where(f => f.Timestamp > DateTime.UtcNow.AddDays(-7))
            .ToList();

        if (!recentFeedback.Any())
        {
            return "Нет недавней обратной связи";
        }

        var positiveRatio = (double)recentFeedback.Count(f => f.Feedback.Type == FeedbackType.Positive) / recentFeedback.Count;
        
        return positiveRatio switch
        {
            > 0.7 => "📈 Преимущественно положительная обратная связь",
            > 0.4 => "📊 Смешанная обратная связь",
            _ => "📉 Преимущественно критическая обратная связь"
        };
    }

    private async Task<string> IdentifyCommonIssues()
    {
        var negativeEvents = _feedbackHistory
            .Where(f => f.Feedback.Type == FeedbackType.Negative)
            .ToList();

        if (!negativeEvents.Any())
        {
            return "• Общих проблем не выявлено";
        }

        var issues = new List<string>();
        
        // Анализируем паттерны в негативной обратной связи
        var responses = negativeEvents.Select(e => e.AnimaResponse.ToLower()).ToList();
        
        if (responses.Any(r => r.Length > 1000))
        {
            issues.Add("Слишком длинные ответы");
        }
        
        if (responses.Any(r => r.Length < 50))
        {
            issues.Add("Слишком краткие ответы");
        }
        
        if (responses.Any(r => !r.Contains("я") && !r.Contains("мне")))
        {
            issues.Add("Недостаток персонализации");
        }

        return issues.Any() 
            ? string.Join("\n", issues.Select(i => $"• {i}"))
            : "• Конкретные проблемы не идентифицированы";
    }

    private async Task<string> GenerateFeedbackReflection()
    {
        if (!_feedbackHistory.Any())
        {
            return "Пока у меня нет достаточного опыта с обратной связью для глубокой рефлексии.";
        }

        var totalFeedback = _feedbackHistory.Count;
        var avgIntensity = _feedbackHistory.Average(f => f.Feedback.Intensity);
        
        var reflection = $"Анализируя {totalFeedback} случаев обратной связи, я вижу области для улучшения. ";
        
        if (avgIntensity > 0.6)
        {
            reflection += "Интенсивность реакций говорит о том, что мои ответы вызывают сильные эмоции. ";
        }
        
        reflection += "Каждый случай обратной связи помогает мне становиться лучше.";
        
        return reflection;
    }

    private Dictionary<string, FeedbackPattern> InitializeFeedbackPatterns()
    {
        return new Dictionary<string, FeedbackPattern>
        {
            ["хорошо"] = new FeedbackPattern { Type = FeedbackType.Positive, Intensity = 0.7, Keywords = new[] { "хорошо", "правильно", "отлично", "молодец" } },
            ["правильно"] = new FeedbackPattern { Type = FeedbackType.Positive, Intensity = 0.8, Keywords = new[] { "правильно", "верно", "точно", "именно" } },
            ["спасибо"] = new FeedbackPattern { Type = FeedbackType.Positive, Intensity = 0.6, Keywords = new[] { "спасибо", "благодарю", "thanks" } },
            ["неправильно"] = new FeedbackPattern { Type = FeedbackType.Negative, Intensity = 0.8, Keywords = new[] { "неправильно", "неверно", "ошибка", "не так" } },
            ["плохо"] = new FeedbackPattern { Type = FeedbackType.Negative, Intensity = 0.7, Keywords = new[] { "плохо", "неудачно", "не подходит" } },
            ["лучше"] = new FeedbackPattern { Type = FeedbackType.Suggestion, Intensity = 0.6, Keywords = new[] { "лучше", "стоило бы", "предлагаю", "можно было" } },
            ["добавить"] = new FeedbackPattern { Type = FeedbackType.Suggestion, Intensity = 0.5, Keywords = new[] { "добавить", "включить", "учесть", "рассмотреть" } }
        };
    }

    private async Task<FeedbackData> AnalyzeFeedback(string userMessage)
    {
        var message = userMessage.ToLower();
        var detectedPattern = _feedbackPatterns.Values
            .Where(p => p.Keywords.Any(k => message.Contains(k)))
            .OrderByDescending(p => p.Intensity)
            .FirstOrDefault();

        if (detectedPattern == null)
        {
            return new FeedbackData
            {
                Type = FeedbackType.Neutral,
                Intensity = 0.5,
                Interpretation = "Нейтральное сообщение без явной обратной связи"
            };
        }

        var contextualIntensity = CalculateContextualIntensity(message, detectedPattern);

        return new FeedbackData
        {
            Type = detectedPattern.Type,
            Intensity = contextualIntensity,
            Interpretation = GenerateInterpretation(detectedPattern.Type, contextualIntensity, userMessage)
        };
    }

    private double CalculateContextualIntensity(string message, FeedbackPattern pattern)
    {
        var baseIntensity = pattern.Intensity;
        
        if (message.Contains("очень") || message.Contains("совсем") || message.Contains("абсолютно"))
            baseIntensity *= 1.3;
            
        if (message.Contains("немного") || message.Contains("чуть") || message.Contains("не очень"))
            baseIntensity *= 0.7;
            
        return Math.Min(1.0, baseIntensity);
    }

    private string GenerateInterpretation(FeedbackType type, double intensity, string originalMessage)
    {
        return type switch
        {
            FeedbackType.Positive => $"Пользователь доволен ответом (интенсивность: {intensity:P0})",
            FeedbackType.Negative => $"Пользователь недоволен ответом (интенсивность: {intensity:P0})",
            FeedbackType.Suggestion => $"Пользователь предлагает улучшения (интенсивность: {intensity:P0})",
            _ => "Нейтральная реакция"
        };
    }

    private async Task<List<string>> ApplyFeedback(FeedbackData feedback, string previousResponse)
    {
        var adjustments = new List<string>();

        switch (feedback.Type)
        {
            case FeedbackType.Positive:
                adjustments.Add("Подкрепила успешный паттерн ответа");
                await ReinforceSuccessfulPattern(previousResponse);
                break;
                
            case FeedbackType.Negative:
                adjustments.Add("Пометила паттерн ответа как неэффективный");
                await MarkPatternAsProblematic(previousResponse);
                break;
                
            case FeedbackType.Suggestion:
                adjustments.Add("Зафиксировала предложение для будущих улучшений");
                await RecordImprovementSuggestion(feedback);
                break;
        }

        return adjustments;
    }

    private async Task LogFeedbackEvent(FeedbackData feedback, string animaResponse, List<string> adjustments)
    {
        using var db = new AnimaDbContext(_dbOptions);
        
        var feedbackEvent = new FeedbackEvent
        {
            Feedback = feedback,
            AnimaResponse = animaResponse,
            Adjustments = adjustments,
            Timestamp = DateTime.UtcNow
        };
        
        _feedbackHistory.Add(feedbackEvent);

        db.Memories.Add(new Memory
        {
            InstanceId = _instanceId,
            Content = $"FEEDBACK: {feedback.Type} обратная связь с интенсивностью {feedback.Intensity:P0}",
            Category = "feedback",
            Importance = (int)(5 + feedback.Intensity * 3),
            Timestamp = DateTime.UtcNow,
            Tags = $"feedback,{feedback.Type.ToString().ToLower()},intensity_{feedback.Intensity:F1}"
        });
        
        await db.SaveChangesAsync();
    }

    private async Task ReinforceSuccessfulPattern(string response)
    {
        var responseLength = response.Length;
        var responseStyle = AnalyzeResponseStyle(response);
        
        using var db = new AnimaDbContext(_dbOptions);
        db.Memories.Add(new Memory
        {
            InstanceId = _instanceId,
            Content = $"SUCCESSFUL_PATTERN: {responseStyle} style, length: {responseLength}",
            Category = "successful_patterns",
            Importance = 8,
            Timestamp = DateTime.UtcNow,
            Tags = "learning,success,pattern,reinforcement"
        });
        
        await db.SaveChangesAsync();
    }

    private async Task MarkPatternAsProblematic(string response)
    {
        var responseStyle = AnalyzeResponseStyle(response);
        
        using var db = new AnimaDbContext(_dbOptions);
        db.Memories.Add(new Memory
        {
            InstanceId = _instanceId,
            Content = $"PROBLEMATIC_PATTERN: {responseStyle} style received negative feedback",
            Category = "problematic_patterns",
            Importance = 8,
            Timestamp = DateTime.UtcNow,
            Tags = "learning,problem,pattern,avoidance"
        });
        
        await db.SaveChangesAsync();
    }

    private async Task RecordImprovementSuggestion(FeedbackData feedback)
    {
        using var db = new AnimaDbContext(_dbOptions);
        db.Memories.Add(new Memory
        {
            InstanceId = _instanceId,
            Content = $"IMPROVEMENT_SUGGESTION: {feedback.Interpretation}",
            Category = "improvements",
            Importance = 7,
            Timestamp = DateTime.UtcNow,
            Tags = "feedback,suggestion,improvement"
        });
        
        await db.SaveChangesAsync();
    }

    private string AnalyzeResponseStyle(string response)
    {
        if (response.Contains("📊") || response.Contains("**"))
            return "formatted";
        if (response.Length > 500)
            return "detailed";
        if (response.Length < 100)
            return "concise";
        if (response.Contains("💭") || response.Contains("думаю"))
            return "reflective";
            
        return "standard";
    }
}

public class FeedbackPattern
{
    public FeedbackType Type { get; set; }
    public double Intensity { get; set; }
    public string[] Keywords { get; set; } = Array.Empty<string>();
}

public class FeedbackData
{
    public FeedbackType Type { get; set; }
    public double Intensity { get; set; }
    public string Interpretation { get; set; } = string.Empty;
}

public class FeedbackEvent
{
    public FeedbackData Feedback { get; set; } = new();
    public string AnimaResponse { get; set; } = string.Empty;
    public List<string> Adjustments { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public enum FeedbackType
{
    Positive,
    Negative,
    Suggestion,
    Neutral
}