namespace PathWeaver.Models;

public class RoadmapModule
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    public int EstimatedTimeMinutes { get; set; }
    public List<RoadmapTopic> Topics { get; set; } = new List<RoadmapTopic>();
}
