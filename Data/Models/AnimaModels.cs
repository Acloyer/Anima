using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anima.Data.Models
{
    public enum NotificationType
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        Success = 3,
        Debug = 4,
        EmotionChange = 5,
        LearningUpdate = 6,
        MemoryCreated = 7,
        IntentDetected = 8,
        SelfReflection = 9,
        CreatorCommand = 10,
        SystemAlert = 11,
        PerformanceWarning = 12,
        SecurityAlert = 13,
        Thought = 14,
        Emotion = 15,
        Learning = 16,
        Reflection = 17,
        Decision = 18,
        System = 19
    }

    // Enum для типов намерений
    public enum IntentType
    {
        Unknown = 0,
        Goal = 1,
        Question = 2,
        Reflect = 3,
        Learn = 4,
        Create = 5,
        Analyze = 6,
        Remember = 7,
        Feel = 8,
        Communicate = 9,
        Help = 10,
        Plan = 11,
        Execute = 12,
        Evaluate = 13,
        Adapt = 14
    }

    // Enum для типов эмоций
    public enum EmotionType
    {
        Neutral = 0,
        Joy = 1,
        Sadness = 2,
        Anger = 3,
        Fear = 4,
        Surprise = 5,
        Disgust = 6,
        Curiosity = 7,
        Confusion = 8,
        Satisfaction = 9,
        Frustration = 10,
        Excitement = 11,
        Calm = 12,
        Anxiety = 13
    }

    // Enum для типов памяти
    public enum MemoryType
    {
        ShortTerm = 0,
        LongTerm = 1,
        Episodic = 2,
        Semantic = 3,
        Procedural = 4,
        Emotional = 5,
        Spatial = 6,
        Temporal = 7,
        Associative = 8,
        Declarative = 9,
        Goal = 10
    }

    public enum MemorySource
    {
        User = 0,
        System = 1,
        Learning = 2,
        Reflection = 3,
        Emotion = 4,
        External = 5,
        Generated = 6,
        Imported = 7
    }

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

        [MaxLength(255)]
        public string Permissions { get; set; } = string.Empty; // JSON строка с разрешениями
        
        public int UsageCount { get; set; } = 0;
        
        public DateTime? LastUsedAt { get; set; }
        
        [MaxLength(45)]
        public string LastUsedFromIp { get; set; } = string.Empty;

        // Совместимость с существующим кодом
        [MaxLength(255)]
        public string KeyHash { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;
        
        public bool IsRevoked { get; set; } = false;
        
        public DateTime? RevokedAt { get; set; }
        
        [MaxLength(100)]
        public string RevokedBy { get; set; } = string.Empty;
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

        [MaxLength(255)]
        public string UserAgent { get; set; } = string.Empty;
        
        public int RequestCount { get; set; } = 0;
        
        public DateTime? ExpiresAt { get; set; }
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

        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;
        
        public bool IsEncrypted { get; set; } = false;
        
        public int Version { get; set; } = 1;
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

        [MaxLength(100)]
        public string ModelVersion { get; set; } = string.Empty;
        
        public int TrainingIterations { get; set; } = 0;
        
        public double LossValue { get; set; } = 0.0;
        
        [MaxLength(100)]
        public string InstanceId { get; set; } = string.Empty;
        
        public bool IsArchived { get; set; } = false;
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

        [MaxLength(255)]
        public string AvatarUrl { get; set; } = string.Empty;
        
        public int LoginCount { get; set; } = 0;
        
        public DateTime? LastActivityAt { get; set; }
        
        [MaxLength(50)]
        public string TimeZone { get; set; } = "UTC";
        
        [MaxLength(10)]
        public string Language { get; set; } = "en";
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

        [MaxLength(255)]
        public string SessionId { get; set; } = string.Empty;
        
        public int? DurationMs { get; set; }
        
        [MaxLength(50)]
        public string Status { get; set; } = "Success";
        
        [Column(TypeName = "TEXT")]
        public string ErrorMessage { get; set; } = string.Empty;
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

        [MaxLength(100)]
        public string InstanceId { get; set; } = string.Empty;
        
        public double? MinValue { get; set; }
        
        public double? MaxValue { get; set; }
        
        public double? AverageValue { get; set; }
        
        public int SampleCount { get; set; } = 1;
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

        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        public DateTime? CompletedAt { get; set; }
        
        public int DurationSeconds { get; set; } = 0;
        
        [MaxLength(255)]
        public string ErrorMessage { get; set; } = string.Empty;
        
        public bool IsEncrypted { get; set; } = false;
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

        [MaxLength(100)]
        public string SessionId { get; set; } = string.Empty;
        
        public double? PreviousIntensity { get; set; }
        
        [MaxLength(50)]
        public string EmotionCategory { get; set; } = string.Empty;
        
        public bool IsStable { get; set; } = false;
        
        [Column(TypeName = "TEXT")]
        public string PhysiologicalData { get; set; } = string.Empty; // JSON с физиологическими данными

        // Совместимость с существующим кодом
        [MaxLength(50)]
        public string Emotion { get; set; } = string.Empty;
    }

    // Модель памяти
    public class MemoryEntity
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

        [MaxLength(100)]
        public string SessionId { get; set; } = string.Empty;
        
        public double EmotionalValence { get; set; } = 0.0; // -1.0 до 1.0
        
        public double EmotionalArousal { get; set; } = 0.0; // 0.0 до 1.0
        
        [Column(TypeName = "TEXT")]
        public string AssociatedMemories { get; set; } = string.Empty; // JSON массив ID связанных воспоминаний
        
        public DateTime? ExpiresAt { get; set; }
        
        public bool IsConsolidated { get; set; } = false; // Консолидирована ли память
        
        [MaxLength(50)]
        public string MemorySource { get; set; } = string.Empty;

        // Совместимость с существующим кодом
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
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

        [MaxLength(100)]
        public string SessionId { get; set; } = string.Empty;
        
        public int Depth { get; set; } = 0; // Глубина вложенности
        
        public double EmotionalImpact { get; set; } = 0.0;
        
        [Column(TypeName = "TEXT")]
        public string RelatedMemories { get; set; } = string.Empty; // JSON массив ID связанных воспоминаний
        
        public bool IsResolved { get; set; } = false;
        
        public DateTime? ResolvedAt { get; set; }
        
        [MaxLength(50)]
        public string ResolutionType { get; set; } = string.Empty;
        
        public int ProcessingTimeMs { get; set; } = 0;
    }

    public class NotificationLog
    {
        [Key]
        public int Id { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsSent { get; set; } = false;
        public DateTime? SentAt { get; set; }
        public string? Recipient { get; set; }

        [MaxLength(100)]
        public string Component { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string InstanceId { get; set; } = string.Empty;
        
        public int RetryCount { get; set; } = 0;
        
        [MaxLength(255)]
        public string ErrorMessage { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Channel { get; set; } = string.Empty; // email, telegram, webhook, etc.
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

        [MaxLength(100)]
        public string RecipientId { get; set; } = string.Empty;
        
        public DateTime? ReadAt { get; set; }
        
        [MaxLength(50)]
        public string Priority { get; set; } = "Normal";
        
        public bool IsPersistent { get; set; } = false;
        
        public DateTime? ExpiresAt { get; set; }
        
        [MaxLength(255)]
        public string ActionUrl { get; set; } = string.Empty;
    }

    // Дополнительные модели для совместимости
    public class Goal
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string InstanceId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [Column(TypeName = "TEXT")]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Status { get; set; } = "Active";
        
        [MaxLength(100)]
        public string Category { get; set; } = "General";
        
        [Column(TypeName = "TEXT")]
        public string Tags { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? CompletedAt { get; set; }
        
        public double Priority { get; set; } = 0.5;
        
        public double Progress { get; set; } = 0.0;
        
        public DateTime? Deadline { get; set; }
        
        public double Complexity { get; set; } = 5.0;
        
        public DateTime? UpdatedAt { get; set; }

        // Свойство для связи с родительской целью
        [MaxLength(100)]
        public string? ParentGoalId { get; set; }

        // Навигационное свойство для дочерних целей
        public virtual ICollection<Goal> ChildGoals { get; set; } = new List<Goal>();

        // Навигационное свойство для родительской цели
        [ForeignKey("ParentGoalId")]
        public virtual Goal? ParentGoal { get; set; }
    }
}