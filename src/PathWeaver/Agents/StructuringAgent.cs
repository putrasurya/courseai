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
        public string SystemMessage => "You are a structuring agent responsible for organizing learning resources into a logical roadmap with modules and topics.";
        public IList<AITool> Tools { get; } = [];

        private readonly UserProfileService _userProfileService;

        public StructuringAgent(IOptions<AzureOpenAIOptions> options, UserProfileService userProfileService)
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
            // to structure the roadmap.
            await Task.Delay(1000); // Simulate work.
            return $"Roadmap structured for '{input}'.";
        }
    }
}

