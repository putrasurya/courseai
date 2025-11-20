using PathWeaver.Models;

namespace PathWeaver.Agents
{
    public interface IOrchestratorAgent : IAgent
    {
        Task<string> StartPlanning(string learningGoal);
        Task<Roadmap> GenerateRoadmap();
        Task<Roadmap> RefineRoadmap(Roadmap roadmap, string userFeedback);
    }
}
