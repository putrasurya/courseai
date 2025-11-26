namespace CourseAI.Models;

public class LearningProfile
{
    public Guid Id { get; set; }
    public string? LearningGoal { get; set; }
    public List<string?> KnownSkills { get; set; } = new();
    public List<string?> PreferredLearningStyles { get; set; } = new();
    public string? ExperienceLevel { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Roadmap> Roadmaps { get; set; } = new List<Roadmap>();
}
