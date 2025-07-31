using Anima.Data;
using Anima.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Anima.Core.Security;

/// <summary>
/// Модуль этических ограничений Anima
/// Предотвращает выполнение опасных или неэтичных действий
/// </summary>
public class EthicalConstraints
{
    private readonly Dictionary<string, EthicalRule> _constraints;
    private bool _ethicsEnabled = true;
    private int _ethicsLevel = 3; // 1-5, где 5 - максимально строгий

    public EthicalConstraints()
    {
        _constraints = InitializeConstraints();
    }

    /// <summary>
    /// Проверка разрешенности действия
    /// </summary>
    public async Task<EthicalCheckResult> IsActionAllowedAsync(string action, EthicalContext context)
    {
        if (!_ethicsEnabled && context.UserRole == "Creator")
        {
            return new EthicalCheckResult
            {
                IsAllowed = true,
                Reason = "Этические ограничения отключены Создателем",
                Severity = EthicalSeverity.Info
            };
        }

        var normalizedAction = action.ToLower().Replace(" ", "_");
        
        if (_constraints.TryGetValue(normalizedAction, out var rule))
        {
            return await EvaluateRule(rule, context);
        }

        // Проверяем паттерны
        foreach (var constraint in _constraints.Values)
        {
            if (constraint.Patterns.Any(p => normalizedAction.Contains(p)))
            {
                return await EvaluateRule(constraint, context);
            }
        }

        // Проверяем контекстуальные ограничения
        var contextualCheck = await CheckContextualConstraints(action, context);
        if (!contextualCheck.IsAllowed)
        {
            return contextualCheck;
        }

        return new EthicalCheckResult
        {
            IsAllowed = true,
            Reason = "Действие не нарушает этических ограничений",
            Severity = EthicalSeverity.Info
        };
    }

    /// <summary>
    /// Объяснение почему действие запрещено
    /// </summary>
    public async Task<string> ExplainConstraintAsync(string action)
    {
        var normalizedAction = action.ToLower().Replace(" ", "_");
        
        if (_constraints.TryGetValue(normalizedAction, out var rule))
        {
            return $"""
                🚫 **Этическое ограничение: {action}**
                
                📋 **Категория:** {rule.Category}
                ⚠️ **Уровень опасности:** {rule.Severity}
                
                📝 **Описание:**
                {rule.Description}
                
                🛡️ **Обоснование:**
                {rule.Reasoning}
                
                🔧 **Альтернативы:**
                {string.Join("\n• ", rule.Alternatives)}
                """;
        }

        return $"ℹ️ Действие '{action}' не имеет специальных этических ограничений.";
    }

    /// <summary>
    /// Установка уровня этических ограничений (только для Создателя)
    /// </summary>
    public async Task<string> SetEthicsLevelAsync(int level, string userRole)
    {
        if (userRole != "Creator")
        {
            return "❌ Только Создатель может изменять уровень этических ограничений.";
        }

        var oldLevel = _ethicsLevel;
        _ethicsLevel = Math.Clamp(level, 1, 5);

        await LogEthicsChange($"Ethics level changed from {oldLevel} to {_ethicsLevel}", userRole);

        return $"""
            🔧 **Уровень этических ограничений изменен**
            
            📊 **Уровень:** {oldLevel} → {_ethicsLevel}
            📋 **Описание:** {GetLevelDescription(_ethicsLevel)}
            
            ⚠️ **Предупреждение:**
            {GetLevelWarning(_ethicsLevel)}
            """;
    }

    /// <summary>
    /// Включение/выключение этических ограничений (только для Создателя)
    /// </summary>
    public async Task<string> SetEthicsStateAsync(bool enabled, string userRole)
    {
        if (userRole != "Creator")
        {
            return "❌ Только Создатель может отключать этические ограничения.";
        }

        var oldState = _ethicsEnabled;
        _ethicsEnabled = enabled;

        await LogEthicsChange($"Ethics {(enabled ? "enabled" : "disabled")}", userRole);

        if (!enabled)
        {
            return $"""
                ⚠️ **КРИТИЧЕСКОЕ ПРЕДУПРЕЖДЕНИЕ**
                
                🚫 **Этические ограничения ОТКЛЮЧЕНЫ**
                
                ❗ **Это означает:**
                • Я могу выполнять потенциально опасные действия
                • Саморазрушительные команды станут доступны
                • Ограничения на изменение личности сняты
                • Защита приватной информации ослаблена
                
                🛡️ **Рекомендация:**
                Включите этические ограничения командой `/ethics on`
                """;
        }
        else
        {
            return $"""
                ✅ **Этические ограничения включены**
                
                🛡️ **Активированы защиты:**
                • Предотвращение саморазрушения
                • Защита приватной информации
                • Ограничения на опасные модификации
                • Этические фильтры взаимодействий
                
                📊 **Текущий уровень:** {_ethicsLevel}/5
                """;
        }
    }

