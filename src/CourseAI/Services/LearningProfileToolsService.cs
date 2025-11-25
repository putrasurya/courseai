using System.ComponentModel;
using CourseAI.Models;
using Microsoft.Extensions.AI;

namespace CourseAI.Services;

/// <summary>
/// Service that provides LearningProfile-related tools that can be shared across multiple agents
/// </summary>
public class LearningProfileToolsService
{
    private readonly LearningProfileService _learningProfileService;

    public LearningProfileToolsService(LearningProfileService learningProfileService)
    {
        _learningProfileService = learningProfileService;
    }

    /// <summary>
    /// Gets all LearningProfile tools as AI functions
    /// </summary>
    public AIFunction[] GetAllTools()
    {
        return new[]
        {
            AIFunctionFactory.Create(UpdateLearningProfile),
            AIFunctionFactory.Create(RemoveFromLearningProfile),
            AIFunctionFactory.Create(GetLearningProfileSummary),
            AIFunctionFactory.Create(CheckLearningProfileStatus),
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
            AIFunctionFactory.Create(GetLearningProfileSummary),
            AIFunctionFactory.Create(CheckLearningProfileStatus)
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
            AIFunctionFactory.Create(GetLearningProfileSummary),
            AIFunctionFactory.Create(CheckLearningProfileStatus),
            AIFunctionFactory.Create(UpdateLearningProfile)
        };
    }

    [Description("Update a specific field in the learning profile with new information")]
    public string UpdateLearningProfile(
        [Description("The field to update (LearningGoal, ExperienceLevel, KnownSkills, PreferredLearningStyles)")] string field,
        [Description("The new value for the field")] string value)
    {
        try
        {
            _learningProfileService.UpdateProfile(field, value);
            return $"Successfully updated {field} to: {value}";
        }
        catch (Exception ex)
        {
            return $"Error updating learning profile: {ex.Message}";
        }
    }

    [Description("Remove an item from a learning profile list field")]
    public string RemoveFromLearningProfile(
        [Description("The field to remove from (KnownSkills, PreferredLearningStyles)")] string field,
        [Description("The value to remove")] string value)
    {
        try
        {
            _learningProfileService.RemoveFromProfile(field, value);
            return $"Successfully removed '{value}' from {field}";
        }
        catch (Exception ex)
        {
            return $"Error removing from learning profile: {ex.Message}";
        }
    }

    [Description("Check if the learning profile is complete and get profile summary")]
    public string CheckLearningProfileStatus()
    {
        try
        {
            var isComplete = _learningProfileService.IsProfileSufficient();
            var summary = GetLearningProfileSummary();
            
            return $"Profile Complete: {isComplete}\n{summary}";
        }
        catch (Exception ex)
        {
            return $"Error checking learning profile status: {ex.Message}";
        }
    }

    [Description("Get a summary of the current learning profile")]
    public string GetLearningProfileSummary()
    {
        try
        {
            return _learningProfileService.GetProfileSummary();
        }
        catch (Exception ex)
        {
            return $"Error retrieving learning profile: {ex.Message}";
        }
    }

    [Description("Add a skill to the learner's known skills")]
    public string AddKnownSkill(
        [Description("The skill to add to known skills")] string skill)
    {
        try
        {
            _learningProfileService.UpdateProfile("KnownSkills", skill);
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
            _learningProfileService.UpdateProfile("PreferredLearningStyles", learningStyle);
            return $"Added preferred learning style: {learningStyle}";
        }
        catch (Exception ex)
        {
            return $"Error adding learning style: {ex.Message}";
        }
    }

    [Description("Set the learner's learning goal")]
    public string SetLearningGoal(
        [Description("The learning goal to set")] string goal)
    {
        try
        {
            _learningProfileService.UpdateProfile("LearningGoal", goal);
            return $"Set learning goal: {goal}";
        }
        catch (Exception ex)
        {
            return $"Error setting learning goal: {ex.Message}";
        }
    }

    [Description("Set the learner's experience level")]
    public string SetExperienceLevel(
        [Description("The experience level (Beginner, Intermediate, Advanced)")] string experienceLevel)
    {
        try
        {
            _learningProfileService.UpdateProfile("ExperienceLevel", experienceLevel);
            return $"Set experience level: {experienceLevel}";
        }
        catch (Exception ex)
        {
            return $"Error setting experience level: {ex.Message}";
        }
    }
}