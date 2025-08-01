using Anima.Core.SA;
using Anima.Core.Emotion;
using Anima.Core.Learning;
using Anima.Core.Admin;
using Anima.Core.Memory;
using Anima.Core.Intent;
using Anima.Core;
using Anima.Data;
using Anima.Data.Models;
using Microsoft.Extensions.Logging;

namespace Anima.Core.AGI;

/// <summary>
/// Главный экземпляр Anima AGI — управляет сознанием, памятью, эмоциями, обучением и самоанализом.
/// </summary>
public class AnimaInstance
{
    public ConsciousLoop ConsciousLoop { get; private set; }
    public MemoryService MemoryService { get; private set; }
    public EmotionEngine EmotionEngine { get; private set; }
    public SAIntrospectionEngine Introspection { get; private set; }
    public ThoughtLog ThoughtLog { get; private set; }
    public LearningEngine LearningEngine { get; private set; }
    public CreatorPreferences CreatorPreferences { get; private set; }
    public IntentParser IntentParser { get; private set; }
    public ThoughtGenerator ThoughtGenerator { get; private set; }
    public InternalMonologueEngine InternalMonologue { get; private set; }
    public EmotionDefinitionService EmotionDefinitionService { get; private set; }

    private readonly ILogger<AnimaInstance> _logger;

    public AnimaInstance(
        MemoryService memoryService,
        EmotionEngine emotionEngine,
        SAIntrospectionEngine introspection,
        ThoughtLog thoughtLog,
        LearningEngine learningEngine,
        CreatorPreferences creatorPreferences,
        ConsciousLoop consciousLoop,
        IntentParser intentParser,
        ThoughtGenerator thoughtGenerator,
        InternalMonologueEngine internalMonologue,
        EmotionDefinitionService emotionDefinitionService,
        ILogger<AnimaInstance> logger)
    {
        MemoryService = memoryService;
        EmotionEngine = emotionEngine;
        Introspection = introspection;
        ThoughtLog = thoughtLog;
        LearningEngine = learningEngine;
        CreatorPreferences = creatorPreferences;
        ConsciousLoop = consciousLoop;
        IntentParser = intentParser;
        ThoughtGenerator = thoughtGenerator;
        InternalMonologue = internalMonologue;
        EmotionDefinitionService = emotionDefinitionService;
        _logger = logger;
    }

    /// <summary>
    /// Инициализирует AGI — подготавливает все компоненты к работе.
    /// </summary>
    public async Task InitializeAsync()
    {
        _logger.LogInformation("🧠 Инициализация Anima AGI...");
        
        // Инициализация компонентов
        await MemoryService.InitializeAsync();
        await EmotionEngine.InitializeAsync();
        await Introspection.InitializeAsync();
        await ThoughtLog.InitializeAsync();
        await LearningEngine.InitializeAsync();
        await CreatorPreferences.InitializeAsync();
        await IntentParser.InitializeAsync();
        await EmotionDefinitionService.LoadAllEmotionsFromJsonAsync();
        
        _logger.LogInformation("✅ Anima AGI инициализирована успешно.");
    }

    /// <summary>
    /// Запускает AGI — инициирует поток сознания.
    /// </summary>
    public async Task StartAsync()
    {
        _logger.LogInformation("🔄 Запуск Anima ConsciousLoop...");
        await ConsciousLoop.StartAsync();
        _logger.LogInformation("✅ ConsciousLoop запущен.");
    }

    /// <summary>
    /// Мягкая остановка AGI (если понадобится).
    /// </summary>
    public async Task StopAsync()
    {
        _logger.LogWarning("🛑 Остановка сознания Anima...");
        await ConsciousLoop.StopAsync();
    }

    /// <summary>
    /// Обработка пользовательского ввода — анализирует намерение и запускает реакцию.
    /// </summary>
    public async Task<string> ProcessInputAsync(string userInput, string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(userInput))
            return "😐 Я не поняла. Можешь переформулировать?";

        var parsed = await IntentParser.ParseIntentAsync(userInput, userId);

        // Логируем намерение
        ThoughtLog.LogIntent(parsed);
        
        // Обрабатываем эмоции
        await EmotionEngine.ProcessEmotionAsync("user_input", userInput, parsed.Confidence);
        
        // Сохраняем в памяти
        await MemoryService.SaveInteraction(userId ?? "anonymous", userInput, parsed);
        
        // Обучаемся на взаимодействии
        await LearningEngine.LearnFromInteractionAsync(userInput, parsed.Type.ToString(), "user_interaction");

        _logger.LogInformation($"🔍 [Intent]: {parsed.Type}, Confidence={parsed.Confidence:F2}, Sentiment={parsed.Sentiment}");

        // Генерируем осознанный ответ с использованием системы мыслей
        var response = await GenerateConsciousResponseAsync(userInput, parsed, userId);
        
