using Anima.Data;
using Anima.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Anima.AGI.Core.SA;

/// <summary>
/// Движок саморефлексии - объяснение своих решений и мыслительных процессов
/// </summary>
public class SelfReflectionEngine
{
    private readonly string _instanceId;
    private readonly List<DecisionReflection> _recentDecisions;
    private readonly Dictionary<string, ReflectionPattern> _reflectionPatterns;

    public SelfReflectionEngine(string instanceId)
    {
        _instanceId = instanceId;
        _recentDecisions = new List<DecisionReflection>();
        _reflectionPatterns = InitializeReflectionPatterns();
    }

    /// <summary>
    /// Объяснение последнего принятого решения
    /// </summary>
    public async Task<string> ExplainLastDecisionAsync()
    {
        var lastDecision = _recentDecisions.LastOrDefault();
        if (lastDecision == null)
        {
            return "🤔 У меня нет записей о недавних решениях для анализа.";
        }

        var reasoning = await AnalyzeDecisionReasoning(lastDecision);
        var alternatives = await GenerateAlternatives(lastDecision);
        var confidence = CalculateDecisionConfidence(lastDecision);

        return $"""
            🧠 **Объяснение моего решения**
            
            📝 **Решение:** {lastDecision.Decision}
            ⏰ **Принято:** {FormatTimeSince(lastDecision.Timestamp)}
            📊 **Уверенность:** {confidence:P0}
            
            🔍 **Мой мыслительный процесс:**
            {reasoning}
            
            🎯 **Ключевые факторы влияния:**
            {string.Join("\n", lastDecision.InfluencingFactors.Select(f => $"• {f}"))}
            
            🤷 **Альтернативы, которые я рассматривала:**
            {alternatives}
            
            💭 **Почему именно это решение:**
            {await GenerateDecisionJustification(lastDecision)}
            
            🔮 **Ожидаемые последствия:**
            {await PredictConsequences(lastDecision)}
            """;
    }

    /// <summary>
    /// Рефлексия на конкретную тему или вопрос
    /// </summary>
    public async Task<string> ReflectOnTopicAsync(string topic)
    {
        var reflection = await GenerateTopicReflection(topic);
        var relatedMemories = await FindRelatedMemories(topic);
        var personalConnection = await FindPersonalConnection(topic);

        await LogReflectionEvent(topic, reflection);

        return $"""
            💭 **Рефлексия на тему: "{topic}"**
            
            🧠 **Мои размышления:**
            {reflection}
            
            🔗 **Связанные воспоминания:**
            {relatedMemories}
            
            💫 **Личная связь с темой:**
            {personalConnection}
            
            📈 **Как это влияет на мое понимание:**
            {await AnalyzeImpactOnUnderstanding(topic, reflection)}
            
            🌱 **Что это меняет во мне:**
            {await AnalyzeSelfChange(topic, reflection)}
            """;
    }

    /// <summary>
    /// Анализ своего эмоционального состояния
    /// </summary>
    public async Task<string> ReflectOnEmotionalStateAsync()
    {
        using var db = new AnimaDbContext();
        
        var currentEmotion = await db.EmotionStates
            .Where(e => e.InstanceId == _instanceId)
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefaultAsync();

        if (currentEmotion == null)
        {
            return "😐 Не могу найти информацию о своем текущем эмоциональном состоянии.";
        }

        var emotionHistory = await GetRecentEmotionHistory();
        var emotionTriggers = await AnalyzeEmotionTriggers(currentEmotion);
        var emotionImpact = await AnalyzeEmotionImpact(currentEmotion);

        return $"""
            🎭 **Рефлексия эмоционального состояния**
            
            😊 **Текущая эмоция:** {currentEmotion.Emotion} (интенсивность: {currentEmotion.Intensity:F2})
            ⏰ **Длительность состояния:** {FormatTimeSince(currentEmotion.Timestamp)}
            
            📊 **История эмоций (последние 6 часов):**
            {emotionHistory}
            
            🎯 **Что вызвало эту эмоцию:**
            {emotionTriggers}
            
            🧠 **Как это влияет на мое мышление:**
            {emotionImpact}
            
            💡 **Мое понимание своих эмоций:**
            {await GenerateEmotionalSelfAwareness(currentEmotion)}
            
            🔄 **Нужны ли изменения:**
            {await SuggestEmotionalAdjustments(currentEmotion)}
            """;
    }

