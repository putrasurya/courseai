using System.ComponentModel;
using CourseAI.Models;

namespace CourseAI.Services;

public class RoadmapService
{
    private Roadmap? _roadmap;

    public RoadmapService()
    {
        _roadmap = new Roadmap();
    }

    public RoadmapService(Roadmap? roadmap)
    {
        _roadmap = roadmap ?? new Roadmap();
    }

    public void SetRoadMap(Roadmap roadmap)
    {
        _roadmap = roadmap;
    }

    public Roadmap? GetRoadMap()
    {
        return _roadmap;
    }

    // Roadmap Management Tools
    [Description("Initialize a new roadmap with basic information")]
    public string InitializeRoadMap(string userProfileSummary)
    {
        _roadmap = new Roadmap
        {
            Id = Guid.NewGuid(),
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow,
            Status = RoadmapStatus.Draft,
            Modules = new List<RoadmapModule>()
        };
        return "Roadmap initialized successfully";
    }

    [Description("Update roadmap status")]
    public string UpdateRoadMapStatus(RoadmapStatus status)
    {
        if (_roadmap == null)
            return "No roadmap available to update";

        _roadmap.Status = status;
        _roadmap.LastModifiedDate = DateTime.UtcNow;
        return $"Roadmap status updated to {status}";
    }

    [Description("Get roadmap summary with modules count and status")]
    public string GetRoadMapSummary()
    {
        if (_roadmap == null)
            return "No roadmap available";

        var moduleCount = _roadmap.Modules.Count;
        var topicCount = _roadmap.Modules.Sum(m => m.Topics.Count);
        var resourceCount = _roadmap.Modules.Sum(m => m.Resources.Count);
        
        return $"Roadmap Status: {_roadmap.Status}, Modules: {moduleCount}, Topics: {topicCount}, Resources: {resourceCount}, Created: {_roadmap.CreatedDate:yyyy-MM-dd}";
    }

    // Module Management Tools
    [Description("Add a new module to the roadmap")]
    public string AddModule(string title, string description, int estimatedDurationHours)
    {
        if (_roadmap == null)
            return "No roadmap available";

        var module = new RoadmapModule
        {
            Title = title,
            Description = description,
            Order = _roadmap.Modules.Count + 1,
            EstimatedDuration = TimeSpan.FromHours(estimatedDurationHours),
            Topics = new List<RoadmapTopic>(),
            Resources = new List<LearningResource>()
        };

        _roadmap.Modules.Add(module);
        _roadmap.LastModifiedDate = DateTime.UtcNow;
        return $"Module '{title}' added successfully";
    }

    [Description("Update an existing module")]
    public string UpdateModule(string currentTitle, string newTitle, string description, int estimatedDurationHours)
    {
        if (_roadmap == null)
            return "No roadmap available";

        var module = _roadmap.Modules.FirstOrDefault(m => m.Title == currentTitle);
        if (module == null)
            return $"Module '{currentTitle}' not found";

        module.Title = newTitle;
        module.Description = description;
        module.EstimatedDuration = TimeSpan.FromHours(estimatedDurationHours);
        _roadmap.LastModifiedDate = DateTime.UtcNow;
        return $"Module updated successfully";
    }

    [Description("Remove a module from the roadmap")]
    public string RemoveModule(string title)
    {
        if (_roadmap == null)
            return "No roadmap available";

        var module = _roadmap.Modules.FirstOrDefault(m => m.Title == title);
        if (module == null)
            return $"Module '{title}' not found";

        _roadmap.Modules.Remove(module);
        
        // Reorder remaining modules
        for (int i = 0; i < _roadmap.Modules.Count; i++)
        {
            _roadmap.Modules[i].Order = i + 1;
        }
        
        _roadmap.LastModifiedDate = DateTime.UtcNow;
        return $"Module '{title}' removed successfully";
    }

