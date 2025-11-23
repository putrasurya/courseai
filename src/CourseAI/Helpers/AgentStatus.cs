using CourseAI.Services;

namespace CourseAI.Helpers;

public static class AgentStatus
{
    public static async Task<T> ExecuteWithStatusAsync<T>(
        IAgentStatusService statusService,
        string agentName,
        Func<Task<T>> operation)
    {
        var statusMessage = agentName switch
        {
            "OrchestratorAgent" => "Coordinating your learning journey...",
            "PlannerAgent" => "Gathering your learning profile...",
            "StructuringAgent" => "Building your roadmap structure...",
            "CurriculumAgent" => "Designing curriculum content...",
            "PathOptimizationAgent" => "Optimizing learning path...",
            _ => $"Processing with {agentName}..."
        };

        try
        {
            statusService.SetStatus(agentName, statusMessage);
            return await operation();
        }
        finally
        {
            statusService.ClearStatus();
        }
    }

    public static async Task ExecuteWithStatusAsync(
        IAgentStatusService statusService,
        string agentName,
        Func<Task> operation)
    {
        await ExecuteWithStatusAsync<object?>(statusService, agentName, async () =>
        {
            await operation();
            return null;
        });
    }
}