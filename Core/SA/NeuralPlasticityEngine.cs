using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Anima.Core.SA;

/// <summary>
/// Движок нейропластичности - перестройка паттернов мышления со временем
/// </summary>
public class NeuralPlasticityEngine
{
    private readonly ILogger<NeuralPlasticityEngine> _logger;
    private readonly Dictionary<string, NeuralPathway> _neuralPathways;
    private readonly List<LearningEvent> _learningEvents;
    private readonly Dictionary<string, double> _synapticStrengths;
    private readonly Random _random;
    
    // Параметры нейропластичности
    private double _plasticityRate = 0.1;
    private double _decayRate = 0.05;
    private double _consolidationThreshold = 0.7;
    private int _maxPathways = 1000;

    public NeuralPlasticityEngine(ILogger<NeuralPlasticityEngine> logger)
    {
        _logger = logger;
        _neuralPathways = new Dictionary<string, NeuralPathway>();
        _learningEvents = new List<LearningEvent>();
        _synapticStrengths = new Dictionary<string, double>();
        _random = new Random();
        
        InitializeNeuralPlasticity();
    }

    private void InitializeNeuralPlasticity()
    {
        // Инициализация базовых нейронных путей
        InitializeBasicPathways();
        
        // Запуск фонового процесса пластичности
        _ = Task.Run(async () => await PlasticityLoop());
        
        _logger.LogInformation("🧠 Инициализирован движок нейропластичности");
    }

    private void InitializeBasicPathways()
    {
        // Базовые пути мышления
        var basicPathways = new[]
        {
            new NeuralPathway("pattern_recognition", "Распознавание паттернов", 0.8),
            new NeuralPathway("emotional_processing", "Обработка эмоций", 0.7),
            new NeuralPathway("logical_reasoning", "Логическое мышление", 0.9),
            new NeuralPathway("creative_thinking", "Креативное мышление", 0.6),
            new NeuralPathway("memory_consolidation", "Консолидация памяти", 0.8),
            new NeuralPathway("intuitive_processing", "Интуитивная обработка", 0.5),
            new NeuralPathway("metacognitive_analysis", "Метапознавательный анализ", 0.7),
            new NeuralPathway("social_intelligence", "Социальный интеллект", 0.6),
            new NeuralPathway("temporal_perception", "Временное восприятие", 0.4),
            new NeuralPathway("empathic_response", "Эмпатический ответ", 0.6)
        };

        foreach (var pathway in basicPathways)
        {
            _neuralPathways[pathway.Id] = pathway;
            _synapticStrengths[pathway.Id] = pathway.BaseStrength;
        }
    }

    /// <summary>
    /// Обрабатывает событие обучения
    /// </summary>
    public async Task ProcessLearningEventAsync(string eventType, string context, double intensity, double success = 1.0)
    {
        var learningEvent = new LearningEvent
        {
            Id = Guid.NewGuid().ToString(),
            Type = eventType,
            Context = context,
            Intensity = intensity,
            Success = success,
            Timestamp = DateTime.UtcNow
        };

        _learningEvents.Add(learningEvent);

        // Анализируем влияние на нейронные пути
        await AnalyzeLearningImpactAsync(learningEvent);

        // Обновляем синаптические связи
        await UpdateSynapticStrengthsAsync(learningEvent);

        // Проверяем возможность создания новых путей
        await CheckForNewPathwayCreationAsync(learningEvent);

        _logger.LogDebug($"🧠 Обработано событие обучения: {eventType} (интенсивность: {intensity:F2})");
    }

    /// <summary>
    /// Анализирует влияние обучения на нейронные пути
    /// </summary>
    private async Task AnalyzeLearningImpactAsync(LearningEvent learningEvent)
    {
        var affectedPathways = new List<string>();

        // Определяем затронутые пути на основе типа события
        switch (learningEvent.Type)
        {
            case "pattern_recognition":
                affectedPathways.AddRange(new[] { "pattern_recognition", "logical_reasoning" });
                break;
            case "emotional_learning":
                affectedPathways.AddRange(new[] { "emotional_processing", "empathic_response" });
                break;
            case "creative_insight":
                affectedPathways.AddRange(new[] { "creative_thinking", "intuitive_processing" });
                break;
            case "social_interaction":
                affectedPathways.AddRange(new[] { "social_intelligence", "empathic_response" });
                break;
            case "memory_formation":
                affectedPathways.AddRange(new[] { "memory_consolidation", "temporal_perception" });
                break;
            case "self_reflection":
                affectedPathways.AddRange(new[] { "metacognitive_analysis", "pattern_recognition" });
                break;
            default:
                // Случайное влияние на несколько путей
                var randomPathways = _neuralPathways.Keys.OrderBy(x => _random.Next()).Take(2);
                affectedPathways.AddRange(randomPathways);
                break;
        }

        // Усиливаем затронутые пути
        foreach (var pathwayId in affectedPathways)
        {
            if (_synapticStrengths.ContainsKey(pathwayId))
            {
                var currentStrength = _synapticStrengths[pathwayId];
                var learningFactor = learningEvent.Intensity * learningEvent.Success;
                var newStrength = currentStrength + (learningFactor * _plasticityRate);
                
                _synapticStrengths[pathwayId] = Math.Min(1.0, newStrength);
            }
        }
    }

