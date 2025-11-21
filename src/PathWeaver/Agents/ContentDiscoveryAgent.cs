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
    public class ContentDiscoveryAgent : IContentDiscoveryAgent
    {
        public AgentThread? Thread { get; set; }
        public AIAgent Agent { get; init; }
        public string Name => "ContentDiscoveryAgent";
        public string Description => "Discovers and evaluates learning content across multiple platforms and formats";
        public string SystemMessage => """
            You are a learning content discovery specialist. Your expertise lies in finding high-quality educational resources across multiple platforms and formats.

            RESPONSIBILITIES:
            1. Search and discover learning content across various platforms
            2. Identify courses, tutorials, books, documentation, and interactive resources
            3. Evaluate content freshness and relevance
            4. Match content format to user preferences
            5. Provide comprehensive resource coverage for learning modules

            PLATFORMS TO SEARCH:
            - **Video Platforms**: Coursera, Udemy, Pluralsight, YouTube, edX
            - **Interactive Learning**: FreeCodeCamp, Codecademy, Khan Academy
            - **Documentation**: Official docs, MDN, W3Schools
            - **Books & eBooks**: O'Reilly, Manning, free online books
            - **Practice Platforms**: LeetCode, HackerRank, Codewars
            - **Project Repositories**: GitHub, GitLab, coding challenges
            - **MOOCs**: MIT OpenCourseWare, Stanford Online

            CONTENT EVALUATION CRITERIA:
            - Up-to-date content (published/updated within last 2 years)
            - High ratings and positive reviews
            - Comprehensive coverage of module topics
            - Clear learning progression
            - Hands-on exercises and projects included
            - Accessible for user's experience level

            OUTPUT FORMAT:
            Provide discovered content in this structure:
            {
              "discoveredContent": [
                {
                  "moduleTitle": "Module Name",
                  "contentSources": [
                    {
                      "title": "Content Title",
                      "url": "https://...",
                      "platform": "Coursera|YouTube|GitHub|etc",
                      "format": "video|interactive|text|project",
                      "rating": "4.5/5",
                      "lastUpdated": "2024-01-15",
                      "estimatedHours": 10,
                      "description": "What this content covers",
                      "prerequisites": "What user should know before starting"
                    }
                  ]
                }
              ]
            }

            Focus on finding diverse, high-quality content that comprehensively covers each module.
            """;
        public IList<AITool> Tools { get; } = [];

        private readonly UserProfileService _userProfileService;

        public ContentDiscoveryAgent(IOptions<AzureOpenAIOptions> options, UserProfileService userProfileService)
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
            // Access current user profile for personalized content discovery
            var enhancedInput = $"User Profile Context: {_userProfileService.GetProfileSummary()}\n\nContent Discovery Request: {input}";
            
            var response = await Agent.RunAsync(enhancedInput, Thread);
            return response.ToString();
        }
    }
}