using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBattleAnswerAndSeedProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BattleAnswers");

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "GymLeaderPets",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 8);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BattleAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BattleSessionId = table.Column<int>(type: "int", nullable: false),
                    VocabularyId = table.Column<int>(type: "int", nullable: false),
                    ChallengerAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChallengerAnsweredMs = table.Column<int>(type: "int", nullable: false),
                    ChallengerIsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    OpponentAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OpponentAnsweredMs = table.Column<int>(type: "int", nullable: false),
                    OpponentIsCorrect = table.Column<bool>(type: "bit", nullable: false),
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
                    { 101, 4, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Defeated Brock, Pewter City Gym Leader", null, "Boulder Badge", 0 },
                    { 102, 4, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Defeated Misty, Cerulean City Gym Leader", null, "Cascade Badge", 0 },
                    { 103, 4, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Defeated Lt. Surge, Vermilion Gym Leader", null, "Thunder Badge", 0 },
                    { 104, 4, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Defeated Erika, Celadon Gym Leader", null, "Rainbow Badge", 0 },
                    { 105, 4, 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Defeated Koga, Fuchsia Gym Leader", null, "Soul Badge", 0 },
                    { 106, 4, 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Defeated Sabrina, Saffron Gym Leader", null, "Marsh Badge", 0 },
                    { 107, 4, 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Defeated Blaine, Cinnabar Gym Leader", null, "Volcano Badge", 0 },
                    { 108, 4, 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Defeated Giovanni, Viridian Gym Leader", null, "Earth Badge", 0 }
                });

            migrationBuilder.InsertData(
                table: "GymLeaders",
                columns: new[] { "Id", "AvatarUrl", "BadgeAchievementId", "BadgeImageUrl", "BadgeName", "CooldownHours", "Description", "GymOrder", "Name", "PassRatePercent", "QuestionCount", "RequiredCefrLevel", "RequiredMemoryState", "Theme", "Title", "VocabThreshold", "XpReward", "XpThreshold" },
                values: new object[,]
                {
                    { 1, null, 101, null, "Boulder Badge", 12, "Brock tests your fundamentals. Solid like rock, his battle demands strong basics.", 1, "Brock", 80, 15, 0, "Learning", 14, "Rock-Type Master", 15, 150, 300 },
                    { 2, null, 102, null, "Cascade Badge", 12, "Misty flows like water. Adaptability is key to overcoming her strategies.", 2, "Misty", 80, 15, 0, "Review", 3, "Water-Type Specialist", 15, 200, 600 },
                    { 3, null, 103, null, "Thunder Badge", 12, "Fast and explosive, Lt. Surge overwhelms unprepared challengers.", 3, "Lt. Surge", 80, 15, 1, "Learning", 4, "Lightning American", 20, 250, 1000 },
                    { 4, null, 104, null, "Rainbow Badge", 12, "Erika’s calm style hides dangerous precision. Patience wins this match.", 4, "Erika", 80, 15, 1, "Review", 1, "Nature-Loving Princess", 20, 300, 1500 },
                    { 5, null, 105, null, "Soul Badge", 12, "Koga uses deception and speed. One mistake and it's over.", 5, "Koga", 80, 15, 2, "Learning", 15, "Poison Ninja Master", 25, 400, 2500 },
                    { 6, null, 106, null, "Marsh Badge", 12, "Sabrina reads your moves before you make them. Precision is mandatory.", 6, "Sabrina", 80, 15, 2, "Review", 9, "Psychic Master", 25, 500, 3500 },
                    { 7, null, 107, null, "Volcano Badge", 12, "Blaine combines knowledge and battle. Expect tricky questions and heat.", 7, "Blaine", 80, 15, 2, "Review", 13, "Fire-Type Quiz Master", 30, 600, 5000 },
                    { 8, null, 108, null, "Earth Badge", 12, "Giovanni is the ultimate test. Strategy, power, and mastery decide victory.", 8, "Giovanni", 80, 15, 3, "Learning", 14, "Team Rocket Boss", 30, 800, 7000 }
                });

            migrationBuilder.InsertData(
                table: "GymLeaderPets",
                columns: new[] { "Id", "BotAccuracy", "BotAvgResponseMs", "GymLeaderId", "Nickname", "PetId", "SlotIndex" },
                values: new object[,]
                {
                    { 1, 0.55000000000000004, 7000, 1, null, 50, 0 },
                    { 2, 0.55000000000000004, 7000, 1, null, 51, 1 },
                    { 3, 0.55000000000000004, 7000, 1, null, 52, 2 },
                    { 4, 0.59999999999999998, 6500, 2, null, 53, 0 },
                    { 5, 0.59999999999999998, 6500, 2, null, 54, 1 },
                    { 6, 0.59999999999999998, 6500, 2, null, 55, 2 },
                    { 7, 0.65000000000000002, 6000, 3, null, 56, 0 },
                    { 8, 0.65000000000000002, 6000, 3, null, 57, 1 },
                    { 9, 0.65000000000000002, 6000, 3, null, 58, 2 },
                    { 10, 0.69999999999999996, 5500, 4, null, 59, 0 },
                    { 11, 0.69999999999999996, 5500, 4, null, 60, 1 },
                    { 12, 0.69999999999999996, 5500, 4, null, 61, 2 },
                    { 13, 0.75, 5000, 5, null, 62, 0 },
                    { 14, 0.75, 5000, 5, null, 63, 1 },
                    { 15, 0.75, 5000, 5, null, 64, 2 },
                    { 16, 0.80000000000000004, 4500, 6, null, 65, 0 },
                    { 17, 0.80000000000000004, 4500, 6, null, 66, 1 },
                    { 18, 0.80000000000000004, 4500, 6, null, 67, 2 },
                    { 19, 0.84999999999999998, 3500, 7, null, 68, 0 },
                    { 20, 0.84999999999999998, 3500, 7, null, 69, 1 },
                    { 21, 0.84999999999999998, 3500, 7, null, 70, 2 },
                    { 22, 0.90000000000000002, 3000, 8, null, 71, 0 },
                    { 23, 0.90000000000000002, 3000, 8, null, 72, 1 },
                    { 24, 0.90000000000000002, 3000, 8, null, 73, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BattleAnswers_BattleSessionId",
                table: "BattleAnswers",
                column: "BattleSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleAnswers_VocabularyId",
                table: "BattleAnswers",
                column: "VocabularyId");
        }
    }
}