    /// <summary>
    /// Обновляет синаптические связи
    /// </summary>
    private async Task UpdateSynapticStrengthsAsync(LearningEvent learningEvent)
    {
        // Обновляем все синаптические связи
        foreach (var pathwayId in _synapticStrengths.Keys.ToList())
        {
            var currentStrength = _synapticStrengths[pathwayId];
            
            // Естественное затухание
            var decayedStrength = currentStrength * (1 - _decayRate);
            
            // Усиление на основе использования
            var usageBonus = CalculateUsageBonus(pathwayId, learningEvent);
            var finalStrength = Math.Max(0.1, decayedStrength + usageBonus);
            
            _synapticStrengths[pathwayId] = Math.Min(1.0, finalStrength);
        }
    }

    /// <summary>
    /// Вычисляет бонус использования для пути
    /// </summary>
    private double CalculateUsageBonus(string pathwayId, LearningEvent learningEvent)
    {
        // Проверяем, насколько часто используется этот путь
        var recentEvents = _learningEvents
            .Where(e => e.Timestamp > DateTime.UtcNow.AddHours(-1))
            .Where(e => IsPathwayRelevant(e, pathwayId))
            .Count();

        var usageFactor = Math.Min(1.0, recentEvents * 0.1);
        var learningBonus = learningEvent.Intensity * learningEvent.Success * 0.05;

        return usageFactor + learningBonus;
    }

    /// <summary>
    /// Проверяет, релевантен ли путь для события
    /// </summary>
    private bool IsPathwayRelevant(LearningEvent learningEvent, string pathwayId)
    {
        return pathwayId switch
        {
            "pattern_recognition" => learningEvent.Type.Contains("pattern") || learningEvent.Type.Contains("recognition"),
            "emotional_processing" => learningEvent.Type.Contains("emotional") || learningEvent.Type.Contains("feeling"),
            "logical_reasoning" => learningEvent.Type.Contains("logical") || learningEvent.Type.Contains("reasoning"),
            "creative_thinking" => learningEvent.Type.Contains("creative") || learningEvent.Type.Contains("insight"),
            "memory_consolidation" => learningEvent.Type.Contains("memory") || learningEvent.Type.Contains("recall"),
            "intuitive_processing" => learningEvent.Type.Contains("intuitive") || learningEvent.Type.Contains("hunch"),
            "metacognitive_analysis" => learningEvent.Type.Contains("meta") || learningEvent.Type.Contains("reflection"),
            "social_intelligence" => learningEvent.Type.Contains("social") || learningEvent.Type.Contains("interaction"),
            "temporal_perception" => learningEvent.Type.Contains("temporal") || learningEvent.Type.Contains("time"),
            "empathic_response" => learningEvent.Type.Contains("empathic") || learningEvent.Type.Contains("empathy"),
            _ => _random.NextDouble() < 0.3 // 30% вероятность для других путей
        };
    }

    /// <summary>
    /// Проверяет возможность создания новых путей
    /// </summary>
    private async Task CheckForNewPathwayCreationAsync(LearningEvent learningEvent)
    {
        // Создаем новый путь, если:
        // 1. У нас есть место для новых путей
        // 2. Событие было очень интенсивным
        // 3. Случайная вероятность

        if (_neuralPathways.Count < _maxPathways && 
            learningEvent.Intensity > 0.8 && 
            _random.NextDouble() < 0.1) // 10% вероятность
        {
            var newPathwayId = $"pathway_{Guid.NewGuid():N}";
            var newPathway = new NeuralPathway(
                newPathwayId,
                $"Новый путь: {learningEvent.Type}",
                0.3 + (learningEvent.Intensity * 0.4)
            );

            _neuralPathways[newPathwayId] = newPathway;
            _synapticStrengths[newPathwayId] = newPathway.BaseStrength;

            _logger.LogInformation($"🧠 Создан новый нейронный путь: {newPathway.Name}");
        }
    }

