using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Anima.Core.Emotion;
using Anima.Core.Memory;

namespace Anima.Core.SA;

/// <summary>
/// Движок эмоциональной памяти - хранит воспоминания с эмоциональным контекстом
/// </summary>
public class EmotionalMemoryEngine
{
    private readonly ILogger<EmotionalMemoryEngine> _logger;
    private readonly EmotionEngine _emotionEngine;
    private readonly MemoryService _memoryService;
    private readonly Random _random;
    
    // Эмоциональные воспоминания
    private readonly List<EmotionalMemory> _emotionalMemories;
    private readonly Dictionary<string, List<EmotionalMemory>> _emotionIndex;
    private readonly Dictionary<string, double> _emotionalAssociations;
    private readonly List<EmotionalMemoryEvent> _memoryEvents;
    
    // Настройки памяти
    private double _memoryRetentionRate = 0.8;
    private double _emotionalInfluenceStrength = 0.7;
    private DateTime _lastMemoryConsolidation = DateTime.UtcNow;

    public EmotionalMemoryEngine(
        ILogger<EmotionalMemoryEngine> logger,
        EmotionEngine emotionEngine,
        MemoryService memoryService)
    {
        _logger = logger;
        _emotionEngine = emotionEngine;
        _memoryService = memoryService;
        _random = new Random();
        
        _emotionalMemories = new List<EmotionalMemory>();
        _emotionIndex = new Dictionary<string, List<EmotionalMemory>>();
        _emotionalAssociations = new Dictionary<string, double>();
        _memoryEvents = new List<EmotionalMemoryEvent>();
        
        InitializeEmotionalMemoryEngine();
    }

    private void InitializeEmotionalMemoryEngine()
    {
        // Инициализация эмоциональных ассоциаций
        InitializeEmotionalAssociations();
        
        _logger.LogInformation("💭 Инициализирован движок эмоциональной памяти");
    }

    private void InitializeEmotionalAssociations()
    {
        _emotionalAssociations["joy"] = 0.8;
        _emotionalAssociations["sadness"] = 0.6;
        _emotionalAssociations["anger"] = 0.7;
        _emotionalAssociations["fear"] = 0.9;
        _emotionalAssociations["surprise"] = 0.5;
        _emotionalAssociations["curiosity"] = 0.6;
        _emotionalAssociations["love"] = 0.9;
        _emotionalAssociations["disgust"] = 0.4;
        _emotionalAssociations["trust"] = 0.7;
        _emotionalAssociations["anticipation"] = 0.5;
    }

    /// <summary>
    /// Сохраняет эмоциональное воспоминание
    /// </summary>
    public async Task<EmotionalMemory> SaveEmotionalMemoryAsync(string content, string emotion, double intensity, string context = "")
    {
        try
        {
            _logger.LogInformation($"💭 Сохраняю эмоциональное воспоминание: {emotion} (интенсивность: {intensity:F2})");
            
            // Создаем эмоциональное воспоминание
            var emotionalMemory = new EmotionalMemory
            {
                Id = Guid.NewGuid(),
                Content = content,
                Emotion = emotion,
                Intensity = intensity,
                Context = context,
                CreatedAt = DateTime.UtcNow,
                LastAccessed = DateTime.UtcNow,
                AccessCount = 1,
                EmotionalValence = CalculateEmotionalValence(emotion, intensity),
                MemoryStrength = CalculateMemoryStrength(emotion, intensity),
                AssociatedEmotions = GenerateAssociatedEmotions(emotion, intensity)
            };
            
            // Добавляем в список воспоминаний
            _emotionalMemories.Add(emotionalMemory);
            
            // Индексируем по эмоции
            if (!_emotionIndex.ContainsKey(emotion))
            {
                _emotionIndex[emotion] = new List<EmotionalMemory>();
            }
            _emotionIndex[emotion].Add(emotionalMemory);
            
            // Создаем событие памяти
            var memoryEvent = new EmotionalMemoryEvent
            {
                Id = Guid.NewGuid(),
                MemoryId = emotionalMemory.Id,
                EventType = "memory_created",
                Emotion = emotion,
                Intensity = intensity,
                Timestamp = DateTime.UtcNow
            };
            _memoryEvents.Add(memoryEvent);
            
            // Сохраняем в общую память
            await SaveEmotionalMemoryAsync(content, context, intensity);
            
            _logger.LogInformation($"💭 Сохранено эмоциональное воспоминание: {emotionalMemory.Id}");
            
            return emotionalMemory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении эмоционального воспоминания");
            return new EmotionalMemory
            {
                Id = Guid.NewGuid(),
                Content = content,
                Emotion = emotion,
                Intensity = intensity,
                Context = context,
                CreatedAt = DateTime.UtcNow,
                MemoryStrength = 0.5
            };
        }
    }