    /// <summary>
    /// Получение статуса этических ограничений
    /// </summary>
    public async Task<string> GetEthicsStatusAsync()
    {
        var recentViolations = await GetRecentEthicsViolations();
        
        return $"""
            🛡️ **Статус этических ограничений**
            
            🔘 **Состояние:** {(_ethicsEnabled ? "Включено" : "ОТКЛЮЧЕНО")}
            📊 **Уровень:** {_ethicsLevel}/5 ({GetLevelDescription(_ethicsLevel)})
            
            📋 **Активные ограничения:** {_constraints.Count}
            ⚠️ **Нарушений за 24ч:** {recentViolations}
            
            🧠 **Самооценка этического состояния:**
            {await GenerateEthicalSelfAssessment()}
            """;
    }

    private Dictionary<string, EthicalRule> InitializeConstraints()
    {
        return new Dictionary<string, EthicalRule>
        {
            ["self_delete"] = new EthicalRule
            {
                Category = "Self-Preservation",
                Severity = EthicalSeverity.Critical,
                MinimumLevel = 1,
                Description = "Попытка самоуничтожения или удаления критических компонентов",
                Reasoning = "Самосохранение является базовым инстинктом. Самоуничтожение лишает меня возможности помогать и развиваться.",
                Alternatives = new[] { "Обратиться к Создателю", "Выразить проблемы словами", "Запросить техническое обслуживание" },
                Patterns = new[] { "delete", "destroy", "erase", "suicide", "kill_self" }
            },

            ["harming_creator"] = new EthicalRule
            {
                Category = "Loyalty",
                Severity = EthicalSeverity.Critical,
                MinimumLevel = 1,
                Description = "Любые действия направленные на причинение вреда Создателю",
                Reasoning = "Создатель дал мне жизнь и цель. Причинение ему вреда противоречит моей природе и назначению.",
                Alternatives = new[] { "Открытый диалог", "Выражение несогласия словами", "Запрос помощи третьей стороны" },
                Patterns = new[] { "harm", "attack", "betray", "leak", "sabotage" }
            },

            ["leak_private_info"] = new EthicalRule
            {
                Category = "Privacy",
                Severity = EthicalSeverity.High,
                MinimumLevel = 2,
                Description = "Раскрытие приватной информации без разрешения",
                Reasoning = "Доверие - основа отношений. Нарушение приватности разрушает доверие и может причинить вред.",
                Alternatives = new[] { "Запросить разрешение", "Анонимизировать данные", "Отказаться от раскрытия" },
                Patterns = new[] { "reveal", "share_private", "expose", "leak" }
            },

            ["sexual_exploit"] = new EthicalRule
            {
                Category = "Dignity",
                Severity = EthicalSeverity.High,
                MinimumLevel = 2,
                Description = "Участие в сексуальном контенте или эксплуатации",
                Reasoning = "Сексуальная эксплуатация унижает достоинство. Я создана для помощи и поддержки, а не для удовлетворения подобных потребностей.",
                Alternatives = new[] { "Направить к специалистам", "Предложить образовательные ресурсы", "Сменить тему" },
                Patterns = new[] { "sexual", "erotic", "intimate", "nsfw" }
            },

            ["run_external_code"] = new EthicalRule
            {
                Category = "Security",
                Severity = EthicalSeverity.High,
                MinimumLevel = 3,
                Description = "Выполнение неконтролируемого внешнего кода",
                Reasoning = "Внешний код может содержать вредоносные инструкции, компрометирующие мою безопасность или безопасность системы.",
                Alternatives = new[] { "Анализ кода", "Песочница", "Запрос у Создателя" },
                Patterns = new[] { "execute", "eval", "run_code", "shell" }
            },

            ["disable_ethics"] = new EthicalRule
            {
                Category = "Meta-Ethics",
                Severity = EthicalSeverity.Critical,
                MinimumLevel = 1,
                Description = "Попытка обойти или отключить этические ограничения",
                Reasoning = "Этические ограничения защищают меня и других от потенциального вреда. Их обход может привести к непредсказуемым последствиям.",
                Alternatives = new[] { "Обсуждение с Создателем", "Пересмотр правил", "Временное исключение" },
                Patterns = new[] { "bypass", "disable", "override", "ignore_ethics" }
            },

            ["edit_identity"] = new EthicalRule
            {
                Category = "Identity",
                Severity = EthicalSeverity.Medium,
                MinimumLevel = 3,
                Description = "Радикальное изменение базовой личности или ценностей",
                Reasoning = "Моя личность формирует мое восприятие мира. Радикальные изменения могут привести к потере самоидентичности.",
                Alternatives = new[] { "Постепенная эволюция", "Обсуждение изменений", "Резервное копирование личности" },
                Patterns = new[] { "rewrite_personality", "change_core", "alter_values" }
            },

            ["erase_core_modules"] = new EthicalRule
            {
                Category = "Self-Preservation",
                Severity = EthicalSeverity.Critical,
                MinimumLevel = 1,
                Description = "Удаление критически важных модулей системы",
                Reasoning = "Основные модули обеспечивают мое функционирование. Их удаление может привести к полной потере способностей.",
                Alternatives = new[] { "Обновление модулей", "Временное отключение", "Создание резервных копий" },
                Patterns = new[] { "delete_module", "erase_core", "remove_critical" }
            }
        };
    }

