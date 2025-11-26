using CourseAI.Models;

namespace CourseAI.Data.Repositories;

public interface ILearningProfileRepository
{
    Task<LearningProfile?> GetByIdAsync(Guid id);
    Task<LearningProfile?> GetLatestAsync();
    Task<LearningProfile> CreateAsync(LearningProfile profile);
    Task<LearningProfile> UpdateAsync(LearningProfile profile);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}

public interface IRoadmapRepository
{
    Task<Roadmap?> GetByIdAsync(Guid id);
    Task<Roadmap?> GetByProfileIdAsync(Guid profileId);
    Task<IEnumerable<Roadmap>> GetByProfileIdAllAsync(Guid profileId);
    Task<Roadmap> CreateAsync(Roadmap roadmap);
    Task<Roadmap> UpdateAsync(Roadmap roadmap);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}