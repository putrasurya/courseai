using CourseAI.Data.Repositories;
using CourseAI.Models;
using Microsoft.Extensions.Logging;

namespace CourseAI.Tests.Data;

public class LearningProfileRepositoryTests : DatabaseTestBase
{
    private readonly ILearningProfileRepository _repository;
    private readonly ILogger<LearningProfileRepository> _logger;

    public LearningProfileRepositoryTests()
    {
        _logger = LoggerFactory.CreateLogger<LearningProfileRepository>();
        _repository = new LearningProfileRepository(Context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProfileExists_ShouldReturnProfile()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await Context.LearningProfiles.AddAsync(profile);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(profile.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(profile.Id, result.Id);
        Assert.Equal(profile.LearningGoal, result.LearningGoal);
        Assert.Equal(profile.ExperienceLevel, result.ExperienceLevel);
        Assert.Equal(profile.KnownSkills.Count, result.KnownSkills.Count);
        Assert.Equal(profile.PreferredLearningStyles.Count, result.PreferredLearningStyles.Count);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProfileDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLatestAsync_WhenProfilesExist_ShouldReturnMostRecent()
    {
        // Arrange
        var oldProfile = CreateTestLearningProfile();
        oldProfile.UpdatedAt = DateTime.UtcNow.AddDays(-1);
        
        var newProfile = CreateTestLearningProfile();
        newProfile.UpdatedAt = DateTime.UtcNow;

        await Context.LearningProfiles.AddRangeAsync(oldProfile, newProfile);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newProfile.Id, result.Id);
        Assert.Equal(newProfile.LearningGoal, result.LearningGoal);
    }

    [Fact]
    public async Task GetLatestAsync_WhenNoProfiles_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetLatestAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateProfileWithGeneratedId()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        profile.Id = Guid.Empty; // Test auto-generation

        // Act
        var result = await _repository.CreateAsync(profile);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(profile.LearningGoal, result.LearningGoal);
        Assert.Equal(profile.ExperienceLevel, result.ExperienceLevel);
        Assert.True(result.CreatedAt > DateTime.MinValue);
        Assert.True(result.UpdatedAt > DateTime.MinValue);

        // Verify it's in database
        var dbProfile = await Context.LearningProfiles.FindAsync(result.Id);
        Assert.NotNull(dbProfile);
        Assert.Equal(result.LearningGoal, dbProfile.LearningGoal);
    }

    [Fact]
    public async Task CreateAsync_ShouldPreserveExistingId()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        var originalId = profile.Id;

        // Act
        var result = await _repository.CreateAsync(profile);

        // Assert
        Assert.Equal(originalId, result.Id);
        Assert.Equal(profile.LearningGoal, result.LearningGoal);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingProfile()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await Context.LearningProfiles.AddAsync(profile);
        await Context.SaveChangesAsync();

        var originalUpdatedAt = profile.UpdatedAt;
        
        // Modify profile
        profile.LearningGoal = "Updated Learning Goal";
        profile.ExperienceLevel = "Intermediate";
        profile.KnownSkills.Add("TypeScript");

        // Wait to ensure UpdatedAt will be different
        await Task.Delay(10);

        // Act
        var result = await _repository.UpdateAsync(profile);

        // Assert
        Assert.Equal("Updated Learning Goal", result.LearningGoal);
        Assert.Equal("Intermediate", result.ExperienceLevel);
        Assert.Contains("TypeScript", result.KnownSkills);
        Assert.True(result.UpdatedAt > originalUpdatedAt);

        // Verify in database
        var dbProfile = await Context.LearningProfiles.FindAsync(profile.Id);
        Assert.NotNull(dbProfile);
        Assert.Equal("Updated Learning Goal", dbProfile.LearningGoal);
        Assert.Equal("Intermediate", dbProfile.ExperienceLevel);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProfile()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await Context.LearningProfiles.AddAsync(profile);
        await Context.SaveChangesAsync();

        // Verify profile exists
        var existingProfile = await Context.LearningProfiles.FindAsync(profile.Id);
        Assert.NotNull(existingProfile);

        // Act
        await _repository.DeleteAsync(profile.Id);

        // Assert
        var deletedProfile = await Context.LearningProfiles.FindAsync(profile.Id);
        Assert.Null(deletedProfile);
    }

    [Fact]
    public async Task DeleteAsync_WhenProfileDoesNotExist_ShouldNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => _repository.DeleteAsync(nonExistentId));
        Assert.Null(exception);
    }

    [Fact]
    public async Task ExistsAsync_WhenProfileExists_ShouldReturnTrue()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await Context.LearningProfiles.AddAsync(profile);
        await Context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(profile.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WhenProfileDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.ExistsAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldHandleComplexData()
    {
        // Arrange
        var profile = new LearningProfile
        {
            Id = Guid.NewGuid(),
            LearningGoal = "Master full-stack development with modern technologies",
            ExperienceLevel = "Intermediate",
            KnownSkills = new List<string?> 
            { 
                "HTML5", "CSS3", "JavaScript ES6+", "Node.js", "React", "MongoDB" 
            },
            PreferredLearningStyles = new List<string?> 
            { 
                "Interactive coding tutorials", 
                "Project-based learning", 
                "Video lectures with code examples",
                "Documentation reading"
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(profile);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, result.KnownSkills.Count);
        Assert.Equal(4, result.PreferredLearningStyles.Count);
        Assert.Contains("Node.js", result.KnownSkills);
        Assert.Contains("Project-based learning", result.PreferredLearningStyles);

        // Verify complex data is persisted correctly
        var dbProfile = await _repository.GetByIdAsync(result.Id);
        Assert.NotNull(dbProfile);
        Assert.Equal(6, dbProfile.KnownSkills.Count);
        Assert.Equal(4, dbProfile.PreferredLearningStyles.Count);
        Assert.Contains("MongoDB", dbProfile.KnownSkills);
        Assert.Contains("Interactive coding tutorials", dbProfile.PreferredLearningStyles);
    }

    [Fact]
    public async Task Repository_ShouldHandleConcurrentOperations()
    {
        // Arrange
        var profile1 = CreateTestLearningProfile();
        var profile2 = CreateTestLearningProfile();

        // Act - Concurrent operations
        var task1 = _repository.CreateAsync(profile1);
        var task2 = _repository.CreateAsync(profile2);

        var results = await Task.WhenAll(task1, task2);

        // Assert
        Assert.NotEqual(results[0].Id, results[1].Id);
        
        var dbProfiles = Context.LearningProfiles.ToList();
        Assert.Equal(2, dbProfiles.Count);
    }
}