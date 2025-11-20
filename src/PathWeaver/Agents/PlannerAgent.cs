namespace PathWeaver.Agents
{
    public class PlannerAgent : IPlannerAgent
    {
        public string Name => "PlannerAgent";

        public async Task<string> Invoke(string input)
        {
            // In the future, this will use the Microsoft Agent Framework
            // to process the input and generate clarifying questions.
            await Task.Delay(3000); // Simulate work.
            return "Based on your goal to learn '" + input + "', what is your current experience level (beginner, intermediate, expert)?";
        }
    }
}
