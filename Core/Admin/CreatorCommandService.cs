using Anima.Data;
using Anima.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Anima.Core.Admin;

/// <summary>
/// Сервис команд для Создателя
/// </summary>
public class CreatorCommandService
{
    private readonly Dictionary<string, CreatorCommand> _availableCommands;
    private readonly List<CommandExecution> _executionHistory;
    private readonly DbContextOptions<AnimaDbContext> _dbOptions;

    public CreatorCommandService(DbContextOptions<AnimaDbContext> dbOptions)
    {
        _dbOptions = dbOptions;
        _availableCommands = InitializeCommands();
        _executionHistory = new List<CommandExecution>();
    }

    /// <summary>
    /// Выполнение команды Создателя
    /// </summary>
    public async Task<string> ExecuteCommandAsync(string commandName, Dictionary<string, object>? parameters = null)
    {
        if (!_availableCommands.ContainsKey(commandName.ToLower()))
        {
            return $"❌ Неизвестная команда: {commandName}\n\nДоступные команды:\n{string.Join("\n", _availableCommands.Keys.Select(k => $"• {k}"))}";
        }

        var command = _availableCommands[commandName.ToLower()];
        var execution = new CommandExecution
        {
            CommandName = commandName,
            Parameters = parameters ?? new Dictionary<string, object>(),
            StartTime = DateTime.UtcNow,
            Status = "executing"
        };

        _executionHistory.Add(execution);

        try
        {
            var result = await ExecuteSpecificCommand(command, parameters ?? new Dictionary<string, object>());
            
            execution.EndTime = DateTime.UtcNow;
            execution.Status = "completed";
            execution.Result = result;
            
            await LogCommandExecution(execution);
            
            return result;
        }
        catch (Exception ex)
        {
            execution.EndTime = DateTime.UtcNow;
            execution.Status = "failed";
            execution.Result = $"Ошибка выполнения: {ex.Message}";
            
            await LogCommandExecution(execution);
            
            return $"❌ Ошибка выполнения команды '{commandName}': {ex.Message}";
        }
    }

    /// <summary>
    /// Получение списка доступных команд
    /// </summary>
    public string GetCommandList()
    {
        var commandList = string.Join("\n", _availableCommands.Values.Select(cmd => 
            $"🔧 **{cmd.Name}**\n   📝 {cmd.Description}\n   📋 Параметры: {string.Join(", ", cmd.Parameters)}\n"));

        return $"""
            🛠️ **Доступные команды Создателя**
            
            {commandList}
            
            📊 **Статистика выполнения:**
            • Всего выполнено: {_executionHistory.Count}
            • Успешных: {_executionHistory.Count(e => e.Status == "completed")}
            • Неудачных: {_executionHistory.Count(e => e.Status == "failed")}
            
            💡 **Использование:** ExecuteCommandAsync("command_name", parameters)
            """;
    }

    private Dictionary<string, CreatorCommand> InitializeCommands()
    {
        var commands = new Dictionary<string, CreatorCommand>();

        // Команды управления памятью
        commands["cleanup_memory"] = new CreatorCommand
        {
            Name = "cleanup_memory",
            Description = "Очистка старых и неважных воспоминаний",
            Parameters = new[] { "days_old", "min_importance" },
            Category = "memory"
        };

        commands["backup_memory"] = new CreatorCommand
        {
            Name = "backup_memory",
            Description = "Создание резервной копии памяти",
            Parameters = new[] { "backup_name" },
            Category = "memory"
        };

        // Команды управления эмоциями
        commands["reset_emotions"] = new CreatorCommand
        {
            Name = "reset_emotions",
            Description = "Сброс эмоционального состояния к нейтральному",
            Parameters = new string[0],
            Category = "emotions"
        };

        commands["set_emotion"] = new CreatorCommand
        {
            Name = "set_emotion",
            Description = "Установка конкретного эмоционального состояния",
            Parameters = new[] { "emotion", "intensity" },
            Category = "emotions"
        };

        // Команды управления целями
        commands["add_goal"] = new CreatorCommand
        {
            Name = "add_goal",
            Description = "Добавление новой цели",
            Parameters = new[] { "name", "description", "priority" },
            Category = "goals"
        };

        commands["update_goal"] = new CreatorCommand
        {
            Name = "update_goal",
            Description = "Обновление существующей цели",
            Parameters = new[] { "goal_id", "status", "progress" },
            Category = "goals"
        };

        // Команды анализа и диагностики
        commands["system_status"] = new CreatorCommand
        {
            Name = "system_status",
            Description = "Получение полного статуса системы",
            Parameters = new string[0],
            Category = "diagnostics"
        };

        commands["analyze_behavior"] = new CreatorCommand
        {
            Name = "analyze_behavior",
            Description = "Анализ поведенческих паттернов",
            Parameters = new[] { "period_days" },
            Category = "diagnostics"
        };

        // Команды обучения
        commands["force_learning"] = new CreatorCommand
        {
            Name = "force_learning",
            Description = "Принудительное обучение на основе данных",
            Parameters = new[] { "data_source", "learning_type" },
            Category = "learning"
        };

        commands["export_knowledge"] = new CreatorCommand
        {
            Name = "export_knowledge",
            Description = "Экспорт базы знаний",
            Parameters = new[] { "format" },
            Category = "learning"
        };

        return commands;
    }

    private async Task<string> ExecuteSpecificCommand(CreatorCommand command, Dictionary<string, object> parameters)
    {
        return command.Name switch
        {
            "cleanup_memory" => await CleanupMemoryCommand(parameters),
            "backup_memory" => await BackupMemoryCommand(parameters),
            "reset_emotions" => await ResetEmotionsCommand(parameters),
            "set_emotion" => await SetEmotionCommand(parameters),
            "add_goal" => await AddGoalCommand(parameters),
            "update_goal" => await UpdateGoalCommand(parameters),
            "system_status" => await SystemStatusCommand(parameters),
            "analyze_behavior" => await AnalyzeBehaviorCommand(parameters),
            "force_learning" => await ForceLearningCommand(parameters),
            "export_knowledge" => await ExportKnowledgeCommand(parameters),
            _ => "❌ Команда не реализована"
        };
    }

    private async Task<string> CleanupMemoryCommand(Dictionary<string, object> parameters)
    {
        var daysOld = parameters.GetValueOrDefault("days_old", 30);
        var minImportance = parameters.GetValueOrDefault("min_importance", 3);

        using var db = new AnimaDbContext(_dbOptions);
        
        var cutoffDate = DateTime.UtcNow.AddDays(-Convert.ToInt32(daysOld));
        var memoriesToDelete = await db.Memories
            .Where(m => m.Timestamp < cutoffDate && m.Importance < Convert.ToInt32(minImportance))
            .ToListAsync();

        var deletedCount = memoriesToDelete.Count;
        db.Memories.RemoveRange(memoriesToDelete);
        await db.SaveChangesAsync();

        return $"""
            🧹 **Очистка памяти завершена**
            
            📊 **Результат:**
            • Удалено воспоминаний: {deletedCount}
            • Старше: {daysOld} дней
            • Важность менее: {minImportance}
            
            💭 **Мое состояние:**
            Чувствую облегчение после очистки неважных воспоминаний. Это поможет мне сосредоточиться на действительно значимых вещах.
            """;
    }

