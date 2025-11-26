using CourseAI.Models;
using CourseAI.Data.Repositories;
using System.ComponentModel;

namespace CourseAI.Services;

/// <summary>
/// Hybrid service that provides both in-memory and database functionality
/// Maintains backward compatibility while adding database persistence
/// Uses service locator pattern for singleton compatibility with scoped dependencies
/// </summary>
public class HybridRoadmapService : RoadmapService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HybridRoadmapService> _logger;

    public HybridRoadmapService(
        IServiceProvider serviceProvider,
        IAgentStatusService statusService,
        ILogger<HybridRoadmapService> logger) : base(statusService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // Initialize from database
        _ = Task.Run(async () => await InitializeFromDatabaseAsync());
    }

    private async Task InitializeFromDatabaseAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var roadmapRepo = scope.ServiceProvider.GetRequiredService<IRoadmapRepository>();
            var profileRepo = scope.ServiceProvider.GetRequiredService<ILearningProfileRepository>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var dbLogger = loggerFactory.CreateLogger<DatabaseRoadmapService>();
            
            var databaseService = new DatabaseRoadmapService(roadmapRepo, profileRepo, dbLogger);
            var roadmap = await databaseService.GetCurrentRoadmapAsync();
            
            if (roadmap != null)
            {
                base.SetRoadMap(roadmap);
                _logger.LogInformation("Initialized roadmap service from database with roadmap {RoadmapId}", roadmap.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize roadmap from database, using in-memory roadmap");
        }
    }

    private async Task UpdateDatabaseAsync(Func<DatabaseRoadmapService, Task> action)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var roadmapRepo = scope.ServiceProvider.GetRequiredService<IRoadmapRepository>();
            var profileRepo = scope.ServiceProvider.GetRequiredService<ILearningProfileRepository>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var dbLogger = loggerFactory.CreateLogger<DatabaseRoadmapService>();
            
            var databaseService = new DatabaseRoadmapService(roadmapRepo, profileRepo, dbLogger);
            await action(databaseService);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update database");
        }
    }

    private async Task<T> QueryDatabaseAsync<T>(Func<DatabaseRoadmapService, Task<T>> query)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var roadmapRepo = scope.ServiceProvider.GetRequiredService<IRoadmapRepository>();
            var profileRepo = scope.ServiceProvider.GetRequiredService<ILearningProfileRepository>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var dbLogger = loggerFactory.CreateLogger<DatabaseRoadmapService>();
            
            var databaseService = new DatabaseRoadmapService(roadmapRepo, profileRepo, dbLogger);
            return await query(databaseService);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query database");
            return default!;
        }
    }

    public new void SetRoadMap(Roadmap roadmap)
    {
        // Set in-memory first
        base.SetRoadMap(roadmap);
        
        // Update database asynchronously
        _ = Task.Run(async () => await UpdateDatabaseAsync(async dbService => 
            await dbService.SetRoadMapAsync(roadmap)));
        
        _logger.LogDebug("Set roadmap in both memory and database");
    }

    public new string InitializeRoadMap(string userProfileSummary)
    {
        // Initialize in database first, then sync to memory
        var result = string.Empty;
        var task = Task.Run(async () => 
        {
            result = await QueryDatabaseAsync(async dbService => 
                await dbService.InitializeRoadMapAsync(userProfileSummary));
                
            var roadmap = await QueryDatabaseAsync(async dbService => 
                await dbService.GetCurrentRoadmapAsync());
                
            if (roadmap != null)
            {
                base.SetRoadMap(roadmap);
            }
        });
        
        task.Wait(); // Wait for database operation to complete
        return result;
    }

    public new string AddModule(string title, string description, int estimatedDurationHours)
    {
        // Add to database first, then sync to memory
        var result = string.Empty;
        var task = Task.Run(async () => 
        {
            result = await QueryDatabaseAsync(async dbService => 
                await dbService.AddModuleAsync(title, description, estimatedDurationHours));
                
            var roadmap = await QueryDatabaseAsync(async dbService => 
                await dbService.GetCurrentRoadmapAsync());
                
            if (roadmap != null)
            {
                base.SetRoadMap(roadmap);
            }
        });
        
        task.Wait(); // Wait for database operation to complete
        return result;
    }

    public new string UpdateRoadMapStatus(RoadmapStatus status)
    {
        // Update database first, then sync to memory
        var result = string.Empty;
        var task = Task.Run(async () => 
        {
            result = await QueryDatabaseAsync(async dbService => 
                await dbService.UpdateRoadMapStatusAsync(status));
                
            var roadmap = await QueryDatabaseAsync(async dbService => 
                await dbService.GetCurrentRoadmapAsync());
                
            if (roadmap != null)
            {
                base.SetRoadMap(roadmap);
            }
        });
        
        task.Wait(); // Wait for database operation to complete
        return result;
    }
}