    [Description("Get list of all modules with basic info")]
    public string GetAllModules()
    {
        if (_roadmap == null)
            return "No roadmap available";

        if (!_roadmap.Modules.Any())
            return "No modules in roadmap";

        var moduleInfo = _roadmap.Modules
            .OrderBy(m => m.Order)
            .Select(m => $"{m.Order}. {m.Title} ({m.EstimatedDuration.TotalHours}h) - {m.Topics.Count} topics, {m.Resources.Count} resources")
            .ToList();

        return string.Join("\n", moduleInfo);
    }

    // Topic Management Tools
    [Description("Add a topic to a specific module")]
    public string AddTopicToModule(string moduleTitle, string topicTitle, string topicDescription, int confidenceScore = 0)
    {
        if (_roadmap == null)
            return "No roadmap available";

        var module = _roadmap.Modules.FirstOrDefault(m => m.Title == moduleTitle);
        if (module == null)
            return $"Module '{moduleTitle}' not found";

        var topic = new RoadmapTopic
        {
            Title = topicTitle,
            Description = topicDescription,
            Order = module.Topics.Count + 1,
            ConfidenceScore = confidenceScore,
            Concepts = new List<RoadmapConcept>()
        };

        module.Topics.Add(topic);
        _roadmap.LastModifiedDate = DateTime.UtcNow;
        return $"Topic '{topicTitle}' added to module '{moduleTitle}'";
    }

    [Description("Update topic confidence score")]
    public string UpdateTopicConfidence(string moduleTitle, string topicTitle, int confidenceScore)
    {
        if (_roadmap == null)
            return "No roadmap available";

        var module = _roadmap.Modules.FirstOrDefault(m => m.Title == moduleTitle);
        if (module == null)
            return $"Module '{moduleTitle}' not found";

        var topic = module.Topics.FirstOrDefault(t => t.Title == topicTitle);
        if (topic == null)
            return $"Topic '{topicTitle}' not found in module '{moduleTitle}'";

        topic.ConfidenceScore = Math.Max(0, Math.Min(100, confidenceScore)); // Ensure 0-100 range
        _roadmap.LastModifiedDate = DateTime.UtcNow;
        return $"Topic '{topicTitle}' confidence updated to {confidenceScore}";
    }

    [Description("Get topics for a specific module")]
    public string GetModuleTopics(string moduleTitle)
    {
        if (_roadmap == null)
            return "No roadmap available";

        var module = _roadmap.Modules.FirstOrDefault(m => m.Title == moduleTitle);
        if (module == null)
            return $"Module '{moduleTitle}' not found";

        if (!module.Topics.Any())
            return $"No topics in module '{moduleTitle}'";

        var topicInfo = module.Topics
            .OrderBy(t => t.Order)
            .Select(t => $"{t.Order}. {t.Title} (Confidence: {t.ConfidenceScore}%) - {t.Concepts.Count} concepts")
            .ToList();

        return string.Join("\n", topicInfo);
    }

    // Concept Management Tools
    [Description("Add a concept to a specific topic")]
    public string AddConceptToTopic(string moduleTitle, string topicTitle, string conceptTitle, string conceptDescription)
    {
        if (_roadmap == null)
            return "No roadmap available";

        var module = _roadmap.Modules.FirstOrDefault(m => m.Title == moduleTitle);
        if (module == null)
            return $"Module '{moduleTitle}' not found";

        var topic = module.Topics.FirstOrDefault(t => t.Title == topicTitle);
        if (topic == null)
            return $"Topic '{topicTitle}' not found in module '{moduleTitle}'";

        var concept = new RoadmapConcept
        {
            Title = conceptTitle,
            Description = conceptDescription,
            Order = topic.Concepts.Count + 1
        };

        topic.Concepts.Add(concept);
        _roadmap.LastModifiedDate = DateTime.UtcNow;
        return $"Concept '{conceptTitle}' added to topic '{topicTitle}'";
    }

