using Microsoft.EntityFrameworkCore.Migrations;

namespace Anima.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Создание таблицы APIKeys
            migrationBuilder.CreateTable(
                name: "APIKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KeyValue = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Permissions = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    UsageCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastUsedFromIp = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    KeyHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsRevoked = table.Column<bool>(type: "INTEGER", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RevokedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APIKeys", x => x.Id);
                });

            // Создание таблицы Memories
            migrationBuilder.CreateTable(
                name: "Memories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MemoryType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Importance = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AccessCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    InstanceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EmotionalValence = table.Column<double>(type: "REAL", nullable: false),
                    EmotionalArousal = table.Column<double>(type: "REAL", nullable: false),
                    AssociatedMemories = table.Column<string>(type: "TEXT", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsConsolidated = table.Column<bool>(type: "INTEGER", nullable: false),
                    MemorySource = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memories", x => x.Id);
                });

            // Создание таблицы EmotionStates
            migrationBuilder.CreateTable(
                name: "EmotionStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmotionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Intensity = table.Column<double>(type: "REAL", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Context = table.Column<string>(type: "TEXT", nullable: true),
                    Trigger = table.Column<string>(type: "TEXT", nullable: true),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    InstanceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PreviousIntensity = table.Column<double>(type: "REAL", nullable: true),
                    EmotionCategory = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsStable = table.Column<bool>(type: "INTEGER", nullable: false),
                    PhysiologicalData = table.Column<string>(type: "TEXT", nullable: true),
                    Emotion = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmotionStates", x => x.Id);
                });

            // Создание таблицы Goals
            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InstanceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ParentGoalId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Priority = table.Column<double>(type: "REAL", nullable: false),
                    Progress = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.Id);
                });

            // Создание индексов для оптимизации производительности
            migrationBuilder.CreateIndex(
                name: "IX_APIKeys_KeyValue",
                table: "APIKeys",
                column: "KeyValue",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_APIKeys_IsActive",
                table: "APIKeys",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Memories_InstanceId",
                table: "Memories",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Memories_MemoryType",
                table: "Memories",
                column: "MemoryType");

            migrationBuilder.CreateIndex(
                name: "IX_Memories_CreatedAt",
                table: "Memories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmotionStates_InstanceId",
                table: "EmotionStates",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_EmotionStates_Timestamp",
                table: "EmotionStates",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_InstanceId",
                table: "Goals",
                column: "InstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_Status",
                table: "Goals",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "APIKeys");
            migrationBuilder.DropTable(name: "Memories");
            migrationBuilder.DropTable(name: "EmotionStates");
            migrationBuilder.DropTable(name: "Goals");
        }
    }
} 