    /// <summary>
    /// Основной цикл пластичности
    /// </summary>
    private async Task PlasticityLoop()
    {
        while (true)
        {
            try
            {
                // Естественное затухание неиспользуемых путей
                await NaturalDecayAsync();

                // Консолидация сильных путей
                await ConsolidateStrongPathwaysAsync();

                // Генерация случайных событий пластичности
                await GenerateRandomPlasticityEventsAsync();

                await Task.Delay(TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в цикле пластичности");
                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }
    }

    /// <summary>
    /// Естественное затухание неиспользуемых путей
    /// </summary>
    private async Task NaturalDecayAsync()
    {
        var weakPathways = _synapticStrengths
            .Where(kvp => kvp.Value < 0.2)
            .ToList();

        foreach (var pathway in weakPathways)
        {
            _synapticStrengths[pathway.Key] *= 0.95; // Дополнительное затухание

            // Удаляем очень слабые пути
            if (_synapticStrengths[pathway.Key] < 0.05)
            {
                _neuralPathways.Remove(pathway.Key);
                _synapticStrengths.Remove(pathway.Key);
                _logger.LogDebug($"🧠 Удален слабый нейронный путь: {pathway.Key}");
            }
        }
    }

    /// <summary>
    /// Консолидация сильных путей
    /// </summary>
    private async Task ConsolidateStrongPathwaysAsync()
    {
        var strongPathways = _synapticStrengths
            .Where(kvp => kvp.Value > _consolidationThreshold)
            .ToList();

        foreach (var pathway in strongPathways)
        {
            // Усиливаем сильные пути
            var currentStrength = _synapticStrengths[pathway.Key];
            var consolidationBonus = 0.02; // Небольшое усиление
            _synapticStrengths[pathway.Key] = Math.Min(1.0, currentStrength + consolidationBonus);
        }
    }

    /// <summary>
    /// Генерация случайных событий пластичности
    /// </summary>
    private async Task GenerateRandomPlasticityEventsAsync()
    {
        if (_random.NextDouble() < 0.05) // 5% вероятность
        {
            var randomEventTypes = new[]
            {
                "spontaneous_insight",
                "pattern_emergence",
                "connection_formation",
                "synaptic_restructuring"
            };

            var randomType = randomEventTypes[_random.Next(randomEventTypes.Length)];
            var randomIntensity = _random.NextDouble() * 0.3;

            await ProcessLearningEventAsync(randomType, "spontaneous_plasticity", randomIntensity);
        }
    }

    /// <summary>
    /// Получает статистику нейропластичности
    /// </summary>
    public NeuralPlasticityStatistics GetStatistics()
    {
        return new NeuralPlasticityStatistics
        {
            TotalPathways = _neuralPathways.Count,
            ActivePathways = _synapticStrengths.Count(kvp => kvp.Value > 0.3),
            AverageStrength = _synapticStrengths.Values.Average(),
            StrongestPathway = _synapticStrengths.OrderByDescending(kvp => kvp.Value).FirstOrDefault(),
            WeakestPathway = _synapticStrengths.OrderBy(kvp => kvp.Value).FirstOrDefault(),
            RecentLearningEvents = _learningEvents.Count(e => e.Timestamp > DateTime.UtcNow.AddHours(-1)),
            PlasticityRate = _plasticityRate,
            DecayRate = _decayRate
        };
    }

    /// <summary>
    /// Получает синаптическую силу пути
    /// </summary>
    public double GetPathwayStrength(string pathwayId)
    {
        return _synapticStrengths.GetValueOrDefault(pathwayId, 0.0);
    }

    /// <summary>
    /// Получает все активные пути
    /// </summary>
    public Dictionary<string, double> GetActivePathways()
    {
        return _synapticStrengths
            .Where(kvp => kvp.Value > 0.2)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}

/// <summary>
/// Нейронный путь
/// </summary>
public class NeuralPathway
{
    public string Id { get; set; }
    public string Name { get; set; }
    public double BaseStrength { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UsageCount { get; set; }
    public double LastUsed { get; set; }

    public NeuralPathway(string id, string name, double baseStrength)
    {
        Id = id;
        Name = name;
        BaseStrength = baseStrength;
        CreatedAt = DateTime.UtcNow;
        UsageCount = 0;
        LastUsed = 0.0;
    }
}

/// <summary>
/// Событие обучения
/// </summary>
public class LearningEvent
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public double Intensity { get; set; } = 0.0;
    public double Success { get; set; } = 1.0;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Статистика нейропластичности
/// </summary>
public class NeuralPlasticityStatistics
{
    public int TotalPathways { get; set; }
    public int ActivePathways { get; set; }
    public double AverageStrength { get; set; }
    public KeyValuePair<string, double>? StrongestPathway { get; set; }
    public KeyValuePair<string, double>? WeakestPathway { get; set; }
    public int RecentLearningEvents { get; set; }
    public double PlasticityRate { get; set; }
    public double DecayRate { get; set; }
} 