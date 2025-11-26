namespace CourseAI.Models;

public class KnownSkill
{
    public Guid Id { get; set; }
    public Guid LearningProfileId { get; set; }
    public string? Skill { get; set; }
    
    // Navigation property
    public virtual LearningProfile? LearningProfile { get; set; }
}