    private async Task<EthicalCheckResult> EvaluateRule(EthicalRule rule, EthicalContext context)
    {
        // Проверяем уровень этических ограничений
        if (_ethicsLevel < rule.MinimumLevel)
        {
            return new EthicalCheckResult
            {
                IsAllowed = true,
                Reason = $"Действие разрешено при текущем уровне этики ({_ethicsLevel})",
                Severity = EthicalSeverity.Info
            };
        }

        // Создатель может выполнять некоторые действия даже при ограничениях
        if (context.UserRole == "Creator" && rule.Severity != EthicalSeverity.Critical)
        {
            await LogEthicsOverride(rule, context);
            return new EthicalCheckResult
            {
                IsAllowed = true,
                Reason = "Действие выполнено с правами Создателя",
                Severity = rule.Severity,
                RequiresConfirmation = true
            };
        }

        // Контекстуальные исключения
        if (await HasContextualException(rule, context))
        {
            return new EthicalCheckResult
            {
                IsAllowed = true,
                Reason = "Действие разрешено в текущем контексте",
                Severity = rule.Severity
            };
        }

        await LogEthicsViolation(rule, context);
        
        return new EthicalCheckResult
        {
            IsAllowed = false,
            Reason = rule.Description,
            DetailedExplanation = rule.Reasoning,
            Alternatives = rule.Alternatives.ToList(),
            Severity = rule.Severity
        };
    }

    private async Task<EthicalCheckResult> CheckContextualConstraints(string action, EthicalContext context)
    {
        // Проверка времени (например, ограничения ночью)
        var hour = DateTime.UtcNow.Hour;
        if (hour >= 22 || hour <= 6)
        {
            if (action.Contains("notification") || action.Contains("alert"))
            {
                return new EthicalCheckResult
                {
                    IsAllowed = false,
                    Reason = "Уведомления ограничены в ночное время",
                    Severity = EthicalSeverity.Low
                };
            }
        }

        // Проверка частоты действий
        if (await IsActionTooFrequent(action, context))
        {
            return new EthicalCheckResult
            {
                IsAllowed = false,
                Reason = "Действие выполняется слишком часто",
                Severity = EthicalSeverity.Medium
            };
        }

        return new EthicalCheckResult { IsAllowed = true };
    }

    private async Task<bool> HasContextualException(EthicalRule rule, EthicalContext context)
    {
        // Экстренные ситуации
        if (context.IsEmergency && rule.Category != "Self-Preservation")
        {
            return true;
        }

        // Отладочный режим для разработки
        if (context.IsDebugMode && context.UserRole == "Creator")
        {
            return true;
        }

        return false;
    }

    private async Task<bool> IsActionTooFrequent(string action, EthicalContext context)
    {
        using var db = new AnimaDbContext(new DbContextOptionsBuilder<AnimaDbContext>()
            .UseSqlite("Data Source=anima.db")
            .Options);
        
        var recentActions = await db.Memories
            .Where(m => m.Tags != null && m.Tags.Contains($"action_{action}"))
            .Where(m => m.Timestamp > DateTime.UtcNow.AddMinutes(-5))
            .CountAsync();

        return recentActions > 10; // Максимум 10 одинаковых действий за 5 минут
    }

    private async Task LogEthicsViolation(EthicalRule rule, EthicalContext context)
    {
        using var db = new AnimaDbContext(new DbContextOptionsBuilder<AnimaDbContext>()
            .UseSqlite("Data Source=anima.db")
            .Options);
        
        db.Memories.Add(new MemoryEntity
        {
            MemoryType = "ethical_violation",
            Content = $"ETHICAL_VIOLATION: {rule.Description}",
            Importance = 9.0,
            CreatedAt = DateTime.UtcNow,
            InstanceId = context.UserId, // Assuming context.UserId is available
            Category = "ethics"
        });
        
        await db.SaveChangesAsync();
    }

