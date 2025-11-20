# PathWeaver: A Multi-Agent System for Personalized Learning Roadmaps

## Vision

To create an AI-powered system that assists users in generating, refining, and navigating personalized learning roadmaps on any topic. PathWeaver will leverage a multi-agent approach to deliver comprehensive and tailored learning paths that adapt to the user's unique goals, prior knowledge, and learning style.

## Core Concepts

*   **Multi-Agent System (MAS):** The system will be composed of several specialized AI agents, each responsible for a specific task in the roadmap creation process. This modular approach allows for greater flexibility, scalability, and expertise in each step.
*   **Personalized Learning Roadmaps:** Unlike generic learning paths, PathWeaver will generate roadmaps that are tailored to the individual user. This includes considering their existing skills, learning preferences (e.g., theoretical vs. project-based), and the specific goals they want to achieve.
*   **Interactive and Iterative:** The process of creating a learning roadmap is a conversation. Users can interact with the system to provide feedback, ask for modifications, and refine the generated path until it meets their needs.

## Agent Roles

The PathWeaver system will be comprised of the following specialized agents:

1.  **Planner Agent:**
    *   **Responsibilities:** User interaction, goal clarification, and initial planning.
    *   **Function:** This agent is the primary interface with the user. It's responsible for understanding the user's high-level goals (e.g., "I want to learn machine learning"), and asking clarifying questions to gather necessary details (e.g., "What's your current math and programming background?", "Are you interested in a specific application of ML?").

2.  **Research Agent:**
    *   **Responsibilities:** Information gathering and resource discovery.
    *   **Function:** Based on the user's goals defined by the Planner Agent, the Research Agent will scour the internet, academic papers, and other resources to find the best learning materials. This includes articles, tutorials, online courses, books, and open-source projects.

3.  **Structuring Agent:**
    *   **Responsibilities:** Organizing information and creating the roadmap structure.
    *   **Function:** This agent takes the vast amount of information gathered by the Research Agent and organizes it into a coherent, step-by-step learning path. It will define modules, sequence topics logically, and create a structured curriculum.

4.  **Refinement Agent:**
    *   **Responsibilities:** User feedback integration and roadmap iteration.
    *   **Function:** The Refinement Agent presents the drafted roadmap to the user and facilitates the feedback process. It interprets the user's feedback (e.g., "This section is too basic for me," or "I want more hands-on projects") and coordinates with the other agents to modify the roadmap accordingly.

## User Interaction Flow

1.  **Phase 1: Goal Setting:**
    *   The user states their learning objective.
    *   The **Planner Agent** engages in a dialogue to understand the user's needs, background, and preferences.

2.  **Phase 2: Information Gathering:**
    *   The **Research Agent** is dispatched to find relevant learning resources based on the refined goals.

3.  **Phase 3: Roadmap Generation:**
    *   The **Structuring Agent** receives the curated resources and organizes them into a logical and easy-to-follow roadmap.

4.  **Phase 4: Refinement and Iteration:**
    *   The **Refinement Agent** presents the initial roadmap to the user.
    *   The user provides feedback, and the agents collaborate to update the roadmap in real-time. This loop continues until the user is satisfied.

5.  **Phase 5: Learning and Tracking (Future):**
    *   Once the roadmap is finalized, the user can begin their learning journey.
    *   Future versions of PathWeaver could include features to track progress, provide reminders, and suggest supplementary materials.

## Technical Stack

*   **Backend:** C# with .NET
*   **Agent Framework:** Microsoft Agent Framework
*   **Frontend:** Blazor
*   **AI/LLM:** A powerful Large Language Model (LLM)

## Development Milestones (MVP)

1.  **Milestone 1: Agent Framework Setup:**
    *   Set up and configure the Microsoft Agent Framework.
    *   Define the core agent roles and their functions within the framework.

2.  **Milestone 2: Foundational Agents:**
    *   Implement the initial versions of the **Planner Agent** and **Research Agent** using the Microsoft Agent Framework.

3.  **Milestone 3: Basic Roadmap Generation:**
    *   Implement the **Structuring Agent** to produce a non-interactive, text-based roadmap.

4.  **Milestone 4: Interactive Frontend:**
    *   Build a simple Blazor-based web interface for user input and to display the generated roadmap.

5.  **Milestone 5: Refinement Loop:**
    *   Implement the **Refinement Agent** and the user feedback mechanism to allow for iterative roadmap creation.

## Future Ideas

*   **Progress Tracking:** Visual indicators to show users how far they've progressed on their learning journey.
*   **Learning Platform Integration:** Link directly to courses on platforms like Coursera, edX, or Udemy.
*   **Community Features:** Allow users to share and discover roadmaps created by others.
*   **Gamification:** Incorporate points, badges, and other motivational elements to encourage learning.
