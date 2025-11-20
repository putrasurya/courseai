namespace PathWeaver.Models;

public class RoadmapTopic
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    public List<RoadmapConcept> Concepts { get; set; } = new();
    public List<LearningResource> Resources { get; set; } = new();
    public int ConfidenceScore { get; set; } = 0;
}
