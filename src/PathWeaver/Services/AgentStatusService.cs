namespace PathWeaver.Services;

public interface IAgentStatusService
{
    string? CurrentStatus { get; }
    string? CurrentAgent { get; }
    bool IsProcessing { get; }
    event Action? OnStatusChanged;
    
    void SetStatus(string agent, string status);
    void ClearStatus();
}

public class AgentStatusService : IAgentStatusService
{
    public string? CurrentStatus { get; private set; }
    public string? CurrentAgent { get; private set; }
    public bool IsProcessing => !string.IsNullOrEmpty(CurrentStatus);
    
    public event Action? OnStatusChanged;

    public void SetStatus(string agent, string status)
    {
        CurrentAgent = agent;
        CurrentStatus = status;
        OnStatusChanged?.Invoke();
    }

    public void ClearStatus()
    {
        CurrentAgent = null;
        CurrentStatus = null;
        OnStatusChanged?.Invoke();
    }
}