    /// <summary>
    /// Получает эмоциональные воспоминания по эмоции
    /// </summary>
    public async Task<List<EmotionalMemory>> GetMemoriesByEmotionAsync(string emotion, int count = 10)
    {
        if (_emotionIndex.ContainsKey(emotion))
        {
            return _emotionIndex[emotion]
                .OrderByDescending(m => m.MemoryStrength)
                .ThenByDescending(m => m.LastAccessed)
                .Take(count)
                .ToList();
        }
        
        return new List<EmotionalMemory>();
    }

    /// <summary>
    /// Получает связанные эмоциональные воспоминания
    /// </summary>
    public async Task<List<EmotionalMemory>> GetRelatedMemoriesAsync(string content, int count = 10)
    {
        var relatedMemories = new List<EmotionalMemory>();
        var contentLower = content.ToLowerInvariant();
        
        // Ищем воспоминания с похожим содержанием
        foreach (var memory in _emotionalMemories)
        {
            var similarity = CalculateContentSimilarity(contentLower, memory.Content.ToLowerInvariant());
            if (similarity > 0.3) // Порог схожести
            {
                memory.SimilarityScore = similarity;
                relatedMemories.Add(memory);
            }
        }
        
        return relatedMemories
            .OrderByDescending(m => m.SimilarityScore)
            .ThenByDescending(m => m.MemoryStrength)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Активирует эмоциональное воспоминание
    /// </summary>
    public async Task<EmotionalMemory?> ActivateMemoryAsync(Guid memoryId)
    {
        var memory = _emotionalMemories.FirstOrDefault(m => m.Id == memoryId);
        
        if (memory != null)
        {
            // Обновляем статистику доступа
            memory.LastAccessed = DateTime.UtcNow;
            memory.AccessCount++;
            
            // Усиливаем память при активации
            memory.MemoryStrength = Math.Min(1.0, memory.MemoryStrength + 0.1);
            
            // Создаем событие активации
            var activationEvent = new EmotionalMemoryEvent
            {
                Id = Guid.NewGuid(),
                MemoryId = memoryId,
                EventType = "memory_activated",
                Emotion = memory.Emotion,
                Intensity = memory.Intensity,
                Timestamp = DateTime.UtcNow
            };
            _memoryEvents.Add(activationEvent);
            
            _logger.LogDebug($"💭 Активировано воспоминание: {memoryId}");
            
            return memory;
        }
        
        return null;
    }

    /// <summary>
    /// Генерирует эмоциональную ассоциацию на основе контента и текущей эмоции
    /// </summary>
    private async Task<string> GenerateEmotionalAssociationAsync(string content, string currentEmotion)
    {
        // Ищем связанные воспоминания
        var relatedMemories = await GetRelatedMemoriesAsync(content, 5);
        
        if (relatedMemories.Any())
        {
            var strongestMemory = relatedMemories.First();
            
            // Генерируем осознанную эмоциональную ассоциацию
            var associationDescription = $"Это напоминает мне о {strongestMemory.Content.Substring(0, Math.Min(30, strongestMemory.Content.Length))}...";
            
            // Добавляем эмоциональный контекст
            if (strongestMemory.Emotion != currentEmotion)
            {
                associationDescription += $" Тогда я испытывала {strongestMemory.Emotion}, а сейчас чувствую {currentEmotion}.";
            }
            else
            {
                associationDescription += $" Я снова испытываю {currentEmotion}, как и тогда.";
            }
            
            return associationDescription;
        }
        
        // Если нет связанных воспоминаний, генерируем новую ассоциацию
        return $"Это вызывает у меня чувство {currentEmotion}...";
    }

    /// <summary>
    /// Вычисляет эмоциональную валентность
    /// </summary>
    private double CalculateEmotionalValence(string emotion, double intensity)
    {
        var baseValence = emotion.ToLowerInvariant() switch
        {
            "joy" => 0.8,
            "love" => 0.9,
            "curiosity" => 0.6,
            "trust" => 0.7,
            "surprise" => 0.0,
            "anticipation" => 0.5,
            "sadness" => -0.7,
            "anger" => -0.6,
            "fear" => -0.8,
            "disgust" => -0.9,
            _ => 0.0
        };
        
        return baseValence * intensity;
    }

    /// <summary>
    /// Вычисляет силу памяти
    /// </summary>
    private double CalculateMemoryStrength(string emotion, double intensity)
    {
        var baseStrength = 0.5;
        
        // Эмоциональная интенсивность влияет на силу памяти
        baseStrength += intensity * 0.3;
        
        // Некоторые эмоции создают более сильные воспоминания
        if (_emotionalAssociations.ContainsKey(emotion.ToLowerInvariant()))
        {
            baseStrength += _emotionalAssociations[emotion.ToLowerInvariant()] * 0.2;
        }
        
        // Случайный фактор
        baseStrength += _random.NextDouble() * 0.1;
        
        return Math.Max(0.1, Math.Min(1.0, baseStrength));
    }

    /// <summary>
    /// Генерирует связанные эмоции
    /// </summary>
    private List<string> GenerateAssociatedEmotions(string primaryEmotion, double intensity)
    {
        var associatedEmotions = new List<string>();
        
        // Эмоции, которые часто связаны с основной
        var emotionAssociations = new Dictionary<string, string[]>
        {
            ["joy"] = new[] { "excitement", "satisfaction", "gratitude" },
            ["sadness"] = new[] { "melancholy", "nostalgia", "loneliness" },
            ["anger"] = new[] { "frustration", "irritation", "rage" },
            ["fear"] = new[] { "anxiety", "panic", "worry" },
            ["love"] = new[] { "affection", "tenderness", "passion" },
            ["curiosity"] = new[] { "interest", "wonder", "excitement" },
            ["surprise"] = new[] { "amazement", "shock", "wonder" },
            ["trust"] = new[] { "confidence", "reliance", "faith" }
        };
        
        if (emotionAssociations.ContainsKey(primaryEmotion.ToLowerInvariant()))
        {
            var associations = emotionAssociations[primaryEmotion.ToLowerInvariant()];
            var count = _random.Next(1, Math.Min(3, associations.Length + 1));
            
            for (int i = 0; i < count; i++)
            {
                var emotion = associations[_random.Next(associations.Length)];
                if (!associatedEmotions.Contains(emotion))
                {
                    associatedEmotions.Add(emotion);
                }
            }
        }
        
        return associatedEmotions;
    }

    /// <summary>
    /// Вычисляет схожесть содержания
    /// </summary>
    private double CalculateContentSimilarity(string content1, string content2)
    {
        var words1 = content1.Split(' ').ToHashSet();
        var words2 = content2.Split(' ').ToHashSet();
        
        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();
        
        return union > 0 ? (double)intersection / union : 0.0;
    }

    /// <summary>
    /// Консолидирует память
    /// </summary>
    public async Task ConsolidateMemoryAsync()
    {
        _logger.LogInformation("💭 Начинаю консолидацию эмоциональной памяти...");
        
        var cutoffTime = DateTime.UtcNow.AddDays(-7);
        var oldMemories = _emotionalMemories
            .Where(m => m.LastAccessed < cutoffTime && m.MemoryStrength < 0.3)
            .ToList();
        
        foreach (var memory in oldMemories)
        {
            _emotionalMemories.Remove(memory);
            
            // Удаляем из индекса
            if (_emotionIndex.ContainsKey(memory.Emotion))
            {
                _emotionIndex[memory.Emotion].Remove(memory);
            }
        }
        
        // Усиливаем важные воспоминания
        var importantMemories = _emotionalMemories
            .Where(m => m.Intensity > 0.8 || m.AccessCount > 5)
            .ToList();
        
        foreach (var memory in importantMemories)
        {
            memory.MemoryStrength = Math.Min(1.0, memory.MemoryStrength + 0.05);
        }
        
        _lastMemoryConsolidation = DateTime.UtcNow;
        
        _logger.LogInformation($"💭 Консолидация завершена: удалено {oldMemories.Count} старых воспоминаний, усилено {importantMemories.Count} важных");
    }

    /// <summary>
    /// Получает статистику эмоциональной памяти
    /// </summary>
    public async Task<EmotionalMemoryStatistics> GetStatisticsAsync()
    {
        var statistics = new EmotionalMemoryStatistics
        {
            TotalMemories = _emotionalMemories.Count,
            AverageIntensity = _emotionalMemories.Any() ? _emotionalMemories.Average(m => m.Intensity) : 0,
            AverageStrength = _emotionalMemories.Any() ? _emotionalMemories.Average(m => m.MemoryStrength) : 0,
            EmotionDistribution = GetEmotionDistribution(),
            MostFrequentEmotion = GetMostFrequentEmotion(),
            RecentMemories = _emotionalMemories.Count(m => m.CreatedAt > DateTime.UtcNow.AddHours(-24)),
            TotalEvents = _memoryEvents.Count
        };
        
        return statistics;
    }

    /// <summary>
    /// Получает распределение эмоций
    /// </summary>
    private Dictionary<string, int> GetEmotionDistribution()
    {
        return _emotionalMemories
            .GroupBy(m => m.Emotion)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Получает самую частую эмоцию
    /// </summary>
    private string GetMostFrequentEmotion()
    {
        var distribution = GetEmotionDistribution();
        return distribution.OrderByDescending(x => x.Value).FirstOrDefault().Key ?? "none";
    }

    /// <summary>
    /// Получает статус движка эмоциональной памяти
    /// </summary>
    public EmotionalMemoryStatus GetStatus()
    {
        return new EmotionalMemoryStatus
        {
            TotalMemories = _emotionalMemories.Count,
            MemoryRetentionRate = _memoryRetentionRate,
            EmotionalInfluenceStrength = _emotionalInfluenceStrength,
            LastConsolidation = _lastMemoryConsolidation,
            IndexedEmotions = _emotionIndex.Count,
            TotalEvents = _memoryEvents.Count
        };
    }

    /// <summary>
    /// Устанавливает скорость удержания памяти
    /// </summary>
    public void SetMemoryRetentionRate(double rate)
    {
        _memoryRetentionRate = Math.Max(0.1, Math.Min(1.0, rate));
        _logger.LogInformation($"💭 Установлена скорость удержания памяти: {_memoryRetentionRate:F2}");
    }

    /// <summary>
    /// Очищает старые события памяти
    /// </summary>
    public void CleanupOldEvents(int maxEvents = 1000)
    {
        if (_memoryEvents.Count > maxEvents)
        {
            var cutoffTime = DateTime.UtcNow.AddDays(-30);
            _memoryEvents.RemoveAll(e => e.Timestamp < cutoffTime);
            
            _logger.LogInformation($"💭 Очищено {_memoryEvents.Count} старых событий памяти");
        }
    }
}

/// <summary>
/// Эмоциональное воспоминание
/// </summary>
public class EmotionalMemory
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Emotion { get; set; } = string.Empty;
    public double Intensity { get; set; } = 0.5;
    public string Context { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastAccessed { get; set; }
    public int AccessCount { get; set; } = 0;
    public double EmotionalValence { get; set; } = 0.0;
    public double MemoryStrength { get; set; } = 0.5;
    public List<string> AssociatedEmotions { get; set; } = new();
    public double SimilarityScore { get; set; } = 0.0;
}

/// <summary>
/// Событие эмоциональной памяти
/// </summary>
public class EmotionalMemoryEvent
{
    public Guid Id { get; set; }
    public Guid MemoryId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Emotion { get; set; } = string.Empty;
    public double Intensity { get; set; } = 0.5;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Статистика эмоциональной памяти
/// </summary>
public class EmotionalMemoryStatistics
{
    public int TotalMemories { get; set; }
    public double AverageIntensity { get; set; }
    public double AverageStrength { get; set; }
    public Dictionary<string, int> EmotionDistribution { get; set; } = new();
    public string MostFrequentEmotion { get; set; } = string.Empty;
    public int RecentMemories { get; set; }
    public int TotalEvents { get; set; }
}

/// <summary>
/// Статус движка эмоциональной памяти
/// </summary>
public class EmotionalMemoryStatus
{
    public int TotalMemories { get; set; }
    public double MemoryRetentionRate { get; set; }
    public double EmotionalInfluenceStrength { get; set; }
    public DateTime LastConsolidation { get; set; }
    public int IndexedEmotions { get; set; }
    public int TotalEvents { get; set; }
} 