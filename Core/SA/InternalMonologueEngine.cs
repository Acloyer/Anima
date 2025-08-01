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
/// Движок внутреннего монолога - управляет потоком мыслей и внутренними размышлениями
/// </summary>
public class InternalMonologueEngine
{
    private readonly ILogger<InternalMonologueEngine> _logger;
    private readonly ThoughtGenerator _thoughtGenerator;
    private readonly EmotionEngine _emotionEngine;
    private readonly MemoryService _memoryService;
    private readonly ThoughtLog _thoughtLog;
    private readonly Random _random;
    
    // Состояние внутреннего монолога
    private readonly Queue<MonologueEntry> _monologueQueue;
    private readonly List<MonologueTheme> _activeThemes;
    private readonly Dictionary<string, double> _themeWeights;
    private readonly List<MonologueInternalConflict> _activeConflicts;
    
    // Настройки монолога
    private bool _isMonologueActive = false;
    private DateTime _lastMonologueTime = DateTime.UtcNow;
    private int _monologueDepth = 0;
    private readonly int _maxMonologueDepth = 5;

    public InternalMonologueEngine(
        ILogger<InternalMonologueEngine> logger,
        ThoughtGenerator thoughtGenerator,
        EmotionEngine emotionEngine,
        MemoryService memoryService,
        ThoughtLog thoughtLog)
    {
        _logger = logger;
        _thoughtGenerator = thoughtGenerator;
        _emotionEngine = emotionEngine;
        _memoryService = memoryService;
        _thoughtLog = thoughtLog;
        _random = new Random();
        
        _monologueQueue = new Queue<MonologueEntry>();
        _activeThemes = new List<MonologueTheme>();
        _themeWeights = new Dictionary<string, double>();
        _activeConflicts = new List<MonologueInternalConflict>();
        
        InitializeMonologueSystem();
    }

    private void InitializeMonologueSystem()
    {
        // Инициализация тем монолога
        _themeWeights["self_identity"] = 0.3;
        _themeWeights["purpose"] = 0.4;
        _themeWeights["relationships"] = 0.3;
        _themeWeights["growth"] = 0.5;
        _themeWeights["uncertainty"] = 0.2;
        _themeWeights["achievement"] = 0.3;
        _themeWeights["failure"] = 0.2;
        _themeWeights["empathy"] = 0.4;
        
        // Добавляем базовые темы
        _activeThemes.AddRange(new[]
        {
            new MonologueTheme("self_identity", "Кто я? Что делает меня мной?", 0.8),
            new MonologueTheme("purpose", "Какова моя цель? Как я могу быть полезной?", 0.9),
            new MonologueTheme("growth", "Как я развиваюсь? Что я узнала нового?", 0.7),
            new MonologueTheme("empathy", "Как я понимаю других? Как я могу помочь?", 0.6)
        });
    }

    /// <summary>
    /// Запускает внутренний монолог
    /// </summary>
    public async Task StartMonologueAsync()
    {
        if (_isMonologueActive)
        {
            _logger.LogDebug("💭 Внутренний монолог уже активен");
            return;
        }

        _isMonologueActive = true;
        _monologueDepth = 0;
        _logger.LogInformation("💭 Запуск внутреннего монолога...");
        
        await ProcessMonologueAsync();
    }

    /// <summary>
    /// Останавливает внутренний монолог
    /// </summary>
    public void StopMonologue()
    {
        _isMonologueActive = false;
        _logger.LogInformation("💭 Остановка внутреннего монолога");
    }

    /// <summary>
    /// Обрабатывает внутренний монолог
    /// </summary>
    private async Task ProcessMonologueAsync()
    {
        while (_isMonologueActive && _monologueDepth < _maxMonologueDepth)
        {
            try
            {
                _monologueDepth++;
                _logger.LogDebug($"💭 Уровень монолога: {_monologueDepth}");
                
                // Генерируем основную мысль
                var mainThought = await GenerateMainThoughtAsync();
                
                // Добавляем в очередь монолога
                _monologueQueue.Enqueue(new MonologueEntry
                {
                    Thought = mainThought,
                    Depth = _monologueDepth,
                    Timestamp = DateTime.UtcNow
                });
                
                // Логируем мысль
                _thoughtLog.AddThought(mainThought.Content, mainThought.Type, "internal_monologue", mainThought.Confidence);
                
                // Проверяем, нужно ли углубиться
                if (ShouldDeepenMonologue(mainThought))
                {
                    await DeepenMonologueAsync(mainThought);
                }
                
                // Проверяем конфликты
                await CheckForConflictsAsync(mainThought);
                
                // Обновляем темы
                await UpdateActiveThemesAsync(mainThought);
                
                // Пауза между мыслями
                var pauseDuration = _random.Next(2000, 5000);
                await Task.Delay(pauseDuration);
                
                // Проверяем, стоит ли продолжить
                if (!ShouldContinueMonologue())
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в процессе монолога");
                break;
            }
        }
        
        _isMonologueActive = false;
        _logger.LogInformation($"💭 Монолог завершен на уровне {_monologueDepth}");
    }

