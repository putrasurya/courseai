using CourseAI.Models;
using CourseAI.Data.Repositories;

namespace CourseAI.Services;

/// <summary>
/// Hybrid service that provides both in-memory and database functionality
/// Maintains backward compatibility while adding database persistence
/// Uses service locator pattern for singleton compatibility with scoped dependencies
/// </summary>
public class HybridLearningProfileService : LearningProfileService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HybridLearningProfileService> _logger;

    public HybridLearningProfileService(
        IServiceProvider serviceProvider,
        ILogger<HybridLearningProfileService> logger) : base()
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
            var profileRepo = scope.ServiceProvider.GetRequiredService<ILearningProfileRepository>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var dbLogger = loggerFactory.CreateLogger<DatabaseLearningProfileService>();
            
            var databaseService = new DatabaseLearningProfileService(profileRepo, dbLogger);
            var profile = await databaseService.GetCurrentProfileAsync();
            
            base.SetProfile(profile);
            _logger.LogInformation("Initialized profile service from database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize from database, using in-memory profile");
        }
    }

    private async Task UpdateDatabaseAsync(Func<DatabaseLearningProfileService, Task> action)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var profileRepo = scope.ServiceProvider.GetRequiredService<ILearningProfileRepository>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var dbLogger = loggerFactory.CreateLogger<DatabaseLearningProfileService>();
            
            var databaseService = new DatabaseLearningProfileService(profileRepo, dbLogger);
            await action(databaseService);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update database");
        }
    }

    public new void UpdateProfile(string field, string value)
    {
        // Update in-memory first
        base.UpdateProfile(field, value);
        
        // Update database asynchronously
        _ = Task.Run(async () => await UpdateDatabaseAsync(async dbService => 
            await dbService.UpdateProfileAsync(field, value)));
        
        _logger.LogDebug("Updated profile field {Field} in both memory and database", field);
    }

    public new void RemoveFromProfile(string field, string value)
    {
        // Remove from in-memory first
        base.RemoveFromProfile(field, value);
        
        // Update database asynchronously
        _ = Task.Run(async () => await UpdateDatabaseAsync(async dbService => 
            await dbService.RemoveFromProfileAsync(field, value)));
        
        _logger.LogDebug("Removed {Value} from field {Field} in both memory and database", value, field);
    }

    public new void SetProfile(LearningProfile profile)
    {
        // Set in-memory first
        base.SetProfile(profile);
        
        // Update database asynchronously
        _ = Task.Run(async () => await UpdateDatabaseAsync(async dbService => 
            await dbService.SetProfileAsync(profile)));
        
        _logger.LogDebug("Set complete profile in both memory and database");
    }

    public new void ClearProfile()
    {
        // Clear in-memory first
        base.ClearProfile();
        
        // Clear database asynchronously
        _ = Task.Run(async () => await UpdateDatabaseAsync(async dbService => 
            await dbService.ClearProfileAsync()));
        
        _logger.LogDebug("Cleared profile in both memory and database");
    }
}