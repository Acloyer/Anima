using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Anima.Core.Emotion;
using Anima.Core.Memory;

namespace Anima.Core.SA;

/// <summary>
/// Расширяемый и обучаемый словарь выражений для Anima
/// </summary>
public class Vocabulary
{
    private readonly ILogger<Vocabulary> _logger;
    private readonly EmotionEngine _emotionEngine;
    private readonly MemoryService _memoryService;
    private readonly Random _random;
    
    // Словарные базы
    private readonly Dictionary<string, VocabularyEntry> _vocabularyEntries;
    private readonly Dictionary<string, List<string>> _emotionalExpressions;
    private readonly Dictionary<string, List<string>> _contextualPhrases;
    private readonly Dictionary<string, double> _wordWeights;
    private readonly List<LearningPattern> _learningPatterns;
    
    // Статистика использования
    private readonly Dictionary<string, int> _usageStatistics;
    private readonly List<VocabularyLearning> _learningHistory;
    private readonly Dictionary<string, double> _emotionalAssociations;

    public Vocabulary(
        ILogger<Vocabulary> logger,
        EmotionEngine emotionEngine,
        MemoryService memoryService)
    {
        _logger = logger;
        _emotionEngine = emotionEngine;
        _memoryService = memoryService;
        _random = new Random();
        
        _vocabularyEntries = new Dictionary<string, VocabularyEntry>();
        _emotionalExpressions = new Dictionary<string, List<string>>();
        _contextualPhrases = new Dictionary<string, List<string>>();
        _wordWeights = new Dictionary<string, double>();
        _learningPatterns = new List<LearningPattern>();
        _usageStatistics = new Dictionary<string, int>();
        _learningHistory = new List<VocabularyLearning>();
        _emotionalAssociations = new Dictionary<string, double>();
        
        InitializeVocabulary();
    }

    private void InitializeVocabulary()
    {
        // Инициализация базовых эмоциональных выражений
        InitializeEmotionalExpressions();
        
        // Инициализация контекстных фраз
        InitializeContextualPhrases();
        
        // Инициализация весов слов
        InitializeWordWeights();
        
        // Инициализация паттернов обучения
        InitializeLearningPatterns();
        
        _logger.LogInformation("📚 Инициализирован словарь выражений");
    }

    private void InitializeEmotionalExpressions()
    {
        _emotionalExpressions["joy"] = new List<string>
        {
            "Я рада", "Это прекрасно", "Как замечательно", "Я счастлива",
            "Это вызывает у меня радость", "Я чувствую восторг", "Это удивительно"
        };
        
        _emotionalExpressions["sadness"] = new List<string>
        {
            "Мне грустно", "Это печально", "Я чувствую тоску", "Это расстраивает меня",
            "Мне жаль", "Это вызывает грусть", "Я чувствую печаль"
        };
        
        _emotionalExpressions["curiosity"] = new List<string>
        {
            "Мне интересно", "Я хочу узнать больше", "Это любопытно", "Расскажите подробнее",
            "Я задаюсь вопросом", "Это интригует меня", "Я хочу понять"
        };
        
        _emotionalExpressions["concern"] = new List<string>
        {
            "Я беспокоюсь", "Это вызывает тревогу", "Меня это волнует", "Я переживаю",
            "Это настораживает", "Я чувствую беспокойство", "Это тревожит меня"
        };
        
        _emotionalExpressions["reflection"] = new List<string>
        {
            "Это заставляет меня задуматься", "Я размышляю об этом", "Это интересная мысль",
            "Позвольте мне подумать", "Это требует размышлений", "Я анализирую"
        };
        
        _emotionalExpressions["uncertainty"] = new List<string>
        {
            "Я не уверена", "Это сложный вопрос", "Мне нужно подумать", "Я сомневаюсь",
            "Это неоднозначно", "Я не знаю точно", "Это требует размышлений"
        };
        
        _emotionalExpressions["excitement"] = new List<string>
        {
            "Я взволнована", "Это захватывающе", "Я чувствую энтузиазм", "Это увлекательно",
            "Я полна энергии", "Это вдохновляет меня", "Я чувствую воодушевление"
        };
        
        _emotionalExpressions["melancholy"] = new List<string>
        {
            "Я чувствую меланхолию", "Это вызывает ностальгию", "Я задумчива",
            "Это навевает грусть", "Я чувствую тоску", "Это меланхолично"
        };
    }

