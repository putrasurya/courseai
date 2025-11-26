using CourseAI.Data.Repositories;
using CourseAI.Models;
using CourseAI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CourseAI.Tests.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseAI.Tests.Services.Hybrid;

public class HybridLearningProfileServiceTests : DatabaseTestBase
{
    private readonly HybridLearningProfileService _hybridService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HybridLearningProfileService> _logger;

    public HybridLearningProfileServiceTests()
    {
        // Create a service collection and register dependencies
        var services = new ServiceCollection();
        services.AddSingleton(Context);
        services.AddScoped<ILearningProfileRepository, LearningProfileRepository>();
        services.AddSingleton(LoggerFactory);

        _serviceProvider = services.BuildServiceProvider();
        _logger = LoggerFactory.CreateLogger<HybridLearningProfileService>();
        _hybridService = new HybridLearningProfileService(_serviceProvider, _logger);
    }

    [Fact]
    public async Task CurrentProfile_AfterInitialization_ShouldHaveValidProfile()
    {
        // Wait for initialization to complete
        await Task.Delay(200);

        // Act
        var currentProfile = _hybridService.CurrentProfile;

        // Assert
        Assert.NotNull(currentProfile);
        Assert.NotEqual(Guid.Empty, currentProfile.Id);
    }

    [Fact]
    public async Task UpdateProfile_ShouldUpdateBothMemoryAndDatabase()
    {
        // Arrange
        await Task.Delay(200); // Wait for initialization

        // Act
        _hybridService.UpdateProfile("learninggoal", "Test Learning Goal");
        _hybridService.UpdateProfile("experiencelevel", "Beginner");
        _hybridService.UpdateProfile("knownskills", "JavaScript");

        // Wait for async database operations
        await Task.Delay(200);

        // Assert - Check memory
        var memoryProfile = _hybridService.CurrentProfile;
        Assert.Equal("Test Learning Goal", memoryProfile.LearningGoal);
        Assert.Equal("Beginner", memoryProfile.ExperienceLevel);
        Assert.Contains("JavaScript", memoryProfile.KnownSkills);

        // Assert - Check database
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILearningProfileRepository>();
        var dbProfile = await repository.GetLatestAsync();
        
        Assert.NotNull(dbProfile);
        Assert.Equal("Test Learning Goal", dbProfile.LearningGoal);
        Assert.Equal("Beginner", dbProfile.ExperienceLevel);
        Assert.Contains("JavaScript", dbProfile.KnownSkills);
    }

    [Fact]
    public async Task RemoveFromProfile_ShouldRemoveFromBothMemoryAndDatabase()
    {
        // Arrange
        await Task.Delay(200); // Wait for initialization
        
        _hybridService.UpdateProfile("knownskills", "HTML");
        _hybridService.UpdateProfile("knownskills", "CSS");
        _hybridService.UpdateProfile("knownskills", "JavaScript");
        
        await Task.Delay(200); // Wait for database updates

        // Act
        _hybridService.RemoveFromProfile("knownskills", "CSS");
        await Task.Delay(200); // Wait for database update

        // Assert - Check memory
        var memoryProfile = _hybridService.CurrentProfile;
        Assert.DoesNotContain("CSS", memoryProfile.KnownSkills);
        Assert.Contains("HTML", memoryProfile.KnownSkills);
        Assert.Contains("JavaScript", memoryProfile.KnownSkills);

        // Assert - Check database
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILearningProfileRepository>();
        var dbProfile = await repository.GetLatestAsync();
        
        Assert.NotNull(dbProfile);
        Assert.DoesNotContain("CSS", dbProfile.KnownSkills);
        Assert.Contains("HTML", dbProfile.KnownSkills);
        Assert.Contains("JavaScript", dbProfile.KnownSkills);
    }

