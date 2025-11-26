using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using CourseAI.Agents;
using CourseAI.Agents.Interfaces;
using CourseAI.Components;
using CourseAI.Models;
using CourseAI.Services;
using CourseAI.Data;
using CourseAI.Data.Repositories;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
    });

// Configure Entity Framework with SQLite
builder.Services.AddDbContext<CourseAIDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                     "Data Source=Data/courseai.db"));

// Register repositories
builder.Services.AddScoped<ILearningProfileRepository, LearningProfileRepository>();
builder.Services.AddScoped<IRoadmapRepository, RoadmapRepository>();

// Configure Azure OpenAI options
builder.Services.Configure<AzureOpenAIOptions>(
    builder.Configuration.GetSection(AzureOpenAIOptions.SectionName));

// Register Instrumented Chat Client
builder.Services.AddSingleton<InstrumentChatClient>();

// Register Agent Status Service
builder.Services.AddSingleton<IAgentStatusService, AgentStatusService>();

// Register hybrid services that provide both in-memory and database persistence
builder.Services.AddSingleton<HybridLearningProfileService>();
builder.Services.AddSingleton<HybridRoadmapService>();

// Register services for agents (using hybrid services for backward compatibility)
builder.Services.AddSingleton<LearningProfileService>(provider => 
    provider.GetRequiredService<HybridLearningProfileService>());
builder.Services.AddSingleton<RoadmapService>(provider => 
    provider.GetRequiredService<HybridRoadmapService>());

builder.Services.AddSingleton<LearningProfileToolsService>();

builder.Services.AddHttpClient<WebSearchService>();
builder.Services.AddSingleton<WebSearchService>();

// Register core agents
builder.Services.AddSingleton<IPlannerAgent, PlannerAgent>();
builder.Services.AddSingleton<IRefinementAgent, RefinementAgent>();

// Register main workflow agents
builder.Services.AddSingleton<IStructuringAgent, StructuringAgent>();

// Register specialized sub-agents (used as tools by main agents)
builder.Services.AddSingleton<ResourceGatheringAgent>();
builder.Services.AddSingleton<ICurriculumArchitectAgent, CurriculumArchitectAgent>();
builder.Services.AddSingleton<IPathOptimizationAgent, PathOptimizationAgent>();

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

// Ensure database is created and apply any pending migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CourseAIDbContext>();
    context.Database.EnsureCreated();
}

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
