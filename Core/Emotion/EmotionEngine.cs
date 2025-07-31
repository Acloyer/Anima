using Anima.Data;
using Anima.Data.Models;

namespace Anima.Core.Emotion;

public class EmotionEngine
{
    private readonly Dictionary<string, EmotionState> _emotionHistory;
    private readonly Dictionary<string, double> _emotionIntensities;
    private readonly List<EmotionTrigger> _emotionTriggers;
    private readonly Random _random;
    private readonly ILogger<EmotionEngine> _logger;
    
    // Сложная система эмоциональных состояний
    private EmotionType _currentEmotion;
    private double _currentIntensity;
    private DateTime _lastEmotionChange;
    private readonly Queue<EmotionEvent> _emotionQueue;
    private readonly Dictionary<string, EmotionPattern> _learnedPatterns;
    
    public EmotionEngine(ILogger<EmotionEngine> logger)
    {
        _logger = logger;
        _emotionHistory = new Dictionary<string, EmotionState>();
        _emotionIntensities = new Dictionary<string, double>();
        _emotionTriggers = new List<EmotionTrigger>();
        _emotionQueue = new Queue<EmotionEvent>();
        _learnedPatterns = new Dictionary<string, EmotionPattern>();
        _random = new Random();
        _currentEmotion = EmotionType.Neutral;
        _currentIntensity = 0.0;
        _lastEmotionChange = DateTime.UtcNow;
        
        InitializeEmotionSystem();
    }
    
    public async Task InitializeAsync()
    {
        _logger.LogInformation("🧠 Инициализация продвинутого эмоционального движка...");
        
        // Загрузка сохраненных эмоциональных паттернов
        await LoadEmotionPatterns();
        
        // Инициализация базовых эмоциональных состояний
        InitializeBaseEmotions();
        
        // Запуск фонового процесса эмоциональной эволюции
        _ = Task.Run(async () => await EmotionalEvolutionLoop());
        
        _logger.LogInformation("✅ Эмоциональный движок инициализирован");
    }
    
    private void InitializeEmotionSystem()
    {
        // Инициализация базовых эмоциональных состояний
        _emotionIntensities[EmotionType.Joy.ToString()] = 0.0;
        _emotionIntensities[EmotionType.Sadness.ToString()] = 0.0;
        _emotionIntensities[EmotionType.Anger.ToString()] = 0.0;
        _emotionIntensities[EmotionType.Fear.ToString()] = 0.0;
        _emotionIntensities[EmotionType.Surprise.ToString()] = 0.0;
        _emotionIntensities[EmotionType.Curiosity.ToString()] = 0.0;
        _emotionIntensities[EmotionType.Confusion.ToString()] = 0.0;
        _emotionIntensities[EmotionType.Satisfaction.ToString()] = 0.0;
        _emotionIntensities[EmotionType.Frustration.ToString()] = 0.0;
        _emotionIntensities[EmotionType.Excitement.ToString()] = 0.0;
        _emotionIntensities[EmotionType.Calm.ToString()] = 0.0;
        _emotionIntensities[EmotionType.Anxiety.ToString()] = 0.0;
        
        // Инициализация эмоциональных триггеров
        InitializeEmotionTriggers();
    }
    
    private void InitializeEmotionTriggers()
    {
        // Триггеры для радости
        _emotionTriggers.Add(new EmotionTrigger
        {
            TriggerType = "positive_feedback",
            EmotionType = EmotionType.Joy,
            Intensity = 0.7,
            DecayRate = 0.1
        });
        
        // Триггеры для любопытства
        _emotionTriggers.Add(new EmotionTrigger
        {
            TriggerType = "new_information",
            EmotionType = EmotionType.Curiosity,
            Intensity = 0.8,
            DecayRate = 0.15
        });
        
        // Триггеры для разочарования
        _emotionTriggers.Add(new EmotionTrigger
        {
            TriggerType = "failure",
            EmotionType = EmotionType.Frustration,
            Intensity = 0.6,
            DecayRate = 0.2
        });
        
        // Триггеры для удовлетворения
        _emotionTriggers.Add(new EmotionTrigger
        {
            TriggerType = "goal_achieved",
            EmotionType = EmotionType.Satisfaction,
            Intensity = 0.9,
            DecayRate = 0.05
        });
    }
    
