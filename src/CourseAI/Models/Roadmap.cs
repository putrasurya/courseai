namespace CourseAI.Models;

public class Roadmap
{
    public Guid Id { get; set; }
    public Guid LearningProfileId { get; set; }
    public LearningProfile? LearningProfile { get; set; }
    public List<RoadmapModule> Modules { get; set; } = new List<RoadmapModule>();
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public RoadmapStatus Status { get; set; }
}
