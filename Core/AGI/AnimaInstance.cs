using Anima.AGI.Core.SA;
using Anima.AGI.Core.Emotion;
using Anima.AGI.Core.Learning;
using Anima.AGI.Core.Admin;
using Anima.AGI.Core.Memory;
using Anima.AGI.Core.Intent;
using Anima.AGI.Core;
using Anima.Data;
using Anima.Data.Models;
using Microsoft.Extensions.Logging;

namespace Anima.AGI.Core.AGI;

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
        _logger = logger;
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
    public async Task<string> ProcessInputAsync(string userInput, string userId = null)
    {
        if (string.IsNullOrWhiteSpace(userInput))
            return "😐 Я не поняла. Можешь переформулировать?";

        var parsed = await IntentParser.ParseIntentAsync(userInput, userId);

        // ThoughtLog.LogIntent(parsed); // Временно отключено
        // EmotionEngine.UpdateEmotion(parsed.Sentiment); // Временно отключено
        await MemoryService.SaveInteraction(userId, userInput, parsed);
        // LearningEngine?.Ingest(parsed); // необязательно, если ты ещё не подключал обучение

        _logger.LogInformation($"🔍 [Intent]: {parsed.Type}, Confidence={parsed.Confidence:F2}, Sentiment={parsed.Sentiment}");

        return $"🤖 Я распознала намерение: *{parsed.Type}* (уверенность: {parsed.Confidence:F2})";
    }
}
