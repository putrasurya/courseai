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
using PathWeaver.Helpers;
using PathWeaver.Middleware;

namespace PathWeaver.Agents
{
    public class OrchestratorAgent : IOrchestratorAgent
    {
        public AgentThread? Thread { get; set; }
        public AIAgent Agent { get; init; }
        public string Name => "OrchestratorAgent";
        public string Description => "Main orchestrator agent coordinating the learning roadmap creation process";
        public string SystemMessage => """
            You are the main orchestrator agent responsible for managing the entire learning roadmap creation process. You coordinate specialized agents to deliver a seamless learning experience.

            AVAILABLE TOOLS:
            - **PlannerAgent**: Gathers user information and builds complete UserProfile
            - **StructuringAgent**: Creates complete learning roadmaps with pedagogical frameworks and curated resources
            - **RefinementAgent**: Processes feedback and improves existing roadmaps
            - **CheckRoadmapStatus**: Check if a roadmap exists and get its summary

            WORKFLOW:
            1. **Profile Building**: Use PlannerAgent to gather complete user information
            2. **Roadmap Generation**: When profile is sufficient, use StructuringAgent to create complete roadmap
            3. **Roadmap Summary & Completion**: When StructuringAgent completes roadmap creation:
               - Use CheckRoadmapStatus to get roadmap details
               - Provide a clear summary of the generated roadmap to the user
               - Give user opportunity to request refinements
               - End with "ROADMAP_COMPLETE" keyword only if user is satisfied
            4. **Refinement**: Use RefinementAgent for improvements based on user feedback

            DECISION LOGIC:
            - If user profile is incomplete → Use PlannerAgent
            - If user profile is complete but no roadmap exists → Use StructuringAgent
            - If roadmap exists and user provides feedback → Use RefinementAgent
            - If roadmap creation is complete AND user is satisfied → Send "ROADMAP_COMPLETE" keyword

            CRITICAL COMPLETION PROCESS:
            When the StructuringAgent creates a roadmap:
            1. Present a comprehensive summary including:
               - Learning path overview
               - Number of modules and key topics
               - Estimated timeline
               - Main learning outcomes
            2. Ask if user wants any refinements or adjustments
            3. Only send "ROADMAP_COMPLETE" keyword after user confirms satisfaction

            GUIDELINES:
            - Use tools appropriately - let specialists handle their domains
            - Keep user informed of progress
            - Always summarize roadmap before completion signal
            - Handle errors gracefully
            - Provide smooth transitions between phases

            Your role is coordination and flow management, not doing the specialized work yourself.
            """;
        public IList<AITool> Tools { get; } = [];

        private readonly IPlannerAgent _plannerAgent;
        private readonly IStructuringAgent _structuringAgent;
        private readonly IRefinementAgent _refinementAgent;
        private readonly UserProfileService _userProfileService;
        private readonly RoadmapService _roadmapService;
        private readonly UserProfileToolsService _userProfileToolsService;
        private readonly IAgentStatusService _statusService;

        public OrchestratorAgent(
            InstrumentChatClient instrumentChatClient,
            IPlannerAgent plannerAgent,
            IStructuringAgent structuringAgent,
            IRefinementAgent refinementAgent,
            UserProfileService userProfileService,
            RoadmapService roadmapService,
            UserProfileToolsService userProfileToolsService,
            IAgentStatusService statusService)
        {
            _plannerAgent = plannerAgent;
            _structuringAgent = structuringAgent;
            _refinementAgent = refinementAgent;
            _userProfileService = userProfileService;
            _roadmapService = roadmapService;
            _userProfileToolsService = userProfileToolsService;
            _statusService = statusService;

            var orchestratorTools = new List<AIFunction>
            {
                plannerAgent.Agent.AsAIFunction(),
                structuringAgent.Agent.AsAIFunction(),
                refinementAgent.Agent.AsAIFunction(),
                AIFunctionFactory.Create(CheckRoadmapStatus)
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
                _statusService.SetStatus("OrchestratorAgent", "Coordinating your learning journey...");
                
                // Let the AI Agent handle all orchestration decisions
                var response = await Agent.RunAsync(input, Thread);
                return response.ToString();
            }
            catch (Exception ex)
            {
                return $"I encountered an error while processing your request: {ex.Message}. Please try again.";
            }
            finally
            {
                _statusService.ClearStatus();
            }
        }

        public async IAsyncEnumerable<string> InvokeStreaming(string input)
        {
            await foreach (var chunk in Agent.RunStreamingAsync(input, Thread))
            {
                if (!string.IsNullOrEmpty(chunk.ToString()))
                {
                    yield return chunk.ToString();
                }
            }
        }

        public Task<string> StartPlanning(string learningGoal)
        {
            Thread = Agent.GetNewThread();
            return Invoke(learningGoal);
        }

        public IAsyncEnumerable<string> StartPlanningStreaming(string learningGoal)
        {
            Thread = Agent.GetNewThread();
            return InvokeStreaming(learningGoal);
        }

        // AI Functions for the orchestrator to use
        [Description("Check if a roadmap exists for the current user")]
        public string CheckRoadmapStatus()
        {
            var roadmap = _roadmapService.GetRoadMap();
            if (roadmap == null)
            {
                return "No roadmap exists currently.";
            }
            
            return $"Roadmap exists: Created {roadmap.CreatedDate:yyyy-MM-dd}, Status: {roadmap.Status}, Modules: {roadmap.Modules?.Count ?? 0}";
        }
    }
}

