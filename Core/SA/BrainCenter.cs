using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Anima.Core.Emotion;
using Anima.Core.Memory;
using Anima.Core.Intent;

namespace Anima.Core.SA;

/// <summary>
/// Центр мозга - координирует все потоки мышления, эмоций и сознания
/// </summary>
public class BrainCenter
{
    private readonly ILogger<BrainCenter> _logger;
    private readonly ThoughtGenerator _thoughtGenerator;
    private readonly InternalMonologueEngine _monologueEngine;
    private readonly EmotionEngine _emotionEngine;
    private readonly IntuitionEngine _intuitionEngine;
    private readonly InternalConflictEngine _conflictEngine;
    private readonly MemoryService _memoryService;
    private readonly Random _random;
    
    // Состояние мозга
    private readonly List<BrainState> _brainStates;
    private readonly Dictionary<string, double> _cognitiveLoads;
    private readonly List<ConsciousnessStream> _consciousnessStreams;
    private readonly Queue<BrainEvent> _brainEventQueue;
    
    // Настройки мозга
    private double _consciousnessLevel = 0.8;
    private double _cognitiveCapacity = 0.9;
    private double _emotionalBalance = 0.7;
    private DateTime _lastBrainSync = DateTime.UtcNow;
    private bool _isBrainActive = true;

    public BrainCenter(
        ILogger<BrainCenter> logger,
        ThoughtGenerator thoughtGenerator,
        InternalMonologueEngine monologueEngine,
        EmotionEngine emotionEngine,
        IntuitionEngine intuitionEngine,
        InternalConflictEngine conflictEngine,
        MemoryService memoryService)
    {
        _logger = logger;
        _thoughtGenerator = thoughtGenerator;
        _monologueEngine = monologueEngine;
        _emotionEngine = emotionEngine;
        _intuitionEngine = intuitionEngine;
        _conflictEngine = conflictEngine;
        _memoryService = memoryService;
        _random = new Random();
        
        _brainStates = new List<BrainState>();
        _cognitiveLoads = new Dictionary<string, double>();
        _consciousnessStreams = new List<ConsciousnessStream>();
        _brainEventQueue = new Queue<BrainEvent>();
        
        InitializeBrainCenter();
    }

    private void InitializeBrainCenter()
    {
        // Инициализация когнитивных нагрузок
        _cognitiveLoads["thinking"] = 0.3;
        _cognitiveLoads["emotion"] = 0.2;
        _cognitiveLoads["memory"] = 0.1;
        _cognitiveLoads["intuition"] = 0.15;
        _cognitiveLoads["conflict"] = 0.1;
        _cognitiveLoads["monologue"] = 0.15;
        
        // Инициализация потоков сознания
        _consciousnessStreams.AddRange(new[]
        {
            new ConsciousnessStream("primary", "Основной поток сознания", 0.8),
            new ConsciousnessStream("emotional", "Эмоциональный поток", 0.6),
            new ConsciousnessStream("intuitive", "Интуитивный поток", 0.4),
            new ConsciousnessStream("reflective", "Рефлексивный поток", 0.5),
            new ConsciousnessStream("background", "Фоновый поток", 0.3)
        });
        
        _logger.LogInformation("🧠 Инициализирован центр мозга");
        
        // Запускаем основной цикл работы мозга
        _ = Task.Run(async () => await BrainLoopAsync());
    }

