namespace PathWeaver.Agents
{
    public class OrchestratorAgent : IOrchestratorAgent
    {
        private readonly IPlannerAgent _plannerAgent;

        public string Name => "OrchestratorAgent";

        public OrchestratorAgent(IPlannerAgent plannerAgent)
        {
            _plannerAgent = plannerAgent;
        }

        public Task<string> Invoke(string input)
        {
            // The orchestrator will delegate to other agents based on the input.
            return _plannerAgent.Invoke(input);
        }

        public Task<string> StartPlanning(string learningGoal)
        {
            // This is the entry point for the planning process.
            return Invoke(learningGoal);
        }
    }
}
