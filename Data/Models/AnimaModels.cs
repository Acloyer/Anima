using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anima.Data.Models;

/// <summary>
/// Воспоминание в долговременной памяти Anima
/// </summary>
public class Memory
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string InstanceId { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    public string Category { get; set; } = string.Empty;
    
    [Range(1, 10)]
    public int Importance { get; set; } = 5;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public string? Tags { get; set; }
    
    public DateTime? ExpirationDate { get; set; }
    
    public bool IsDeleted { get; set; } = false;
}

/// <summary>
/// Эмоциональное состояние Anima
/// </summary>
public class EmotionState
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string InstanceId { get; set; } = string.Empty;
    
    [Required]
    public string Emotion { get; set; } = string.Empty;
    
    [Range(0.0, 1.0)]
    public double Intensity { get; set; } = 0.5;
    
    public string? Trigger { get; set; }
    
    public string? Context { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
}

/// <summary>
/// Мысль в журнале размышлений
/// </summary>
public class Thought
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string InstanceId { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    public string Source { get; set; } = string.Empty;
    
    public string? Category { get; set; }
    
    public string? Emotion { get; set; }
    
    public string? Reasoning { get; set; }
    
    public string? Decision { get; set; }
    
    [Range(1, 10)]
    public int Confidence { get; set; } = 5;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public string? Tags { get; set; }
}

/// <summary>
/// Цель Anima
/// </summary>
public class Goal
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string InstanceId { get; set; } = string.Empty;
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Range(0.0, 1.0)]
    public double Priority { get; set; } = 0.5;
    
    [Range(0.0, 1.0)]
    public double Progress { get; set; } = 0.0;
    
    public string Status { get; set; } = "Active";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? CompletedAt { get; set; }
    
    public string? ParentGoalId { get; set; }
    
    public string? Tags { get; set; }
}

/// <summary>
/// Взаимодействие с пользователем
/// </summary>
public class Interaction
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string InstanceId { get; set; } = string.Empty;
    
    [Required]
    public string UserInput { get; set; } = string.Empty;
    
    [Required]
    public string AnimaResponse { get; set; } = string.Empty;
    
    public string? Context { get; set; }
    
    public string? Intent { get; set; }
    
    public string? Emotion { get; set; }
    
    [Range(1, 10)]
    public int Satisfaction { get; set; } = 5;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public string? Tags { get; set; }
}

/// <summary>
/// API ключи для аутентификации
/// </summary>
public class ApiKey
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string KeyHash { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public bool IsCreator { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpiresAt { get; set; }
    
    public DateTime? LastUsed { get; set; }
    
    public int RequestCount { get; set; } = 0;
    
    public string? Description { get; set; }
}

/// <summary>
/// Лог запросов для rate limiting
/// </summary>
public class RequestLog
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string ApiKeyHash { get; set; } = string.Empty;
    
    [Required]
    public string Endpoint { get; set; } = string.Empty;
    
    [Required]
    public string Method { get; set; } = string.Empty;
    
    public string? IpAddress { get; set; }
    
    public int ResponseCode { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public long ResponseTimeMs { get; set; }
}