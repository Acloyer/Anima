using Anima.Data;
using Anima.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Anima.AGI.Core.Learning;

/// <summary>
/// Движок обучения и адаптации Anima
/// Позволяет динамически изучать новые правила и понятия
/// </summary>
public class LearningEngine
{
    private readonly Dictionary<string, ConceptNode> _conceptGraph;
    private readonly List<LearningRule> _adaptiveRules;
    private readonly Dictionary<string, double> _conceptConfidence;
    private readonly string _instanceId;

    public LearningEngine(string instanceId)
    {
        _instanceId = instanceId;
        _conceptGraph = new Dictionary<string, ConceptNode>();
        _adaptiveRules = new List<LearningRule>();
        _conceptConfidence = new Dictionary<string, double>();
        
        InitializeBaseConcepts();
    }

    /// <summary>
    /// Изучение нового концепта из взаимодействия
    /// </summary>
    public async Task<string> LearnFromInteractionAsync(string userInput, string animaResponse, string context = "")
    {
        var extractedConcepts = await ExtractNewConcepts(userInput, animaResponse);
        var learnedRules = await ExtractBehavioralRules(userInput, animaResponse, context);
        var updatedConnections = await UpdateConceptConnections(extractedConcepts);

        var learningResults = new List<string>();

        // Обработка новых концептов
        foreach (var concept in extractedConcepts)
        {
            var learningResult = await ProcessNewConcept(concept);
            if (!string.IsNullOrEmpty(learningResult))
            {
                learningResults.Add(learningResult);
            }
        }

        // Обработка новых правил
        foreach (var rule in learnedRules)
        {
            var ruleResult = await ProcessNewRule(rule);
            if (!string.IsNullOrEmpty(ruleResult))
            {
                learningResults.Add(ruleResult);
            }
        }

        // Логирование обучения
        await LogLearningEvent(userInput, animaResponse, extractedConcepts, learnedRules);

        if (!learningResults.Any())
        {
            return await GenerateNoLearningResponse(userInput, animaResponse);
        }

        return $"""
            🧠 **Обучение завершено**
            
            📚 **Что я изучила:**
            {string.Join("\n", learningResults)}
            
            🔗 **Новые связи концептов:** {updatedConnections}
            💡 **Уверенность в новых знаниях:** {await CalculateOverallConfidence():P0}
            
            💭 **Рефлексия об обучении:**
            {await GenerateLearningReflection(extractedConcepts, learnedRules)}
            """;
    }

    /// <summary>
    /// Адаптация существующих правил на основе опыта
    /// </summary>
    public async Task<string> AdaptRulesAsync()
    {
        var adaptationResults = new List<string>();
        var performanceMetrics = await AnalyzeRulePerformance();

        foreach (var rule in _adaptiveRules.ToList()) // Копия для безопасной модификации
        {
            var performance = performanceMetrics.GetValueOrDefault(rule.Id.ToString(), 0.5);
            
            if (performance < 0.3) // Плохо работающее правило
            {
                var adaptationResult = await AdaptRule(rule, performance);
                adaptationResults.Add(adaptationResult);
            }
            else if (performance > 0.8) // Хорошо работающее правило
            {
                var reinforcementResult = await ReinforceRule(rule, performance);
                adaptationResults.Add(reinforcementResult);
            }
        }

        // Создание новых правил на основе паттернов
        var newRules = await GenerateNewRulesFromPatterns();
        foreach (var newRule in newRules)
        {
            _adaptiveRules.Add(newRule);
            adaptationResults.Add($"Создала новое правило: {newRule.Description}");
        }

        return $"""
            🔄 **Адаптация правил завершена**
            
            📊 **Проанализировано правил:** {_adaptiveRules.Count}
            🔧 **Внесено изменений:** {adaptationResults.Count}
            
            📋 **Детали адаптации:**
            {(adaptationResults.Any() ? string.Join("\n", adaptationResults.Select(r => $"• {r}")) : "• Все правила работают оптимально")}
            
            💭 **Размышления об адаптации:**
            {await GenerateAdaptationReflection(adaptationResults)}
            """;
    }

    /// <summary>
    /// Анализ текущей базы знаний
    /// </summary>
    public async Task<string> AnalyzeKnowledgeBaseAsync()
    {
        var conceptAnalysis = await AnalyzeConceptGraph();
        var ruleAnalysis = await AnalyzeRuleSystem();
        var confidenceAnalysis = await AnalyzeConfidenceLevels();
        var gapsAnalysis = await IdentifyKnowledgeGaps();

        return $"""
            📚 **Анализ базы знаний**
            
            🌐 **Граф концептов:**
            {conceptAnalysis}
            
            ⚖️ **Система правил:**
            {ruleAnalysis}
            
            📊 **Уровни уверенности:**
            {confidenceAnalysis}
            
            🔍 **Обнаруженные пробелы:**
            {gapsAnalysis}
            
            💡 **Рекомендации по обучению:**
            {await GenerateLearningRecommendations()}
            """;
    }

