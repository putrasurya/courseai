using CourseAI.Data.Repositories;
using CourseAI.Models;
using CourseAI.Services;
using Microsoft.Extensions.Logging;
using CourseAI.Tests.Data;

namespace CourseAI.Tests.Services.Database;

public class DatabaseRoadmapServiceTests : DatabaseTestBase
{
    private readonly DatabaseRoadmapService _roadmapService;
    private readonly IRoadmapRepository _roadmapRepository;
    private readonly ILearningProfileRepository _profileRepository;
    private readonly ILogger<DatabaseRoadmapService> _logger;

    public DatabaseRoadmapServiceTests()
    {
        _logger = LoggerFactory.CreateLogger<DatabaseRoadmapService>();
        _roadmapRepository = new RoadmapRepository(Context);
        _profileRepository = new LearningProfileRepository(Context);
        _roadmapService = new DatabaseRoadmapService(_roadmapRepository, _profileRepository, _logger);
    }

    [Fact]
    public async Task GetCurrentRoadmapAsync_WhenRoadmapExists_ShouldReturnLatestRoadmap()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        var oldRoadmap = CreateTestRoadmap(profile.Id);
        oldRoadmap.LastModifiedDate = DateTime.UtcNow.AddDays(-1);

        var newRoadmap = CreateTestRoadmap(profile.Id);
        newRoadmap.LastModifiedDate = DateTime.UtcNow;

        await _roadmapRepository.CreateAsync(oldRoadmap);
        await _roadmapRepository.CreateAsync(newRoadmap);

