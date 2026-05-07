using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRealTimeBattle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChallengerPetIds",
                table: "BattleSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChallengerTotalScore",
                table: "BattleSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentRound",
                table: "BattleSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OpponentPetIds",
                table: "BattleSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OpponentTotalScore",
                table: "BattleSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BattleRounds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BattleSessionId = table.Column<int>(type: "int", nullable: false),
                    RoundIndex = table.Column<int>(type: "int", nullable: false),
                    VocabularyId = table.Column<int>(type: "int", nullable: false),
                    P1Score = table.Column<int>(type: "int", nullable: true),
                    P1AnswerMs = table.Column<int>(type: "int", nullable: true),
                    P1Correct = table.Column<bool>(type: "bit", nullable: false),
                    P1Answer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    P2Score = table.Column<int>(type: "int", nullable: true),
                    P2AnswerMs = table.Column<int>(type: "int", nullable: true),
                    P2Correct = table.Column<bool>(type: "bit", nullable: false),
                    P2Answer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DamageDealt = table.Column<int>(type: "int", nullable: false),
                    DamagedPlayer = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattleRounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BattleRounds_BattleSessions_BattleSessionId",
                        column: x => x.BattleSessionId,
                        principalTable: "BattleSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BattleRounds_Vocabularies_VocabularyId",
                        column: x => x.VocabularyId,
                        principalTable: "Vocabularies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GymLeaderPets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GymLeaderId = table.Column<int>(type: "int", nullable: false),
                    PetId = table.Column<int>(type: "int", nullable: false),
                    SlotIndex = table.Column<int>(type: "int", nullable: false),
                    Nickname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BotAccuracy = table.Column<double>(type: "float", nullable: false),
                    BotAvgResponseMs = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GymLeaderPets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GymLeaderPets_GymLeaders_GymLeaderId",
                        column: x => x.GymLeaderId,
                        principalTable: "GymLeaders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GymLeaderPets_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BattlePetStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BattleSessionId = table.Column<int>(type: "int", nullable: false),
                    PlayerIndex = table.Column<int>(type: "int", nullable: false),
                    SlotIndex = table.Column<int>(type: "int", nullable: false),
                    UserOwnedPetId = table.Column<int>(type: "int", nullable: true),
                    GymLeaderPetId = table.Column<int>(type: "int", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxHp = table.Column<int>(type: "int", nullable: false),
                    CurrentHp = table.Column<int>(type: "int", nullable: false),
                    IsFainted = table.Column<bool>(type: "bit", nullable: false),
                    FaintedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattlePetStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BattlePetStates_BattleSessions_BattleSessionId",
                        column: x => x.BattleSessionId,
                        principalTable: "BattleSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BattlePetStates_GymLeaderPets_GymLeaderPetId",
                        column: x => x.GymLeaderPetId,
                        principalTable: "GymLeaderPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BattlePetStates_UserOwnedPets_UserOwnedPetId",
                        column: x => x.UserOwnedPetId,
                        principalTable: "UserOwnedPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 101,
                column: "Description",
                value: "Defeated Brock, Pewter City Gym Leader");

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Defeated Misty, Cerulean City Gym Leader", "Cascade Badge" });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Defeated Lt. Surge, Vermilion Gym Leader", "Thunder Badge" });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Defeated Erika, Celadon Gym Leader", "Rainbow Badge" });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Defeated Koga, Fuchsia Gym Leader", "Soul Badge" });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 106,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Defeated Sabrina, Saffron Gym Leader", "Marsh Badge" });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 107,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Defeated Blaine, Cinnabar Gym Leader", "Volcano Badge" });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 108,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Defeated Giovanni, Viridian Gym Leader", "Earth Badge" });

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

            migrationBuilder.UpdateData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name", "Theme", "Title" },
                values: new object[] { "Brock tests your fundamentals. Solid like rock, his battle demands strong basics.", "Brock", 14, "Rock-Type Master" });

            migrationBuilder.UpdateData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BadgeName", "Description", "Name", "Theme", "Title" },
                values: new object[] { "Cascade Badge", "Misty flows like water. Adaptability is key to overcoming her strategies.", "Misty", 3, "Water-Type Specialist" });

            migrationBuilder.UpdateData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BadgeName", "Description", "Name", "Theme", "Title" },
                values: new object[] { "Thunder Badge", "Fast and explosive, Lt. Surge overwhelms unprepared challengers.", "Lt. Surge", 4, "Lightning American" });

            migrationBuilder.UpdateData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "BadgeName", "Description", "Name", "Theme", "Title" },
                values: new object[] { "Rainbow Badge", "Erika’s calm style hides dangerous precision. Patience wins this match.", "Erika", 1, "Nature-Loving Princess" });

            migrationBuilder.UpdateData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "BadgeName", "Description", "Name", "Theme", "Title" },
                values: new object[] { "Soul Badge", "Koga uses deception and speed. One mistake and it's over.", "Koga", 15, "Poison Ninja Master" });

            migrationBuilder.UpdateData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "BadgeName", "Description", "Name", "Theme", "Title" },
                values: new object[] { "Marsh Badge", "Sabrina reads your moves before you make them. Precision is mandatory.", "Sabrina", 9, "Psychic Master" });

            migrationBuilder.UpdateData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "BadgeName", "Description", "Name", "Theme", "Title" },
                values: new object[] { "Volcano Badge", "Blaine combines knowledge and battle. Expect tricky questions and heat.", "Blaine", 13, "Fire-Type Quiz Master" });

            migrationBuilder.UpdateData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "BadgeName", "Description", "Name", "Theme", "Title" },
                values: new object[] { "Earth Badge", "Giovanni is the ultimate test. Strategy, power, and mastery decide victory.", "Giovanni", 14, "Team Rocket Boss" });

            migrationBuilder.CreateIndex(
                name: "IX_BattlePetStates_BattleSessionId",
                table: "BattlePetStates",
                column: "BattleSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_BattlePetStates_GymLeaderPetId",
                table: "BattlePetStates",
                column: "GymLeaderPetId");

            migrationBuilder.CreateIndex(
                name: "IX_BattlePetStates_UserOwnedPetId",
                table: "BattlePetStates",
                column: "UserOwnedPetId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleRounds_BattleSessionId_RoundIndex",
                table: "BattleRounds",
                columns: new[] { "BattleSessionId", "RoundIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BattleRounds_VocabularyId",
                table: "BattleRounds",
                column: "VocabularyId");

            migrationBuilder.CreateIndex(
                name: "IX_GymLeaderPets_GymLeaderId_SlotIndex",
                table: "GymLeaderPets",
                columns: new[] { "GymLeaderId", "SlotIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GymLeaderPets_PetId",
                table: "GymLeaderPets",
                column: "PetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BattlePetStates");

            migrationBuilder.DropTable(
                name: "BattleRounds");

            migrationBuilder.DropTable(
                name: "GymLeaderPets");

            migrationBuilder.DropColumn(
                name: "ChallengerPetIds",
                table: "BattleSessions");

            migrationBuilder.DropColumn(
                name: "ChallengerTotalScore",
                table: "BattleSessions");

            migrationBuilder.DropColumn(
                name: "CurrentRound",
                table: "BattleSessions");

            migrationBuilder.DropColumn(
                name: "OpponentPetIds",
                table: "BattleSessions");

            migrationBuilder.DropColumn(
                name: "OpponentTotalScore",
                table: "BattleSessions");

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 101,
                column: "Description",
                value: "Defeated Norm, Guardian of Daily Life");

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Defeated Flora, Guardian of Nature", "Leaf Badge" });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Defeated Hail, Guardian of Weather", "Frost Badge" });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Defeated Marina, Guardian of Food", "Tide Badge" });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 105,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Defeated Volt, Guardian of Technology", "Spark Badge" });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 106,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Defeated Aero, Guardian of Travel", "Wing Badge" });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 107,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Defeated Lumi, Guardian of Health", "Glow Badge" });

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 108,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Defeated Brawl, Guardian of Sports", "Iron Badge" });

            migrationBuilder.UpdateData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name", "Theme", "Title" },
                values: new object[] { "Norm greets every newcomer with a warm smile. Her words are simple, but mastering them is the foundation of your journey.", "Norm", 0, "Guardian of Daily Life" });

            migrationBuilder.UpdateData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BadgeName", "Description", "Name", "Theme", "Title" },
                values: new object[] { "Leaf Badge", "Flora speaks in the language of forests and living things. She rewards those who have truly internalized the world around them.", "Flora", 1, "Guardian of Nature" });

            migrationBuilder.UpdateData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BadgeName", "Description", "Name", "Theme", "Title" },
                values: new object[] { "Frost Badge", "Hail's temperament shifts like the wind. Only those who can describe the sky in all its moods can earn her trust.", "Hail", 2, "Guardian of Weather" });

            migrationBuilder.UpdateData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "BadgeName", "Description", "Name", "Theme", "Title" },
                values: new object[] { "Tide Badge", "Marina believes language is best shared over a meal. Prove to her you can navigate any kitchen or restaurant conversation.", "Marina", 3, "Guardian of Food" });

            migrationBuilder.UpdateData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "BadgeName", "Description", "Name", "Theme", "Title" },
                values: new object[] { "Spark Badge", "Volt moves at the speed of electricity. Only the digitally fluent can keep up with his rapid-fire tech vocabulary.", "Volt", 4, "Guardian of Technology" });

            migrationBuilder.UpdateData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "BadgeName", "Description", "Name", "Theme", "Title" },
                values: new object[] { "Wing Badge", "Aero has circled the globe many times over. She tests your ability to navigate the world — literally and linguistically.", "Aero", 5, "Guardian of Travel" });

            migrationBuilder.UpdateData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "BadgeName", "Description", "Name", "Theme", "Title" },
                values: new object[] { "Glow Badge", "Lumi radiates calm and wisdom. She demands precision — the language of health leaves no room for misunderstanding.", "Lumi", 6, "Guardian of Health" });

            migrationBuilder.UpdateData(
                table: "GymLeaders",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "BadgeName", "Description", "Name", "Theme", "Title" },
                values: new object[] { "Iron Badge", "Brawl is the ultimate test of endurance. This battle will push your B2 vocabulary to the limit — no shortcuts allowed.", "Brawl", 7, "Guardian of Sports" });
        }
    }
}
