using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseAI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LearningGoal = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    KnownSkills = table.Column<string>(type: "TEXT", nullable: false),
                    PreferredLearningStyles = table.Column<string>(type: "TEXT", nullable: false),
                    ExperienceLevel = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KnownSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LearningProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Skill = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnownSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnownSkills_LearningProfiles_LearningProfileId",
                        column: x => x.LearningProfileId,
                        principalTable: "LearningProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreferredLearningStyles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LearningProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Style = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferredLearningStyles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreferredLearningStyles_LearningProfiles_LearningProfileId",
                        column: x => x.LearningProfileId,
                        principalTable: "LearningProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roadmaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LearningProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Modules = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roadmaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roadmaps_LearningProfiles_LearningProfileId",
                        column: x => x.LearningProfileId,
                        principalTable: "LearningProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoadmapModules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoadmapId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    EstimatedDuration = table.Column<long>(type: "INTEGER", nullable: false),
                    Topics = table.Column<string>(type: "TEXT", nullable: false),
                    Resources = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoadmapModules_Roadmaps_RoadmapId",
                        column: x => x.RoadmapId,
                        principalTable: "Roadmaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearningResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoadmapModuleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Url = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningResources_RoadmapModules_RoadmapModuleId",
                        column: x => x.RoadmapModuleId,
                        principalTable: "RoadmapModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoadmapTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoadmapModuleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Concepts = table.Column<string>(type: "TEXT", nullable: false),
                    ConfidenceScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoadmapTopics_RoadmapModules_RoadmapModuleId",
                        column: x => x.RoadmapModuleId,
                        principalTable: "RoadmapModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoadmapConcepts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoadmapTopicId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapConcepts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoadmapConcepts_RoadmapTopics_RoadmapTopicId",
                        column: x => x.RoadmapTopicId,
                        principalTable: "RoadmapTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnownSkills_LearningProfileId",
                table: "KnownSkills",
                column: "LearningProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningResources_RoadmapModuleId",
                table: "LearningResources",
                column: "RoadmapModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_PreferredLearningStyles_LearningProfileId",
                table: "PreferredLearningStyles",
                column: "LearningProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapConcepts_RoadmapTopicId",
                table: "RoadmapConcepts",
                column: "RoadmapTopicId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapModules_RoadmapId",
                table: "RoadmapModules",
                column: "RoadmapId");

            migrationBuilder.CreateIndex(
                name: "IX_Roadmaps_LearningProfileId",
                table: "Roadmaps",
                column: "LearningProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapTopics_RoadmapModuleId",
                table: "RoadmapTopics",
                column: "RoadmapModuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KnownSkills");

            migrationBuilder.DropTable(
                name: "LearningResources");

            migrationBuilder.DropTable(
                name: "PreferredLearningStyles");

            migrationBuilder.DropTable(
                name: "RoadmapConcepts");

            migrationBuilder.DropTable(
                name: "RoadmapTopics");

            migrationBuilder.DropTable(
                name: "RoadmapModules");

            migrationBuilder.DropTable(
                name: "Roadmaps");

            migrationBuilder.DropTable(
                name: "LearningProfiles");
        }
    }
}
