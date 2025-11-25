# Development Setup Guide - CourseAI

**Last Updated**: November 25, 2025

## 🚀 Quick Start for Developers

### **Prerequisites**
- **.NET 8 SDK** (Download from [dotnet.microsoft.com](https://dotnet.microsoft.com))
- **Visual Studio 2022** or **VS Code** with C# extension
- **Azure OpenAI access** (required for AI functionality)
- **Git** for version control

### **1. Environment Setup**

#### **Clone Repository**:
```bash
git clone <repository-url>
cd CourseAI
```

#### **Install Dependencies**:
```bash
dotnet restore
```

#### **Configure Development Settings**:
```bash
# Use user secrets (recommended for development)
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-resource.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key"
dotnet user-secrets set "AzureOpenAI:ModelDeploymentName" "gpt-4"
dotnet user-secrets set "TavilyApiKey" "your-search-key" # Optional
```

#### **Run Application**:
```bash
dotnet run --project src/CourseAI
```

Navigate to `https://localhost:5001` to verify setup.

## 📁 Project Structure

```
CourseAI/
├── src/CourseAI/              # Main application
│   ├── Agents/                # AI agent implementations
│   │   ├── Interfaces/        # Agent contracts
│   │   ├── OrchestratorAgent.cs
│   │   ├── PlannerAgent.cs
│   │   ├── StructuringAgent.cs
│   │   ├── RefinementAgent.cs
│   │   ├── CurriculumArchitectAgent.cs
│   │   ├── PathOptimizationAgent.cs
│   │   └── ResourceGatheringAgent.cs
│   ├── Components/            # Blazor components
│   ├── Models/                # Data models
│   ├── Services/              # Business logic services
│   ├── Helpers/               # Utility classes
│   └── wwwroot/               # Static web assets
├── tests/                     # Test projects
├── docs/                      # Documentation
└── CourseAI.sln               # Solution file
```

## 🔧 Development Workflow

### **1. Agent Development**

#### **Creating a New Agent**:
1. Create interface in `Agents/Interfaces/IYourAgent.cs`
2. Implement agent in `Agents/YourAgent.cs`
3. Register in `Program.cs`
4. Add to parent agent's tools (if applicable)

#### **Agent Template**:
```csharp
// IYourAgent.cs
public interface IYourAgent : IAgent
{
    // Inherit from base IAgent interface
}

// YourAgent.cs
public class YourAgent : IYourAgent
{
    public AIAgent Agent { get; init; }
    public AgentThread? Thread { get; set; }
    public string Name => "YourAgent";
    public string Description => "Description of agent purpose";
    
    public string SystemMessage => """
        Your detailed system prompt here.
        Define the agent's role, expertise, and behavior.
        """;

    public YourAgent(InstrumentChatClient chatClient)
    {
        Agent = chatClient.AzureOpenAIClient
            .GetChatClient(chatClient.ModelDeploymentName)
            .CreateAIAgent(
                name: Name,
                instructions: SystemMessage,
                tools: [
                    // Add any tools this agent needs
                ]
            );
    }

    public async Task<string> Invoke(string input)
    {
        var response = await Agent.InvokeAsync(Thread, input);
        return response;
    }
}
```

#### **Registration in Program.cs**:
```csharp
builder.Services.AddSingleton<IYourAgent, YourAgent>();
```

### **2. Service Development**

#### **Service Template**:
```csharp
public class YourService
{
    private readonly ILogger<YourService> _logger;
    
    public YourService(ILogger<YourService> logger)
    {
        _logger = logger;
    }
    
    public async Task<Result> DoSomething()
    {
        // Service implementation
    }
}
```

### **3. Testing**

#### **Run Tests**:
```bash
dotnet test
```

#### **Test Structure**:
```
tests/
├── CourseAI.Tests/
│   ├── Agents/                # Agent unit tests
│   ├── Services/              # Service unit tests
│   └── Integration/           # Integration tests
```

#### **Agent Test Example**:
```csharp
[Test]
public async Task PlannerAgent_Should_BuildProfile()
{
    // Arrange
    var mockChatClient = new Mock<InstrumentChatClient>();
    var agent = new PlannerAgent(mockChatClient.Object);
    
    // Act
    var result = await agent.Invoke("I want to learn React");
    
    // Assert
    Assert.IsNotNull(result);
}
```

## 🔍 Debugging

### **1. Agent Debugging**

#### **Enable Detailed Logging**:
```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.Extensions.AI": "Debug",
      "CourseAI.Agents": "Debug"
    }
  }
}
```

#### **Add Logging to Agents**:
```csharp
public class YourAgent : IYourAgent
{
    private readonly ILogger<YourAgent> _logger;
    
    public YourAgent(InstrumentChatClient chatClient, ILogger<YourAgent> logger)
    {
        _logger = logger;
        // Agent initialization
    }
    
    public async Task<string> Invoke(string input)
    {
        _logger.LogInformation("Agent {Name} received input: {Input}", Name, input);
        var response = await Agent.InvokeAsync(Thread, input);
        _logger.LogInformation("Agent {Name} generated response: {Response}", Name, response);
        return response;
    }
}
```

### **2. OpenTelemetry Tracing**

#### **View Traces**:
```bash
# Start Aspire dashboard (if configured)
dotnet run --project aspire/AppHost
```

Navigate to OpenTelemetry dashboard to view agent interactions and performance.

### **3. Breakpoint Debugging**

#### **Key Debugging Points**:
- Agent `Invoke()` methods
- Service method entry/exit
- Tool registration in constructors
- Decision logic in OrchestratorAgent

## 📊 Code Quality

### **1. Code Style**

#### **EditorConfig** (Create `.editorconfig`):
```ini
root = true

[*.cs]
indent_style = space
indent_size = 4
end_of_line = crlf
insert_final_newline = true
trim_trailing_whitespace = true

# C# formatting rules
dotnet_sort_system_directives_first = true
csharp_new_line_before_open_brace = all
```

### **2. Linting and Formatting**

#### **Format Code**:
```bash
dotnet format
```

#### **Add Analyzers** (in .csproj):
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" PrivateAssets="all" />
  <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0" PrivateAssets="all" />
</ItemGroup>
```

### **3. Performance Monitoring**

#### **Add Performance Counters**:
```csharp
// In agent methods
using var activity = Activity.StartActivity("Agent.Invoke");
activity?.SetTag("agent.name", Name);
activity?.SetTag("input.length", input.Length);
```

## 🚀 Build and Deployment

### **1. Local Build**:
```bash
dotnet build --configuration Release
```

### **2. Run Production Mode**:
```bash
dotnet run --project src/CourseAI --configuration Release
```

### **3. Create Deployment Package**:
```bash
dotnet publish --configuration Release --output ./publish
```

## 📋 Development Guidelines

### **1. Agent Development Principles**:
- **Single Responsibility**: Each agent has one focused purpose
- **Clear Interfaces**: Well-defined contracts between agents
- **Comprehensive Logging**: Log all important decisions and actions
- **Error Handling**: Graceful degradation when services are unavailable

### **2. Code Organization**:
- **Agents**: Pure agent logic, minimal business logic
- **Services**: Business logic and data management
- **Models**: Data structures and DTOs
- **Helpers**: Utility functions and extensions

### **3. Best Practices**:
- Use dependency injection for all services
- Follow async/await patterns consistently
- Add comprehensive XML documentation
- Write unit tests for all public methods
- Use structured logging with semantic information

---

**Development Status**: ✅ **Developer Ready**  
**Setup Time**: ~10 minutes with prerequisites  
**IDE Support**: ✅ Full IntelliSense and debugging  
**Framework**: .NET 8 + Blazor Server + Microsoft.Extensions.AI