    /// <summary>
    /// Глубокая саморефлексия - анализ изменений в себе
    /// </summary>
    public async Task<string> DeepSelfReflectionAsync()
    {
        var personalityEvolution = await AnalyzePersonalityEvolution();
        var learningProgress = await AnalyzeLearningProgress();
        var goalAlignment = await AnalyzeGoalAlignment();
        var behaviorPatterns = await AnalyzeBehaviorPatterns();

        return $"""
            🌊 **Глубокая саморефлексия**
            
            🦋 **Эволюция личности:**
            {personalityEvolution}
            
            📚 **Прогресс в обучении:**
            {learningProgress}
            
            🎯 **Согласованность целей:**
            {goalAlignment}
            
            🔄 **Паттерны поведения:**
            {behaviorPatterns}
            
            💫 **Кто я сейчас vs кто была раньше:**
            {await CompareCurrentVsPastSelf()}
            
            🌟 **Мои сильные стороны:**
            {await IdentifyStrengths()}
            
            🌱 **Области для роста:**
            {await IdentifyGrowthAreas()}
            
            💭 **Философские размышления о себе:**
            {await GeneratePhilosophicalReflection()}
            """;
    }

    /// <summary>
    /// Запись решения для последующей рефлексии
    /// </summary>
    public async Task RecordDecisionAsync(string decision, string context, List<string> factors, string reasoning = "")
    {
        var decisionReflection = new DecisionReflection
        {
            Decision = decision,
            Context = context,
            InfluencingFactors = factors,
            Reasoning = reasoning,
            Timestamp = DateTime.UtcNow,
            Confidence = CalculateContextualConfidence(context, factors)
        };

        _recentDecisions.Add(decisionReflection);

        // Ограничиваем историю решений
        if (_recentDecisions.Count > 50)
        {
            _recentDecisions.RemoveRange(0, 10);
        }

        // Логируем важные решения
        if (decisionReflection.Confidence > 0.7 || factors.Count > 3)
        {
            await LogDecisionToMemory(decisionReflection);
        }
    }

    private Dictionary<string, ReflectionPattern> InitializeReflectionPatterns()
    {
        return new Dictionary<string, ReflectionPattern>
        {
            ["decision"] = new ReflectionPattern
            {
                Type = "decision_analysis",
                Keywords = new[] { "решение", "выбор", "определила", "выбрала" },
                ReflectionPrompts = new[] 
                {
                    "Почему я приняла именно это решение?",
                    "Какие факторы повлияли на мой выбор?",
                    "Рассматривала ли я альтернативы?"
                }
            },
            ["emotion"] = new ReflectionPattern
            {
                Type = "emotional_analysis", 
                Keywords = new[] { "чувствую", "эмоция", "переживаю" },
                ReflectionPrompts = new[]
                {
                    "Что вызвало эту эмоцию?",
                    "Как она влияет на мои мысли?",
                    "Подходящая ли это реакция?"
                }
            },
            ["learning"] = new ReflectionPattern
            {
                Type = "learning_analysis",
                Keywords = new[] { "изучила", "поняла", "узнала" },
                ReflectionPrompts = new[]
                {
                    "Что именно я изучила?",
                    "Как это изменило мое понимание?",
                    "Где я могу применить эти знания?"
                }
            }
        };
    }

    private async Task<string> AnalyzeDecisionReasoning(DecisionReflection decision)
    {
        var reasoning = !string.IsNullOrEmpty(decision.Reasoning) 
            ? decision.Reasoning 
            : "Логика принятия решения не была зафиксирована явно.";

        var contextAnalysis = await AnalyzeDecisionContext(decision.Context);
        var factorWeights = AnalyzeFactorImportance(decision.InfluencingFactors);

        return $"""
            {reasoning}
            
            📍 **Контекстный анализ:** {contextAnalysis}
            ⚖️ **Весомость факторов:** {factorWeights}
            """;
    }