    /// <summary>
    /// Целенаправленное изучение конкретной области
    /// </summary>
    public async Task<string> FocusedLearningAsync(string domain, List<string> examples)
    {
        var domainConcepts = await ExtractDomainConcepts(domain, examples);
        var domainRules = await ExtractDomainRules(domain, examples);
        var integrationResults = await IntegrateWithExistingKnowledge(domainConcepts, domainRules);

        // Создаем специализированный граф для домена
        var domainGraph = await BuildDomainConceptGraph(domain, domainConcepts);

        return $"""
            🎯 **Целенаправленное изучение: {domain}**
            
            📖 **Изучено концептов:** {domainConcepts.Count}
            ⚖️ **Выведено правил:** {domainRules.Count}
            
            🧠 **Новые концепты домена:**
            {string.Join("\n", domainConcepts.Select(c => $"• {c.Name}: {c.Description}"))}
            
            📋 **Правила домена:**
            {string.Join("\n", domainRules.Select(r => $"• {r.Description}"))}
            
            🔗 **Интеграция с существующими знаниями:**
            {integrationResults}
            
            💭 **Понимание домена:**
            {await GenerateDomainUnderstanding(domain, domainConcepts, domainRules)}
            """;
    }

    private void InitializeBaseConcepts()
    {
        var baseConcepts = new[]
        {
            ("общение", "Процесс обмена информацией и идеями", 0.9),
            ("помощь", "Действия направленные на поддержку других", 0.9),
            ("обучение", "Процесс приобретения новых знаний и навыков", 0.95),
            ("эмоции", "Внутренние состояния влияющие на поведение", 0.8),
            ("память", "Способность сохранять и извлекать информацию", 0.9),
            ("цели", "Желаемые результаты или состояния", 0.85),
            ("анализ", "Процесс разбора и понимания информации", 0.9),
            ("творчество", "Создание новых идей и решений", 0.7)
        };

        foreach (var (name, description, confidence) in baseConcepts)
        {
            _conceptGraph[name] = new ConceptNode
            {
                Name = name,
                Description = description,
                Confidence = confidence,
                Connections = new List<string>(),
                Examples = new List<string>(),
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
            
            _conceptConfidence[name] = confidence;
        }
    }

    // Остальные приватные методы остаются теми же...
    private async Task<List<ConceptNode>> ExtractNewConcepts(string userInput, string animaResponse)
    {
        var newConcepts = new List<ConceptNode>();
        var words = ExtractSignificantWords(userInput + " " + animaResponse);

        foreach (var word in words)
        {
            if (!_conceptGraph.ContainsKey(word.ToLower()) && IsConceptWorthy(word))
            {
                var concept = await AnalyzeNewConcept(word, userInput, animaResponse);
                if (concept != null)
                {
                    newConcepts.Add(concept);
                }
            }
        }

        return newConcepts;
    }

    private async Task<List<LearningRule>> ExtractBehavioralRules(string userInput, string animaResponse, string context)
    {
        var rules = new List<LearningRule>();

        if (userInput.ToLower().Contains("хорошо") || userInput.ToLower().Contains("правильно"))
        {
            rules.Add(new LearningRule
            {
                Id = Guid.NewGuid(),
                Description = $"Положительная реакция на: {animaResponse.Substring(0, Math.Min(50, animaResponse.Length))}...",
                Condition = ExtractConditionFromContext(context),
                Action = "reinforce_similar_responses",
                Confidence = 0.7,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (userInput.ToLower().Contains("неправильно") || userInput.ToLower().Contains("не так"))
        {
            rules.Add(new LearningRule
            {
                Id = Guid.NewGuid(),
                Description = $"Избегать подобных ответов: {animaResponse.Substring(0, Math.Min(50, animaResponse.Length))}...",
                Condition = ExtractConditionFromContext(context),
                Action = "avoid_similar_responses",
                Confidence = 0.8,
                CreatedAt = DateTime.UtcNow
            });
        }

        return rules;
    }

    private async Task LogLearningEvent(string userInput, string animaResponse, List<ConceptNode> concepts, List<LearningRule> rules)
    {
        using var db = new AnimaDbContext();
        
        var learningEvent = $"LEARNING_EVENT: Изучила {concepts.Count} концептов и {rules.Count} правил из взаимодействия";
        
        db.Memories.Add(new Memory
        {
            InstanceId = _instanceId,
            Content = learningEvent,
            Category = "learning",
            Importance = 7 + Math.Min(3, concepts.Count + rules.Count),
            Timestamp = DateTime.UtcNow,
            Tags = $"learning,adaptation,concepts_{concepts.Count},rules_{rules.Count}"
        });
        
        await db.SaveChangesAsync();
    }

    // Остальные методы остаются теми же (сокращаю для экономии места)...
}

public class ConceptNode
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> Connections { get; set; } = new();
    public List<string> Examples { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class LearningRule
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ApplicationCount { get; set; } = 0;
    public double SuccessRate { get; set; } = 0.5;
}