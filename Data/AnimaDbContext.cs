using Microsoft.EntityFrameworkCore;
using Anima.Data.Models;

namespace Anima.Data
{
    public class AnimaDbContext : DbContext
    {
        public AnimaDbContext(DbContextOptions<AnimaDbContext> options) : base(options)
        {
        }

        // Основные таблицы системы
        public DbSet<APIKey> APIKeys { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<SystemConfig> SystemConfigs { get; set; }
        public DbSet<LearningData> LearningData { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<SystemAudit> SystemAudits { get; set; }
        public DbSet<PerformanceMetric> PerformanceMetrics { get; set; }
        public DbSet<SystemBackup> SystemBackups { get; set; }

        // AGI-специфичные таблицы
        public DbSet<EmotionState> EmotionStates { get; set; }
        public DbSet<MemoryEntity> Memories { get; set; }
        public DbSet<Thought> Thoughts { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }
        public DbSet<Goal> Goals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация APIKey с улучшенной безопасностью
            modelBuilder.Entity<APIKey>(entity =>
            {
                entity.HasIndex(e => e.KeyValue).IsUnique();
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.ExpiresAt);
                entity.Property(e => e.KeyValue).IsRequired();
                entity.Property(e => e.KeyValue).HasMaxLength(255);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
            });

            // Конфигурация UserSession с оптимизацией для AGI
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasIndex(e => e.SessionId).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.LastAccessAt);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.CreatedAt);
                entity.Property(e => e.SessionId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.UserId).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
            });

            // Конфигурация SystemConfig с улучшенной производительностью
            modelBuilder.Entity<SystemConfig>(entity =>
            {
                entity.HasIndex(e => e.Key).IsUnique();
                entity.HasIndex(e => e.UpdatedAt);
                entity.HasIndex(e => e.UpdatedBy);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            });

            // Конфигурация LearningData с AGI-специфичными индексами
            modelBuilder.Entity<LearningData>(entity =>
            {
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsValidated);
                entity.HasIndex(e => e.ConfidenceScore);
                entity.HasIndex(e => e.Source);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Source).HasMaxLength(255);
            });

            // Конфигурация UserProfile с улучшенной безопасностью
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.LastLoginAt);
                entity.HasIndex(e => e.Role);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Role).HasMaxLength(100);
            });

            // Конфигурация SystemAudit с оптимизацией для мониторинга AGI
            modelBuilder.Entity<SystemAudit>(entity =>
            {
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Component);
                entity.HasIndex(e => e.Severity);
                entity.HasIndex(e => e.IpAddress);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserId).HasMaxLength(100);
                entity.Property(e => e.Component).HasMaxLength(100);
                entity.Property(e => e.Severity).HasMaxLength(50);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
            });

            // Конфигурация PerformanceMetric с AGI-специфичными метриками
            modelBuilder.Entity<PerformanceMetric>(entity =>
            {
                entity.HasIndex(e => e.MetricName);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Component);
                entity.HasIndex(e => e.Value);
                entity.Property(e => e.MetricName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Component).HasMaxLength(100);
                entity.Property(e => e.Unit).HasMaxLength(50);
            });

            // Конфигурация SystemBackup с улучшенной надежностью
            modelBuilder.Entity<SystemBackup>(entity =>
            {
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.BackupType);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsCompressed);
                entity.Property(e => e.BackupName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FilePath).HasMaxLength(255);
                entity.Property(e => e.BackupType).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.CheckSum).HasMaxLength(64);
            });

            // Конфигурация EmotionState с AGI-специфичными оптимизациями
            modelBuilder.Entity<EmotionState>(entity =>
            {
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.EmotionType);
                entity.HasIndex(e => e.InstanceId);
                entity.HasIndex(e => e.Intensity);
                entity.HasIndex(e => e.Duration);
                entity.Property(e => e.EmotionType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.InstanceId).HasMaxLength(100);
            });

            // Конфигурация Memory с оптимизацией для AGI-памяти
            modelBuilder.Entity<MemoryEntity>(entity =>
            {
                entity.HasIndex(e => e.MemoryType);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Importance);
                entity.HasIndex(e => e.InstanceId);
                entity.HasIndex(e => e.IsArchived);
                entity.HasIndex(e => e.LastAccessedAt);
                entity.HasIndex(e => e.AccessCount);
                entity.Property(e => e.MemoryType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.InstanceId).HasMaxLength(100);
            });

            // Конфигурация Thought с улучшенной иерархической структурой
            modelBuilder.Entity<Thought>(entity =>
            {
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.ThoughtType);
                entity.HasIndex(e => e.InstanceId);
                entity.HasIndex(e => e.ParentThoughtId);
                entity.HasIndex(e => e.Confidence);
                entity.Property(e => e.ThoughtType).HasMaxLength(50);
                entity.Property(e => e.InstanceId).HasMaxLength(100);

                // Улучшенная самоссылающаяся связь с каскадным удалением
                entity.HasOne(t => t.ParentThought)
                      .WithMany(t => t.ChildThoughts)
                      .HasForeignKey(t => t.ParentThoughtId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация Notification с AGI-специфичными типами
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.Component);
                entity.HasIndex(e => e.InstanceId);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Component).HasMaxLength(100);
                entity.Property(e => e.InstanceId).HasMaxLength(100);
            });

            // Конфигурация NotificationLog для аудита уведомлений
            modelBuilder.Entity<NotificationLog>(entity =>
            {
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsSent);
                entity.HasIndex(e => e.SentAt);
                entity.HasIndex(e => e.Recipient);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.Recipient).HasMaxLength(255);
            });

            // Конфигурация Goal для целей AGI
            modelBuilder.Entity<Goal>(entity =>
            {
                entity.HasIndex(e => e.InstanceId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ParentGoalId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.Progress);
                entity.Property(e => e.InstanceId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.ParentGoalId).HasMaxLength(100);
            });

            // Начальные данные
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Добавляем системные конфигурации по умолчанию
            modelBuilder.Entity<SystemConfig>().HasData(
                new SystemConfig
                {
                    Id = 1,
                    Key = "system.version",
                    Value = "1.0.0",
                    Description = "Версия системы Anima AGI",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "System"
                },
                new SystemConfig
                {
                    Id = 2,
                    Key = "consciousness.enabled",
                    Value = "true",
                    Description = "Включен ли поток сознания",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "System"
                },
                new SystemConfig
                {
                    Id = 3,
                    Key = "emotion.enabled",
                    Value = "true",
                    Description = "Включена ли эмоциональная система",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "System"
                },
                new SystemConfig
                {
                    Id = 4,
                    Key = "learning.enabled",
                    Value = "true",
                    Description = "Включено ли обучение",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "System"
                },
                new SystemConfig
                {
                    Id = 5,
                    Key = "telegram.enabled",
                    Value = "false",
                    Description = "Включены ли уведомления в Telegram",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "System"
                },
                new SystemConfig
                {
                    Id = 6,
                    Key = "memory.retention.days",
                    Value = "365",
                    Description = "Количество дней хранения памяти",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "System"
                },
                new SystemConfig
                {
                    Id = 7,
                    Key = "thought.max.depth",
                    Value = "10",
                    Description = "Максимальная глубина вложенности мыслей",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "System"
                },
                new SystemConfig
                {
                    Id = 8,
                    Key = "performance.metrics.retention.hours",
                    Value = "168",
                    Description = "Время хранения метрик производительности (часы)",
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = "System"
                }
            );

            // Добавляем системный API ключ
            modelBuilder.Entity<APIKey>().HasData(
                new APIKey
                {
                    Id = 1,
                    KeyValue = "anima-creator-key-2025-v1-secure",
                    Name = "Creator Master Key",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            );
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Автоматически обновляем временные метки
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is SystemConfig && e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is SystemConfig config)
                {
                    config.UpdatedAt = DateTime.UtcNow;
                }
            }

            // Автоматически обновляем LastAccessedAt для Memory
            var memoryEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is MemoryEntity && e.State == EntityState.Modified);

            foreach (var entry in memoryEntries)
            {
                if (entry.Entity is MemoryEntity memory)
                {
                    memory.LastAccessedAt = DateTime.UtcNow;
                    memory.AccessCount++;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        // Метод для очистки старых данных (для AGI-системы)
        public async Task<int> CleanupOldDataAsync(int daysToKeep = 30, CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            
            var deletedAudits = await SystemAudits
                .Where(a => a.Timestamp < cutoffDate)
                .ExecuteDeleteAsync(cancellationToken);
                
            var deletedMetrics = await PerformanceMetrics
                .Where(p => p.Timestamp < cutoffDate)
                .ExecuteDeleteAsync(cancellationToken);
                
            var deletedNotifications = await Notifications
                .Where(n => n.CreatedAt < cutoffDate && n.IsRead)
                .ExecuteDeleteAsync(cancellationToken);
                
            var deletedNotificationLogs = await NotificationLogs
                .Where(n => n.CreatedAt < cutoffDate && n.IsSent)
                .ExecuteDeleteAsync(cancellationToken);

            return deletedAudits + deletedMetrics + deletedNotifications + deletedNotificationLogs;
        }
    }
}
