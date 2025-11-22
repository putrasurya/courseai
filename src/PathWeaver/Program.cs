using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using PathWeaver.Agents;
using PathWeaver.Agents.Interfaces;
using PathWeaver.Components;
using PathWeaver.Models;
using PathWeaver.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
    });

// Configure Azure OpenAI options
builder.Services.Configure<AzureOpenAIOptions>(
    builder.Configuration.GetSection(AzureOpenAIOptions.SectionName));

// Register Instrumented Chat Client
builder.Services.AddSingleton<InstrumentChatClient>();

// Register Agent Status Service
builder.Services.AddSingleton<IAgentStatusService, AgentStatusService>();

// Register application services
builder.Services.AddSingleton<UserProfileService>();
builder.Services.AddSingleton<UserProfileToolsService>();
builder.Services.AddSingleton<RoadmapService>();

// Register core agents
builder.Services.AddSingleton<IPlannerAgent, PlannerAgent>();
builder.Services.AddSingleton<IRefinementAgent, RefinementAgent>();

// Register main workflow agents
builder.Services.AddSingleton<IStructuringAgent, StructuringAgent>();

// Register specialized sub-agents (used as tools by main agents)
builder.Services.AddSingleton<IResearchAgent, ResearchAgent>();
builder.Services.AddSingleton<IContentDiscoveryAgent, ContentDiscoveryAgent>();
builder.Services.AddSingleton<ISkillMappingAgent, SkillMappingAgent>();
builder.Services.AddSingleton<IResourceEvaluationAgent, ResourceEvaluationAgent>();
builder.Services.AddSingleton<ICurriculumArchitectAgent, CurriculumArchitectAgent>();
builder.Services.AddSingleton<IPathOptimizationAgent, PathOptimizationAgent>();
builder.Services.AddSingleton<IExperienceDesignAgent, ExperienceDesignAgent>();

// Register main coordinator
builder.Services.AddSingleton<IOrchestratorAgent, OrchestratorAgent>();

// Configure OpenTelemetry and Aspire
var resourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddService("MyApplication");

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(resourceBuilder)
    .AddSource("MyApplication")
    .AddSource("*Microsoft.Extensions.AI") // Listen to the Experimental.Microsoft.Extensions.AI source for chat client telemetry
    .AddSource("*Microsoft.Extensions.Agents*") // Listen to the Experimental.Microsoft.Extensions.Agents source for agent telemetry
    .AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"))
    .Build();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
