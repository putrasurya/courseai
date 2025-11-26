using CourseAI.Data.Repositories;
using CourseAI.Models;
using Microsoft.Extensions.Logging;

namespace CourseAI.Tests.Data;

public class SimpleLearningProfileRepositoryTests : DatabaseTestBase
{
    private readonly ILearningProfileRepository _repository;

    public SimpleLearningProfileRepositoryTests()
    {
        _repository = new LearningProfileRepository(Context);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateProfile()
    {
        // Arrange
        var profile = new LearningProfile
        {
            Id = Guid.NewGuid(),
            LearningGoal = "Learn React",
            ExperienceLevel = "Beginner",
            KnownSkills = new List<string?> { "HTML", "CSS" },
            PreferredLearningStyles = new List<string?> { "Videos" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(profile);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(profile.LearningGoal, result.LearningGoal);
        Assert.Equal(profile.ExperienceLevel, result.ExperienceLevel);
        Assert.Equal(2, result.KnownSkills.Count);
        Assert.Contains("HTML", result.KnownSkills);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProfile()
    {
        // Arrange
        var profile = new LearningProfile
        {
            Id = Guid.NewGuid(),
            LearningGoal = "Learn TypeScript",
            ExperienceLevel = "Intermediate",
            KnownSkills = new List<string?> { "JavaScript" },
            PreferredLearningStyles = new List<string?> { "Documentation" }
        };

        await _repository.CreateAsync(profile);

        // Act
        var result = await _repository.GetByIdAsync(profile.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(profile.Id, result.Id);
        Assert.Equal("Learn TypeScript", result.LearningGoal);
        Assert.Equal("Intermediate", result.ExperienceLevel);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProfile()
    {
        // Arrange
        var profile = new LearningProfile
        {
            Id = Guid.NewGuid(),
            LearningGoal = "Learn Python",
            ExperienceLevel = "Beginner"
        };

        await _repository.CreateAsync(profile);

        // Act
        profile.ExperienceLevel = "Intermediate";
        profile.LearningGoal = "Master Python";
        var result = await _repository.UpdateAsync(profile);

        // Assert
        Assert.Equal("Master Python", result.LearningGoal);
        Assert.Equal("Intermediate", result.ExperienceLevel);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProfile()
    {
        // Arrange
        var profile = new LearningProfile
        {
            Id = Guid.NewGuid(),
            LearningGoal = "Learn Go"
        };

        await _repository.CreateAsync(profile);

        // Act
        await _repository.DeleteAsync(profile.Id);

        // Assert
        var result = await _repository.GetByIdAsync(profile.Id);
        Assert.Null(result);
    }
}