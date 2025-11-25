# Agents Overview - CourseAI System

**Last Updated**: November 25, 2025  
**Total Agents**: 7 Active

## 🎭 Complete Agent Catalog

### **Core Workflow Agents (4)**

#### **🎯 OrchestratorAgent** - Main Controller
```csharp
Class: OrchestratorAgent : IOrchestratorAgent
Location: /src/CourseAI/Agents/OrchestratorAgent.cs
Status: ✅ Active (Main Entry Point)
```

**Responsibilities**:
- Primary user interaction point
- Workflow decision making
- Agent coordination
- Automatic roadmap generation triggering

**Decision Logic**:
```csharp
if (profile incomplete) → Use PlannerAgent
if (profile complete && no roadmap) → Use StructuringAgent  
if (roadmap exists && feedback) → Use RefinementAgent
```

**Key Features**:
- Automatic profile completeness detection
- Seamless conversation flow
- No manual button interactions required
- Smart progression between phases

---

#### **📋 PlannerAgent** - Profile Builder
```csharp
Class: PlannerAgent : IPlannerAgent
Location: /src/CourseAI/Agents/PlannerAgent.cs
Status: ✅ Active
```

**Responsibilities**:
- User profile building through conversation
- Learning goal clarification
- Experience level assessment
- Learning preference capture

**Tools Available**:
- `updateUserProfile` - Update profile fields
- `removeFromUserProfile` - Remove profile data
- `getUserProfileSummary` - Get current profile state

**System Message Focus**:
- Conversational profile gathering
- GDPR-compliant data handling
- Profile completeness assessment
- User-friendly questioning approach

---

#### **🏗️ StructuringAgent** - Framework Coordinator  
```csharp
Class: StructuringAgent : IStructuringAgent
Location: /src/CourseAI/Agents/StructuringAgent.cs
Status: ✅ Active (Primary Roadmap Creator)
```

**Responsibilities**:
- Learning framework creation
- Coordination of specialized design tools
- Quality validation and completeness checking
- Resource integration

**Specialized Tools**:
- `CurriculumArchitectAgent` - Educational design
- `PathOptimizationAgent` - Sequence optimization  
- `ResourceGatheringAgent` - Resource discovery

**Workflow Process**:
1. Initialize roadmap with user profile
2. Use CurriculumArchitectAgent for educational design
3. Use PathOptimizationAgent for sequence optimization
4. Create focused modules and specific topics
5. Add granular key concepts (3-5 per topic)
6. Use ResourceGatheringAgent for resource discovery
7. Validate completeness and quality
8. Finalize roadmap with proper status

**Quality Requirements**:
- **Modules**: Focused skill areas (e.g., "HTML Fundamentals")
- **Topics**: Specific subtopics (e.g., "Semantic Elements")  
- **Key Concepts**: Granular objectives (e.g., "Article vs Section usage")
- **Resources**: 5-8 quality resources per module

---

#### **✨ RefinementAgent** - Feedback Processor
```csharp
Class: RefinementAgent : IRefinementAgent  
Location: /src/CourseAI/Agents/RefinementAgent.cs
Status: ✅ Active
```

**Responsibilities**:
- Process user feedback on existing roadmaps
- Make targeted improvements
- Maintain roadmap quality standards
- Handle user refinement requests

**Capabilities**:
- Analyze existing roadmap structure
- Apply user feedback selectively
- Maintain educational quality
- Preserve roadmap coherence

---

### **Specialized Tool Agents (3)**

#### **🎓 CurriculumArchitectAgent** - Educational Designer
```csharp
Class: CurriculumArchitectAgent : ICurriculumArchitectAgent
Location: /src/CourseAI/Agents/CurriculumArchitectAgent.cs
Usage: Tool for StructuringAgent
Status: ✅ Active as Tool
```

