using PathWeaver.Models;

namespace PathWeaver.Agents
{
    public interface IPlannerAgent : IAgent
    {
        UserProfile? GetCurrentUserProfile();
    }
}
