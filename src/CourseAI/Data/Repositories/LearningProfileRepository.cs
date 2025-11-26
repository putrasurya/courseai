using CourseAI.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseAI.Data.Repositories;

public class LearningProfileRepository : ILearningProfileRepository
{
    private readonly CourseAIDbContext _context;

    public LearningProfileRepository(CourseAIDbContext context)
    {
        _context = context;
    }

    public async Task<LearningProfile?> GetByIdAsync(Guid id)
    {
        return await _context.LearningProfiles
            .Include(p => p.Roadmaps)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<LearningProfile?> GetLatestAsync()
    {
        return await _context.LearningProfiles
            .Include(p => p.Roadmaps)
            .OrderByDescending(p => p.UpdatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<LearningProfile> CreateAsync(LearningProfile profile)
    {
        if (profile.Id == Guid.Empty)
        {
            profile.Id = Guid.NewGuid();
        }
        
        profile.CreatedAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;

        _context.LearningProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task<LearningProfile> UpdateAsync(LearningProfile profile)
    {
        profile.UpdatedAt = DateTime.UtcNow;
        _context.LearningProfiles.Update(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task DeleteAsync(Guid id)
    {
        var profile = await _context.LearningProfiles.FindAsync(id);
        if (profile != null)
        {
            _context.LearningProfiles.Remove(profile);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.LearningProfiles.AnyAsync(p => p.Id == id);
    }
}