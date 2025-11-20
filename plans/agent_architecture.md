## Introduction

This document outlines the high-level architecture for the Multi-Agent System (MAS) within PathWeaver, focusing on how different agents interact, communicate, and share information to collaboratively generate personalized learning roadmaps. **This architecture is built upon the Microsoft Agent Framework.**

## Agent Interaction Model

The agent interaction model is based on the core components and patterns provided by the Microsoft Agent Framework.

*   **Agents:** Our specialized agents (Planner, Research, Structuring, Refinement) will be implemented as instances of the framework's agent abstraction. Each agent is configured with a specific set of instructions and has access to the framework's core services.
*   **Agent Capabilities (Tools/Functions):** The functionalities of our agents are exposed as capabilities (sometimes referred to as tools or functions) that an agent can invoke. These capabilities can be implemented as native code functions or through prompts to an underlying LLM. For example, the Research Agent would have capabilities for web searching or summarizing content.
*   **Orchestration:** Agent collaboration is achieved through an orchestrator pattern. A primary "Orchestrator Agent" can manage and invoke other specialized agents, passing context and instructions between them. This allows for a structured and auditable flow of control, where the orchestrator directs the workflow (e.g., orchestrator instructs Planner Agent, then instructs Research Agent based on Planner's output). The framework's context management and interaction history facilitate this communication.
*   **Shared Context:** Context is managed through the arguments passed between agent invocations and potentially a shared session state maintained by the framework. Each agent invocation receives the necessary information (like the user profile or a list of resources) and returns its output, which the orchestrator then processes or passes to the next agent in the chain.

## Agent Orchestration

A central orchestrator component, likely implemented as a dedicated agent within the Microsoft Agent Framework, will manage the overall workflow.

*   **Orchestrator Agent:** A top-level agent will manage the overall process of roadmap creation.
*   **Workflow Management:** The orchestrator will invoke the specialized agents in sequence, for example:
    1.  Interact with the **Planner Agent** to get the `UserProfile`.
    2.  Use the `UserProfile` to invoke the **Research Agent**.
    3.  Pass the research results to the **Structuring Agent**.
    4.  Present the draft to the user and invoke the **Refinement Agent** with user feedback.
*   **State Management:** The orchestrator is responsible for maintaining the high-level state of the roadmap generation process, gathering the outputs from each agent to build the final `Roadmap` object. The framework's interaction history provides a built-in ledger of all agent activities.

## Data Flow and Shared Data Structures

The core shared data structure will be the `Roadmap` object, which will evolve throughout the agent interaction process. This object will be assembled by the Orchestrator Agent based on the outputs of the specialized agents.

### `Roadmap` Data Structure (Conceptual)

```csharp
public class Roadmap
{
    public Guid Id { get; set; }
    public UserProfile UserProfile { get; set; } // User's goals, existing knowledge, preferences
    public List<RoadmapModule> Modules { get; set; } = new List<RoadmapModule>();
    public List<LearningResource> SuggestedResources { get; set; } = new List<LearningResource>();
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public RoadmapStatus Status { get; set; } // e.g., Draft, AwaitingFeedback, Approved, Active
    // Other properties for overall roadmap context
}

public class UserProfile
{
    public string LearningGoal { get; set; }
    public List<string> KnownSkills { get; set; }
    public List<string> PreferredLearningStyles { get set; } // e.g., "visual", "hands-on", "reading"
    public string ExperienceLevel { get; set; } // e.g., "beginner", "intermediate", "expert"
    // Other user-specific data
}

public class RoadmapModule
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int Order { get; set; }
    public List<RoadmapTopic> Topics { get; set; } = new List<RoadmapTopic>();
    // Estimated time, prerequisites, etc.
}

public class RoadmapTopic
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int Order { get; set; }
    public List<LearningResource> Resources { get; set; } = new List<LearningResource>();
    // Learning objectives, keywords, etc.
}

public class LearningResource
{
    public string Title { get; set; }
    public string Url { get; set; }
    public string Type { get; set; } // e.g., "article", "video", "course", "book", "project"
    public string Source { get; set; } // e.g., "YouTube", "Coursera", "Medium"
    public string Description { get; set; }
    public int EstimatedTimeMinutes { get; set; }
    public double RelevanceScore { get; set; } // How relevant is this to the topic/user profile
}
```

## High-Level Agent Responsibilities and Interactions

### 1. Planner Agent

*   **Role:** User Interface, Goal Elicitation, Initial Context Setup. Implemented as an agent within the Microsoft Agent Framework.
*   **Capabilities:** Utilizes functions within its domain to ask clarifying questions and summarize the user's profile.
*   **Process:** Engages in a direct interaction with the user, orchestrated by the main application or a top-level agent.
*   **Output:** A structured `UserProfile` object.

### 2. Research Agent

*   **Role:** Information Retrieval, Resource Curation. Implemented as an agent within the Microsoft Agent Framework.
*   **Capabilities:** Equipped with functions for web searches, summarizing content from external sources, and evaluating resource quality.
*   **Input:** `UserProfile` and specific topics to research.
*   **Output:** A list of `LearningResource` objects.

### 3. Structuring Agent

*   **Role:** Roadmap Generation, Content Organization. Implemented as an agent within the Microsoft Agent Framework.
*   **Capabilities:** Uses sophisticated functions to analyze resources, identify themes, and organize them into a logical sequence of `RoadmapModule` and `RoadmapTopic` objects.
*   **Input:** `UserProfile` and a list of `LearningResource`s.
*   **Output:** A structured list of `RoadmapModule`s.

### 4. Refinement Agent

*   **Role:** User Feedback Integration, Iteration Management. Implemented as an agent within the Microsoft Agent Framework.
*   **Capabilities:** Has functions to interpret natural language feedback from the user and translate it into actionable directives (e.g., "regenerate_module", "find_more_resources", "change_topic_order").
*   **Input:** A drafted `Roadmap` structure and user feedback.
*   **Output:** A set of instructions for the Orchestrator Agent to re-invoke other agents with new parameters.

This architectural overview provides a foundation for the next steps in designing and implementing the PathWeaver system.