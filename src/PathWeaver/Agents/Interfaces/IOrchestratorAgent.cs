using PathWeaver.Models;

namespace PathWeaver.Agents.Interfaces
{
    public interface IOrchestratorAgent : IAgent
    {
        Task<string> StartPlanning(string learningGoal);
    }
}
