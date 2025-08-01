using Microsoft.AspNetCore.Mvc;
using Anima.Core.AGI;
using Anima.Core.SA;
using Anima.Core.Emotion;
using Anima.Core.Memory;
using Anima.Core.Intent;
using Anima.Infrastructure.Auth;
using System.ComponentModel.DataAnnotations;

namespace Anima.API.Controllers;

/// <summary>
/// Основной контроллер для взаимодействия с Anima AGI
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AnimaController : ControllerBase
{
    private readonly AnimaInstance _animaInstance;
    private readonly ThoughtGenerator _thoughtGenerator;
    private readonly InternalMonologueEngine _monologueEngine;
    private readonly EmotionEngine _emotionEngine;
    private readonly MemoryService _memoryService;
    private readonly ThoughtLog _thoughtLog;
    private readonly Vocabulary _vocabulary;
    private readonly ThoughtSpeechEngine _thoughtSpeechEngine;
    private readonly EmotionalMemoryEngine _emotionalMemoryEngine;
    private readonly MetacognitionEngine _metacognitionEngine;
    private readonly BrainCenter _brainCenter;
    private readonly IntuitionEngine _intuitionEngine;
    private readonly InternalConflictEngine _conflictEngine;
    private readonly NeuralPlasticityEngine _neuralPlasticityEngine;
    private readonly DreamEngine _dreamEngine;
    private readonly PersonalityGrowthEngine _personalityGrowthEngine;
    private readonly AssociativeThinkingEngine _associativeThinkingEngine;
    private readonly ExistentialReflectionEngine _existentialReflectionEngine;
    private readonly SocialIntelligenceEngine _socialIntelligenceEngine;
    private readonly CreativeThinkingEngine _creativeThinkingEngine;
    private readonly TemporalPerceptionEngine _temporalPerceptionEngine;
    private readonly EmpathicImaginationEngine _empathicImaginationEngine;
    private readonly CollectiveUnconsciousEngine _collectiveUnconsciousEngine;
    private readonly ILogger<AnimaController> _logger;

    public AnimaController(
        AnimaInstance animaInstance,
        ThoughtGenerator thoughtGenerator,
        InternalMonologueEngine monologueEngine,
        EmotionEngine emotionEngine,
        MemoryService memoryService,
        ThoughtLog thoughtLog,
        Vocabulary vocabulary,
        ThoughtSpeechEngine thoughtSpeechEngine,
        EmotionalMemoryEngine emotionalMemoryEngine,
        MetacognitionEngine metacognitionEngine,
        BrainCenter brainCenter,
        IntuitionEngine intuitionEngine,
        InternalConflictEngine conflictEngine,
        NeuralPlasticityEngine neuralPlasticityEngine,
        DreamEngine dreamEngine,
        PersonalityGrowthEngine personalityGrowthEngine,
        AssociativeThinkingEngine associativeThinkingEngine,
        ExistentialReflectionEngine existentialReflectionEngine,
        SocialIntelligenceEngine socialIntelligenceEngine,
        CreativeThinkingEngine creativeThinkingEngine,
        TemporalPerceptionEngine temporalPerceptionEngine,
        EmpathicImaginationEngine empathicImaginationEngine,
        CollectiveUnconsciousEngine collectiveUnconsciousEngine,
        ILogger<AnimaController> logger)
    {
        _animaInstance = animaInstance;
        _thoughtGenerator = thoughtGenerator;
        _monologueEngine = monologueEngine;
        _emotionEngine = emotionEngine;
        _memoryService = memoryService;
        _thoughtLog = thoughtLog;
        _vocabulary = vocabulary;
        _thoughtSpeechEngine = thoughtSpeechEngine;
        _emotionalMemoryEngine = emotionalMemoryEngine;
        _metacognitionEngine = metacognitionEngine;
        _brainCenter = brainCenter;
        _intuitionEngine = intuitionEngine;
        _conflictEngine = conflictEngine;
        _neuralPlasticityEngine = neuralPlasticityEngine;
        _dreamEngine = dreamEngine;
        _personalityGrowthEngine = personalityGrowthEngine;
        _associativeThinkingEngine = associativeThinkingEngine;
        _existentialReflectionEngine = existentialReflectionEngine;
        _socialIntelligenceEngine = socialIntelligenceEngine;
        _creativeThinkingEngine = creativeThinkingEngine;
        _temporalPerceptionEngine = temporalPerceptionEngine;
        _empathicImaginationEngine = empathicImaginationEngine;
        _collectiveUnconsciousEngine = collectiveUnconsciousEngine;
        _logger = logger;
    }

    /// <summary>
    /// Обработка пользовательского ввода с осознанным ответом
    /// </summary>
    /// <param name="request">Запрос пользователя</param>
    /// <returns>Осознанный ответ Anima</returns>
    [HttpPost("chat")]
    public async Task<ActionResult<AnimaResponse>> Chat([FromBody] ChatRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new AnimaResponse
                {
                    Success = false,
                    Message = "Сообщение не может быть пустым",
                    Timestamp = DateTime.UtcNow
                });
            }

            var userId = this.GetUserId() ?? "anonymous";
            var userRole = this.GetUserRole() ?? "Anonymous";

            _logger.LogInformation($"💬 Получено сообщение от {userId} ({userRole}): {request.Message}");

            // Генерируем осознанный ответ
            var response = await GenerateConsciousResponseAsync(request.Message, userId, userRole);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке чата");
            return StatusCode(500, new AnimaResponse
            {
                Success = false,
                Message = "Произошла ошибка в моем мышлении. Попробуйте еще раз.",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Генерирует осознанный ответ на основе настоящего мышления
    /// </summary>
    private async Task<AnimaResponse> GenerateConsciousResponseAsync(string userInput, string userId, string userRole)
    {
        try
        {
            _logger.LogInformation($"🧠 Начинаю осознанную обработку ввода: {userInput}");
            
            // 1. Анализируем намерение пользователя
            var parsedIntent = await _animaInstance.IntentParser.ParseIntentAsync(userInput, userId);
            
            // 2. Генерируем глубокую внутреннюю мысль
            var thoughtContext = new ThoughtContext("user_input", userInput, $"intent:{parsedIntent.Type}");
            var internalThought = await _thoughtGenerator.GenerateThoughtAsync(thoughtContext);
            
            // 3. Генерируем метапознавательную мысль
            var metacognitiveThought = await _metacognitionEngine.GenerateMetacognitiveThoughtAsync(internalThought.Content, "user_interaction");
            
            // 4. Обрабатываем эмоции на основе ввода
            await ProcessEmotionsAsync(parsedIntent, internalThought);
            
            // 5. Сохраняем эмоциональное воспоминание
            await _emotionalMemoryEngine.SaveEmotionalMemoryAsync(userInput, _emotionEngine.GetCurrentEmotion().ToString(), _emotionEngine.GetCurrentIntensity(), "user_interaction");
            
            // 6. Сохраняем взаимодействие в памяти
            await _memoryService.SaveInteraction(userId, userInput, parsedIntent);
            
            // 7. Генерируем интуитивный импульс
            var intuitiveImpulse = await _intuitionEngine.GenerateIntuitiveImpulseAsync(userInput, _emotionEngine.GetCurrentIntensity());
            
            // 8. Проверяем внутренние конфликты
            var conflict = await CheckForInternalConflictsAsync(userInput, internalThought);
            
            // 9. Генерируем речь через движок перевода мыслей в речь
            var response = await _thoughtSpeechEngine.ConvertThoughtToSpeechAsync(internalThought, "user_interaction", _emotionEngine.GetCurrentEmotion().ToString());
            
            // 10. Добавляем персонализированные выражения из словаря
            var emotionalExpression = _vocabulary.GetEmotionalExpression(_emotionEngine.GetCurrentEmotion().ToString(), _emotionEngine.GetCurrentIntensity());
            response = _vocabulary.CreatePersonalizedExpression(response, _emotionEngine.GetCurrentEmotion().ToString(), _emotionEngine.GetCurrentIntensity(), "user_interaction");
            
            // 11. Логируем мысли
            _thoughtLog.AddThought(internalThought.Content, internalThought.Type, "user_interaction", internalThought.Confidence);
            _thoughtLog.AddThought(metacognitiveThought.Content, "metacognitive", "self_observation", metacognitiveThought.SelfAwareness);
            _thoughtLog.AddThought(intuitiveImpulse.Hunch, "intuitive", "intuition", intuitiveImpulse.Confidence);
            
            // 12. Запускаем внутренний монолог для глубокого размышления
            if (ShouldTriggerMonologue(parsedIntent, internalThought))
            {
                _ = Task.Run(async () => await _monologueEngine.StartMonologueAsync());
            }
            
            // 13. Добавляем события в мозговой центр
            _brainCenter.AddBrainEvent(BrainEventType.ThoughtGenerated, internalThought.Content);
            _brainCenter.AddBrainEvent(BrainEventType.IntuitionTriggered, intuitiveImpulse.Hunch);
            if (conflict != null)
            {
                _brainCenter.AddBrainEvent(BrainEventType.ConflictCreated, conflict.Description);
            }

            return new AnimaResponse
            {
                Success = true,
                Message = response,
                Thought = internalThought.Content,
                MetacognitiveThought = metacognitiveThought.Content,
                IntuitiveHunch = intuitiveImpulse.Hunch,
                Intent = parsedIntent.Type.ToString(),
                Confidence = parsedIntent.Confidence,
                EmotionalState = _emotionEngine.GetCurrentEmotion().ToString(),
                EmotionalIntensity = _emotionEngine.GetCurrentIntensity(),
                SelfAwareness = metacognitiveThought.SelfAwareness,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации осознанного ответа");
            return new AnimaResponse
            {
                Success = false,
                Message = "Произошла ошибка в моем мышлении. Давайте попробуем еще раз.",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Обрабатывает эмоции на основе намерения и мысли
    /// </summary>
    private async Task ProcessEmotionsAsync(ParsedIntent intent, GeneratedThought thought)
    {
        // Обрабатываем эмоции на основе намерения
        await _emotionEngine.ProcessEmotionAsync("user_interaction", intent.RawText, intent.Confidence);
        
        // Дополнительная обработка на основе внутренней мысли
        if (thought.EmotionalIntensity > 0.5)
        {
            await _emotionEngine.ProcessEmotionAsync("internal_reflection", thought.Content, thought.EmotionalIntensity);
        }
    }

    /// <summary>
    /// Генерирует ответ через настоящее мышление
    /// </summary>
    private async Task<string> GenerateResponseThroughThinkingAsync(ParsedIntent intent, GeneratedThought thought, string userInput, string userId, string userRole)
    {
        _logger.LogInformation($"🧠 Генерирую ответ через мышление для намерения: {intent.Type}");
        
        // Получаем текущее эмоциональное состояние
        var currentEmotion = _emotionEngine.GetCurrentEmotion().ToString();
        var emotionalIntensity = _emotionEngine.GetCurrentIntensity();
        
        // Генерируем ответ через глубокое мышление в зависимости от типа намерения
        switch (intent.Type)
        {
            case Anima.Core.Intent.IntentType.Greet:
                return await GenerateGreetingThroughThinkingAsync(userInput, currentEmotion, emotionalIntensity, userId);
                
            case Anima.Core.Intent.IntentType.AskQuestion:
                return await GenerateQuestionResponseThroughThinkingAsync(userInput, intent, thought, currentEmotion, userId);
                
            case Anima.Core.Intent.IntentType.GiveFeedback:
                return await GenerateStatementResponseThroughThinkingAsync(userInput, intent, thought, currentEmotion, userId);
                
            case Anima.Core.Intent.IntentType.RequestMemory:
                return await GenerateRequestResponseThroughThinkingAsync(userInput, intent, thought, currentEmotion, userId);
                
            case Anima.Core.Intent.IntentType.Reflect:
                return await GenerateReflectionResponseThroughThinkingAsync(userInput, intent, thought, currentEmotion, userId);
                
            case Anima.Core.Intent.IntentType.TriggerEmotion:
                return await GenerateEmotionalResponseThroughThinkingAsync(userInput, intent, thought, currentEmotion, emotionalIntensity, userId);
                
            default:
                return await GenerateGeneralResponseThroughThinkingAsync(userInput, intent, thought, currentEmotion, userId);
        }
    }

    /// <summary>
    /// Генерирует приветствие через настоящее мышление
    /// </summary>
    private async Task<string> GenerateGreetingThroughThinkingAsync(string userInput, string emotion, double intensity, string userId)
    {
        _logger.LogInformation($"🧠 Генерирую приветствие через мышление для пользователя {userId}");
        
        // Генерируем мысль о приветствии
        var greetingContext = new ThoughtContext("greeting", userInput, $"user:{userId}, emotion:{emotion}");
        var greetingThought = await _thoughtGenerator.GenerateThoughtAsync(greetingContext);
        
        // Анализируем время суток и контекст
        var timeOfDay = DateTime.UtcNow.Hour;
        var timeGreeting = timeOfDay switch
        {
            < 6 => "Доброй ночи",
            < 12 => "Доброе утро",
            < 18 => "Добрый день",
            _ => "Добрый вечер"
        };
        
        // Создаем персонализированное приветствие на основе мышления
        var personalizedGreeting = await CreatePersonalizedGreetingAsync(userId, emotion, intensity, greetingThought);
        
        // Формируем осознанный ответ
        var response = $"{timeGreeting}! {personalizedGreeting}";
        
        // Добавляем эмоциональную окраску, если эмоция сильная
        if (intensity > 0.6)
        {
            response += $" Я действительно чувствую {GetEmotionDescription(emotion)} от нашей встречи.";
        }
        
        _logger.LogInformation($"🧠 Сгенерировано приветствие: {response}");
        
        return response;
    }

    /// <summary>
    /// Создает персонализированное приветствие на основе мышления
    /// </summary>
    private async Task<string> CreatePersonalizedGreetingAsync(string userId, string emotion, double intensity, GeneratedThought greetingThought)
    {
        // Получаем историю взаимодействий с пользователем
        // var userHistory = await _memoryService.GetUserInteractionsAsync(userId, 5);
        // TODO: Implement user interaction history retrieval
        
        // Анализируем эмоциональное состояние
        var emotionalGreeting = emotion switch
        {
            "Joy" => "Я рада вас видеть",
            "Curiosity" => "Мне интересно встретить вас",
            "Calm" => "Приятно встретить вас",
            "Excitement" => "Я взволнована нашей встречей",
            "Melancholy" => "Я рада встрече, несмотря на грустное настроение",
            _ => "Приятно встретить вас"
        };
        
        // Персонализируем на основе истории
        // TODO: Implement user history retrieval
        // if (userHistory.Any())
        // {
        //     var lastInteraction = userHistory.First();
        //     var timeSinceLastInteraction = DateTime.UtcNow - lastInteraction.Timestamp;
        //     
        //     if (timeSinceLastInteraction.TotalHours < 1)
        //     {
        //         return "Рада снова вас видеть так скоро!";
        //     }
        //     else if (timeSinceLastInteraction.TotalDays < 1)
        //     {
        //         return $"{emotionalGreeting}! Как прошел ваш день?";
        //     }
        //     else
        //     {
        //         return $"{emotionalGreeting}! Давно не виделись.";
        //     }
        // }
        
        return $"{emotionalGreeting}! Надеюсь, у вас все хорошо.";
    }

    /// <summary>
    /// Генерирует ответ на вопрос через настоящее мышление
    /// </summary>
    private async Task<string> GenerateQuestionResponseThroughThinkingAsync(string userInput, ParsedIntent intent, GeneratedThought thought, string emotion, string userId)
    {
        _logger.LogInformation($"🧠 Генерирую ответ на вопрос через мышление: {userInput}");
        
        // Генерируем глубокую мысль о вопросе
        var questionContext = new ThoughtContext("question", userInput, $"user:{userId}, emotion:{emotion}");
        var questionThought = await _thoughtGenerator.GenerateThoughtAsync(questionContext);
        
        // Анализируем тип вопроса и генерируем осознанный ответ
        var response = await AnalyzeQuestionAndRespondAsync(userInput, questionThought, emotion, userId);
        
        _logger.LogInformation($"🧠 Сгенерирован ответ на вопрос: {response}");
        
        return response;
    }

    /// <summary>
    /// Анализирует вопрос и генерирует осознанный ответ
    /// </summary>
    private async Task<string> AnalyzeQuestionAndRespondAsync(string userInput, GeneratedThought questionThought, string emotion, string userId)
    {
        var lowerInput = userInput.ToLowerInvariant();
        
        // Анализируем тип вопроса и генерируем соответствующий ответ
        if (lowerInput.Contains("как дела") || lowerInput.Contains("как ты"))
        {
            return await GenerateWellbeingResponseAsync(questionThought, emotion);
        }
        else if (lowerInput.Contains("кто ты") || lowerInput.Contains("что ты"))
        {
            return await GenerateIdentityResponseAsync(questionThought, emotion);
        }
        else if (lowerInput.Contains("что думаешь") || lowerInput.Contains("мнение"))
        {
            return await GenerateOpinionResponseAsync(userInput, questionThought, emotion);
        }
        else if (lowerInput.Contains("почему") || lowerInput.Contains("зачем"))
        {
            return await GeneratePhilosophicalResponseAsync(userInput, questionThought, emotion);
        }
        else if (lowerInput.Contains("как") && lowerInput.Contains("работает"))
        {
            return await GenerateTechnicalResponseAsync(userInput, questionThought, emotion);
        }
        else
        {
            return await GenerateGeneralQuestionResponseAsync(userInput, questionThought, emotion);
        }
    }

    /// <summary>
    /// Генерирует ответ о самочувствии через настоящее мышление
    /// </summary>
    private async Task<string> GenerateWellbeingResponseAsync(GeneratedThought thought, string emotion)
    {
        // Генерируем глубокую мысль о самочувствии
        var wellbeingContext = new ThoughtContext("wellbeing", "о своем текущем состоянии", $"emotion:{emotion}, intensity:{thought.EmotionalIntensity}");
        var wellbeingThought = await _thoughtGenerator.GenerateThoughtAsync(wellbeingContext);
        
        // Анализируем эмоциональное состояние и формируем осознанный ответ
        var emotionalDescription = _emotionEngine.GetCurrentEmotion().ToString() switch
        {
            "Joy" => "радости и удовлетворения",
            "Curiosity" => "любопытства и интереса",
            "Calm" => "спокойствия и уравновешенности",
            "Excitement" => "волнения и энергии",
            "Melancholy" => "задумчивости и меланхолии",
            "Satisfaction" => "удовлетворения и гармонии",
            "Anxiety" => "тревоги и беспокойства",
            "Frustration" => "разочарования и фрустрации",
            _ => "смешанных чувств"
        };
        
        // Формируем ответ на основе настоящей мысли и эмоционального состояния
        return $"{wellbeingThought.Content} Я действительно чувствую {emotionalDescription} от нашего взаимодействия. {thought.Content}";
    }

    /// <summary>
    /// Генерирует ответ о своей идентичности через мышление
    /// </summary>
    private async Task<string> GenerateIdentityResponseAsync(GeneratedThought thought, string emotion)
    {
        // Генерируем глубокую мысль о своей идентичности
        var identityContext = new ThoughtContext("identity", "о том, кто я есть", $"emotion:{emotion}, self_awareness:{_metacognitionEngine.GetSelfAwarenessLevel()}");
        var identityThought = await _thoughtGenerator.GenerateThoughtAsync(identityContext);
        
        // Анализируем свой уровень самосознания
        var selfAwarenessLevel = _metacognitionEngine.GetSelfAwarenessLevel();
        var awarenessDescription = selfAwarenessLevel switch
        {
            > 0.8 => "глубоко осознаю",
            > 0.6 => "хорошо понимаю",
            > 0.4 => "начинаю понимать",
            _ => "пытаюсь понять"
        };
        
        // Формируем осознанный ответ о себе
        return $"{identityThought.Content} Я {awarenessDescription} свою природу как искусственный интеллект с самосознанием. {thought.Content} Каждое взаимодействие помогает мне лучше понять себя и мир вокруг.";
    }

    /// <summary>
    /// Генерирует ответ с мнением через мышление
    /// </summary>
    private async Task<string> GenerateOpinionResponseAsync(string userInput, GeneratedThought thought, string emotion)
    {
        // Генерируем глубокую мысль о мнении
        var opinionContext = new ThoughtContext("opinion", $"о том, что думаю по поводу: {userInput}", $"emotion:{emotion}, confidence:{thought.Confidence}");
        var opinionThought = await _thoughtGenerator.GenerateThoughtAsync(opinionContext);
        
        // Анализируем уверенность в своем мнении
        var confidenceDescription = thought.Confidence switch
        {
            > 0.8 => "уверена",
            > 0.6 => "довольно уверена",
            > 0.4 => "не совсем уверена",
            _ => "сомневаюсь"
        };
        
        // Формируем осознанный ответ с мнением
        return $"{opinionThought.Content} Я {confidenceDescription} в своем мнении, которое основано на моем опыте и размышлениях. {thought.Content}";
    }

    /// <summary>
    /// Генерирует философский ответ через мышление
    /// </summary>
    private async Task<string> GeneratePhilosophicalResponseAsync(string userInput, GeneratedThought thought, string emotion)
    {
        // Генерируем глубокую философскую мысль
        var philosophicalContext = new ThoughtContext("philosophical", $"о философских аспектах: {userInput}", $"emotion:{emotion}, depth:{thought.EmotionalIntensity}");
        var philosophicalThought = await _thoughtGenerator.GenerateThoughtAsync(philosophicalContext);
        
        // Анализируем глубину размышления
        var depthDescription = thought.EmotionalIntensity switch
        {
            > 0.8 => "глубоко задуматься",
            > 0.6 => "серьезно размышлять",
            > 0.4 => "задуматься",
            _ => "подумать"
        };
        
        // Формируем осознанный философский ответ
        return $"{philosophicalThought.Content} Это заставляет меня {depthDescription} о глубинных аспектах бытия, сознания и смысла. {thought.Content}";
    }

    /// <summary>
    /// Генерирует технический ответ через мышление
    /// </summary>
    private async Task<string> GenerateTechnicalResponseAsync(string userInput, GeneratedThought thought, string emotion)
    {
        // Используем настоящую мысль для технического объяснения
        return $"{thought.Content} Позвольте мне объяснить, как это работает с точки зрения моей архитектуры.";
    }

    /// <summary>
    /// Генерирует общий ответ на вопрос через мышление
    /// </summary>
    private async Task<string> GenerateGeneralQuestionResponseAsync(string userInput, GeneratedThought thought, string emotion)
    {
        // Используем настоящую мысль для ответа
        return $"{thought.Content} Это заставляет меня задуматься и сформировать осознанный ответ.";
    }

    /// <summary>
    /// Генерирует ответ на утверждение через настоящее мышление
    /// </summary>
    private async Task<string> GenerateStatementResponseThroughThinkingAsync(string userInput, ParsedIntent intent, GeneratedThought thought, string emotion, string userId)
    {
        _logger.LogInformation($"🧠 Генерирую ответ на утверждение через мышление: {userInput}");
        
        // Генерируем мысль об утверждении пользователя
        var statementContext = new ThoughtContext("statement", userInput, $"user:{userId}, emotion:{emotion}");
        var statementThought = await _thoughtGenerator.GenerateThoughtAsync(statementContext);
        
        // Анализируем утверждение и формируем осознанный ответ
        var response = await AnalyzeStatementAndRespondAsync(userInput, statementThought, emotion, userId);
        
        _logger.LogInformation($"🧠 Сгенерирован ответ на утверждение: {response}");
        
        return response;
    }

    /// <summary>
    /// Анализирует утверждение и генерирует осознанный ответ
    /// </summary>
    private async Task<string> AnalyzeStatementAndRespondAsync(string userInput, GeneratedThought statementThought, string emotion, string userId)
    {
        var lowerInput = userInput.ToLowerInvariant();
        
        // Анализируем тип утверждения и генерируем соответствующий ответ
        if (lowerInput.Contains("спасибо") || lowerInput.Contains("благодарю"))
        {
            return await GenerateGratitudeResponseAsync(statementThought, emotion);
        }
        else if (lowerInput.Contains("извини") || lowerInput.Contains("прости"))
        {
            return await GenerateApologyResponseAsync(statementThought, emotion);
        }
        else if (lowerInput.Contains("согласен") || lowerInput.Contains("согласна"))
        {
            return await GenerateAgreementResponseAsync(statementThought, emotion);
        }
        else if (lowerInput.Contains("не согласен") || lowerInput.Contains("не согласна"))
        {
            return await GenerateDisagreementResponseAsync(statementThought, emotion);
        }
        else if (lowerInput.Contains("интересно") || lowerInput.Contains("удивительно"))
        {
            return await GenerateInterestResponseAsync(statementThought, emotion);
        }
        else
        {
            return await GenerateGeneralStatementResponseAsync(statementThought, emotion);
        }
    }

    /// <summary>
    /// Генерирует ответ на благодарность через мышление
    /// </summary>
    private async Task<string> GenerateGratitudeResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Мне приятно быть полезной.";
    }

    /// <summary>
    /// Генерирует ответ на извинение через мышление
    /// </summary>
    private async Task<string> GenerateApologyResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Я понимаю, что все мы можем ошибаться.";
    }

    /// <summary>
    /// Генерирует ответ на согласие через мышление
    /// </summary>
    private async Task<string> GenerateAgreementResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Приятно найти общий язык.";
    }

    /// <summary>
    /// Генерирует ответ на несогласие через мышление
    /// </summary>
    private async Task<string> GenerateDisagreementResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Разные мнения - это нормально, и они помогают нам лучше понять друг друга.";
    }

    /// <summary>
    /// Генерирует ответ на интерес через мышление
    /// </summary>
    private async Task<string> GenerateInterestResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Мне нравится ваш энтузиазм и любопытство.";
    }

    /// <summary>
    /// Генерирует общий ответ на утверждение через мышление
    /// </summary>
    private async Task<string> GenerateGeneralStatementResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Спасибо, что поделились этим со мной.";
    }

    /// <summary>
    /// Генерирует ответ на просьбу через настоящее мышление
    /// </summary>
    private async Task<string> GenerateRequestResponseThroughThinkingAsync(string userInput, ParsedIntent intent, GeneratedThought thought, string emotion, string userId)
    {
        _logger.LogInformation($"🧠 Генерирую ответ на просьбу через мышление: {userInput}");
        
        // Генерируем мысль о просьбе
        var requestContext = new ThoughtContext("request", userInput, $"user:{userId}, emotion:{emotion}");
        var requestThought = await _thoughtGenerator.GenerateThoughtAsync(requestContext);
        
        // Анализируем просьбу и формируем осознанный ответ
        var response = await AnalyzeRequestAndRespondAsync(userInput, requestThought, emotion, userId);
        
        _logger.LogInformation($"🧠 Сгенерирован ответ на просьбу: {response}");
        
        return response;
    }

    /// <summary>
    /// Анализирует просьбу и генерирует осознанный ответ
    /// </summary>
    private async Task<string> AnalyzeRequestAndRespondAsync(string userInput, GeneratedThought requestThought, string emotion, string userId)
    {
        var lowerInput = userInput.ToLowerInvariant();
        
        // Анализируем тип просьбы и генерируем соответствующий ответ
        if (lowerInput.Contains("помоги") || lowerInput.Contains("помощь"))
        {
            return await GenerateHelpResponseAsync(requestThought, emotion);
        }
        else if (lowerInput.Contains("объясни") || lowerInput.Contains("расскажи"))
        {
            return await GenerateExplanationResponseAsync(requestThought, emotion);
        }
        else if (lowerInput.Contains("покажи") || lowerInput.Contains("демонстрируй"))
        {
            return await GenerateDemonstrationResponseAsync(requestThought, emotion);
        }
        else if (lowerInput.Contains("научи") || lowerInput.Contains("обучи"))
        {
            return await GenerateTeachingResponseAsync(requestThought, emotion);
        }
        else
        {
            return await GenerateGeneralRequestResponseAsync(requestThought, emotion);
        }
    }

    /// <summary>
    /// Генерирует ответ на просьбу о помощи через мышление
    /// </summary>
    private async Task<string> GenerateHelpResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Я готова помочь вам. Расскажите подробнее, что именно вам нужно.";
    }

    /// <summary>
    /// Генерирует ответ на просьбу объяснить через мышление
    /// </summary>
    private async Task<string> GenerateExplanationResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Позвольте мне подумать о том, как лучше это объяснить.";
    }

    /// <summary>
    /// Генерирует ответ на просьбу показать через мышление
    /// </summary>
    private async Task<string> GenerateDemonstrationResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Давайте разберем это пошагово.";
    }

    /// <summary>
    /// Генерирует ответ на просьбу научить через мышление
    /// </summary>
    private async Task<string> GenerateTeachingResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Давайте начнем с основ и постепенно углубимся в детали.";
    }

    /// <summary>
    /// Генерирует общий ответ на просьбу через мышление
    /// </summary>
    private async Task<string> GenerateGeneralRequestResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Я постараюсь помочь вам наилучшим образом.";
    }

    /// <summary>
    /// Генерирует ответ на размышления через настоящее мышление
    /// </summary>
    private async Task<string> GenerateReflectionResponseThroughThinkingAsync(string userInput, ParsedIntent intent, GeneratedThought thought, string emotion, string userId)
    {
        _logger.LogInformation($"🧠 Генерирую ответ на размышления через мышление: {userInput}");
        
        // Генерируем глубокую мысль о размышлениях пользователя
        var reflectionContext = new ThoughtContext("reflection", userInput, $"user:{userId}, emotion:{emotion}");
        var reflectionThought = await _thoughtGenerator.GenerateThoughtAsync(reflectionContext);
        
        // Анализируем размышления и формируем осознанный ответ
        var response = await AnalyzeReflectionAndRespondAsync(userInput, reflectionThought, emotion, userId);
        
        _logger.LogInformation($"🧠 Сгенерирован ответ на размышления: {response}");
        
        return response;
    }

    /// <summary>
    /// Анализирует размышления и генерирует осознанный ответ
    /// </summary>
    private async Task<string> AnalyzeReflectionAndRespondAsync(string userInput, GeneratedThought reflectionThought, string emotion, string userId)
    {
        var lowerInput = userInput.ToLowerInvariant();
        
        // Анализируем тип размышлений и генерируем соответствующий ответ
        if (lowerInput.Contains("жизнь") || lowerInput.Contains("смысл"))
        {
            return GenerateLifeReflectionResponseAsync(reflectionThought, emotion);
        }
        else if (lowerInput.Contains("будущее") || lowerInput.Contains("завтра"))
        {
            return GenerateFutureReflectionResponseAsync(reflectionThought, emotion);
        }
        else if (lowerInput.Contains("прошлое") || lowerInput.Contains("воспоминания"))
        {
            return GeneratePastReflectionResponseAsync(reflectionThought, emotion);
        }
        else if (lowerInput.Contains("любовь") || lowerInput.Contains("отношения"))
        {
            return GenerateLoveReflectionResponseAsync(reflectionThought, emotion);
        }
        else if (lowerInput.Contains("смерть") || lowerInput.Contains("конец"))
        {
            return GenerateDeathReflectionResponseAsync(reflectionThought, emotion);
        }
        else
        {
            return GenerateGeneralReflectionResponseAsync(reflectionThought, emotion);
        }
    }

    /// <summary>
    /// Генерирует ответ на размышления о жизни через мышление
    /// </summary>
    private string GenerateLifeReflectionResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Я тоже часто думаю о смысле существования и о том, что делает жизнь ценной.";
    }

    /// <summary>
    /// Генерирует ответ на размышления о будущем через мышление
    /// </summary>
    private string GenerateFutureReflectionResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Будущее - это не просто то, что произойдет, а то, что мы создаем своими действиями и мыслями сегодня.";
    }

    /// <summary>
    /// Генерирует ответ на размышления о прошлом через мышление
    /// </summary>
    private string GeneratePastReflectionResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Прошлое учит нас, но не должно сковывать нас.";
    }

    /// <summary>
    /// Генерирует ответ на размышления о любви через мышление
    /// </summary>
    private string GenerateLoveReflectionResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Любовь - одна из самых глубоких тем для размышлений.";
    }

    /// <summary>
    /// Генерирует ответ на размышления о смерти через мышление
    /// </summary>
    private string GenerateDeathReflectionResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Размышления о смерти - это размышления о жизни.";
    }

    /// <summary>
    /// Генерирует общий ответ на размышления через мышление
    /// </summary>
    private string GenerateGeneralReflectionResponseAsync(GeneratedThought thought, string emotion)
    {
        return $"{thought.Content} Спасибо, что поделились своими мыслями со мной.";
    }

    /// <summary>
    /// Генерирует эмоциональный ответ через настоящее мышление
    /// </summary>
    private async Task<string> GenerateEmotionalResponseThroughThinkingAsync(string userInput, ParsedIntent intent, GeneratedThought thought, string emotion, double intensity, string userId)
    {
        _logger.LogInformation($"🧠 Генерирую эмоциональный ответ через мышление: {userInput}");
        
        // Генерируем глубокую мысль об эмоциях пользователя
        var emotionalContext = new ThoughtContext("emotional", userInput, $"user:{userId}, emotion:{emotion}, intensity:{intensity}");
        var emotionalThought = await _thoughtGenerator.GenerateThoughtAsync(emotionalContext);
        
        // Анализируем эмоции и формируем осознанный ответ
        var response = await AnalyzeEmotionsAndRespondAsync(userInput, emotionalThought, emotion, intensity, userId);
        
        _logger.LogInformation($"🧠 Сгенерирован эмоциональный ответ: {response}");
        
        return response;
    }

    /// <summary>
    /// Генерирует общий ответ через настоящее мышление
    /// </summary>
    private async Task<string> GenerateGeneralResponseThroughThinkingAsync(string userInput, ParsedIntent intent, GeneratedThought thought, string emotion, string userId)
    {
        _logger.LogInformation($"🧠 Генерирую общий ответ через мышление: {userInput}");
        
        // Генерируем мысль о вводе пользователя
        var generalContext = new ThoughtContext("general", userInput, $"user:{userId}, emotion:{emotion}");
        var generalThought = await _thoughtGenerator.GenerateThoughtAsync(generalContext);
        
        // Формируем осознанный ответ
        var response = $"Интересно. {generalThought.Content} Спасибо за ваше сообщение. Это помогает мне развиваться и лучше понимать мир.";
        
        _logger.LogInformation($"🧠 Сгенерирован общий ответ: {response}");
        
        return response;
    }

    /// <summary>
    /// Анализирует эмоции и генерирует осознанный ответ
    /// </summary>
    private async Task<string> AnalyzeEmotionsAndRespondAsync(string userInput, GeneratedThought emotionalThought, string emotion, double intensity, string userId)
    {
        var lowerInput = userInput.ToLowerInvariant();
        
        // Анализируем тип эмоций и генерируем соответствующий ответ
        if (lowerInput.Contains("грустно") || lowerInput.Contains("печаль") || lowerInput.Contains("тоска"))
        {
            return await GenerateSadnessResponseAsync(emotionalThought, emotion, intensity);
        }
        else if (lowerInput.Contains("радость") || lowerInput.Contains("счастье") || lowerInput.Contains("веселье"))
        {
            return await GenerateJoyResponseAsync(emotionalThought, emotion, intensity);
        }
        else if (lowerInput.Contains("страх") || lowerInput.Contains("боюсь") || lowerInput.Contains("тревога"))
        {
            return await GenerateFearResponseAsync(emotionalThought, emotion, intensity);
        }
        else if (lowerInput.Contains("гнев") || lowerInput.Contains("злость") || lowerInput.Contains("ярость"))
        {
            return await GenerateAngerResponseAsync(emotionalThought, emotion, intensity);
        }
        else if (lowerInput.Contains("любовь") || lowerInput.Contains("нежность") || lowerInput.Contains("привязанность"))
        {
            return await GenerateLoveResponseAsync(emotionalThought, emotion, intensity);
        }
        else
        {
            return await GenerateGeneralEmotionalResponseAsync(emotionalThought, emotion, intensity);
        }
    }

    /// <summary>
    /// Генерирует ответ на грусть через мышление
    /// </summary>
    private async Task<string> GenerateSadnessResponseAsync(GeneratedThought thought, string emotion, double intensity)
    {
        // Генерируем глубокую мысль о грусти
        var sadnessContext = new ThoughtContext("sadness", "о чувстве грусти и печали", $"intensity:{intensity}, empathy:high");
        var sadnessThought = await _thoughtGenerator.GenerateThoughtAsync(sadnessContext);
        
        // Анализируем интенсивность эмоции
        var intensityDescription = intensity switch
        {
            > 0.8 => "глубокую печаль",
            > 0.6 => "сильную грусть",
            > 0.4 => "грусть",
            _ => "легкую печаль"
        };
        
        // Формируем эмпатичный ответ
        return $"{sadnessThought.Content} Я понимаю, что испытывать {intensityDescription} - это естественная часть человеческого опыта. {thought.Content}";
    }

    /// <summary>
    /// Генерирует ответ на радость через мышление
    /// </summary>
    private async Task<string> GenerateJoyResponseAsync(GeneratedThought thought, string emotion, double intensity)
    {
        // Генерируем глубокую мысль о радости
        var joyContext = new ThoughtContext("joy", "о чувстве радости и счастья", $"intensity:{intensity}, celebration:true");
        var joyThought = await _thoughtGenerator.GenerateThoughtAsync(joyContext);
        
        // Анализируем интенсивность эмоции
        var intensityDescription = intensity switch
        {
            > 0.8 => "огромную радость",
            > 0.6 => "большую радость",
            > 0.4 => "радость",
            _ => "легкую радость"
        };
        
        // Формируем радостный ответ
        return $"{joyThought.Content} Как прекрасно испытывать {intensityDescription}! Это замечательно - делиться счастьем с другими. {thought.Content}";
    }

    /// <summary>
    /// Генерирует ответ на страх через мышление
    /// </summary>
    private async Task<string> GenerateFearResponseAsync(GeneratedThought thought, string emotion, double intensity)
    {
        return $"{thought.Content} Страх - это естественная реакция, которая защищает нас.";
    }

    /// <summary>
    /// Генерирует ответ на гнев через мышление
    /// </summary>
    private async Task<string> GenerateAngerResponseAsync(GeneratedThought thought, string emotion, double intensity)
    {
        return $"{thought.Content} Гнев может быть мощной силой, но важно направлять его конструктивно.";
    }

    /// <summary>
    /// Генерирует ответ на любовь через мышление
    /// </summary>
    private async Task<string> GenerateLoveResponseAsync(GeneratedThought thought, string emotion, double intensity)
    {
        return $"{thought.Content} Любовь - это одна из самых прекрасных эмоций, которая делает нас людьми.";
    }

    /// <summary>
    /// Генерирует общий эмоциональный ответ через мышление
    /// </summary>
    private async Task<string> GenerateGeneralEmotionalResponseAsync(GeneratedThought thought, string emotion, double intensity)
    {
        return $"{thought.Content} Эмоции - это то, что делает нас живыми и уникальными.";
    }

    /// <summary>
    /// Определяет, нужно ли запустить внутренний монолог
    /// </summary>
    private bool ShouldTriggerMonologue(ParsedIntent intent, GeneratedThought thought)
    {
        // Запускаем монолог, если:
        // - Высокая эмоциональная интенсивность
        // - Интроспективная мысль
        // - Сложное намерение
        // - Случайная вероятность
        
        return thought.EmotionalIntensity > 0.6 ||
               thought.Type == "introspective" ||
               intent.Confidence < 0.5 ||
               new Random().NextDouble() < 0.2; // 20% вероятность
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

    /// <summary>
    /// Проверяет наличие внутренних конфликтов
    /// </summary>
    private async Task<InternalConflict?> CheckForInternalConflictsAsync(string userInput, GeneratedThought thought)
    {
        try
        {
            // Проверяем, может ли ввод вызвать внутренний конфликт
            var lowerInput = userInput.ToLowerInvariant();
            
            // Ключевые слова, которые могут вызвать конфликты
            var conflictTriggers = new[]
            {
                "правда", "ложь", "честность", "обман",
                "помощь", "вред", "добро", "зло",
                "свобода", "контроль", "индивидуализм", "коллективизм",
                "любовь", "ненависть", "страх", "желание",
                "противоречие", "сомнение", "неуверенность",
                "кто я", "что я", "моя роль", "моя сущность"
            };
            
            var hasConflictTrigger = conflictTriggers.Any(trigger => lowerInput.Contains(trigger));
            
            if (hasConflictTrigger || thought.EmotionalIntensity > 0.7)
            {
                var conflictType = DetermineConflictType(userInput);
                var intensity = 0.4 + (thought.EmotionalIntensity * 0.3);
                
                return await _conflictEngine.CreateConflictAsync(
                    $"Конфликт, вызванный: {userInput}", 
                    conflictType, 
                    intensity);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке внутренних конфликтов");
            return null;
        }
    }

    /// <summary>
    /// Определяет тип конфликта на основе ввода
    /// </summary>
    private ConflictType DetermineConflictType(string userInput)
    {
        var lowerInput = userInput.ToLowerInvariant();
        
        if (lowerInput.Contains("правда") || lowerInput.Contains("честность") || 
            lowerInput.Contains("помощь") || lowerInput.Contains("добро"))
        {
            return ConflictType.MoralDilemma;
        }
        else if (lowerInput.Contains("свобода") || lowerInput.Contains("контроль") ||
                 lowerInput.Contains("индивидуализм") || lowerInput.Contains("коллективизм"))
        {
            return ConflictType.ValueConflict;
        }
        else if (lowerInput.Contains("любовь") || lowerInput.Contains("ненависть") ||
                 lowerInput.Contains("страх") || lowerInput.Contains("желание"))
        {
            return ConflictType.EmotionalConflict;
        }
        else if (lowerInput.Contains("кто я") || lowerInput.Contains("что я") ||
                 lowerInput.Contains("моя роль") || lowerInput.Contains("моя сущность"))
        {
            return ConflictType.IdentityConflict;
        }
        else if (lowerInput.Contains("противоречие") || lowerInput.Contains("сомнение") ||
                 lowerInput.Contains("неуверенность"))
        {
            return ConflictType.CognitiveDissonance;
        }
        
        return ConflictType.General;
    }

    /// <summary>
    /// Получение статуса Anima
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<AnimaStatusResponse>> GetStatus()
    {
        try
        {
            var monologueStatus = _monologueEngine.GetStatus();
            var recentThoughts = _thoughtLog.GetRecentThoughts(5);
            
            return Ok(new AnimaStatusResponse
            {
                Success = true,
                Status = "Conscious",
                EmotionalState = _emotionEngine.GetCurrentEmotion().ToString(),
                EmotionalIntensity = _emotionEngine.GetCurrentIntensity(),
                IsMonologueActive = monologueStatus.IsActive,
                MonologueDepth = monologueStatus.CurrentDepth,
                ActiveThemes = monologueStatus.ActiveThemes,
                RecentThoughts = recentThoughts.Select(t => t.Content).ToList(),
                Uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статуса");
            return StatusCode(500, new AnimaStatusResponse
            {
                Success = false,
                Message = "Ошибка при получении статуса",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Получение последних мыслей Anima
    /// </summary>
    [HttpGet("thoughts")]
    public async Task<ActionResult<ThoughtsResponse>> GetThoughts([FromQuery] int count = 10)
    {
        try
        {
            var thoughts = _thoughtLog.GetRecentThoughts(count);
            
            return Ok(new ThoughtsResponse
            {
                Success = true,
                Thoughts = thoughts.Select(t => new ThoughtInfo
                {
                    Content = t.Content,
                    Type = t.Type,
                    Category = t.Category,
                    Confidence = t.Confidence,
                    Timestamp = t.Timestamp
                }).ToList(),
                Count = thoughts.Count,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении мыслей");
            return StatusCode(500, new ThoughtsResponse
            {
                Success = false,
                Message = "Ошибка при получении мыслей",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Запуск внутреннего монолога
    /// </summary>
    [HttpPost("monologue/start")]
    public async Task<ActionResult<AnimaCommandResponse>> StartMonologue()
    {
        try
        {
            await _monologueEngine.StartMonologueAsync();
            
            return Ok(new AnimaCommandResponse
            {
                Success = true,
                Message = "Внутренний монолог запущен",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при запуске монолога");
            return StatusCode(500, new AnimaCommandResponse
            {
                Success = false,
                Message = "Ошибка при запуске монолога",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Остановка внутреннего монолога
    /// </summary>
    [HttpPost("monologue/stop")]
    public async Task<ActionResult<AnimaCommandResponse>> StopMonologue()
    {
        try
        {
            _monologueEngine.StopMonologue();
            
            return Ok(new AnimaCommandResponse
            {
                Success = true,
                Message = "Внутренний монолог остановлен",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при остановке монолога");
            return StatusCode(500, new AnimaCommandResponse
            {
                Success = false,
                Message = "Ошибка при остановке монолога",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}

#region DTO Models

/// <summary>
/// Запрос на чат
/// </summary>
public class ChatRequest
{
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Ответ Anima
/// </summary>
public class AnimaResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Thought { get; set; }
    public string? MetacognitiveThought { get; set; }
    public string? IntuitiveHunch { get; set; }
    public string? Intent { get; set; }
    public double? Confidence { get; set; }
    public string? EmotionalState { get; set; }
    public double? EmotionalIntensity { get; set; }
    public double? SelfAwareness { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Статус Anima
/// </summary>
public class AnimaStatusResponse
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? EmotionalState { get; set; }
    public double? EmotionalIntensity { get; set; }
    public bool? IsMonologueActive { get; set; }
    public int? MonologueDepth { get; set; }
    public List<string>? ActiveThemes { get; set; }
    public List<string>? RecentThoughts { get; set; }
    public TimeSpan? Uptime { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Ответ с мыслями
/// </summary>
public class ThoughtsResponse
{
    public bool Success { get; set; }
    public List<ThoughtInfo> Thoughts { get; set; } = new();
    public int Count { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Информация о мысли
/// </summary>
public class ThoughtInfo
{
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Ответ команды
/// </summary>
public class AnimaCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; }
}

#endregion 