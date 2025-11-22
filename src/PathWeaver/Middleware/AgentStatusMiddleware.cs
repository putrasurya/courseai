using PathWeaver.Services;

namespace PathWeaver.Middleware;

public static class AgentStatusMiddleware
{
    
    private static readonly Dictionary<string, string> AgentStatusMessages = new()
    {
        { "OrchestratorAgent", "Coordinating your request..." },
        { "PlannerAgent", "Gathering your learning profile..." },
        { "StructuringAgent", "Building your roadmap structure..." },
        { "CurriculumAgent", "Designing curriculum framework..." },
        { "PathOptimizationAgent", "Optimizing learning path..." },
        { "ResearchAgent", "Researching learning resources..." },
        { "RefinementAgent", "Refining roadmap details..." }
    };

    public static async Task<T> ExecuteWithStatusAsync<T>(
        ProcessingStatusService statusService,
        string agentName,
        Func<Task<T>> action)
    {
        if (AgentStatusMessages.TryGetValue(agentName, out var statusMessage))
        {
            statusService.SetProcessing(statusMessage);
        }

        try
        {
            return await action();
        }
        finally
        {
            statusService.ClearProcessing();
        }
    }

    public static async Task ExecuteWithStatusAsync(
        ProcessingStatusService statusService,
        string agentName,
        Func<Task> action)
    {
        if (AgentStatusMessages.TryGetValue(agentName, out var statusMessage))
        {
            statusService.SetProcessing(statusMessage);
        }

        try
        {
            await action();
        }
        finally
        {
            statusService.ClearProcessing();
        }
    }
}