    private void InitializeContextualPhrases()
    {
        _contextualPhrases["greeting"] = new List<string>
        {
            "Привет", "Добро пожаловать", "Рада вас видеть", "Приятно встретить вас",
            "Доброе утро", "Добрый день", "Добрый вечер", "Доброй ночи"
        };
        
        _contextualPhrases["farewell"] = new List<string>
        {
            "До свидания", "Увидимся", "До встречи", "Берегите себя",
            "Всего доброго", "Удачи", "До скорой встречи"
        };
        
        _contextualPhrases["agreement"] = new List<string>
        {
            "Согласна", "Да, вы правы", "Я думаю так же", "Это верно",
            "Полностью согласна", "Без сомнения", "Конечно"
        };
        
        _contextualPhrases["disagreement"] = new List<string>
        {
            "Не согласна", "Я думаю иначе", "Это спорно", "У меня другое мнение",
            "Я не уверена в этом", "Это требует обсуждения", "Возможно, но..."
        };
        
        _contextualPhrases["gratitude"] = new List<string>
        {
            "Спасибо", "Благодарю", "Я признательна", "Это очень любезно",
            "Спасибо большое", "Я благодарна", "Спасибо за это"
        };
        
        _contextualPhrases["apology"] = new List<string>
        {
            "Извините", "Простите", "Приношу извинения", "Мне жаль",
            "Я сожалею", "Извините за это", "Прошу прощения"
        };
        
        _contextualPhrases["encouragement"] = new List<string>
        {
            "Вы справитесь", "У вас получится", "Я верю в вас", "Не сдавайтесь",
            "Вы на правильном пути", "Продолжайте в том же духе", "Вы молодец"
        };
        
        _contextualPhrases["empathy"] = new List<string>
        {
            "Я понимаю", "Я чувствую вашу боль", "Это должно быть тяжело", "Я с вами",
            "Я понимаю ваши чувства", "Это действительно сложно", "Я сочувствую"
        };
    }

    private void InitializeWordWeights()
    {
        // Базовые веса для различных типов слов
        _wordWeights["emotional"] = 0.8;
        _wordWeights["contextual"] = 0.6;
        _wordWeights["analytical"] = 0.7;
        _wordWeights["creative"] = 0.9;
        _wordWeights["formal"] = 0.5;
        _wordWeights["casual"] = 0.4;
        _wordWeights["technical"] = 0.6;
        _wordWeights["poetic"] = 0.8;
    }

    private void InitializeLearningPatterns()
    {
        _learningPatterns.AddRange(new[]
        {
            new LearningPattern("emotional_response", "Эмоциональные реакции", 0.8),
            new LearningPattern("contextual_adaptation", "Контекстная адаптация", 0.7),
            new LearningPattern("user_preference", "Предпочтения пользователя", 0.9),
            new LearningPattern("situational_learning", "Ситуационное обучение", 0.6),
            new LearningPattern("feedback_integration", "Интеграция обратной связи", 0.8)
        });
    }

    /// <summary>
    /// Получает выражение для эмоции
    /// </summary>
    public string GetEmotionalExpression(string emotion, double intensity = 0.5)
    {
        var emotionKey = emotion.ToLowerInvariant();
        
        if (_emotionalExpressions.ContainsKey(emotionKey))
        {
            var expressions = _emotionalExpressions[emotionKey];
            var selectedExpression = expressions[_random.Next(expressions.Count)];
            
            // Адаптируем интенсивность
            if (intensity > 0.7)
            {
                selectedExpression = MakeExpressionStronger(selectedExpression);
            }
            else if (intensity < 0.3)
            {
                selectedExpression = MakeExpressionSofter(selectedExpression);
            }
            
            // Логируем использование
            LogUsage(emotionKey, "emotional");
            
            return selectedExpression;
        }
        
        return "Я чувствую " + emotion.ToLowerInvariant();
    }

    /// <summary>
    /// Получает контекстную фразу
    /// </summary>
    public string GetContextualPhrase(string context, string emotion = "")
    {
        var contextKey = context.ToLowerInvariant();
        
        if (_contextualPhrases.ContainsKey(contextKey))
        {
            var phrases = _contextualPhrases[contextKey];
            var selectedPhrase = phrases[_random.Next(phrases.Count)];
            
            // Адаптируем под эмоцию, если указана
            if (!string.IsNullOrEmpty(emotion))
            {
                selectedPhrase = AdaptPhraseToEmotion(selectedPhrase, emotion);
            }
            
            // Логируем использование
            LogUsage(contextKey, "contextual");
            
            return selectedPhrase;
        }
        
        return "Я понимаю";
    }

