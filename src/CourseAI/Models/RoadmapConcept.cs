namespace CourseAI.Models;

public class RoadmapConcept
{
    public Guid Id { get; set; }
    public Guid RoadmapTopicId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    
    // Navigation property
    public virtual RoadmapTopic? RoadmapTopic { get; set; }
}
