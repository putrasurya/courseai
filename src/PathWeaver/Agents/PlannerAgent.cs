using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using PathWeaver.Models;
using PathWeaver.Services;
using Azure.Identity;
using OpenAI;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using PathWeaver.Agents.Interfaces;

namespace PathWeaver.Agents
{
    public class PlannerAgent : IPlannerAgent
    {
        public AgentThread? Thread { get; set; }
        public AIAgent Agent { get; init; }
        public string Name => "PlannerAgent";
        public string Description => "Gathers user information and builds complete UserProfile through conversation";
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

        public IList<AITool> Tools { get; } = [];

        private readonly UserProfileService _userProfileService;
        private readonly UserProfileToolsService _userProfileToolsService;
        private readonly IAgentStatusService _statusService;

        public PlannerAgent(InstrumentChatClient instrumentChatClient, UserProfileService userProfileService, UserProfileToolsService userProfileToolsService,
            IAgentStatusService statusService)
        {
            _userProfileService = userProfileService;
            _userProfileToolsService = userProfileToolsService;
            _statusService = statusService;
            
            Agent = new ChatClientAgent(
                instrumentChatClient.ChatClient,
                name: Name,
                description: Description,
                instructions: SystemMessage,
                tools: _userProfileToolsService.GetPlannerTools())
                .AsBuilder()
                .Use(runFunc:CustomAgentRunMiddleware, runStreamingFunc:null)
                .Build();;
        }
        
        async Task<AgentRunResponse> CustomAgentRunMiddleware(
            IEnumerable<ChatMessage> messages,
            AgentThread? thread,
            AgentRunOptions? options,
            AIAgent innerAgent,
            CancellationToken cancellationToken)
        {
            _statusService.SetStatus("PlannerAgent", "Gathering your learning profile...");
            await Task.Delay(1000);
            var response = await innerAgent.RunAsync(messages, thread, options, cancellationToken).ConfigureAwait(false);
            Console.WriteLine($"Agent Id: {innerAgent.Id}");
            Console.WriteLine($"Agent Name: {innerAgent.Name}");
            return response;
        }

        public async Task<string> Invoke(string input)
        {
            var profileSummary = _userProfileService.GetProfileSummary();
            var contextualInput = string.IsNullOrEmpty(profileSummary) || profileSummary == "No user profile available." 
                ? input 
                : $"Current User Profile Context: {profileSummary}\n\nUser Input: {input}";
            
            var response = await Agent.RunAsync(contextualInput, Thread);
            return response.ToString();
        }

        public UserProfile? GetCurrentUserProfile()
        {
            return _userProfileService.CurrentProfile;
        }

        // Tool functions that the AI can call are now handled by UserProfileToolsService
    }
}
