using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anima.Data.Models;

/// <summary>
/// Контекст базы данных для Anima
/// </summary>
public class AnimaDbContext : DbContext
{
    public DbSet<Memory> Memories { get; set; }
    public DbSet<EmotionState> EmotionStates { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<Thought> Thoughts { get; set; }
    public DbSet<APIKey> APIKeys { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<SystemConfig> SystemConfigs { get; set; }
    public DbSet<LearningData> LearningData { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=anima.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Конфигурация индексов
        modelBuilder.Entity<Memory>()
            .HasIndex(m => m.Category);
        
        modelBuilder.Entity<Memory>()
            .HasIndex(m => m.Timestamp);
        
        modelBuilder.Entity<Memory>()
            .HasIndex(m => m.Importance);

        modelBuilder.Entity<EmotionState>()
            .HasIndex(e => e.Timestamp);

        modelBuilder.Entity<Goal>()
            .HasIndex(g => g.Status);

        modelBuilder.Entity<APIKey>()
            .HasIndex(a => a.KeyHash)
            .IsUnique();

        modelBuilder.Entity<APIKey>()
            .HasIndex(a => a.UserId);

        // Конфигурация ограничений
        modelBuilder.Entity<APIKey>()
            .Property(a => a.KeyHash)
            .HasMaxLength(512);

        modelBuilder.Entity<Memory>()
            .Property(m => m.Content)
            .HasMaxLength(10000);

        modelBuilder.Entity<SystemConfig>()
            .HasKey(sc => sc.Key);
    }
}

/// <summary>
/// API ключ для аутентификации
/// </summary>
[Table("APIKeys")]
public class APIKey
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string KeyHash { get; set; } = string.Empty; // SHA256 хеш ключа

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "User"; // Creator, Admin, User, Demo

    [Required]
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUsedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool IsRevoked { get; set; } = false;

    public DateTime? RevokedAt { get; set; }

    [MaxLength(100)]
    public string? RevokedBy { get; set; }

    [MaxLength(100)]
    public string CreatedBy { get; set; } = "System";

    public int UsageCount { get; set; } = 0;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? AllowedIPs { get; set; } // JSON массив разрешенных IP
}

/// <summary>
/// Пользовательская сессия
/// </summary>
[Table("UserSessions")]
public class UserSession
{
    [Key]
    [MaxLength(32)]
    public string SessionId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    [MaxLength(45)]
    public string? IPAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    public bool IsRevoked { get; set; } = false;

    [MaxLength(1000)]
    public string? SessionData { get; set; } // JSON для дополнительных данных сессии
}

/// <summary>
/// Системная конфигурация
/// </summary>
[Table("SystemConfigs")]
public class SystemConfig
{
    [Key]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Value { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Category { get; set; } = "general";

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string UpdatedBy { get; set; } = "System";

    public bool IsReadOnly { get; set; } = false;

    public bool IsEncrypted { get; set; } = false;
}

/// <summary>
/// Данные обучения и адаптации
/// </summary>
[Table("LearningData")]
public class LearningData
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty; // concept, rule, pattern, skill

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(5000)]
    public string Data { get; set; } = string.Empty; // JSON данные

    [Range(0.0, 1.0)]
    public double Confidence { get; set; } = 0.5;

    [Range(0.0, 1.0)]
    public double Accuracy { get; set; } = 0.5;

    public int UsageCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUsedAt { get; set; }

    [MaxLength(200)]
    public string Source { get; set; } = "learning"; // learning, injection, evolution

    [MaxLength(300)]
    public string? Tags { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(100)]
    public string? ParentId { get; set; } // для иерархических структур

    [MaxLength(500)]
    public string? Associations { get; set; } // JSON массив связанных концептов
}

/// <summary>
/// Расширенная информация о пользователе
/// </summary>
[Table("UserProfiles")]
public class UserProfile
{
    [Key]
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DisplayName { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(100)]
    public string? PreferredLanguage { get; set; } = "ru";

    [MaxLength(50)]
    public string? TimeZone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastSeenAt { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(2000)]
    public string? Preferences { get; set; } // JSON с пользовательскими настройками

    [MaxLength(1000)]
    public string? InteractionHistory { get; set; } // JSON со статистикой взаимодействий

    public int TotalInteractions { get; set; } = 0;

    [Range(1, 10)]
    public int TrustLevel { get; set; } = 5; // уровень доверия к пользователю
}

/// <summary>
/// Аудит системных событий
/// </summary>
[Table("SystemAudit")]
public class SystemAudit
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Component { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? UserId { get; set; }

    [MaxLength(45)]
    public string? IPAddress { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string? Details { get; set; } // JSON с дополнительными деталями

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string Severity { get; set; } = "Info"; // Debug, Info, Warning, Error, Critical

    [MaxLength(100)]
    public string? CorrelationId { get; set; } // для связи связанных событий

    public bool IsAnomaly { get; set; } = false; // флаг аномального события

    [MaxLength(200)]
    public string? Source { get; set; } // источник события
}

/// <summary>
/// Метрики производительности системы
/// </summary>
[Table("PerformanceMetrics")]
public class PerformanceMetric
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string MetricName { get; set; } = string.Empty;

    [Required]
    public double Value { get; set; }

    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Component { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(300)]
    public string? Tags { get; set; } // JSON с дополнительными метками

    public TimeSpan? Duration { get; set; } // для метрик времени выполнения

    [MaxLength(500)]
    public string? Context { get; set; } // JSON с контекстом измерения
}

/// <summary>
/// Backup и восстановление данных
/// </summary>
[Table("SystemBackups")]
public class SystemBackup
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    public string BackupType { get; set; } = string.Empty; // full, incremental, component

    [MaxLength(200)]
    public string? Component { get; set; } // для компонентных бэкапов

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string CreatedBy { get; set; } = "System";

    public long SizeBytes { get; set; } = 0;

    [MaxLength(500)]
    public string? FilePath { get; set; }

    [MaxLength(128)]
    public string? Checksum { get; set; } // для проверки целостности

    public bool IsCompressed { get; set; } = false;

    public bool IsEncrypted { get; set; } = false;

    public DateTime? ExpiresAt { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Created"; // Created, InProgress, Completed, Failed

    [MaxLength(1000)]
    public string? Metadata { get; set; } // JSON с метаданными бэкапа
}