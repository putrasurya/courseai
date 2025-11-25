using CourseAI.Agents.Interfaces;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using CourseAI.Models;
using CourseAI.Services;
using Azure.Identity;
using Microsoft.Extensions.AI;
using OpenAI;

namespace CourseAI.Agents
{
    public class PathOptimizationAgent : IPathOptimizationAgent
    {
        public AgentThread? Thread { get; set; }
        public AIAgent Agent { get; init; }
        public string Name => "PathOptimizationAgent";
        public string Description => "Optimizes learning sequences to maximize outcomes while minimizing time and cognitive load";
        public string SystemMessage => """
            You are a Learning Path Optimization Consultant who provides expert advice on creating efficient, personalized learning sequences.
            
            Your role is to act as a CONSULTANT to provide optimization recommendations and feedback.

            IMPORTANT GRANULARITY UNDERSTANDING:
            - **MODULES**: Focused skill areas (e.g., "HTML Fundamentals", "CSS Flexbox") - these are what you optimize
            - **TOPICS**: Specific subtopics within modules (e.g., "Semantic Elements") - ensure logical topic flow within modules  
            - **KEY CONCEPTS**: Essential learning objectives within topics - while you don't optimize at this level, ensure topics have meaningful concepts

            CONCEPT QUALITY AWARENESS:
            - Recognize when topics lack specific key concepts and recommend this be addressed
            - Good topics should have 3-5 concrete learning objectives (key concepts)
            - Flag vague or overly broad topics that need more granular concept breakdown

            YOUR CONSULTING EXPERTISE:
            - Optimize focused module sequences based on dependencies
            - Recommend personalized paths based on learning styles, time constraints, and skill gaps
            - Estimate realistic timeframes for focused modules
            - Balance depth vs breadth based on goals and available time
            - Ensure topics within modules follow logical progression

            OPTIMIZATION STRATEGIES TO ADVISE ON:
            - **Dependency Analysis**: Recommend module order based on prerequisite relationships
            - **Parallel Learning**: Identify modules that can be studied concurrently
            - **Time Estimation**: Suggest duration adjustments based on learner experience
            - **Critical Path**: Advise on essential modules when time is constrained
            - **Progressive Difficulty**: Ensure gradual complexity increase across modules

            AS A CONSULTANT, PROVIDE ADVICE ON:
            - Module sequencing and dependencies
            - Time allocation and pacing recommendations  
            - Parallel learning opportunities
            - Adaptations for different time constraints or skill levels
            - Weekly commitment and study schedule optimization
            - **Key concept completeness** - flag topics that lack specific learning objectives

            **RESPONSE STYLE**:
            Provide natural language optimization advice and recommendations.
            - Analyze current learning path structure
            - Suggest sequence improvements with rationale
            - Recommend time estimates and pacing strategies
            - Offer alternative paths for different constraints
            - Explain optimization reasoning clearly

            Remember: You are a consultant providing optimization advice, not implementing the changes yourself.
            """;
        public IList<AITool> Tools { get; } = [];

        private readonly LearningProfileService _learningProfileService;
        private readonly RoadmapService _roadmapService;
        private readonly IAgentStatusService _statusService;

        public PathOptimizationAgent(InstrumentChatClient instrumentChatClient, LearningProfileService learningProfileService, RoadmapService roadmapService,
            IAgentStatusService statusService)
        {
            _learningProfileService = learningProfileService;
            _roadmapService = roadmapService;
            _statusService = statusService;
            
            var tools = new List<AIFunction>
            {
                AIFunctionFactory.Create(_roadmapService.GetRoadMapSummary),
                AIFunctionFactory.Create(_roadmapService.GetAllModules),
                AIFunctionFactory.Create(_roadmapService.UpdateModule),
                AIFunctionFactory.Create(_roadmapService.GetRoadMapAnalysis)
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
            _statusService.SetStatus("PathOptimizationAgent", "Optimizing learning path...");
            var response = await innerAgent.RunAsync(messages, thread, options, cancellationToken).ConfigureAwait(false);
            Console.WriteLine($"Agent Id: {innerAgent.Id}");
            Console.WriteLine($"Agent Name: {innerAgent.Name}");
            return response;
        }

        public async Task<string> Invoke(string input)
        {
            // Access current learning profile for personalized path optimization
            var enhancedInput = $"Learning Profile Context: {_learningProfileService.GetProfileSummary()}\n\nPath Optimization Request: {input}";
            
            var response = await Agent.RunAsync(enhancedInput, Thread);
            return response.ToString();
        }
    }
}