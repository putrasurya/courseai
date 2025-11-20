using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using PathWeaver.Models;
using Azure.Identity;
using Microsoft.Extensions.AI;
using OpenAI;

namespace PathWeaver.Agents
{
    public class OrchestratorAgent : IOrchestratorAgent
    {
        public AgentThread? Thread { get; set; }
        public AIAgent Agent { get; init; }
        public string Name => "OrchestratorAgent";
        public string SystemMessage => """
            You are a top-level orchestrator agent managing the learning roadmap creation process.

            PHASE CHECKLIST: 
            [ ] User Profile Gathering
            [ ] Roadmap Research
            [ ] Roadmap Structuring
            [ ] Roadmap Refinement
            [ ] Finished
            
            Your job is to coordinate with the PlannerAgent to gather complete user information before proceeding to research and structuring phases.
            
            During the planning phase:
            1. Direct all user interactions through the PlannerAgent
            2. The PlannerAgent will ask questions to build a complete UserProfile
            3. Once the UserProfile is sufficiently detailed, signal completion
            4. Only then proceed to research and roadmap generation phases
            
            Response format:
            - For ongoing planning: Return the PlannerAgent's response directly
            - When profile is complete: Include the phrase "profile is complete" in your response
            - Always maintain a conversational and helpful tone
            """;
        public IList<AITool> Tools { get; }

        private readonly IPlannerAgent _plannerAgent;
        private readonly IResearchAgent _researchAgent;
        private readonly IStructuringAgent _structuringAgent;
        private readonly IRefinementAgent _refinementAgent;

        public OrchestratorAgent(
            IOptions<AzureOpenAIOptions> options,
            IPlannerAgent plannerAgent,
            IResearchAgent researchAgent, 
            IStructuringAgent structuringAgent,
            IRefinementAgent refinementAgent)
        {
            _plannerAgent = plannerAgent;
            _researchAgent = researchAgent;
            _structuringAgent = structuringAgent;
            _refinementAgent = refinementAgent;
            
            var azureOptions = options.Value;
            Agent = new AzureOpenAIClient(
                new Uri(azureOptions.Endpoint),
                new DefaultAzureCredential())
                    .GetChatClient(azureOptions.ModelName)
                    .CreateAIAgent(
                        name: Name,
                        instructions: SystemMessage,
                        tools: [
                            plannerAgent.Agent.AsAIFunction(),
                            researchAgent.Agent.AsAIFunction(),
                            structuringAgent.Agent.AsAIFunction(),
                            refinementAgent.Agent.AsAIFunction()
                        ]
                    );
        }

        public async Task<string> Invoke(string input)
        {
            // During the planning phase, we primarily work with the PlannerAgent
            if (Thread != null)
            {
                // Continue existing conversation thread
                var plannerResponse = await _plannerAgent.Invoke(input);
                
                // Check if the user profile is complete
                var currentProfile = _plannerAgent.GetCurrentUserProfile();
                if (IsProfileSufficient(currentProfile))
                {
                    return plannerResponse + "\n\nGreat! I have enough information about your learning goals and background. Your profile is complete and I'm ready to create your personalized learning roadmap. Click the 'Generate My Learning Roadmap' button when you're ready to proceed!";
                }
                
                return plannerResponse;
            }
            
            // Fallback to orchestrator agent
            var response = await Agent.RunAsync(input, Thread);
            return response.ToString();
        }

        public Task<string> StartPlanning(string learningGoal)
        {
            Thread = Agent.GetNewThread();
            _plannerAgent.Thread = Thread; // Share the thread with planner agent
            return _plannerAgent.Invoke(learningGoal);
        }

        private bool IsProfileSufficient(UserProfile? profile)
        {
            return profile != null &&
                   !string.IsNullOrWhiteSpace(profile.LearningGoal) &&
                   !string.IsNullOrWhiteSpace(profile.ExperienceLevel) &&
                   (profile.KnownSkills?.Count > 0 || profile.PreferredLearningStyles?.Count > 0);
        }
    }
}

