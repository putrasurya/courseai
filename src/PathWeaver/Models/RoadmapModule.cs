using System.Linq;

namespace PathWeaver.Models;

public class RoadmapModule
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public List<RoadmapTopic> Topics { get; set; } = new List<RoadmapTopic>();
    public List<LearningResource> Resources { get; set; } = new List<LearningResource>();
    public int AverageConfidence => Topics.Any() ? (int)Topics.Average(t => t.ConfidenceScore) : 0;
}