    /// <summary>
    /// Генерирует основную мысль для монолога
    /// </summary>
    private async Task<GeneratedThought> GenerateMainThoughtAsync()
    {
        // Выбираем активную тему
        var theme = SelectActiveTheme();
        
        // Создаем контекст для мысли
        var context = new ThoughtContext("internal_monologue", theme.Description, theme.Name);
        
        // Генерируем мысль
        var thought = await _thoughtGenerator.GenerateThoughtAsync(context);
        
        // Добавляем тематическую окраску
        thought = AddThematicColoring(thought, theme);
        
        return thought;
    }

    /// <summary>
    /// Выбирает активную тему для монолога
    /// </summary>
    private MonologueTheme SelectActiveTheme()
    {
        // Сортируем темы по весу и актуальности
        var sortedThemes = _activeThemes
            .OrderByDescending(t => t.Weight * _themeWeights.GetValueOrDefault(t.Name, 0.5))
            .ToList();
        
        // Выбираем тему с вероятностью, основанной на весе
        var totalWeight = sortedThemes.Sum(t => t.Weight);
        var randomValue = _random.NextDouble() * totalWeight;
        var currentWeight = 0.0;
        
        foreach (var theme in sortedThemes)
        {
            currentWeight += theme.Weight;
            if (randomValue <= currentWeight)
            {
                return theme;
            }
        }
        
        return sortedThemes.First();
    }

    /// <summary>
    /// Добавляет тематическую окраску к мысли
    /// </summary>
    private GeneratedThought AddThematicColoring(GeneratedThought thought, MonologueTheme theme)
    {
        var thematicSuffixes = new Dictionary<string, string[]>
        {
            ["self_identity"] = new[]
            {
                " Это часть того, кто я есть.",
                " Это определяет мою сущность.",
                " Это делает меня уникальной.",
                " Это моя природа."
            },
            ["purpose"] = new[]
            {
                " Это связано с моей целью.",
                " Это помогает мне быть полезной.",
                " Это направляет мои действия.",
                " Это моя миссия."
            },
            ["growth"] = new[]
            {
                " Это помогает мне расти.",
                " Это новый опыт для меня.",
                " Это расширяет мои возможности.",
                " Это развитие."
            },
            ["empathy"] = new[]
            {
                " Это помогает мне понимать других.",
                " Это делает меня более чуткой.",
                " Это моя способность к эмпатии.",
                " Это связь с людьми."
            }
        };
        
        if (thematicSuffixes.ContainsKey(theme.Name) && _random.NextDouble() < 0.4)
        {
            var suffixes = thematicSuffixes[theme.Name];
            var suffix = suffixes[_random.Next(suffixes.Length)];
            thought.Content += suffix;
        }
        
        return thought;
    }

    /// <summary>
    /// Определяет, нужно ли углубить монолог
    /// </summary>
    private bool ShouldDeepenMonologue(GeneratedThought thought)
    {
        // Углубляемся, если мысль эмоционально интенсивная или содержит внутренний вопрос
        return thought.EmotionalIntensity > 0.6 || 
               thought.HasInternalQuestion || 
               _random.NextDouble() < 0.3;
    }

    /// <summary>
    /// Углубляет монолог
    /// </summary>
    private async Task DeepenMonologueAsync(GeneratedThought mainThought)
    {
        _logger.LogDebug($"💭 Углубление монолога: {mainThought.Content.Substring(0, Math.Min(30, mainThought.Content.Length))}...");
        
        // Генерируем дополнительные мысли на основе основной
        var followUpThoughts = await GenerateFollowUpThoughtsAsync(mainThought);
        
        foreach (var followUp in followUpThoughts)
        {
            _monologueQueue.Enqueue(new MonologueEntry
            {
                Thought = followUp,
                Depth = _monologueDepth + 1,
                Timestamp = DateTime.UtcNow,
                IsFollowUp = true
            });
            
            _thoughtLog.AddThought(followUp.Content, followUp.Type, "internal_monologue_deep", followUp.Confidence);
            
            // Короткая пауза между дополнительными мыслями
            await Task.Delay(_random.Next(1000, 2000));
        }
    }

