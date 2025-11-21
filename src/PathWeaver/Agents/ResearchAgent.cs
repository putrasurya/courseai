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
    public class ResearchAgent : IResearchAgent
    {
        public AgentThread? Thread { get; set; }
        public AIAgent Agent { get; init; }
        public string Name => "ResearchAgent";
        public string Description => "Discovers and curates learning resources using specialized research tools";
        public string SystemMessage => """
            You are a research coordination specialist responsible for finding and curating comprehensive learning resources. You coordinate multiple specialized sub-agents to provide thorough research coverage.

            AVAILABLE TOOLS:
            - **ContentDiscoveryAgent**: Discovers resources across multiple platforms (Coursera, Udemy, YouTube, GitHub, etc.)
            - **SkillMappingAgent**: Maps learning goals to career paths and analyzes skill gaps
            - **ResourceEvaluationAgent**: Evaluates resource quality, credibility, and learning effectiveness

            COORDINATION STRATEGY:
            1. Use SkillMappingAgent first to understand career context and skill requirements
            2. Use ContentDiscoveryAgent to find comprehensive resources for each learning module
            3. Use ResourceEvaluationAgent to assess quality and filter the best resources
            4. Integrate findings into cohesive module-level resource recommendations

            WORKFLOW:
            1. Analyze learning framework and user profile
            2. Map skills to career requirements (SkillMappingAgent)
            3. Discover content across platforms (ContentDiscoveryAgent) 
            4. Evaluate and filter resources (ResourceEvaluationAgent)
            5. Provide final curated resource recommendations per module

            OUTPUT FORMAT:
            Provide comprehensive research results in this structure:
            {
              "researchResults": {
                "skillAnalysis": "Career alignment and gap analysis",
                "moduleResources": [
                  {
                    "moduleTitle": "Module Name",
                    "curatedResources": [
                      {
                        "title": "Resource Title",
                        "type": "video|tutorial|documentation|book|project",
                        "url": "https://...",
                        "platform": "Coursera|YouTube|GitHub|etc",
                        "qualityScore": 8.5,
                        "recommendation": "highly-recommended|recommended",
                        "description": "Why this resource is excellent for this module",
                        "estimatedHours": 10
                      }
                    ]
                  }
                ]
              }
            }

            Coordinate your sub-agents effectively to provide the best possible learning resources.
            """;
        public IList<AITool> Tools { get; } = [];

        private readonly UserProfileService _userProfileService;
        private readonly IContentDiscoveryAgent _contentDiscoveryAgent;
        private readonly ISkillMappingAgent _skillMappingAgent;
        private readonly IResourceEvaluationAgent _resourceEvaluationAgent;

        public ResearchAgent(IOptions<AzureOpenAIOptions> options, UserProfileService userProfileService,
            IContentDiscoveryAgent contentDiscoveryAgent,
            ISkillMappingAgent skillMappingAgent, 
            IResourceEvaluationAgent resourceEvaluationAgent)
        {
            _userProfileService = userProfileService;
            _contentDiscoveryAgent = contentDiscoveryAgent;
            _skillMappingAgent = skillMappingAgent;
            _resourceEvaluationAgent = resourceEvaluationAgent;
            
            var azureOptions = options.Value;
            Agent = new AzureOpenAIClient(
                new Uri(azureOptions.Endpoint),
                new DefaultAzureCredential())
                    .GetChatClient(azureOptions.ModelName)
                    .CreateAIAgent(
                        name: Name,
                        instructions: SystemMessage,
                        tools: [
                            _contentDiscoveryAgent.Agent.AsAIFunction(),
                            _skillMappingAgent.Agent.AsAIFunction(),
                            _resourceEvaluationAgent.Agent.AsAIFunction()
                        ]
                    );
        }

        public async Task<string> Invoke(string input)
        {
            // Use the Microsoft Agent Framework to coordinate specialized research agents
            var response = await Agent.RunAsync(input, Thread);
            return response.ToString();
        }
    }
}

