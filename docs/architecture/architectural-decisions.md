# Architectural Decision Records - CourseAI

**Last Updated**: November 25, 2025

## Overview

This document tracks major architectural decisions made during CourseAI development, including rationale and historical context.

## Decision Records

### **ADR-001: Multi-Agent Architecture (October 2025)**

**Status**: ✅ **Implemented**  
**Decision**: Adopt specialized multi-agent architecture over monolithic AI approach

**Context**:
- Initial single-agent approach was too generalized
- Need for specialized expertise in different domains
- Microsoft.Extensions.AI framework supports agent composition

**Decision**:
- Implement OrchestratorAgent as main coordinator
- Create specialized agents for specific domains (planning, structuring, refinement)
- Use hierarchical agent composition pattern

**Consequences**:
✅ **Positive**:
- Better quality outputs due to focused expertise
- Easier to maintain and enhance individual agents
- Clear separation of concerns
- Scalable architecture

❌ **Trade-offs**:
- Increased complexity in agent coordination
- More configuration and setup required

---

### **ADR-002: Tool-Based Agent Integration (November 2025)**

**Status**: ✅ **Implemented**  
**Decision**: Use specialized agents as tools within main workflow agents

**Context**:
- Original approach had all 8 agents as top-level services
- Framework complexity was increasing
- Need for cleaner coordination patterns

**Decision**:
- Main workflow agents: OrchestratorAgent, PlannerAgent, StructuringAgent, RefinementAgent
- Specialized agents as tools: CurriculumArchitectAgent, PathOptimizationAgent, ResourceGatheringAgent
- Use Microsoft.Extensions.AI `AsAIFunction()` pattern

**Implementation**:
```csharp
// StructuringAgent uses specialized agents as tools
Agent = chatClient.CreateAIAgent(
    name: Name,
    instructions: SystemMessage,
    tools: [
        _curriculumArchitectAgent.Agent.AsAIFunction(),
        _pathOptimizationAgent.Agent.AsAIFunction(),
        _resourceGatheringAgent.Agent.AsAIFunction()
    ]
);
```

**Consequences**:
✅ **Positive**:
- Cleaner architecture following framework patterns
- Better encapsulation of specialized logic
- Reduced complexity in OrchestratorAgent
- Easier dependency management

❌ **Trade-offs**:
- More indirection in agent communication
- Tool agents cannot be used independently

---

### **ADR-003: Automatic Workflow Progression (November 2025)**

**Status**: ✅ **Implemented**  
**Decision**: Remove manual UI interactions for roadmap generation

**Context**:
- Original design required users to click "Generate Roadmap" button
- Defeated purpose of having intelligent orchestrator
- User experience was fragmented

**Decision**:
- OrchestratorAgent automatically detects when profile is complete
- Seamlessly transitions from planning to roadmap generation
- No manual button interactions required

**Implementation**:
```csharp
public async Task<string> Invoke(string input)
{
    // Check profile completeness and automatically progress
    if (_learningProfileService.IsProfileSufficient())
    {
        return await AutoGenerateRoadmap();
    }
    
    return await _plannerAgent.Invoke(input);
}
```

**Consequences**:
✅ **Positive**:
- Seamless user experience
- True AI orchestration
- Reduced UI complexity
- Natural conversation flow

❌ **Trade-offs**:
- Less user control over timing
- Requires robust profile completeness detection

---

### **ADR-004: Module-Level Resource Architecture (November 2025)**

**Status**: ✅ **Implemented**  
**Decision**: Assign learning resources at module level instead of topic level

**Context**:
- Topic-level resources were too fragmented
- Real learning resources typically cover entire skill areas
- Better alignment with how users actually learn

**Decision**:
- Move resources from `RoadmapTopic` to `RoadmapModule`
- Each module has 5-8 comprehensive resources
- Resources cover all topics within the module

**Implementation**:
```csharp
public class RoadmapModule
{
    public List<LearningResource> Resources { get; set; } = [];
    // Resources cover all topics in this module
}

public class RoadmapTopic
{
    // Resources property removed
    // Topics focus on learning objectives only
}
```

**Consequences**:
✅ **Positive**:
- More cohesive learning materials
- Better resource utilization
- Matches real-world learning patterns
- Reduced resource fragmentation

❌ **Trade-offs**:
- Less granular resource targeting
- Potential resource overlap across topics

---

### **ADR-005: Web Search Integration (November 2025)**

**Status**: ✅ **Implemented**  
**Decision**: Integrate real-time web search for resource discovery

