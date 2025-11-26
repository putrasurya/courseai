using CourseAI.Data.Repositories;
using CourseAI.Models;
using Microsoft.Extensions.Logging;

namespace CourseAI.Tests.Data;

public class RoadmapRepositoryTests : DatabaseTestBase
{
    private readonly IRoadmapRepository _roadmapRepository;
    private readonly ILearningProfileRepository _profileRepository;
    private readonly ILogger<RoadmapRepository> _logger;

    public RoadmapRepositoryTests()
    {
        _logger = LoggerFactory.CreateLogger<RoadmapRepository>();
        _roadmapRepository = new RoadmapRepository(Context);
        _profileRepository = new LearningProfileRepository(Context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRoadmapExists_ShouldReturnRoadmapWithProfile()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        var roadmap = CreateTestRoadmap(profile.Id);
        await Context.Roadmaps.AddAsync(roadmap);
        await Context.SaveChangesAsync();

        // Act
        var result = await _roadmapRepository.GetByIdAsync(roadmap.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(roadmap.Id, result.Id);
        Assert.Equal(profile.Id, result.LearningProfileId);
        Assert.NotNull(result.LearningProfile);
        Assert.Equal(profile.LearningGoal, result.LearningProfile.LearningGoal);
        Assert.Equal(roadmap.Status, result.Status);
        Assert.Equal(roadmap.Modules.Count, result.Modules.Count);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRoadmapDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _roadmapRepository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByProfileIdAsync_WhenRoadmapsExist_ShouldReturnLatest()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        var oldRoadmap = CreateTestRoadmap(profile.Id);
        oldRoadmap.LastModifiedDate = DateTime.UtcNow.AddDays(-1);

        var newRoadmap = CreateTestRoadmap(profile.Id);
        newRoadmap.LastModifiedDate = DateTime.UtcNow;

        await Context.Roadmaps.AddRangeAsync(oldRoadmap, newRoadmap);
        await Context.SaveChangesAsync();

        // Act
        var result = await _roadmapRepository.GetByProfileIdAsync(profile.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newRoadmap.Id, result.Id);
        Assert.Equal(profile.Id, result.LearningProfileId);
    }

    [Fact]
    public async Task GetByProfileIdAsync_WhenNoRoadmaps_ShouldReturnNull()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        // Act
        var result = await _roadmapRepository.GetByProfileIdAsync(profile.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByProfileIdAllAsync_ShouldReturnAllRoadmapsForProfile()
    {
        // Arrange
        var profile1 = CreateTestLearningProfile();
        var profile2 = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile1);
        await _profileRepository.CreateAsync(profile2);

        var roadmap1a = CreateTestRoadmap(profile1.Id);
        var roadmap1b = CreateTestRoadmap(profile1.Id);
        var roadmap2 = CreateTestRoadmap(profile2.Id);

        await Context.Roadmaps.AddRangeAsync(roadmap1a, roadmap1b, roadmap2);
        await Context.SaveChangesAsync();

        // Act
        var result = await _roadmapRepository.GetByProfileIdAllAsync(profile1.Id);

        // Assert
        var roadmapsList = result.ToList();
        Assert.Equal(2, roadmapsList.Count);
        Assert.All(roadmapsList, r => Assert.Equal(profile1.Id, r.LearningProfileId));
        Assert.DoesNotContain(roadmapsList, r => r.Id == roadmap2.Id);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateRoadmapWithGeneratedId()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        var roadmap = CreateTestRoadmap(profile.Id);
        roadmap.Id = Guid.Empty; // Test auto-generation

        // Act
        var result = await _roadmapRepository.CreateAsync(roadmap);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(profile.Id, result.LearningProfileId);
        Assert.Equal(roadmap.Status, result.Status);
        Assert.True(result.CreatedDate > DateTime.MinValue);
        Assert.True(result.LastModifiedDate > DateTime.MinValue);

        // Verify complex data
        Assert.Equal(roadmap.Modules.Count, result.Modules.Count);
        var firstModule = result.Modules.First();
        Assert.Equal("React Fundamentals", firstModule.Title);
        Assert.Equal(1, firstModule.Topics.Count);
        Assert.Equal(1, firstModule.Resources.Count);
    }

    [Fact]
    public async Task CreateAsync_ShouldPreserveComplexRoadmapStructure()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        var roadmap = new Roadmap
        {
            Id = Guid.NewGuid(),
            LearningProfileId = profile.Id,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow,
            Status = RoadmapStatus.InProgress,
            Modules = new List<RoadmapModule>
            {
                new RoadmapModule
                {
                    Id = Guid.NewGuid(),
                    RoadmapId = Guid.NewGuid(),
                    Title = "Advanced React Patterns",
                    Description = "Learn advanced React concepts",
                    Order = 1,
                    EstimatedDuration = TimeSpan.FromHours(40),
                    Topics = new List<RoadmapTopic>
                    {
                        new RoadmapTopic
                        {
                            Id = Guid.NewGuid(),
                            RoadmapModuleId = Guid.NewGuid(),
                            Title = "Hooks",
                            Description = "Master React Hooks",
                            Order = 1,
                            ConfidenceScore = 75,
                            Concepts = new List<RoadmapConcept>
                            {
                                new RoadmapConcept
                                {
                                    Id = Guid.NewGuid(),
                                    RoadmapTopicId = Guid.NewGuid(),
                                    Title = "useState Hook",
                                    Description = "State management with hooks",
                                    Order = 1
                                },
                                new RoadmapConcept
                                {
                                    Id = Guid.NewGuid(),
                                    RoadmapTopicId = Guid.NewGuid(),
                                    Title = "useEffect Hook",
                                    Description = "Side effects with hooks",
                                    Order = 2
                                }
                            }
                        }
                    },
                    Resources = new List<LearningResource>
                    {
                        new LearningResource
                        {
                            Id = Guid.NewGuid(),
                            RoadmapModuleId = Guid.NewGuid(),
                            Title = "React Hooks Documentation",
                            Url = "https://react.dev/reference/react",
                            Type = ResourceType.Documentation,
                            Source = "React.dev",
                            Description = "Official hooks documentation"
                        },
                        new LearningResource
                        {
                            Id = Guid.NewGuid(),
                            RoadmapModuleId = Guid.NewGuid(),
                            Title = "Advanced React Course",
                            Url = "https://example.com/advanced-react",
                            Type = ResourceType.Course,
                            Source = "Example Platform",
                            Description = "Comprehensive advanced React course"
                        }
                    }
                }
            }
        };

        // Act
        var result = await _roadmapRepository.CreateAsync(roadmap);

        // Assert
        Assert.Equal(RoadmapStatus.InProgress, result.Status);
        Assert.Single(result.Modules);
        
        var module = result.Modules.First();
        Assert.Equal("Advanced React Patterns", module.Title);
        Assert.Equal(TimeSpan.FromHours(40), module.EstimatedDuration);
        Assert.Single(module.Topics);
        Assert.Equal(2, module.Resources.Count);
        
        var topic = module.Topics.First();
        Assert.Equal("Hooks", topic.Title);
        Assert.Equal(75, topic.ConfidenceScore);
        Assert.Equal(2, topic.Concepts.Count);
        
        var concepts = topic.Concepts.OrderBy(c => c.Order).ToList();
        Assert.Equal("useState Hook", concepts[0].Title);
        Assert.Equal("useEffect Hook", concepts[1].Title);
        
        var resources = module.Resources.OrderBy(r => r.Type).ToList();
        Assert.Equal(ResourceType.Course, resources[0].Type);
        Assert.Equal(ResourceType.Documentation, resources[1].Type);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateRoadmapAndTimestamp()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        var roadmap = CreateTestRoadmap(profile.Id);
        await _roadmapRepository.CreateAsync(roadmap);

        var originalLastModified = roadmap.LastModifiedDate;
        
        // Modify roadmap
        roadmap.Status = RoadmapStatus.Completed;
        roadmap.Modules.First().Title = "Updated Module Title";

        // Wait to ensure timestamp will be different
        await Task.Delay(10);

        // Act
        var result = await _roadmapRepository.UpdateAsync(roadmap);

        // Assert
        Assert.Equal(RoadmapStatus.Completed, result.Status);
        Assert.Equal("Updated Module Title", result.Modules.First().Title);
        Assert.True(result.LastModifiedDate > originalLastModified);

        // Verify in database
        var dbRoadmap = await _roadmapRepository.GetByIdAsync(roadmap.Id);
        Assert.NotNull(dbRoadmap);
        Assert.Equal(RoadmapStatus.Completed, dbRoadmap.Status);
        Assert.Equal("Updated Module Title", dbRoadmap.Modules.First().Title);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveRoadmap()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        var roadmap = CreateTestRoadmap(profile.Id);
        await _roadmapRepository.CreateAsync(roadmap);

        // Verify roadmap exists
        var existingRoadmap = await _roadmapRepository.GetByIdAsync(roadmap.Id);
        Assert.NotNull(existingRoadmap);

        // Act
        await _roadmapRepository.DeleteAsync(roadmap.Id);

        // Assert
        var deletedRoadmap = await _roadmapRepository.GetByIdAsync(roadmap.Id);
        Assert.Null(deletedRoadmap);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnCorrectStatus()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        var roadmap = CreateTestRoadmap(profile.Id);
        await _roadmapRepository.CreateAsync(roadmap);

        // Act & Assert
        var exists = await _roadmapRepository.ExistsAsync(roadmap.Id);
        Assert.True(exists);

        var notExists = await _roadmapRepository.ExistsAsync(Guid.NewGuid());
        Assert.False(notExists);
    }

    [Fact]
    public async Task Repository_ShouldHandleMultipleProfilesAndRoadmaps()
    {
        // Arrange
        var profile1 = CreateTestLearningProfile();
        var profile2 = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile1);
        await _profileRepository.CreateAsync(profile2);

        var roadmap1a = CreateTestRoadmap(profile1.Id);
        var roadmap1b = CreateTestRoadmap(profile1.Id);
        var roadmap2 = CreateTestRoadmap(profile2.Id);

        // Act
        await _roadmapRepository.CreateAsync(roadmap1a);
        await _roadmapRepository.CreateAsync(roadmap1b);
        await _roadmapRepository.CreateAsync(roadmap2);

        // Assert
        var allRoadmapsForProfile1 = await _roadmapRepository.GetByProfileIdAllAsync(profile1.Id);
        var allRoadmapsForProfile2 = await _roadmapRepository.GetByProfileIdAllAsync(profile2.Id);

        Assert.Equal(2, allRoadmapsForProfile1.Count());
        Assert.Single(allRoadmapsForProfile2);
        
        var latestForProfile1 = await _roadmapRepository.GetByProfileIdAsync(profile1.Id);
        var latestForProfile2 = await _roadmapRepository.GetByProfileIdAsync(profile2.Id);
        
        Assert.NotNull(latestForProfile1);
        Assert.NotNull(latestForProfile2);
        Assert.Equal(profile1.Id, latestForProfile1.LearningProfileId);
        Assert.Equal(profile2.Id, latestForProfile2.LearningProfileId);
    }

    [Fact]
    public async Task CreateAsync_ShouldHandleEmptyModulesAndNullValues()
    {
        // Arrange
        var profile = CreateTestLearningProfile();
        await _profileRepository.CreateAsync(profile);

        var roadmap = new Roadmap
        {
            Id = Guid.NewGuid(),
            LearningProfileId = profile.Id,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow,
            Status = RoadmapStatus.Draft,
            Modules = new List<RoadmapModule>() // Empty modules list
        };

        // Act
        var result = await _roadmapRepository.CreateAsync(roadmap);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Modules);
        Assert.Equal(RoadmapStatus.Draft, result.Status);

        // Verify in database
        var dbRoadmap = await _roadmapRepository.GetByIdAsync(result.Id);
        Assert.NotNull(dbRoadmap);
        Assert.Empty(dbRoadmap.Modules);
    }
}