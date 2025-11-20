using PathWeaver.Models;
using System.Text.Json;

namespace PathWeaver.Services;

public class UserProfileService
{
    private UserProfile _currentProfile;
    private readonly object _lock = new object();

    public event Action<UserProfile>? ProfileChanged;

    public UserProfile CurrentProfile
    {
        get
        {
            lock (_lock)
            {
                return _currentProfile;
            }
        }
        private set
        {
            lock (_lock)
            {
                _currentProfile = value;
                ProfileChanged?.Invoke(_currentProfile);
            }
        }
    }

    public UserProfileService()
    {
        _currentProfile = new UserProfile();
    }

    // Profile update methods
    public void UpdateProfile(string field, string value)
    {
        lock (_lock)
        {
            switch (field.ToLower())
            {
                case "learninggoal":
                    _currentProfile.LearningGoal = value;
                    break;
                case "experiencelevel":
                    _currentProfile.ExperienceLevel = value;
                    break;
                case "knownskills":
                    if (!string.IsNullOrEmpty(value) && !_currentProfile.KnownSkills.Contains(value))
                        _currentProfile.KnownSkills.Add(value);
                    break;
                case "preferredlearningstyles":
                    if (!string.IsNullOrEmpty(value) && !_currentProfile.PreferredLearningStyles.Contains(value))
                        _currentProfile.PreferredLearningStyles.Add(value);
                    break;
            }
            
            // Trigger change notification
            ProfileChanged?.Invoke(_currentProfile);
        }
    }

    public void RemoveFromProfile(string field, string value)
    {
        lock (_lock)
        {
            switch (field.ToLower())
            {
                case "knownskills":
                    _currentProfile.KnownSkills.Remove(value);
                    break;
                case "preferredlearningstyles":
                    _currentProfile.PreferredLearningStyles.Remove(value);
                    break;
            }
            
            // Trigger change notification
            ProfileChanged?.Invoke(_currentProfile);
        }
    }

    public string GetProfileSummary()
    {
        lock (_lock)
        {
            if (_currentProfile == null)
                return "No user profile available.";

            return $"Learning Goal: {_currentProfile.LearningGoal ?? "Not specified"}\n" +
                   $"Experience Level: {_currentProfile.ExperienceLevel ?? "Not specified"}\n" +
                   $"Known Skills: {string.Join(", ", _currentProfile.KnownSkills.Where(s => !string.IsNullOrEmpty(s)))}\n" +
                   $"Preferred Learning Styles: {string.Join(", ", _currentProfile.PreferredLearningStyles.Where(s => !string.IsNullOrEmpty(s)))}";
        }
    }

    public bool IsProfileSufficient()
    {
        lock (_lock)
        {
            return _currentProfile != null &&
                   !string.IsNullOrWhiteSpace(_currentProfile.LearningGoal) &&
                   !string.IsNullOrWhiteSpace(_currentProfile.ExperienceLevel) &&
                   (_currentProfile.KnownSkills?.Count > 0 || _currentProfile.PreferredLearningStyles?.Count > 0);
        }
    }

    public void ClearProfile()
    {
        CurrentProfile = new UserProfile();
    }

    public void SetProfile(UserProfile profile)
    {
        CurrentProfile = profile ?? new UserProfile();
    }

    public UserProfile GetProfileCopy()
    {
        lock (_lock)
        {
            // Return a deep copy to prevent external modifications
            var json = JsonSerializer.Serialize(_currentProfile);
            return JsonSerializer.Deserialize<UserProfile>(json) ?? new UserProfile();
        }
    }
}