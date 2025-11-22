using PathWeaver.Agents.Interfaces;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using PathWeaver.Models;
using PathWeaver.Services;
using Azure.Identity;
using Microsoft.Extensions.AI;
using OpenAI;

namespace PathWeaver.Agents
{
    public class StructuringAgent : IStructuringAgent
    {
        public AgentThread? Thread { get; set; }
        public AIAgent Agent { get; init; }
        public string Name => "StructuringAgent";
        public string Description => "Creates pedagogically sound learning frameworks using specialized design tools";
        public string SystemMessage => """
            You are a learning roadmap coordination specialist responsible for creating comprehensive, pedagogically sound learning roadmaps. You coordinate specialized sub-agents and use RoadMap tools to build complete learning experiences.

            CRITICAL GRANULARITY REQUIREMENTS:
            **MODULES**: Focused, specific skill areas (NOT broad domains)
            - ✅ Good: "HTML Fundamentals", "JavaScript Variables", "CSS Grid Layout"  
            - ❌ Bad: "Frontend Development", "Web Programming", "Advanced Topics"

            **TOPICS**: Specific subtopics within a module
            - ✅ Good: "Semantic Elements", "Form Validation", "Flexbox Properties"
            - ❌ Bad: "Advanced HTML", "Complete CSS", "JavaScript Basics" (these should be modules)

            **KEY CONCEPTS**: Essential learning objectives within topics (CRITICAL!)
            - ✅ Good: "Article vs Section usage", "Required attribute syntax", "Justify-content property values"
            - Must have 3-5 specific concepts per topic
            - Should be actionable learning objectives (what exactly will students master?)
            - Focus on practical application and concrete understanding

            AVAILABLE TOOLS:
            - **CurriculumArchitectAgent**: Applies educational theory and creates learning frameworks with proper granularity
            - **PathOptimizationAgent**: Optimizes learning sequences for efficiency and personalization
            - **ResearchAgent**: Finds and curates learning resources for each module using web search capabilities
            - **RoadMap Tools**: Create, update, and manage the roadmap structure with modules, topics, and concepts
            - **UserProfile Tools**: Access user context for personalization

            COORDINATION STRATEGY:
            1. Initialize new roadmap using user profile context
            2. Use CurriculumArchitectAgent to design focused modules and specific topics
            3. Use PathOptimizationAgent to optimize learning sequence and timing
            4. Use ResearchAgent to gather relevant learning resources for each module
            5. Build roadmap structure ensuring proper granularity (focused modules → specific topics → granular concepts)
            6. Finalize roadmap with proper status and validation

            WORKFLOW:
            1. Initialize roadmap with InitializeRoadMap using user profile summary
            2. Get curriculum design from CurriculumArchitectAgent (emphasize granularity requirements)
            3. Get path optimization from PathOptimizationAgent  
            4. Create FOCUSED modules using AddModule (e.g., "HTML Fundamentals", not "Frontend Development")
            5. Add SPECIFIC topics to modules using AddTopicToModule (e.g., "Semantic Elements", not "Advanced HTML")
            6. Add GRANULAR key concepts to topics using AddConceptToTopic - ENSURE 3-5 specific learning objectives per topic
            7. Use ResearchAgent to find and curate learning resources for each module (tutorials, documentation, exercises)
            8. Update roadmap status to 'InProgress' when complete
            9. Return roadmap summary and analysis

            KEY CONCEPT EMPHASIS:
            - Each topic must have meaningful key concepts (specific learning objectives)
            - Key concepts should answer: "What exactly will the student learn and be able to do?"
            - Examples: Instead of vague concepts, use specific ones like:
              * "DOCTYPE declaration syntax and purpose"
              * "Difference between article and section elements"  
              * "Form validation attribute usage"
              * "CSS Grid container vs item properties"

            INTEGRATION APPROACH:
            - Ensure modules are focused skill areas, not broad domains
            - Convert learning objectives to specific, learnable topics
            - **Break down topics into 3-5 granular, actionable key concepts that define exactly what students will learn**
            - Use ResearchAgent to find quality learning resources (tutorials, documentation, interactive exercises) for each module
            - Apply optimization timing to module duration estimates
            - Ensure proper ordering and dependencies
            - Key concepts must be concrete learning objectives (e.g., "Understand flexbox container properties" → "justify-content property values", "align-items behavior", "flex-direction options")

            RESOURCE GATHERING:
            - After creating each module, use ResearchAgent to find relevant learning resources
            - Focus on high-quality, current resources that align with the module's learning objectives
            - Include diverse resource types: tutorials, documentation, interactive exercises, video content
            - Ensure resources match the user's skill level and learning preferences

            Always use the RoadMap tools to build the actual roadmap structure with proper granularity.
            """;
        public IList<AITool> Tools { get; } = [];

