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
        public DbSet<Memory> Memories { get; set; }
        public DbSet<Thought> Thoughts { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация APIKey
            modelBuilder.Entity<APIKey>(entity =>
            {
                entity.HasIndex(e => e.KeyValue).IsUnique();
                entity.HasIndex(e => e.CreatedAt);
                entity.Property(e => e.KeyValue).IsRequired();
            });

            // Конфигурация UserSession
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasIndex(e => e.SessionId).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.LastAccessAt);
            });

            // Конфигурация SystemConfig
            modelBuilder.Entity<SystemConfig>(entity =>
            {
                entity.HasIndex(e => e.Key).IsUnique();
                entity.Property(e => e.Key).IsRequired();
            });

            // Конфигурация LearningData
            modelBuilder.Entity<LearningData>(entity =>
            {
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsValidated);
            });

            // Конфигурация UserProfile
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.Email);
                entity.Property(e => e.UserId).IsRequired();
            });

            // Конфигурация SystemAudit
            modelBuilder.Entity<SystemAudit>(entity =>
            {
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Component);
            });

            // Конфигурация PerformanceMetric
            modelBuilder.Entity<PerformanceMetric>(entity =>
            {
                entity.HasIndex(e => e.MetricName);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Component);
            });

            // Конфигурация SystemBackup
            modelBuilder.Entity<SystemBackup>(entity =>
            {
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.BackupType);
                entity.HasIndex(e => e.Status);
            });

            // Конфигурация EmotionState
            modelBuilder.Entity<EmotionState>(entity =>
            {
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.EmotionType);
                entity.HasIndex(e => e.InstanceId);
            });

            // Конфигурация Memory
            modelBuilder.Entity<Memory>(entity =>
            {
                entity.HasIndex(e => e.MemoryType);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Importance);
                entity.HasIndex(e => e.InstanceId);
                entity.HasIndex(e => e.IsArchived);
            });

            // Конфигурация Thought с самоссылающейся связью
            modelBuilder.Entity<Thought>(entity =>
            {
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.ThoughtType);
                entity.HasIndex(e => e.InstanceId);
                entity.HasIndex(e => e.ParentThoughtId);

                entity.HasOne(t => t.ParentThought)
                      .WithMany(t => t.ChildThoughts)
                      .HasForeignKey(t => t.ParentThoughtId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Конфигурация Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.Component);
                entity.HasIndex(e => e.InstanceId);
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

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}