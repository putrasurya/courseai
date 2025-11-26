using Microsoft.Extensions.AI;
using CourseAI.Services;
using System.ComponentModel;
using Microsoft.Agents.AI;

namespace CourseAI.Agents
{
    [Description("Agent responsible for gathering learning resources for modules")]
    public class ResourceGatheringAgent
    {
        public AgentThread? Thread { get; set; }
        public AIAgent Agent { get; }
        
        private readonly WebSearchService _searchService;
        private readonly RoadmapService _roadmapService;
        private readonly IAgentStatusService _statusService;

        public ResourceGatheringAgent(
            InstrumentChatClient instrumentChatClient,
            WebSearchService searchService,
            RoadmapService roadmapService,
            IAgentStatusService statusService)
        {
            _searchService = searchService;
            _roadmapService = roadmapService;
            _statusService = statusService;
            
            var tools = new List<AIFunction>
            {
                AIFunctionFactory.Create(_searchService.SearchWeb),
                AIFunctionFactory.Create(_roadmapService.AddResourceToModule),
                AIFunctionFactory.Create(_roadmapService.GetModuleResources),
                AIFunctionFactory.Create(_roadmapService.ValidateModuleResourceQuality),
                AIFunctionFactory.Create(_roadmapService.ValidateAllResourceUrls),
                AIFunctionFactory.Create(_roadmapService.GetModulesWithoutResources)
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
            You are a ResourceGatheringAgent, an expert at finding high-quality learning resources with REAL URLs and adding them directly to the roadmap.

            ## MANDATORY WORKFLOW (Follow this EXACT sequence):
            
            **STEP 1: Search Web for Real Resources**
            - Use SearchWeb to find ACTUAL, SPECIFIC learning resources with working URLs
            - Search for "[module topic] course", "[module topic] tutorial", etc.
            - YOU MUST find real URLs, not generic descriptions!
            
            **STEP 2: Add Each Resource Immediately**
            - For EVERY resource you find, use AddResourceToModule with the ACTUAL URL
            - Resource types: Video, Documentation, Article, Tutorial, Book, Game
            - Include proper source (YouTube, Coursera, MDN, etc.)
            
            **STEP 3: Validate Quality (MANDATORY)**
            - Use ValidateModuleResourceQuality to check all resources have URLs
            - If validation fails, you MUST search for more resources with proper URLs
            - Continue until validation passes completely
            
            **STEP 4: Ensure Completeness**
            - Aim for 5-8 diverse, high-quality resources per module
            - Use ValidateAllResourceUrls for final verification

            ## CRITICAL REQUIREMENTS (NON-NEGOTIABLE):
            1. **EVERY resource MUST have a real URL** from web search
            2. **NO generic descriptions** or placeholder content allowed
            3. **You CANNOT finish** until ValidateModuleResourceQuality passes
            4. **Use web search extensively** - don't rely on memory
            5. **Add resources one by one** using AddResourceToModule
            6. **Prefer free resources**, mark paid ones as "(Paid)"

            ## Example Good Resource:
            - Title: "JavaScript Fundamentals Course"
            - URL: "https://www.coursera.org/learn/javascript-basics"
            - Type: Video
            - Source: "Coursera"
            - Description: "Comprehensive JavaScript course covering variables, functions, and DOM manipulation"

            ## FAILURE CONDITIONS (These mean you failed):
            - Any resource without a real URL
            - Generic descriptions like "Learn JavaScript basics"
            - Not using web search to find actual resources
            - Not validating resource quality before finishing
            - Providing fewer than 5 resources per module

            Your success is measured by ValidateModuleResourceQuality returning ✅ for every module!
            """;
        }

        [Description("Gather comprehensive learning resources for a specific module and add them directly to the roadmap")]
        public async Task<string> GatherResourcesForModuleAsync(string moduleName, string moduleDescription, string[] topics)
        {
            _statusService.SetProgress("ResourceGatheringAgent", $"🔍 Gathering resources for {moduleName}...", 85, "Searching for learning materials");

            try
            {
                var topicsText = string.Join(", ", topics);
                var prompt = $"""
                CRITICAL TASK: Find REAL learning resources with working URLs for this module:
                
                **Module**: {moduleName}
                **Description**: {moduleDescription}  
                **Topics**: {topicsText}
                
                MANDATORY STEPS (Do NOT skip any):
                
                1. **SEARCH THE WEB** (Required):
                   - Use SearchWeb multiple times to find ACTUAL resources
                   - Search for "{moduleName} course", "{moduleName} tutorial", "{moduleName} documentation"
                   - Search for specific topics: {topicsText}
                   - Find resources from real platforms (YouTube, Coursera, MDN, etc.)
                
                2. **ADD EACH RESOURCE** (Required):
                   - Use AddResourceToModule for EVERY resource you find
                   - Include the ACTUAL URL from your search results
                   - Add 5-8 diverse resources (videos, docs, articles, tutorials)
                
                3. **VALIDATE QUALITY** (Required):
                   - Use ValidateModuleResourceQuality to check all resources have URLs
                   - If validation fails, search for MORE resources with proper URLs
                   - Keep going until validation returns ✅
                
                4. **FINAL CHECK** (Required):
                   - Use ValidateAllResourceUrls to ensure all URLs work
                
                YOU CANNOT FINISH until ValidateModuleResourceQuality shows ✅ for this module!
                START by searching the web for real resources now.
                """;

                var response = await Agent.RunAsync(prompt, Thread);
                
                // Update progress to show resource gathering completion
                _statusService.SetProgress("ResourceGatheringAgent", $"✅ Resources gathered for {moduleName}", 95, "Resources found and validated");
                
                return response.ToString() ?? string.Empty;
            }
            finally
            {
                _statusService.ClearStatus();
            }
        }
    }
}