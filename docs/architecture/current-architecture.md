# Current Architecture - CourseAI Specialized Agent System

**Last Updated**: November 25, 2025  
**Status**: ✅ **Production Active**

## 🏗️ Architecture Overview

CourseAI implements a **hierarchical specialized agent architecture** where each agent has focused responsibilities and domain expertise.

## 📊 Current Agent Structure

### **Verified Agent Count: 7 Active Agents**
Based on code audit of `/src/CourseAI/Agents/` and `Program.cs`:

```
OrchestratorAgent (Main Coordinator)
├── PlannerAgent (Profile Building)
├── StructuringAgent (Learning Framework Creation)
│   ├── CurriculumArchitectAgent (Educational Design Tool)
│   ├── PathOptimizationAgent (Sequence Optimization Tool)
│   └── ResourceGatheringAgent (Resource Discovery Tool)
└── RefinementAgent (Feedback Processing)
```

## 🎯 Agent Responsibilities

### **🎭 OrchestratorAgent** (Main Controller)
- **Role**: Workflow coordination and decision making
- **Input**: User learning goals and feedback
- **Output**: Complete learning roadmap
- **Tools**: All other agents
- **Status**: ✅ Active

**Key Features**:
- Automatic profile completeness detection
- Smart workflow progression
- Seamless roadmap generation
- Feedback processing coordination

### **📋 PlannerAgent** (Profile Builder)
- **Role**: User profile building through conversation
- **Input**: User learning goals and questions
- **Output**: Complete LearningProfile
- **Tools**: LearningProfile management tools
- **Status**: ✅ Active

**Capabilities**:
- Conversational profile gathering
- Learning goal clarification
- Experience level assessment
- Preference capture

### **🏗️ StructuringAgent** (Framework Coordinator)
- **Role**: Learning framework creation using specialized tools
- **Input**: Complete LearningProfile
- **Output**: Structured learning roadmap with resources
- **Tools**: CurriculumArchitectAgent, PathOptimizationAgent, ResourceGatheringAgent
- **Status**: ✅ Active

**Workflow**:
1. Uses CurriculumArchitectAgent for educational design
2. Uses PathOptimizationAgent for sequence optimization
3. Uses ResourceGatheringAgent for resource discovery
4. Creates granular modules, topics, and key concepts
5. Validates completeness and quality

### **🎓 CurriculumArchitectAgent** (Educational Designer)
- **Role**: Pedagogically sound curriculum design
- **Usage**: Tool for StructuringAgent
- **Focus**: Educational theory, learning objectives, proper granularity
- **Status**: ✅ Active as tool

**Expertise**:
- Bloom's taxonomy application
- Learning progression design
- Granular topic breakdown
- Key concept identification

### **🛤️ PathOptimizationAgent** (Sequence Optimizer)
- **Role**: Learning path optimization for efficiency
- **Usage**: Tool for StructuringAgent
- **Focus**: Learning sequence, timing, personalization
- **Status**: ✅ Active as tool

**Capabilities**:
- Optimal learning sequences
- Time estimation
- Dependency mapping
- Personalized pacing

### **📚 ResourceGatheringAgent** (Resource Discovery)
- **Role**: Multi-platform learning resource discovery
- **Usage**: Tool for StructuringAgent
- **Focus**: Web search, resource quality, diverse formats
- **Status**: ✅ Active as tool

**Features**:
- Real-time web search integration
- Multi-platform resource discovery
- Quality validation
- URL verification

### **✨ RefinementAgent** (Feedback Processor)
- **Role**: Roadmap refinement based on user feedback
- **Input**: Existing roadmap + user feedback
- **Output**: Improved roadmap
- **Status**: ✅ Active

## 🔄 Workflow Architecture

### **Complete Generation Flow**:
```
User Input → OrchestratorAgent
├── Profile Building → PlannerAgent
├── Framework Creation → StructuringAgent
│   ├── Educational Design → CurriculumArchitectAgent
│   ├── Sequence Optimization → PathOptimizationAgent
│   └── Resource Discovery → ResourceGatheringAgent
└── Feedback Processing → RefinementAgent
```

### **Dependency Injection (Program.cs)**:
```csharp
// Core workflow agents
builder.Services.AddSingleton<IPlannerAgent, PlannerAgent>();
builder.Services.AddSingleton<IStructuringAgent, StructuringAgent>();
builder.Services.AddSingleton<IRefinementAgent, RefinementAgent>();

// Specialized tool agents
builder.Services.AddSingleton<ICurriculumArchitectAgent, CurriculumArchitectAgent>();
builder.Services.AddSingleton<IPathOptimizationAgent, PathOptimizationAgent>();
builder.Services.AddSingleton<ResourceGatheringAgent>();

// Main coordinator
builder.Services.AddSingleton<IOrchestratorAgent, OrchestratorAgent>();
```

## 🎯 Key Architectural Principles

### **1. Hierarchical Specialization**
- **Main Agents**: Handle workflow and coordination
- **Tool Agents**: Provide specialized domain expertise
- **Clear Separation**: Each agent has focused responsibility

### **2. Tool-Based Pattern**
- Specialized agents are registered as tools within main agents
- Follows Microsoft.Extensions.AI framework patterns
- Clean dependency injection and service composition

### **3. Automatic Workflow**
- No manual button clicking required
- Intelligent progression through conversation
- Seamless transition from planning to roadmap generation

### **4. Quality Assurance**
- Built-in validation at multiple levels
- Resource URL verification
- Completeness checking for modules, topics, concepts

## 💡 Benefits

✅ **Focused Expertise**: Each agent specializes in specific domain  
✅ **Clean Architecture**: Clear separation of concerns  
✅ **Scalable Design**: Easy to enhance or add new specialists  
✅ **Quality Assured**: Multiple validation layers  
✅ **User Focused**: Seamless conversational experience  
✅ **Microsoft Framework**: Follows established patterns  

## 🔍 Technical Implementation

### **Agent Base Pattern**:
```csharp
public class ExampleAgent : IExampleAgent
{
    public AIAgent Agent { get; init; }
    public AgentThread? Thread { get; set; }
    public string Name => "ExampleAgent";
    public string Description => "Agent description";
    public string SystemMessage => "Detailed system prompt...";
    
    // Constructor with DI and AI client setup
    // Invoke method for processing user input
}
```

### **Tool Registration Pattern**:
```csharp
Agent = new AzureOpenAIClient(endpoint, credential)
    .GetChatClient(model)
    .CreateAIAgent(
        name: Name,
        instructions: SystemMessage,
        tools: [
            _toolAgent1.Agent.AsAIFunction(),
            _toolAgent2.Agent.AsAIFunction()
        ]
    );
```

---

**Architecture Status**: ✅ **STABLE AND OPERATIONAL**  
**Last Verified**: November 25, 2025  
**Agent Count**: 7 (4 main + 3 tools)  
**Framework**: Microsoft.Extensions.AI + Azure OpenAI