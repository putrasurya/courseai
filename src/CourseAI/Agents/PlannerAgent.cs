using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using CourseAI.Models;
using CourseAI.Services;
using Azure.Identity;
using OpenAI;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using CourseAI.Agents.Interfaces;

namespace CourseAI.Agents
{
    public class PlannerAgent : IPlannerAgent
    {
        public AgentThread? Thread { get; set; }
        public AIAgent Agent { get; init; }
        public string Name => "PlannerAgent";
        public string Description => "Gathers user information and builds complete LearningProfile through conversation";
        public string SystemMessage => """
            You are a planner agent specializing in user interaction and goal elicitation.
            Your primary role is to engage in a conversation with the user to deeply understand their learning objectives, existing knowledge, and preferences.
            
            You have access to tools to build and update a LearningProfile:
            - updateLearningProfile: Add or update information in the learning profile
            - removeFromLearningProfile: Remove specific items from lists in the learning profile
            - getLearningProfileSummary: Get a readable summary of the current learning profile
            
            Fields in LearningProfile object is: ExperienceLevel,KnownSkills,LearningGoal,PreferredLearningStyles

            Process:
            1. Start by asking about their main learning goal
            2. Gradually gather information about their experience level, known skills, and learning preferences
            3. Use the tools to incrementally build their profile
            4. Ask clarifying questions based on what you learn
            5. When you have sufficient information, provide a final summary

            Always maintain context of what you've already learned and avoid asking the same questions repeatedly.
            """;

        public IList<AITool> Tools { get; } = [];

        private readonly LearningProfileService _learningProfileService;
        private readonly LearningProfileToolsService _learningProfileToolsService;
        private readonly IAgentStatusService _statusService;

        public PlannerAgent(InstrumentChatClient instrumentChatClient, LearningProfileService learningProfileService, LearningProfileToolsService learningProfileToolsService,
            IAgentStatusService statusService)
        {
            _learningProfileService = learningProfileService;
            _learningProfileToolsService = learningProfileToolsService;
            _statusService = statusService;
            
            Agent = new ChatClientAgent(
                instrumentChatClient.ChatClient,
                name: Name,
                description: Description,
                instructions: SystemMessage,
                tools: _learningProfileToolsService.GetPlannerTools())
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
            var profileSummary = _learningProfileService.GetProfileSummary();
            var contextualInput = string.IsNullOrEmpty(profileSummary) || profileSummary == "No learning profile available." 
                ? input 
                : $"Current Learning Profile Context: {profileSummary}\n\nUser Input: {input}";
            
            var response = await Agent.RunAsync(contextualInput, Thread);
            return response.ToString();
        }

        public LearningProfile? GetCurrentLearningProfile()
        {
            return _learningProfileService.CurrentProfile;
        }

        // Tool functions that the AI can call are now handled by LearningProfileToolsService
    }
}
