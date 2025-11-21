using PathWeaver.Models;

namespace PathWeaver.Agents.Interfaces
{
    public interface IOrchestratorAgent : IAgent
    {
        Task<string> StartPlanning(string learningGoal);
        IAsyncEnumerable<string> StartPlanningStreaming(string learningGoal);
        IAsyncEnumerable<string> InvokeStreaming(string input);
    }
}
