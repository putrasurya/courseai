# Testing Strategy for CourseAI

## Introduction

This document outlines the comprehensive testing strategy for the CourseAI multi-agent system, built upon the **Microsoft Agent Framework**. Given the complexity introduced by multiple interacting agents and Large Language Model (LLM) integration, a layered approach to testing is essential to ensure the system's correctness, reliability, performance, and security.

## Testing Levels

### 1. Unit Tests

*   **Purpose:** To verify the correctness of individual, isolated components (methods, functions, classes).
*   **Scope:** Each agent's internal logic, utility functions, data model operations, service methods.
*   **Frameworks:** NUnit or xUnit (preferred for .NET projects).
*   **Methodology:**
    *   Tests should be fast, isolated, and repeatable.
    *   Use mocking frameworks (e.g., Moq) to isolate units under test from their dependencies (e.g., LLM clients, database repositories, external APIs).
    *   Focus on covering all branches and edge cases within individual code units.

### 2. Integration Tests

*   **Purpose:** To verify the interactions between different components and external services.
*   **Scope:**
    *   **Agent-to-Agent Communication:** Ensure messages are correctly sent, received, and processed between agents (e.g., Planner -> Research, Structuring -> Refinement).
    *   **Agent-to-LLM Integration:** Verify that prompts are correctly constructed, sent to the LLM, and responses are parsed as expected. This will involve more nuanced testing of capability engineering.
    *   **Application-to-Database:** Ensure EF Core mappings are correct, and data can be successfully persisted and retrieved.
    *   **Agent-to-External APIs:** Verify interactions with any external research APIs or learning platforms.
*   **Frameworks:** NUnit or xUnit, often using a real (but controlled) instance of the external service or a test double that simulates realistic behavior more closely than a mock.
*   **Methodology:**
    *   For database integration, use in-memory databases (e.g., SQLite for EF Core tests) or clean test databases that are reset for each test run.
    *   For LLM integration, use controlled, deterministic inputs and expect specific (or range-bound) outputs where possible. Consider using LLM "snapshot" testing where a known good output is compared to current output, with allowances for minor variations.

### 3. End-to-End (E2E) Tests

*   **Purpose:** To simulate real user scenarios and verify that the entire system (frontend to backend, including all integrations) works as a cohesive unit.
*   **Scope:**
    *   User onboarding (goal setting with Planner Agent).
    *   Roadmap generation and refinement loop.
    *   Saving and loading roadmaps.
    *   Authentication and authorization flows.
*   **Frameworks:** Playwright (preferred for Blazor applications), Selenium, or Cypress.
*   **Methodology:**
    *   Tests should run against a deployed (or locally running) instance of the application.
    *   Automate UI interactions to mimic user actions.
    *   Validate the displayed UI elements and the resulting backend state.
    *   These tests are typically slower and more brittle but provide high confidence in the overall system.

### 4. LLM-Specific Testing / Evaluation

*   **Purpose:** To evaluate the quality, relevance, factual accuracy, and coherence of LLM-generated content.
*   **Scope:** Outputs from Planner (questions, summaries), Research (resource summaries, query generation), Structuring (module/topic generation, descriptions), and Refinement (feedback interpretation, suggested changes).
*   **Frameworks/Tools:**
    *   **Human-in-the-Loop Evaluation:** Manual review of LLM outputs for quality and relevance is paramount.
    *   **Automated Metrics:** For certain aspects, metrics like ROUGE (for summarization), BLEU (for text generation), or custom similarity scores can be used, though these have limitations for open-ended generation.
    *   **Prompt/Capability Configuration Validation Tests:** Ensure that changes to prompts/capability configurations do not degrade the quality or format of LLM responses.
    *   **Regression Suites:** Maintain a suite of specific prompts and their expected "good" (human-validated) responses to catch regressions.
*   **Methodology:**
    *   Develop a dataset of diverse user prompts and expected roadmap outcomes.
    *   Regularly run LLMs against this dataset and evaluate the output, potentially flagging deviations for human review.
    *   Establish clear criteria for what constitutes a "good" LLM response for each agent's task.

## Testing Frameworks and Tools (Summary)

*   **.NET Unit/Integration Tests:** NUnit/xUnit, Moq
*   **E2E Tests:** Playwright
*   **LLM Evaluation:** Custom scripts, human evaluation, potentially specialized LLM evaluation libraries.

## Test Data Management

*   **Synthetic Data:** Generate realistic but synthetic user profiles, learning goals, and resource data for robust testing.
*   **Seeding Database:** Use EF Core migrations and seeding capabilities to populate test databases with known good data.
*   **LLM Prompt Examples:** Maintain a versioned collection of example prompts and expected LLM outputs for integration and LLM-specific tests.
*   **Anonymization:** Ensure any real user data used for testing (if unavoidable) is properly anonymized and compliant with privacy regulations.

## Mocking and Stubbing

*   **LLM APIs:** Crucial for unit and most integration tests. Mock the LLM client to return predefined responses, allowing tests to run quickly and deterministically without incurring API costs or network latency.
*   **Database:** Use in-memory databases or repository pattern with in-memory implementations for faster unit tests.
*   **External Research APIs:** Mock or stub these APIs to control their responses during testing.

## CI/CD Integration

*   **Automated Test Execution:** All unit, integration, and E2E tests will be integrated into the Continuous Integration (CI) pipeline.
*   **Pre-Commit/Pre-Merge Hooks:** Fast unit tests should run before code is committed or merged to ensure immediate feedback.
*   **Deployment Gates:** Successful execution of all test suites (including E2E) will be a prerequisite for code deployment to staging or production environments.

## Performance Testing

*   **Purpose:** To ensure the system can handle anticipated load and identify performance bottlenecks.
*   **Scope:** Agent processing times, LLM response times, database query performance, UI responsiveness under load.
*   **Tools:** Apache JMeter, K6, or custom load testing scripts.
*   **Methodology:** Simulate concurrent users and measure response times, throughput, and resource utilization.

## Security Testing

*   **Purpose:** To identify vulnerabilities and ensure the system is protected against common attacks.
*   **Scope:** Authentication, authorization, data encryption, input validation, API security.
*   **Methodology:**
    *   Regular security audits and code reviews.
    *   Automated static application security testing (SAST) and dynamic application security testing (DAST) tools.
    *   Penetration testing (manual and automated).

By adopting this comprehensive testing strategy, CourseAI aims to deliver a high-quality, reliable, and secure personalized learning roadmap solution.