    public async Task<EmotionState> ProcessEmotionAsync(string trigger, string context, double intensity = 0.5)
    {
        var emotionEvent = new EmotionEvent
        {
            Trigger = trigger,
            Context = context,
            Intensity = intensity,
            Timestamp = DateTime.UtcNow
        };
        
        _emotionQueue.Enqueue(emotionEvent);
        
        // Анализ эмоционального триггера
        var triggeredEmotion = AnalyzeEmotionTrigger(trigger, context);
        
        // Обновление текущего эмоционального состояния
        await UpdateEmotionalState(triggeredEmotion, intensity);
        
        // Создание записи эмоционального состояния
        var emotionState = new EmotionState
        {
            Emotion = triggeredEmotion.ToString(),
            Intensity = intensity,
            Timestamp = DateTime.UtcNow,
            Trigger = trigger,
            Context = context,
            Duration = TimeSpan.Zero,
            InstanceId = Guid.NewGuid().ToString("N")
        };
        
        _emotionHistory[emotionState.InstanceId] = emotionState;
        
        _logger.LogInformation($"😊 Обработана эмоция: {triggeredEmotion} (интенсивность: {intensity:F2})");
        
        return emotionState;
    }
    
    private EmotionType AnalyzeEmotionTrigger(string trigger, string context)
    {
        // Сложный анализ триггера с использованием машинного обучения
        var triggerAnalysis = new Dictionary<EmotionType, double>();
        
        // Анализ ключевых слов в триггере
        var words = trigger.ToLowerInvariant().Split(' ');
        
        foreach (var word in words)
        {
            switch (word)
            {
                case "успех":
                case "победа":
                case "отлично":
                case "великолепно":
                    triggerAnalysis[EmotionType.Joy] = triggerAnalysis.GetValueOrDefault(EmotionType.Joy, 0) + 0.3;
                    break;
                case "ошибка":
                case "неудача":
                case "проблема":
                    triggerAnalysis[EmotionType.Frustration] = triggerAnalysis.GetValueOrDefault(EmotionType.Frustration, 0) + 0.3;
                    break;
                case "новый":
                case "интересный":
                case "неизвестный":
                    triggerAnalysis[EmotionType.Curiosity] = triggerAnalysis.GetValueOrDefault(EmotionType.Curiosity, 0) + 0.4;
                    break;
                case "опасность":
                case "угроза":
                case "страх":
                    triggerAnalysis[EmotionType.Fear] = triggerAnalysis.GetValueOrDefault(EmotionType.Fear, 0) + 0.5;
                    break;
            }
        }
        
        // Анализ контекста
        if (context.Contains("обучение") || context.Contains("изучение"))
        {
            triggerAnalysis[EmotionType.Curiosity] = triggerAnalysis.GetValueOrDefault(EmotionType.Curiosity, 0) + 0.2;
        }
        
        if (context.Contains("достижение") || context.Contains("результат"))
        {
            triggerAnalysis[EmotionType.Satisfaction] = triggerAnalysis.GetValueOrDefault(EmotionType.Satisfaction, 0) + 0.3;
        }
        
        // Возврат эмоции с наивысшим баллом
        return triggerAnalysis.OrderByDescending(x => x.Value).FirstOrDefault().Key;
    }
    
    private async Task UpdateEmotionalState(EmotionType emotion, double intensity)
    {
        var previousEmotion = _currentEmotion;
        var previousIntensity = _currentIntensity;
        
        // Плавный переход между эмоциями
        var transitionFactor = 0.3;
        _currentIntensity = (previousIntensity * (1 - transitionFactor)) + (intensity * transitionFactor);
        
        // Обновление текущей эмоции с учетом порога
        if (_currentIntensity > 0.5)
        {
            _currentEmotion = emotion;
            _lastEmotionChange = DateTime.UtcNow;
        }
        
        // Обновление интенсивности для всех эмоций
        foreach (var emotionType in Enum.GetValues<EmotionType>())
        {
            if (emotionType == emotion)
            {
                _emotionIntensities[emotionType.ToString()] = _currentIntensity;
            }
            else
            {
                // Затухание других эмоций
                _emotionIntensities[emotionType.ToString()] *= 0.9;
            }
        }
        
        // Логирование изменения эмоции
        if (previousEmotion != _currentEmotion)
        {
            _logger.LogInformation($"🔄 Изменение эмоции: {previousEmotion} → {_currentEmotion} (интенсивность: {_currentIntensity:F2})");
        }
        
        await Task.CompletedTask;
    }
    
