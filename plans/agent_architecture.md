## Introduction

This document outlines the current Multi-Agent System (MAS) architecture within PathWeaver, focusing on how agents interact, communicate, and share information to collaboratively generate personalized learning roadmaps. **This architecture is built upon the Microsoft Agent Framework and has evolved through iterative development.**

## Current Agent Architecture (Updated November 2024)

### Core Principles
*   **Single Orchestrator:** All user interactions flow through the OrchestratorAgent
*   **Tool-Based Agents:** Specialized agents serve as tools for higher-level agents
*   **Service-Based State:** Shared data managed through centralized services
*   **Quality Enforcement:** Built-in validation ensures complete roadmaps
*   **Real-Time Feedback:** User receives live status updates during processing

### Agent Hierarchy

```
OrchestratorAgent (Main Entry Point)
├── PlannerAgent (Tool) - Profile gathering and conversation management
├── StructuringAgent (Tool) - Roadmap creation and coordination
│   ├── CurriculumAgent (Tool) - Educational structure consultation
│   ├── PathOptimizationAgent (Tool) - Learning path optimization
│   └── ResourceGatheringAgent (Tool) - Learning resource discovery
└── Shared Services
    ├── UserProfileService - Manages user profile state and tools
    ├── RoadmapService - Manages roadmap state and tools
    └── StatusService - Provides real-time activity notifications
```

## Agent Orchestration

### OrchestratorAgent (Primary Controller)
*   **Role:** Single entry point for all user interactions, conversation flow management
*   **Capabilities:** 
    - User conversation management
    - Profile completion detection
    - Automatic roadmap generation triggering
    - Progress communication to UI
*   **Key Features:**
    - Detects when profile is complete enough for roadmap generation
    - Sends "ROADMAP_COMPLETE" signal to trigger UI navigation
    - Provides roadmap summary before completion
    - Uses PlannerAgent and StructuringAgent as tools

### PlannerAgent (Profile Management)
*   **Role:** User profile gathering through conversational AI
*   **Capabilities:** 
    - Asks clarifying questions about learning goals
    - Extracts and structures user information
    - Uses UserProfileService tools for data management
*   **Integration:** Serves as a tool for OrchestratorAgent

### StructuringAgent (Roadmap Coordination)
*   **Role:** Coordinates roadmap creation using consultant agents
*   **Capabilities:**
    - Uses CurriculumAgent for educational structure advice
    - Uses PathOptimizationAgent for learning path feedback
    - Uses ResourceGatheringAgent for learning materials
    - Constructs roadmap using RoadmapService tools
    - Enforces quality validation (modules have topics, topics have key concepts)
*   **Quality Checks:** 
    - Ensures all modules have topics
    - Ensures all topics have key concepts
    - Ensures all modules have learning resources

### Consultant Agents (Natural Language Advisors)

#### CurriculumAgent
*   **Role:** Educational structure consultation and validation
*   **Capabilities:**
    - Provides natural language feedback on curriculum design
    - Suggests improvements for learning progression
    - Uses web search to validate current industry practices
    - Ensures educational best practices are followed
*   **Output:** Natural language recommendations (not JSON)

#### PathOptimizationAgent
*   **Role:** Learning path optimization consultation
*   **Capabilities:**
    - Analyzes learning sequences for logical flow
    - Suggests reordering or restructuring recommendations
    - Provides feedback on difficulty progression
*   **Output:** Natural language optimization advice

#### ResourceGatheringAgent  
*   **Role:** Learning resource discovery and curation
*   **Capabilities:**
    - Searches web for relevant learning materials
    - Finds courses, tutorials, documentation, and practice resources
    - Evaluates resource quality and relevance
    - Uses Tavily web search API for current resources
*   **Output:** Curated list of learning resources per module

## Data Flow and Shared Services

### Centralized State Management
The system uses service-based architecture for shared state management, following DRY principles:

#### UserProfileService
*   **Purpose:** Centralized UserProfile management
*   **Tools Provided:** 
    - Update learning goals, skills, preferences
    - Check profile completeness
    - Get profile summary
*   **Usage:** Different agents get only the tools they need (selective tool assignment)

#### RoadmapService  
*   **Purpose:** Centralized Roadmap construction and management
*   **Tools Provided:**
    - Create/update modules and topics
    - Add key concepts and resources
    - Get roadmap analysis and summary
    - Quality validation checks
*   **Usage:** StructuringAgent uses these tools to build roadmap incrementally

#### StatusService
*   **Purpose:** Real-time user notification system
*   **Features:** 
    - Agents send status updates during processing
    - UI automatically displays current activity
    - Provides transparency for long-running operations

## Key Architectural Improvements

### Quality Enforcement
*   **Module Validation:** Every module must have topics
*   **Topic Validation:** Every topic must have key concepts  
*   **Resource Validation:** Every module must have learning resources
*   **Automatic Retry:** Agents retry if quality checks fail

### User Experience Enhancements
*   **Live Status Updates:** Users see what agents are doing in real-time
*   **Automatic Navigation:** System automatically moves to roadmap page when complete
*   **Conversational Flow:** Natural back-and-forth until profile is complete
*   **Interactive Chat:** Markdown rendering, auto-scroll, auto-focus

### Technical Integration
*   **Microsoft Agent Framework:** All agents built on MAF foundation
*   **OpenTelemetry:** Full observability and tracing
*   **Tavily Search:** Web search capability for current information
*   **Blazor UI:** Responsive chat interface with component separation

### Eliminated Complexity
*   **No Research Agent:** Replaced with simpler ResourceGatheringAgent
*   **No Sub-Agents:** Removed unnecessary agent hierarchy 
*   **No JSON Responses:** Consultant agents use natural language
*   **No Manual Triggers:** Automatic roadmap generation based on profile completeness

This architecture provides a clean, maintainable, and user-friendly system for generating personalized learning roadmaps through AI agent collaboration.