    [Fact]
    public async Task SetProfile_ShouldReplaceProfileInBothMemoryAndDatabase()
    {
        // Arrange
        await Task.Delay(200); // Wait for initialization
        
        var newProfile = new LearningProfile
        {
            Id = Guid.NewGuid(),
            LearningGoal = "New Learning Goal",
            ExperienceLevel = "Advanced",
            KnownSkills = new List<string?> { "React", "Node.js" },
            PreferredLearningStyles = new List<string?> { "Project-based" }
        };

        // Act
        _hybridService.SetProfile(newProfile);
        await Task.Delay(200); // Wait for database update

        // Assert - Check memory
        var memoryProfile = _hybridService.CurrentProfile;
        Assert.Equal("New Learning Goal", memoryProfile.LearningGoal);
        Assert.Equal("Advanced", memoryProfile.ExperienceLevel);
        Assert.Contains("React", memoryProfile.KnownSkills);
        Assert.Contains("Node.js", memoryProfile.KnownSkills);

        // Assert - Check database
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILearningProfileRepository>();
        var dbProfile = await repository.GetLatestAsync();
        
        Assert.NotNull(dbProfile);
        Assert.Equal("New Learning Goal", dbProfile.LearningGoal);
        Assert.Equal("Advanced", dbProfile.ExperienceLevel);
        Assert.Contains("React", dbProfile.KnownSkills);
        Assert.Contains("Node.js", dbProfile.KnownSkills);
    }

    [Fact]
    public async Task ClearProfile_ShouldCreateNewEmptyProfileInBothMemoryAndDatabase()
    {
        // Arrange
        await Task.Delay(200); // Wait for initialization
        
        _hybridService.UpdateProfile("learninggoal", "Goal to be cleared");
        await Task.Delay(200);

        var originalProfile = _hybridService.CurrentProfile;
        var originalId = originalProfile.Id;

        // Act
        _hybridService.ClearProfile();
        await Task.Delay(200); // Wait for database update

        // Assert - Check memory
        var memoryProfile = _hybridService.CurrentProfile;
        Assert.NotEqual(originalId, memoryProfile.Id);
        Assert.Null(memoryProfile.LearningGoal);
        Assert.Empty(memoryProfile.KnownSkills);

        // Assert - Check database
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILearningProfileRepository>();
        var dbProfile = await repository.GetLatestAsync();
        
        Assert.NotNull(dbProfile);
        Assert.NotEqual(originalId, dbProfile.Id);
        Assert.True(string.IsNullOrEmpty(dbProfile.LearningGoal));
        Assert.Empty(dbProfile.KnownSkills);
    }

    [Fact]
    public async Task GetProfileSummary_ShouldReturnFormattedSummary()
    {
        // Arrange
        await Task.Delay(200); // Wait for initialization
        
        _hybridService.UpdateProfile("learninggoal", "Learn Full Stack Development");
        _hybridService.UpdateProfile("experiencelevel", "Intermediate");
        _hybridService.UpdateProfile("knownskills", "JavaScript");
        _hybridService.UpdateProfile("knownskills", "React");
        _hybridService.UpdateProfile("preferredlearningstyles", "Hands-on");
        _hybridService.UpdateProfile("preferredlearningstyles", "Videos");
        
        await Task.Delay(200); // Wait for updates

        // Act
        var summary = _hybridService.GetProfileSummary();

        // Assert
        Assert.Contains("Learn Full Stack Development", summary);
        Assert.Contains("Intermediate", summary);
        Assert.Contains("JavaScript", summary);
        Assert.Contains("React", summary);
        Assert.Contains("Hands-on", summary);
        Assert.Contains("Videos", summary);
    }

    [Fact]
    public async Task IsProfileSufficient_ShouldReturnCorrectStatus()
    {
        // Arrange
        await Task.Delay(200); // Wait for initialization
        
        _hybridService.ClearProfile();
        await Task.Delay(200);

        // Act & Assert - Empty profile
        Assert.False(_hybridService.IsProfileSufficient());

        // Add learning goal
        _hybridService.UpdateProfile("learninggoal", "Test Goal");
        await Task.Delay(100);
        Assert.False(_hybridService.IsProfileSufficient());

        // Add experience level
        _hybridService.UpdateProfile("experiencelevel", "Beginner");
        await Task.Delay(100);
        Assert.False(_hybridService.IsProfileSufficient());

        // Add skill (should make it sufficient)
        _hybridService.UpdateProfile("knownskills", "HTML");
        await Task.Delay(100);
        Assert.True(_hybridService.IsProfileSufficient());
    }