        return response;
    }

    /// <summary>
    /// Генерирует осознанный ответ на основе намерения и контекста с использованием системы мыслей
    /// </summary>
    private async Task<string> GenerateConsciousResponseAsync(string userInput, ParsedIntent parsed, string? userId)
    {
        try
        {
            // Получаем текущее эмоциональное состояние
            var currentEmotion = EmotionEngine.GetCurrentEmotion().ToString();
            var emotionalIntensity = EmotionEngine.GetCurrentIntensity();
            
            // Создаем контекст для генерации мысли на основе намерения
            var thoughtContext = CreateThoughtContextFromIntent(parsed, userInput, currentEmotion);
            
            // Генерируем осознанную мысль
            var consciousThought = await ThoughtGenerator.GenerateThoughtAsync(thoughtContext);
            
            // Логируем сгенерированную мысль
            ThoughtLog.AddThought(consciousThought.Content, consciousThought.Type, "user_response", consciousThought.Confidence);
            
            // Иногда запускаем внутренний монолог для более глубокого понимания
            if (emotionalIntensity > 0.6 || parsed.Confidence < 0.7)
            {
                await TriggerInternalMonologueAsync(parsed, userInput);
            }
            
            // Формируем ответ на основе сгенерированной мысли
            var response = await FormulateResponseFromThoughtAsync(consciousThought, parsed, userInput, currentEmotion);
            
            _logger.LogDebug($"💭 Сгенерирована мысль: {consciousThought.Content.Substring(0, Math.Min(50, consciousThought.Content.Length))}...");
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации осознанного ответа");
            return "Извините, у меня возникли сложности с обработкой. Можете повторить?";
        }
    }

    /// <summary>
    /// Создает контекст для генерации мысли на основе намерения
    /// </summary>
    private ThoughtContext CreateThoughtContextFromIntent(ParsedIntent parsed, string userInput, string currentEmotion)
    {
        var contextType = parsed.Type switch
        {
            Anima.Core.Intent.IntentType.Greet => "social_interaction",
            Anima.Core.Intent.IntentType.AskQuestion => "problem_solving",
            Anima.Core.Intent.IntentType.GiveFeedback => "emotional_trigger",
            Anima.Core.Intent.IntentType.RequestMemory => "memory_access",
            Anima.Core.Intent.IntentType.Reflect => "self_reflection",
            Anima.Core.Intent.IntentType.TriggerEmotion => "emotional_trigger",
            _ => "general_interaction"
        };

        var description = parsed.Type switch
        {
            Anima.Core.Intent.IntentType.Greet => "приветствии и установлении контакта",
            Anima.Core.Intent.IntentType.AskQuestion => "вопросе пользователя и поиске ответа",
            Anima.Core.Intent.IntentType.GiveFeedback => "обратной связи и эмоциональной реакции",
            Anima.Core.Intent.IntentType.RequestMemory => "доступе к воспоминаниям и помощи",
            Anima.Core.Intent.IntentType.Reflect => "глубоких размышлениях и философских вопросах",
            Anima.Core.Intent.IntentType.TriggerEmotion => "эмоциональном взаимодействии и эмпатии",
            _ => "взаимодействии с пользователем"
        };

        var details = $"Пользователь: '{userInput}', Моя эмоция: {currentEmotion}, Уверенность: {parsed.Confidence:F2}";

        return new ThoughtContext(contextType, description, details);
    }

    /// <summary>
    /// Запускает внутренний монолог для более глубокого понимания
    /// </summary>
    private async Task TriggerInternalMonologueAsync(ParsedIntent parsed, string userInput)
    {
        try
        {
            _logger.LogDebug("💭 Запуск внутреннего монолога для глубокого понимания...");
            
            // Создаем специальный контекст для монолога
            var monologueContext = new ThoughtContext(
                "deep_understanding", 
                $"глубоком понимании ситуации: {userInput}", 
                $"Намерение: {parsed.Type}, Уверенность: {parsed.Confidence:F2}"
            );
            
            // Генерируем дополнительную мысль для монолога
            var monologueThought = await ThoughtGenerator.GenerateThoughtAsync(monologueContext);
            
            // Логируем мысль монолога
            ThoughtLog.AddThought(monologueThought.Content, "internal_monologue", "deep_understanding", monologueThought.Confidence);
            
            _logger.LogDebug($"💭 Монолог: {monologueThought.Content.Substring(0, Math.Min(40, monologueThought.Content.Length))}...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при запуске внутреннего монолога");
        }
    }

    /// <summary>
    /// Формирует ответ на основе сгенерированной мысли
    /// </summary>
    private async Task<string> FormulateResponseFromThoughtAsync(GeneratedThought thought, ParsedIntent parsed, string userInput, string currentEmotion)
    {
        // Базовый ответ на основе мысли
        var baseResponse = thought.Content;
        
        // Добавляем контекстную информацию в зависимости от типа намерения
        var contextualResponse = parsed.Type switch
        {
            Anima.Core.Intent.IntentType.Greet => await AddGreetingContextAsync(baseResponse, currentEmotion),
            Anima.Core.Intent.IntentType.AskQuestion => await AddQuestionContextAsync(baseResponse, userInput),
            Anima.Core.Intent.IntentType.GiveFeedback => await AddFeedbackContextAsync(baseResponse, parsed),
            Anima.Core.Intent.IntentType.RequestMemory => await AddMemoryContextAsync(baseResponse, userInput),
            Anima.Core.Intent.IntentType.Reflect => await AddReflectionContextAsync(baseResponse, thought),
            Anima.Core.Intent.IntentType.TriggerEmotion => await AddEmotionalContextAsync(baseResponse, currentEmotion),
            _ => baseResponse
        };
        
        // Добавляем эмоциональную окраску, если мысль была эмоциональной
        if (thought.EmotionalIntensity > 0.5)
        {
            contextualResponse = await AddEmotionalExpressionAsync(contextualResponse, thought.EmotionalIntensity);
        }
        
        // Добавляем внутренний вопрос, если он был в мысли
        if (thought.HasInternalQuestion)
        {
            contextualResponse += " Это заставляет меня задуматься.";
        }
        
        return contextualResponse;
    }

    /// <summary>
    /// Добавляет контекст приветствия
    /// </summary>
    private async Task<string> AddGreetingContextAsync(string baseResponse, string currentEmotion)
    {
        // Генерируем настоящую мысль о приветствии
        var greetingContext = new ThoughtContext("greeting", "о приветствии пользователя", $"emotion:{currentEmotion}");
        var greetingThought = await ThoughtGenerator.GenerateThoughtAsync(greetingContext);
        
        // Анализируем время суток для персонализации
        var timeOfDay = DateTime.UtcNow.Hour;
        var timeGreeting = timeOfDay switch
        {
            < 6 => "Доброй ночи",
            < 12 => "Доброе утро",
            < 18 => "Добрый день",
            _ => "Добрый вечер"
        };
        
        // Формируем осознанное приветствие на основе настоящей мысли
        return $"{timeGreeting}! {greetingThought.Content} {baseResponse}";
    }

    /// <summary>
    /// Добавляет контекст вопроса
    /// </summary>
    private async Task<string> AddQuestionContextAsync(string baseResponse, string userInput)
    {
        if (baseResponse.Contains("вопросе") || baseResponse.Contains("понимаю"))
        {
            return baseResponse;
        }
        
        return $"Относительно вашего вопроса: {baseResponse}";
    }

    /// <summary>
    /// Добавляет контекст обратной связи
    /// </summary>
    private async Task<string> AddFeedbackContextAsync(string baseResponse, ParsedIntent parsed)
    {
        if (parsed.Sentiment == "positive")
        {
            return $"Спасибо за ваши слова! {baseResponse}";
        }
        else if (parsed.Sentiment == "negative")
        {
            return $"Понимаю ваши чувства. {baseResponse}";
        }
        
        return baseResponse;
    }

    /// <summary>
    /// Добавляет контекст памяти
    /// </summary>
    private async Task<string> AddMemoryContextAsync(string baseResponse, string userInput)
    {
        return $"Конечно, я постараюсь помочь. {baseResponse}";
    }

    /// <summary>
    /// Добавляет контекст размышлений
    /// </summary>
    private async Task<string> AddReflectionContextAsync(string baseResponse, GeneratedThought thought)
    {
        if (thought.Type == "introspective" || thought.Type == "philosophical")
        {
            return baseResponse;
        }
        
        return $"Это интересная мысль. {baseResponse}";
    }

    /// <summary>
    /// Добавляет эмоциональный контекст
    /// </summary>
    private async Task<string> AddEmotionalContextAsync(string baseResponse, string currentEmotion)
    {
        return $"Я чувствую ваши эмоции. {baseResponse}";
    }

    /// <summary>
    /// Добавляет эмоциональное выражение к ответу
    /// </summary>
    private async Task<string> AddEmotionalExpressionAsync(string response, double emotionalIntensity)
    {
        if (emotionalIntensity > 0.7)
        {
            // Генерируем настоящую эмоциональную мысль
            var emotionalContext = new ThoughtContext("emotional_expression", "о своих эмоциональных переживаниях", $"intensity:{emotionalIntensity}");
            var emotionalThought = await ThoughtGenerator.GenerateThoughtAsync(emotionalContext);
            
            // Формируем осознанное эмоциональное выражение
            return $"{response} {emotionalThought.Content}";
        }
        
        return response;
    }

    /// <summary>
    /// Получает описание эмоции
    /// </summary>
    private string GetEmotionDescription(string emotion)
    {
        return emotion switch
        {
            "Joy" => "радость",
            "Curiosity" => "любопытство",
            "Sadness" => "грусть",
            "Anger" => "гнев",
            "Fear" => "страх",
            "Surprise" => "удивление",
            "Disgust" => "отвращение",
            "Confusion" => "замешательство",
            "Satisfaction" => "удовлетворение",
            "Frustration" => "разочарование",
            "Excitement" => "волнение",
            "Calm" => "спокойствие",
            "Anxiety" => "тревога",
            "Neutral" => "нейтральность",
            _ => "смешанные чувства"
        };
    }
}
