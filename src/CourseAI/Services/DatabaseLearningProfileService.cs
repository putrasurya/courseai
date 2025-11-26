using CourseAI.Models;
using CourseAI.Data.Repositories;
using System.Text.Json;

namespace CourseAI.Services;

public class DatabaseLearningProfileService
{
    private readonly ILearningProfileRepository _profileRepository;
    private readonly object _lock = new object();
    private LearningProfile? _cachedProfile;
    private readonly ILogger<DatabaseLearningProfileService> _logger;

    public event Action<LearningProfile>? ProfileChanged;

    public DatabaseLearningProfileService(ILearningProfileRepository profileRepository, ILogger<DatabaseLearningProfileService> logger)
    {
        _profileRepository = profileRepository;
        _logger = logger;
    }

    public async Task<LearningProfile> GetCurrentProfileAsync()
    {
        lock (_lock)
        {
            if (_cachedProfile != null)
                return _cachedProfile;
        }

        var profile = await _profileRepository.GetLatestAsync();
        if (profile == null)
        {
            profile = new LearningProfile { Id = Guid.NewGuid() };
            profile = await _profileRepository.CreateAsync(profile);
            _logger.LogInformation("Created new learning profile with ID {ProfileId}", profile.Id);
        }

        lock (_lock)
        {
            _cachedProfile = profile;
        }

        return profile;
    }

    public LearningProfile CurrentProfile
    {
        get
        {
            // For synchronous access, return cached profile or create a new one
            lock (_lock)
            {
                if (_cachedProfile != null)
                    return _cachedProfile;
                
                // If no cached profile, return a temporary one
                // The caller should use GetCurrentProfileAsync for proper database access
                return new LearningProfile();
            }
        }
    }

    public async Task UpdateProfileAsync(string field, string value)
    {
        var profile = await GetCurrentProfileAsync();
        
        lock (_lock)
        {
            switch (field.ToLower())
            {
                case "learninggoal":
                    profile.LearningGoal = value;
                    break;
                case "experiencelevel":
                    profile.ExperienceLevel = value;
                    break;
                case "knownskills":
                    if (!string.IsNullOrEmpty(value) && !profile.KnownSkills.Contains(value))
                        profile.KnownSkills.Add(value);
                    break;
                case "preferredlearningstyles":
                    if (!string.IsNullOrEmpty(value) && !profile.PreferredLearningStyles.Contains(value))
                        profile.PreferredLearningStyles.Add(value);
                    break;
            }
        }

        await _profileRepository.UpdateAsync(profile);
        ProfileChanged?.Invoke(profile);
        _logger.LogInformation("Updated profile field {Field} with value {Value}", field, value);
    }

    public async Task RemoveFromProfileAsync(string field, string value)
    {
        var profile = await GetCurrentProfileAsync();
        
        lock (_lock)
        {
            switch (field.ToLower())
            {
                case "knownskills":
                    profile.KnownSkills.Remove(value);
                    break;
                case "preferredlearningstyles":
                    profile.PreferredLearningStyles.Remove(value);
                    break;
            }
        }

        await _profileRepository.UpdateAsync(profile);
        ProfileChanged?.Invoke(profile);
        _logger.LogInformation("Removed {Value} from profile field {Field}", value, field);
    }

    public string GetProfileSummary()
    {
        var profile = CurrentProfile;
        if (profile == null)
            return "No learning profile available.";

        return $"Learning Goal: {profile.LearningGoal ?? "Not specified"}\n" +
               $"Experience Level: {profile.ExperienceLevel ?? "Not specified"}\n" +
               $"Known Skills: {string.Join(", ", profile.KnownSkills.Where(s => !string.IsNullOrEmpty(s)))}\n" +
               $"Preferred Learning Styles: {string.Join(", ", profile.PreferredLearningStyles.Where(s => !string.IsNullOrEmpty(s)))}";
    }

    public bool IsProfileSufficient()
    {
        var profile = CurrentProfile;
        return profile != null &&
               !string.IsNullOrWhiteSpace(profile.LearningGoal) &&
               !string.IsNullOrWhiteSpace(profile.ExperienceLevel) &&
               (profile.KnownSkills?.Count > 0 || profile.PreferredLearningStyles?.Count > 0);
    }

    public async Task ClearProfileAsync()
    {
        var profile = new LearningProfile { Id = Guid.NewGuid() };
        profile = await _profileRepository.CreateAsync(profile);
        
        lock (_lock)
        {
            _cachedProfile = profile;
        }
        
        ProfileChanged?.Invoke(profile);
        _logger.LogInformation("Cleared profile and created new one with ID {ProfileId}", profile.Id);
    }

    public async Task SetProfileAsync(LearningProfile profile)
    {
        if (profile.Id == Guid.Empty)
        {
            profile.Id = Guid.NewGuid();
            profile = await _profileRepository.CreateAsync(profile);
        }
        else
        {
            profile = await _profileRepository.UpdateAsync(profile);
        }

        lock (_lock)
        {
            _cachedProfile = profile;
        }
        
        ProfileChanged?.Invoke(profile);
        _logger.LogInformation("Set profile with ID {ProfileId}", profile.Id);
    }

    public LearningProfile GetProfileCopy()
    {
        var profile = CurrentProfile;
        // Return a deep copy to prevent external modifications
        var json = JsonSerializer.Serialize(profile);
        return JsonSerializer.Deserialize<LearningProfile>(json) ?? new LearningProfile();
    }

    // Backward compatibility methods for the existing LearningProfileService interface
    public void UpdateProfile(string field, string value)
    {
        // Fire and forget for backward compatibility
        _ = Task.Run(async () => await UpdateProfileAsync(field, value));
    }

    public void RemoveFromProfile(string field, string value)
    {
        // Fire and forget for backward compatibility  
        _ = Task.Run(async () => await RemoveFromProfileAsync(field, value));
    }

    public void ClearProfile()
    {
        // Fire and forget for backward compatibility
        _ = Task.Run(async () => await ClearProfileAsync());
    }

    public void SetProfile(LearningProfile profile)
    {
        // Fire and forget for backward compatibility
        _ = Task.Run(async () => await SetProfileAsync(profile));
    }
}