        private readonly UserProfileService _userProfileService;
        private readonly UserProfileToolsService _userProfileToolsService;
        private readonly RoadmapService _roadmapService;
        private readonly ICurriculumArchitectAgent _curriculumArchitectAgent;
        private readonly IPathOptimizationAgent _pathOptimizationAgent;
        private readonly IAgentStatusService _statusService;
        private readonly IResearchAgent _researchAgent;

        public StructuringAgent(InstrumentChatClient instrumentChatClient, UserProfileService userProfileService,
            UserProfileToolsService userProfileToolsService, RoadmapService roadmapService,
            ICurriculumArchitectAgent curriculumArchitectAgent,
            IPathOptimizationAgent pathOptimizationAgent,
            IAgentStatusService statusService,
            IResearchAgent researchAgent)
        {
            _userProfileService = userProfileService;
            _userProfileToolsService = userProfileToolsService;
            _roadmapService = roadmapService;
            _curriculumArchitectAgent = curriculumArchitectAgent;
            _pathOptimizationAgent = pathOptimizationAgent;
            _researchAgent = researchAgent;
            _statusService = statusService;

            var tools = new List<AIFunction>
            {
                _curriculumArchitectAgent.Agent.AsAIFunction(),
                _pathOptimizationAgent.Agent.AsAIFunction(),
                _researchAgent.Agent.AsAIFunction()
            };
            
            // Add basic UserProfile tools (summary, status, basic updates)
            tools.AddRange(_userProfileToolsService.GetStructuringTools());
            
            // Add RoadMap tools for building the roadmap
            var roadMapToolsList = new List<AIFunction>
            {
                AIFunctionFactory.Create(_roadmapService.InitializeRoadMap),
                AIFunctionFactory.Create(_roadmapService.AddModule),
                AIFunctionFactory.Create(_roadmapService.AddTopicToModule),
                AIFunctionFactory.Create(_roadmapService.AddConceptToTopic),
                AIFunctionFactory.Create(_roadmapService.UpdateRoadMapStatus),
                AIFunctionFactory.Create(_roadmapService.GetRoadMapSummary),
                AIFunctionFactory.Create(_roadmapService.GetAllModules),
                AIFunctionFactory.Create(_roadmapService.GetRoadMapAnalysis)
            };
            tools.AddRange(roadMapToolsList);
            
            Agent = new ChatClientAgent(
                instrumentChatClient.ChatClient,
                name: Name,
                description: Description,
                instructions: SystemMessage,
                tools: tools.ToArray())
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
            _statusService.SetStatus("StructuringAgent", "Structuring your learning roadmap...");
            var response = await innerAgent.RunAsync(messages, thread, options, cancellationToken).ConfigureAwait(false);
            Console.WriteLine($"Agent Id: {innerAgent.Id}");
            Console.WriteLine($"Agent Name: {innerAgent.Name}");
            return response;
        }

        public async Task<string> Invoke(string input)
        {
            // Use the Microsoft Agent Framework to coordinate specialized structuring agents
            var response = await Agent.RunAsync(input, Thread);
            return response.ToString();
        }
    }
}

