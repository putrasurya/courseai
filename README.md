# CourseAI - AI Learning Roadmap Generator 🤖

> **Create personalized learning roadmaps through simple conversation with AI.**

## 💡 The Problem It Solves

**Learning something new** shouldn't be overwhelming:

❌ **"Where do I even start?"**  
❌ **"Which resources are actually good?"**  
❌ **"What's the right order to learn things?"**  

**CourseAI eliminates this confusion:**
✅ **Personalized learning paths** tailored to your level  
✅ **Quality-vetted resources** from across the web  
✅ **Logical progression** based on educational theory  
✅ **Conversational interface** that adapts to your goals  

## 🎯 How It Works

1. **💬 Tell It What You Want to Learn**
   - "I want to become a front-end developer"
   - "Help me learn machine learning as a beginner"

2. **📋 Get Your Complete Learning Roadmap**
   - Step-by-step modules with clear objectives
   - Curated resources (videos, courses, docs)
   - Time estimates and prerequisites

3. **🔄 Refine and Improve**
   - "Add more hands-on practice"
   - "I prefer video tutorials"

## 🚀 Quick Start

```bash
git clone https://github.com/putrasurya/courseai
cd CourseAI
dotnet run --project src/CourseAI
```

Visit `https://localhost:5001` to start creating your learning roadmap!

## 🛠️ Technology Stack

- **.NET 8** - Modern web framework
- **Blazor Server** - Interactive UI with real-time updates
- **Azure OpenAI** - GPT-4 integration
- **Microsoft Agent Framework** - AI agent coordination
- **Tavily Search** - Real-time web resource discovery

---

## ⚙️ Setup

### Required Configuration
```json
{
  "AzureOpenAI": {
    "Endpoint": "your-azure-openai-endpoint",
    "ApiKey": "your-api-key", 
    "ModelDeploymentName": "gpt-4"
  },
  "TavilyApiKey": "your-tavily-api-key"
}
```

## 📚 Documentation

- **[Architecture Overview](docs/architecture/current-architecture.md)** - System design and AI agents
- **[Development Guide](docs/development/getting-started.md)** - Setup and development workflow
- **[Configuration](docs/implementation/configuration.md)** - Detailed setup instructions

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.

