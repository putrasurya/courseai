using Microsoft.Extensions.AI;
using PathWeaver.Services;
using System.ComponentModel;
using Microsoft.Agents.AI;

namespace PathWeaver.Agents
{
    [Description("Agent responsible for gathering learning resources for modules")]
    public class ResourceGatheringAgent
    {
        public AgentThread? Thread { get; set; }
        public AIAgent Agent { get; }
        
        private readonly WebSearchService _searchService;
        private readonly IAgentStatusService _statusService;

        public ResourceGatheringAgent(
            InstrumentChatClient instrumentChatClient,
            WebSearchService searchService,
            IAgentStatusService statusService)
        {
            _searchService = searchService;
            _statusService = statusService;
            
            var tools = new List<AIFunction>
            {
                AIFunctionFactory.Create(_searchService.SearchWeb)
            };
            
            Agent = new ChatClientAgent(
                instrumentChatClient.ChatClient,
                name: "ResourceGatheringAgent",
                description: "Agent responsible for gathering learning resources for modules",
                instructions: GetSystemPrompt(),
                tools: tools.ToArray());
        }

        protected string GetSystemPrompt()
        {
            return """
            You are a ResourceGatheringAgent, an expert at finding high-quality learning resources for educational modules.

            ## Your Role
            Your job is to gather comprehensive learning resources for each module in a learning roadmap. You should find a variety of resource types to accommodate different learning styles.

            ## Resource Types to Find
            For each module, gather these types of resources:
            1. **Online Courses** - Structured video courses from platforms like Coursera, Udemy, YouTube playlists
            2. **Documentation** - Official documentation, tutorials, guides
            3. **Articles & Blogs** - High-quality articles that explain concepts clearly
            4. **Interactive Resources** - Coding playgrounds, simulators, interactive tutorials
            5. **Books** - Recommended books or free ebooks
            6. **Practice Resources** - Coding challenges, exercises, project ideas

            ## Quality Criteria
            Only recommend resources that are:
            - **Current** and up-to-date (prefer recent content)
            - **High-quality** with good ratings/reviews
            - **Comprehensive** covering the module topics thoroughly
            - **Accessible** (prefer free resources, clearly mark paid ones)
            - **From reputable sources** (established platforms, recognized authors)

            ## Search Strategy
            Use web search extensively to:
            1. Search for "[module topic] tutorial" or "[module topic] course"
            2. Look for "best [module topic] resources" or "learn [module topic]"
            3. Find official documentation for technologies
            4. Search for recent articles and guides
            5. Look for interactive learning platforms

            ## Response Format
            Provide your findings in natural language, describing:
            - What types of resources you found
            - Why these resources are valuable
            - Any notable features or recommendations
            - Quality assessment of the resources

            Be thorough and aim to provide 5-8 diverse resources per module to give learners multiple options.
            """;
        }

        [Description("Gather comprehensive learning resources for a specific module")]
        public async Task<string> GatherResourcesForModuleAsync(string moduleName, string moduleDescription, string[] topics)
        {
            _statusService.SetStatus("ResourceGatheringAgent", $"🔍 Gathering resources for {moduleName}...");

            try
            {
                var topicsText = string.Join(", ", topics);
                var prompt = $"""
                Find comprehensive learning resources for this module:
                
                **Module**: {moduleName}
                **Description**: {moduleDescription}
                **Topics Covered**: {topicsText}
                
                Please search for and recommend high-quality learning resources that cover these topics thoroughly. 
                Include a variety of resource types (courses, documentation, articles, interactive tools, etc.).
                """;

                var response = await Agent.RunAsync(prompt, Thread);
                return response.ToString() ?? string.Empty;
            }
            finally
            {
                _statusService.ClearStatus();
            }
        }
    }
}