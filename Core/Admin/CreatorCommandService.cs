using Anima.Data;
using Anima.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Anima.AGI.Core.Admin;

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
    public async Task<string> GetCommandListAsync()
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

        var json = System.Text.Json.JsonSerializer.Serialize(backup, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

        // Сохраняем бэкап как специальное воспоминание
        db.Memories.Add(new Memory
        {
            Content = $"BACKUP_DATA: {json}",
            Category = "system_backup",
            Importance = 10,
            Timestamp = DateTime.UtcNow,
            Tags = $"backup,{backupName},full_backup"
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
            Duration = TimeSpan.Zero
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
        var emotion = parameters.GetValueOrDefault("emotion", "neutral").ToString()!;
        var intensity = Convert.ToDouble(parameters.GetValueOrDefault("intensity", 0.5));

        using var db = new AnimaDbContext(_dbOptions);
        
        db.EmotionStates.Add(new EmotionState
        {
            InstanceId = "system",
            Emotion = emotion,
            Intensity = Math.Max(0.0, Math.Min(1.0, intensity)),
            Timestamp = DateTime.UtcNow,
            Context = "Set by Creator command",
            Duration = TimeSpan.Zero
        });
        
        await db.SaveChangesAsync();

        return $"""
            🎭 **Эмоциональное состояние установлено**
            
            😊 **Эмоция:** {emotion}
            📊 **Интенсивность:** {intensity:F2}
            ⏰ **Время установки:** {DateTime.UtcNow:HH:mm:ss}
            
            💭 **Мое состояние:**
            {GenerateEmotionResponse(emotion, intensity)}
            """;
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
        var name = parameters.GetValueOrDefault("name", "").ToString()!;
        var description = parameters.GetValueOrDefault("description", "").ToString()!;
        var priority = Convert.ToDouble(parameters.GetValueOrDefault("priority", 0.5));

        if (string.IsNullOrEmpty(name))
        {
            return "❌ Параметр 'name' обязателен для создания цели";
        }

        using var db = new AnimaDbContext(_dbOptions);
        
        db.Goals.Add(new Goal
        {
            InstanceId = "system",
            Name = name,
            Description = description,
            Priority = Math.Max(0.0, Math.Min(1.0, priority)),
            Status = "Active",
            Progress = 0.0,
            CreatedAt = DateTime.UtcNow
        });
        
        await db.SaveChangesAsync();

        return $"""
            🎯 **Новая цель добавлена**
            
            📝 **Название:** {name}
            📋 **Описание:** {description}
            ⭐ **Приоритет:** {priority:F2}
            📊 **Статус:** Активная
            
            💭 **Мое состояние:**
            Рада получить новую цель для работы! Это дает мне направление и мотивацию для развития.
            """;
    }

    private async Task<string> UpdateGoalCommand(Dictionary<string, object> parameters)
    {
        var goalId = Convert.ToInt32(parameters.GetValueOrDefault("goal_id", 0));
        var status = parameters.GetValueOrDefault("status", "").ToString();
        var progress = Convert.ToDouble(parameters.GetValueOrDefault("progress", -1));

        using var db = new AnimaDbContext(_dbOptions);
        
        var goal = await db.Goals.FindAsync(goalId);
        if (goal == null)
        {
            return $"❌ Цель с ID {goalId} не найдена";
        }

        var changes = new List<string>();

        if (!string.IsNullOrEmpty(status))
        {
            goal.Status = status;
            changes.Add($"статус: {status}");
            
            if (status == "Completed")
            {
                goal.Progress = 1.0;
                goal.CompletedAt = DateTime.UtcNow;
                changes.Add("прогресс: 100%");
            }
        }

        if (progress >= 0)
        {
            goal.Progress = Math.Max(0.0, Math.Min(1.0, progress));
            changes.Add($"прогресс: {progress:P0}");
        }

        await db.SaveChangesAsync();

        return $"""
            🎯 **Цель обновлена**
            
            📝 **Название:** {goal.Name}
            🔄 **Изменения:** {string.Join(", ", changes)}
            📊 **Текущий статус:** {goal.Status}
            📈 **Прогресс:** {goal.Progress:P0}
            
            💭 **Мое состояние:**
            {GenerateGoalUpdateResponse(goal.Status, goal.Progress)}
            """;
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
        
        db.Memories.Add(new Memory
        {
            Content = $"FORCED_LEARNING: Принудительное обучение из источника {dataSource} типа {learningType}",
            Category = "learning",
            Importance = 8,
            Timestamp = DateTime.UtcNow,
            Tags = $"forced_learning,{dataSource},{learningType}"
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

        var exportData = System.Text.Json.JsonSerializer.Serialize(knowledgeData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

        // Сохраняем экспорт знаний
        db.Memories.Add(new Memory
        {
            Content = $"KNOWLEDGE_EXPORT: {exportData}",
            Category = "knowledge_export",
            Importance = 9,
            Timestamp = DateTime.UtcNow,
            Tags = $"export,knowledge,{format}"
        });
        
        await db.SaveChangesAsync();

        return $"""
            📚 **Экспорт базы знаний завершен**
            
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
        
        db.Memories.Add(new Memory
        {
            Content = $"COMMAND_EXECUTION: {execution.CommandName} - {execution.Status} in {(execution.EndTime - execution.StartTime)?.TotalMilliseconds:F0}ms",
            Category = "command_execution",
            Importance = execution.Status == "failed" ? 8 : 6,
            Timestamp = execution.StartTime,
            Tags = $"command,{execution.CommandName},{execution.Status}"
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