using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CourseAI.Models;

public class CourseAIDbContext : DbContext
{
    public CourseAIDbContext(DbContextOptions<CourseAIDbContext> options) : base(options)
    {
    }

    public DbSet<LearningProfile> LearningProfiles { get; set; }
    public DbSet<KnownSkill> KnownSkills { get; set; }
    public DbSet<PreferredLearningStyle> PreferredLearningStyles { get; set; }
    public DbSet<Roadmap> Roadmaps { get; set; }
    public DbSet<RoadmapModule> RoadmapModules { get; set; }
    public DbSet<RoadmapTopic> RoadmapTopics { get; set; }
    public DbSet<RoadmapConcept> RoadmapConcepts { get; set; }
    public DbSet<LearningResource> LearningResources { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // LearningProfile configuration
        modelBuilder.Entity<LearningProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.LearningGoal).HasMaxLength(500);
            entity.Property(e => e.ExperienceLevel).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("datetime('now')");
            
            // Configure complex properties as JSON for backward compatibility
            entity.Property(e => e.KnownSkills)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<string?>>(v, (JsonSerializerOptions)null!) ?? new List<string?>());
                    
            entity.Property(e => e.PreferredLearningStyles)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<string?>>(v, (JsonSerializerOptions)null!) ?? new List<string?>());
        });

        // KnownSkill configuration
        modelBuilder.Entity<KnownSkill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Skill).HasMaxLength(200);
            
            entity.HasOne(e => e.LearningProfile)
                .WithMany()
                .HasForeignKey(e => e.LearningProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PreferredLearningStyle configuration
        modelBuilder.Entity<PreferredLearningStyle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Style).HasMaxLength(200);
            
            entity.HasOne(e => e.LearningProfile)
                .WithMany()
                .HasForeignKey(e => e.LearningProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Roadmap configuration
        modelBuilder.Entity<Roadmap>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Status).HasConversion<string>();
            
            entity.HasOne(e => e.LearningProfile)
                .WithMany(p => p.Roadmaps)
                .HasForeignKey(e => e.LearningProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure complex properties as JSON for backward compatibility
            entity.Property(e => e.Modules)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<RoadmapModule>>(v, (JsonSerializerOptions)null!) ?? new List<RoadmapModule>());
        });

        // RoadmapModule configuration
        modelBuilder.Entity<RoadmapModule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.EstimatedDuration).HasConversion(
                v => v.Ticks,
                v => new TimeSpan(v));
            
            entity.HasOne(e => e.Roadmap)
                .WithMany()
                .HasForeignKey(e => e.RoadmapId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure complex properties as JSON for backward compatibility
            entity.Property(e => e.Topics)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<RoadmapTopic>>(v, (JsonSerializerOptions)null!) ?? new List<RoadmapTopic>());
                    
            entity.Property(e => e.Resources)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<LearningResource>>(v, (JsonSerializerOptions)null!) ?? new List<LearningResource>());
        });

        // RoadmapTopic configuration
        modelBuilder.Entity<RoadmapTopic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.Property(e => e.Description).HasMaxLength(1000);
            
            entity.HasOne(e => e.RoadmapModule)
                .WithMany()
                .HasForeignKey(e => e.RoadmapModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure complex properties as JSON for backward compatibility
            entity.Property(e => e.Concepts)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<RoadmapConcept>>(v, (JsonSerializerOptions)null!) ?? new List<RoadmapConcept>());
        });

        // RoadmapConcept configuration
        modelBuilder.Entity<RoadmapConcept>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.Property(e => e.Description).HasMaxLength(1000);
            
            entity.HasOne(e => e.RoadmapTopic)
                .WithMany()
                .HasForeignKey(e => e.RoadmapTopicId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // LearningResource configuration
        modelBuilder.Entity<LearningResource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.Property(e => e.Url).HasMaxLength(1000);
            entity.Property(e => e.Source).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Type).HasConversion<string>();
            
            entity.HasOne(e => e.RoadmapModule)
                .WithMany()
                .HasForeignKey(e => e.RoadmapModuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}