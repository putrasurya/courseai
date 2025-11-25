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
    public class RefinementAgent : IRefinementAgent
    {
        public AgentThread? Thread { get; set; }
        public AIAgent Agent { get; init; }
        public string Name => "RefinementAgent";
        public string Description => "Processes feedback and improves existing roadmaps based on user input";
        public string SystemMessage => "You are a refinement agent responsible for updating and modifying a roadmap based on user feedback.";
        public IList<AITool> Tools { get; } = [];

        private readonly LearningProfileService _learningProfileService;
        private readonly IAgentStatusService _statusService;

        public RefinementAgent(InstrumentChatClient instrumentChatClient, LearningProfileService learningProfileService,
            IAgentStatusService statusService)
        {
            _learningProfileService = learningProfileService;
            _statusService = statusService;
            
            Agent = new ChatClientAgent(
                instrumentChatClient.ChatClient,
                name: Name,
                description: Description,
                instructions: SystemMessage)
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
            _statusService.SetStatus("RefinementAgent", "Refining your learning roadmap...");
            var response = await innerAgent.RunAsync(messages, thread, options, cancellationToken).ConfigureAwait(false);
            Console.WriteLine($"Agent Id: {innerAgent.Id}");
            Console.WriteLine($"Agent Name: {innerAgent.Name}");
            return response;
        }

        public async Task<string> Invoke(string input)
        {
            // In the future, this will use the Microsoft Agent Framework
            // to refine the roadmap based on user feedback.
            await Task.Delay(1000); // Simulate work.
            return $"Roadmap refined based on '{input}'.";
        }
    }
}

