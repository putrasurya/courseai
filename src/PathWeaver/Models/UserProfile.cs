namespace PathWeaver.Models;

public class UserProfile
{
    public string? LearningGoal { get; set; }
    public List<string?> KnownSkills { get; set; } = new();
    public List<string?> PreferredLearningStyles { get; set; } = new();
    public string? ExperienceLevel { get; set; }
}