        // Act
        var result = await _roadmapService.GetCurrentRoadmapAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newRoadmap.Id, result.Id);
        Assert.Equal(profile.Id, result.LearningProfileId);
    }

    [Fact]
    public async Task GetCurrentRoadmapAsync_WhenNoProfileExists_ShouldReturnNull()
    {
        // Act
        var result = await _roadmapService.GetCurrentRoadmapAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentRoadmapAsync_WhenProfileExistsButNoRoadmap_ShouldReturnNull()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        // Act
        var result = await _roadmapService.GetCurrentRoadmapAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task InitializeRoadMapAsync_WithValidProfile_ShouldCreateRoadmap()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        // Act
        var result = await _roadmapService.InitializeRoadMapAsync("Test user profile summary");

        // Assert
        Assert.Equal("Roadmap initialized successfully", result);

        // Verify roadmap was created
        var roadmap = await _roadmapService.GetCurrentRoadmapAsync();
        Assert.NotNull(roadmap);
        Assert.Equal(profile.Id, roadmap.LearningProfileId);
        Assert.Equal(RoadmapStatus.Draft, roadmap.Status);
        Assert.Empty(roadmap.Modules);
    }

    [Fact]
    public async Task InitializeRoadMapAsync_WithoutProfile_ShouldReturnError()
    {
        // Act
        var result = await _roadmapService.InitializeRoadMapAsync("Test summary");

        // Assert
        Assert.Equal("No learning profile available. Please create a profile first.", result);
    }

    [Fact]
    public async Task SetRoadMapAsync_ShouldUpdateOrCreateRoadmap()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        var roadmap = CreateTestRoadmap(profile.Id);
        roadmap.Status = RoadmapStatus.InProgress;

        // Act
        await _roadmapService.SetRoadMapAsync(roadmap);

        // Assert
        var savedRoadmap = await _roadmapService.GetCurrentRoadmapAsync();
        Assert.NotNull(savedRoadmap);
        Assert.Equal(roadmap.Id, savedRoadmap.Id);
        Assert.Equal(RoadmapStatus.InProgress, savedRoadmap.Status);
    }

    [Fact]
    public async Task SetRoadMapAsync_WithoutLearningProfileId_ShouldAssignLatestProfile()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        var roadmap = CreateTestRoadmap();
        roadmap.LearningProfileId = Guid.Empty; // No profile assigned

        // Act
        await _roadmapService.SetRoadMapAsync(roadmap);

        // Assert
        var savedRoadmap = await _roadmapService.GetCurrentRoadmapAsync();
        Assert.NotNull(savedRoadmap);
        Assert.Equal(profile.Id, savedRoadmap.LearningProfileId);
    }

    [Fact]
    public async Task UpdateRoadMapStatusAsync_ShouldUpdateStatus()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        await _roadmapService.InitializeRoadMapAsync("Test summary");
        var originalRoadmap = await _roadmapService.GetCurrentRoadmapAsync();
        Assert.NotNull(originalRoadmap);
        var originalModified = originalRoadmap.LastModifiedDate;

        // Wait to ensure timestamp will be different
        await Task.Delay(10);

        // Act
        var result = await _roadmapService.UpdateRoadMapStatusAsync(RoadmapStatus.Completed);

        // Assert
        Assert.Equal("Roadmap status updated to Completed", result);

        var updatedRoadmap = await _roadmapService.GetCurrentRoadmapAsync();
        Assert.NotNull(updatedRoadmap);
        Assert.Equal(RoadmapStatus.Completed, updatedRoadmap.Status);
        Assert.True(updatedRoadmap.LastModifiedDate > originalModified);
    }

    [Fact]
    public async Task UpdateRoadMapStatusAsync_WithoutRoadmap_ShouldReturnError()
    {
        // Act
        var result = await _roadmapService.UpdateRoadMapStatusAsync(RoadmapStatus.Completed);

        // Assert
        Assert.Equal("No roadmap available to update", result);
    }

    [Fact]
    public async Task AddModuleAsync_ShouldAddModuleToRoadmap()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        await _roadmapService.InitializeRoadMapAsync("Test summary");

        // Act
        var result = await _roadmapService.AddModuleAsync("Test Module", "Test Description", 10);

        // Assert
        Assert.Equal("Module 'Test Module' added successfully", result);

        var roadmap = await _roadmapService.GetCurrentRoadmapAsync();
        Assert.NotNull(roadmap);
        Assert.Single(roadmap.Modules);

        var module = roadmap.Modules.First();
        Assert.Equal("Test Module", module.Title);
        Assert.Equal("Test Description", module.Description);
        Assert.Equal(TimeSpan.FromHours(10), module.EstimatedDuration);
        Assert.Equal(1, module.Order);
        Assert.Empty(module.Topics);
        Assert.Empty(module.Resources);
    }

    [Fact]
    public async Task AddModuleAsync_WithoutRoadmap_ShouldReturnError()
    {
        // Act
        var result = await _roadmapService.AddModuleAsync("Test Module", "Description", 5);

        // Assert
        Assert.Equal("No roadmap available", result);
    }

    [Fact]
    public async Task GetRoadMapSummary_ShouldReturnFormattedSummary()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        var roadmap = CreateTestRoadmap(profile.Id);
        roadmap.Status = RoadmapStatus.InProgress;
        await _roadmapRepository.CreateAsync(roadmap);

        // Act
        var summary = _roadmapService.GetRoadMapSummary();

        // Assert
        Assert.Contains("InProgress", summary);
        Assert.Contains("Modules: 1", summary);
        Assert.Contains("Topics: 1", summary);
        Assert.Contains("Resources: 1", summary);
        Assert.Contains(roadmap.CreatedDate.ToString("yyyy-MM-dd"), summary);
    }

    [Fact]
    public async Task GetRoadMapSummary_WithoutRoadmap_ShouldReturnError()
    {
        // Act
        var summary = _roadmapService.GetRoadMapSummary();

        // Assert
        Assert.Equal("No roadmap available", summary);
    }

    [Fact]
    public async Task GetAllModules_ShouldReturnModuleList()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        await _roadmapService.InitializeRoadMapAsync("Test summary");
        await _roadmapService.AddModuleAsync("Module 1", "Description 1", 5);
        await _roadmapService.AddModuleAsync("Module 2", "Description 2", 8);

        // Act
        var result = _roadmapService.GetAllModules();

        // Assert
        Assert.Contains("1. Module 1 (5h)", result);
        Assert.Contains("2. Module 2 (8h)", result);
        Assert.Contains("0 topics, 0 resources", result);
    }

    [Fact]
    public async Task GetAllModules_WithoutRoadmap_ShouldReturnError()
    {
        // Act
        var result = _roadmapService.GetAllModules();

        // Assert
        Assert.Equal("No roadmap available", result);
    }

    [Fact]
    public async Task GetAllModules_WithEmptyRoadmap_ShouldReturnEmptyMessage()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        await _roadmapService.InitializeRoadMapAsync("Test summary");

        // Act
        var result = _roadmapService.GetAllModules();

        // Assert
        Assert.Equal("No modules in roadmap", result);
    }

    [Fact]
    public async Task BackwardCompatibility_SynchronousMethods_ShouldWork()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        // Act - Use synchronous backward compatibility methods
        var initResult = _roadmapService.InitializeRoadMap("Test summary");
        var addResult = _roadmapService.AddModule("Sync Module", "Sync Description", 3);
        var statusResult = _roadmapService.UpdateRoadMapStatus(RoadmapStatus.InProgress);

        // Assert
        Assert.Equal("Roadmap initialized successfully", initResult);
        Assert.Equal("Module 'Sync Module' added successfully", addResult);
        Assert.Equal("Roadmap status updated to InProgress", statusResult);

        // Verify changes persisted
        var roadmap = await _roadmapService.GetCurrentRoadmapAsync();
        Assert.NotNull(roadmap);
        Assert.Equal(RoadmapStatus.InProgress, roadmap.Status);
        Assert.Single(roadmap.Modules);
        Assert.Equal("Sync Module", roadmap.Modules.First().Title);
    }

    [Fact]
    public async Task Service_ShouldHandleComplexRoadmapOperations()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        // Act - Create complex roadmap
        await _roadmapService.InitializeRoadMapAsync("Comprehensive learning path");
        await _roadmapService.AddModuleAsync("Fundamentals", "Basic concepts", 20);
        await _roadmapService.AddModuleAsync("Intermediate", "Advanced concepts", 30);
        await _roadmapService.AddModuleAsync("Advanced", "Expert level", 40);
        await _roadmapService.UpdateRoadMapStatusAsync(RoadmapStatus.InProgress);

        // Assert
        var roadmap = await _roadmapService.GetCurrentRoadmapAsync();
        Assert.NotNull(roadmap);
        Assert.Equal(RoadmapStatus.InProgress, roadmap.Status);
        Assert.Equal(3, roadmap.Modules.Count);

        var modules = roadmap.Modules.OrderBy(m => m.Order).ToList();
        Assert.Equal("Fundamentals", modules[0].Title);
        Assert.Equal("Intermediate", modules[1].Title);
        Assert.Equal("Advanced", modules[2].Title);
        Assert.Equal(TimeSpan.FromHours(20), modules[0].EstimatedDuration);
        Assert.Equal(TimeSpan.FromHours(30), modules[1].EstimatedDuration);
        Assert.Equal(TimeSpan.FromHours(40), modules[2].EstimatedDuration);
    }

    [Fact]
    public async Task Service_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var profile1 = CreateTestLearningProfile();
        var profile2 = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile1);
        await _profileRepository.CreateAsync(profile2);

        // Act - Create roadmaps for different profiles
        var roadmap1 = CreateTestRoadmap(profile1.Id);
        var roadmap2 = CreateTestRoadmap(profile2.Id);
        await _roadmapService.SetRoadMapAsync(roadmap1);

        // Switch to profile2 by creating a new service (simulating different user)
        var profile2Service = new DatabaseRoadmapService(_roadmapRepository, _profileRepository, _logger);
        
        // Delete profile1 to simulate getting profile2 as latest
        await Context.LearningProfiles.RemoveAsync(await Context.LearningProfiles.FindAsync(profile1.Id)!);
        await Context.SaveChangesAsync();
        
        await profile2Service.SetRoadMapAsync(roadmap2);

        // Assert - Each roadmap should be associated with correct profile
        var savedRoadmap1 = await _roadmapRepository.GetByIdAsync(roadmap1.Id);
        var savedRoadmap2 = await _roadmapRepository.GetByIdAsync(roadmap2.Id);

        Assert.NotNull(savedRoadmap1);
        Assert.NotNull(savedRoadmap2);
        Assert.Equal(profile1.Id, savedRoadmap1.LearningProfileId);
        Assert.Equal(profile2.Id, savedRoadmap2.LearningProfileId);
        Assert.NotEqual(savedRoadmap1.Id, savedRoadmap2.Id);
    }
}