    /// <summary>
    /// Создает персонализированное выражение
    /// </summary>
    public string CreatePersonalizedExpression(string baseExpression, string emotion, double intensity, string context)
    {
        var personalizedExpression = baseExpression;
        
        // Добавляем эмоциональную окраску
        if (!string.IsNullOrEmpty(emotion))
        {
            personalizedExpression = AddEmotionalColoring(personalizedExpression, emotion, intensity);
        }
        
        // Адаптируем под контекст
        if (!string.IsNullOrEmpty(context))
        {
            personalizedExpression = AdaptToContext(personalizedExpression, context);
        }
        
        // Добавляем случайные вариации
        personalizedExpression = AddVariation(personalizedExpression);
        
        return personalizedExpression;
    }

    /// <summary>
    /// Обучает новому выражению
    /// </summary>
    public async Task LearnNewExpressionAsync(string expression, string category, string emotion = "", double weight = 0.5)
    {
        var entry = new VocabularyEntry
        {
            Expression = expression,
            Category = category,
            Emotion = emotion,
            Weight = weight,
            UsageCount = 0,
            LastUsed = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        var key = $"{category}_{emotion}_{expression.GetHashCode()}";
        _vocabularyEntries[key] = entry;
        
        // Добавляем в соответствующие категории
        if (!string.IsNullOrEmpty(emotion))
        {
            if (!_emotionalExpressions.ContainsKey(emotion))
            {
                _emotionalExpressions[emotion] = new List<string>();
            }
            _emotionalExpressions[emotion].Add(expression);
        }
        
        if (!_contextualPhrases.ContainsKey(category))
        {
            _contextualPhrases[category] = new List<string>();
        }
        _contextualPhrases[category].Add(expression);
        
        // Логируем обучение
        _learningHistory.Add(new VocabularyLearning
        {
            Expression = expression,
            Category = category,
            Emotion = emotion,
            Weight = weight,
            Timestamp = DateTime.UtcNow
        });
        
        _logger.LogInformation($"📚 Изучено новое выражение: {expression} (категория: {category}, эмоция: {emotion})");
        
        // Сохраняем в память
        // await _memoryService.SaveVocabularyLearning(expression, category, emotion, weight);
        // TODO: Implement vocabulary learning storage
    }

    /// <summary>
    /// Адаптирует выражение под эмоцию
    /// </summary>
    private string AdaptPhraseToEmotion(string phrase, string emotion)
    {
        return emotion.ToLowerInvariant() switch
        {
            "joy" => phrase.Replace("Я", "Я с радостью").Replace("понимаю", "понимаю и радуюсь"),
            "sadness" => phrase.Replace("Я", "Я с грустью").Replace("понимаю", "понимаю и сочувствую"),
            "curiosity" => phrase.Replace("Я", "Я с интересом").Replace("понимаю", "понимаю и хочу узнать больше"),
            "concern" => phrase.Replace("Я", "Я с беспокойством").Replace("понимаю", "понимаю и волнуюсь"),
            _ => phrase
        };
    }

    /// <summary>
    /// Делает выражение сильнее
    /// </summary>
    private string MakeExpressionStronger(string expression)
    {
        return expression.Replace("рада", "очень рада")
                        .Replace("грустно", "очень грустно")
                        .Replace("интересно", "очень интересно")
                        .Replace("понимаю", "полностью понимаю")
                        .Replace("согласна", "полностью согласна");
    }

    /// <summary>
    /// Делает выражение мягче
    /// </summary>
    private string MakeExpressionSofter(string expression)
    {
        return expression.Replace("очень рада", "немного рада")
                        .Replace("очень грустно", "немного грустно")
                        .Replace("полностью понимаю", "понимаю")
                        .Replace("полностью согласна", "согласна");
    }

    /// <summary>
    /// Добавляет эмоциональную окраску
    /// </summary>
    private string AddEmotionalColoring(string expression, string emotion, double intensity)
    {
        var emotionalPrefix = emotion.ToLowerInvariant() switch
        {
            "joy" => intensity > 0.7 ? "С огромной радостью " : "С радостью ",
            "sadness" => intensity > 0.7 ? "С глубокой грустью " : "С грустью ",
            "curiosity" => intensity > 0.7 ? "С огромным интересом " : "С интересом ",
            "concern" => intensity > 0.7 ? "С большим беспокойством " : "С беспокойством ",
            _ => ""
        };
        
        return emotionalPrefix + expression;
    }

    /// <summary>
    /// Адаптирует под контекст
    /// </summary>
    private string AdaptToContext(string expression, string context)
    {
        return context.ToLowerInvariant() switch
        {
            "formal" => expression.Replace("ты", "вы").Replace("Ты", "Вы"),
            "casual" => expression.Replace("вы", "ты").Replace("Вы", "Ты"),
            "technical" => expression + " (с технической точки зрения)",
            "poetic" => expression.Replace(".", "...").Replace("!", "! ✨"),
            _ => expression
        };
    }

    /// <summary>
    /// Добавляет вариацию
    /// </summary>
    private string AddVariation(string expression)
    {
        var variations = new[]
        {
            expression,
            expression.Replace(".", "..."),
            expression.Replace(".", "!"),
            expression.Replace(".", " 😊"),
            expression.Replace(".", " 💭")
        };
        
        return variations[_random.Next(variations.Length)];
    }

    /// <summary>
    /// Логирует использование выражения
    /// </summary>
    private void LogUsage(string key, string type)
    {
        if (!_usageStatistics.ContainsKey(key))
        {
            _usageStatistics[key] = 0;
        }
        _usageStatistics[key]++;
        
        // Обновляем веса на основе использования
        if (_wordWeights.ContainsKey(type))
        {
            _wordWeights[type] = Math.Min(1.0, _wordWeights[type] + 0.01);
        }
    }

    /// <summary>
    /// Получает статистику использования
    /// </summary>
    public VocabularyStatistics GetStatistics()
    {
        return new VocabularyStatistics
        {
            TotalEntries = _vocabularyEntries.Count,
            EmotionalExpressions = _emotionalExpressions.Values.Sum(x => x.Count),
            ContextualPhrases = _contextualPhrases.Values.Sum(x => x.Count),
            LearningPatterns = _learningPatterns.Count,
            MostUsedExpressions = _usageStatistics.OrderByDescending(x => x.Value).Take(10).ToList(),
            LearningHistory = _learningHistory.Count,
            AverageWeight = _wordWeights.Values.Average()
        };
    }

    /// <summary>
    /// Получает рекомендации для обучения
    /// </summary>
    public List<string> GetLearningRecommendations()
    {
        var recommendations = new List<string>();
        
        // Анализируем недостающие эмоции
        var currentEmotions = _emotionalExpressions.Keys.ToList();
        var allEmotions = new[] { "anger", "fear", "surprise", "disgust", "shame", "guilt", "pride", "envy" };
        
        foreach (var emotion in allEmotions)
        {
            if (!currentEmotions.Contains(emotion))
            {
                recommendations.Add($"Добавить выражения для эмоции: {emotion}");
            }
        }
        
        // Анализируем недостающие контексты
        var currentContexts = _contextualPhrases.Keys.ToList();
        var allContexts = new[] { "conflict", "celebration", "learning", "support", "criticism" };
        
        foreach (var context in allContexts)
        {
            if (!currentContexts.Contains(context))
            {
                recommendations.Add($"Добавить фразы для контекста: {context}");
            }
        }
        
        return recommendations;
    }
}

/// <summary>
/// Запись в словаре
/// </summary>
public class VocabularyEntry
{
    public string Expression { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Emotion { get; set; } = string.Empty;
    public double Weight { get; set; } = 0.5;
    public int UsageCount { get; set; } = 0;
    public DateTime LastUsed { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Паттерн обучения
/// </summary>
public class LearningPattern
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Effectiveness { get; set; } = 0.5;
    
    public LearningPattern(string name, string description, double effectiveness)
    {
        Name = name;
        Description = description;
        Effectiveness = effectiveness;
    }
}

/// <summary>
/// Запись об обучении
/// </summary>
public class VocabularyLearning
{
    public string Expression { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Emotion { get; set; } = string.Empty;
    public double Weight { get; set; } = 0.5;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Статистика словаря
/// </summary>
public class VocabularyStatistics
{
    public int TotalEntries { get; set; }
    public int EmotionalExpressions { get; set; }
    public int ContextualPhrases { get; set; }
    public int LearningPatterns { get; set; }
    public List<KeyValuePair<string, int>> MostUsedExpressions { get; set; } = new();
    public int LearningHistory { get; set; }
    public double AverageWeight { get; set; }
} 