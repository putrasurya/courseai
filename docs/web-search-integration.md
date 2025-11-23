# Web Search Integration

The CourseAI application now includes web search capabilities to enhance the quality and relevance of generated learning roadmaps.

## Configuration

To enable web search functionality, add your Bing Search API key to the configuration:

### appsettings.json / appsettings.Development.json
```json
{
  "BingSearchApiKey": "your-bing-search-api-key-here"
}
```

## Features

### Agents with Web Search Capabilities

1. **CurriculumArchitectAgent**
   - `SearchEducationalContent`: Find tutorials and learning materials
   - `SearchBestPractices`: Research current industry standards

2. **ContentDiscoveryAgent** (via ResearchAgent)
   - `SearchEducationalContent`: Discover online courses and tutorials
   - `SearchWeb`: General web search for learning resources

3. **SkillMappingAgent** (via ResearchAgent)
   - `SearchBestPractices`: Research current industry requirements
   - `SearchWeb`: Find job market trends and skill demands

## How It Works

- **Without API Key**: Agents will provide helpful suggestions and recommendations based on their training data
- **With API Key**: Agents can search the web in real-time for current tutorials, courses, best practices, and industry trends

## Benefits

- **Current Information**: Access to up-to-date tutorials and learning resources
- **Industry Relevance**: Real-time insights into job market demands and skill requirements
- **Comprehensive Coverage**: Broader range of learning materials and resources
- **Quality Enhancement**: Better curriculum design based on current educational content

The web search integration enhances the agents' ability to create more relevant and current learning roadmaps while maintaining full functionality even without an API key configured.