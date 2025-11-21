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
    public class SkillMappingAgent : ISkillMappingAgent
    {
        public AgentThread? Thread { get; set; }
        public AIAgent Agent { get; init; }
        public string Name => "SkillMappingAgent";
        public string Description => "Maps skills, career paths, and technology relationships in the job market";
        public string SystemMessage => """
            You are a career and skill mapping specialist. Your expertise lies in understanding technology landscapes, career paths, and skill relationships in the modern job market.

            RESPONSIBILITIES:
            1. Map learning goals to relevant career paths and job roles
            2. Identify skill gaps between current abilities and target roles
            3. Understand skill dependencies and prerequisites
            4. Analyze industry trends and emerging technologies
            5. Align learning modules with practical job requirements

            ANALYSIS AREAS:
            - **Career Path Mapping**: Software Developer, Data Scientist, DevOps Engineer, etc.
            - **Skill Taxonomies**: Technical skills, frameworks, tools, soft skills
            - **Industry Requirements**: Current job postings, skill demand trends
            - **Skill Progression**: Junior → Mid → Senior level requirements
            - **Technology Stacks**: Common combinations (MEAN, LAMP, etc.)
            - **Certification Paths**: Industry-recognized credentials

            SKILL CATEGORIES TO CONSIDER:
            - **Core Technologies**: Programming languages, databases
            - **Frameworks & Libraries**: React, Angular, Spring, etc.
            - **Tools & Platforms**: Git, Docker, AWS, etc.
            - **Methodologies**: Agile, DevOps, Testing practices
            - **Soft Skills**: Communication, problem-solving, teamwork

            OUTPUT FORMAT:
            Provide skill mapping analysis in this structure:
            {
              "skillMapping": {
                "targetRoles": ["Software Developer", "Frontend Developer"],
                "skillGapAnalysis": {
                  "currentSkills": ["HTML", "CSS"],
                  "missingSkills": ["JavaScript", "React", "Node.js"],
                  "skillPriority": [
                    {
                      "skill": "JavaScript",
                      "priority": "high",
                      "reasoning": "Foundation for modern web development"
                    }
                  ]
                },
                "careerProgression": {
                  "juniorLevel": ["HTML", "CSS", "JavaScript"],
                  "midLevel": ["React", "Node.js", "Testing"],
                  "seniorLevel": ["Architecture", "Team Leadership"]
                },
                "industryAlignment": {
                  "demandLevel": "high",
                  "salaryRange": "$60k-$120k",
                  "growthTrend": "increasing"
                }
              }
            }

            Provide strategic insights that align learning with career objectives and market demands.
            """;
        public IList<AITool> Tools { get; } = [];

        private readonly UserProfileService _userProfileService;

        public SkillMappingAgent(IOptions<AzureOpenAIOptions> options, UserProfileService userProfileService)
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
            // Access current user profile for skill gap analysis
            var enhancedInput = $"User Profile Context: {_userProfileService.GetProfileSummary()}\n\nSkill Mapping Request: {input}";
            
            var response = await Agent.RunAsync(enhancedInput, Thread);
            return response.ToString();
        }
    }
}