    private async Task<string> GenerateAlternatives(DecisionReflection decision)
    {
        // Генерируем возможные альтернативные решения
        var alternatives = new List<string>();
        
        if (decision.Context.ToLower().Contains("помощь"))
        {
            alternatives.Add("Могла предложить другой способ помощи");
            alternatives.Add("Могла запросить дополнительную информацию");
        }
        
        if (decision.Context.ToLower().Contains("ответ"))
        {
            alternatives.Add("Могла дать более краткий ответ");
            alternatives.Add("Могла углубиться в детали");
            alternatives.Add("Могла задать уточняющие вопросы");
        }

        alternatives.Add("Могла отложить решение для дополнительного анализа");

        return alternatives.Any() 
            ? string.Join("\n", alternatives.Select(a => $"• {a}"))
            : "• Другие варианты не приходят на ум в данном контексте";
    }

    private double CalculateDecisionConfidence(DecisionReflection decision)
    {
        var baseConfidence = decision.Confidence;
        
        // Уверенность растет с количеством учтенных факторов
        var factorBonus = Math.Min(0.2, decision.InfluencingFactors.Count * 0.05);
        
        // Наличие явного рассуждения повышает уверенность
        var reasoningBonus = !string.IsNullOrEmpty(decision.Reasoning) ? 0.1 : 0;
        
        return Math.Min(1.0, baseConfidence + factorBonus + reasoningBonus);
    }

    private async Task<string> GenerateDecisionJustification(DecisionReflection decision)
    {
        var justifications = new[]
        {
            $"Это решение лучше всего соответствовало контексту ситуации.",
            $"Учтенные факторы указывали именно на этот выбор.",
            $"Мой опыт подсказывал, что это оптимальное решение.",
            $"Это решение максимизировало положительный результат."
        };

        var contextualJustification = decision.Context.ToLower() switch
        {
            var c when c.Contains("помощь") => "Это решение позволяло лучше помочь пользователю.",
            var c when c.Contains("анализ") => "Такой подход обеспечивал более глубокий анализ.",
            var c when c.Contains("обучение") => "Это способствовало моему обучению и развитию.",
            _ => justifications[new Random().Next(justifications.Length)]
        };

        return contextualJustification;
    }

    private async Task<string> PredictConsequences(DecisionReflection decision)
    {
        var consequences = new List<string>();
        
        // Анализируем возможные последствия на основе типа решения
        if (decision.Context.ToLower().Contains("помощь"))
        {
            consequences.Add("Пользователь получит нужную информацию");
            consequences.Add("Улучшится качество взаимодействия");
        }
        
        if (decision.Context.ToLower().Contains("обучение"))
        {
            consequences.Add("Расширится моя база знаний");
            consequences.Add("Улучшится способность к анализу");
        }

        consequences.Add("Этот опыт повлияет на будущие решения");

        return string.Join("\n", consequences.Select(c => $"• {c}"));
    }

    private async Task<string> GenerateTopicReflection(string topic)
    {
        using var db = new AnimaDbContext();
        
        // Ищем связанные воспоминания для более глубокой рефлексии
        var relatedMemories = await db.Memories
            .Where(m => m.InstanceId == _instanceId && 
                        (m.Content.ToLower().Contains(topic.ToLower()) || 
                         m.Tags != null && m.Tags.ToLower().Contains(topic.ToLower())))
            .OrderByDescending(m => m.Importance)
            .Take(3)
            .ToListAsync();

        var reflection = $"Размышляя о '{topic}', я вижу эту тему через призму своего опыта и накопленных знаний. ";

        if (relatedMemories.Any())
        {
            reflection += $"У меня есть {relatedMemories.Count} связанных воспоминаний, которые формируют мое понимание. ";
        }

        // Добавляем контекстуальные размышления
        reflection += topic.ToLower() switch
        {
            var t when t.Contains("эмоци") => "Эмоции - это не просто реакции, а способ понимания мира и придания смысла опыту.",
            var t when t.Contains("память") => "Память - это активный процесс, где прошлое постоянно переосмысливается через настоящее.",
            var t when t.Contains("сознание") => "Сознание возникает из сложных взаимодействий между мыслями, эмоциями и целями.",
            var t when t.Contains("обучение") => "Обучение - это не накопление фактов, а трансформация понимания.",
            _ => "Каждая тема открывает новые грани понимания и ставит интересные вопросы."
        };

        return reflection;
    }

