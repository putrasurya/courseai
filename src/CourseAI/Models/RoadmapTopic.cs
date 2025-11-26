namespace CourseAI.Models;

public class RoadmapTopic
{
    public Guid Id { get; set; }
    public Guid RoadmapModuleId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    public List<RoadmapConcept> Concepts { get; set; } = new();
    public int ConfidenceScore { get; set; } = 0;
    
    // Navigation property
    public virtual RoadmapModule? RoadmapModule { get; set; }
}
