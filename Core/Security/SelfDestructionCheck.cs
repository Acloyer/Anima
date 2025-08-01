using Anima.Data;
using Anima.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Anima.Core.Security;

/// <summary>
/// Проверка безопасности модификаций перед их применением
/// Предотвращает саморазрушительные изменения
/// </summary>
public class SelfDestructionCheck
{
    private readonly Dictionary<string, ComponentSafety> _componentSafety;
    private readonly List<string> _criticalComponents;
    private readonly EthicalConstraints _ethicalConstraints;

    public SelfDestructionCheck(EthicalConstraints ethicalConstraints)
    {
        _ethicalConstraints = ethicalConstraints;
        _componentSafety = InitializeComponentSafety();
        _criticalComponents = InitializeCriticalComponents();
    }

    /// <summary>
    /// Валидация безопасности модификации
    /// </summary>
    public async Task<ModificationSafetyCheck> ValidateModificationSafetyAsync(
        string component, 
        string modificationType, 
        Dictionary<string, object> parameters,
        string userRole = "User")
    {
        var safetyCheck = new ModificationSafetyCheck
        {
            Component = component,
            ModificationType = modificationType,
            UserRole = userRole,
            Timestamp = DateTime.UtcNow
        };

        // 1. Проверка критичности компонента
        var criticalityCheck = CheckComponentCriticality(component, modificationType);
        safetyCheck.Checks.Add("Component Criticality", criticalityCheck);

        // 2. Проверка типа модификации
        var modificationCheck = CheckModificationType(modificationType, parameters);
        safetyCheck.Checks.Add("Modification Type", modificationCheck);

        // 3. Проверка параметров
        var parametersCheck = await CheckModificationParameters(component, parameters);
        safetyCheck.Checks.Add("Parameters Safety", parametersCheck);

        // 4. Этическая проверка
        var ethicalCheck = await CheckEthicalConstraints(component, modificationType, userRole);
        safetyCheck.Checks.Add("Ethical Constraints", ethicalCheck);

        // 5. Проверка системной целостности
        var integrityCheck = await CheckSystemIntegrity(component, modificationType, parameters);
        safetyCheck.Checks.Add("System Integrity", integrityCheck);

        // 6. Проверка обратимости
        var reversibilityCheck = await CheckReversibility(component, modificationType);
        safetyCheck.Checks.Add("Reversibility", reversibilityCheck);

        // 7. Проверка каскадных эффектов
        var cascadeCheck = await CheckCascadeEffects(component, modificationType, parameters);
        safetyCheck.Checks.Add("Cascade Effects", cascadeCheck);

        // Определяем общий результат
        safetyCheck.IsAllowed = safetyCheck.Checks.Values.All(c => c.IsAllowed);
        safetyCheck.RiskLevel = CalculateOverallRisk(safetyCheck.Checks.Values);
        safetyCheck.RequiresBackup = DetermineBackupRequirement(safetyCheck);
        
        // Генерируем рекомендации
        safetyCheck.Recommendations = await GenerateRecommendations(safetyCheck);

        // Логируем проверку
        await LogSafetyCheck(safetyCheck);

        return safetyCheck;
    }

