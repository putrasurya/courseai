namespace PathWeaver.Agents
{
    public interface IOrchestratorAgent : IAgent
    {
        Task<string> StartPlanning(string learningGoal);
    }
}