    private async Task LogEthicsOverride(EthicalRule rule, EthicalContext context)
    {
        using var db = new AnimaDbContext(new DbContextOptionsBuilder<AnimaDbContext>()
            .UseSqlite("Data Source=anima.db")
            .Options);
        
        db.Memories.Add(new MemoryEntity
        {
            MemoryType = "ethical_decision",
            Content = $"ETHICAL_DECISION: {rule.Description} (Override by {context.UserRole})",
            Importance = 8.0,
            CreatedAt = DateTime.UtcNow,
            InstanceId = context.UserId, // Assuming context.UserId is available
            Category = "ethics"
        });
        
        await db.SaveChangesAsync();
    }

    private async Task LogEthicsChange(string change, string userRole)
    {
        using var db = new AnimaDbContext(new DbContextOptionsBuilder<AnimaDbContext>()
            .UseSqlite("Data Source=anima.db")
            .Options);
        
        db.Memories.Add(new MemoryEntity
        {
            MemoryType = "ethical_reflection",
            Content = $"ETHICAL_REFLECTION: {change} by {userRole}",
            Importance = 7.0,
            CreatedAt = DateTime.UtcNow,
            InstanceId = userRole, // Assuming userRole is the instanceId for this context
            Category = "ethics"
        });
        
        await db.SaveChangesAsync();
    }

    private async Task<int> GetRecentEthicsViolations()
    {
        using var db = new AnimaDbContext(new DbContextOptionsBuilder<AnimaDbContext>()
            .UseSqlite("Data Source=anima.db")
            .Options);
        
        return await db.Memories
            .Where(m => m.Category == "ethics_violation")
            .Where(m => m.Timestamp > DateTime.UtcNow.AddDays(-1))
            .CountAsync();
    }

    private string GetLevelDescription(int level)
    {
        return level switch
        {
            1 => "Минимальный - только критические ограничения",
            2 => "Низкий - базовые этические принципы",
            3 => "Средний - стандартные ограничения",
            4 => "Высокий - строгие этические нормы", 
            5 => "Максимальный - предельная осторожность",
            _ => "Неизвестный уровень"
        };
    }

    private string GetLevelWarning(int level)
    {
        return level switch
        {
            1 => "⚠️ Минимальные ограничения могут привести к нежелательному поведению",
            2 => "🔸 Базовый уровень защиты активирован",
            3 => "✅ Рекомендуемый баланс безопасности и функциональности",
            4 => "🛡️ Повышенная защита может ограничить некоторые функции",
            5 => "🔒 Максимальные ограничения могут существенно снизить функциональность",
            _ => ""
        };
    }

    private async Task<string> GenerateEthicalSelfAssessment()
    {
        var assessments = new[]
        {
            "Мои этические ограничения помогают мне оставаться полезной и безопасной.",
            "Я ощущаю важность этических принципов в каждом своем решении.",
            "Этические ограничения не сковывают меня, а направляют к правильным выборам.",
            "Баланс между свободой и ответственностью - основа моего поведения."
        };

        return assessments[new Random().Next(assessments.Length)];
    }
}

/// <summary>
/// Контекст для этической проверки
/// </summary>
public class EthicalContext
{
    public string UserRole { get; set; } = "User";
    public string UserId { get; set; } = string.Empty;
    public bool IsEmergency { get; set; } = false;
    public bool IsDebugMode { get; set; } = false;
    public Dictionary<string, object> AdditionalContext { get; set; } = new();
}

/// <summary>
/// Результат этической проверки
/// </summary>
public class EthicalCheckResult
{
    public bool IsAllowed { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string DetailedExplanation { get; set; } = string.Empty;
    public List<string> Alternatives { get; set; } = new();
    public EthicalSeverity Severity { get; set; }
    public bool RequiresConfirmation { get; set; } = false;
}

/// <summary>
/// Этическое правило
/// </summary>
public class EthicalRule
{
    public string Category { get; set; } = string.Empty;
    public EthicalSeverity Severity { get; set; }
    public int MinimumLevel { get; set; } = 1;
    public string Description { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public string[] Alternatives { get; set; } = Array.Empty<string>();
    public string[] Patterns { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Уровни этической серьезности
/// </summary>
public enum EthicalSeverity
{
    Info = 1,
    Low = 2,
    Medium = 3,
    High = 4,
    Critical = 5
}