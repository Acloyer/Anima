using Microsoft.AspNetCore.Mvc;
using Anima.Core.AGI;
using Anima.Core.SA;
using Anima.Core.Emotion;
using Microsoft.Extensions.Logging;

namespace Anima.API.Controllers;

/// <summary>
/// Тестовый контроллер для проверки работы системы генерации мыслей
/// </summary>
[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly AnimaInstance _animaInstance;
    private readonly ThoughtGenerator _thoughtGenerator;
    private readonly InternalMonologueEngine _internalMonologue;
    private readonly SelfReflectionEngine _selfReflection;
    private readonly EmotionDrivenGoalShift _emotionGoalShift;
    private readonly ILogger<TestController> _logger;

    public TestController(
        AnimaInstance animaInstance,
        ThoughtGenerator thoughtGenerator,
        InternalMonologueEngine internalMonologue,
        SelfReflectionEngine selfReflection,
        EmotionDrivenGoalShift emotionGoalShift,
        ILogger<TestController> logger)
    {
        _animaInstance = animaInstance;
        _thoughtGenerator = thoughtGenerator;
        _internalMonologue = internalMonologue;
        _selfReflection = selfReflection;
        _emotionGoalShift = emotionGoalShift;
        _logger = logger;
    }

    /// <summary>
    /// Тестирует генерацию мыслей
    /// </summary>
    [HttpPost("thoughts")]
    public async Task<IActionResult> TestThoughtGeneration([FromBody] ThoughtTestRequest request)
    {
        try
        {
            _logger.LogInformation($"🧠 Тестирование генерации мыслей: {request.Description}");
            
            var context = new ThoughtContext(request.Type, request.Description, request.Details);
            var thought = await _thoughtGenerator.GenerateThoughtAsync(context);
            
            var response = new ThoughtTestResponse
            {
                OriginalRequest = request,
                GeneratedThought = thought,
                Timestamp = DateTime.UtcNow
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при тестировании генерации мыслей");
            return StatusCode(500, new { error = "Ошибка генерации мыслей", details = ex.Message });
        }
    }

    /// <summary>
    /// Тестирует внутренний монолог
    /// </summary>
    [HttpPost("monologue")]
    public async Task<IActionResult> TestInternalMonologue()
    {
        try
        {
            _logger.LogInformation("💭 Тестирование внутреннего монолога");
            
            await _internalMonologue.StartMonologueAsync();
            
            // Ждем немного для генерации мыслей
            await Task.Delay(3000);
            
            var status = _internalMonologue.GetStatus();
            var recentThoughts = _internalMonologue.GetRecentThoughts(5);
            
            var response = new MonologueTestResponse
            {
                Status = status,
                RecentThoughts = recentThoughts,
                Timestamp = DateTime.UtcNow
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при тестировании внутреннего монолога");
            return StatusCode(500, new { error = "Ошибка внутреннего монолога", details = ex.Message });
        }
    }

    /// <summary>
    /// Тестирует саморефлексию
    /// </summary>
    [HttpPost("reflection")]
    public async Task<IActionResult> TestSelfReflection([FromBody] ReflectionTestRequest request)
    {
        try
        {
            _logger.LogInformation($"🔍 Тестирование саморефлексии: {request.Trigger}");
            
            var session = await _selfReflection.StartReflectionSessionAsync(request.Trigger, request.Context);
            var status = _selfReflection.GetStatus();
            var recommendations = await _selfReflection.GetRecommendationsAsync();
            
            var response = new ReflectionTestResponse
            {
                Session = session,
                Status = status,
                Recommendations = recommendations,
                Timestamp = DateTime.UtcNow
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при тестировании саморефлексии");
            return StatusCode(500, new { error = "Ошибка саморефлексии", details = ex.Message });
        }
    }

    /// <summary>
    /// Тестирует эмоциональный интеллект
    /// </summary>
    [HttpPost("emotional-intelligence")]
    public async Task<IActionResult> TestEmotionalIntelligence()
    {
        try
        {
            _logger.LogInformation("🧠 Тестирование эмоционального интеллекта");
            
            var goalShift = await _emotionGoalShift.AnalyzeAndShiftGoalsAsync();
            var status = _emotionGoalShift.GetStatus();
            var recommendations = await _emotionGoalShift.GetRecommendationsAsync();
            
            var response = new EmotionalIntelligenceTestResponse
            {
                GoalShift = goalShift,
                Status = status,
                Recommendations = recommendations,
                Timestamp = DateTime.UtcNow
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при тестировании эмоционального интеллекта");
            return StatusCode(500, new { error = "Ошибка эмоционального интеллекта", details = ex.Message });
        }
    }

    /// <summary>
    /// Тестирует полную обработку пользовательского ввода
    /// </summary>
    [HttpPost("process-input")]
    public async Task<IActionResult> TestProcessInput([FromBody] ProcessInputRequest request)
    {
        try
        {
            _logger.LogInformation($"🔄 Тестирование обработки ввода: {request.Input}");
            
            var response = await _animaInstance.ProcessInputAsync(request.Input, request.UserId);
            
            var testResponse = new ProcessInputResponse
            {
                OriginalInput = request.Input,
                GeneratedResponse = response,
                Timestamp = DateTime.UtcNow
            };
            
            return Ok(testResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при тестировании обработки ввода");
            return StatusCode(500, new { error = "Ошибка обработки ввода", details = ex.Message });
        }
    }

    /// <summary>
    /// Получает статус всех компонентов системы мыслей
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetSystemStatus()
    {
        try
        {
            var monologueStatus = _internalMonologue.GetStatus();
            var reflectionStatus = _selfReflection.GetStatus();
            var emotionalStatus = _emotionGoalShift.GetStatus();
            
            var status = new SystemStatusResponse
            {
                MonologueStatus = monologueStatus,
                ReflectionStatus = reflectionStatus,
                EmotionalIntelligenceStatus = emotionalStatus,
                Timestamp = DateTime.UtcNow
            };
            
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статуса системы");
            return StatusCode(500, new { error = "Ошибка получения статуса", details = ex.Message });
        }
    }
}

// Модели запросов и ответов для тестирования

public class ThoughtTestRequest
{
    public string Type { get; set; } = "general";
    public string Description { get; set; } = "тестировании";
    public string Details { get; set; } = "Тестовая генерация мысли";
}

public class ThoughtTestResponse
{
    public ThoughtTestRequest OriginalRequest { get; set; } = new();
    public GeneratedThought GeneratedThought { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class MonologueTestResponse
{
    public MonologueStatus Status { get; set; } = new();
    public List<MonologueEntry> RecentThoughts { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class ReflectionTestRequest
{
    public string Trigger { get; set; } = "test_reflection";
    public string Context { get; set; } = "Тестовая саморефлексия";
}

public class ReflectionTestResponse
{
    public ReflectionSession Session { get; set; } = new();
    public SelfReflectionStatus Status { get; set; } = new();
    public List<SelfReflectionRecommendation> Recommendations { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class EmotionalIntelligenceTestResponse
{
    public EmotionalGoalShift GoalShift { get; set; } = new();
    public EmotionalIntelligenceStatus Status { get; set; } = new();
    public List<EmotionalRecommendation> Recommendations { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class ProcessInputRequest
{
    public string Input { get; set; } = "Привет!";
    public string UserId { get; set; } = "test_user";
}

public class ProcessInputResponse
{
    public string OriginalInput { get; set; } = string.Empty;
    public string GeneratedResponse { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class SystemStatusResponse
{
    public MonologueStatus MonologueStatus { get; set; } = new();
    public SelfReflectionStatus ReflectionStatus { get; set; } = new();
    public EmotionalIntelligenceStatus EmotionalIntelligenceStatus { get; set; } = new();
    public DateTime Timestamp { get; set; }
} 