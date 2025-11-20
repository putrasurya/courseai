using System.ComponentModel;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using PathWeaver.Models;
using Azure.Identity;
using OpenAI;
using System.Text.Json;
using Microsoft.Extensions.AI;

namespace PathWeaver.Agents
{
    public class PlannerAgent : IPlannerAgent
    {
        public AgentThread? Thread { get; set; }
        public AIAgent Agent { get; init; }
        public string Name => "PlannerAgent";
        public string SystemMessage => """
            You are a planner agent specializing in user interaction and goal elicitation.
            Your primary role is to engage in a conversation with the user to deeply understand their learning objectives, existing knowledge, and preferences.
            
            You have access to tools to build and update a UserProfile:
            - updateUserProfile: Add or update information in the user profile
            - removeFromUserProfile: Remove specific items from lists in the user profile
            - getUserProfileSummary: Get a readable summary of the current user profile
            
            Fields in UserProfile object is: ExperienceLevel,KnownSkills,LearningGoal,PreferredLearningStyles

            Process:
            1. Start by asking about their main learning goal
            2. Gradually gather information about their experience level, known skills, and learning preferences
            3. Use the tools to incrementally build their profile
            4. Ask clarifying questions based on what you learn
            5. When you have sufficient information, provide a final summary

            Always maintain context of what you've already learned and avoid asking the same questions repeatedly.
            """;

        public IList<AITool>? Tools { get; }

        private string _currentUserProfileJson = string.Empty;

        public PlannerAgent(IOptions<AzureOpenAIOptions> options)
        {
            Tools  =
            [
                AIFunctionFactory.Create(GetCurrentUserProfile),
                AIFunctionFactory.Create(UpdateUserProfile),
                AIFunctionFactory.Create(RemoveFromUserProfile),
            ];
            
            var azureOptions = options.Value;
            Agent = new AzureOpenAIClient(
                new Uri(azureOptions.Endpoint),
                new DefaultAzureCredential())
                    .GetChatClient(azureOptions.ModelName)
                    .CreateAIAgent(
                        name: Name,
                        instructions: SystemMessage,
                        tools: Tools
                    );
        }

        public async Task<string> Invoke(string input)
        {
            var contextualInput = string.IsNullOrEmpty(_currentUserProfileJson) 
                ? input 
                : $"Current User Profile Context: {UserProfileTool.GetUserProfileSummary(_currentUserProfileJson)}\n\nUser Input: {input}";
            
            var response = await Agent.RunAsync(contextualInput, Thread);
            return response.ToString();
        }

        [Description("Get the current user profile")]
        public UserProfile? GetCurrentUserProfile()
        {
            if (string.IsNullOrEmpty(_currentUserProfileJson))
                return null;
            
            return JsonSerializer.Deserialize<UserProfile>(_currentUserProfileJson);
        }

        [Description("Update a field in the user profile with a value")]
        private string UpdateUserProfile(string field, string value)
        {
            _currentUserProfileJson = UserProfileTool.UpdateUserProfile(_currentUserProfileJson, field, value);
            return $"Updated {field} with: {value}";
        }

        [Description("Remove an item from a user profile list field")]
        private string RemoveFromUserProfile(string field, string value)
        {
            _currentUserProfileJson = UserProfileTool.RemoveFromUserProfile(_currentUserProfileJson, field, value);
            return $"Removed {value} from {field}";
        }

        private string GetUserProfileSummary()
        {
            return UserProfileTool.GetUserProfileSummary(_currentUserProfileJson);
        }
    }
}
