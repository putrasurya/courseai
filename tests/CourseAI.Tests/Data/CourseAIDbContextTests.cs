using CourseAI.Models;
using Microsoft.EntityFrameworkCore;
using CourseAI.Tests.Data;

namespace CourseAI.Tests.Data;

public class CourseAIDbContextTests : DatabaseTestBase
{
    [Fact]
    public void DbContext_ShouldHaveAllRequiredDbSets()
    {
        // Assert
        Assert.NotNull(Context.LearningProfiles);
        Assert.NotNull(Context.KnownSkills);
        Assert.NotNull(Context.PreferredLearningStyles);
        Assert.NotNull(Context.Roadmaps);
        Assert.NotNull(Context.RoadmapModules);
        Assert.NotNull(Context.RoadmapTopics);
        Assert.NotNull(Context.RoadmapConcepts);
        Assert.NotNull(Context.LearningResources);
    }

    [Fact]
    public async Task DbContext_ShouldSaveAndRetrieveComplexData()
    {
        // Arrange
        var profile = new LearningProfile
        {
            Id = Guid.NewGuid(),
            LearningGoal = "Complex Learning Goal",
            ExperienceLevel = "Advanced",
            KnownSkills = new List<string?> { "C#", "TypeScript", "React", "Azure" },
            PreferredLearningStyles = new List<string?> { "Interactive", "Documentation", "Video" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

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
                    Title = "Advanced Patterns",
                    Description = "Learn advanced programming patterns",
                    Order = 1,
                    EstimatedDuration = TimeSpan.FromHours(25),
                    Topics = new List<RoadmapTopic>
                    {
                        new RoadmapTopic
                        {
                            Id = Guid.NewGuid(),
                            RoadmapModuleId = Guid.NewGuid(),
                            Title = "Design Patterns",
                            Description = "Common design patterns",
                            Order = 1,
                            ConfidenceScore = 85,
                            Concepts = new List<RoadmapConcept>
                            {
                                new RoadmapConcept
                                {
                                    Id = Guid.NewGuid(),
                                    RoadmapTopicId = Guid.NewGuid(),
                                    Title = "Singleton Pattern",
                                    Description = "Ensure a class has only one instance",
                                    Order = 1
                                },
                                new RoadmapConcept
                                {
                                    Id = Guid.NewGuid(),
                                    RoadmapTopicId = Guid.NewGuid(),
                                    Title = "Factory Pattern",
                                    Description = "Create objects without specifying exact class",
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
                            Title = "Design Patterns Book",
                            Url = "https://example.com/design-patterns",
                            Type = ResourceType.Book,
                            Source = "Example Publisher",
                            Description = "Comprehensive guide to design patterns"
                        },
                        new LearningResource
                        {
                            Id = Guid.NewGuid(),
                            RoadmapModuleId = Guid.NewGuid(),
                            Title = "Patterns in C#",
                            Url = "https://example.com/patterns-csharp",
                            Type = ResourceType.Tutorial,
                            Source = "Example Tutorial Site",
                            Description = "Design patterns implementation in C#"
                        }
                    }
                }
            }
        };

        // Act
        await Context.LearningProfiles.AddAsync(profile);
        await Context.Roadmaps.AddAsync(roadmap);
        await Context.SaveChangesAsync();

        // Assert
        var savedProfile = await Context.LearningProfiles.FindAsync(profile.Id);
        var savedRoadmap = await Context.Roadmaps.FindAsync(roadmap.Id);

        Assert.NotNull(savedProfile);
        Assert.NotNull(savedRoadmap);

        // Verify profile data
        Assert.Equal("Complex Learning Goal", savedProfile.LearningGoal);
        Assert.Equal("Advanced", savedProfile.ExperienceLevel);
        Assert.Equal(4, savedProfile.KnownSkills.Count);
        Assert.Contains("C#", savedProfile.KnownSkills);
        Assert.Contains("TypeScript", savedProfile.KnownSkills);
        Assert.Equal(3, savedProfile.PreferredLearningStyles.Count);
        Assert.Contains("Interactive", savedProfile.PreferredLearningStyles);

        // Verify roadmap data
        Assert.Equal(profile.Id, savedRoadmap.LearningProfileId);
        Assert.Equal(RoadmapStatus.InProgress, savedRoadmap.Status);
        Assert.Single(savedRoadmap.Modules);

        var savedModule = savedRoadmap.Modules.First();
        Assert.Equal("Advanced Patterns", savedModule.Title);
        Assert.Equal(TimeSpan.FromHours(25), savedModule.EstimatedDuration);
        Assert.Single(savedModule.Topics);
        Assert.Equal(2, savedModule.Resources.Count);

        var savedTopic = savedModule.Topics.First();
        Assert.Equal("Design Patterns", savedTopic.Title);
        Assert.Equal(85, savedTopic.ConfidenceScore);
        Assert.Equal(2, savedTopic.Concepts.Count);

        var concepts = savedTopic.Concepts.OrderBy(c => c.Order).ToList();
        Assert.Equal("Singleton Pattern", concepts[0].Title);
        Assert.Equal("Factory Pattern", concepts[1].Title);

        var resources = savedModule.Resources.OrderBy(r => r.Type).ToList();
        Assert.Equal(ResourceType.Book, resources[0].Type);
        Assert.Equal(ResourceType.Tutorial, resources[1].Type);
        Assert.Equal("Design Patterns Book", resources[0].Title);
    }

    [Fact]
    public async Task DbContext_ShouldHandleEnumConversions()
    {
        // Arrange
        var roadmap = new Roadmap
        {
            Id = Guid.NewGuid(),
            LearningProfileId = Guid.NewGuid(),
            Status = RoadmapStatus.Completed,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow,
            Modules = new List<RoadmapModule>
            {
                new RoadmapModule
                {
                    Id = Guid.NewGuid(),
                    RoadmapId = Guid.NewGuid(),
                    Title = "Test Module",
                    Order = 1,
                    EstimatedDuration = TimeSpan.FromHours(1),
                    Topics = new List<RoadmapTopic>(),
                    Resources = new List<LearningResource>
                    {
                        new LearningResource
                        {
                            Id = Guid.NewGuid(),
                            RoadmapModuleId = Guid.NewGuid(),
                            Title = "Test Resource",
                            Type = ResourceType.Video,
                            Url = "https://example.com",
                            Source = "Example"
                        }
                    }
                }
            }
        };

        // Act
        await Context.Roadmaps.AddAsync(roadmap);
        await Context.SaveChangesAsync();

        // Clear context to ensure fresh read
        Context.ChangeTracker.Clear();

        // Assert
        var savedRoadmap = await Context.Roadmaps.FindAsync(roadmap.Id);
        Assert.NotNull(savedRoadmap);
        Assert.Equal(RoadmapStatus.Completed, savedRoadmap.Status);

        var savedResource = savedRoadmap.Modules.First().Resources.First();
        Assert.Equal(ResourceType.Video, savedResource.Type);
    }

    [Fact]
    public async Task DbContext_ShouldHandleTimeSpanConversions()
    {
        // Arrange
        var testDurations = new[]
        {
            TimeSpan.FromMinutes(30),
            TimeSpan.FromHours(2.5),
            TimeSpan.FromDays(1),
            TimeSpan.FromHours(40.75)
        };

        // Act & Assert
        for (int i = 0; i < testDurations.Length; i++)
        {
            var roadmap = new Roadmap
            {
                Id = Guid.NewGuid(),
                LearningProfileId = Guid.NewGuid(),
                Status = RoadmapStatus.Draft,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Modules = new List<RoadmapModule>
                {
                    new RoadmapModule
                    {
                        Id = Guid.NewGuid(),
                        RoadmapId = Guid.NewGuid(),
                        Title = $"Module {i}",
                        Order = i + 1,
                        EstimatedDuration = testDurations[i],
                        Topics = new List<RoadmapTopic>(),
                        Resources = new List<LearningResource>()
                    }
                }
            };

            await Context.Roadmaps.AddAsync(roadmap);
        }

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Verify all durations were saved and loaded correctly
        var savedRoadmaps = await Context.Roadmaps.ToListAsync();
        Assert.Equal(testDurations.Length, savedRoadmaps.Count);

        foreach (var roadmap in savedRoadmaps)
        {
            var module = roadmap.Modules.First();
            var expectedDuration = testDurations[module.Order - 1];
            Assert.Equal(expectedDuration, module.EstimatedDuration);
        }
    }

    [Fact]
    public async Task DbContext_ShouldHandleEmptyAndNullCollections()
    {
        // Arrange
        var profileWithEmptyLists = new LearningProfile
        {
            Id = Guid.NewGuid(),
            LearningGoal = "Test Goal",
            ExperienceLevel = "Beginner",
            KnownSkills = new List<string?>(), // Empty list
            PreferredLearningStyles = new List<string?>(), // Empty list
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var roadmapWithEmptyModules = new Roadmap
        {
            Id = Guid.NewGuid(),
            LearningProfileId = profileWithEmptyLists.Id,
            Status = RoadmapStatus.Draft,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow,
            Modules = new List<RoadmapModule>() // Empty list
        };

        // Act
        await Context.LearningProfiles.AddAsync(profileWithEmptyLists);
        await Context.Roadmaps.AddAsync(roadmapWithEmptyModules);
        await Context.SaveChangesAsync();

        // Clear and reload
        Context.ChangeTracker.Clear();

        // Assert
        var savedProfile = await Context.LearningProfiles.FindAsync(profileWithEmptyLists.Id);
        var savedRoadmap = await Context.Roadmaps.FindAsync(roadmapWithEmptyModules.Id);

        Assert.NotNull(savedProfile);
        Assert.NotNull(savedRoadmap);

        Assert.Empty(savedProfile.KnownSkills);
        Assert.Empty(savedProfile.PreferredLearningStyles);
        Assert.Empty(savedRoadmap.Modules);
    }

    [Fact]
    public async Task DbContext_ShouldHandleUnicodeAndSpecialCharacters()
    {
        // Arrange
        var profile = new LearningProfile
        {
            Id = Guid.NewGuid(),
            LearningGoal = "Apprendre la programmation avec émojis 🚀 and unicode characters: αβγ",
            ExperienceLevel = "Débutant",
            KnownSkills = new List<string?> 
            { 
                "JavaScript (ES6+)", 
                "C# & .NET", 
                "SQL queries with 'quotes'",
                "HTML/CSS with <tags>",
                "日本語プログラミング" // Japanese
            },
            PreferredLearningStyles = new List<string?> 
            { 
                "Vidéos en français", 
                "Hands-on with \"quotes\"",
                "Books & documentation"
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        await Context.LearningProfiles.AddAsync(profile);
        await Context.SaveChangesAsync();

        // Clear and reload
        Context.ChangeTracker.Clear();

        // Assert
        var savedProfile = await Context.LearningProfiles.FindAsync(profile.Id);
        Assert.NotNull(savedProfile);

        Assert.Equal("Apprendre la programmation avec émojis 🚀 and unicode characters: αβγ", savedProfile.LearningGoal);
        Assert.Equal("Débutant", savedProfile.ExperienceLevel);
        Assert.Contains("JavaScript (ES6+)", savedProfile.KnownSkills);
        Assert.Contains("C# & .NET", savedProfile.KnownSkills);
        Assert.Contains("SQL queries with 'quotes'", savedProfile.KnownSkills);
        Assert.Contains("HTML/CSS with <tags>", savedProfile.KnownSkills);
        Assert.Contains("日本語プログラミング", savedProfile.KnownSkills);
        Assert.Contains("Vidéos en français", savedProfile.PreferredLearningStyles);
        Assert.Contains("Hands-on with \"quotes\"", savedProfile.PreferredLearningStyles);
        Assert.Contains("Books & documentation", savedProfile.PreferredLearningStyles);
    }

    [Fact]
    public async Task DbContext_ShouldHandleLargeDataSets()
    {
        // Arrange - Create profile with many skills and styles
        var manySkills = Enumerable.Range(1, 50)
            .Select(i => $"Skill {i}")
            .ToList<string?>();

        var manyStyles = Enumerable.Range(1, 30)
            .Select(i => $"Learning Style {i}")
            .ToList<string?>();

        var profile = new LearningProfile
        {
            Id = Guid.NewGuid(),
            LearningGoal = "Master everything",
            ExperienceLevel = "Guru",
            KnownSkills = manySkills,
            PreferredLearningStyles = manyStyles,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create roadmap with many modules
        var manyModules = Enumerable.Range(1, 20)
            .Select(i => new RoadmapModule
            {
                Id = Guid.NewGuid(),
                RoadmapId = Guid.NewGuid(),
                Title = $"Module {i}",
                Description = $"Description for module {i}",
                Order = i,
                EstimatedDuration = TimeSpan.FromHours(i * 2),
                Topics = new List<RoadmapTopic>(),
                Resources = new List<LearningResource>()
            }).ToList();

        var roadmap = new Roadmap
        {
            Id = Guid.NewGuid(),
            LearningProfileId = profile.Id,
            Status = RoadmapStatus.InProgress,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow,
            Modules = manyModules
        };

        // Act
        await Context.LearningProfiles.AddAsync(profile);
        await Context.Roadmaps.AddAsync(roadmap);
        await Context.SaveChangesAsync();

        // Clear and reload
        Context.ChangeTracker.Clear();

        // Assert
        var savedProfile = await Context.LearningProfiles.FindAsync(profile.Id);
        var savedRoadmap = await Context.Roadmaps.FindAsync(roadmap.Id);

        Assert.NotNull(savedProfile);
        Assert.NotNull(savedRoadmap);

        Assert.Equal(50, savedProfile.KnownSkills.Count);
        Assert.Equal(30, savedProfile.PreferredLearningStyles.Count);
        Assert.Equal(20, savedRoadmap.Modules.Count);

        Assert.Contains("Skill 25", savedProfile.KnownSkills);
        Assert.Contains("Learning Style 15", savedProfile.PreferredLearningStyles);
        
        var module10 = savedRoadmap.Modules.FirstOrDefault(m => m.Title == "Module 10");
        Assert.NotNull(module10);
        Assert.Equal(TimeSpan.FromHours(20), module10.EstimatedDuration);
    }

    [Fact]
    public void DbContext_ShouldHaveProperModelConfiguration()
    {
        // Act
        var model = Context.Model;
        
        // Assert - Check that all entities are configured
        var learningProfileEntity = model.FindEntityType(typeof(LearningProfile));
        var roadmapEntity = model.FindEntityType(typeof(Roadmap));
        var moduleEntity = model.FindEntityType(typeof(RoadmapModule));
        var topicEntity = model.FindEntityType(typeof(RoadmapTopic));
        var conceptEntity = model.FindEntityType(typeof(RoadmapConcept));
        var resourceEntity = model.FindEntityType(typeof(LearningResource));

        Assert.NotNull(learningProfileEntity);
        Assert.NotNull(roadmapEntity);
        Assert.NotNull(moduleEntity);
        Assert.NotNull(topicEntity);
        Assert.NotNull(conceptEntity);
        Assert.NotNull(resourceEntity);

        // Check primary keys
        Assert.NotNull(learningProfileEntity.FindPrimaryKey());
        Assert.NotNull(roadmapEntity.FindPrimaryKey());
        Assert.NotNull(moduleEntity.FindPrimaryKey());
        Assert.NotNull(topicEntity.FindPrimaryKey());
        Assert.NotNull(conceptEntity.FindPrimaryKey());
        Assert.NotNull(resourceEntity.FindPrimaryKey());

        // Check foreign keys
        var roadmapForeignKeys = roadmapEntity.GetForeignKeys().ToList();
        Assert.Single(roadmapForeignKeys); // Should have FK to LearningProfile
        Assert.Equal(nameof(LearningProfile), roadmapForeignKeys.First().PrincipalEntityType.ShortName());
    }
}