    [Description("Get concepts for a specific topic")]
    public string GetTopicConcepts(string moduleTitle, string topicTitle)
    {
        if (_roadmap == null)
            return "No roadmap available";

        var module = _roadmap.Modules.FirstOrDefault(m => m.Title == moduleTitle);
        if (module == null)
            return $"Module '{moduleTitle}' not found";

        var topic = module.Topics.FirstOrDefault(t => t.Title == topicTitle);
        if (topic == null)
            return $"Topic '{topicTitle}' not found in module '{moduleTitle}'";

        if (!topic.Concepts.Any())
            return $"No concepts in topic '{topicTitle}'";

        var conceptInfo = topic.Concepts
            .OrderBy(c => c.Order)
            .Select(c => $"{c.Order}. {c.Title}: {c.Description}")
            .ToList();

        return string.Join("\n", conceptInfo);
    }

    // Resource Management Tools
    [Description("Add a learning resource to a module")]
    public string AddResourceToModule(string moduleTitle, string resourceTitle, string url, ResourceType type, string source, string description)
    {
        if (_roadmap == null)
            return "No roadmap available";

        var module = _roadmap.Modules.FirstOrDefault(m => m.Title == moduleTitle);
        if (module == null)
            return $"Module '{moduleTitle}' not found";

        var resource = new LearningResource
        {
            Title = resourceTitle,
            Url = url,
            Type = type,
            Source = source,
            Description = description
        };

        module.Resources.Add(resource);
        _roadmap.LastModifiedDate = DateTime.UtcNow;
        return $"Resource '{resourceTitle}' added to module '{moduleTitle}'";
    }

    [Description("Get resources for a specific module")]
    public string GetModuleResources(string moduleTitle)
    {
        if (_roadmap == null)
            return "No roadmap available";

        var module = _roadmap.Modules.FirstOrDefault(m => m.Title == moduleTitle);
        if (module == null)
            return $"Module '{moduleTitle}' not found";

        if (!module.Resources.Any())
            return $"No resources in module '{moduleTitle}'";

        var resourceInfo = module.Resources
            .Select(r => $"• {r.Title} ({r.Type}) - {r.Source} - {r.Url}")
            .ToList();

        return string.Join("\n", resourceInfo);
    }

    [Description("Remove a resource from a module")]
    public string RemoveResourceFromModule(string moduleTitle, string resourceTitle)
    {
        if (_roadmap == null)
            return "No roadmap available";

        var module = _roadmap.Modules.FirstOrDefault(m => m.Title == moduleTitle);
        if (module == null)
            return $"Module '{moduleTitle}' not found";

        var resource = module.Resources.FirstOrDefault(r => r.Title == resourceTitle);
        if (resource == null)
            return $"Resource '{resourceTitle}' not found in module '{moduleTitle}'";

        module.Resources.Remove(resource);
        _roadmap.LastModifiedDate = DateTime.UtcNow;
        return $"Resource '{resourceTitle}' removed from module '{moduleTitle}'";
    }

    // Resource Quality Tools
    [Description("Validate that all resources in a module have proper URLs and titles")]
    public string ValidateModuleResourceQuality(string moduleTitle)
    {
        if (_roadmap == null)
            return "No roadmap available";

        var module = _roadmap.Modules.FirstOrDefault(m => m.Title == moduleTitle);
        if (module == null)
            return $"Module '{moduleTitle}' not found";

        if (!module.Resources.Any())
            return $"Module '{moduleTitle}' has no resources to validate";

        var issues = new List<string>();
        
        foreach (var resource in module.Resources)
        {
            // Check for missing or invalid URL
            if (string.IsNullOrWhiteSpace(resource.Url))
            {
                issues.Add($"Resource '{resource.Title}' is missing URL");
            }
            else if (!Uri.TryCreate(resource.Url, UriKind.Absolute, out _))
            {
                issues.Add($"Resource '{resource.Title}' has invalid URL: {resource.Url}");
            }
            
            // Check for missing title
            if (string.IsNullOrWhiteSpace(resource.Title))
            {
                issues.Add($"Resource with URL '{resource.Url}' is missing title");
            }
            
            // Check for generic/placeholder content
            if (resource.Title?.Contains("placeholder", StringComparison.OrdinalIgnoreCase) == true ||
                resource.Description?.Contains("placeholder", StringComparison.OrdinalIgnoreCase) == true)
            {
                issues.Add($"Resource '{resource.Title}' appears to be a placeholder");
            }
        }

        if (!issues.Any())
            return $"✅ All {module.Resources.Count} resources in module '{moduleTitle}' have proper URLs and titles";

        return $"❌ Resource quality issues in module '{moduleTitle}':\n{string.Join("\n", issues)}";
    }

