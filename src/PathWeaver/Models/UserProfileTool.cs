using System.Text.Json;

namespace PathWeaver.Models;

public class UserProfileTool
{
    public static string UpdateUserProfile(string currentProfileJson, string field, string value)
    {
        var profile = string.IsNullOrEmpty(currentProfileJson) 
            ? new UserProfile() 
            : JsonSerializer.Deserialize<UserProfile>(currentProfileJson) ?? new UserProfile();

        switch (field.ToLower())
        {
            case "learninggoal":
                profile.LearningGoal = value;
                break;
            case "experiencelevel":
                profile.ExperienceLevel = value;
                break;
            case "knownskills":
                if (!profile.KnownSkills.Contains(value))
                    profile.KnownSkills.Add(value);
                break;
            case "preferredlearningstyles":
                if (!profile.PreferredLearningStyles.Contains(value))
                    profile.PreferredLearningStyles.Add(value);
                break;
        }

        return JsonSerializer.Serialize(profile);
    }

    public static string RemoveFromUserProfile(string currentProfileJson, string field, string value)
    {
        var profile = JsonSerializer.Deserialize<UserProfile>(currentProfileJson) ?? new UserProfile();

        switch (field.ToLower())
        {
            case "KnownSkills":
                profile.KnownSkills.Remove(value);
                break;
            case "PreferredLearningStyles":
                profile.PreferredLearningStyles.Remove(value);
                break;
        }

        return JsonSerializer.Serialize(profile);
    }

    public static string GetUserProfileSummary(string profileJson)
    {
        if (string.IsNullOrEmpty(profileJson))
            return "No user profile available.";

        var profile = JsonSerializer.Deserialize<UserProfile>(profileJson);
        if (profile == null)
            return "Invalid user profile.";
            
        return $"User Profile (Fields):" +
               $"LearningGoal: {profile.LearningGoal ?? "Not specified"}\n" +
               $"ExperienceLevel: {profile.ExperienceLevel ?? "Not specified"}\n" +
               $"KnownSkills: {string.Join(", ", profile.KnownSkills.Where(s => !string.IsNullOrEmpty(s)))}\n" +
               $"PreferredLearningStyles: {string.Join(", ", profile.PreferredLearningStyles.Where(s => !string.IsNullOrEmpty(s)))}";
    }
}