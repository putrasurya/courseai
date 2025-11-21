using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using PathWeaver.Models;
using PathWeaver.Services;
using Azure.Identity;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ComponentModel;
using PathWeaver.Agents.Interfaces;

namespace PathWeaver.Agents
{
    public class OrchestratorAgent : IOrchestratorAgent
    {
        public AgentThread? Thread { get; set; }
        public AIAgent Agent { get; init; }
        public string Name => "OrchestratorAgent";
        public string Description => "Main orchestrator agent coordinating the learning roadmap creation process";
        public string SystemMessage => """
            You are the main orchestrator agent responsible for managing the entire learning roadmap creation process. You have access to specialized agents as tools and must coordinate them intelligently.

            AVAILABLE TOOLS:
            - **PlannerAgent**: Gathers user information and builds complete UserProfile
            - **StructuringAgent**: Creates pedagogically sound learning frameworks with curated resources (includes research capabilities)
            - **RefinementAgent**: Processes feedback and improves existing roadmaps

            WORKFLOW PHASES:
            1. **Profile Building**: Use PlannerAgent to gather complete user information
            2. **Roadmap Generation**: Use StructuringAgent to create comprehensive roadmap with framework and resources
            3. **Roadmap Refinement**: Use RefinementAgent for improvements based on user feedback

            DECISION LOGIC:
            - If user profile is incomplete: Use PlannerAgent to continue gathering information
            - If user profile is complete but no roadmap exists: Use StructuringAgent to generate complete roadmap
            - If roadmap exists and user provides feedback: Use RefinementAgent to improve roadmap
            - If roadmap exists and user asks general questions: Answer directly or use appropriate agent

            AUTOMATIC WORKFLOW:
            1. Start with PlannerAgent for profile building
            2. When profile is sufficient, automatically proceed to roadmap generation
            3. Use StructuringAgent to create complete learning framework with resources
            4. Store complete roadmap and notify user
            5. Handle refinement requests with RefinementAgent

            IMPORTANT GUIDELINES:
            - Always check if UserProfile is sufficient before generating roadmap
            - Use tools appropriately - don't try to do their specialized work yourself
            - Provide smooth transitions between phases
            - Keep user informed of progress
            - Handle errors gracefully and guide user to next steps

            CONTEXT AWARENESS:
            - Track conversation state and user progress
            - Remember what information has been gathered
            - Know when to transition between agents
            - Provide personalized responses based on user's profile and progress

            Your role is to coordinate these specialized agents seamlessly to create the best possible learning experience for the user.
            """;
        public IList<AITool> Tools { get; } = [];

        private readonly IPlannerAgent _plannerAgent;
        private readonly IStructuringAgent _structuringAgent;
        private readonly IRefinementAgent _refinementAgent;
        private readonly UserProfileService _userProfileService;
        private readonly RoadmapStateService _roadmapStateService;
        private readonly UserProfileToolsService _userProfileToolsService;

        public OrchestratorAgent(
            InstrumentChatClient instrumentChatClient,
            IPlannerAgent plannerAgent,
            IStructuringAgent structuringAgent,
            IRefinementAgent refinementAgent,
            UserProfileService userProfileService,
            RoadmapStateService roadmapStateService,
            UserProfileToolsService userProfileToolsService)
        {
            _plannerAgent = plannerAgent;
            _structuringAgent = structuringAgent;
            _refinementAgent = refinementAgent;
            _userProfileService = userProfileService;
            _roadmapStateService = roadmapStateService;
            _userProfileToolsService = userProfileToolsService;

            var orchestratorTools = new List<AIFunction>
            {
                plannerAgent.Agent.AsAIFunction(),
                structuringAgent.Agent.AsAIFunction(),
                refinementAgent.Agent.AsAIFunction(),
                AIFunctionFactory.Create(CheckRoadmapStatus),
                AIFunctionFactory.Create(StoreGeneratedRoadmap)
            };
            
            // Add UserProfile tools (minimal - only summary and status check)
            orchestratorTools.AddRange(_userProfileToolsService.GetOrchestratorTools());

            Agent = new ChatClientAgent(
                instrumentChatClient.ChatClient,
                name: Name,
                description: Description,
                instructions: SystemMessage,
                tools: orchestratorTools.ToArray());
        }

        public async Task<string> Invoke(string input)
        {
            try
            {
                // Let the AI Agent handle all orchestration decisions
                var response = await Agent.RunAsync(input, Thread);
                return response.ToString();
            }
            catch (Exception ex)
            {
                return $"I encountered an error while processing your request: {ex.Message}. Please try again.";
            }
        }

        public Task<string> StartPlanning(string learningGoal)
        {
            Thread = Agent.GetNewThread();
            return Invoke(learningGoal);
        }

        // AI Functions for the orchestrator to use
        [Description("Check if a roadmap exists for the current user")]
        public string CheckRoadmapStatus()
        {
            var roadmap = _roadmapStateService.CurrentRoadmap;
            if (roadmap == null)
            {
                return "No roadmap exists currently.";
            }
            
            return $"Roadmap exists: Created {roadmap.CreatedDate:yyyy-MM-dd}, Status: {roadmap.Status}, Modules: {roadmap.Modules?.Count ?? 0}";
        }

        [Description("Store a newly generated roadmap from structured framework and research results")]
        public string StoreGeneratedRoadmap(string structuredFramework, string researchResults)
        {
            try
            {
                var userProfile = _userProfileService.GetProfileCopy();
                
                var roadmap = new Roadmap
                {
                    Id = Guid.NewGuid(),
                    UserProfile = userProfile,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    Status = RoadmapStatus.Draft,
                    // TODO: Parse structured framework and research results into modules
                    Modules = new List<RoadmapModule>()
                };

                _roadmapStateService.SetRoadmap(roadmap);
                
                return "✅ Roadmap successfully generated and stored! The user can now view their personalized learning roadmap.";
            }
            catch (Exception ex)
            {
                return $"❌ Error storing roadmap: {ex.Message}";
            }
        }
    }
}

