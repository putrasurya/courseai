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
    public class ResearchAgent : IResearchAgent
    {
        public AgentThread? Thread { get; set; }
        public AIAgent Agent { get; init; }
        public string Name => "ResearchAgent";
        public string SystemMessage => "You are a research agent responsible for finding and curating learning resources based on a user's profile and learning goals.";
        public IList<AITool> Tools { get; } = [];

        private readonly UserProfileService _userProfileService;

        public ResearchAgent(IOptions<AzureOpenAIOptions> options, UserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
            
            var azureOptions = options.Value;
            Agent = new AzureOpenAIClient(
                new Uri(azureOptions.Endpoint),
                new DefaultAzureCredential())
                    .GetChatClient(azureOptions.ModelName)
                    .CreateAIAgent(instructions: SystemMessage);
        }

        public async Task<string> Invoke(string input)
        {
            // In the future, this will use the Microsoft Agent Framework
            // to search for resources based on the input.
            await Task.Delay(1000); // Simulate work.
            return $"Here are some resources for '{input}'.";
        }
    }
}

