using PathWeaver.Models;
using PathWeaver.Services;
using Xunit;

namespace PathWeaver.Tests.Services;

public class RoadmapServiceTests
{
    private readonly RoadmapService _roadmapService;

    public RoadmapServiceTests()
    {
        _roadmapService = new RoadmapService();
    }

    [Fact]
    public void InitializeRoadMap_ShouldCreateNewRoadmap_WhenValidInputProvided()
    {
        // Arrange
        var userProfileSummary = "User wants to learn web development";

        // Act
        var result = _roadmapService.InitializeRoadMap(userProfileSummary);

        // Assert
        Assert.Equal("Roadmap initialized successfully", result);
        var roadmap = _roadmapService.GetRoadMap();
        Assert.NotNull(roadmap);
        Assert.NotEqual(Guid.Empty, roadmap.Id);
        Assert.Equal(RoadmapStatus.Draft, roadmap.Status);
        Assert.Empty(roadmap.Modules);
    }

    [Fact]
    public void UpdateRoadMapStatus_ShouldUpdateStatus_WhenValidStatusProvided()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");

        // Act
        var result = _roadmapService.UpdateRoadMapStatus(RoadmapStatus.Active);

        // Assert
        Assert.Equal("Roadmap status updated to Active", result);
        var roadmap = _roadmapService.GetRoadMap();
        Assert.Equal(RoadmapStatus.Active, roadmap?.Status);
    }

    [Fact]
    public void UpdateRoadMapStatus_ShouldUpdateStatus_WhenEmptyRoadmapExists()
    {
        // Act
        var result = _roadmapService.UpdateRoadMapStatus(RoadmapStatus.Active);

        // Assert
        Assert.Equal("Roadmap status updated to Active", result);
    }

    [Fact]
    public void GetRoadMapSummary_ShouldReturnSummary_WhenRoadmapExists()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");

        // Act
        var result = _roadmapService.GetRoadMapSummary();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Roadmap Status: Draft", result);
        Assert.Contains("Modules: 0", result);
        Assert.Contains("Topics: 0", result);
        Assert.Contains("Resources: 0", result);
    }

    [Fact]
    public void GetRoadMapSummary_ShouldReturnSummary_WhenEmptyRoadmapExists()
    {
        // Act
        var result = _roadmapService.GetRoadMapSummary();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Modules: 0", result);
        Assert.Contains("Topics: 0", result);
        Assert.Contains("Resources: 0", result);
    }

    [Fact]
    public void AddModule_ShouldAddModuleToRoadmap_WhenValidInputProvided()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        var title = "HTML Basics";
        var description = "Learn HTML fundamentals";
        var estimatedDurationHours = 40;

        // Act
        var result = _roadmapService.AddModule(title, description, estimatedDurationHours);

        // Assert
        Assert.Equal("Module 'HTML Basics' added successfully", result);
        var roadmap = _roadmapService.GetRoadMap();
        Assert.Single(roadmap?.Modules ?? new List<RoadmapModule>());
        var module = roadmap?.Modules.First();
        Assert.Equal(title, module?.Title);
        Assert.Equal(description, module?.Description);
        Assert.Equal(TimeSpan.FromHours(estimatedDurationHours), module?.EstimatedDuration);
        Assert.Equal(1, module?.Order);
    }

    [Fact]
    public void AddModule_ShouldAddModuleToEmptyRoadmap_WhenValidInputProvided()
    {
        // Act
        var result = _roadmapService.AddModule("Test", "Description", 10);

        // Assert
        Assert.Equal("Module 'Test' added successfully", result);
    }

    [Fact]
    public void UpdateModule_ShouldUpdateModule_WhenValidInputProvided()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        _roadmapService.AddModule("Original Title", "Original Description", 20);

        // Act
        var result = _roadmapService.UpdateModule("Original Title", "Updated Title", "Updated Description", 30);

        // Assert
        Assert.Equal("Module updated successfully", result);
        var roadmap = _roadmapService.GetRoadMap();
        var module = roadmap?.Modules.First();
        Assert.Equal("Updated Title", module?.Title);
        Assert.Equal("Updated Description", module?.Description);
        Assert.Equal(TimeSpan.FromHours(30), module?.EstimatedDuration);
    }

    [Fact]
    public void RemoveModule_ShouldRemoveModule_WhenModuleExists()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        _roadmapService.AddModule("Module 1", "Description 1", 10);
        _roadmapService.AddModule("Module 2", "Description 2", 15);

        // Act
        var result = _roadmapService.RemoveModule("Module 1");

        // Assert
        Assert.Equal("Module 'Module 1' removed successfully", result);
        var roadmap = _roadmapService.GetRoadMap();
        Assert.Single(roadmap?.Modules ?? new List<RoadmapModule>());
        Assert.Equal("Module 2", roadmap?.Modules.First().Title);
        Assert.Equal(1, roadmap?.Modules.First().Order); // Should be reordered
    }

    [Fact]
    public void AddTopicToModule_ShouldAddTopic_WhenValidInputProvided()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        _roadmapService.AddModule("HTML Basics", "Learn HTML", 20);

        // Act
        var result = _roadmapService.AddTopicToModule("HTML Basics", "HTML Tags", "Learn about HTML tags", 75);

        // Assert
        Assert.Equal("Topic 'HTML Tags' added to module 'HTML Basics'", result);
        var roadmap = _roadmapService.GetRoadMap();
        var module = roadmap?.Modules.First();
        Assert.Single(module?.Topics ?? new List<RoadmapTopic>());
        var topic = module?.Topics.First();
        Assert.Equal("HTML Tags", topic?.Title);
        Assert.Equal("Learn about HTML tags", topic?.Description);
        Assert.Equal(75, topic?.ConfidenceScore);
    }

    [Fact]
    public void UpdateTopicConfidence_ShouldUpdateConfidence_WhenValidInputProvided()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        _roadmapService.AddModule("HTML Basics", "Learn HTML", 20);
        _roadmapService.AddTopicToModule("HTML Basics", "HTML Tags", "Learn about HTML tags", 50);

        // Act
        var result = _roadmapService.UpdateTopicConfidence("HTML Basics", "HTML Tags", 85);

        // Assert
        Assert.Equal("Topic 'HTML Tags' confidence updated to 85", result);
        var roadmap = _roadmapService.GetRoadMap();
        var topic = roadmap?.Modules.First().Topics.First();
        Assert.Equal(85, topic?.ConfidenceScore);
    }

    [Fact]
    public void AddConceptToTopic_ShouldAddConcept_WhenValidInputProvided()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        _roadmapService.AddModule("HTML Basics", "Learn HTML", 20);
        _roadmapService.AddTopicToModule("HTML Basics", "HTML Tags", "Learn about HTML tags", 50);

        // Act
        var result = _roadmapService.AddConceptToTopic("HTML Basics", "HTML Tags", "div element", "Block-level container");

        // Assert
        Assert.Equal("Concept 'div element' added to topic 'HTML Tags'", result);
        var roadmap = _roadmapService.GetRoadMap();
        var topic = roadmap?.Modules.First().Topics.First();
        Assert.Single(topic?.Concepts ?? new List<RoadmapConcept>());
        var concept = topic?.Concepts.First();
        Assert.Equal("div element", concept?.Title);
        Assert.Equal("Block-level container", concept?.Description);
    }

    [Fact]
    public void AddResourceToModule_ShouldAddResource_WhenValidInputProvided()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        _roadmapService.AddModule("HTML Basics", "Learn HTML", 20);

        // Act
        var result = _roadmapService.AddResourceToModule(
            "HTML Basics", 
            "MDN HTML Guide", 
            "https://developer.mozilla.org/en-US/docs/Web/HTML",
            ResourceType.Documentation,
            "Mozilla",
            "Comprehensive HTML documentation"
        );

        // Assert
        Assert.Equal("Resource 'MDN HTML Guide' added to module 'HTML Basics'", result);
        var roadmap = _roadmapService.GetRoadMap();
        var module = roadmap?.Modules.First();
        Assert.Single(module?.Resources ?? new List<LearningResource>());
        var resource = module?.Resources.First();
        Assert.Equal("MDN HTML Guide", resource?.Title);
        Assert.Equal("https://developer.mozilla.org/en-US/docs/Web/HTML", resource?.Url);
        Assert.Equal(ResourceType.Documentation, resource?.Type);
    }

    [Fact]
    public void GetRoadMapAnalysis_ShouldReturnAnalysis_WhenRoadmapHasData()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        _roadmapService.AddModule("HTML Basics", "Learn HTML", 20);
        _roadmapService.AddTopicToModule("HTML Basics", "HTML Tags", "Learn about HTML tags", 75);

        // Act
        var result = _roadmapService.GetRoadMapAnalysis();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Total Modules: 1", result);
        Assert.Contains("Total Topics: 1", result);
        Assert.Contains("Total Estimated Duration: 20 hours", result);
        Assert.Contains("Average Confidence Score: 75", result);
    }

    [Fact]
    public void GetRoadMapAnalysis_WhenNoRoadmap_ReturnsNoRoadmapMessage()
    {
        // Arrange
        var service = new RoadmapService();
        service.SetRoadMap(null!);

        // Act
        var result = service.GetRoadMapAnalysis();

        // Assert
        Assert.Equal("No roadmap available", result);
    }

    [Fact]
    public void GetRoadMapAnalysis_WhenEmptyRoadmap_ReturnsEmptyMessage()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test Goal");

        // Act
        var result = _roadmapService.GetRoadMapAnalysis();

        // Assert
        Assert.Equal("Roadmap is empty - no modules available", result);
    }

    [Fact]
    public void GetRoadMapAnalysis_WhenRoadmapHasModulesWithoutTopics_DoesNotThrow()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test Goal");
        _roadmapService.AddModule("Module 1", "Description 1", 10);

        // Act & Assert
        var exception = Record.Exception(() => _roadmapService.GetRoadMapAnalysis());
        Assert.Null(exception);
        
        var result = _roadmapService.GetRoadMapAnalysis();
        Assert.Contains("Average Confidence Score: 0.0%", result);
        Assert.Contains("Total Modules: 1", result);
        Assert.Contains("Total Topics: 0", result);
    }

    [Fact]
    public void GetRoadMapAnalysis_WithMultipleModulesAndTopics_CalculatesCorrectAverageConfidence()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test Goal");
        _roadmapService.AddModule("Module 1", "Description 1", 10);
        _roadmapService.AddModule("Module 2", "Description 2", 15);
        _roadmapService.AddTopicToModule("Module 1", "Topic 1", "Description", 80);
        _roadmapService.AddTopicToModule("Module 2", "Topic 2", "Description", 90);

        // Act
        var result = _roadmapService.GetRoadMapAnalysis();

        // Assert
        Assert.Contains("Average Confidence Score: 85.0%", result); // (80 + 90) / 2 = 85
        Assert.Contains("Total Modules: 2", result);
        Assert.Contains("Total Topics: 2", result);
    }

    [Fact]
    public void ValidateRoadmapQuality_WithAllTopicsHavingConcepts_ReturnsSuccess()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        _roadmapService.AddModule("Module 1", "Description", 10);
        _roadmapService.AddTopicToModule("Module 1", "Topic 1", "Description", 75);
        _roadmapService.AddConceptToTopic("Module 1", "Topic 1", "Concept 1", "Description 1");
        _roadmapService.AddConceptToTopic("Module 1", "Topic 1", "Concept 2", "Description 2");
        _roadmapService.AddConceptToTopic("Module 1", "Topic 1", "Concept 3", "Description 3");

        // Act
        var result = _roadmapService.ValidateRoadmapQuality();

        // Assert
        Assert.Contains("✅ Roadmap validation passed", result);
    }

    [Fact]
    public void ValidateRoadmapQuality_WithTopicsMissingConcepts_ReturnsValidationErrors()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        _roadmapService.AddModule("Module 1", "Description", 10);
        _roadmapService.AddTopicToModule("Module 1", "Topic 1", "Description", 75);
        // Not adding any concepts

        // Act
        var result = _roadmapService.ValidateRoadmapQuality();

        // Assert
        Assert.Contains("❌ Roadmap validation issues found", result);
        Assert.Contains("CRITICAL: Topics without any key concepts", result);
        Assert.Contains("Module 'Module 1' > Topic 'Topic 1'", result);
    }

    [Fact]
    public void GetTopicsNeedingConcepts_WithInsufficientConcepts_ReturnsTopicsList()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        _roadmapService.AddModule("Module 1", "Description", 10);
        _roadmapService.AddTopicToModule("Module 1", "Topic 1", "Description", 75);
        _roadmapService.AddConceptToTopic("Module 1", "Topic 1", "Concept 1", "Description 1");
        // Only 1 concept, needs 3-5

        // Act
        var result = _roadmapService.GetTopicsNeedingConcepts();

        // Assert
        Assert.Contains("Topics needing key concepts", result);
        Assert.Contains("Module: 'Module 1' | Topic: 'Topic 1' | Current Concepts: 1", result);
    }

    [Fact]
    public void GetTopicsNeedingConcepts_WithSufficientConcepts_ReturnsSuccess()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        _roadmapService.AddModule("Module 1", "Description", 10);
        _roadmapService.AddTopicToModule("Module 1", "Topic 1", "Description", 75);
        _roadmapService.AddConceptToTopic("Module 1", "Topic 1", "Concept 1", "Description 1");
        _roadmapService.AddConceptToTopic("Module 1", "Topic 1", "Concept 2", "Description 2");
        _roadmapService.AddConceptToTopic("Module 1", "Topic 1", "Concept 3", "Description 3");

        // Act
        var result = _roadmapService.GetTopicsNeedingConcepts();

        // Assert
        Assert.Contains("✅ All topics have sufficient key concepts", result);
    }

    [Fact]
    public void ValidateRoadmapQuality_ShouldFailValidation_WhenModuleHasNoTopics()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        _roadmapService.AddModule("HTML Basics", "Learn HTML", 20);
        // Note: Not adding any topics to the module

        // Act
        var result = _roadmapService.ValidateRoadmapQuality();

        // Assert
        Assert.Contains("CRITICAL: Modules without any topics:", result);
        Assert.Contains("Module 'HTML Basics'", result);
        Assert.Contains("❌ Roadmap validation issues found:", result);
    }

    [Fact]
    public void ValidateRoadmapQuality_ShouldFailValidation_WhenTopicHasNoConcepts()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        _roadmapService.AddModule("HTML Basics", "Learn HTML", 20);
        _roadmapService.AddTopicToModule("HTML Basics", "HTML Tags", "Learn about HTML tags", 50);
        // Note: Not adding any concepts to the topic

        // Act
        var result = _roadmapService.ValidateRoadmapQuality();

        // Assert
        Assert.Contains("CRITICAL: Topics without any key concepts:", result);
        Assert.Contains("Module 'HTML Basics' > Topic 'HTML Tags'", result);
        Assert.Contains("❌ Roadmap validation issues found:", result);
    }

    [Fact]
    public void ValidateRoadmapQuality_ShouldPassValidation_WhenAllModulesHaveTopicsWithConcepts()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        _roadmapService.AddModule("HTML Basics", "Learn HTML", 20);
        _roadmapService.AddTopicToModule("HTML Basics", "HTML Tags", "Learn about HTML tags", 50);
        _roadmapService.AddConceptToTopic("HTML Basics", "HTML Tags", "div element", "Block-level container");
        _roadmapService.AddConceptToTopic("HTML Basics", "HTML Tags", "span element", "Inline container");
        _roadmapService.AddConceptToTopic("HTML Basics", "HTML Tags", "p element", "Paragraph element");

        // Act
        var result = _roadmapService.ValidateRoadmapQuality();

        // Assert
        Assert.Contains("✅ Roadmap validation passed", result);
        Assert.Contains("All modules have topics and all topics have appropriate key concepts", result);
    }

    [Fact]
    public void GetModulesNeedingTopics_ShouldReturnEmptyModules()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        _roadmapService.AddModule("HTML Basics", "Learn HTML", 20);
        _roadmapService.AddModule("CSS Basics", "Learn CSS", 25);
        _roadmapService.AddTopicToModule("HTML Basics", "HTML Tags", "Learn about HTML tags", 50);
        // Note: CSS Basics module has no topics

        // Act
        var result = _roadmapService.GetModulesNeedingTopics();

        // Assert
        Assert.Contains("Module: 'CSS Basics' | Current Topics: 0", result);
        Assert.DoesNotContain("HTML Basics", result);
    }

    [Fact]
    public void GetModulesNeedingTopics_ShouldReturnSuccessMessage_WhenAllModulesHaveTopics()
    {
        // Arrange
        _roadmapService.InitializeRoadMap("Test summary");
        _roadmapService.AddModule("HTML Basics", "Learn HTML", 20);
        _roadmapService.AddTopicToModule("HTML Basics", "HTML Tags", "Learn about HTML tags", 50);

        // Act
        var result = _roadmapService.GetModulesNeedingTopics();

        // Assert
        Assert.Equal("✅ All modules have topics.", result);
    }
}