    [Description("Get all modules that are missing resources")]
    public string GetModulesWithoutResources()
    {
        if (_roadmap == null)
            return "No roadmap available";

        var modulesWithoutResources = _roadmap.Modules
            .Where(m => !m.Resources.Any())
            .Select(m => m.Title)
            .ToList();

        if (!modulesWithoutResources.Any())
            return "✅ All modules have resources";

        return $"❌ Modules missing resources: {string.Join(", ", modulesWithoutResources)}";
    }

    [Description("Validate that all module resources have actual working URLs")]
    public string ValidateAllResourceUrls()
    {
        if (_roadmap == null)
            return "No roadmap available";

        var invalidUrls = new List<string>();
        var moduleResourceCount = new Dictionary<string, int>();

        foreach (var module in _roadmap.Modules)
        {
            moduleResourceCount[module.Title] = module.Resources.Count;
            
            foreach (var resource in module.Resources)
            {
                if (string.IsNullOrWhiteSpace(resource.Url))
                {
                    invalidUrls.Add($"Module '{module.Title}' - Resource '{resource.Title}': Missing URL");
                }
                else if (!Uri.TryCreate(resource.Url, UriKind.Absolute, out var uri))
                {
                    invalidUrls.Add($"Module '{module.Title}' - Resource '{resource.Title}': Invalid URL format");
                }
                else if (uri.Scheme != "http" && uri.Scheme != "https")
                {
                    invalidUrls.Add($"Module '{module.Title}' - Resource '{resource.Title}': URL must use HTTP/HTTPS");
                }
            }
        }

        var summary = string.Join("\n", moduleResourceCount.Select(kv => $"- {kv.Key}: {kv.Value} resources"));

        if (!invalidUrls.Any())
            return $"✅ All resources have valid URLs\n\nResource Summary:\n{summary}";

        return $"❌ Resource URL validation issues:\n{string.Join("\n", invalidUrls)}\n\nResource Summary:\n{summary}";
    }

    // Analysis Tools
    [Description("Get detailed roadmap analysis")]
    public string GetRoadMapAnalysis()
    {
        if (_roadmap == null)
            return "No roadmap available";

        var totalModules = _roadmap.Modules.Count;
        
        if (totalModules == 0)
            return "Roadmap is empty - no modules available";

        var totalTopics = _roadmap.Modules.Sum(m => m.Topics.Count);
        var totalConcepts = _roadmap.Modules.SelectMany(m => m.Topics).Sum(t => t.Concepts.Count);
        var totalResources = _roadmap.Modules.Sum(m => m.Resources.Count);
        var totalDuration = _roadmap.Modules.Sum(m => m.EstimatedDuration.TotalHours);
        
        var modulesWithTopics = _roadmap.Modules.Where(m => m.Topics.Any()).ToList();
        var averageConfidence = modulesWithTopics.Any() ? modulesWithTopics.Average(m => m.AverageConfidence) : 0;

        return $@"Roadmap Analysis:
- Status: {_roadmap.Status}
- Total Modules: {totalModules}
- Total Topics: {totalTopics}
- Total Concepts: {totalConcepts}
- Total Resources: {totalResources}
- Total Estimated Duration: {totalDuration} hours
- Average Confidence Score: {averageConfidence:F1}%
- Created: {_roadmap.CreatedDate:yyyy-MM-dd HH:mm}
- Last Modified: {_roadmap.LastModifiedDate:yyyy-MM-dd HH:mm}";
    }

