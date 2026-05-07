using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGymLeaderSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GymLeaders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BadgeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BadgeImageUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BadgeAchievementId = table.Column<int>(type: "int", nullable: true),
                    Theme = table.Column<int>(type: "int", nullable: false),
                    RequiredCefrLevel = table.Column<int>(type: "int", nullable: false),
                    GymOrder = table.Column<int>(type: "int", nullable: false),
                    XpThreshold = table.Column<int>(type: "int", nullable: false),
                    VocabThreshold = table.Column<int>(type: "int", nullable: false),
                    RequiredMemoryState = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    XpReward = table.Column<int>(type: "int", nullable: false),
                    QuestionCount = table.Column<int>(type: "int", nullable: false),
                    PassRatePercent = table.Column<int>(type: "int", nullable: false),
                    CooldownHours = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GymLeaders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GymLeaders_Achievements_BadgeAchievementId",
                        column: x => x.BadgeAchievementId,
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BattleSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChallengerUserId = table.Column<int>(type: "int", nullable: false),
                    OpponentUserId = table.Column<int>(type: "int", nullable: true),
                    GymLeaderId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    ChallengerCorrect = table.Column<int>(type: "int", nullable: false),
                    OpponentCorrect = table.Column<int>(type: "int", nullable: false),
                    ChallengerWon = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattleSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BattleSessions_GymLeaders_GymLeaderId",
                        column: x => x.GymLeaderId,
                        principalTable: "GymLeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BattleSessions_Users_ChallengerUserId",
                        column: x => x.ChallengerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BattleSessions_Users_OpponentUserId",
                        column: x => x.OpponentUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserGymProgresses",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GymLeaderId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalAttempts = table.Column<int>(type: "int", nullable: false),
                    LastAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DefeatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BestScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGymProgresses", x => new { x.UserId, x.GymLeaderId });
                    table.ForeignKey(
                        name: "FK_UserGymProgresses_GymLeaders_GymLeaderId",
                        column: x => x.GymLeaderId,
                        principalTable: "GymLeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserGymProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BattleAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BattleSessionId = table.Column<int>(type: "int", nullable: false),
                    VocabularyId = table.Column<int>(type: "int", nullable: false),
                    ChallengerAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChallengerIsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    ChallengerAnsweredMs = table.Column<int>(type: "int", nullable: false),
                    OpponentAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OpponentIsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    OpponentAnsweredMs = table.Column<int>(type: "int", nullable: false),
                    QuestionOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattleAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BattleAnswers_BattleSessions_BattleSessionId",
                        column: x => x.BattleSessionId,
                        principalTable: "BattleSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BattleAnswers_Vocabularies_VocabularyId",
                        column: x => x.VocabularyId,
                        principalTable: "Vocabularies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "Id", "ConditionType", "ConditionValue", "CreatedAt", "Description", "ItemId", "Name", "RewardItemId" },
                values: new object[,]
                {
                    { 101, 4, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Defeated Norm, Guardian of Daily Life", null, "Boulder Badge", 0 },
                    { 102, 4, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Defeated Flora, Guardian of Nature", null, "Leaf Badge", 0 },
                    { 103, 4, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Defeated Hail, Guardian of Weather", null, "Frost Badge", 0 },
                    { 104, 4, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Defeated Marina, Guardian of Food", null, "Tide Badge", 0 },
                    { 105, 4, 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Defeated Volt, Guardian of Technology", null, "Spark Badge", 0 },
                    { 106, 4, 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Defeated Aero, Guardian of Travel", null, "Wing Badge", 0 },
                    { 107, 4, 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Defeated Lumi, Guardian of Health", null, "Glow Badge", 0 },
                    { 108, 4, 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Defeated Brawl, Guardian of Sports", null, "Iron Badge", 0 }
                });

            migrationBuilder.InsertData(
                table: "GymLeaders",
                columns: new[] { "Id", "AvatarUrl", "BadgeAchievementId", "BadgeImageUrl", "BadgeName", "CooldownHours", "Description", "GymOrder", "Name", "PassRatePercent", "QuestionCount", "RequiredCefrLevel", "RequiredMemoryState", "Theme", "Title", "VocabThreshold", "XpReward", "XpThreshold" },
                values: new object[,]
                {
                    { 1, null, 101, null, "Boulder Badge", 12, "Norm greets every newcomer with a warm smile. Her words are simple, but mastering them is the foundation of your journey.", 1, "Norm", 80, 15, 0, "Learning", 0, "Guardian of Daily Life", 15, 150, 300 },
                    { 2, null, 102, null, "Leaf Badge", 12, "Flora speaks in the language of forests and living things. She rewards those who have truly internalized the world around them.", 2, "Flora", 80, 15, 0, "Review", 1, "Guardian of Nature", 15, 200, 600 },
                    { 3, null, 103, null, "Frost Badge", 12, "Hail's temperament shifts like the wind. Only those who can describe the sky in all its moods can earn her trust.", 3, "Hail", 80, 15, 1, "Learning", 2, "Guardian of Weather", 20, 250, 1000 },
                    { 4, null, 104, null, "Tide Badge", 12, "Marina believes language is best shared over a meal. Prove to her you can navigate any kitchen or restaurant conversation.", 4, "Marina", 80, 15, 1, "Review", 3, "Guardian of Food", 20, 300, 1500 },
                    { 5, null, 105, null, "Spark Badge", 12, "Volt moves at the speed of electricity. Only the digitally fluent can keep up with his rapid-fire tech vocabulary.", 5, "Volt", 80, 15, 2, "Learning", 4, "Guardian of Technology", 25, 400, 2500 },
                    { 6, null, 106, null, "Wing Badge", 12, "Aero has circled the globe many times over. She tests your ability to navigate the world — literally and linguistically.", 6, "Aero", 80, 15, 2, "Review", 5, "Guardian of Travel", 25, 500, 3500 },
                    { 7, null, 107, null, "Glow Badge", 12, "Lumi radiates calm and wisdom. She demands precision — the language of health leaves no room for misunderstanding.", 7, "Lumi", 80, 15, 2, "Review", 6, "Guardian of Health", 30, 600, 5000 },
                    { 8, null, 108, null, "Iron Badge", 12, "Brawl is the ultimate test of endurance. This battle will push your B2 vocabulary to the limit — no shortcuts allowed.", 8, "Brawl", 80, 15, 3, "Learning", 7, "Guardian of Sports", 30, 800, 7000 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BattleAnswers_BattleSessionId",
                table: "BattleAnswers",
                column: "BattleSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleAnswers_VocabularyId",
                table: "BattleAnswers",
                column: "VocabularyId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleSessions_ChallengerUserId_Status",
                table: "BattleSessions",
                columns: new[] { "ChallengerUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BattleSessions_GymLeaderId",
                table: "BattleSessions",
                column: "GymLeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleSessions_OpponentUserId",
                table: "BattleSessions",
                column: "OpponentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GymLeaders_BadgeAchievementId",
                table: "GymLeaders",
                column: "BadgeAchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGymProgresses_GymLeaderId",
                table: "UserGymProgresses",
                column: "GymLeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGymProgresses_UserId_Status",
                table: "UserGymProgresses",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BattleAnswers");

            migrationBuilder.DropTable(
                name: "UserGymProgresses");

            migrationBuilder.DropTable(
                name: "BattleSessions");

            migrationBuilder.DropTable(
                name: "GymLeaders");

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 108);
        }
    }
}
