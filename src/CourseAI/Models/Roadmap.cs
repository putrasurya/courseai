namespace CourseAI.Models;

public class Roadmap
{
    public Guid Id { get; set; }
    public UserProfile? UserProfile { get; set; }
    public List<RoadmapModule> Modules { get; set; } = new List<RoadmapModule>();
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public RoadmapStatus Status { get; set; }
}
