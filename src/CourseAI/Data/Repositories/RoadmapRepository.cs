using CourseAI.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseAI.Data.Repositories;

public class RoadmapRepository : IRoadmapRepository
{
    private readonly CourseAIDbContext _context;

    public RoadmapRepository(CourseAIDbContext context)
    {
        _context = context;
    }

    public async Task<Roadmap?> GetByIdAsync(Guid id)
    {
        return await _context.Roadmaps
            .Include(r => r.LearningProfile)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Roadmap?> GetByProfileIdAsync(Guid profileId)
    {
        return await _context.Roadmaps
            .Include(r => r.LearningProfile)
            .Where(r => r.LearningProfileId == profileId)
            .OrderByDescending(r => r.LastModifiedDate)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Roadmap>> GetByProfileIdAllAsync(Guid profileId)
    {
        return await _context.Roadmaps
            .Include(r => r.LearningProfile)
            .Where(r => r.LearningProfileId == profileId)
            .OrderByDescending(r => r.LastModifiedDate)
            .ToListAsync();
    }

    public async Task<Roadmap> CreateAsync(Roadmap roadmap)
    {
        if (roadmap.Id == Guid.Empty)
        {
            roadmap.Id = Guid.NewGuid();
        }
        
        roadmap.CreatedDate = DateTime.UtcNow;
        roadmap.LastModifiedDate = DateTime.UtcNow;

        _context.Roadmaps.Add(roadmap);
        await _context.SaveChangesAsync();
        return roadmap;
    }

    public async Task<Roadmap> UpdateAsync(Roadmap roadmap)
    {
        roadmap.LastModifiedDate = DateTime.UtcNow;
        _context.Roadmaps.Update(roadmap);
        await _context.SaveChangesAsync();
        return roadmap;
    }

    public async Task DeleteAsync(Guid id)
    {
        var roadmap = await _context.Roadmaps.FindAsync(id);
        if (roadmap != null)
        {
            _context.Roadmaps.Remove(roadmap);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Roadmaps.AnyAsync(r => r.Id == id);
    }
}