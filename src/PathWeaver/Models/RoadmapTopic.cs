namespace PathWeaver.Models;

public class RoadmapTopic
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int Order { get; set; }
    public List<LearningResource> Resources { get; set; } = new List<LearningResource>();
}