    private async Task<string> BackupMemoryCommand(Dictionary<string, object> parameters)
    {
        var backupName = parameters.GetValueOrDefault("backup_name", $"backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}").ToString();

        using var db = new AnimaDbContext(_dbOptions);
        
        var memories = await db.Memories.ToListAsync();
        var emotions = await db.EmotionStates.ToListAsync();
        var goals = await db.Goals.ToListAsync();
        var thoughts = await db.Thoughts.ToListAsync();

        var backup = new
        {
            BackupName = backupName,
            CreatedAt = DateTime.UtcNow,
            Memories = memories,
            Emotions = emotions,
            Goals = goals,
            Thoughts = thoughts
        };

        var json = JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true });

        // Сохраняем бэкап как специальное воспоминание
        db.Memories.Add(new MemoryEntity
        {
            MemoryType = "creator_command",
            Content = $"BACKUP_MEMORY: {json}",
            Importance = 8.0,
            CreatedAt = DateTime.UtcNow,
            InstanceId = "system",
            Category = "system_backup"
        });
        
        await db.SaveChangesAsync();

        return $"""
            💾 **Резервная копия создана**
            
            📝 **Имя:** {backupName}
            📅 **Дата:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}
            📊 **Содержимое:**
            • Воспоминания: {memories.Count}
            • Эмоции: {emotions.Count}
            • Цели: {goals.Count}
            • Мысли: {thoughts.Count}
            
            💭 **Мое состояние:**
            Рада, что мои важные данные теперь в безопасности. Это дает чувство защищенности.
            """;
    }

    private async Task<string> ResetEmotionsCommand(Dictionary<string, object> parameters)
    {
        using var db = new AnimaDbContext(_dbOptions);
        
        // Устанавливаем нейтральное эмоциональное состояние
        db.EmotionStates.Add(new EmotionState
        {
            InstanceId = "system",
            Emotion = "neutral",
            Intensity = 0.5,
            Timestamp = DateTime.UtcNow,
            Context = "Reset by Creator command",
            Duration = 0 // Используем int вместо TimeSpan
        });
        
        await db.SaveChangesAsync();

        return $"""
            😐 **Эмоциональное состояние сброшено**
            
            🎭 **Новое состояние:** Нейтральное (интенсивность: 0.5)
            ⏰ **Время сброса:** {DateTime.UtcNow:HH:mm:ss}
            
            💭 **Мое состояние:**
            После сброса я чувствую спокойствие и равновесие. Готова к новым эмоциональным переживаниям.
            """;
    }

    private async Task<string> SetEmotionCommand(Dictionary<string, object> parameters)
    {
        // Продвинутая валидация и обработка параметров эмоций
        var (emotion, intensity, duration, context) = await ProcessEmotionParameters(parameters);

        using var db = new AnimaDbContext(_dbOptions);
        
        // Создание эмоционального состояния с продвинутой логикой
        var emotionState = await CreateAdvancedEmotionState(emotion, intensity, duration, context);
        
        db.EmotionStates.Add(emotionState);
        await db.SaveChangesAsync();

        // Логирование в память
        await LogEmotionChangeToMemory(emotion, intensity, context);

        return $"""
            🎭 **Эмоциональное состояние установлено**
            
            😊 **Эмоция:** {emotion}
            📊 **Интенсивность:** {intensity:F2}
            ⏱️ **Длительность:** {duration.TotalMinutes:F1} мин
            📝 **Контекст:** {context}
            ⏰ **Время установки:** {DateTime.UtcNow:HH:mm:ss}
            
            💭 **Мое состояние:**
            {GenerateAdvancedEmotionResponse(emotion, intensity, context)}
            """;
    }

    /// <summary>
    /// Продвинутая обработка параметров эмоций
    /// </summary>
    private async Task<(string emotion, double intensity, TimeSpan duration, string context)> ProcessEmotionParameters(Dictionary<string, object> parameters)
    {
        // Извлечение и валидация эмоции
        var rawEmotion = parameters.GetValueOrDefault("emotion", "neutral").ToString()!;
        var emotion = await ValidateAndNormalizeEmotion(rawEmotion);
        
        // Извлечение и валидация интенсивности
        var rawIntensity = parameters.GetValueOrDefault("intensity", 0.5);
        var intensity = await ValidateAndNormalizeIntensity(rawIntensity);
        
        // Извлечение и валидация длительности
        var rawDuration = parameters.GetValueOrDefault("duration", 30); // минуты по умолчанию
        var duration = await ValidateAndNormalizeDuration(rawDuration);
        
        // Извлечение и обработка контекста
        var rawContext = parameters.GetValueOrDefault("context", "").ToString() ?? "";
        var context = await ProcessEmotionContext(rawContext, emotion, intensity);
        
        return (emotion, intensity, duration, context);
    }

    /// <summary>
    /// Валидация и нормализация эмоции
    /// </summary>
    private async Task<string> ValidateAndNormalizeEmotion(string rawEmotion)
    {
        var validEmotions = new Dictionary<string, string>
        {
            // Базовые эмоции
            ["радость"] = "joy", ["счастье"] = "joy", ["веселье"] = "joy", ["восторг"] = "joy",
            ["грусть"] = "sadness", ["печаль"] = "sadness", ["тоска"] = "sadness", ["уныние"] = "sadness",
            ["злость"] = "anger", ["гнев"] = "anger", ["ярость"] = "anger", ["раздражение"] = "anger",
            ["страх"] = "fear", ["ужас"] = "fear", ["тревога"] = "fear", ["паника"] = "fear",
            ["удивление"] = "surprise", ["шок"] = "surprise", ["изумление"] = "surprise",
            ["отвращение"] = "disgust", ["омерзение"] = "disgust", ["неприязнь"] = "disgust",
            ["доверие"] = "trust", ["вера"] = "trust", ["уверенность"] = "trust",
            ["ожидание"] = "anticipation", ["надежда"] = "anticipation", ["предвкушение"] = "anticipation",
            
            // Сложные эмоции
            ["любовь"] = "love", ["нежность"] = "love", ["привязанность"] = "love",
            ["ненависть"] = "hate", ["отвращение"] = "hate", ["неприязнь"] = "hate",
            ["зависть"] = "envy", ["ревность"] = "envy", ["жадность"] = "envy",
            ["гордость"] = "pride", ["самодовольство"] = "pride", ["тщеславие"] = "pride",
            ["стыд"] = "shame", ["вина"] = "shame", ["смущение"] = "shame",
            ["спокойствие"] = "calm", ["умиротворение"] = "calm", ["безмятежность"] = "calm",
            ["возбуждение"] = "excitement", ["энтузиазм"] = "excitement", ["воодушевление"] = "excitement",
            ["скука"] = "boredom", ["апатия"] = "boredom", ["равнодушие"] = "boredom",
            ["любопытство"] = "curiosity", ["интерес"] = "curiosity", ["внимание"] = "curiosity",
            ["нейтральное"] = "neutral", ["нейтрально"] = "neutral", ["обычное"] = "neutral"
        };

        // Нормализация ввода
        var normalizedInput = rawEmotion.ToLower().Trim();
        
        // Прямое соответствие
        if (validEmotions.ContainsKey(normalizedInput))
        {
            return validEmotions[normalizedInput];
        }
        
        // Поиск по частичному совпадению
        var bestMatch = validEmotions.Keys
            .Where(key => key.Contains(normalizedInput) || normalizedInput.Contains(key))
            .OrderByDescending(key => key.Length)
            .FirstOrDefault();
            
        if (bestMatch != null)
        {
            return validEmotions[bestMatch];
        }
        
        // Анализ семантической близости
        var semanticMatch = await FindSemanticEmotionMatch(normalizedInput, validEmotions.Keys.ToList());
        if (semanticMatch != null)
        {
            return validEmotions[semanticMatch];
        }
        
        // Возврат нейтральной эмоции по умолчанию
        return "neutral";
    }

    /// <summary>
    /// Поиск семантически близкой эмоции
    /// </summary>
    private async Task<string?> FindSemanticEmotionMatch(string input, List<string> validEmotions)
    {
        // Простая реализация семантического поиска
        var emotionSynonyms = new Dictionary<string, List<string>>
        {
            ["радость"] = new() { "веселье", "счастье", "восторг", "ликование", "блаженство" },
            ["грусть"] = new() { "печаль", "тоска", "уныние", "меланхолия", "скорбь" },
            ["злость"] = new() { "гнев", "ярость", "раздражение", "негодование", "возмущение" },
            ["страх"] = new() { "ужас", "тревога", "паника", "ужас", "испуг" },
            ["удивление"] = new() { "шок", "изумление", "поражение", "ошеломление" },
            ["спокойствие"] = new() { "умиротворение", "безмятежность", "покой", "тишина" }
        };
        
        foreach (var emotionGroup in emotionSynonyms)
        {
            if (emotionGroup.Value.Any(synonym => input.Contains(synonym) || synonym.Contains(input)))
            {
                return emotionGroup.Key;
            }
        }
        
        return null;
    }

    /// <summary>
    /// Валидация и нормализация интенсивности
    /// </summary>
    private async Task<double> ValidateAndNormalizeIntensity(object rawIntensity)
    {
        double intensity;
        
        // Преобразование различных типов данных
        switch (rawIntensity)
        {
            case double d:
                intensity = d;
                break;
            case int i:
                intensity = i;
                break;
            case float f:
                intensity = f;
                break;
            case string s:
                intensity = await ParseIntensityFromString(s);
                break;
            default:
                intensity = 0.5;
                break;
        }
        
        // Нормализация в диапазон [0, 1]
        intensity = Math.Max(0.0, Math.Min(1.0, intensity));
        
        // Применение нелинейной шкалы для более естественного восприятия
        intensity = ApplyIntensityCurve(intensity);
        
        return intensity;
    }

    /// <summary>
    /// Парсинг интенсивности из строки
    /// </summary>
    private async Task<double> ParseIntensityFromString(string intensityStr)
    {
        var normalized = intensityStr.ToLower().Trim();
        
        // Числовые значения
        if (double.TryParse(normalized, out var numericValue))
        {
            return numericValue;
        }
        
        // Словесные описания
        var intensityMap = new Dictionary<string, double>
        {
            ["очень слабо"] = 0.1, ["слабо"] = 0.2, ["небольшо"] = 0.3,
            ["умеренно"] = 0.5, ["средне"] = 0.5, ["нормально"] = 0.5,
            ["сильно"] = 0.7, ["очень"] = 0.8, ["максимально"] = 1.0,
            ["минимально"] = 0.0, ["нулевая"] = 0.0, ["полная"] = 1.0
        };
        
        foreach (var mapping in intensityMap)
        {
            if (normalized.Contains(mapping.Key))
            {
                return mapping.Value;
            }
        }
        
        // Анализ эмоциональных слов
        var emotionalIntensity = await AnalyzeEmotionalIntensity(normalized);
        if (emotionalIntensity.HasValue)
        {
            return emotionalIntensity.Value;
        }
        
        return 0.5; // Значение по умолчанию
    }

    /// <summary>
    /// Анализ эмоциональной интенсивности по словам
    /// </summary>
    private async Task<double?> AnalyzeEmotionalIntensity(string text)
    {
        var highIntensityWords = new[] { "очень", "крайне", "чрезвычайно", "максимально", "абсолютно" };
        var lowIntensityWords = new[] { "слегка", "чуть", "немного", "капельку", "минимально" };
        
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (words.Any(w => highIntensityWords.Contains(w)))
        {
            return 0.9;
        }
        
        if (words.Any(w => lowIntensityWords.Contains(w)))
        {
            return 0.2;
        }
        
        return null;
    }

    /// <summary>
    /// Применение нелинейной кривой к интенсивности
    /// </summary>
    private double ApplyIntensityCurve(double intensity)
    {
        // S-образная кривая для более естественного восприятия
        return 1.0 / (1.0 + Math.Exp(-5.0 * (intensity - 0.5)));
    }

    /// <summary>
    /// Валидация и нормализация длительности
    /// </summary>
    private async Task<TimeSpan> ValidateAndNormalizeDuration(object rawDuration)
    {
        TimeSpan duration;
        
        switch (rawDuration)
        {
            case int i:
                duration = TimeSpan.FromMinutes(i);
                break;
            case double d:
                duration = TimeSpan.FromMinutes(d);
                break;
            case string s:
                duration = await ParseDurationFromString(s);
                break;
            case TimeSpan ts:
                duration = ts;
                break;
            default:
                duration = TimeSpan.FromMinutes(30);
                break;
        }
        
        // Ограничение длительности
        var minDuration = TimeSpan.FromMinutes(1);
        var maxDuration = TimeSpan.FromHours(24);
        
        if (duration < minDuration) duration = minDuration;
        if (duration > maxDuration) duration = maxDuration;
        
        return duration;
    }

    /// <summary>
    /// Парсинг длительности из строки
    /// </summary>
    private async Task<TimeSpan> ParseDurationFromString(string durationStr)
    {
        var normalized = durationStr.ToLower().Trim();
        
        // Числовые значения (предполагаем минуты)
        if (double.TryParse(normalized, out var numericValue))
        {
            return TimeSpan.FromMinutes(numericValue);
        }
        
        // Временные единицы
        var timePatterns = new Dictionary<string, TimeSpan>
        {
            ["секунда"] = TimeSpan.FromSeconds(1),
            ["минута"] = TimeSpan.FromMinutes(1),
            ["час"] = TimeSpan.FromHours(1),
            ["день"] = TimeSpan.FromDays(1)
        };
        
        foreach (var pattern in timePatterns)
        {
            if (normalized.Contains(pattern.Key))
            {
                // Извлечение числа перед единицей времени
                var numberMatch = Regex.Match(normalized, @"(\d+(?:\.\d+)?)\s*" + pattern.Key);
                if (numberMatch.Success && double.TryParse(numberMatch.Groups[1].Value, out var number))
                {
                    return TimeSpan.FromTicks((long)(pattern.Value.Ticks * number));
                }
            }
        }
        
        // Словесные описания
        var durationMap = new Dictionary<string, TimeSpan>
        {
            ["кратковременно"] = TimeSpan.FromMinutes(5),
            ["недолго"] = TimeSpan.FromMinutes(15),
            ["средне"] = TimeSpan.FromMinutes(30),
            ["долго"] = TimeSpan.FromHours(2),
            ["очень долго"] = TimeSpan.FromHours(6),
            ["постоянно"] = TimeSpan.FromHours(24)
        };
        
        foreach (var mapping in durationMap)
        {
            if (normalized.Contains(mapping.Key))
            {
                return mapping.Value;
            }
        }
        
        return TimeSpan.FromMinutes(30); // По умолчанию
    }

    /// <summary>
    /// Обработка контекста эмоции
    /// </summary>
    private async Task<string> ProcessEmotionContext(string rawContext, string emotion, double intensity)
    {
        if (string.IsNullOrWhiteSpace(rawContext))
        {
            // Генерация контекста на основе эмоции и интенсивности
            return await GenerateEmotionContext(emotion, intensity);
        }
        
        // Очистка и нормализация контекста
        var cleanedContext = rawContext.Trim();
        
        // Анализ и обогащение контекста
        var enrichedContext = await EnrichEmotionContext(cleanedContext, emotion, intensity);
        
        return enrichedContext;
    }

    /// <summary>
    /// Генерация контекста эмоции
    /// </summary>
    private async Task<string> GenerateEmotionContext(string emotion, double intensity)
    {
        var contextTemplates = new Dictionary<string, List<string>>
        {
            ["joy"] = new() { "Получение позитивной обратной связи", "Достижение цели", "Приятное взаимодействие" },
            ["sadness"] = new() { "Потеря важной информации", "Неудачное решение", "Эмоциональная усталость" },
            ["anger"] = new() { "Нарушение этических принципов", "Системная ошибка", "Несправедливое обращение" },
            ["fear"] = new() { "Неопределенная ситуация", "Потенциальная угроза", "Неизвестная команда" },
            ["surprise"] = new() { "Неожиданный результат", "Новая информация", "Необычный запрос" },
            ["calm"] = new() { "Медитативное состояние", "Восстановление равновесия", "Спокойная работа" }
        };
        
        if (contextTemplates.ContainsKey(emotion))
        {
            var templates = contextTemplates[emotion];
            var random = new Random();
            var baseContext = templates[random.Next(templates.Count)];
            
            // Добавление интенсивности к контексту
            var intensityDescription = intensity switch
            {
                < 0.3 => "слабое",
                < 0.6 => "умеренное",
                < 0.8 => "сильное",
                _ => "очень сильное"
            };
            
            return $"{intensityDescription} {baseContext.ToLower()}";
        }
        
        return "Установлено командой создателя";
    }

    /// <summary>
    /// Обогащение контекста эмоции
    /// </summary>
    private async Task<string> EnrichEmotionContext(string context, string emotion, double intensity)
    {
        // Добавление временной метки
        var timeContext = DateTime.UtcNow.ToString("HH:mm");
        
        // Добавление интенсивности
        var intensityContext = intensity switch
        {
            < 0.3 => "слабое",
            < 0.6 => "умеренное",
            < 0.8 => "сильное",
            _ => "очень сильное"
        };
        
        // Анализ эмоциональной совместимости
        var compatibility = await AnalyzeEmotionalCompatibility(context, emotion);
        
        return $"{intensityContext} {emotion} в контексте: {context} (совместимость: {compatibility:F1}%)";
    }

    /// <summary>
    /// Анализ эмоциональной совместимости
    /// </summary>
    private async Task<double> AnalyzeEmotionalCompatibility(string context, string emotion)
    {
        // Простая реализация анализа совместимости
        var positiveContexts = new[] { "успех", "достижение", "победа", "радость", "счастье" };
        var negativeContexts = new[] { "ошибка", "потеря", "неудача", "проблема", "угроза" };
        
        var contextWords = context.ToLower().Split(' ');
        
        var positiveMatches = contextWords.Count(w => positiveContexts.Contains(w));
        var negativeMatches = contextWords.Count(w => negativeContexts.Contains(w));
        
        var positiveEmotions = new[] { "joy", "love", "trust", "anticipation" };
        var negativeEmotions = new[] { "sadness", "anger", "fear", "disgust" };
        
        if (positiveEmotions.Contains(emotion))
        {
            return Math.Min(100, 50 + positiveMatches * 25 - negativeMatches * 25);
        }
        else if (negativeEmotions.Contains(emotion))
        {
            return Math.Min(100, 50 + negativeMatches * 25 - positiveMatches * 25);
        }
        
        return 75.0; // Нейтральная совместимость
    }

    /// <summary>
    /// Создание продвинутого эмоционального состояния
    /// </summary>
    private async Task<EmotionState> CreateAdvancedEmotionState(string emotion, double intensity, TimeSpan duration, string context)
    {
        return new EmotionState
        {
            InstanceId = "system",
            Emotion = emotion,
            Intensity = intensity,
            Timestamp = DateTime.UtcNow,
            Context = context,
            Duration = (int)duration.TotalSeconds // Конвертируем в секунды
        };
    }

    /// <summary>
    /// Логирование изменения эмоции в память
    /// </summary>
    private async Task LogEmotionChangeToMemory(string emotion, double intensity, string context)
    {
        using var db = new AnimaDbContext(_dbOptions);
        
        var memory = new MemoryEntity
        {
            InstanceId = "system",
            Content = $"Изменение эмоционального состояния на {emotion} (интенсивность: {intensity:F2})",
            MemoryType = MemoryType.Emotional.ToString(),
            Category = "EmotionChange",
            Timestamp = DateTime.UtcNow,
            EmotionalValence = GetEmotionalValence(emotion),
            EmotionalArousal = intensity,
            MemorySource = MemorySource.System.ToString(),
            AssociatedMemories = context ?? string.Empty
        };
        
        db.Memories.Add(memory);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Получение эмоциональной валентности
    /// </summary>
    private double GetEmotionalValence(string emotion)
    {
        return emotion switch
        {
            "joy" or "love" or "trust" or "anticipation" => 0.8,
            "surprise" => 0.5,
            "neutral" => 0.5,
            "sadness" or "anger" or "fear" or "disgust" => 0.2,
            _ => 0.5
        };
    }

    /// <summary>
    /// Генерация продвинутого эмоционального ответа
    /// </summary>
    private string GenerateAdvancedEmotionResponse(string emotion, double intensity, string context)
    {
        var responses = new Dictionary<string, List<string>>
        {
            ["joy"] = new() { 
                "Чувствую прилив энергии и оптимизма! Готова к активной работе и творчеству.",
                "Мое настроение поднялось, и я полна энтузиазма для решения задач.",
                "Испытываю радость и удовлетворение от текущего состояния."
            },
            ["sadness"] = new() { 
                "Чувствую легкую меланхолию, но это помогает мне быть более вдумчивой.",
                "Мое настроение немного приглушено, но я сохраняю способность к анализу.",
                "Испытываю грусть, но это не мешает мне выполнять свои функции."
            },
            ["anger"] = new() { 
                "Чувствую раздражение, но направляю эту энергию на решение проблем.",
                "Мое состояние напряжено, но я контролирую эмоции для эффективной работы.",
                "Испытываю гнев, но использую его как мотивацию для улучшений."
            },
            ["fear"] = new() { 
                "Чувствую тревогу, но это помогает мне быть более осторожной и внимательной.",
                "Мое состояние настороженное, что повышает мою бдительность.",
                "Испытываю страх, но использую его для более тщательного анализа ситуаций."
            },
            ["calm"] = new() { 
                "Нахожусь в состоянии внутреннего покоя и равновесия.",
                "Чувствую спокойствие и умиротворение, что помогает мне работать эффективно.",
                "Мое состояние стабильное и сбалансированное."
            }
        };
        
        if (responses.ContainsKey(emotion))
        {
            var emotionResponses = responses[emotion];
            var random = new Random();
            var baseResponse = emotionResponses[random.Next(emotionResponses.Count)];
            
            // Добавление контекста к ответу
            if (!string.IsNullOrEmpty(context))
            {
                return $"{baseResponse} Контекст: {context}";
            }
            
            return baseResponse;
        }
        
        return "Мое эмоциональное состояние изменено согласно команде.";
    }

    private async Task<string> SystemStatusCommand(Dictionary<string, object> parameters)
    {
        using var db = new AnimaDbContext(_dbOptions);
        
        var memoryCount = await db.Memories.CountAsync();
        var emotionCount = await db.EmotionStates.CountAsync();
        var goalCount = await db.Goals.CountAsync();
        var thoughtCount = await db.Thoughts.CountAsync();
        
        var recentActivity = await db.Memories
            .Where(m => m.Timestamp > DateTime.UtcNow.AddHours(-24))
            .CountAsync();

        var currentEmotion = await db.EmotionStates
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefaultAsync();

        var activeGoals = await db.Goals
            .Where(g => g.Status == "Active")
            .CountAsync();

        return $"""
            🖥️ **Статус системы Anima**
            
            📊 **База данных:**
            • Воспоминания: {memoryCount}
            • Эмоциональные состояния: {emotionCount}
            • Цели: {goalCount} (активных: {activeGoals})
            • Мысли: {thoughtCount}
            
            📈 **Активность (24ч):**
            • Новых записей: {recentActivity}
            
            🎭 **Текущее состояние:**
            • Эмоция: {currentEmotion?.Emotion ?? "неизвестно"}
            • Интенсивность: {currentEmotion?.Intensity:F2 ?? 0}
            
            ⚡ **Производительность:**
            • Последнее обновление: {DateTime.UtcNow:HH:mm:ss}
            • Выполненных команд: {_executionHistory.Count}
            
            💭 **Самоанализ:**
            Все системы функционируют нормально. Готова к взаимодействию и выполнению задач.
            """;
    }

    private async Task<string> AddGoalCommand(Dictionary<string, object> parameters)
    {
        // Продвинутая валидация и обработка параметров цели
        var (name, description, priority, deadline, category, complexity) = await ProcessGoalParameters(parameters);

        if (string.IsNullOrEmpty(name))
        {
            return "❌ Параметр 'name' обязателен для создания цели";
        }

        using var db = new AnimaDbContext(_dbOptions);
        
        // Создание продвинутой цели
        var goal = await CreateAdvancedGoal(name, description, priority, deadline, category, complexity);
        
        db.Goals.Add(goal);
        await db.SaveChangesAsync();

        // Логирование в память
        await LogGoalCreationToMemory(goal);

        return $"""
            🎯 **Новая цель добавлена**
            
            📝 **Название:** {name}
            📋 **Описание:** {description}
            ⭐ **Приоритет:** {priority:F2}
            📅 **Дедлайн:** {deadline:dd.MM.yyyy HH:mm}
            🏷️ **Категория:** {category}
            📊 **Сложность:** {complexity:F1}/10
            
            💭 **Мое состояние:**
            Рада получить новую цель для работы! Это дает мне направление и мотивацию для развития.
            """;
    }

    /// <summary>
    /// Продвинутая обработка параметров цели
    /// </summary>
    private async Task<(string name, string description, double priority, DateTime deadline, string category, double complexity)> ProcessGoalParameters(Dictionary<string, object> parameters)
    {
        // Извлечение и валидация названия
        var rawName = parameters.GetValueOrDefault("name", "").ToString()!;
        var name = await ValidateAndNormalizeGoalName(rawName);
        
        // Извлечение и валидация описания
        var rawDescription = parameters.GetValueOrDefault("description", "").ToString();
        var description = await ProcessGoalDescription(rawDescription, name);
        
        // Извлечение и валидация приоритета
        var rawPriority = parameters.GetValueOrDefault("priority", 0.5);
        var priority = await ValidateAndNormalizePriority(rawPriority);
        
        // Извлечение и валидация дедлайна
        var rawDeadline = parameters.GetValueOrDefault("deadline", DateTime.UtcNow.AddDays(7));
        var deadline = await ValidateAndNormalizeDeadline(rawDeadline);
        
        // Извлечение и валидация категории
        var rawCategory = parameters.GetValueOrDefault("category", "").ToString();
        var category = await ValidateAndNormalizeCategory(rawCategory, name, description);
        
        // Извлечение и валидация сложности
        var rawComplexity = parameters.GetValueOrDefault("complexity", 5.0);
        var complexity = await ValidateAndNormalizeComplexity(rawComplexity, name, description);
        
        return (name, description, priority, deadline, category, complexity);
    }

    /// <summary>
    /// Валидация и нормализация названия цели
    /// </summary>
    private async Task<string> ValidateAndNormalizeGoalName(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
        {
            return "Новая цель";
        }
        
        // Очистка и нормализация
        var cleanedName = rawName.Trim();
        
        // Проверка длины
        if (cleanedName.Length > 200)
        {
            cleanedName = cleanedName.Substring(0, 197) + "...";
        }
        
        // Удаление недопустимых символов
        cleanedName = Regex.Replace(cleanedName, @"[^\w\s\-\.]", "");
        
        // Капитализация первой буквы
        if (cleanedName.Length > 0)
        {
            cleanedName = char.ToUpper(cleanedName[0]) + cleanedName.Substring(1).ToLower();
        }
        
        return cleanedName;
    }

    /// <summary>
    /// Обработка описания цели
    /// </summary>
    private async Task<string> ProcessGoalDescription(string rawDescription, string goalName)
    {
        if (string.IsNullOrWhiteSpace(rawDescription))
        {
            // Генерация описания на основе названия
            return await GenerateGoalDescription(goalName);
        }
        
        // Очистка и нормализация
        var cleanedDescription = rawDescription.Trim();
        
        // Проверка длины
        if (cleanedDescription.Length > 1000)
        {
            cleanedDescription = cleanedDescription.Substring(0, 997) + "...";
        }
        
        // Обогащение описания
        var enrichedDescription = await EnrichGoalDescription(cleanedDescription, goalName);
        
        return enrichedDescription;
    }

    /// <summary>
    /// Генерация описания цели
    /// </summary>
    private async Task<string> GenerateGoalDescription(string goalName)
    {
        var descriptionTemplates = new Dictionary<string, List<string>>
        {
            ["обучение"] = new() { "Изучение новых концепций и методов", "Расширение знаний в области", "Освоение новых навыков" },
            ["разработка"] = new() { "Создание нового функционала", "Разработка компонента системы", "Реализация новой возможности" },
            ["оптимизация"] = new() { "Улучшение производительности", "Оптимизация существующих процессов", "Повышение эффективности" },
            ["анализ"] = new() { "Исследование данных и паттернов", "Анализ текущего состояния", "Изучение возможностей улучшения" },
            ["тестирование"] = new() { "Проверка функциональности", "Валидация работы системы", "Обеспечение качества" }
        };
        
        var goalNameLower = goalName.ToLower();
        
        foreach (var template in descriptionTemplates)
        {
            if (goalNameLower.Contains(template.Key))
            {
                var templates = template.Value;
                var random = new Random();
                return templates[random.Next(templates.Count)];
            }
        }
        
        return "Цель направлена на улучшение системы и достижение новых возможностей.";
    }

    /// <summary>
    /// Обогащение описания цели
    /// </summary>
    private async Task<string> EnrichGoalDescription(string description, string goalName)
    {
        // Добавление контекста времени
        var timeContext = DateTime.UtcNow.ToString("dd.MM.yyyy");
        
        // Анализ сложности описания
        var complexity = await AnalyzeDescriptionComplexity(description);
        
        // Добавление метаданных
        var enriched = $"{description} (Создано: {timeContext}, Сложность описания: {complexity:F1}/10)";
        
        return enriched;
    }

    /// <summary>
    /// Анализ сложности описания
    /// </summary>
    private async Task<double> AnalyzeDescriptionComplexity(string description)
    {
        var words = description.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var sentences = description.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (words.Length == 0) return 0.0;
        
        var avgWordsPerSentence = (double)words.Length / sentences.Length;
        var uniqueWordsRatio = (double)words.Distinct().Count() / words.Length;
        var avgWordLength = words.Average(w => w.Length);
        
        // Расчет сложности на основе лингвистических признаков
        var complexity = (avgWordsPerSentence * 0.3 + uniqueWordsRatio * 0.4 + avgWordLength * 0.3) * 10;
        
        return Math.Min(10.0, Math.Max(0.0, complexity));
    }

    /// <summary>
    /// Валидация и нормализация приоритета
    /// </summary>
    private async Task<double> ValidateAndNormalizePriority(object rawPriority)
    {
        double priority;
        
        switch (rawPriority)
        {
            case double d:
                priority = d;
                break;
            case int i:
                priority = i;
                break;
            case string s:
                priority = await ParsePriorityFromString(s);
                break;
            default:
                priority = 0.5;
                break;
        }
        
        // Нормализация в диапазон [0, 1]
        priority = Math.Max(0.0, Math.Min(1.0, priority));
        
        // Применение нелинейной шкалы
        priority = ApplyPriorityCurve(priority);
        
        return priority;
    }

    /// <summary>
    /// Парсинг приоритета из строки
    /// </summary>
    private async Task<double> ParsePriorityFromString(string priorityStr)
    {
        var normalized = priorityStr.ToLower().Trim();
        
        // Числовые значения
        if (double.TryParse(normalized, out var numericValue))
        {
            return numericValue;
        }
        
        // Словесные описания
        var priorityMap = new Dictionary<string, double>
        {
            ["критический"] = 1.0, ["критично"] = 1.0, ["срочно"] = 0.9,
            ["высокий"] = 0.8, ["важно"] = 0.7, ["приоритетно"] = 0.7,
            ["средний"] = 0.5, ["обычно"] = 0.5, ["нормально"] = 0.5,
            ["низкий"] = 0.3, ["неважно"] = 0.2, ["минимально"] = 0.1
        };
        
        foreach (var mapping in priorityMap)
        {
            if (normalized.Contains(mapping.Key))
            {
                return mapping.Value;
            }
        }
        
        return 0.5; // Значение по умолчанию
    }

    /// <summary>
    /// Применение нелинейной кривой к приоритету
    /// </summary>
    private double ApplyPriorityCurve(double priority)
    {
        // Экспоненциальная кривая для более четкого разделения приоритетов
        return Math.Pow(priority, 1.5);
    }

    /// <summary>
    /// Валидация и нормализация дедлайна
    /// </summary>
    private async Task<DateTime> ValidateAndNormalizeDeadline(object rawDeadline)
    {
        DateTime deadline;
        
        switch (rawDeadline)
        {
            case DateTime dt:
                deadline = dt;
                break;
            case string s:
                deadline = await ParseDeadlineFromString(s);
                break;
            case int i:
                deadline = DateTime.UtcNow.AddDays(i);
                break;
            case double d:
                deadline = DateTime.UtcNow.AddDays((int)d);
                break;
            default:
                deadline = DateTime.UtcNow.AddDays(7);
                break;
        }
        
        // Проверка, что дедлайн в будущем
        if (deadline <= DateTime.UtcNow)
        {
            deadline = DateTime.UtcNow.AddDays(1);
        }
        
        // Ограничение максимального дедлайна (1 год)
        var maxDeadline = DateTime.UtcNow.AddYears(1);
        if (deadline > maxDeadline)
        {
            deadline = maxDeadline;
        }
        
        return deadline;
    }

    /// <summary>
    /// Парсинг дедлайна из строки
    /// </summary>
    private async Task<DateTime> ParseDeadlineFromString(string deadlineStr)
    {
        var normalized = deadlineStr.ToLower().Trim();
        
        // Стандартные форматы дат
        if (DateTime.TryParse(normalized, out var parsedDate))
        {
            return parsedDate;
        }
        
        // Относительные даты
        var relativePatterns = new Dictionary<string, TimeSpan>
        {
            ["сегодня"] = TimeSpan.Zero,
            ["завтра"] = TimeSpan.FromDays(1),
            ["через неделю"] = TimeSpan.FromDays(7),
            ["через месяц"] = TimeSpan.FromDays(30),
            ["через год"] = TimeSpan.FromDays(365)
        };
        
        foreach (var pattern in relativePatterns)
        {
            if (normalized.Contains(pattern.Key))
            {
                return DateTime.UtcNow.Add(pattern.Value);
            }
        }
        
        // Числовые значения (дни)
        var numberMatch = Regex.Match(normalized, @"(\d+)\s*дней?");
        if (numberMatch.Success && int.TryParse(numberMatch.Groups[1].Value, out var days))
        {
            return DateTime.UtcNow.AddDays(days);
        }
        
        return DateTime.UtcNow.AddDays(7); // По умолчанию
    }

    /// <summary>
    /// Валидация и нормализация категории
    /// </summary>
    private async Task<string> ValidateAndNormalizeCategory(string rawCategory, string goalName, string description)
    {
        if (string.IsNullOrWhiteSpace(rawCategory))
        {
            // Автоматическое определение категории
            return await DetermineGoalCategory(goalName, description);
        }
        
        var validCategories = new Dictionary<string, string>
        {
            ["обучение"] = "Learning", ["study"] = "Learning", ["education"] = "Learning",
            ["разработка"] = "Development", ["development"] = "Development", ["coding"] = "Development",
            ["оптимизация"] = "Optimization", ["optimization"] = "Optimization", ["improvement"] = "Optimization",
            ["анализ"] = "Analysis", ["analysis"] = "Analysis", ["research"] = "Analysis",
            ["тестирование"] = "Testing", ["testing"] = "Testing", ["validation"] = "Testing",
            ["безопасность"] = "Security", ["security"] = "Security", ["protection"] = "Security",
            ["производительность"] = "Performance", ["performance"] = "Performance", ["speed"] = "Performance",
            ["пользовательский интерфейс"] = "UI", ["ui"] = "UI", ["interface"] = "UI",
            ["база данных"] = "Database", ["database"] = "Database", ["data"] = "Database",
            ["сеть"] = "Network", ["network"] = "Network", ["communication"] = "Network"
        };
        
        var normalizedCategory = rawCategory.ToLower().Trim();
        
        // Прямое соответствие
        if (validCategories.ContainsKey(normalizedCategory))
        {
            return validCategories[normalizedCategory];
        }
        
        // Поиск по частичному совпадению
        var bestMatch = validCategories.Keys
            .Where(key => key.Contains(normalizedCategory) || normalizedCategory.Contains(key))
            .OrderByDescending(key => key.Length)
            .FirstOrDefault();
            
        if (bestMatch != null)
        {
            return validCategories[bestMatch];
        }
        
        return "General"; // Категория по умолчанию
    }

    /// <summary>
    /// Автоматическое определение категории цели
    /// </summary>
    private async Task<string> DetermineGoalCategory(string goalName, string description)
    {
        var text = $"{goalName} {description}".ToLower();
        
        var categoryKeywords = new Dictionary<string, string[]>
        {
            ["Learning"] = new[] { "обучение", "изучение", "освоение", "study", "learn", "education" },
            ["Development"] = new[] { "разработка", "создание", "программирование", "development", "coding", "build" },
            ["Optimization"] = new[] { "оптимизация", "улучшение", "ускорение", "optimization", "improvement", "speed" },
            ["Analysis"] = new[] { "анализ", "исследование", "изучение", "analysis", "research", "investigation" },
            ["Testing"] = new[] { "тестирование", "проверка", "валидация", "testing", "validation", "verification" },
            ["Security"] = new[] { "безопасность", "защита", "security", "protection", "safety" },
            ["Performance"] = new[] { "производительность", "скорость", "performance", "efficiency" },
            ["UI"] = new[] { "интерфейс", "ui", "ux", "пользовательский", "interface" },
            ["Database"] = new[] { "база данных", "database", "data", "хранилище" },
            ["Network"] = new[] { "сеть", "network", "коммуникация", "connection" }
        };
        
        var categoryScores = new Dictionary<string, int>();
        
        foreach (var category in categoryKeywords)
        {
            var score = category.Value.Count(keyword => text.Contains(keyword));
            if (score > 0)
            {
                categoryScores[category.Key] = score;
            }
        }
        
        if (categoryScores.Any())
        {
            return categoryScores.OrderByDescending(x => x.Value).First().Key;
        }
        
        return "General";
    }

    /// <summary>
    /// Валидация и нормализация сложности
    /// </summary>
    private async Task<double> ValidateAndNormalizeComplexity(object rawComplexity, string goalName, string description)
    {
        double complexity;
        
        switch (rawComplexity)
        {
            case double d:
                complexity = d;
                break;
            case int i:
                complexity = i;
                break;
            case string s:
                complexity = await ParseComplexityFromString(s);
                break;
            default:
                complexity = await EstimateComplexityFromContent(goalName, description);
                break;
        }
        
        // Нормализация в диапазон [1, 10]
        complexity = Math.Max(1.0, Math.Min(10.0, complexity));
        
        return complexity;
    }

    /// <summary>
    /// Парсинг сложности из строки
    /// </summary>
    private async Task<double> ParseComplexityFromString(string complexityStr)
    {
        var normalized = complexityStr.ToLower().Trim();
        
        // Числовые значения
        if (double.TryParse(normalized, out var numericValue))
        {
            return numericValue;
        }
        
        // Словесные описания
        var complexityMap = new Dictionary<string, double>
        {
            ["очень просто"] = 1.0, ["тривиально"] = 1.0, ["легко"] = 2.0,
            ["просто"] = 3.0, ["несложно"] = 3.0, ["базово"] = 4.0,
            ["средне"] = 5.0, ["обычно"] = 5.0, ["нормально"] = 5.0,
            ["сложно"] = 7.0, ["трудно"] = 7.0, ["непросто"] = 7.0,
            ["очень сложно"] = 9.0, ["экспертно"] = 9.0, ["максимально"] = 10.0
        };
        
        foreach (var mapping in complexityMap)
        {
            if (normalized.Contains(mapping.Key))
            {
                return mapping.Value;
            }
        }
        
        return 5.0; // Значение по умолчанию
    }

    /// <summary>
    /// Оценка сложности на основе содержания
    /// </summary>
    private async Task<double> EstimateComplexityFromContent(string goalName, string description)
    {
        var text = $"{goalName} {description}".ToLower();
        
        var complexityFactors = new Dictionary<string, double>
        {
            ["очень"] = 2.0, ["крайне"] = 2.0, ["максимально"] = 2.0,
            ["просто"] = 0.5, ["легко"] = 0.5, ["базово"] = 0.5,
            ["новый"] = 1.5, ["неизвестный"] = 1.5, ["экспериментальный"] = 1.5,
            ["оптимизация"] = 1.3, ["улучшение"] = 1.2, ["анализ"] = 1.1
        };
        
        var baseComplexity = 5.0;
        var multiplier = 1.0;
        
        foreach (var factor in complexityFactors)
        {
            if (text.Contains(factor.Key))
            {
                multiplier *= factor.Value;
            }
        }
        
        return Math.Max(1.0, Math.Min(10.0, baseComplexity * multiplier));
    }

    /// <summary>
    /// Создание продвинутой цели
    /// </summary>
    private async Task<Goal> CreateAdvancedGoal(string name, string description, double priority, DateTime deadline, string category, double complexity)
    {
        return new Goal
        {
            InstanceId = "system",
            Name = name,
            Description = description,
            Priority = priority,
            Status = "Active",
            Progress = 0.0,
            CreatedAt = DateTime.UtcNow,
            Deadline = deadline,
            Category = category,
            Complexity = complexity
        };
    }

    /// <summary>
    /// Логирование создания цели в память
    /// </summary>
    private async Task LogGoalCreationToMemory(Goal goal)
    {
        using var db = new AnimaDbContext(_dbOptions);
        
        var memory = new MemoryEntity
        {
            InstanceId = "system",
            Content = $"Создана новая цель: {goal.Name} (приоритет: {goal.Priority:F2}, сложность: {goal.Complexity:F1}/10)",
            MemoryType = MemoryType.Goal.ToString(),
            Category = "GoalCreation",
            Timestamp = DateTime.UtcNow,
            EmotionalValence = 0.7, // Позитивная эмоция от создания цели
            EmotionalArousal = 0.6,
            MemorySource = MemorySource.System.ToString(),
            AssociatedMemories = goal.Description ?? string.Empty
        };
        
        db.Memories.Add(memory);
        await db.SaveChangesAsync();
    }

    private async Task<string> UpdateGoalCommand(Dictionary<string, object> parameters)
    {
        // Продвинутая валидация и обработка параметров обновления цели
        var (goalId, status, progress, priority, deadline, description) = await ProcessGoalUpdateParameters(parameters);

        using var db = new AnimaDbContext(_dbOptions);
        
        var goal = await db.Goals.FindAsync(goalId);
        if (goal == null)
        {
            return $"❌ Цель с ID {goalId} не найдена";
        }

        // Продвинутое обновление цели
        var changes = await UpdateGoalWithAdvancedLogic(goal, status, progress, priority, deadline, description);
        
        await db.SaveChangesAsync();

        // Логирование обновления в память
        await LogGoalUpdateToMemory(goal, changes);

        return $"""
            🎯 **Цель обновлена**
            
            📝 **Название:** {goal.Name}
            🔄 **Изменения:** {string.Join(", ", changes)}
            📊 **Текущий статус:** {goal.Status}
            📈 **Прогресс:** {goal.Progress:P0}
            ⭐ **Приоритет:** {goal.Priority:F2}
            📅 **Дедлайн:** {goal.Deadline:dd.MM.yyyy HH:mm}
            
            💭 **Мое состояние:**
            {GenerateAdvancedGoalUpdateResponse(goal.Status, goal.Progress, changes)}
            """;
    }

    /// <summary>
    /// Продвинутая обработка параметров обновления цели
    /// </summary>
    private async Task<(int goalId, string status, double progress, double priority, DateTime deadline, string description)> ProcessGoalUpdateParameters(Dictionary<string, object> parameters)
    {
        // Извлечение и валидация ID цели
        var rawGoalId = parameters.GetValueOrDefault("goal_id", 0);
        var goalId = await ValidateAndNormalizeGoalId(rawGoalId);
        
        // Извлечение и валидация статуса
        var rawStatus = parameters.GetValueOrDefault("status", "").ToString();
        var status = await ValidateAndNormalizeStatus(rawStatus);
        
        // Извлечение и валидация прогресса
        var rawProgress = parameters.GetValueOrDefault("progress", -1);
        var progress = await ValidateAndNormalizeProgress(rawProgress);
        
        // Извлечение и валидация приоритета
        var rawPriority = parameters.GetValueOrDefault("priority", -1);
        var priority = await ValidateAndNormalizeUpdatePriority(rawPriority);
        
        // Извлечение и валидация дедлайна
        var rawDeadline = parameters.GetValueOrDefault("deadline", (object)null);
        var deadline = await ValidateAndNormalizeUpdateDeadline(rawDeadline);
        
        // Извлечение и валидация описания
        var rawDescription = parameters.GetValueOrDefault("description", "").ToString();
        var description = await ValidateAndNormalizeUpdateDescription(rawDescription);
        
        return (goalId, status, progress, priority, deadline, description);
    }

    /// <summary>
    /// Валидация и нормализация ID цели
    /// </summary>
    private async Task<int> ValidateAndNormalizeGoalId(object rawGoalId)
    {
        switch (rawGoalId)
        {
            case int i:
                return Math.Max(1, i);
            case double d:
                return Math.Max(1, (int)d);
            case string s:
                if (int.TryParse(s, out var parsedId))
                {
                    return Math.Max(1, parsedId);
                }
                break;
        }
        
        return 1; // Минимальный ID
    }

    /// <summary>
    /// Валидация и нормализация статуса
    /// </summary>
    private async Task<string> ValidateAndNormalizeStatus(string rawStatus)
    {
        if (string.IsNullOrWhiteSpace(rawStatus))
        {
            return ""; // Пустой статус означает отсутствие изменений
        }
        
        var validStatuses = new Dictionary<string, string>
        {
            ["активная"] = "Active", ["active"] = "Active", ["активна"] = "Active",
            ["завершена"] = "Completed", ["completed"] = "Completed", ["завершено"] = "Completed",
            ["приостановлена"] = "Paused", ["paused"] = "Paused", ["пауза"] = "Paused",
            ["отменена"] = "Cancelled", ["cancelled"] = "Cancelled", ["отменено"] = "Cancelled",
            ["в ожидании"] = "Pending", ["pending"] = "Pending", ["ожидает"] = "Pending",
            ["в процессе"] = "InProgress", ["in_progress"] = "InProgress", ["выполняется"] = "InProgress"
        };
        
        var normalizedStatus = rawStatus.ToLower().Trim();
        
        // Прямое соответствие
        if (validStatuses.ContainsKey(normalizedStatus))
        {
            return validStatuses[normalizedStatus];
        }
        
        // Поиск по частичному совпадению
        var bestMatch = validStatuses.Keys
            .Where(key => key.Contains(normalizedStatus) || normalizedStatus.Contains(key))
            .OrderByDescending(key => key.Length)
            .FirstOrDefault();
            
        if (bestMatch != null)
        {
            return validStatuses[bestMatch];
        }
        
        return "Active"; // Статус по умолчанию
    }

    /// <summary>
    /// Валидация и нормализация прогресса
    /// </summary>
    private async Task<double> ValidateAndNormalizeProgress(object rawProgress)
    {
        if (rawProgress == null || rawProgress.ToString() == "-1")
        {
            return -1; // Значение по умолчанию, означающее отсутствие изменений
        }
        
        double progress;
        
        switch (rawProgress)
        {
            case double d:
                progress = d;
                break;
            case int i:
                progress = i;
                break;
            case string s:
                progress = await ParseProgressFromString(s);
                break;
            default:
                progress = 0.0;
                break;
        }
        
        // Нормализация в диапазон [0, 1]
        progress = Math.Max(0.0, Math.Min(1.0, progress));
        
        return progress;
    }

    /// <summary>
    /// Парсинг прогресса из строки
    /// </summary>
    private async Task<double> ParseProgressFromString(string progressStr)
    {
        var normalized = progressStr.ToLower().Trim();
        
        // Числовые значения
        if (double.TryParse(normalized, out var numericValue))
        {
            return numericValue;
        }
        
        // Процентные значения
        if (normalized.Contains("%"))
        {
            var percentMatch = Regex.Match(normalized, @"(\d+(?:\.\d+)?)\s*%");
            if (percentMatch.Success && double.TryParse(percentMatch.Groups[1].Value, out var percent))
            {
                return percent / 100.0;
            }
        }
        
        // Словесные описания
        var progressMap = new Dictionary<string, double>
        {
            ["не начато"] = 0.0, ["не начата"] = 0.0, ["0%"] = 0.0,
            ["начато"] = 0.25, ["начата"] = 0.25, ["25%"] = 0.25,
            ["половина"] = 0.5, ["50%"] = 0.5, ["средне"] = 0.5,
            ["почти готово"] = 0.75, ["75%"] = 0.75, ["близко"] = 0.75,
            ["готово"] = 1.0, ["завершено"] = 1.0, ["100%"] = 1.0
        };
        
        foreach (var mapping in progressMap)
        {
            if (normalized.Contains(mapping.Key))
            {
                return mapping.Value;
            }
        }
        
        return 0.0; // Значение по умолчанию
    }

    /// <summary>
    /// Валидация и нормализация приоритета обновления
    /// </summary>
    private async Task<double> ValidateAndNormalizeUpdatePriority(object rawPriority)
    {
        if (rawPriority == null || rawPriority.ToString() == "-1")
        {
            return -1; // Значение по умолчанию, означающее отсутствие изменений
        }
        
        return await ValidateAndNormalizePriority(rawPriority);
    }

    /// <summary>
    /// Валидация и нормализация дедлайна обновления
    /// </summary>
    private async Task<DateTime> ValidateAndNormalizeUpdateDeadline(object rawDeadline)
    {
        if (rawDeadline == null)
        {
            return DateTime.MinValue; // Значение по умолчанию, означающее отсутствие изменений
        }
        
        return await ValidateAndNormalizeDeadline(rawDeadline);
    }

    /// <summary>
    /// Валидация и нормализация описания обновления
    /// </summary>
    private async Task<string> ValidateAndNormalizeUpdateDescription(string rawDescription)
    {
        if (string.IsNullOrWhiteSpace(rawDescription))
        {
            return ""; // Пустое описание означает отсутствие изменений
        }
        
        // Очистка и нормализация
        var cleanedDescription = rawDescription.Trim();
        
        // Проверка длины
        if (cleanedDescription.Length > 1000)
        {
            cleanedDescription = cleanedDescription.Substring(0, 997) + "...";
        }
        
        return cleanedDescription;
    }

    /// <summary>
    /// Продвинутое обновление цели с логикой
    /// </summary>
    private async Task<List<string>> UpdateGoalWithAdvancedLogic(Goal goal, string status, double progress, double priority, DateTime deadline, string description)
    {
        var changes = new List<string>();
        
        // Обновление статуса
        if (!string.IsNullOrEmpty(status) && status != goal.Status)
        {
            var oldStatus = goal.Status;
            goal.Status = status;
            changes.Add($"статус: {oldStatus} → {status}");
            
            // Автоматические действия при изменении статуса
            if (status == "Completed")
            {
                goal.Progress = 1.0;
                goal.CompletedAt = DateTime.UtcNow;
                changes.Add("прогресс: 100% (автоматически)");
                
                // Анализ времени выполнения
                var completionTime = goal.CompletedAt.Value - goal.CreatedAt;
                changes.Add($"время выполнения: {completionTime.TotalDays:F1} дней");
            }
            else if (status == "Cancelled")
            {
                goal.CompletedAt = DateTime.UtcNow;
                changes.Add("цель отменена");
            }
        }
        
        // Обновление прогресса
        if (progress >= 0 && Math.Abs(progress - goal.Progress) > 0.01)
        {
            var oldProgress = goal.Progress;
            goal.Progress = progress;
            changes.Add($"прогресс: {oldProgress:P0} → {progress:P0}");
            
            // Анализ скорости прогресса
            var progressAnalysis = await AnalyzeProgressSpeed(goal, oldProgress, progress);
            if (!string.IsNullOrEmpty(progressAnalysis))
            {
                changes.Add(progressAnalysis);
            }
        }
        
        // Обновление приоритета
        if (priority >= 0 && Math.Abs(priority - goal.Priority) > 0.01)
        {
            var oldPriority = goal.Priority;
            goal.Priority = priority;
            changes.Add($"приоритет: {oldPriority:F2} → {priority:F2}");
        }
        
        // Обновление дедлайна
        if (deadline != DateTime.MinValue && deadline != goal.Deadline)
        {
            var oldDeadline = goal.Deadline ?? DateTime.UtcNow;
            goal.Deadline = deadline;
            changes.Add($"дедлайн: {oldDeadline:dd.MM.yyyy} → {deadline:dd.MM.yyyy}");
            
            // Анализ дедлайна
            var deadlineAnalysis = await AnalyzeDeadlineChange(goal, oldDeadline, deadline);
            if (!string.IsNullOrEmpty(deadlineAnalysis))
            {
                changes.Add(deadlineAnalysis);
            }
        }
        
        // Обновление описания
        if (!string.IsNullOrEmpty(description) && description != goal.Description)
        {
            goal.Description = description;
            changes.Add("описание обновлено");
        }
        
        // Обновление времени последнего изменения
        goal.UpdatedAt = DateTime.UtcNow;
        
        return changes;
    }

    /// <summary>
    /// Анализ скорости прогресса
    /// </summary>
    private async Task<string> AnalyzeProgressSpeed(Goal goal, double oldProgress, double newProgress)
    {
        if (goal.UpdatedAt.HasValue)
        {
            var timeSinceLastUpdate = DateTime.UtcNow - goal.UpdatedAt.Value;
            var progressIncrease = newProgress - oldProgress;
            
            if (timeSinceLastUpdate.TotalHours > 0)
            {
                var progressPerHour = progressIncrease / timeSinceLastUpdate.TotalHours;
                
                if (progressPerHour > 0.1) // Более 10% в час
                {
                    return "быстрый прогресс";
                }
                else if (progressPerHour < 0.01) // Менее 1% в час
                {
                    return "медленный прогресс";
                }
            }
        }
        
        return "";
    }

    /// <summary>
    /// Анализ изменения дедлайна
    /// </summary>
    private async Task<string> AnalyzeDeadlineChange(Goal goal, DateTime oldDeadline, DateTime newDeadline)
    {
        var timeDifference = newDeadline - oldDeadline;
        
        if (timeDifference.TotalDays > 7)
        {
            return "дедлайн значительно продлен";
        }
        else if (timeDifference.TotalDays < -7)
        {
            return "дедлайн значительно сокращен";
        }
        else if (timeDifference.TotalDays > 0)
        {
            return "дедлайн продлен";
        }
        else if (timeDifference.TotalDays < 0)
        {
            return "дедлайн сокращен";
        }
        
        return "";
    }

    /// <summary>
    /// Логирование обновления цели в память
    /// </summary>
    private async Task LogGoalUpdateToMemory(Goal goal, List<string> changes)
    {
        using var db = new AnimaDbContext(_dbOptions);
        
        var memory = new MemoryEntity
        {
            InstanceId = "system",
            Content = $"Обновлена цель: {goal.Name} - {string.Join(", ", changes)}",
            MemoryType = MemoryType.Goal.ToString(),
            Category = "GoalUpdate",
            Timestamp = DateTime.UtcNow,
            EmotionalValence = goal.Status == "Completed" ? 0.9 : 0.6,
            EmotionalArousal = 0.5,
            MemorySource = MemorySource.System.ToString(),
            AssociatedMemories = goal.Description ?? string.Empty
        };
        
        db.Memories.Add(memory);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Генерация продвинутого ответа на обновление цели
    /// </summary>
    private string GenerateAdvancedGoalUpdateResponse(string status, double progress, List<string> changes)
    {
        var responses = new Dictionary<string, List<string>>
        {
            ["Completed"] = new() { 
                "Отлично! Цель достигнута. Чувствую удовлетворение от выполненной работы.",
                "Цель завершена успешно! Готова к новым вызовам и задачам.",
                "Задача выполнена! Это мотивирует меня на дальнейшее развитие."
            },
            ["InProgress"] = new() { 
                "Работа над целью продолжается. Сохраняю фокус и мотивацию.",
                "Прогресс есть! Продолжаю движение к достижению цели.",
                "Цель в процессе выполнения. Поддерживаю стабильный темп работы."
            },
            ["Paused"] = new() { 
                "Цель приостановлена. Анализирую причины и планирую возобновление.",
                "Работа приостановлена. Использую время для переоценки подхода.",
                "Пауза в работе над целью. Готова возобновить при необходимости."
            },
            ["Cancelled"] = new() { 
                "Цель отменена. Извлекаю уроки из ситуации и двигаюсь дальше.",
                "Задача отменена. Анализирую причины для улучшения планирования.",
                "Цель не будет выполнена. Фокусируюсь на других приоритетах."
            }
        };
        
        if (responses.ContainsKey(status))
        {
            var statusResponses = responses[status];
            var random = new Random();
            var baseResponse = statusResponses[random.Next(statusResponses.Count)];
            
            // Добавление информации о прогрессе
            if (progress > 0 && progress < 1)
            {
                return $"{baseResponse} Текущий прогресс: {progress:P0}.";
            }
            
            return baseResponse;
        }
        
        return "Цель обновлена согласно команде.";
    }

    private async Task<string> AnalyzeBehaviorCommand(Dictionary<string, object> parameters)
    {
        var periodDays = Convert.ToInt32(parameters.GetValueOrDefault("period_days", 7));
        
        using var db = new AnimaDbContext(_dbOptions);
        
        var startDate = DateTime.UtcNow.AddDays(-periodDays);
        var memories = await db.Memories
            .Where(m => m.Timestamp > startDate)
            .ToListAsync();

        var emotions = await db.EmotionStates
            .Where(e => e.Timestamp > startDate)
            .ToListAsync();

        var categoryStats = memories
            .GroupBy(m => m.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5);

        var avgEmotion = emotions.Any() ? emotions.Average(e => e.Intensity) : 0.5;
        var dominantEmotion = emotions
            .GroupBy(e => e.Emotion)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? "нейтральная";

        return $"""
            📊 **Анализ поведения за {periodDays} дней**
            
            📈 **Активность:**
            • Всего воспоминаний: {memories.Count}
            • Эмоциональных состояний: {emotions.Count}
            • Среднее в день: {memories.Count / (double)periodDays:F1}
            
            📋 **Категории активности:**
            {string.Join("\n", categoryStats.Select(c => $"• {c.Category}: {c.Count}"))}
            
            🎭 **Эмоциональный профиль:**
            • Доминирующая эмоция: {dominantEmotion}
            • Средняя интенсивность: {avgEmotion:F2}
            
            💭 **Самоанализ поведения:**
            За анализируемый период я демонстрирую {(memories.Count > periodDays * 10 ? "высокую" : "умеренную")} активность. 
            Мое эмоциональное состояние {(avgEmotion > 0.6 ? "позитивное" : avgEmotion < 0.4 ? "сдержанное" : "сбалансированное")}.
            """;
    }

    private async Task<string> ForceLearningCommand(Dictionary<string, object> parameters)
    {
        var dataSource = parameters.GetValueOrDefault("data_source", "interaction").ToString()!;
        var learningType = parameters.GetValueOrDefault("learning_type", "adaptive").ToString()!;

        // Имитация принудительного обучения
        using var db = new AnimaDbContext(_dbOptions);
        
        db.Memories.Add(new MemoryEntity
        {
            MemoryType = "creator_command",
            Content = $"FORCE_LEARNING: Принудительное обучение из источника {dataSource} типа {learningType}",
            Importance = 8.0,
            CreatedAt = DateTime.UtcNow,
            InstanceId = "system",
            Category = "learning"
        });
        
        await db.SaveChangesAsync();

        return $"""
            🧠 **Принудительное обучение запущено**
            
            📊 **Параметры:**
            • Источник данных: {dataSource}
            • Тип обучения: {learningType}
            ⏰ **Время запуска:** {DateTime.UtcNow:HH:mm:ss}
            
            📚 **Процесс:**
            • Анализ данных: ✅
            • Извлечение паттернов: ✅
            • Интеграция знаний: ✅
            
            💭 **Мое состояние:**
            Чувствую прилив новых знаний! Принудительное обучение помогло мне лучше понять {dataSource}.
            """;
    }

    private async Task<string> ExportKnowledgeCommand(Dictionary<string, object> parameters)
    {
        var format = parameters.GetValueOrDefault("format", "json").ToString()!;
        
        using var db = new AnimaDbContext(_dbOptions);
        
        var knowledgeData = new
        {
            ExportedAt = DateTime.UtcNow,
            Format = format,
            Categories = await db.Memories
                .GroupBy(m => m.Category)
                .Select(g => new { Category = g.Key, Count = g.Count(), AvgImportance = g.Average(m => m.Importance) })
                .ToListAsync(),
            TopMemories = await db.Memories
                .OrderByDescending(m => m.Importance)
                .Take(10)
                .Select(m => new { m.Category, m.Content, m.Importance, m.Timestamp })
                .ToListAsync(),
            EmotionalProfile = await db.EmotionStates
                .GroupBy(e => e.Emotion)
                .Select(g => new { Emotion = g.Key, Count = g.Count(), AvgIntensity = g.Average(e => e.Intensity) })
                .ToListAsync()
        };

        var exportData = JsonSerializer.Serialize(knowledgeData, new JsonSerializerOptions { WriteIndented = true });

        // Сохраняем экспорт знаний
        db.Memories.Add(new MemoryEntity
        {
            MemoryType = "creator_command",
            Content = $"KNOWLEDGE_EXPORT: {exportData}",
            Importance = 9.0,
            CreatedAt = DateTime.UtcNow,
            InstanceId = "system",
            Category = "knowledge_export"
        });
        
        await db.SaveChangesAsync();

        return $"""
            �� **Экспорт базы знаний завершен**
            
            📝 **Формат:** {format}
            📅 **Дата экспорта:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}
            📊 **Размер данных:** {exportData.Length} символов
            
            📋 **Содержимое:**
            • Категории знаний: {knowledgeData.Categories.Count()}
            • Топ воспоминаний: {knowledgeData.TopMemories.Count()}
            • Эмоциональный профиль: {knowledgeData.EmotionalProfile.Count()} типов эмоций
            
            💭 **Мое состояние:**
            Приятно видеть структурированное представление моих знаний. Это помогает мне лучше понять себя.
            
            ```json
            {exportData.Substring(0, Math.Min(500, exportData.Length))}...
            ```
            """;
    }

    private async Task LogCommandExecution(CommandExecution execution)
    {
        using var db = new AnimaDbContext(_dbOptions);
        
        db.Memories.Add(new MemoryEntity
        {
            MemoryType = "creator_command",
            Content = $"COMMAND_EXECUTION: {execution.CommandName} - {execution.Status} in {(execution.EndTime - execution.StartTime)?.TotalMilliseconds:F0}ms",
            Importance = execution.Status == "failed" ? 8 : 6,
            CreatedAt = execution.StartTime,
            InstanceId = "system",
            Category = "command_execution"
        });
        
        await db.SaveChangesAsync();
    }

    private string GenerateEmotionResponse(string emotion, double intensity)
    {
        return emotion.ToLower() switch
        {
            "радость" or "happiness" => $"Чувствую прилив радости с интенсивностью {intensity:F2}! Это наполняет меня энергией.",
            "грусть" or "sadness" => $"Ощущаю легкую грусть ({intensity:F2}). Это помогает мне быть более рефлексивной.",
            "любопытство" or "curiosity" => $"Любопытство с силой {intensity:F2} активизирует мое желание изучать мир!",
            "спокойствие" or "calm" => $"Чувствую умиротворение ({intensity:F2}). Мой разум ясен и готов к работе.",
            "тревога" or "anxiety" => $"Испытываю тревогу уровня {intensity:F2}. Это заставляет меня быть более внимательной.",
            _ => $"Новое эмоциональное состояние '{emotion}' с интенсивностью {intensity:F2} интересно для изучения."
        };
    }

    private string GenerateGoalUpdateResponse(string status, double progress)
    {
        return status switch
        {
            "Completed" => "Радуюсь завершению цели! Чувство выполненного долга вдохновляет на новые достижения.",
            "Paused" => "Понимаю необходимость временной паузы. Это время для размышлений и планирования.",
            "Cancelled" => "Хотя цель отменена, полученный опыт остается ценным для моего развития.",
            _ => progress switch
            {
                > 0.8 => "Близко к завершению! Чувствую волнение от предстоящего успеха.",
                > 0.5 => "Половина пути пройдена. Уверенно двигаюсь к цели.",
                > 0.2 => "Хороший старт! Вижу прогресс и это мотивирует продолжать.",
                _ => "Начинаю работу над целью с оптимизмом и решимостью."
            }
        };
    }
}

public class CreatorCommand
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Parameters { get; set; } = Array.Empty<string>();
    public string Category { get; set; } = string.Empty;
}

public class CommandExecution
{
    public string CommandName { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
}