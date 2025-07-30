using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anima.Data.Models
{
    // Модель API ключа для аутентификации
    public class APIKey
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string KeyValue { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ExpiresAt { get; set; }
        
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
    }

    // Модель пользовательской сессии
    public class UserSession
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string SessionId { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime LastAccessAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        [MaxLength(45)]
        public string IpAddress { get; set; } = string.Empty;
        
        [Column(TypeName = "TEXT")]
        public string SessionData { get; set; } = string.Empty;
    }

    // Модель системной конфигурации
    public class SystemConfig
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;
        
        [Column(TypeName = "TEXT")]
        public string Value { get; set; } = string.Empty;
        
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
    }

    // Модель данных обучения
    public class LearningData
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;
        
        [Column(TypeName = "TEXT")]
        public string InputData { get; set; } = string.Empty;
        
        [Column(TypeName = "TEXT")]
        public string OutputData { get; set; } = string.Empty;
        
        [Column(TypeName = "TEXT")]
        public string Context { get; set; } = string.Empty;
        
        public double ConfidenceScore { get; set; } = 0.0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsValidated { get; set; } = false;
        
        [MaxLength(255)]
        public string Source { get; set; } = string.Empty;
    }

    // Модель профиля пользователя
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Column(TypeName = "TEXT")]
        public string Preferences { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        [MaxLength(100)]
        public string Role { get; set; } = "User";
    }

    // Модель аудита системы
    public class SystemAudit
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;
        
        [Column(TypeName = "TEXT")]
        public string Details { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [MaxLength(45)]
        public string IpAddress { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string Component { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Severity { get; set; } = "Info";
    }

    // Модель метрик производительности
    public class PerformanceMetric
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string MetricName { get; set; } = string.Empty;
        
        public double Value { get; set; } = 0.0;
        
        [MaxLength(50)]
        public string Unit { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string Component { get; set; } = string.Empty;
        
        [Column(TypeName = "TEXT")]
        public string Metadata { get; set; } = string.Empty;
    }

    // Модель системного бэкапа
    public class SystemBackup
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string BackupName { get; set; } = string.Empty;
        
        [MaxLength(255)]
        public string FilePath { get; set; } = string.Empty;
        
        public long FileSize { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(50)]
        public string BackupType { get; set; } = "Full";
        
        [MaxLength(50)]
        public string Status { get; set; } = "Completed";
        
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
        
        public bool IsCompressed { get; set; } = true;
        
        [MaxLength(64)]
        public string CheckSum { get; set; } = string.Empty;
    }

    // Модель эмоционального состояния
    public class EmotionState
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string EmotionType { get; set; } = string.Empty;
        
        public double Intensity { get; set; } = 0.0;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [Column(TypeName = "TEXT")]
        public string Context { get; set; } = string.Empty;
        
        [Column(TypeName = "TEXT")]
        public string Trigger { get; set; } = string.Empty;
        
        public int Duration { get; set; } = 0; // в секундах
        
        [MaxLength(100)]
        public string InstanceId { get; set; } = string.Empty;
    }

    // Модель памяти
    public class Memory
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string MemoryType { get; set; } = string.Empty;
        
        [Column(TypeName = "TEXT")]
        public string Content { get; set; } = string.Empty;
        
        public double Importance { get; set; } = 0.0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
        
        public int AccessCount { get; set; } = 0;
        
        [Column(TypeName = "TEXT")]
        public string Tags { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string InstanceId { get; set; } = string.Empty;
        
        public bool IsArchived { get; set; } = false;
    }

    // Модель мыслей
    public class Thought
    {
        [Key]
        public int Id { get; set; }
        
        [Column(TypeName = "TEXT")]
        public string Content { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string ThoughtType { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [Column(TypeName = "TEXT")]
        public string Context { get; set; } = string.Empty;
        
        public double Confidence { get; set; } = 0.0;
        
        [MaxLength(100)]
        public string InstanceId { get; set; } = string.Empty;
        
        public int? ParentThoughtId { get; set; }
        
        [ForeignKey("ParentThoughtId")]
        public virtual Thought? ParentThought { get; set; }
        
        public virtual ICollection<Thought> ChildThoughts { get; set; } = new List<Thought>();
    }

    // Перечисления для типов уведомлений
    public enum NotificationType
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        Success = 3,
        Debug = 4,
        Critical = 5,
        Emotion = 6,
        Learning = 7,
        Memory = 8,
        Consciousness = 9
    }

    // Модель уведомлений
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        
        public NotificationType Type { get; set; } = NotificationType.Info;
        
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;
        
        [Column(TypeName = "TEXT")]
        public string Message { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsRead { get; set; } = false;
        
        [MaxLength(100)]
        public string Component { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string InstanceId { get; set; } = string.Empty;
        
        [Column(TypeName = "TEXT")]
        public string Metadata { get; set; } = string.Empty;
    }
}