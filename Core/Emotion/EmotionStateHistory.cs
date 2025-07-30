using Anima.Data;
using Anima.Data.Models;
using Microsoft.EntityFrameworkCore;
using DbContext = Anima.Data.Models.AnimaDbContext;

namespace Anima.AGI.Core.Emotion;

public class EmotionStateHistory
{
    private readonly DbContext _db;

    public EmotionStateHistory(DbContext db)
    {
        _db = db;
    }

    public async Task<List<Anima.Data.Models.EmotionState>> GetRecentAsync(TimeSpan range)
    {
        var since = DateTime.UtcNow - range;
        return await _db.EmotionStates
            .Where(e => e.Timestamp >= since)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task<Anima.Data.Models.EmotionState?> GetLatestAsync()
    {
        return await _db.EmotionStates
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task RecordEmotionAsync(string emotion, double intensity)
    {
        var state = new Anima.Data.Models.EmotionState
        {
            Emotion = emotion,
            Intensity = intensity,
            Timestamp = DateTime.UtcNow
        };
        _db.EmotionStates.Add(state);
        await _db.SaveChangesAsync();
    }

    public async Task<int> ClearAllAsync()
    {
        var all = await _db.EmotionStates.ToListAsync();
        _db.EmotionStates.RemoveRange(all);
        await _db.SaveChangesAsync();
        return all.Count;
    }

    public async Task<string> GetStatsAsync()
    {
        var total = await _db.EmotionStates.CountAsync();
        var dominant = await _db.EmotionStates
            .GroupBy(e => e.Emotion)
            .OrderByDescending(g => g.Count())
            .Select(g => new { g.Key, Count = g.Count() })
            .FirstOrDefaultAsync();

        return $"🎭 Total: {total} entries\nDominant: {dominant?.Key ?? "none"} ({dominant?.Count ?? 0})";
    }
}