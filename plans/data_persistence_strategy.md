# Data Persistence Strategy for PathWeaver

## Introduction

This document outlines the strategy for data persistence in PathWeaver, detailing how user profiles, generated learning roadmaps, learning resources, and other critical application data will be stored, managed, and accessed. A robust data persistence layer is essential for maintaining user state, progress, and ensuring the long-term functionality of the multi-agent system.

## Database Choice

Given the structured nature of the `Roadmap`, `UserProfile`, `Modules`, `Topics`, and `LearningResources` entities, and the relationships between them, a **relational database** is a strong candidate for PathWeaver.

*   **Primary Choice: PostgreSQL with Entity Framework Core (EF Core)**
    *   **Justification:**
        *   **Strong Schema Enforcement:** Ensures data integrity and consistency for structured data.
        *   **Relational Model:** Perfectly suited for representing the relationships between Users, Roadmaps, Modules, Topics, and Resources.
        *   **Scalability:** PostgreSQL is highly scalable and can handle a large volume of data and concurrent users.
        *   **Reliability & Durability:** ACID compliance ensures data transactions are processed reliably.
        *   **Rich Querying Capabilities:** SQL provides powerful tools for querying and aggregating complex data.
        *   **.NET Ecosystem Integration:** EF Core provides an excellent Object-Relational Mapper (ORM) for .NET applications, simplifying data access and manipulation with C# objects.
        *   **Open Source:** Cost-effective and widely supported.
*   **Alternative/Local Development: SQLite with Entity Framework Core**
    *   **Justification:** Ideal for local development and testing due to its file-based nature, requiring no separate server setup. Can easily be swapped for PostgreSQL in production without significant code changes due to EF Core.

## Data Models

The core data models will closely mirror the conceptual structures defined in `agent_architecture.md`, translated into EF Core entities.

*   **`User` (or `UserProfile`):**
    *   `Id` (Primary Key)
    *   `ExternalUserId` (e.g., from an authentication provider)
    *   `LearningGoal`
    *   `KnownSkills` (JSON or separate entity)
    *   `PreferredLearningStyles` (JSON or separate entity)
    *   `ExperienceLevel`
    *   `Email`, `Username` (for authentication/identification)
    *   `CreatedDate`, `LastLoginDate`

*   **`Roadmap`:**
    *   `Id` (Primary Key)
    *   `UserId` (Foreign Key to `User`)
    *   `Title`
    *   `Description`
    *   `CreatedDate`, `LastModifiedDate`
    *   `Status` (e.g., Draft, AwaitingFeedback, Approved, Active)

*   **`RoadmapModule`:**
    *   `Id` (Primary Key)
    *   `RoadmapId` (Foreign Key to `Roadmap`)
    *   `Title`, `Description`
    *   `Order`
    *   `EstimatedTimeMinutes`

*   **`RoadmapTopic`:**
    *   `Id` (Primary Key)
    *   `RoadmapModuleId` (Foreign Key to `RoadmapModule`)
    *   `Title`, `Description`
    *   `Order`
    *   `LearningObjectives` (JSON or separate entity)

*   **`LearningResource`:**
    *   `Id` (Primary Key)
    *   `RoadmapTopicId` (Foreign Key to `RoadmapTopic` - nullable, as resources might be in `SuggestedResources` first)
    *   `Title`, `Url`, `Type`, `Source`
    *   `Description`
    *   `EstimatedTimeMinutes`
    *   `RelevanceScore`
    *   `IsApprovedByAgent` (boolean)
    *   `IsApprovedByUser` (boolean)
    *   `DiscoveredDate`

*   **`AgentState` (Optional/Advanced):**
    *   Could store specific agent conversation contexts, temporary results, or feedback loops for more complex agent state recovery. For an MVP, this might be managed in-memory.

## Storage Requirements

*   **Structured Data:** The majority of the data (User profiles, Roadmap structure, Resource metadata) is highly structured and fits well into relational tables.
*   **Semi-structured Data:** Fields like `KnownSkills`, `PreferredLearningStyles`, `LearningObjectives` could be stored as JSON columns within PostgreSQL, allowing for flexibility without abandoning the relational model.
*   **Text Data:** Descriptions and summaries can be large text fields. PostgreSQL's `TEXT` type is suitable.
*   **No Binary Blobs (Initially):** We don't anticipate storing binary data (images, videos) directly in the database. Resources will be stored as URLs.

## Read/Write Patterns

*   **Reads:**
    *   Frequent reads of `UserProfile` during user sessions.
    *   Frequent reads of `Roadmap` and its associated `Modules`, `Topics`, and `Resources` when a user views their roadmap.
    *   Searches for resources by the Research Agent.
*   **Writes:**
    *   Writes/updates to `Roadmap` and its sub-entities during roadmap generation and refinement (especially by Structuring and Refinement Agents).
    *   Updates to `UserProfile` by the user.
    *   Insertion of new `LearningResource` entries.

## Scalability & Performance

*   **Database Indexing:** Appropriate indexing on foreign keys and frequently queried columns (e.g., `UserId`, `RoadmapId`, `Title`) will be crucial for query performance.
*   **Connection Pooling:** EF Core and PostgreSQL drivers will utilize connection pooling to efficiently manage database connections.
*   **Lazy vs. Eager Loading:** Strategically use EF Core's lazy or eager loading to fetch only necessary data, optimizing query performance.
*   **Caching:** Implement caching (e.g., in-memory or distributed cache like Redis) for frequently accessed, relatively static data (e.g., user profiles, static parts of a roadmap) to reduce database load.
*   **Database Sharding/Replication (Future):** If the application scales to a very large user base, PostgreSQL supports replication for read scaling and can be sharded (though more complex) for write scaling.

## Backup & Recovery

*   Standard database backup procedures will be implemented (e.g., daily full backups, continuous archiving/WAL for point-in-time recovery).
*   Deployment strategies will ensure high availability for the database.

## Security

*   **Authentication & Authorization:** Access to user-specific data will be strictly controlled through the application's authentication and authorization mechanisms. Users can only access their own roadmaps and profiles.
*   **Data Encryption:**
    *   **In Transit:** Use SSL/TLS for all connections between the application and the database.
    *   **At Rest:** Utilize database-level encryption features provided by the hosting environment or PostgreSQL itself.
*   **Least Privilege:** Database users will be configured with the minimum necessary permissions.
*   **Input Validation:** All data received from the UI or external sources will be thoroughly validated before being stored to prevent injection attacks and ensure data integrity.

This data persistence strategy provides a solid foundation for PathWeaver, ensuring that all valuable information is stored securely, efficiently, and in a way that supports the dynamic nature of the multi-agent system.