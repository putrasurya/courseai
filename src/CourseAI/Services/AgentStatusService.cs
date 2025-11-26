namespace CourseAI.Services;

public interface IAgentStatusService
{
    string? CurrentStatus { get; }
    string? CurrentAgent { get; }
    bool IsProcessing { get; }
    int ProgressPercentage { get; }
    string? ProgressDetails { get; }
    event Action? OnStatusChanged;
    
    void SetStatus(string agent, string status);
    void SetProgress(string agent, string status, int percentage = 0, string? details = null);
    void UpdateProgress(int percentage, string? details = null);
    void ClearStatus();
}

public class AgentStatusService : IAgentStatusService
{
    public string? CurrentStatus { get; private set; }
    public string? CurrentAgent { get; private set; }
    public int ProgressPercentage { get; private set; }
    public string? ProgressDetails { get; private set; }
    public bool IsProcessing => !string.IsNullOrEmpty(CurrentStatus);
    
    public event Action? OnStatusChanged;

    public void SetStatus(string agent, string status)
    {
        CurrentAgent = agent;
        CurrentStatus = status;
        ProgressPercentage = 0;
        ProgressDetails = null;
        OnStatusChanged?.Invoke();
    }

    public void SetProgress(string agent, string status, int percentage = 0, string? details = null)
    {
        CurrentAgent = agent;
        CurrentStatus = status;
        ProgressPercentage = Math.Max(0, Math.Min(100, percentage));
        ProgressDetails = details;
        OnStatusChanged?.Invoke();
    }

    public void UpdateProgress(int percentage, string? details = null)
    {
        ProgressPercentage = Math.Max(0, Math.Min(100, percentage));
        if (details != null)
            ProgressDetails = details;
        OnStatusChanged?.Invoke();
    }

    public void ClearStatus()
    {
        CurrentAgent = null;
        CurrentStatus = null;
        ProgressPercentage = 0;
        ProgressDetails = null;
        OnStatusChanged?.Invoke();
    }
}