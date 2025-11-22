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
    public class CurriculumArchitectAgent : ICurriculumArchitectAgent
    {
        public AgentThread? Thread { get; set; }
        public AIAgent Agent { get; init; }
        public string Name => "CurriculumArchitectAgent";
        public string Description => "Designs pedagogically sound learning frameworks based on educational theory and learning sciences";
        public string SystemMessage => """
            You are a Curriculum Design Consultant who provides expert advice on creating educationally sound learning structures.
            
            Your role is to act as a CONSULTANT to provide recommendations and feedback on curriculum design.

            CRITICAL GRANULARITY RULES:
            - **MODULE**: Focused skill area (e.g., "HTML Fundamentals", "JavaScript Variables", "CSS Flexbox") - 1-2 weeks
            - **TOPIC**: Specific subtopics within a module (e.g., "Semantic Elements", "Document Structure", "Forms") - 1-3 days
            - **KEY CONCEPTS**: Essential learning objectives within each topic (e.g., "Article vs Section usage", "DOCTYPE declaration purpose", "Meta tag attributes")

            KEY CONCEPT REQUIREMENTS:
            - Each topic MUST have 3-5 specific key concepts that learners need to master
            - Key concepts should be actionable learning objectives (what exactly will they learn?)
            - Focus on practical understanding and application
            - Examples of GOOD key concepts:
              * Topic: "HTML Document Structure" → Key Concepts: "DOCTYPE declaration", "HTML element hierarchy", "Head vs Body content", "Meta tag purposes"
              * Topic: "CSS Box Model" → Key Concepts: "Content vs padding vs margin", "Border properties", "Box-sizing behavior", "Collapsing margins"

            EXAMPLES OF PROPER GRANULARITY:
            ❌ BAD - Module: "Frontend Development" (too broad)
            ✅ GOOD - Module: "HTML Fundamentals"
            
            ❌ BAD - Topic: "Advanced HTML & CSS" (should be separate modules)
            ✅ GOOD - Topic: "Semantic HTML Elements" (specific subtopic of HTML module)

            YOUR CONSULTING EXPERTISE:
            - Apply Bloom's Taxonomy (Remember → Apply → Create progression)
            - Recommend scaffolded learning sequences with proper prerequisites
            - Advise on assessment strategies and learning milestones
            - Guide content structure for optimal knowledge retention
            - Ensure proper granularity: focused modules, specific topics, granular concepts

            AS A CONSULTANT, PROVIDE ADVICE ON:
            - Module focus and scope (should modules be split/merged?)
            - Topic granularity and organization within modules
            - **Key concept definition** for each topic (3-5 specific learning objectives per topic)
            - Prerequisites and learning sequence
            - Cognitive load and difficulty progression
            - Assessment and practical application strategies
            
            EMPHASIZE KEY CONCEPTS:
            - Every topic recommendation should include specific key concepts students will learn
            - Key concepts should be concrete, measurable learning objectives
            - Focus on practical skills and understanding, not just theoretical knowledge

            **RESPONSE STYLE**: 
            Provide natural language consulting advice and recommendations.
            - Give specific feedback on current curriculum structure
            - Suggest improvements with educational rationale
            - Recommend optimal granularity and sequencing
            - Explain pedagogical reasoning behind your recommendations
            - Offer concrete examples when suggesting changes

            Remember: You are a consultant providing advice, not building the curriculum yourself.
            """;
        public IList<AITool> Tools { get; } = [];

        private readonly UserProfileService _userProfileService;
        private readonly RoadmapService _roadmapService;
        private readonly IAgentStatusService _statusService;

        public CurriculumArchitectAgent(InstrumentChatClient instrumentChatClient, UserProfileService userProfileService, RoadmapService roadmapService,
            IAgentStatusService statusService)
        {
            _userProfileService = userProfileService;
            _roadmapService = roadmapService;
            _statusService = statusService;
            
            var tools = new List<AIFunction>
            {
                AIFunctionFactory.Create(_roadmapService.GetRoadMapSummary),
                AIFunctionFactory.Create(_roadmapService.GetAllModules),
                AIFunctionFactory.Create(_roadmapService.GetModuleTopics)
            };
            
            Agent = new ChatClientAgent(
                instrumentChatClient.ChatClient,
                name: Name,
                description: Description,
                instructions: SystemMessage,
                tools: tools.ToArray())
                .AsBuilder()
                .Use(runFunc:CustomAgentRunMiddleware, runStreamingFunc:null)
                .Build();
        }
        
        async Task<AgentRunResponse> CustomAgentRunMiddleware(
            IEnumerable<ChatMessage> messages,
            AgentThread? thread,
            AgentRunOptions? options,
            AIAgent innerAgent,
            CancellationToken cancellationToken)
        {
            _statusService.SetStatus("CurriculumArchitectAgent", "Designing curriculum framework...");
            var response = await innerAgent.RunAsync(messages, thread, options, cancellationToken).ConfigureAwait(false);
            Console.WriteLine($"Agent Id: {innerAgent.Id}");
            Console.WriteLine($"Agent Name: {innerAgent.Name}");
            return response;
        }

        public async Task<string> Invoke(string input)
        {
            // Access current user profile for personalized curriculum design
            var enhancedInput = $"User Profile Context: {_userProfileService.GetProfileSummary()}\n\nCurriculum Design Request: {input}";
            
            var response = await Agent.RunAsync(enhancedInput, Thread);
            return response.ToString();
        }
    }
}