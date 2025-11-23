using CourseAI.Models;

namespace CourseAI.Agents.Interfaces
{
    public interface IOrchestratorAgent : IAgent
    {
        Task<string> StartPlanning(string learningGoal);
        IAsyncEnumerable<string> StartPlanningStreaming(string learningGoal);
        IAsyncEnumerable<string> InvokeStreaming(string input);
    }
}
