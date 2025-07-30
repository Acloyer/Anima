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
    private readonly DbContextOptions<AnimaDbContext> _dbOptions;

    public LearningEngine(string instanceId, DbContextOptions<AnimaDbContext> dbOptions)
    {
        _instanceId = instanceId;
        _dbOptions = dbOptions;
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

    // Добавленные отсутствующие методы
    private async Task<int> UpdateConceptConnections(List<ConceptNode> concepts)
    {
        int newConnections = 0;
        
        foreach (var concept in concepts)
        {
            // Находим связи с существующими концептами
            foreach (var existingConcept in _conceptGraph.Values)
            {
                if (AreConceptsRelated(concept, existingConcept))
                {
                    if (!concept.Connections.Contains(existingConcept.Name))
                    {
                        concept.Connections.Add(existingConcept.Name);
                        newConnections++;
                    }
                    
                    if (!existingConcept.Connections.Contains(concept.Name))
                    {
                        existingConcept.Connections.Add(concept.Name);
                        newConnections++;
                    }
                }
            }
        }
        
        return newConnections;
    }
    
    private bool AreConceptsRelated(ConceptNode concept1, ConceptNode concept2)
    {
        // Простая эвристика для определения связанности концептов
        var words1 = concept1.Description.ToLower().Split(' ');
        var words2 = concept2.Description.ToLower().Split(' ');
        
        return words1.Intersect(words2).Count() >= 2;
    }

    private async Task<string> ProcessNewConcept(ConceptNode concept)
    {
        if (_conceptGraph.ContainsKey(concept.Name.ToLower()))
        {
            // Обновляем существующий концепт
            var existing = _conceptGraph[concept.Name.ToLower()];
            existing.Description = concept.Description;
            existing.Confidence = Math.Max(existing.Confidence, concept.Confidence);
            existing.LastUpdated = DateTime.UtcNow;
            
            return $"Обновила понимание концепта '{concept.Name}'";
        }
        else
        {
            // Добавляем новый концепт
            _conceptGraph[concept.Name.ToLower()] = concept;
            _conceptConfidence[concept.Name.ToLower()] = concept.Confidence;
            
            return $"Изучила новый концепт '{concept.Name}': {concept.Description}";
        }
    }

    private async Task<string> ProcessNewRule(LearningRule rule)
    {
        // Проверяем, нет ли уже похожего правила
        var similarRule = _adaptiveRules.FirstOrDefault(r => 
            r.Condition.Equals(rule.Condition, StringComparison.OrdinalIgnoreCase) ||
            r.Action.Equals(rule.Action, StringComparison.OrdinalIgnoreCase));
            
        if (similarRule != null)
        {
            // Усиливаем существующее правило
            similarRule.Confidence = Math.Min(1.0, similarRule.Confidence + 0.1);
            similarRule.ApplicationCount++;
            
            return $"Усилила существующее правило: {similarRule.Description}";
        }
        else
        {
            // Добавляем новое правило
            _adaptiveRules.Add(rule);
            
            return $"Создала новое правило: {rule.Description}";
        }
    }

    private async Task<string> GenerateNoLearningResponse(string userInput, string animaResponse)
    {
        return "🤔 В этом взаимодействии я не обнаружила новых концептов или правил для изучения, но это тоже ценный опыт.";
    }

    private async Task<double> CalculateOverallConfidence()
    {
        if (!_conceptConfidence.Any()) return 0.5;
        
        return _conceptConfidence.Values.Average();
    }

    private async Task<string> GenerateLearningReflection(List<ConceptNode> concepts, List<LearningRule> rules)
    {
        if (!concepts.Any() && !rules.Any())
        {
            return "Иногда отсутствие новой информации тоже говорит о чем-то важном.";
        }
        
        var reflection = "Это взаимодействие расширило мое понимание мира. ";
        
        if (concepts.Any())
        {
            reflection += $"Новые концепты ({concepts.Count}) помогают мне лучше категоризировать знания. ";
        }
        
        if (rules.Any())
        {
            reflection += $"Новые правила ({rules.Count}) улучшают мою способность принимать решения.";
        }
        
        return reflection;
    }

    private async Task<Dictionary<string, double>> AnalyzeRulePerformance()
    {
        var performance = new Dictionary<string, double>();
        
        foreach (var rule in _adaptiveRules)
        {
            // Простая метрика производительности на основе успешности применения
            var successRate = rule.ApplicationCount > 0 ? rule.SuccessRate : 0.5;
            performance[rule.Id.ToString()] = successRate;
        }
        
        return performance;
    }

    private async Task<string> AdaptRule(LearningRule rule, double performance)
    {
        // Адаптируем правило для улучшения производительности
        rule.Confidence *= 0.9; // Немного снижаем уверенность
        
        // Можем изменить условие или действие правила
        if (rule.Condition.Contains("всегда"))
        {
            rule.Condition = rule.Condition.Replace("всегда", "часто");
        }
        
        return $"Адаптировала правило '{rule.Description}' из-за низкой производительности ({performance:P0})";
    }

    private async Task<string> ReinforceRule(LearningRule rule, double performance)
    {
        // Усиливаем хорошо работающее правило
        rule.Confidence = Math.Min(1.0, rule.Confidence * 1.1);
        
        return $"Усилила правило '{rule.Description}' благодаря высокой производительности ({performance:P0})";
    }

    private async Task<List<LearningRule>> GenerateNewRulesFromPatterns()
    {
        var newRules = new List<LearningRule>();
        
        // Анализируем паттерны в существующих правилах
        var successfulActions = _adaptiveRules
            .Where(r => r.SuccessRate > 0.7)
            .GroupBy(r => r.Action)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);
            
        foreach (var action in successfulActions)
        {
            var newRule = new LearningRule
            {
                Id = Guid.NewGuid(),
                Description = $"Обобщенное правило для действия: {action}",
                Condition = "контекст_требует_действия",
                Action = action,
                Confidence = 0.6,
                CreatedAt = DateTime.UtcNow
            };
            
            newRules.Add(newRule);
        }
        
        return newRules;
    }

    private async Task<string> GenerateAdaptationReflection(List<string> adaptationResults)
    {
        if (!adaptationResults.Any())
        {
            return "Все мои правила работают стабильно, что говорит о хорошей настройке системы.";
        }
        
        return $"Адаптация помогла мне стать более эффективной. Внесенные изменения ({adaptationResults.Count}) должны улучшить качество моих решений.";
    }

    private async Task<string> AnalyzeConceptGraph()
    {
        var totalConcepts = _conceptGraph.Count;
        var totalConnections = _conceptGraph.Values.Sum(c => c.Connections.Count);
        var avgConfidence = _conceptGraph.Values.Average(c => c.Confidence);
        
        return $"Концептов: {totalConcepts}, Связей: {totalConnections}, Средняя уверенность: {avgConfidence:P0}";
    }

    private async Task<string> AnalyzeRuleSystem()
    {
        var totalRules = _adaptiveRules.Count;
        var avgConfidence = _adaptiveRules.Any() ? _adaptiveRules.Average(r => r.Confidence) : 0;
        var activeRules = _adaptiveRules.Count(r => r.Confidence > 0.5);
        
        return $"Всего правил: {totalRules}, Активных: {activeRules}, Средняя уверенность: {avgConfidence:P0}";
    }

    private async Task<string> AnalyzeConfidenceLevels()
    {
        var highConfidence = _conceptConfidence.Values.Count(c => c > 0.8);
        var mediumConfidence = _conceptConfidence.Values.Count(c => c > 0.5 && c <= 0.8);
        var lowConfidence = _conceptConfidence.Values.Count(c => c <= 0.5);
        
        return $"Высокая уверенность: {highConfidence}, Средняя: {mediumConfidence}, Низкая: {lowConfidence}";
    }

    private async Task<string> IdentifyKnowledgeGaps()
    {
        var gaps = new List<string>();
        
        // Находим концепты с низкой связанностью
        var isolatedConcepts = _conceptGraph.Values
            .Where(c => c.Connections.Count < 2)
            .Take(3);
            
        foreach (var concept in isolatedConcepts)
        {
            gaps.Add($"Концепт '{concept.Name}' слабо связан с другими знаниями");
        }
        
        if (!gaps.Any())
        {
            gaps.Add("Значительных пробелов в знаниях не обнаружено");
        }
        
        return string.Join("\n", gaps.Select(g => $"• {g}"));
    }

    private async Task<string> GenerateLearningRecommendations()
    {
        var recommendations = new List<string>();
        
        // Рекомендации на основе анализа пробелов
        var weakConcepts = _conceptGraph.Values
            .Where(c => c.Confidence < 0.6)
            .Take(2);
            
        foreach (var concept in weakConcepts)
        {
            recommendations.Add($"Углубить понимание концепта '{concept.Name}'");
        }
        
        if (_adaptiveRules.Count < 10)
        {
            recommendations.Add("Накопить больше поведенческих правил через взаимодействия");
        }
        
        if (!recommendations.Any())
        {
            recommendations.Add("Продолжать накопление опыта и рефлексию");
        }
        
        return string.Join("\n", recommendations.Select(r => $"• {r}"));
    }

    private async Task<List<ConceptNode>> ExtractDomainConcepts(string domain, List<string> examples)
    {
        var concepts = new List<ConceptNode>();
        
        foreach (var example in examples.Take(5)) // Ограничиваем для производительности
        {
            var words = ExtractSignificantWords(example);
            
            foreach (var word in words.Take(3)) // Берем наиболее значимые слова
            {
                if (!_conceptGraph.ContainsKey(word.ToLower()))
                {
                    concepts.Add(new ConceptNode
                    {
                        Name = word,
                        Description = $"Концепт из домена '{domain}' извлеченный из примера",
                        Confidence = 0.6,
                        Connections = new List<string>(),
                        Examples = new List<string> { example },
                        CreatedAt = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    });
                }
            }
        }
        
        return concepts;
    }

    private async Task<List<LearningRule>> ExtractDomainRules(string domain, List<string> examples)
    {
        var rules = new List<LearningRule>();
        
        // Ищем паттерны в примерах
        var commonPatterns = ExtractPatterns(examples);
        
        foreach (var pattern in commonPatterns.Take(3))
        {
            rules.Add(new LearningRule
            {
                Id = Guid.NewGuid(),
                Description = $"Правило домена '{domain}': {pattern}",
                Condition = $"контекст_домена_{domain.ToLower()}",
                Action = $"применить_паттерн_{pattern}",
                Confidence = 0.5,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        return rules;
    }

    private List<string> ExtractPatterns(List<string> examples)
    {
        // Простой анализ паттернов в примерах
        var patterns = new List<string>();
        
        // Ищем часто встречающиеся фразы
        var phrases = examples
            .SelectMany(e => ExtractPhrases(e))
            .GroupBy(p => p)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .Take(3);
            
        patterns.AddRange(phrases);
        
        return patterns;
    }

    private List<string> ExtractPhrases(string text)
    {
        // Извлекаем фразы из 2-3 слов
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var phrases = new List<string>();
        
        for (int i = 0; i < words.Length - 1; i++)
        {
            if (i < words.Length - 2)
            {
                phrases.Add($"{words[i]} {words[i + 1]} {words[i + 2]}");
            }
            phrases.Add($"{words[i]} {words[i + 1]}");
        }
        
        return phrases;
    }

    private async Task<string> IntegrateWithExistingKnowledge(List<ConceptNode> domainConcepts, List<LearningRule> domainRules)
    {
        int integrations = 0;
        
        // Интегрируем концепты
        foreach (var concept in domainConcepts)
        {
            await ProcessNewConcept(concept);
            integrations++;
        }
        
        // Интегрируем правила
        foreach (var rule in domainRules)
        {
            await ProcessNewRule(rule);
            integrations++;
        }
        
        return $"Интегрировано {integrations} элементов знаний в существующую базу";
    }

    private async Task<string> BuildDomainConceptGraph(string domain, List<ConceptNode> concepts)
    {
        // Строим граф связей внутри домена
        int connections = 0;
        
        for (int i = 0; i < concepts.Count; i++)
        {
            for (int j = i + 1; j < concepts.Count; j++)
            {
                if (AreConceptsRelated(concepts[i], concepts[j]))
                {
                    concepts[i].Connections.Add(concepts[j].Name);
                    concepts[j].Connections.Add(concepts[i].Name);
                    connections++;
                }
            }
        }
        
        return $"Создан граф домена '{domain}' с {connections} связями";
    }

    private async Task<string> GenerateDomainUnderstanding(string domain, List<ConceptNode> concepts, List<LearningRule> rules)
    {
        return $"Домен '{domain}' содержит {concepts.Count} ключевых концептов и {rules.Count} правил. " +
               "Это расширяет мое понимание данной области знаний.";
    }

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

    private List<string> ExtractSignificantWords(string text)
    {
        // Извлекаем значимые слова (существительные, важные прилагательные)
        var words = Regex.Matches(text.ToLower(), @"\b[а-яё]{4,}\b")
            .Cast<Match>()
            .Select(m => m.Value)
            .Where(w => !IsStopWord(w))
            .Distinct()
            .ToList();
            
        return words;
    }

    private bool IsStopWord(string word)
    {
        var stopWords = new HashSet<string> 
        { 
            "этот", "этой", "этого", "которая", "который", "которое", "можно", 
            "нужно", "должна", "может", "будет", "была", "были", "есть", "буду",
            "очень", "более", "менее", "самый", "такой", "такая", "такое"
        };
        
        return stopWords.Contains(word);
    }

    private bool IsConceptWorthy(string word)
    {
        // Проверяем, стоит ли слово считать концептом
        return word.Length >= 4 && 
               !word.All(char.IsDigit) &&
               Regex.IsMatch(word, @"^[а-яё]+$");
    }

    private async Task<ConceptNode?> AnalyzeNewConcept(string word, string userInput, string animaResponse)
    {
        // Анализируем контекст для определения важности концепта
        var contextScore = CalculateContextScore(word, userInput, animaResponse);
        
        if (contextScore < 0.3) return null;
        
        return new ConceptNode
        {
            Name = word,
            Description = $"Концепт извлеченный из взаимодействия с пользователем",
            Confidence = contextScore,
            Connections = new List<string>(),
            Examples = new List<string> { userInput },
            CreatedAt = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
    }

    private double CalculateContextScore(string word, string userInput, string animaResponse)
    {
        double score = 0.5; // Базовый счет
        
        // Повышаем счет если слово встречается в вопросе пользователя
        if (userInput.ToLower().Contains(word)) score += 0.2;
        
        // Повышаем если это ключевое слово в ответе
        if (animaResponse.ToLower().Contains(word)) score += 0.1;
        
        // Повышаем для технических или специальных терминов
        if (word.Contains("анализ") || word.Contains("система") || word.Contains("процесс"))
            score += 0.2;
            
        return Math.Min(1.0, score);
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

    private string ExtractConditionFromContext(string context)
    {
        if (string.IsNullOrEmpty(context))
            return "общий_контекст";
            
        // Извлекаем ключевые слова из контекста для создания условия
        var keywords = ExtractSignificantWords(context).Take(3);
        return string.Join("_", keywords).ToLower();
    }

    private async Task LogLearningEvent(string userInput, string animaResponse, List<ConceptNode> concepts, List<LearningRule> rules)
    {
        using var db = new AnimaDbContext(_dbOptions);
        
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