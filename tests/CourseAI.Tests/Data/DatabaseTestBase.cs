using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CourseAI.Models;
using CourseAI.Data;

namespace CourseAI.Tests.Data;

public abstract class DatabaseTestBase : IDisposable
{
    protected CourseAIDbContext Context { get; private set; }
    protected ILoggerFactory LoggerFactory { get; private set; }

    protected DatabaseTestBase()
    {
        LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug);
        });

        var options = new DbContextOptionsBuilder<CourseAIDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .LogTo(message => System.Diagnostics.Debug.WriteLine(message))
            .Options;

        Context = new CourseAIDbContext(options);
        Context.Database.EnsureCreated();
    }

    protected LearningProfile CreateTestLearningProfile()
    {
        return new LearningProfile
        {
            Id = Guid.NewGuid(),
            LearningGoal = "Learn React development",
            ExperienceLevel = "Beginner",
            KnownSkills = new List<string?> { "HTML", "CSS", "JavaScript" },
            PreferredLearningStyles = new List<string?> { "Video tutorials", "Hands-on practice" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    protected Roadmap CreateTestRoadmap(Guid? learningProfileId = null)
    {
        var profileId = learningProfileId ?? Guid.NewGuid();
        
        return new Roadmap
        {
            Id = Guid.NewGuid(),
            LearningProfileId = profileId,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow,
            Status = RoadmapStatus.Draft,
            Modules = new List<RoadmapModule>
            {
                new RoadmapModule
                {
                    Id = Guid.NewGuid(),
                    RoadmapId = Guid.NewGuid(),
                    Title = "React Fundamentals",
                    Description = "Learn the basics of React",
                    Order = 1,
                    EstimatedDuration = TimeSpan.FromHours(20),
                    Topics = new List<RoadmapTopic>
                    {
                        new RoadmapTopic
                        {
                            Id = Guid.NewGuid(),
                            RoadmapModuleId = Guid.NewGuid(),
                            Title = "Components",
                            Description = "Learn about React components",
                            Order = 1,
                            ConfidenceScore = 0,
                            Concepts = new List<RoadmapConcept>
                            {
                                new RoadmapConcept
                                {
                                    Id = Guid.NewGuid(),
                                    RoadmapTopicId = Guid.NewGuid(),
                                    Title = "Functional Components",
                                    Description = "Understanding functional components",
                                    Order = 1
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
                            Title = "React Official Tutorial",
                            Url = "https://react.dev/learn",
                            Type = ResourceType.Tutorial,
                            Source = "React.dev",
                            Description = "Official React tutorial"
                        }
                    }
                }
            }
        };
    }

    public void Dispose()
    {
        Context.Dispose();
        LoggerFactory.Dispose();
    }
}