    /// <summary>
    /// Validates that all modules have topics, all topics have key concepts, and all modules have resources
    /// </summary>
    /// <returns>Validation report with any missing topics, key concepts, or resources</returns>
    [Description("Validates roadmap quality by checking that all modules have topics, all topics have key concepts, and all modules have resources")]
    public string ValidateRoadmapQuality()
    {
        if (_roadmap == null)
        {
            return "No roadmap exists to validate.";
        }

        var issues = new List<string>();
        var modulesWithoutTopics = new List<string>();
        var topicsWithoutConcepts = new List<string>();
        var modulesWithoutResources = new List<string>();

        foreach (var module in _roadmap.Modules)
        {
            // Check if module has no topics
            if (module.Topics == null || !module.Topics.Any())
            {
                modulesWithoutTopics.Add($"Module '{module.Title}'");
            }
            else
            {
                // Check topics for key concepts
                foreach (var topic in module.Topics)
                {
                    if (topic.Concepts == null || !topic.Concepts.Any())
                    {
                        topicsWithoutConcepts.Add($"Module '{module.Title}' > Topic '{topic.Title}'");
                    }
                    else if (topic.Concepts.Count < 3)
                    {
                        issues.Add($"Module '{module.Title}' > Topic '{topic.Title}' has only {topic.Concepts.Count} key concepts (should have 3-5)");
                    }
                }
            }

            // Check if module has no resources
            if (module.Resources == null || !module.Resources.Any())
            {
                modulesWithoutResources.Add($"Module '{module.Title}'");
            }
        }

        if (modulesWithoutTopics.Any())
        {
            issues.Add("CRITICAL: Modules without any topics:");
            issues.AddRange(modulesWithoutTopics);
        }

        if (topicsWithoutConcepts.Any())
        {
            issues.Add("CRITICAL: Topics without any key concepts:");
            issues.AddRange(topicsWithoutConcepts);
        }

        if (modulesWithoutResources.Any())
        {
            issues.Add("CRITICAL: Modules without any resources:");
            issues.AddRange(modulesWithoutResources);
        }

        if (!issues.Any())
        {
            return "✅ Roadmap validation passed: All modules have topics, all topics have appropriate key concepts, and all modules have resources.";
        }

        return $"❌ Roadmap validation issues found:\n{string.Join("\n", issues)}";
    }

    /// <summary>
    /// Gets a list of topics that are missing key concepts
    /// </summary>
    /// <returns>List of topics needing key concepts</returns>
    [Description("Gets topics that need key concepts added")]
    public string GetTopicsNeedingConcepts()
    {
        if (_roadmap == null)
        {
            return "No roadmap exists.";
        }

        var topicsNeedingConcepts = new List<string>();

        foreach (var module in _roadmap.Modules)
        {
            foreach (var topic in module.Topics)
            {
                if (topic.Concepts == null || topic.Concepts.Count < 3)
                {
                    var currentCount = topic.Concepts?.Count ?? 0;
                    topicsNeedingConcepts.Add($"Module: '{module.Title}' | Topic: '{topic.Title}' | Current Concepts: {currentCount}");
                }
            }
        }

        if (!topicsNeedingConcepts.Any())
        {
            return "✅ All topics have sufficient key concepts.";
        }

        return $"Topics needing key concepts (should have 3-5 each):\n{string.Join("\n", topicsNeedingConcepts)}";
    }

    /// <summary>
    /// Gets a list of modules that have no topics
    /// </summary>
    /// <returns>List of modules needing topics</returns>
    [Description("Gets modules that need topics added")]
    public string GetModulesNeedingTopics()
    {
        if (_roadmap == null)
        {
            return "No roadmap exists.";
        }

        var modulesNeedingTopics = new List<string>();

        foreach (var module in _roadmap.Modules)
        {
            if (module.Topics == null || !module.Topics.Any())
            {
                modulesNeedingTopics.Add($"Module: '{module.Title}' | Current Topics: 0");
            }
        }

        if (!modulesNeedingTopics.Any())
        {
            return "✅ All modules have topics.";
        }

        return $"Modules needing topics:\n{string.Join("\n", modulesNeedingTopics)}";
    }