**Context**:
- Static knowledge becomes outdated quickly
- Need for current tutorials and best practices
- Educational landscape changes rapidly

**Decision**:
- Integrate Tavily Search API for real-time resource discovery
- Add web search tools to CurriculumArchitectAgent and ResourceGatheringAgent
- Graceful degradation when API unavailable

**Implementation**:
```csharp
// Web search tools available to agents
[Description("Search for educational content, tutorials, and learning resources")]
public async Task<string> SearchEducationalContent(string query)

[Description("Search for current best practices and industry standards")]  
public async Task<string> SearchBestPractices(string query)
```

**Consequences**:
✅ **Positive**:
- Current, up-to-date resource recommendations
- Better resource quality and relevance
- Industry trend awareness
- Enhanced learning roadmap quality

❌ **Trade-offs**:
- External API dependency
- Additional configuration complexity
- Potential rate limiting and costs

---

### **ADR-006: Quality Validation Pipeline (November 2025)**

**Status**: ✅ **Implemented**  
**Decision**: Implement comprehensive quality validation throughout roadmap creation

**Context**:
- Need to ensure roadmap completeness
- Resource quality was inconsistent
- User experience degraded with missing components

**Decision**:
- Add validation tools at multiple levels
- Mandatory validation before roadmap completion
- Quality enforcement sequence in StructuringAgent

**Implementation**:
```csharp
// Validation tools
ValidateRoadmapQuality();
GetTopicsNeedingConcepts();
GetModulesNeedingResources();
ValidateModuleResourceQuality();
ValidateAllResourceUrls();
```

**Consequences**:
✅ **Positive**:
- Consistent roadmap quality
- Complete learning paths
- Validated resource URLs
- Better user experience

❌ **Trade-offs**:
- Increased generation time
- More complex agent logic
- Potential for validation failures

---

### **ADR-007: Granular Learning Structure (November 2025)**

**Status**: ✅ **Implemented**  
**Decision**: Enforce granular modules, topics, and key concepts

**Context**:
- Generic "Advanced Topics" modules were not helpful
- Users needed specific, actionable learning objectives
- Better progression tracking required

**Decision**:
- **Modules**: Focused skill areas (e.g., "HTML Fundamentals")
- **Topics**: Specific subtopics (e.g., "Semantic Elements")
- **Key Concepts**: Granular objectives (e.g., "Article vs Section usage")
- Mandatory 3-5 key concepts per topic

**Implementation**:
```
✅ Good Granularity:
Module: "JavaScript Variables and Functions"
├── Topic: "Variable Declarations"
│   ├── Concept: "let vs const usage rules"
│   ├── Concept: "Block scope behavior"
│   └── Concept: "Hoisting patterns"
└── Topic: "Function Types"
    ├── Concept: "Arrow function syntax"
    ├── Concept: "Function hoisting differences"
    └── Concept: "this binding in functions"

❌ Bad Granularity:
Module: "Advanced JavaScript"
├── Topic: "Complex Features"
└── Topic: "Modern Syntax"
```

**Consequences**:
✅ **Positive**:
- Clear learning objectives
- Better progress tracking
- Specific, actionable goals
- Enhanced learning outcomes

❌ **Trade-offs**:
- More complex agent logic
- Increased validation requirements
- Longer generation time

---

## **Decision Impact Summary**

| ADR | Impact | Complexity | User Benefit |
|-----|---------|------------|--------------|
| ADR-001 | High | Medium | High |
| ADR-002 | Medium | Low | Medium |
| ADR-003 | High | Low | High |
| ADR-004 | Medium | Low | High |
| ADR-005 | High | Medium | High |
| ADR-006 | Medium | High | High |
| ADR-007 | High | High | High |

## **Future Considerations**

### **Potential Future ADRs**:

1. **Agent Performance Optimization**: Parallel agent execution, caching strategies
2. **Multi-Language Support**: Internationalization of agents and content
3. **Adaptive Learning Paths**: Dynamic roadmap adjustment based on user progress
4. **Industry-Specific Agents**: Specialized agents for different domains (Web Dev, Data Science)
5. **User Feedback Integration**: Machine learning from user roadmap ratings and modifications

### **Monitoring Requirements**:
- Track agent response times
- Monitor resource validation success rates
- Measure user satisfaction with generated roadmaps
- Analyze most requested refinements

---

**ADR Status**: ✅ **Current and Complete**  
**Next Review**: December 2025  
**Architecture Stability**: High (no major changes planned)