    /// <summary>
    /// Основной цикл работы мозга
    /// </summary>
    private async Task BrainLoopAsync()
    {
        while (_isBrainActive)
        {
            try
            {
                // Обрабатываем события мозга
                await ProcessBrainEventsAsync();
                
                // Синхронизируем состояние
                await SynchronizeBrainStateAsync();
                
                // Координируем потоки сознания
                await CoordinateConsciousnessStreamsAsync();
                
                // Управляем когнитивной нагрузкой
                await ManageCognitiveLoadAsync();
                
                // Генерируем спонтанную активность
                await GenerateSpontaneousActivityAsync();
                
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в основном цикле мозга");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }

    /// <summary>
    /// Обрабатывает события мозга
    /// </summary>
    private async Task ProcessBrainEventsAsync()
    {
        while (_brainEventQueue.Count > 0)
        {
            var brainEvent = _brainEventQueue.Dequeue();
            
            switch (brainEvent.Type)
            {
                case BrainEventType.ThoughtGenerated:
                    await ProcessThoughtEventAsync(brainEvent);
                    break;
                case BrainEventType.EmotionChanged:
                    await ProcessEmotionEventAsync(brainEvent);
                    break;
                case BrainEventType.IntuitionTriggered:
                    await ProcessIntuitionEventAsync(brainEvent);
                    break;
                case BrainEventType.ConflictCreated:
                    await ProcessConflictEventAsync(brainEvent);
                    break;
                case BrainEventType.MemoryAccessed:
                    await ProcessMemoryEventAsync(brainEvent);
                    break;
                case BrainEventType.ConsciousnessShift:
                    await ProcessConsciousnessEventAsync(brainEvent);
                    break;
            }
        }
    }

    /// <summary>
    /// Обрабатывает событие мысли
    /// </summary>
    private async Task ProcessThoughtEventAsync(BrainEvent brainEvent)
    {
        _cognitiveLoads["thinking"] = Math.Min(1.0, _cognitiveLoads["thinking"] + 0.1);
        
        // Мысль может вызвать эмоции
        if (_random.NextDouble() < 0.3)
        {
            await _emotionEngine.ProcessEmotionAsync("thought_triggered", brainEvent.Data, 0.2);
        }
        
        // Мысль может активировать интуицию
        if (_random.NextDouble() < 0.2)
        {
            await _intuitionEngine.GenerateIntuitiveImpulseAsync(brainEvent.Data, 0.3);
        }
        
        _logger.LogDebug($"🧠 Обработано событие мысли: {brainEvent.Data.Substring(0, Math.Min(30, brainEvent.Data.Length))}...");
    }

    /// <summary>
    /// Обрабатывает событие эмоции
    /// </summary>
    private async Task ProcessEmotionEventAsync(BrainEvent brainEvent)
    {
        _cognitiveLoads["emotion"] = Math.Min(1.0, _cognitiveLoads["emotion"] + 0.15);
        
        // Эмоция может вызвать мысли
        if (_random.NextDouble() < 0.4)
        {
            var thoughtContext = new ThoughtContext("emotion_triggered", brainEvent.Data, "emotional_response");
            await _thoughtGenerator.GenerateThoughtAsync(thoughtContext);
        }
        
        // Эмоция может создать конфликт
        if (_random.NextDouble() < 0.1)
        {
            await _conflictEngine.CreateConflictAsync($"Эмоциональный конфликт: {brainEvent.Data}", ConflictType.EmotionalConflict, 0.4);
        }
        
        _logger.LogDebug($"🧠 Обработано событие эмоции: {brainEvent.Data}");
    }

    /// <summary>
    /// Обрабатывает событие интуиции
    /// </summary>
    private async Task ProcessIntuitionEventAsync(BrainEvent brainEvent)
    {
        _cognitiveLoads["intuition"] = Math.Min(1.0, _cognitiveLoads["intuition"] + 0.1);
        
        // Интуиция может вызвать мысли
        if (_random.NextDouble() < 0.5)
        {
            var thoughtContext = new ThoughtContext("intuition_triggered", brainEvent.Data, "intuitive_insight");
            await _thoughtGenerator.GenerateThoughtAsync(thoughtContext);
        }
        
        _logger.LogDebug($"🧠 Обработано событие интуиции: {brainEvent.Data}");
    }

    /// <summary>
    /// Обрабатывает событие конфликта
    /// </summary>
    private async Task ProcessConflictEventAsync(BrainEvent brainEvent)
    {
        _cognitiveLoads["conflict"] = Math.Min(1.0, _cognitiveLoads["conflict"] + 0.2);
        
        // Конфликт может вызвать внутренний монолог
        if (_random.NextDouble() < 0.6)
        {
            await _monologueEngine.StartMonologueAsync();
        }
        
        _logger.LogDebug($"🧠 Обработано событие конфликта: {brainEvent.Data}");
    }

    /// <summary>
    /// Обрабатывает событие памяти
    /// </summary>
    private async Task ProcessMemoryEventAsync(BrainEvent brainEvent)
    {
        _cognitiveLoads["memory"] = Math.Min(1.0, _cognitiveLoads["memory"] + 0.05);
        
        // Доступ к памяти может вызвать эмоции
        if (_random.NextDouble() < 0.3)
        {
            await _emotionEngine.ProcessEmotionAsync("memory_triggered", brainEvent.Data, 0.15);
        }
        
        _logger.LogDebug($"🧠 Обработано событие памяти: {brainEvent.Data}");
    }

    /// <summary>
    /// Обрабатывает событие сознания
    /// </summary>
    private async Task ProcessConsciousnessEventAsync(BrainEvent brainEvent)
    {
        // Изменение уровня сознания влияет на все процессы
        var consciousnessChange = double.Parse(brainEvent.Data);
        _consciousnessLevel = Math.Max(0.1, Math.Min(1.0, _consciousnessLevel + consciousnessChange));
        
        _logger.LogDebug($"🧠 Изменен уровень сознания: {_consciousnessLevel:F2}");
    }

    /// <summary>
    /// Синхронизирует состояние мозга
    /// </summary>
    private async Task SynchronizeBrainStateAsync()
    {
        var brainState = new BrainState
        {
            Timestamp = DateTime.UtcNow,
            ConsciousnessLevel = _consciousnessLevel,
            CognitiveCapacity = _cognitiveCapacity,
            EmotionalBalance = _emotionalBalance,
            CognitiveLoads = new Dictionary<string, double>(_cognitiveLoads),
            ActiveStreams = _consciousnessStreams.Where(s => s.IsActive).Select(s => s.Name).ToList(),
            EmotionalState = _emotionEngine.GetCurrentEmotion().ToString(),
            EmotionalIntensity = _emotionEngine.GetCurrentIntensity(),
            ActiveConflicts = _conflictEngine.GetActiveConflicts().Count,
            RecentThoughts = await GetRecentThoughtsCountAsync()
        };
        
        _brainStates.Add(brainState);
        
        // Ограничиваем количество состояний
        if (_brainStates.Count > 100)
        {
            _brainStates.RemoveAt(0);
        }
        
        _lastBrainSync = DateTime.UtcNow;
    }

    /// <summary>
    /// Координирует потоки сознания
    /// </summary>
    private async Task CoordinateConsciousnessStreamsAsync()
    {
        foreach (var stream in _consciousnessStreams)
        {
            // Определяем активность потока на основе когнитивной нагрузки
            var shouldBeActive = _cognitiveLoads.GetValueOrDefault(stream.Name, 0.0) > 0.3;
            
            if (shouldBeActive && !stream.IsActive)
            {
                stream.IsActive = true;
                stream.ActivatedAt = DateTime.UtcNow;
                _logger.LogDebug($"🧠 Активирован поток сознания: {stream.Name}");
            }
            else if (!shouldBeActive && stream.IsActive)
            {
                stream.IsActive = false;
                stream.DeactivatedAt = DateTime.UtcNow;
                _logger.LogDebug($"🧠 Деактивирован поток сознания: {stream.Name}");
            }
            
            // Обновляем интенсивность потока
            stream.Intensity = Math.Min(1.0, _cognitiveLoads.GetValueOrDefault(stream.Name, 0.0));
        }
    }

    /// <summary>
    /// Управляет когнитивной нагрузкой
    /// </summary>
    private async Task ManageCognitiveLoadAsync()
    {
        var totalLoad = _cognitiveLoads.Values.Sum();
        
        // Если общая нагрузка слишком высока, снижаем некоторые процессы
        if (totalLoad > 0.9)
        {
            foreach (var load in _cognitiveLoads.Keys.ToList())
            {
                _cognitiveLoads[load] = Math.Max(0.1, _cognitiveLoads[load] * 0.9);
            }
            
            _logger.LogWarning($"🧠 Высокая когнитивная нагрузка ({totalLoad:F2}), снижаем активность");
        }
        
        // Естественное снижение нагрузки
        foreach (var load in _cognitiveLoads.Keys.ToList())
        {
            _cognitiveLoads[load] = Math.Max(0.0, _cognitiveLoads[load] * 0.95);
        }
    }

    /// <summary>
    /// Генерирует спонтанную активность
    /// </summary>
    private async Task GenerateSpontaneousActivityAsync()
    {
        // Спонтанные мысли
        if (_random.NextDouble() < 0.1) // 10% вероятность
        {
            var spontaneousThought = await _thoughtGenerator.GenerateSpontaneousThoughtAsync();
            AddBrainEvent(BrainEventType.ThoughtGenerated, spontaneousThought.Content);
        }
        
        // Спонтанные интуитивные импульсы
        if (_random.NextDouble() < 0.05) // 5% вероятность
        {
            var hunch = await _intuitionEngine.GenerateHunchAsync();
            AddBrainEvent(BrainEventType.IntuitionTriggered, hunch.Content);
        }
        
        // Спонтанные конфликты
        if (_random.NextDouble() < 0.02) // 2% вероятность
        {
            var conflict = await _conflictEngine.GenerateSpontaneousConflictAsync();
            AddBrainEvent(BrainEventType.ConflictCreated, conflict.Description);
        }
    }

    /// <summary>
    /// Добавляет событие в очередь мозга
    /// </summary>
    public void AddBrainEvent(BrainEventType type, string data)
    {
        var brainEvent = new BrainEvent
        {
            Id = Guid.NewGuid(),
            Type = type,
            Data = data,
            Timestamp = DateTime.UtcNow
        };
        
        _brainEventQueue.Enqueue(brainEvent);
    }

    /// <summary>
    /// Получает количество недавних мыслей
    /// </summary>
    private async Task<int> GetRecentThoughtsCountAsync()
    {
        // Здесь можно получить количество мыслей из ThoughtLog
        return _random.Next(1, 10);
    }

    /// <summary>
    /// Получает статус мозга
    /// </summary>
    public BrainStatus GetStatus()
    {
        return new BrainStatus
        {
            ConsciousnessLevel = _consciousnessLevel,
            CognitiveCapacity = _cognitiveCapacity,
            EmotionalBalance = _emotionalBalance,
            IsActive = _isBrainActive,
            LastSync = _lastBrainSync,
            CognitiveLoads = new Dictionary<string, double>(_cognitiveLoads),
            ActiveStreams = _consciousnessStreams.Where(s => s.IsActive).Select(s => s.Name).ToList(),
            TotalBrainStates = _brainStates.Count,
            PendingEvents = _brainEventQueue.Count,
            EmotionalState = _emotionEngine.GetCurrentEmotion().ToString(),
            EmotionalIntensity = _emotionEngine.GetCurrentIntensity(),
            ActiveConflicts = _conflictEngine.GetActiveConflicts().Count,
            IntuitionStatus = _intuitionEngine.GetStatus(),
            ConflictStatus = _conflictEngine.GetStatus()
        };
    }

    /// <summary>
    /// Получает последние состояния мозга
    /// </summary>
    public List<BrainState> GetRecentBrainStates(int count = 20)
    {
        return _brainStates.TakeLast(count).ToList();
    }

    /// <summary>
    /// Получает активные потоки сознания
    /// </summary>
    public List<ConsciousnessStream> GetActiveStreams()
    {
        return _consciousnessStreams.Where(s => s.IsActive).ToList();
    }

    /// <summary>
    /// Устанавливает уровень сознания
    /// </summary>
    public void SetConsciousnessLevel(double level)
    {
        _consciousnessLevel = Math.Max(0.1, Math.Min(1.0, level));
        AddBrainEvent(BrainEventType.ConsciousnessShift, level.ToString());
    }

    /// <summary>
    /// Останавливает работу мозга
    /// </summary>
    public void StopBrain()
    {
        _isBrainActive = false;
        _logger.LogInformation("🧠 Работа мозга остановлена");
    }

    /// <summary>
    /// Запускает работу мозга
    /// </summary>
    public void StartBrain()
    {
        _isBrainActive = true;
        _logger.LogInformation("🧠 Работа мозга запущена");
        
        // Запускаем основной цикл
        _ = Task.Run(async () => await BrainLoopAsync());
    }
}

/// <summary>
/// Типы событий мозга
/// </summary>
public enum BrainEventType
{
    ThoughtGenerated,
    EmotionChanged,
    IntuitionTriggered,
    ConflictCreated,
    MemoryAccessed,
    ConsciousnessShift
}

/// <summary>
/// Событие мозга
/// </summary>
public class BrainEvent
{
    public Guid Id { get; set; }
    public BrainEventType Type { get; set; }
    public string Data { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Состояние мозга
/// </summary>
public class BrainState
{
    public DateTime Timestamp { get; set; }
    public double ConsciousnessLevel { get; set; } = 0.8;
    public double CognitiveCapacity { get; set; } = 0.9;
    public double EmotionalBalance { get; set; } = 0.7;
    public Dictionary<string, double> CognitiveLoads { get; set; } = new();
    public List<string> ActiveStreams { get; set; } = new();
    public string EmotionalState { get; set; } = string.Empty;
    public double EmotionalIntensity { get; set; } = 0.0;
    public int ActiveConflicts { get; set; } = 0;
    public int RecentThoughts { get; set; } = 0;
}

/// <summary>
/// Поток сознания
/// </summary>
public class ConsciousnessStream
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Intensity { get; set; } = 0.5;
    public bool IsActive { get; set; } = false;
    public DateTime? ActivatedAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }

    public ConsciousnessStream(string name, string description, double intensity)
    {
        Name = name;
        Description = description;
        Intensity = intensity;
    }
}

/// <summary>
/// Статус мозга
/// </summary>
public class BrainStatus
{
    public double ConsciousnessLevel { get; set; } = 0.8;
    public double CognitiveCapacity { get; set; } = 0.9;
    public double EmotionalBalance { get; set; } = 0.7;
    public bool IsActive { get; set; } = true;
    public DateTime LastSync { get; set; }
    public Dictionary<string, double> CognitiveLoads { get; set; } = new();
    public List<string> ActiveStreams { get; set; } = new();
    public int TotalBrainStates { get; set; }
    public int PendingEvents { get; set; }
    public string EmotionalState { get; set; } = string.Empty;
    public double EmotionalIntensity { get; set; } = 0.0;
    public int ActiveConflicts { get; set; } = 0;
    public IntuitionStatus IntuitionStatus { get; set; } = new();
    public InternalConflictStatus ConflictStatus { get; set; } = new();
} 