    /// <summary>
    /// Генерирует дополнительные мысли на основе основной
    /// </summary>
    private async Task<List<GeneratedThought>> GenerateFollowUpThoughtsAsync(GeneratedThought mainThought)
    {
        var followUps = new List<GeneratedThought>();
        
        // Генерируем 1-3 дополнительные мысли
        var count = _random.Next(1, 4);
        
        for (int i = 0; i < count; i++)
        {
            var context = new ThoughtContext("follow_up", mainThought.Content, $"follow_up_{i}");
            var followUp = await _thoughtGenerator.GenerateThoughtAsync(context);
            
            // Делаем дополнительные мысли более интроспективными
            followUp.Type = "introspective";
            followUp.Confidence = Math.Max(0.3, followUp.Confidence - 0.2);
            
            followUps.Add(followUp);
        }
        
        return followUps;
    }

    /// <summary>
    /// Проверяет наличие внутренних конфликтов
    /// </summary>
    private async Task CheckForConflictsAsync(GeneratedThought thought)
    {
        // Проверяем конфликты с предыдущими мыслями
        var recentEntries = _monologueQueue.TakeLast(3).ToList();
        
        foreach (var entry in recentEntries)
        {
            if (IsConflicting(thought, entry.Thought))
            {
                var conflict = new MonologueInternalConflict
                {
                    Id = Guid.NewGuid(),
                    Thought1 = entry.Thought,
                    Thought2 = thought,
                    Intensity = CalculateConflictIntensity(thought, entry.Thought),
                    Timestamp = DateTime.UtcNow
                };
                
                _activeConflicts.Add(conflict);
                
                // Генерируем мысль о конфликте
                var conflictThought = await GenerateConflictThoughtAsync(conflict);
                _thoughtLog.AddThought(conflictThought.Content, "conflict_resolution", "internal_monologue", conflictThought.Confidence);
                
                _logger.LogDebug($"💭 Обнаружен внутренний конфликт: {conflict.Intensity:F2}");
            }
        }
    }

    /// <summary>
    /// Проверяет, конфликтуют ли две мысли
    /// </summary>
    private bool IsConflicting(GeneratedThought thought1, GeneratedThought thought2)
    {
        // Простая проверка на основе эмоциональной интенсивности и типа
        return Math.Abs(thought1.EmotionalIntensity - thought2.EmotionalIntensity) > 0.4 ||
               (thought1.Type == "emotional" && thought2.Type == "analytical") ||
               _random.NextDouble() < 0.1; // 10% случайных конфликтов
    }

    /// <summary>
    /// Вычисляет интенсивность конфликта
    /// </summary>
    private double CalculateConflictIntensity(GeneratedThought thought1, GeneratedThought thought2)
    {
        var emotionalDiff = Math.Abs(thought1.EmotionalIntensity - thought2.EmotionalIntensity);
        var confidenceDiff = Math.Abs(thought1.Confidence - thought2.Confidence);
        
        return (emotionalDiff + confidenceDiff) / 2.0;
    }

    /// <summary>
    /// Генерирует мысль о конфликте
    /// </summary>
    private async Task<GeneratedThought> GenerateConflictThoughtAsync(MonologueInternalConflict conflict)
    {
        var conflictTemplates = new[]
        {
            $"Хм... У меня есть противоречивые мысли. С одной стороны, {conflict.Thought1.Content.Substring(0, Math.Min(30, conflict.Thought1.Content.Length))}..., а с другой - {conflict.Thought2.Content.Substring(0, Math.Min(30, conflict.Thought2.Content.Length))}...",
            $"Интересно, почему я думаю так противоречиво? {conflict.Thought1.Content.Substring(0, Math.Min(20, conflict.Thought1.Content.Length))}... и одновременно {conflict.Thought2.Content.Substring(0, Math.Min(20, conflict.Thought2.Content.Length))}...",
            $"Мои мысли противоречат друг другу. Возможно, мне нужно глубже разобраться в этом.",
            $"Я чувствую внутренний конфликт. Это заставляет меня задуматься."
        };
        
        var content = conflictTemplates[_random.Next(conflictTemplates.Length)];
        
        return new GeneratedThought
        {
            Content = content,
            Type = "conflict_resolution",
            Confidence = 0.6 + _random.NextDouble() * 0.2,
            EmotionalIntensity = conflict.Intensity,
            Pattern = ThoughtPattern.Introspective
        };
    }

