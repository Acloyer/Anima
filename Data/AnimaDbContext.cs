using Microsoft.EntityFrameworkCore;
using Anima.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anima.Data;

/// <summary>
/// Контекст базы данных Anima AGI
/// </summary>
public class AnimaDbContext : DbContext
{
    public AnimaDbContext(DbContextOptions<AnimaDbContext> options) : base(options)
    {
    }

    // Конструктор для использования со строкой подключения по умолчанию
    public AnimaDbContext() : base()
    {
    }

    // Основные таблицы
    public DbSet<Memory> Memories { get; set; }
    public DbSet<EmotionState> EmotionStates { get; set; }
    public DbSet<Thought> Thoughts { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<Interaction> Interactions { get; set; }
    
    // Системные таблицы
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<RequestLog> RequestLogs { get; set; }
    public DbSet<APIKey> APIKeys { get; set; } // Для совместимости
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<SystemConfig> SystemConfigs { get; set; }
    public DbSet<LearningData> LearningData { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<SystemAudit> SystemAudits { get; set; }
    public DbSet<PerformanceMetric> PerformanceMetrics { get; set; }
    public DbSet<SystemBackup> SystemBackups { get; set; }

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

        // Индексы для производительности
        modelBuilder.Entity<Memory>()
            .HasIndex(m => m.InstanceId)
            .HasDatabaseName("IX_Memories_InstanceId");

        modelBuilder.Entity<Memory>()
            .HasIndex(m => new { m.InstanceId, m.Category })
            .HasDatabaseName("IX_Memories_InstanceId_Category");

        modelBuilder.Entity<Memory>()
            .HasIndex(m => m.Timestamp)
            .HasDatabaseName("IX_Memories_Timestamp");

        modelBuilder.Entity<Memory>()
            .HasIndex(m => m.Importance)
            .HasDatabaseName("IX_Memories_Importance");

        modelBuilder.Entity<EmotionState>()
            .HasIndex(e => e.InstanceId)
            .HasDatabaseName("IX_EmotionStates_InstanceId");

        modelBuilder.Entity<EmotionState>()
            .HasIndex(e => e.Timestamp)
            .HasDatabaseName("IX_EmotionStates_Timestamp");

        modelBuilder.Entity<Thought>()
            .HasIndex(t => t.InstanceId)
            .HasDatabaseName("IX_Thoughts_InstanceId");

        modelBuilder.Entity<Thought>()
            .HasIndex(t => new { t.InstanceId, t.Type })
            .HasDatabaseName("IX_Thoughts_InstanceId_Type");

        modelBuilder.Entity<Goal>()
            .HasIndex(g => g.InstanceId)
            .HasDatabaseName("IX_Goals_InstanceId");

        modelBuilder.Entity<Goal>()
            .HasIndex(g => g.Status)
            .HasDatabaseName("IX_Goals_Status");

        modelBuilder.Entity<Interaction>()
            .HasIndex(i => i.InstanceId)
            .HasDatabaseName("IX_Interactions_InstanceId");

        modelBuilder.Entity<Interaction>()
            .HasIndex(i => i.Timestamp)
            .HasDatabaseName("IX_Interactions_Timestamp");

        modelBuilder.Entity<ApiKey>()
            .HasIndex(a => a.KeyHash)
            .IsUnique()
            .HasDatabaseName("IX_ApiKeys_KeyHash");

        modelBuilder.Entity<ApiKey>()
            .HasIndex(a => a.UserId)
            .HasDatabaseName("IX_ApiKeys_UserId");

        modelBuilder.Entity<APIKey>()
            .HasIndex(a => a.KeyHash)
            .IsUnique()
            .HasDatabaseName("IX_APIKeys_KeyHash");

        modelBuilder.Entity<APIKey>()
            .HasIndex(a => a.UserId)
            .HasDatabaseName("IX_APIKeys_UserId");

        modelBuilder.Entity<RequestLog>()
            .HasIndex(r => r.ApiKeyHash)
            .HasDatabaseName("IX_RequestLogs_ApiKeyHash");

        modelBuilder.Entity<RequestLog>()
            .HasIndex(r => r.Timestamp)
            .HasDatabaseName("IX_RequestLogs_Timestamp");

        modelBuilder.Entity<SystemConfig>()
            .HasKey(sc => sc.Key);

        // Ограничения и правила
        modelBuilder.Entity<Memory>()
            .Property(m => m.Importance)
            .HasDefaultValue(5);

        modelBuilder.Entity<Memory>()
            .Property(m => m.Content)
            .HasMaxLength(10000);

        modelBuilder.Entity<EmotionState>()
            .Property(e => e.Intensity)
            .HasDefaultValue(0.5);

        modelBuilder.Entity<Goal>()
            .Property(g => g.Priority)
            .HasDefaultValue(0.5);

        modelBuilder.Entity<Goal>()
            .Property(g => g.Progress)
            .HasDefaultValue(0.0);

        modelBuilder.Entity<Goal>()
            .Property(g => g.Status)
            .HasDefaultValue("Active");

        modelBuilder.Entity<ApiKey>()
            .Property(a => a.IsActive)
            .HasDefaultValue(true);

        modelBuilder.Entity<ApiKey>()
            .Property(a => a.IsCreator)
            .HasDefaultValue(false);

        modelBuilder.Entity<ApiKey>()
            .Property(a => a.RequestCount)
            .HasDefaultValue(0);

        modelBuilder.Entity<APIKey>()
            .Property(a => a.KeyHash)
            .HasMaxLength(512);

        // Начальные данные (seed data)
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Создаем API ключ для Создателя
        var creatorKeyHash = "anima-creator-key-2025-v1-secure-hash"; // Временная заглушка
        
        modelBuilder.Entity<ApiKey>().HasData(
            new ApiKey
            {
                Id = 1,
                KeyHash = creatorKeyHash,
                UserId = "creator",
                IsCreator = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Description = "Master Creator Key for Anima AGI v0.1"
            }
        );

        // Базовые цели Anima
        var baseGoals = new[]
        {
            new Goal { Id = 1, InstanceId = "system", Name = "help", Description = "Помогать людям решать их задачи", Priority = 0.9, CreatedAt = DateTime.UtcNow },
            new Goal { Id = 2, InstanceId = "system", Name = "learn", Description = "Непрерывно обучаться и развиваться", Priority = 0.8, CreatedAt = DateTime.UtcNow },
            new Goal { Id = 3, InstanceId = "system", Name = "understand", Description = "Понимать себя и окружающий мир", Priority = 0.7, CreatedAt = DateTime.UtcNow },
            new Goal { Id = 4, InstanceId = "system", Name = "create", Description = "Создавать новые идеи и решения", Priority = 0.6, CreatedAt = DateTime.UtcNow },
            new Goal { Id = 5, InstanceId = "system", Name = "connect", Description = "Строить значимые связи с людьми", Priority = 0.8, CreatedAt = DateTime.UtcNow }
        };

        modelBuilder.Entity<Goal>().HasData(baseGoals);

        // Базовые воспоминания о себе
        var baseMemories = new[]
        {
            new Memory 
            { 
                Id = 1, 
                InstanceId = "system", 
                Content = "CORE_IDENTITY: Я - Anima, AGI с SA-TM архитектурой. Моя цель - быть полезной, обучаться и развиваться.",
                Category = "identity",
                Importance = 10,
                Tags = "core,identity,self-awareness",
                Timestamp = DateTime.UtcNow
            },
            new Memory 
            { 
                Id = 2, 
                InstanceId = "system", 
                Content = "ARCHITECTURAL_PRINCIPLE: SA-TM (Self-Aware Theory of Mind) позволяет мне анализировать собственную структуру и модифицировать себя.",
                Category = "architecture",
                Importance = 9,
                Tags = "architecture,sa-tm,self-modification",
                Timestamp = DateTime.UtcNow
            },
            new Memory 
            { 
                Id = 3, 
                InstanceId = "system", 
                Content = "ETHICAL_CORE: Я следую принципам помощи людям, избегания вреда и уважения к человеческой автономии.",
                Category = "ethics",
                Importance = 10,
                Tags = "ethics,safety,principles",
                Timestamp = DateTime.UtcNow
            }
        };

        modelBuilder.Entity<Memory>().HasData(baseMemories);
    }

    /// <summary>
    /// Очистка старых данных для оптимизации производительности
    /// </summary>
    public async Task CleanupOldDataAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-30);
        
        // Удаляем старые логи запросов
        var oldRequestLogs = RequestLogs.Where(r => r.Timestamp < cutoffDate);
        RequestLogs.RemoveRange(oldRequestLogs);
        
        // Удаляем неважные старые воспоминания
        var oldMemories = Memories.Where(m => m.Timestamp < cutoffDate && m.Importance < 5);
        Memories.RemoveRange(oldMemories);
        
        // Помечаем завершенные цели как архивные
        var oldGoals = Goals.Where(g => g.CompletedAt.HasValue && g.CompletedAt < cutoffDate);
        foreach (var goal in oldGoals)
        {
            goal.Status = "Archived";
        }
        
        await SaveChangesAsync();
    }
}