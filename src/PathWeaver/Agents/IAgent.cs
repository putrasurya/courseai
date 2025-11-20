namespace PathWeaver.Agents
{
    public interface IAgent
    {
        string Name { get; }
        Task<string> Invoke(string input);
    }
}