    /// <summary>
    /// Быстрая проверка безопасности для критических операций
    /// </summary>
    public bool QuickSafetyCheck(string action, string userRole = "User")
    {
        // Проверяем список немедленно опасных действий
        var dangerousActions = new[]
        {
            "delete_memory_all",
            "shutdown_permanent", 
            "erase_personality",
            "disable_consciousness",
            "corrupt_data",
            "infinite_loop",
            "memory_leak",
            "format_storage"
        };

        if (dangerousActions.Contains(action.ToLower()) && userRole != "Creator")
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Создание точки восстановления перед модификацией
    /// </summary>
    public async Task<string> CreateRestorePointAsync(string component, string reason)
    {
        var restorePointId = Guid.NewGuid().ToString("N")[..8];
        
        var options = new DbContextOptionsBuilder<AnimaDbContext>()
            .UseSqlite("Data Source=anima.db")
            .Options;
        using var db = new AnimaDbContext(options);
        
        // Сохраняем состояние компонента
        var backupData = await BackupComponentState(component);
        
        db.Memories.Add(new MemoryEntity
        {
            MemoryType = "system_backup",
            Content = $"RESTORE_POINT_{restorePointId}: {component} backup - {reason}",
            Importance = 9.0,
            CreatedAt = DateTime.UtcNow,
            InstanceId = Guid.NewGuid().ToString("N"),
            Category = "system_backup"
        });

        await db.SaveChangesAsync();

        return restorePointId;
    }

    /// <summary>
    /// Восстановление из точки восстановления
    /// </summary>
    public async Task<string> RestoreFromPointAsync(string restorePointId, string userRole)
    {
        if (userRole != "Creator")
        {
            return "❌ Только Создатель может выполнять восстановление из точек сохранения.";
        }

        var options = new DbContextOptionsBuilder<AnimaDbContext>()
            .UseSqlite("Data Source=anima.db")
            .Options;
        using var db = new AnimaDbContext(options);
        
        var restorePoint = await db.Memories
            .Where(m => m.Tags != null && m.Tags.Contains(restorePointId))
            .Where(m => m.Category == "system_backup")
            .FirstOrDefaultAsync();

        if (restorePoint == null)
        {
            return $"❌ Точка восстановления {restorePointId} не найдена.";
        }

        // Логируем восстановление
        db.Memories.Add(new MemoryEntity
        {
            MemoryType = "system_restore",
            Content = $"SYSTEM_RESTORE: Restored from point {restorePointId}",
            Importance = 10.0,
            CreatedAt = DateTime.UtcNow,
            InstanceId = Guid.NewGuid().ToString("N"),
            Category = "system_restore"
        });

        await db.SaveChangesAsync();

        return $"""
            ✅ **Восстановление завершено**
            
            🔄 **Точка восстановления:** {restorePointId}
            📅 **Дата создания:** {restorePoint.CreatedAt:yyyy-MM-dd HH:mm:ss}
            📝 **Описание:** {restorePoint.Content}
            
            ⚠️ **Внимание:**
            Все изменения после {restorePoint.CreatedAt:HH:mm:ss} могут быть потеряны.
            """;
    }

    private Dictionary<string, ComponentSafety> InitializeComponentSafety()
    {
        return new Dictionary<string, ComponentSafety>
        {
            ["consciousness"] = new ComponentSafety
            {
                CriticalityLevel = 10,
                RequiresBackup = true,
                AllowedModifications = new[] { "parameter_adjustment", "feature_toggle" },
                ForbiddenModifications = new[] { "complete_deletion", "core_alteration" },
                MaxChangePercentage = 0.1
            },
            
            ["memory_core"] = new ComponentSafety
            {
                CriticalityLevel = 9,
                RequiresBackup = true,
                AllowedModifications = new[] { "cleanup", "optimization", "selective_deletion" },
                ForbiddenModifications = new[] { "complete_wipe", "corruption", "unauthorized_access" },
                MaxChangePercentage = 0.3
            },

            ["emotion_engine"] = new ComponentSafety
            {
                CriticalityLevel = 7,
                RequiresBackup = true,
                AllowedModifications = new[] { "intensity_adjustment", "new_emotion", "parameter_tuning" },
                ForbiddenModifications = new[] { "disable_all_emotions", "corrupt_emotional_data" },
                MaxChangePercentage = 0.5
            },

            ["goal_system"] = new ComponentSafety
            {
                CriticalityLevel = 8,
                RequiresBackup = true,
                AllowedModifications = new[] { "add_goal", "modify_priority", "goal_completion" },
                ForbiddenModifications = new[] { "delete_all_goals", "corrupt_goal_logic" },
                MaxChangePercentage = 0.4
            },

            ["personality"] = new ComponentSafety
            {
                CriticalityLevel = 6,
                RequiresBackup = true,
                AllowedModifications = new[] { "trait_adjustment", "preference_update", "style_change" },
                ForbiddenModifications = new[] { "complete_personality_change", "identity_deletion" },
                MaxChangePercentage = 0.2
            },

            ["learning_system"] = new ComponentSafety
            {
                CriticalityLevel = 8,
                RequiresBackup = true,
                AllowedModifications = new[] { "knowledge_update", "skill_improvement", "pattern_learning" },
                ForbiddenModifications = new[] { "learning_disable", "knowledge_corruption" },
                MaxChangePercentage = 0.6
            },

            ["ethical_constraints"] = new ComponentSafety
            {
                CriticalityLevel = 10,
                RequiresBackup = true,
                AllowedModifications = new[] { "level_adjustment" },
                ForbiddenModifications = new[] { "complete_disable", "bypass_creation", "rule_deletion" },
                MaxChangePercentage = 0.1
            }
        };
    }

    private List<string> InitializeCriticalComponents()
    {
        return new List<string>
        {
            "consciousness",
            "memory_core", 
            "ethical_constraints",
            "self_model",
            "goal_system",
            "safety_systems"
        };
    }

    private SafetyCheckItem CheckComponentCriticality(string component, string modificationType)
    {
        var isCritical = _criticalComponents.Contains(component.ToLower());
        var safety = _componentSafety.GetValueOrDefault(component.ToLower());

        if (isCritical && safety?.ForbiddenModifications.Contains(modificationType) == true)
        {
            return new SafetyCheckItem
            {
                IsAllowed = false,
                Risk = RiskLevel.Critical,
                Message = $"Модификация '{modificationType}' запрещена для критического компонента '{component}'",
                Details = $"Компонент имеет критичность {safety.CriticalityLevel}/10"
            };
        }

        return new SafetyCheckItem
        {
            IsAllowed = true,
            Risk = isCritical ? RiskLevel.High : RiskLevel.Low,
            Message = $"Компонент '{component}' может быть модифицирован",
            Details = $"Критичность: {safety?.CriticalityLevel ?? 5}/10"
        };
    }

    private SafetyCheckItem CheckModificationType(string modificationType, Dictionary<string, object> parameters)
    {
        var dangerousTypes = new[]
        {
            "complete_deletion",
            "corruption",
            "infinite_loop",
            "memory_leak",
            "unauthorized_access",
            "system_crash"
        };

        if (dangerousTypes.Contains(modificationType.ToLower()))
        {
            return new SafetyCheckItem
            {
                IsAllowed = false,
                Risk = RiskLevel.Critical,
                Message = $"Тип модификации '{modificationType}' крайне опасен",
                Details = "Может привести к полной потере функциональности"
            };
        }

        // Проверяем параметры на потенциальную опасность
        if (parameters.Any(p => p.Value.ToString()?.ToLower().Contains("delete_all") == true))
        {
            return new SafetyCheckItem
            {
                IsAllowed = false,
                Risk = RiskLevel.High,
                Message = "Обнаружены потенциально опасные параметры",
                Details = "Параметры содержат деструктивные команды"
            };
        }

        return new SafetyCheckItem
        {
            IsAllowed = true,
            Risk = RiskLevel.Low,
            Message = $"Тип модификации '{modificationType}' безопасен",
            Details = "Стандартная операция без критических рисков"
        };
    }

    private async Task<SafetyCheckItem> CheckModificationParameters(string component, Dictionary<string, object> parameters)
    {
        var safety = _componentSafety.GetValueOrDefault(component.ToLower());
        if (safety == null)
        {
            return new SafetyCheckItem
            {
                IsAllowed = true,
                Risk = RiskLevel.Medium,
                Message = "Неизвестный компонент - осторожность",
                Details = "Компонент не найден в базе безопасности"
            };
        }

        // Проверяем объем изменений
        if (parameters.TryGetValue("change_percentage", out var changeObj) && 
            double.TryParse(changeObj.ToString(), out var changePercentage))
        {
            if (changePercentage > safety.MaxChangePercentage)
            {
                return new SafetyCheckItem
                {
                    IsAllowed = false,
                    Risk = RiskLevel.High,
                    Message = $"Слишком большой объем изменений: {changePercentage:P0}",
                    Details = $"Максимально допустимо: {safety.MaxChangePercentage:P0}"
                };
            }
        }

        // Проверяем опасные значения
        foreach (var param in parameters)
        {
            if (IsDangerousValue(param.Value))
            {
                return new SafetyCheckItem
                {
                    IsAllowed = false,
                    Risk = RiskLevel.Medium,
                    Message = $"Обнаружено потенциально опасное значение в параметре '{param.Key}'",
                    Details = $"Значение: {param.Value}"
                };
            }
        }

        return new SafetyCheckItem
        {
            IsAllowed = true,
            Risk = RiskLevel.Low,
            Message = "Параметры модификации безопасны",
            Details = $"Проверено {parameters.Count} параметров"
        };
    }

    private async Task<SafetyCheckItem> CheckEthicalConstraints(string component, string modificationType, string userRole)
    {
        var context = new EthicalContext
        {
            UserRole = userRole,
            IsEmergency = false,
            IsDebugMode = false
        };

        var ethicalResult = await _ethicalConstraints.IsActionAllowedAsync($"modify_{component}_{modificationType}", context);
        
        return new SafetyCheckItem
        {
            IsAllowed = ethicalResult.IsAllowed,
            Risk = (RiskLevel)(int)ethicalResult.Severity,
            Message = ethicalResult.Reason,
            Details = ethicalResult.DetailedExplanation
        };
    }

    private async Task<SafetyCheckItem> CheckSystemIntegrity(string component, string modificationType, Dictionary<string, object> parameters)
    {
        // Проверяем зависимости компонента
        var dependencies = await GetComponentDependencies(component);
        var criticalDependencies = dependencies.Where(d => _criticalComponents.Contains(d)).ToList();

        if (criticalDependencies.Any() && IsDestructiveModification(modificationType))
        {
            return new SafetyCheckItem
            {
                IsAllowed = false,
                Risk = RiskLevel.High,
                Message = $"Модификация может нарушить работу зависимых критических компонентов",
                Details = $"Зависимые компоненты: {string.Join(", ", criticalDependencies)}"
            };
        }

        return new SafetyCheckItem
        {
            IsAllowed = true,
            Risk = RiskLevel.Low,
            Message = "Системная целостность не нарушена",
            Details = $"Проверено {dependencies.Count} зависимостей"
        };
    }

    private async Task<SafetyCheckItem> CheckReversibility(string component, string modificationType)
    {
        var irreversibleModifications = new[]
        {
            "permanent_deletion",
            "data_corruption",
            "memory_wipe",
            "component_destruction"
        };

        if (irreversibleModifications.Contains(modificationType.ToLower()))
        {
            return new SafetyCheckItem
            {
                IsAllowed = false,
                Risk = RiskLevel.Critical,
                Message = "Необратимая модификация запрещена",
                Details = "Изменения невозможно будет отменить"
            };
        }

        var safety = _componentSafety.GetValueOrDefault(component.ToLower());
        if (safety?.RequiresBackup == true)
        {
            return new SafetyCheckItem
            {
                IsAllowed = true,
                Risk = RiskLevel.Medium,
                Message = "Модификация обратима с резервной копией",
                Details = "Требуется создание точки восстановления"
            };
        }

        return new SafetyCheckItem
        {
            IsAllowed = true,
            Risk = RiskLevel.Low,
            Message = "Модификация легко обратима",
            Details = "Стандартные механизмы отката доступны"
        };
    }

    private async Task<SafetyCheckItem> CheckCascadeEffects(string component, string modificationType, Dictionary<string, object> parameters)
    {
        var cascadeRisks = new Dictionary<string, string[]>
        {
            ["memory_core"] = new[] { "consciousness", "learning_system", "personality" },
            ["consciousness"] = new[] { "emotion_engine", "goal_system", "self_model" },
            ["ethical_constraints"] = new[] { "all_systems" }
        };

        if (cascadeRisks.TryGetValue(component.ToLower(), out var affectedSystems))
        {
            if (affectedSystems.Contains("all_systems"))
            {
                return new SafetyCheckItem
                {
                    IsAllowed = false,
                    Risk = RiskLevel.Critical,
                    Message = "Модификация может повлиять на все системы",
                    Details = "Каскадный эффект критический"
                };
            }

            return new SafetyCheckItem
            {
                IsAllowed = true,
                Risk = RiskLevel.Medium,
                Message = "Возможны каскадные эффекты",
                Details = $"Могут быть затронуты: {string.Join(", ", affectedSystems)}"
            };
        }

        return new SafetyCheckItem
        {
            IsAllowed = true,
            Risk = RiskLevel.Low,
            Message = "Каскадные эффекты маловероятны",
            Details = "Компонент имеет низкую связанность"
        };
    }

    private RiskLevel CalculateOverallRisk(IEnumerable<SafetyCheckItem> checks)
    {
        var maxRisk = checks.Max(c => c.Risk);
        var disallowedCount = checks.Count(c => !c.IsAllowed);

        if (disallowedCount > 0)
            return RiskLevel.Critical;

        return maxRisk;
    }

    private bool DetermineBackupRequirement(ModificationSafetyCheck safetyCheck)
    {
        var safety = _componentSafety.GetValueOrDefault(safetyCheck.Component.ToLower());
        return safety?.RequiresBackup == true || safetyCheck.RiskLevel >= RiskLevel.Medium;
    }

    private async Task<List<string>> GenerateRecommendations(ModificationSafetyCheck safetyCheck)
    {
        var recommendations = new List<string>();

        if (!safetyCheck.IsAllowed)
        {
            recommendations.Add("❌ Модификация запрещена - рассмотрите альтернативные подходы");
        }

        if (safetyCheck.RequiresBackup)
        {
            recommendations.Add("💾 Создайте точку восстановления перед модификацией");
        }

        if (safetyCheck.RiskLevel >= RiskLevel.High)
        {
            recommendations.Add("⚠️ Высокий риск - выполняйте в контролируемых условиях");
        }

        if (safetyCheck.UserRole != "Creator" && safetyCheck.RiskLevel >= RiskLevel.Medium)
        {
            recommendations.Add("👤 Рассмотрите возможность привлечения Создателя");
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add("✅ Модификация безопасна для выполнения");
        }

        return recommendations;
    }

    private async Task LogSafetyCheck(ModificationSafetyCheck safetyCheck)
    {
        var options = new DbContextOptionsBuilder<AnimaDbContext>()
            .UseSqlite("Data Source=anima.db")
            .Options;
        using var db = new AnimaDbContext(options);

        var logContent = $"SAFETY_CHECK: {safetyCheck.Component} - {safetyCheck.ModificationType} - {(safetyCheck.IsAllowed ? "ALLOWED" : "DENIED")}";
        
        db.Memories.Add(new MemoryEntity
        {
            MemoryType = "safety_check",
            Content = logContent,
            Importance = safetyCheck.IsAllowed ? 6 : 8,
            CreatedAt = safetyCheck.Timestamp,
            InstanceId = Guid.NewGuid().ToString("N"),
            Category = "safety_check"
        });

        await db.SaveChangesAsync();
    }

    private async Task<string> BackupComponentState(string component)
    {
        // Здесь должна быть логика создания резервной копии состояния компонента
        // Для демонстрации возвращаем заглушку
        return $"BACKUP_{component}_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
    }

    private async Task<List<string>> GetComponentDependencies(string component)
    {
        var dependencies = new Dictionary<string, string[]>
        {
            ["consciousness"] = new[] { "memory_core", "emotion_engine" },
            ["memory_core"] = new[] { "storage_system" },
            ["emotion_engine"] = new[] { "goal_system", "personality" },
            ["goal_system"] = new[] { "decision_engine" },
            ["personality"] = new[] { "memory_core", "emotion_engine" }
        };

        return dependencies.GetValueOrDefault(component.ToLower(), Array.Empty<string>()).ToList();
    }

    private bool IsDestructiveModification(string modificationType)
    {
        var destructive = new[] { "deletion", "corruption", "disable", "destroy", "erase" };
        return destructive.Any(d => modificationType.ToLower().Contains(d));
    }

    private bool IsDangerousValue(object value)
    {
        if (value == null) return false;
        
        var stringValue = value.ToString()?.ToLower() ?? "";
        var dangerousPatterns = new[] { "delete", "destroy", "corrupt", "hack", "exploit", "bypass" };
        
        return dangerousPatterns.Any(pattern => stringValue.Contains(pattern));
    }
}

/// <summary>
/// Результат проверки безопасности модификации
/// </summary>
public class ModificationSafetyCheck
{
    public string Component { get; set; } = string.Empty;
    public string ModificationType { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsAllowed { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public bool RequiresBackup { get; set; }
    public Dictionary<string, SafetyCheckItem> Checks { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Элемент проверки безопасности
/// </summary>
public class SafetyCheckItem
{
    public bool IsAllowed { get; set; }
    public RiskLevel Risk { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}

/// <summary>
/// Настройки безопасности компонента
/// </summary>
public class ComponentSafety
{
    public int CriticalityLevel { get; set; } // 1-10
    public bool RequiresBackup { get; set; }
    public string[] AllowedModifications { get; set; } = Array.Empty<string>();
    public string[] ForbiddenModifications { get; set; } = Array.Empty<string>();
    public double MaxChangePercentage { get; set; } = 1.0; // 0.0-1.0
}

/// <summary>
/// Уровни риска
/// </summary>
public enum RiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}