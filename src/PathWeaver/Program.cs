using PathWeaver.Agents;
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

// Register application services
builder.Services.AddScoped<RoadmapStateService>();
builder.Services.AddSingleton<UserProfileService>();

builder.Services.AddSingleton<IPlannerAgent, PlannerAgent>();
builder.Services.AddSingleton<IResearchAgent, ResearchAgent>();
builder.Services.AddSingleton<IStructuringAgent, StructuringAgent>();
builder.Services.AddSingleton<IRefinementAgent, RefinementAgent>();
builder.Services.AddSingleton<IOrchestratorAgent, OrchestratorAgent>();

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
