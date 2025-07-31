using Anima.Data;
using Anima.Data.Models;
using Microsoft.EntityFrameworkCore;
using MemoryModel = Anima.Data.Models.MemoryEntity;
using Anima.Core.Intent;
// using DbContext = Anima.Data.Models.AnimaDbContext;
using DbContext = Anima.Data.AnimaDbContext;

namespace Anima.Core.Memory;

public class MemoryService
{
    private readonly DbContext _db;

    public MemoryService(DbContext db)
    {
        _db = db;
    }

    public async Task InitializeAsync()
    {
        // Инициализация сервиса памяти
        await Task.CompletedTask;
    }

    public async Task<List<MemoryModel>> GetRecentMemoriesAsync(int count = 10)
    {
        return await _db.Memories
            .OrderByDescending(m => m.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<MemoryModel?> GetMemoryByIdAsync(int id)
    {
        return await _db.Memories.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<MemoryModel>> FindByKeywordAsync(string keyword)
    {
        return await _db.Memories
            .Where(m => m.Content.Contains(keyword))
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<int> DeleteByTagAsync(string tag)
    {
        var memories = await _db.Memories
            .Where(m => m.Tags != null && m.Tags.Contains(tag))
            .ToListAsync();

        _db.Memories.RemoveRange(memories);
        await _db.SaveChangesAsync();
        return memories.Count;
    }

    public async Task<int> DeleteBeforeDateAsync(DateTime before)
    {
        var memories = await _db.Memories
            .Where(m => m.Timestamp < before)
            .ToListAsync();

        _db.Memories.RemoveRange(memories);
        await _db.SaveChangesAsync();
        return memories.Count;
    }

    public async Task<int> PurgeAllAsync()
    {
        var all = await _db.Memories.ToListAsync();
        _db.Memories.RemoveRange(all);
        await _db.SaveChangesAsync();
        return all.Count;
    }

    public async Task<string> GetMemoryStatsAsync()
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

    public async Task SaveInteraction(string userId, string userInput, ParsedIntent parsedIntent)
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
}