    /// <summary>
    /// Adds learning resources to a specific module
    /// </summary>
    /// <param name="moduleTitle">The title of the module</param>
    /// <param name="resourcesDescription">Description of the learning resources to add</param>
    /// <returns>Success message</returns>
    [Description("Adds learning resources to a specific module")]
    public string AddResourcesToModule(string moduleTitle, string resourcesDescription)
    {
        if (_roadmap == null)
        {
            return "No roadmap exists. Please initialize the roadmap first.";
        }

        var module = _roadmap.Modules.FirstOrDefault(m => m.Title?.Equals(moduleTitle, StringComparison.OrdinalIgnoreCase) == true);
        if (module == null)
        {
            return $"Module '{moduleTitle}' not found.";
        }

        if (module.Resources == null)
        {
            module.Resources = new List<LearningResource>();
        }

        // Parse the structured resource format from ResourceGatheringAgent
        var resources = ParseResourcesFromDescription(resourcesDescription);
        
        if (resources.Any())
        {
            module.Resources.AddRange(resources);
            return $"Added {resources.Count} resources to module '{moduleTitle}' successfully.";
        }
        else
        {
            // Fallback: create a single resource if parsing fails
            var resource = new LearningResource
            {
                Title = $"Resources for {moduleTitle}",
                Description = resourcesDescription,
                Type = ResourceType.Article,
                Url = string.Empty
            };
            module.Resources.Add(resource);
            return $"Added general resource description to module '{moduleTitle}' successfully.";
        }
    }

    private List<LearningResource> ParseResourcesFromDescription(string description)
    {
        var resources = new List<LearningResource>();
        
        // Split by "**RESOURCE" to find individual resources
        var resourceBlocks = description.Split(new[] { "**RESOURCE" }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var block in resourceBlocks)
        {
            // Skip if this block doesn't contain resource data (like the first block which might be empty or just whitespace)
            if (string.IsNullOrWhiteSpace(block) || !block.Contains("- Title:"))
                continue;
                
            var resource = ParseSingleResource(block);
            if (resource != null && !string.IsNullOrWhiteSpace(resource.Url))
            {
                resources.Add(resource);
            }
        }
        
        return resources;
    }

    private LearningResource? ParseSingleResource(string block)
    {
        var lines = block.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var resource = new LearningResource();
        
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            
            if (trimmed.StartsWith("- Title:", StringComparison.OrdinalIgnoreCase))
            {
                resource.Title = trimmed.Substring(8).Trim();
            }
            else if (trimmed.StartsWith("- URL:", StringComparison.OrdinalIgnoreCase))
            {
                resource.Url = trimmed.Substring(6).Trim();
            }
            else if (trimmed.StartsWith("- Type:", StringComparison.OrdinalIgnoreCase))
            {
                var typeStr = trimmed.Substring(7).Trim();
                if (Enum.TryParse<ResourceType>(typeStr, true, out var resourceType))
                {
                    resource.Type = resourceType;
                }
            }
            else if (trimmed.StartsWith("- Source:", StringComparison.OrdinalIgnoreCase))
            {
                resource.Source = trimmed.Substring(9).Trim();
            }
            else if (trimmed.StartsWith("- Description:", StringComparison.OrdinalIgnoreCase))
            {
                resource.Description = trimmed.Substring(14).Trim();
            }
        }
        
        // Only return if we have at least title and URL
        return !string.IsNullOrWhiteSpace(resource.Title) && !string.IsNullOrWhiteSpace(resource.Url) 
            ? resource 
            : null;
    }

    /// <summary>
    /// Gets a list of modules that have no resources
    /// </summary>
    /// <returns>List of modules needing resources</returns>
    [Description("Gets modules that need resources added")]
    public string GetModulesNeedingResources()
    {
        if (_roadmap == null)
        {
            return "No roadmap exists.";
        }

        var modulesNeedingResources = new List<string>();

        foreach (var module in _roadmap.Modules)
        {
            if (module.Resources == null || !module.Resources.Any())
            {
                modulesNeedingResources.Add($"Module: '{module.Title}' | Current Resources: 0");
            }
        }

        if (!modulesNeedingResources.Any())
        {
            return "✅ All modules have resources.";
        }

        return $"Modules needing resources:\n{string.Join("\n", modulesNeedingResources)}";
    }
}