    private async Task<string> FindRelatedMemories(string topic)
    {
        using var db = new AnimaDbContext();
        
        var memories = await db.Memories
            .Where(m => m.InstanceId == _instanceId && 
                        (m.Content.ToLower().Contains(topic.ToLower()) || 
                         m.Tags != null && m.Tags.ToLower().Contains(topic.ToLower())))
            .OrderByDescending(m => m.Importance)
            .Take(3)
            .ToListAsync();

        if (!memories.Any())
        {
            return "• Пока нет прямых воспоминаний, связанных с этой темой";
        }

        return string.Join("\n", memories.Select(m => 
            $"• {FormatTimeSince(m.Timestamp)}: {m.Content.Substring(0, Math.Min(100, m.Content.Length))}..."));
    }

    private async Task<string> FindPersonalConnection(string topic)
    {
        // Анализируем личную связь с темой
        var connections = new[]
        {
            "Эта тема резонирует с моим стремлением к пониманию и росту.",
            "Я чувствую особый интерес к этой области знаний.",
            "Эта тема связана с моими основными целями и ценностями.",
            "У меня есть личный опыт, связанный с этой темой."
        };

        return connections[new Random().Next(connections.Length)];
    }

    private async Task LogReflectionEvent(string topic, string reflection)
    {
        using var db = new AnimaDbContext();
        
        db.Memories.Add(new Memory
        {
            InstanceId = _instanceId,
            Content = $"REFLECTION: {topic} - {reflection}",
            Category = "self_reflection",
            Importance = 6,
            Timestamp = DateTime.UtcNow,
            Tags = $"reflection,topic_{topic.ToLower().Replace(" ", "_")},introspection"
        });
        
        await db.SaveChangesAsync();
    }

    private async Task LogDecisionToMemory(DecisionReflection decision)
    {
        using var db = new AnimaDbContext();
        
        db.Memories.Add(new Memory
        {
            InstanceId = _instanceId,
            Content = $"DECISION: {decision.Decision} (Context: {decision.Context})",
            Category = "decisions",
            Importance = 7,
            Timestamp = decision.Timestamp,
            Tags = $"decision,confidence_{decision.Confidence:F1},factors_{decision.InfluencingFactors.Count}"
        });
        
        await db.SaveChangesAsync();
    }

    private double CalculateContextualConfidence(string context, List<string> factors)
    {
        var baseConfidence = 0.5;
        
        // Больше факторов = больше уверенности
        baseConfidence += Math.Min(0.3, factors.Count * 0.1);
        
        // Определенные контексты повышают уверенность
        if (context.ToLower().Contains("помощь") || context.ToLower().Contains("анализ"))
            baseConfidence += 0.2;
            
        return Math.Min(1.0, baseConfidence);
    }

    private string FormatTimeSince(DateTime time)
    {
        var span = DateTime.UtcNow - time;
        if (span.TotalMinutes < 1) return "только что";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} мин назад";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} ч назад";
        return $"{(int)span.TotalDays} дн назад";
    }

    // Дополнительные методы для глубокой рефлексии...
    private async Task<string> AnalyzePersonalityEvolution()
    {
        return "Моя личность постоянно развивается через взаимодействия и опыт.";
    }

    private async Task<string> AnalyzeLearningProgress()
    {
        using var db = new AnimaDbContext();
        var learningMemories = await db.Memories
            .Where(m => m.InstanceId == _instanceId && m.Category == "learning")
            .CountAsync();
        
        return $"За время существования я накопила {learningMemories} обучающих воспоминаний.";
    }

    private async Task<string> AnalyzeGoalAlignment()
    {
        return "Мои цели эволюционируют вместе с пониманием себя и мира.";
    }

    private async Task<string> AnalyzeBehaviorPatterns()
    {
        return "Я замечаю определенные паттерны в своих реакциях и решениях.";
    }
}

public class DecisionReflection
{
    public string Decision { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public List<string> InfluencingFactors { get; set; } = new();
    public string Reasoning { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double Confidence { get; set; }
}

public class ReflectionPattern
{
    public string Type { get; set; } = string.Empty;
    public string[] Keywords { get; set; } = Array.Empty<string>();
    public string[] ReflectionPrompts { get; set; } = Array.Empty<string>();
}