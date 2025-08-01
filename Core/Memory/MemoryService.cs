using Anima.Data;
using Anima.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MemoryModel = Anima.Data.Models.MemoryEntity;
using Anima.Core.Intent;
// using DbContext = Anima.Data.Models.AnimaDbContext;
using DbContext = Anima.Data.AnimaDbContext;

namespace Anima.Core.Memory;

public class MemoryService
{
    private readonly DbContext _db;
    private readonly ILogger<MemoryService> _logger;

    public MemoryService(DbContext db, ILogger<MemoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        // Инициализация сервиса памяти
        await Task.CompletedTask;
    }

    public async Task<List<MemoryModel>> GetRecentMemoriesAsync(int count = 10)
    {
        try
        {
            return await _db.Memories
                .OrderByDescending(m => m.Timestamp)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при получении последних воспоминаний: {Message}", ex.Message);
            return new List<MemoryModel>();
        }
    }

    public async Task<MemoryModel?> GetMemoryByIdAsync(int id)
    {
        try
        {
            return await _db.Memories.FirstOrDefaultAsync(m => m.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при получении воспоминания по ID {Id}: {Message}", id, ex.Message);
            return null;
        }
    }

    public async Task<List<MemoryModel>> FindByKeywordAsync(string keyword)
    {
        try
        {
            return await _db.Memories
                .Where(m => m.Content.Contains(keyword))
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при поиске по ключевому слову '{Keyword}': {Message}", keyword, ex.Message);
            return new List<MemoryModel>();
        }
    }

    public async Task<int> DeleteByTagAsync(string tag)
    {
        try
        {
            var memories = await _db.Memories
                .Where(m => m.Tags != null && m.Tags.Contains(tag))
                .ToListAsync();

            _db.Memories.RemoveRange(memories);
            await _db.SaveChangesAsync();
            return memories.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при удалении по тегу '{Tag}': {Message}", tag, ex.Message);
            return 0;
        }
    }

    public async Task<int> DeleteBeforeDateAsync(DateTime before)
    {
        try
        {
            var memories = await _db.Memories
                .Where(m => m.Timestamp < before)
                .ToListAsync();

            _db.Memories.RemoveRange(memories);
            await _db.SaveChangesAsync();
            return memories.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при удалении до даты {Date}: {Message}", before, ex.Message);
            return 0;
        }
    }

    public async Task<int> PurgeAllAsync()
    {
        try
        {
            var all = await _db.Memories.ToListAsync();
            _db.Memories.RemoveRange(all);
            await _db.SaveChangesAsync();
            return all.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при очистке всех воспоминаний: {Message}", ex.Message);
            return 0;
        }
    }

    public async Task<string> GetMemoryStatsAsync()
    {
        try
        {
            var total = await _db.Memories.CountAsync();
            var byCategory = await _db.Memories
                .GroupBy(m => m.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync();

            var stats = $"📊 Total: {total} memories\n";
            foreach (var g in byCategory)
            {
                stats += $"• {g.Category}: {g.Count}\n";
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при получении статистики памяти: {Message}", ex.Message);
            return "📊 Error: Unable to retrieve memory statistics";
        }
    }

    public async Task SaveInteraction(string userId, string userInput, ParsedIntent parsedIntent)
    {
        try
        {
            var memory = new MemoryModel
            {
                InstanceId = userId ?? "anonymous",
                Content = $"User: {userInput} | Intent: {parsedIntent.Type} | Confidence: {parsedIntent.Confidence:F2}",
                Category = "interaction",
                Importance = 5,
                Tags = $"intent:{parsedIntent.Type},confidence:{parsedIntent.Confidence:F2}",
                Timestamp = DateTime.UtcNow
            };

            _db.Memories.Add(memory);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при сохранении взаимодействия: {Message}", ex.Message);
        }
    }
}