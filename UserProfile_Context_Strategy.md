# UserProfile Context Management in CourseAI

## Overview

The CourseAI system now uses a **hybrid approach** for managing UserProfile context:

1. **Agent Tools** - The PlannerAgent has specialized tools to build and maintain the UserProfile
2. **Context Passing** - The profile data is passed as context between agent interactions
3. **State Management** - Each agent maintains its relevant state internally

## How It Works

### 1. PlannerAgent with UserProfile Tools

The PlannerAgent now has three key tools:
- `updateUserProfile(field, value)` - Adds/updates profile information
- `removeFromUserProfile(field, value)` - Removes items from profile lists
- `getUserProfileSummary()` - Gets readable summary of current profile

### 2. Conversational Context

```csharp
public async Task<string> Invoke(string input)
{
    var contextualInput = string.IsNullOrEmpty(_currentUserProfileJson) 
        ? input 
        : $"Current User Profile Context: {UserProfileTool.GetUserProfileSummary(_currentUserProfileJson)}\n\nUser Input: {input}";
    
    var response = await Agent.RunAsync(contextualInput, Thread);
    return response.ToString();
}
```

### 3. Orchestrated Workflow

```csharp
public async Task<Roadmap> GenerateRoadmap(string learningGoal)
{
    // Phase 1: Gather user profile through planner agent
    await _plannerAgent.Invoke(learningGoal);
    var userProfile = _plannerAgent.GetCurrentUserProfile();
    
    // Phase 2: Research with profile context
    var researchInput = $"Learning Goal: {userProfile.LearningGoal}, Experience: {userProfile.ExperienceLevel}...";
    var researchResult = await _researchAgent.Invoke(researchInput);
    
    // Phase 3: Structure roadmap with full context
    var structuringInput = $"User Profile: {JsonSerializer.Serialize(userProfile)}\nResearch Results: {researchResult}";
    // ...
}
```

## Benefits of This Approach

1. **Incremental Building** - Profile is built gradually through conversation
2. **Context Awareness** - Each interaction has full context of what's been learned
3. **Tool-Based Updates** - AI can precisely update specific profile fields
4. **Type Safety** - Strong typing through UserProfile model
5. **Flexibility** - Can easily extend with more profile fields or validation

## Example Flow

1. User: "I want to learn React"
2. PlannerAgent calls `updateUserProfile("learningGoal", "React")`
3. PlannerAgent asks: "What's your JavaScript experience?"
4. User: "I'm a beginner"
5. PlannerAgent calls `updateUserProfile("experienceLevel", "beginner")`
6. And so on...

The profile is continuously maintained and passed as context to subsequent agents.