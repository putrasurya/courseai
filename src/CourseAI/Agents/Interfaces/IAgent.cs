using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using CourseAI.Models;

namespace CourseAI.Agents.Interfaces
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

