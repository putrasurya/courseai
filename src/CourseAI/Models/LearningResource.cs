namespace CourseAI.Models;

public class LearningResource
{
    public Guid Id { get; set; }
    public Guid RoadmapModuleId { get; set; }
    public string? Title { get; set; }
    public string? Url { get; set; }
    public ResourceType Type { get; set; }
    public string? Source { get; set; }
    public string? Description { get; set; }
    
    // Navigation property
    public virtual RoadmapModule? RoadmapModule { get; set; }
}