**Expertise Areas**:
- Educational theory application (Bloom's Taxonomy, Scaffolding)
- Learning objective creation
- Prerequisite mapping
- Skill progression design
- Assessment strategy

**Web Search Tools**:
- `SearchEducationalContent` - Find tutorials and learning materials
- `SearchBestPractices` - Research current industry standards

**Focus**:
- Pedagogically sound curriculum design
- Proper learning granularity
- SMART learning objectives
- Progressive skill building

---

#### **🛤️ PathOptimizationAgent** - Sequence Optimizer
```csharp  
Class: PathOptimizationAgent : IPathOptimizationAgent
Location: /src/CourseAI/Agents/PathOptimizationAgent.cs
Usage: Tool for StructuringAgent
Status: ✅ Active as Tool
```

**Specializations**:
- Learning sequence optimization
- Time estimation and pacing
- Dependency mapping
- Personalized learning paths
- Efficiency maximization

**Optimization Factors**:
- Cognitive load management
- Knowledge transfer effectiveness
- User time constraints
- Learning style preferences
- Skill building dependencies

---

#### **📚 ResourceGatheringAgent** - Resource Discovery
```csharp
Class: ResourceGatheringAgent
Location: /src/CourseAI/Agents/ResourceGatheringAgent.cs  
Usage: Tool for StructuringAgent
Status: ✅ Active as Tool (No Interface)
```

**Capabilities**:
- Multi-platform resource discovery
- Real-time web search integration
- Resource quality assessment
- URL validation
- Diverse format sourcing

**Resource Sources**:
- YouTube tutorials
- Documentation sites
- Online courses (Coursera, Udemy)
- Interactive platforms
- GitHub repositories
- Educational websites

**Quality Validation**:
- URL accessibility checking
- Content relevance assessment
- Resource diversity ensuring
- User skill level matching

---

## 🔧 Agent Integration Patterns

### **Dependency Injection Registration**:
```csharp
// Core workflow agents
builder.Services.AddSingleton<IPlannerAgent, PlannerAgent>();
builder.Services.AddSingleton<IStructuringAgent, StructuringAgent>();
builder.Services.AddSingleton<IRefinementAgent, RefinementAgent>();

// Specialized tool agents  
builder.Services.AddSingleton<ICurriculumArchitectAgent, CurriculumArchitectAgent>();
builder.Services.AddSingleton<IPathOptimizationAgent, PathOptimizationAgent>();
builder.Services.AddSingleton<ResourceGatheringAgent>(); // No interface

// Main coordinator
builder.Services.AddSingleton<IOrchestratorAgent, OrchestratorAgent>();
```

### **Tool Registration Pattern**:
```csharp
// In StructuringAgent constructor
Agent = new AzureOpenAIClient(endpoint, credential)
    .GetChatClient(model)
    .CreateAIAgent(
        name: Name,
        instructions: SystemMessage,
        tools: [
            _curriculumArchitectAgent.Agent.AsAIFunction(),
            _pathOptimizationAgent.Agent.AsAIFunction(),
            _resourceGatheringAgent.Agent.AsAIFunction(),
            // ... RoadMap management tools
        ]
    );
```

## 🎯 Agent Communication Flow

```
User Input
↓
OrchestratorAgent (Decision Making)
├── Profile Building → PlannerAgent
├── Framework Creation → StructuringAgent
│   ├── Educational Design → CurriculumArchitectAgent  
│   ├── Path Optimization → PathOptimizationAgent
│   └── Resource Discovery → ResourceGatheringAgent
└── Feedback Processing → RefinementAgent
↓
Complete Learning Roadmap
```

## 💡 Key Design Principles

### **1. Single Responsibility**
Each agent has one focused area of expertise and responsibility.

### **2. Hierarchical Tools**  
Specialized agents serve as tools for main workflow agents.

### **3. Quality Assurance**
Multiple validation layers ensure completeness and quality.

### **4. Conversational Flow**
Natural language interaction without manual UI interactions.

### **5. Microsoft Framework Compliance**
Follows Microsoft.Extensions.AI patterns and best practices.

---

**Agent System Status**: ✅ **FULLY OPERATIONAL**  
**Last Verified**: November 25, 2025  
**Framework**: Microsoft.Extensions.AI + Azure OpenAI  
**Total LOC**: ~2000+ lines across all agents