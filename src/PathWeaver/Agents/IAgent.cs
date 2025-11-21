using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using PathWeaver.Models;

namespace PathWeaver.Agents
{
    public interface IAgent
    {
        AgentThread? Thread { get; set; }
        AIAgent Agent { get; init; }
        string Name { get; }
        string Description { get; }
        string SystemMessage { get; }
        IList<AITool> Tools { get; }
        Task<string> Invoke(string input);
    }
}

