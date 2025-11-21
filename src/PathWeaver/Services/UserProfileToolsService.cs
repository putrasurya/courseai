using System.ComponentModel;
using PathWeaver.Models;
using Microsoft.Extensions.AI;

namespace PathWeaver.Services;

/// <summary>
/// Service that provides UserProfile-related tools that can be shared across multiple agents
/// </summary>
public class UserProfileToolsService
{
    private readonly UserProfileService _userProfileService;

    public UserProfileToolsService(UserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    /// <summary>
    /// Gets all UserProfile tools as AI functions
    /// </summary>
    public AIFunction[] GetAllTools()
    {
        return new[]
        {
            AIFunctionFactory.Create(UpdateUserProfile),
            AIFunctionFactory.Create(RemoveFromUserProfile),
            AIFunctionFactory.Create(GetUserProfileSummary),
            AIFunctionFactory.Create(CheckUserProfileStatus),
            AIFunctionFactory.Create(AddKnownSkill),
            AIFunctionFactory.Create(AddPreferredLearningStyle),
            AIFunctionFactory.Create(SetLearningGoal),
            AIFunctionFactory.Create(SetExperienceLevel)
        };
    }

    /// <summary>
    /// Gets minimal tools for orchestrator (only summary and status check)
    /// </summary>
    public AIFunction[] GetOrchestratorTools()
    {
        return new[]
        {
            AIFunctionFactory.Create(GetUserProfileSummary),
            AIFunctionFactory.Create(CheckUserProfileStatus)
        };
    }

    /// <summary>
    /// Gets comprehensive tools for planner (all profile management)
    /// </summary>
    public AIFunction[] GetPlannerTools()
    {
        return GetAllTools();
    }

    /// <summary>
    /// Gets basic profile tools for structuring agent (summary and basic updates)
    /// </summary>
    public AIFunction[] GetStructuringTools()
    {
        return new[]
        {
            AIFunctionFactory.Create(GetUserProfileSummary),
            AIFunctionFactory.Create(CheckUserProfileStatus),
            AIFunctionFactory.Create(UpdateUserProfile)
        };
    }

    [Description("Update a specific field in the user profile with new information")]
    public string UpdateUserProfile(
        [Description("The field to update (LearningGoal, ExperienceLevel, KnownSkills, PreferredLearningStyles)")] string field,
        [Description("The new value for the field")] string value)
    {
        try
        {
            _userProfileService.UpdateProfile(field, value);
            return $"Successfully updated {field} to: {value}";
        }
        catch (Exception ex)
        {
            return $"Error updating user profile: {ex.Message}";
        }
    }

    [Description("Remove an item from a user profile list field")]
    public string RemoveFromUserProfile(
        [Description("The field to remove from (KnownSkills, PreferredLearningStyles)")] string field,
        [Description("The value to remove")] string value)
    {
        try
        {
            _userProfileService.RemoveFromProfile(field, value);
            return $"Successfully removed '{value}' from {field}";
        }
        catch (Exception ex)
        {
            return $"Error removing from user profile: {ex.Message}";
        }
    }

    [Description("Check if the user profile is complete and get profile summary")]
    public string CheckUserProfileStatus()
    {
        try
        {
            var isComplete = _userProfileService.IsProfileSufficient();
            var summary = GetUserProfileSummary();
            
            return $"Profile Complete: {isComplete}\n{summary}";
        }
        catch (Exception ex)
        {
            return $"Error checking user profile status: {ex.Message}";
        }
    }

    [Description("Get a summary of the current user profile")]
    public string GetUserProfileSummary()
    {
        try
        {
            return _userProfileService.GetProfileSummary();
        }
        catch (Exception ex)
        {
            return $"Error retrieving user profile: {ex.Message}";
        }
    }

    [Description("Add a skill to the user's known skills")]
    public string AddKnownSkill(
        [Description("The skill to add to known skills")] string skill)
    {
        try
        {
            _userProfileService.UpdateProfile("KnownSkills", skill);
            return $"Added skill: {skill}";
        }
        catch (Exception ex)
        {
            return $"Error adding skill: {ex.Message}";
        }
    }

    [Description("Add a preferred learning style")]
    public string AddPreferredLearningStyle(
        [Description("The learning style to add (Visual, Auditory, Kinesthetic, Reading/Writing)")] string learningStyle)
    {
        try
        {
            _userProfileService.UpdateProfile("PreferredLearningStyles", learningStyle);
            return $"Added preferred learning style: {learningStyle}";
        }
        catch (Exception ex)
        {
            return $"Error adding learning style: {ex.Message}";
        }
    }

    [Description("Set the user's learning goal")]
    public string SetLearningGoal(
        [Description("The learning goal to set")] string goal)
    {
        try
        {
            _userProfileService.UpdateProfile("LearningGoal", goal);
            return $"Set learning goal: {goal}";
        }
        catch (Exception ex)
        {
            return $"Error setting learning goal: {ex.Message}";
        }
    }

    [Description("Set the user's experience level")]
    public string SetExperienceLevel(
        [Description("The experience level (Beginner, Intermediate, Advanced)")] string experienceLevel)
    {
        try
        {
            _userProfileService.UpdateProfile("ExperienceLevel", experienceLevel);
            return $"Set experience level: {experienceLevel}";
        }
        catch (Exception ex)
        {
            return $"Error setting experience level: {ex.Message}";
        }
    }
}