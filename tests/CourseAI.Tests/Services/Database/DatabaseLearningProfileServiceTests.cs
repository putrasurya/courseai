using CourseAI.Data.Repositories;
using CourseAI.Models;
using CourseAI.Services;
using Microsoft.Extensions.Logging;
using CourseAI.Tests.Data;

namespace CourseAI.Tests.Services.Database;

public class DatabaseLearningProfileServiceTests : DatabaseTestBase
{
    private readonly DatabaseLearningProfileService _service;
    private readonly ILearningProfileRepository _repository;
    private readonly ILogger<DatabaseLearningProfileService> _logger;

    public DatabaseLearningProfileServiceTests()
    {
        _logger = LoggerFactory.CreateLogger<DatabaseLearningProfileService>();
        _repository = new LearningProfileRepository(Context);
        _service = new DatabaseLearningProfileService(_repository, _logger);
    }

    [Fact]
    public async Task GetCurrentProfileAsync_WhenProfileExists_ShouldReturnLatestProfile()
    {
        // Arrange
        var oldProfile = CreateTestLearningProfile();
        oldProfile.UpdatedAt = DateTime.UtcNow.AddDays(-1);
        
        var newProfile = CreateTestLearningProfile();
        newProfile.UpdatedAt = DateTime.UtcNow;
        newProfile.LearningGoal = "Latest Learning Goal";

        await _repository.CreateAsync(oldProfile);
        await _repository.CreateAsync(newProfile);

        // Act
        var result = await _service.GetCurrentProfileAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newProfile.Id, result.Id);
        Assert.Equal("Latest Learning Goal", result.LearningGoal);
    }

    [Fact]
    public async Task GetCurrentProfileAsync_WhenNoProfileExists_ShouldCreateNewProfile()
    {
        // Act
        var result = await _service.GetCurrentProfileAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(result.CreatedAt > DateTime.MinValue);
        Assert.True(result.UpdatedAt > DateTime.MinValue);

        // Verify it was saved to database
        var dbProfile = await _repository.GetByIdAsync(result.Id);
        Assert.NotNull(dbProfile);
        Assert.Equal(result.Id, dbProfile.Id);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldUpdateFieldsCorrectly()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _repository.CreateAsync(profile);

        // Act - Update different fields
        await _service.UpdateProfileAsync("learninggoal", "Updated Learning Goal");
        await _service.UpdateProfileAsync("experiencelevel", "Advanced");
        await _service.UpdateProfileAsync("knownskills", "Python");
        await _service.UpdateProfileAsync("preferredlearningstyles", "Live coding sessions");

        // Assert
        var updatedProfile = await _service.GetCurrentProfileAsync();
        Assert.Equal("Updated Learning Goal", updatedProfile.LearningGoal);
        Assert.Equal("Advanced", updatedProfile.ExperienceLevel);
        Assert.Contains("Python", updatedProfile.KnownSkills);
        Assert.Contains("Live coding sessions", updatedProfile.PreferredLearningStyles);

        // Verify in database
        var dbProfile = await _repository.GetByIdAsync(updatedProfile.Id);
        Assert.NotNull(dbProfile);
        Assert.Equal("Updated Learning Goal", dbProfile.LearningGoal);
        Assert.Contains("Python", dbProfile.KnownSkills);
    }

    [Fact]
    public async Task UpdateProfileAsync_ShouldNotAddDuplicateSkillsOrStyles()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        profile.KnownSkills = new List<string?> { "JavaScript" };
        profile.PreferredLearningStyles = new List<string?> { "Videos" };
        await _repository.CreateAsync(profile);

        // Act - Try to add existing items
        await _service.UpdateProfileAsync("knownskills", "JavaScript"); // Duplicate
        await _service.UpdateProfileAsync("preferredlearningstyles", "Videos"); // Duplicate
        await _service.UpdateProfileAsync("knownskills", "TypeScript"); // New
        await _service.UpdateProfileAsync("preferredlearningstyles", "Tutorials"); // New

        // Assert
        var updatedProfile = await _service.GetCurrentProfileAsync();
        Assert.Single(updatedProfile.KnownSkills.Where(s => s == "JavaScript"));
        Assert.Single(updatedProfile.PreferredLearningStyles.Where(s => s == "Videos"));
        Assert.Contains("TypeScript", updatedProfile.KnownSkills);
        Assert.Contains("Tutorials", updatedProfile.PreferredLearningStyles);
    }

    [Fact]
    public async Task RemoveFromProfileAsync_ShouldRemoveItemsCorrectly()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        profile.KnownSkills = new List<string?> { "HTML", "CSS", "JavaScript" };
        profile.PreferredLearningStyles = new List<string?> { "Videos", "Books", "Practice" };
        await _repository.CreateAsync(profile);

        // Act
        await _service.RemoveFromProfileAsync("knownskills", "CSS");
        await _service.RemoveFromProfileAsync("preferredlearningstyles", "Books");

        // Assert
        var updatedProfile = await _service.GetCurrentProfileAsync();
        Assert.DoesNotContain("CSS", updatedProfile.KnownSkills);
        Assert.DoesNotContain("Books", updatedProfile.PreferredLearningStyles);
        Assert.Contains("HTML", updatedProfile.KnownSkills);
        Assert.Contains("JavaScript", updatedProfile.KnownSkills);
        Assert.Contains("Videos", updatedProfile.PreferredLearningStyles);
        Assert.Contains("Practice", updatedProfile.PreferredLearningStyles);
    }

    [Fact]
    public async Task SetProfileAsync_WithNewProfile_ShouldCreateProfile()
    {
        // Arrange
        var newProfile = CreateTestLearningProfile();
        newProfile.Id = Guid.Empty; // New profile

        // Act
        await _service.SetProfileAsync(newProfile);

        // Assert
        var currentProfile = await _service.GetCurrentProfileAsync();
        Assert.NotEqual(Guid.Empty, currentProfile.Id);
        Assert.Equal(newProfile.LearningGoal, currentProfile.LearningGoal);
        Assert.Equal(newProfile.ExperienceLevel, currentProfile.ExperienceLevel);

        // Verify in database
        var dbProfile = await _repository.GetByIdAsync(currentProfile.Id);
        Assert.NotNull(dbProfile);
        Assert.Equal(newProfile.LearningGoal, dbProfile.LearningGoal);
    }

    [Fact]
    public async Task SetProfileAsync_WithExistingProfile_ShouldUpdateProfile()
    {
        // Arrange
        var existingProfile = CreateTestLearningProfile();
        await _repository.CreateAsync(existingProfile);

        // Modify the profile
        existingProfile.LearningGoal = "Modified Learning Goal";
        existingProfile.ExperienceLevel = "Expert";

        // Act
        await _service.SetProfileAsync(existingProfile);

        // Assert
        var currentProfile = await _service.GetCurrentProfileAsync();
        Assert.Equal(existingProfile.Id, currentProfile.Id);
        Assert.Equal("Modified Learning Goal", currentProfile.LearningGoal);
        Assert.Equal("Expert", currentProfile.ExperienceLevel);
    }

    [Fact]
    public async Task ClearProfileAsync_ShouldCreateNewEmptyProfile()
    {
        // Arrange
        var existingProfile = CreateTestLearningProfile();
        await _repository.CreateAsync(existingProfile);
        var originalId = existingProfile.Id;

        // Act
        await _service.ClearProfileAsync();

        // Assert
        var currentProfile = await _service.GetCurrentProfileAsync();
        Assert.NotEqual(originalId, currentProfile.Id);
        Assert.Null(currentProfile.LearningGoal);
        Assert.Null(currentProfile.ExperienceLevel);
        Assert.Empty(currentProfile.KnownSkills);
        Assert.Empty(currentProfile.PreferredLearningStyles);

        // Verify old profile still exists in database
        var oldProfile = await _repository.GetByIdAsync(originalId);
        Assert.NotNull(oldProfile);
    }

    [Fact]
    public async Task GetProfileSummary_ShouldReturnFormattedSummary()
    {
        // Arrange
        var profile = new LearningProfile
        {
            Id = Guid.NewGuid(),
            LearningGoal = "Learn React Development",
            ExperienceLevel = "Intermediate",
            KnownSkills = new List<string?> { "HTML", "CSS", "JavaScript" },
            PreferredLearningStyles = new List<string?> { "Video tutorials", "Hands-on practice" }
        };
        await _repository.CreateAsync(profile);

        // Act
        var summary = _service.GetProfileSummary();

        // Assert
        Assert.Contains("Learn React Development", summary);
        Assert.Contains("Intermediate", summary);
        Assert.Contains("HTML, CSS, JavaScript", summary);
        Assert.Contains("Video tutorials, Hands-on practice", summary);
    }

    [Fact]
    public async Task IsProfileSufficient_ShouldReturnCorrectStatus()
    {
        // Arrange - Empty profile
        await _service.ClearProfileAsync();

        // Act & Assert - Insufficient profile
        var isEmpty = _service.IsProfileSufficient();
        Assert.False(isEmpty);

        // Arrange - Profile with learning goal only
        await _service.UpdateProfileAsync("learninggoal", "Learn React");
        var hasGoalOnly = _service.IsProfileSufficient();
        Assert.False(hasGoalOnly);

        // Arrange - Profile with goal and experience
        await _service.UpdateProfileAsync("experiencelevel", "Beginner");
        var hasGoalAndExperience = _service.IsProfileSufficient();
        Assert.False(hasGoalAndExperience);

        // Arrange - Complete profile
        await _service.UpdateProfileAsync("knownskills", "JavaScript");
        var isComplete = _service.IsProfileSufficient();
        Assert.True(isComplete);
    }

    [Fact]
    public async Task ProfileChanged_EventShouldFireWhenProfileUpdated()
    {
        // Arrange
        LearningProfile? changedProfile = null;
        _service.ProfileChanged += (profile) => changedProfile = profile;

        // Act
        await _service.UpdateProfileAsync("learninggoal", "Test Goal");

        // Assert
        Assert.NotNull(changedProfile);
        Assert.Equal("Test Goal", changedProfile.LearningGoal);
    }

    [Fact]
    public async Task BackwardCompatibility_SynchronousMethods_ShouldWork()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _repository.CreateAsync(profile);

        // Act - Use synchronous backward compatibility methods
        _service.UpdateProfile("learninggoal", "Sync Updated Goal");
        _service.UpdateProfile("knownskills", "React");

        // Wait a moment for async operations to complete
        await Task.Delay(100);

        // Assert
        var currentProfile = await _service.GetCurrentProfileAsync();
        Assert.Equal("Sync Updated Goal", currentProfile.LearningGoal);
        Assert.Contains("React", currentProfile.KnownSkills);
    }

    [Fact]
    public async Task GetProfileCopy_ShouldReturnDeepCopy()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _repository.CreateAsync(profile);

        // Act
        var copy = _service.GetProfileCopy();
        copy.LearningGoal = "Modified Copy";
        copy.KnownSkills.Add("New Skill");

        // Assert
        var original = await _service.GetCurrentProfileAsync();
        Assert.NotEqual("Modified Copy", original.LearningGoal);
        Assert.DoesNotContain("New Skill", original.KnownSkills);
    }

    [Fact]
    public async Task Service_ShouldHandleConcurrentUpdates()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _repository.CreateAsync(profile);

        // Act - Concurrent updates
        var task1 = _service.UpdateProfileAsync("knownskills", "Skill1");
        var task2 = _service.UpdateProfileAsync("knownskills", "Skill2");
        var task3 = _service.UpdateProfileAsync("preferredlearningstyles", "Style1");

        await Task.WhenAll(task1, task2, task3);

        // Assert
        var updatedProfile = await _service.GetCurrentProfileAsync();
        Assert.Contains("Skill1", updatedProfile.KnownSkills);
        Assert.Contains("Skill2", updatedProfile.KnownSkills);
        Assert.Contains("Style1", updatedProfile.PreferredLearningStyles);
    }
}