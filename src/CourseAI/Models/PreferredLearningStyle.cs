namespace CourseAI.Models;

public class PreferredLearningStyle
{
    public Guid Id { get; set; }
    public Guid LearningProfileId { get; set; }
    public string? Style { get; set; }
    
    // Navigation property
    public virtual LearningProfile? LearningProfile { get; set; }
}