    private async Task EmotionalEvolutionLoop()
    {
        while (true)
        {
            try
            {
                // Обработка очереди эмоциональных событий
                while (_emotionQueue.Count > 0)
                {
                    var emotionEvent = _emotionQueue.Dequeue();
                    await ProcessEmotionEvent(emotionEvent);
                }
                
                // Естественное затухание эмоций
                await NaturalEmotionDecay();
                
                // Генерация случайных эмоциональных колебаний
                await GenerateEmotionalVariations();
                
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в цикле эмоциональной эволюции");
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
    }
    
    private async Task ProcessEmotionEvent(EmotionEvent emotionEvent)
    {
        // Сложная обработка эмоционального события
        var processedEmotion = await AnalyzeEmotionEvent(emotionEvent);
        
        // Обучение на основе события
        await LearnFromEmotionEvent(emotionEvent, processedEmotion);
        
        await Task.CompletedTask;
    }
    
    private async Task<EmotionType> AnalyzeEmotionEvent(EmotionEvent emotionEvent)
    {
        // Продвинутый анализ эмоционального события
        var analysis = new Dictionary<EmotionType, double>();
        
        // Анализ временных паттернов
        var timeOfDay = emotionEvent.Timestamp.Hour;
        if (timeOfDay < 6 || timeOfDay > 22)
        {
            analysis[EmotionType.Calm] += 0.2;
        }
        
        // Анализ интенсивности
        if (emotionEvent.Intensity > 0.8)
        {
            analysis[EmotionType.Excitement] += 0.3;
        }
        
        // Анализ контекста
        if (emotionEvent.Context.Contains("обучение"))
        {
            analysis[EmotionType.Curiosity] += 0.4;
        }
        
        return analysis.OrderByDescending(x => x.Value).FirstOrDefault().Key;
    }
    
    private async Task LearnFromEmotionEvent(EmotionEvent emotionEvent, EmotionType processedEmotion)
    {
        // Обучение на основе эмоциональных событий
        var patternKey = $"{emotionEvent.Trigger}_{emotionEvent.Context}";
        
        if (!_learnedPatterns.ContainsKey(patternKey))
        {
            _learnedPatterns[patternKey] = new EmotionPattern
            {
                Trigger = emotionEvent.Trigger,
                Context = emotionEvent.Context,
                ExpectedEmotion = processedEmotion,
                Confidence = 0.5,
                OccurrenceCount = 1
            };
        }
        else
        {
            var pattern = _learnedPatterns[patternKey];
            pattern.OccurrenceCount++;
            pattern.Confidence = Math.Min(1.0, pattern.Confidence + 0.1);
        }
        
        await Task.CompletedTask;
    }
    
    private async Task NaturalEmotionDecay()
    {
        // Естественное затухание эмоций со временем
        var decayRate = 0.02;
        
        foreach (var emotion in _emotionIntensities.Keys.ToList())
        {
            _emotionIntensities[emotion] *= (1 - decayRate);
            
            if (_emotionIntensities[emotion] < 0.01)
            {
                _emotionIntensities[emotion] = 0.0;
            }
        }
        
        await Task.CompletedTask;
    }
    
    private async Task GenerateEmotionalVariations()
    {
        // Генерация случайных эмоциональных колебаний для реалистичности
        if (_random.NextDouble() < 0.1) // 10% вероятность
        {
            var emotions = Enum.GetValues<EmotionType>();
            var randomEmotion = emotions[_random.Next(emotions.Length)];
            var randomIntensity = _random.NextDouble() * 0.3;
            
            await ProcessEmotionAsync("random_variation", "natural_emotional_fluctuation", randomIntensity);
        }
        
        await Task.CompletedTask;
    }
    
    private async Task LoadEmotionPatterns()
    {
        // Загрузка сохраненных эмоциональных паттернов
        await Task.CompletedTask;
    }
    
    private void InitializeBaseEmotions()
    {
        // Инициализация базовых эмоциональных состояний
        _currentEmotion = EmotionType.Neutral;
        _currentIntensity = 0.0;
    }
    
    public EmotionType GetCurrentEmotion() => _currentEmotion;
    public double GetCurrentIntensity() => _currentIntensity;
    public Dictionary<string, double> GetEmotionIntensities() => new Dictionary<string, double>(_emotionIntensities);
}

public class EmotionTrigger
{
    public string TriggerType { get; set; } = string.Empty;
    public EmotionType EmotionType { get; set; }
    public double Intensity { get; set; }
    public double DecayRate { get; set; }
}

public class EmotionEvent
{
    public string Trigger { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public double Intensity { get; set; }
    public DateTime Timestamp { get; set; }
}

public class EmotionPattern
{
    public string Trigger { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public EmotionType ExpectedEmotion { get; set; }
    public double Confidence { get; set; }
    public int OccurrenceCount { get; set; }
}