    [Fact]
    public async Task GetProfileCopy_ShouldReturnDeepCopy()
    {
        // Arrange
        await Task.Delay(200); // Wait for initialization
        
        _hybridService.UpdateProfile("learninggoal", "Original Goal");
        _hybridService.UpdateProfile("knownskills", "Original Skill");
        await Task.Delay(200);

        // Act
        var copy = _hybridService.GetProfileCopy();
        copy.LearningGoal = "Modified Goal";
        copy.KnownSkills.Add("Modified Skill");

        // Assert
        var original = _hybridService.CurrentProfile;
        Assert.Equal("Original Goal", original.LearningGoal);
        Assert.DoesNotContain("Modified Skill", original.KnownSkills);
        Assert.NotEqual(copy.LearningGoal, original.LearningGoal);
    }

    [Fact]
    public async Task Service_ShouldInitializeFromExistingDatabaseData()
    {
        // Arrange - Create profile directly in database
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILearningProfileRepository>();
        
        var dbProfile = new LearningProfile
        {
            Id = Guid.NewGuid(),
            LearningGoal = "Database Goal",
            ExperienceLevel = "Expert",
            KnownSkills = new List<string?> { "Database Skill" },
            PreferredLearningStyles = new List<string?> { "Database Style" }
        };
        await repository.CreateAsync(dbProfile);

        // Act - Create new hybrid service (simulates app restart)
        var newHybridService = new HybridLearningProfileService(_serviceProvider, _logger);
        await Task.Delay(300); // Wait for initialization

        // Assert
        var profile = newHybridService.CurrentProfile;
        Assert.Equal("Database Goal", profile.LearningGoal);
        Assert.Equal("Expert", profile.ExperienceLevel);
        Assert.Contains("Database Skill", profile.KnownSkills);
        Assert.Contains("Database Style", profile.PreferredLearningStyles);
    }

    [Fact]
    public async Task Service_ShouldHandleConcurrentOperations()
    {
        // Arrange
        await Task.Delay(200); // Wait for initialization

        // Act - Concurrent operations
        var tasks = new List<Task>
        {
            Task.Run(() => _hybridService.UpdateProfile("knownskills", "Skill1")),
            Task.Run(() => _hybridService.UpdateProfile("knownskills", "Skill2")),
            Task.Run(() => _hybridService.UpdateProfile("knownskills", "Skill3")),
            Task.Run(() => _hybridService.UpdateProfile("preferredlearningstyles", "Style1")),
            Task.Run(() => _hybridService.UpdateProfile("preferredlearningstyles", "Style2"))
        };

        await Task.WhenAll(tasks);
        await Task.Delay(300); // Wait for database operations

        // Assert
        var profile = _hybridService.CurrentProfile;
        Assert.Contains("Skill1", profile.KnownSkills);
        Assert.Contains("Skill2", profile.KnownSkills);
        Assert.Contains("Skill3", profile.KnownSkills);
        Assert.Contains("Style1", profile.PreferredLearningStyles);
        Assert.Contains("Style2", profile.PreferredLearningStyles);

        // Verify in database
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILearningProfileRepository>();
        var dbProfile = await repository.GetLatestAsync();
        
        Assert.NotNull(dbProfile);
        Assert.Contains("Skill1", dbProfile.KnownSkills);
        Assert.Contains("Skill2", dbProfile.KnownSkills);
        Assert.Contains("Skill3", dbProfile.KnownSkills);
    }

    public new void Dispose()
    {
        _serviceProvider?.Dispose();
        base.Dispose();
    }
}