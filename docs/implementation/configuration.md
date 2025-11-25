# Configuration Guide - CourseAI Setup

**Last Updated**: November 25, 2025

## 🔧 Required Configuration

### **1. Azure OpenAI Setup**

#### **appsettings.json Configuration**:
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource-name.openai.azure.com/",
    "ApiKey": "your-azure-openai-api-key",
    "ModelDeploymentName": "gpt-4"
  }
}
```

#### **Environment Variables (Alternative)**:
```bash
export AZURE_OPENAI_ENDPOINT="https://your-resource-name.openai.azure.com/"
export AZURE_OPENAI_API_KEY="your-azure-openai-api-key"
export AZURE_OPENAI_MODEL_DEPLOYMENT="gpt-4"
```

### **2. Web Search Integration (Optional)**

#### **Tavily Search API Configuration**:
```json
{
  "TavilyApiKey": "your-tavily-api-key"
}
```

**Benefits with API Key**:
- Real-time resource discovery
- Current tutorial and course recommendations
- Up-to-date industry trend analysis
- Enhanced resource quality and relevance

**Without API Key**:
- System functions with static knowledge
- Recommendations based on training data
- No real-time web search capabilities

### **3. Application Settings**

#### **Complete appsettings.json Example**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.Extensions.AI": "Information"
    }
  },
  "AllowedHosts": "*",
  "AzureOpenAI": {
    "Endpoint": "https://your-resource-name.openai.azure.com/",
    "ApiKey": "your-azure-openai-api-key", 
    "ModelDeploymentName": "gpt-4"
  },
  "TavilyApiKey": "your-tavily-api-key"
}
```

#### **Development Settings (appsettings.Development.json)**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.Extensions.AI": "Debug"
    }
  },
  "DetailedErrors": true,
  "AzureOpenAI": {
    "Endpoint": "https://your-dev-resource.openai.azure.com/",
    "ApiKey": "your-development-api-key",
    "ModelDeploymentName": "gpt-4"
  }
}
```

## 🔐 Security Considerations

### **1. API Key Management**

#### **Production Environment**:
- Use Azure Key Vault for API key storage
- Configure managed identity for Azure resources
- Never commit API keys to source control

#### **Development Environment**:
- Use User Secrets for local development:
```bash
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key"
dotnet user-secrets set "BingSearchApiKey" "your-search-key"
```

### **2. Configuration Options Class**

#### **AzureOpenAIOptions.cs**:
```csharp
public class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";
    
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ModelDeploymentName { get; set; } = string.Empty;
}
```

## 🚀 Azure OpenAI Resource Setup

### **1. Create Azure OpenAI Resource**:
```bash
# Using Azure CLI
az cognitiveservices account create \
    --name "courseai-openai" \
    --resource-group "courseai-rg" \
    --location "East US" \
    --kind "OpenAI" \
    --sku "S0"
```

### **2. Deploy Required Models**:
- **Model**: GPT-4 (recommended) or GPT-3.5-turbo
- **Deployment Name**: Use consistent naming (e.g., "gpt-4")
- **Version**: Latest available

### **3. Configure Access**:
- Enable API access in Azure portal
- Generate API keys
- Configure network access rules if needed

## 🔍 Tavily Search API Setup (Optional)

### **1. Create Tavily Account**:
- Visit [tavily.com](https://tavily.com)
- Sign up for a developer account
- Choose appropriate plan for your usage

### **2. Get API Key**:
- Navigate to your Tavily dashboard
- Find API Keys section
- Copy your API key for configuration

## 🛠️ Local Development Setup

### **1. Prerequisites**:
- .NET 8 SDK
- Visual Studio 2022 or VS Code
- Azure OpenAI access

### **2. Setup Steps**:
```bash
# Clone repository
git clone <repository-url>
cd CourseAI

# Configure user secrets (recommended for development)
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:Endpoint" "your-endpoint"
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-key"
dotnet user-secrets set "AzureOpenAI:ModelDeploymentName" "gpt-4"
dotnet user-secrets set "TavilyApiKey" "your-search-key"

# Restore dependencies
dotnet restore

# Run application
dotnet run --project src/CourseAI
```

### **3. Verify Configuration**:
- Navigate to `https://localhost:5001`
- Start a conversation with the system
- Verify AI responses are working

## 📊 OpenTelemetry Configuration (Optional)

### **Aspire Integration**:
```csharp
// Program.cs - Already configured
var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService("CourseAI");

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddSource("*Microsoft.Extensions.AI")
    .AddSource("*Microsoft.Extensions.Agents*")
    .AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"))
    .Build();
```

## ⚠️ Troubleshooting

### **Common Issues**:

#### **1. Azure OpenAI Authentication Error**:
```
Error: Unable to authenticate with Azure OpenAI
```
**Solution**: 
- Verify endpoint URL format
- Check API key validity
- Ensure model deployment exists

#### **2. Web Search Not Working**:
```
Warning: Web search capabilities disabled
```
**Solution**:
- Add Tavily API key to configuration
- Verify API key has valid subscription
- Check API usage limits and quotas

#### **3. Agent Registration Error**:
```
Error: Unable to resolve service for type 'IAgentName'
```
**Solution**:
- Verify agent is registered in Program.cs
- Check interface and implementation match
- Ensure proper dependency injection setup

### **Configuration Validation**:
Add this to verify your setup:
```csharp
// In Program.cs - Add before app.Run()
var azureOptions = app.Services.GetRequiredService<IOptions<AzureOpenAIOptions>>();
if (string.IsNullOrEmpty(azureOptions.Value.Endpoint))
{
    throw new InvalidOperationException("Azure OpenAI endpoint not configured");
}
```

---

**Configuration Status**: ✅ **Complete Guide**  
**Security Level**: 🔒 **Production Ready**  
**Setup Time**: ~15 minutes with Azure resources