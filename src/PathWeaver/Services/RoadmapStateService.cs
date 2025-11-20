using PathWeaver.Models;

namespace PathWeaver.Services;

public class RoadmapStateService
{
    private Roadmap? _currentRoadmap;

    public event Action? RoadmapChanged;

    public Roadmap? CurrentRoadmap
    {
        get => _currentRoadmap;
        set
        {
            _currentRoadmap = value;
            RoadmapChanged?.Invoke();
        }
    }

    public void SetRoadmap(Roadmap roadmap)
    {
        CurrentRoadmap = roadmap;
    }

    public void ClearRoadmap()
    {
        CurrentRoadmap = null;
    }
}