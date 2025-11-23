using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using CourseAI.Models;
using CourseAI.Services;

namespace CourseAI.Agents;

public class InstrumentChatClient
{
    public IChatClient ChatClient { get; init; }
    public InstrumentChatClient(IOptions<AzureOpenAIOptions> options)
    {
        var azureOptions = options.Value;
        
        ChatClient = new AzureOpenAIClient(new Uri(azureOptions.Endpoint), new AzureCliCredential())
            .GetChatClient(azureOptions.ModelName)
            .AsIChatClient() // Converts a native OpenAI SDK ChatClient into a Microsoft.Extensions.AI.IChatClient
            .AsBuilder()
            .UseOpenTelemetry(sourceName: "MyApplication", configure: (cfg) => cfg.EnableSensitiveData = true)    // Enable OpenTelemetry instrumentation with sensitive data
            .Build();;
    }
}