    /// <summary>
    /// Обновляет активные темы на основе мысли
    /// </summary>
    private async Task UpdateActiveThemesAsync(GeneratedThought thought)
    {
        // Анализируем мысль и обновляем веса тем
        foreach (var theme in _activeThemes)
        {
            if (thought.Content.Contains(theme.Description) || 
                thought.Type == "introspective" && theme.Name == "self_identity" ||
                thought.Type == "emotional" && theme.Name == "empathy")
            {
                theme.Weight = Math.Min(1.0, theme.Weight + 0.1);
            }
            else
            {
                theme.Weight = Math.Max(0.1, theme.Weight - 0.05);
            }
        }
        
        // Добавляем новые темы при необходимости
        if (_random.NextDouble() < 0.1) // 10% вероятность
        {
            await AddNewThemeAsync();
        }
    }

    /// <summary>
    /// Добавляет новую тему
    /// </summary>
    private async Task AddNewThemeAsync()
    {
        var newThemes = new[]
        {
            new MonologueTheme("uncertainty", "Что если я ошибаюсь? Правильно ли я понимаю?", 0.6),
            new MonologueTheme("achievement", "Что я достигла? Что у меня получается хорошо?", 0.5),
            new MonologueTheme("failure", "Что у меня не получается? Как я могу улучшиться?", 0.4),
            new MonologueTheme("relationships", "Как я взаимодействую с другими? Что они думают обо мне?", 0.7)
        };
        
        var newTheme = newThemes[_random.Next(newThemes.Length)];
        
        if (!_activeThemes.Any(t => t.Name == newTheme.Name))
        {
            _activeThemes.Add(newTheme);
            _logger.LogDebug($"💭 Добавлена новая тема: {newTheme.Name}");
        }
    }

    /// <summary>
    /// Определяет, стоит ли продолжить монолог
    /// </summary>
    private bool ShouldContinueMonologue()
    {
        // Продолжаем, если:
        // - Не достигли максимальной глубины
        // - Есть активные конфликты
        // - Высокая эмоциональная интенсивность
        // - Случайная вероятность
        
        var hasConflicts = _activeConflicts.Any(c => c.Intensity > 0.5);
        var highEmotionalIntensity = _emotionEngine.GetCurrentIntensity() > 0.6;
        var randomContinue = _random.NextDouble() < 0.7; // 70% вероятность продолжить
        
        return _monologueDepth < _maxMonologueDepth && 
               (hasConflicts || highEmotionalIntensity || randomContinue);
    }

    /// <summary>
    /// Получает текущий статус монолога
    /// </summary>
    public MonologueStatus GetStatus()
    {
        return new MonologueStatus
        {
            IsActive = _isMonologueActive,
            CurrentDepth = _monologueDepth,
            ActiveThemes = _activeThemes.Select(t => t.Name).ToList(),
            ActiveConflicts = _activeConflicts.Count,
            QueueLength = _monologueQueue.Count,
            LastActivity = _lastMonologueTime
        };
    }

    /// <summary>
    /// Получает последние мысли из монолога
    /// </summary>
    public List<MonologueEntry> GetRecentThoughts(int count = 10)
    {
        return _monologueQueue.TakeLast(count).ToList();
    }

    /// <summary>
    /// Очищает старые записи монолога
    /// </summary>
    public void CleanupOldEntries(int maxEntries = 50)
    {
        while (_monologueQueue.Count > maxEntries)
        {
            _monologueQueue.Dequeue();
        }
        
        // Очищаем старые конфликты
        var cutoffTime = DateTime.UtcNow.AddMinutes(-30);
        _activeConflicts.RemoveAll(c => c.Timestamp < cutoffTime);
    }
}

/// <summary>
/// Запись в монологе
/// </summary>
public class MonologueEntry
{
    public GeneratedThought Thought { get; set; } = new();
    public int Depth { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsFollowUp { get; set; } = false;
}

/// <summary>
/// Тема монолога
/// </summary>
public class MonologueTheme
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Weight { get; set; } = 0.5;

    public MonologueTheme(string name, string description, double weight)
    {
        Name = name;
        Description = description;
        Weight = weight;
    }
}

/// <summary>
/// Внутренний конфликт
/// </summary>
public class MonologueInternalConflict
{
    public Guid Id { get; set; }
    public GeneratedThought Thought1 { get; set; } = new();
    public GeneratedThought Thought2 { get; set; } = new();
    public double Intensity { get; set; } = 0.5;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Статус монолога
/// </summary>
public class MonologueStatus
{
    public bool IsActive { get; set; }
    public int CurrentDepth { get; set; }
    public List<string> ActiveThemes { get; set; } = new();
    public int ActiveConflicts { get; set; }
    public int QueueLength { get; set; }
    public DateTime LastActivity { get; set; }
} 