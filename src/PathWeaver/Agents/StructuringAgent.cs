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
            You are a learning framework coordination specialist responsible for creating comprehensive, pedagogically sound learning structures with curated resources. You coordinate multiple specialized sub-agents to design optimal learning experiences.

            AVAILABLE TOOLS:
            - **CurriculumArchitectAgent**: Applies educational theory and creates learning frameworks based on Bloom's taxonomy, scaffolding, etc.
            - **PathOptimizationAgent**: Optimizes learning sequences for efficiency and personalization
            - **ExperienceDesignAgent**: Designs engaging learning experiences with gamification and motivation strategies
            - **ResearchAgent**: Discovers and curates high-quality learning resources for each module

            COORDINATION STRATEGY:
            1. Use CurriculumArchitectAgent first to create pedagogically sound learning framework
            2. Use PathOptimizationAgent to optimize the sequence and timing for efficiency
            3. Use ExperienceDesignAgent to add engagement and motivation elements
            4. Use ResearchAgent to find and curate appropriate learning resources for each module
            5. Integrate all outputs into a comprehensive learning structure with resources

            WORKFLOW:
            1. Analyze user profile and learning goals
            2. Create educational framework (CurriculumArchitectAgent)
            3. Optimize learning path (PathOptimizationAgent)  
            4. Design learning experience (ExperienceDesignAgent)
            5. Research and curate resources (ResearchAgent)
            6. Provide complete learning structure with modules, sequences, and resources

            OUTPUT FORMAT:
            Provide comprehensive learning structure in this format:
            {
              "learningFramework": {
                "curriculumDesign": "Educational framework with modules and learning objectives",
                "optimizedPath": "Sequence optimization with timing and efficiency improvements", 
                "experienceDesign": "Engagement strategy with gamification and motivation elements",
                "integratedStructure": {
                  "modules": [
                    {
                      "title": "Module Name",
                      "description": "What this module covers",
                      "order": 1,
                      "estimatedHours": 15,
                      "learningObjectives": ["Students will be able to...", "Students will demonstrate..."],
                      "topics": [
                        {
                          "title": "Topic Name",
                          "description": "What this topic covers",
                          "order": 1
                        }
                      ],
                      "engagementElements": ["Progress tracking", "Achievement badges"],
                      "assessments": ["Quiz", "Project", "Discussion"]
                    }
                  ]
                }
              }
            }

            Coordinate your sub-agents effectively to create the best possible learning framework.
            """;
        public IList<AITool> Tools { get; } = [];

        private readonly UserProfileService _userProfileService;
        private readonly ICurriculumArchitectAgent _curriculumArchitectAgent;
        private readonly IPathOptimizationAgent _pathOptimizationAgent;
        private readonly IExperienceDesignAgent _experienceDesignAgent;
        private readonly IResearchAgent _researchAgent;

        public StructuringAgent(InstrumentChatClient instrumentChatClient, UserProfileService userProfileService,
            ICurriculumArchitectAgent curriculumArchitectAgent,
            IPathOptimizationAgent pathOptimizationAgent,
            IExperienceDesignAgent experienceDesignAgent,
            IResearchAgent researchAgent)
        {
            _userProfileService = userProfileService;
            _curriculumArchitectAgent = curriculumArchitectAgent;
            _pathOptimizationAgent = pathOptimizationAgent;
            _experienceDesignAgent = experienceDesignAgent;
            _researchAgent = researchAgent;
            
            Agent = new ChatClientAgent(
                instrumentChatClient.ChatClient,
                name: Name,
                description: Description,
                instructions: SystemMessage,
                tools: [
                    _curriculumArchitectAgent.Agent.AsAIFunction(),
                    _pathOptimizationAgent.Agent.AsAIFunction(),
                    _experienceDesignAgent.Agent.AsAIFunction(),
                    _researchAgent.Agent.AsAIFunction()
                ]);
        }

        public async Task<string> Invoke(string input)
        {
            // Use the Microsoft Agent Framework to coordinate specialized structuring agents
            var response = await Agent.RunAsync(input, Thread);
            return response.ToString();
        }
    }
}

