using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WordSoul.Application.DTOs.Battle;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Infrastructure.Persistence
{
    /// <summary>
    /// Triển khai IArenaBattleService – xử lý toàn bộ state machine Battle real-time.
    /// PvE: bot giả lập tự động submit câu trả lời dựa trên BotAccuracy + BotAvgResponseMs.
    /// </summary>
    public class ArenaBattleService : IArenaBattleService
    {
        private readonly WordSoulDbContext _db;
        private readonly ILogger<ArenaBattleService> _logger;
        private readonly IMemoryCache _cache;
        private static readonly Random _rng = new();

        public ArenaBattleService(WordSoulDbContext db, ILogger<ArenaBattleService> logger, IMemoryCache cache)
        {
            _db = db;
            _logger = logger;
            _cache = cache;
        }

        // ═══════════════════════════════════════════════════════════
        // CREATE SESSION (gọi từ REST API)
        // ═══════════════════════════════════════════════════════════

        public async Task<int> CreateSessionAsync(
            StartArenaBattleRequestDto dto, int userId, CancellationToken ct = default)
        {
            // 1. Validate: người chơi phải chọn đúng 3 pet
            if (dto.SelectedPetIds.Count != 3)
                throw new InvalidOperationException("Bạn phải chọn đúng 3 Pokémon.");

            // 2. Validate: tất cả pet phải thuộc user
            var ownedPets = await _db.UserOwnedPets
                .AsNoTracking()
                .Where(uop => uop.UserId == userId
                              && dto.SelectedPetIds.Contains(uop.PetId))
                .Include(uop => uop.Pet)
                .ToListAsync(ct);
            
            // Lấy 1 record UserOwnedPet đại diện cho mỗi PetId (nếu user có nhiều instance trùng PetId)
            ownedPets = ownedPets.GroupBy(p => p.PetId).Select(g => g.First()).ToList();

            if (ownedPets.Count != 3)
                throw new InvalidOperationException("Một hoặc nhiều Pokémon không hợp lệ hoặc không thuộc bạn.");

            // 3. Validate: user đã unlock Gym
            var gymProgress = await _db.UserGymProgresses
                .AsNoTracking()
                .FirstOrDefaultAsync(ugp => ugp.UserId == userId && ugp.GymLeaderId == dto.GymLeaderId, ct);

            if (gymProgress == null || gymProgress.Status == GymStatus.Locked)
                throw new InvalidOperationException("Gym này chưa được mở khóa.");

            // 4. Load Gym + GymLeaderPets
            var gym = await _db.GymLeaders
                .Include(gl => gl.GymLeaderPets).ThenInclude(gp => gp.Pet)
                .FirstOrDefaultAsync(gl => gl.Id == dto.GymLeaderId, ct)
                ?? throw new KeyNotFoundException($"Gym {dto.GymLeaderId} không tồn tại.");

            if (gym.GymLeaderPets.Count < 3)
                throw new InvalidOperationException("Gym Leader chưa có đủ 3 Pokémon trong hệ thống.");

            // 5. Chọn câu hỏi
            var vocabs = await SelectBattleVocabsAsync(userId, gym, ct);
            if (vocabs.Count == 0)
                throw new InvalidOperationException("Không đủ từ vựng để bắt đầu trận đấu.");

            // 6. Tạo BattleSession
            var session = new BattleSession
            {
                ChallengerUserId = userId,
                GymLeaderId = gym.Id,
                Type = BattleType.GymBattle,
                Status = BattleStatus.InProgress,
                StartedAt = DateTime.UtcNow,
                TotalQuestions = vocabs.Count,
                ChallengerPetIds = JsonSerializer.Serialize(ownedPets.Select(p => p.Id).ToList()),
                OpponentPetIds = JsonSerializer.Serialize(gym.GymLeaderPets.OrderBy(p => p.SlotIndex).Select(p => p.Id).ToList())
            };

            _db.BattleSessions.Add(session);
            await _db.SaveChangesAsync(ct);

            // 7. Lưu vocab choices vào BattleRounds (chỉ VocabularyId, kết quả điền sau)
            for (int i = 0; i < vocabs.Count; i++)
            {
                _db.BattleRounds.Add(new BattleRound
                {
                    BattleSessionId = session.Id,
                    RoundIndex = i,
                    VocabularyId = vocabs[i].Id,
                    StartedAt = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("ArenaBattle session {S} created for user {U} vs gym {G}", session.Id, userId, gym.Id);
            return session.Id;
        }

        // ═══════════════════════════════════════════════════════════
        // START BATTLE (gọi từ SignalR hub khi player ready)
        // ═══════════════════════════════════════════════════════════

        public async Task<BattleStartedDto?> StartBattleAsync(
            int sessionId, int userId, CancellationToken ct = default)
        {
            var session = await _db.BattleSessions
                .Include(s => s.GymLeader).ThenInclude(gl => gl!.GymLeaderPets).ThenInclude(p => p.Pet)
                .Include(s => s.Rounds.OrderBy(r => r.RoundIndex))
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.ChallengerUserId == userId, ct);

            if (session == null || session.Status != BattleStatus.InProgress) return null;

            // Tạo BattlePetStates nếu chưa có
            var existingStates = await _db.BattlePetStates
                .Where(bps => bps.BattleSessionId == sessionId)
                .ToListAsync(ct);

            if (existingStates.Count == 0)
            {
                // P1: người chơi
                var p1PetIds = JsonSerializer.Deserialize<List<int>>(session.ChallengerPetIds ?? "[]")!;
                var p1Pets = await _db.UserOwnedPets
                    .Include(uop => uop.Pet)
                    .Where(uop => p1PetIds.Contains(uop.Id))
                    .ToListAsync(ct);

                for (int i = 0; i < p1PetIds.Count; i++)
                {
                    var pet = p1Pets.First(p => p.Id == p1PetIds[i]);
                    int maxHp = CalculateMaxHp(pet.Pet!.Rarity, pet.Level);
                    _db.BattlePetStates.Add(new BattlePetState
                    {
                        BattleSessionId = sessionId,
                        PlayerIndex = 1,
                        SlotIndex = i,
                        UserOwnedPetId = pet.Id,
                        DisplayName = pet.Pet?.Name ?? "?",
                        ImageUrl = pet.Pet?.ImageUrl,
                        PetType = pet.Pet!.Type,
                        SecondaryPetType = pet.Pet.SecondaryType,
                        MaxHp = maxHp, CurrentHp = maxHp
                    });
                }

                // P2: bot (GymLeaderPets)
                var gymPets = session.GymLeader!.GymLeaderPets.OrderBy(p => p.SlotIndex).ToList();
                for (int i = 0; i < gymPets.Count; i++)
                {
                    int maxHp = CalculateMaxHp(gymPets[i].Pet!.Rarity, gymPets[i].Level);
                    _db.BattlePetStates.Add(new BattlePetState
                    {
                        BattleSessionId = sessionId,
                        PlayerIndex = 2,
                        SlotIndex = i,
                        GymLeaderPetId = gymPets[i].Id,
                        DisplayName = gymPets[i].Nickname ?? gymPets[i].Pet?.Name ?? "?",
                        ImageUrl = gymPets[i].Pet?.ImageUrl,
                        PetType = gymPets[i].Pet!.Type,
                        SecondaryPetType = gymPets[i].Pet?.SecondaryType,
                        MaxHp = maxHp, CurrentHp = maxHp
                    });
                }
                await _db.SaveChangesAsync(ct);
            }

            // Load states
            var petStates = await _db.BattlePetStates
                .Where(bps => bps.BattleSessionId == sessionId)
                .ToListAsync(ct);

            var firstRound = session.Rounds.First();
            var firstVocab = await _db.Vocabularies.FindAsync([firstRound.VocabularyId], ct);

            return new BattleStartedDto
            {
                BattleSessionId = sessionId,
                TotalRounds = session.TotalQuestions,
                P1Pets = petStates.Where(p => p.PlayerIndex == 1).OrderBy(p => p.SlotIndex)
                    .Select(MapPetState).ToList(),
                P2Pets = petStates.Where(p => p.PlayerIndex == 2).OrderBy(p => p.SlotIndex)
                    .Select(MapPetState).ToList(),
                FirstQuestion = BuildRoundQuestion(firstRound, firstVocab!,
                    await _db.Vocabularies.AsNoTracking()
                        .Where(v => v.CEFRLevel == firstVocab!.CEFRLevel)
                        .ToListAsync(ct)),
                Opponent = new OpponentInfoDto
                {
                    Name = session.GymLeader!.Name,
                    AvatarUrl = session.GymLeader.AvatarUrl,
                    IsBot = true
                }
            };
        }

        // ═══════════════════════════════════════════════════════════
        // SUBMIT ANSWER
        // ═══════════════════════════════════════════════════════════

        public async Task<SubmitAnswerResultWrapper?> SubmitAnswerAsync(
            SubmitRoundAnswerDto dto, int userId, CancellationToken ct = default)
        {
            var session = await _db.BattleSessions
                .Include(s => s.GymLeader).ThenInclude(gl => gl!.GymLeaderPets)
                .Include(s => s.Rounds.OrderBy(r => r.RoundIndex))
                .FirstOrDefaultAsync(s => s.Id == dto.BattleSessionId && s.ChallengerUserId == userId, ct);

            if (session == null || session.Status != BattleStatus.InProgress) return null;

            var round = session.Rounds.FirstOrDefault(r => r.RoundIndex == dto.RoundIndex);
            if (round == null || round.ResolvedAt.HasValue) return null;

            var vocab = await _db.Vocabularies.FindAsync([round.VocabularyId], ct)
                ?? throw new KeyNotFoundException("Vocab không tồn tại.");

            bool isFillIn = round.RoundIndex % 2 == 1 && !string.IsNullOrWhiteSpace(vocab.Description)
                && vocab.Word != null && vocab.Description.Contains(vocab.Word, StringComparison.OrdinalIgnoreCase);
            
            string correctAnswer = isFillIn ? vocab.Word ?? "" : vocab.Meaning ?? "";

            // 1. Chấm điểm P1
            bool p1Correct = string.Equals(dto.Answer.Trim(), correctAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
            int p1Score = p1Correct ? CalculateScore(dto.ElapsedMs) : 0;

            // 2. Bot giả lập
            var gymPets = session.GymLeader!.GymLeaderPets.OrderBy(g => g.SlotIndex).ToList();
            var botPet = gymPets.First();
            bool botCorrect = _rng.NextDouble() < botPet.BotAccuracy;
            int botMs = SimulateBotMs(botPet.BotAvgResponseMs);
            int p2Score = botCorrect ? CalculateScore(botMs) : 0;

            // 3. Tính damage
            int scoreDiff = Math.Abs(p1Score - p2Score);

            // SUDDEN DEATH MECHANIC
            int damageMultiplier = 1;
            if (dto.RoundIndex >= 14) damageMultiplier = 5; // Round 15+ x5
            else if (dto.RoundIndex >= 9) damageMultiplier = 2; // Round 10+ x2

            int baseDamage = scoreDiff == 0 ? 0 : Math.Clamp(scoreDiff / 10, 1, 20) * damageMultiplier;
            int damagedPlayer = p1Score > p2Score ? 2 : (p2Score > p1Score ? 1 : 0);

            // Load pet states up front for type effectiveness
            var petStates = await _db.BattlePetStates
                .Where(bps => bps.BattleSessionId == dto.BattleSessionId)
                .ToListAsync(ct);

            double typeMultiplier = 1.0;
            string? typeEffectivenessText = null;
            int damage = baseDamage;

            if (baseDamage > 0 && damagedPlayer > 0)
            {
                int attackerPlayerIndex = damagedPlayer == 1 ? 2 : 1;
                var attackerPet = petStates.Where(p => p.PlayerIndex == attackerPlayerIndex && !p.IsFainted).OrderBy(p => p.SlotIndex).FirstOrDefault();
                var defenderPet = petStates.Where(p => p.PlayerIndex == damagedPlayer && !p.IsFainted).OrderBy(p => p.SlotIndex).FirstOrDefault();
                
                if (attackerPet != null && defenderPet != null)
                {
                    typeMultiplier = WordSoul.Domain.DomainServices.TypeEffectivenessCalculator.Calculate(
                        attackerPet.PetType, defenderPet.PetType, defenderPet.SecondaryPetType);
                    
                    if (typeMultiplier != 1.0)
                    {
                        typeEffectivenessText = WordSoul.Domain.DomainServices.TypeEffectivenessCalculator.GetEffectivenessText(typeMultiplier);
                    }
                    
                    damage = (int)(baseDamage * typeMultiplier);
                }

                ApplyDamage(petStates, damagedPlayer, damage);
            }

            // 4. Cập nhật round
            round.P1Score = p1Score;
            round.P1Answer = dto.Answer;
            round.P1Correct = p1Correct;
            round.P1AnswerMs = dto.ElapsedMs;
            round.P2Score = p2Score;
            round.P2Answer = botCorrect ? correctAnswer : "???";
            round.P2Correct = botCorrect;
            round.P2AnswerMs = botMs;
            round.DamageDealt = damage;
            round.DamagedPlayer = damagedPlayer;
            round.TypeMultiplier = typeMultiplier;
            round.ResolvedAt = DateTime.UtcNow;

            // 5. Tính điểm session

            // 6. Cập nhật session scores
            if (p1Correct) session.ChallengerCorrect++;
            if (botCorrect) session.OpponentCorrect++;
            session.ChallengerTotalScore += p1Score;
            session.OpponentTotalScore += p2Score;
            session.CurrentRound = dto.RoundIndex + 1;

            // 7. Kiểm tra kết thúc
            bool allP1Fainted = petStates.Where(p => p.PlayerIndex == 1).All(p => p.IsFainted);
            bool allP2Fainted = petStates.Where(p => p.PlayerIndex == 2).All(p => p.IsFainted);

            BattleEndedDto? battleEnded = null;
            RoundQuestionDto? nextQuestion = null;

            // Chế độ sinh tồn 100%: Chỉ kết thúc khi 1 trong 2 bên chết hết sạch Pet (hoặc lỡ hết câu hỏi dữ trữ)
            bool noMoreQuestions = dto.RoundIndex >= session.TotalQuestions - 1;
            if (allP1Fainted || allP2Fainted || noMoreQuestions)
            {
                // Xác định người thắng
                bool p1Won = allP2Fainted && !allP1Fainted;
                session.ChallengerWon = p1Won;
                session.Status = BattleStatus.Completed;
                session.CompletedAt = DateTime.UtcNow;

                int xpEarned = 0;
                bool badgeEarned = false;
                string? badgeName = null;
                string? badgeImageUrl = null;

                if (p1Won)
                {
                    // Phát thưởng giống BattleService cũ
                    var gym = session.GymLeader!;
                    var user = await _db.Users.FindAsync([userId], ct);
                    if (user != null) { user.XP += gym.XpReward; xpEarned = gym.XpReward; }

                    var gymProgress = await _db.UserGymProgresses
                        .FirstOrDefaultAsync(ugp => ugp.UserId == userId && ugp.GymLeaderId == gym.Id, ct);
                    if (gymProgress != null && gymProgress.Status != GymStatus.Defeated)
                    {
                        gymProgress.Status = GymStatus.Defeated;
                        gymProgress.DefeatedAt = DateTime.UtcNow;
                    }
                    gymProgress!.TotalAttempts++;
                    gymProgress.LastAttemptAt = DateTime.UtcNow;
                    int roundsPlayed = dto.RoundIndex + 1;
                    int scoreP = roundsPlayed == 0 ? 0
                        : (int)Math.Round((double)session.ChallengerCorrect / roundsPlayed * 100);
                    if (scoreP > gymProgress.BestScore) gymProgress.BestScore = scoreP;

                    // Badge
                    if (gym.BadgeAchievementId.HasValue)
                    {
                        var ach = await _db.Achievements.FindAsync([gym.BadgeAchievementId.Value], ct);
                        if (ach != null)
                        {
                            var exists = await _db.UserAchievements.AnyAsync(
                                ua => ua.UserId == userId && ua.AchievementId == ach.Id, ct);
                            if (!exists)
                            {
                                _db.UserAchievements.Add(new UserAchievement
                                {
                                    UserId = userId, AchievementId = ach.Id,
                                    ProgressValue = 1, IsCompleted = true,
                                    CompletedAt = DateTime.UtcNow
                                });
                                badgeEarned = true;
                                badgeName = ach.Name;
                                badgeImageUrl = gym.BadgeImageUrl;
                            }
                        }
                    }
                }

                battleEnded = new BattleEndedDto
                {
                    BattleSessionId = session.Id,
                    P1Won = p1Won,
                    P1TotalScore = session.ChallengerTotalScore,
                    P2TotalScore = session.OpponentTotalScore,
                    P1CorrectCount = session.ChallengerCorrect,
                    P2CorrectCount = session.OpponentCorrect,
                    TotalRounds = dto.RoundIndex + 1,
                    XpEarned = xpEarned,
                    BadgeEarned = badgeEarned,
                    BadgeName = badgeName,
                    BadgeImageUrl = badgeImageUrl
                };
            }
            else
            {
                // Tạo câu hỏi tiếp theo
                var nextRound = session.Rounds.ElementAtOrDefault(dto.RoundIndex + 1);
                if (nextRound != null)
                {
                    var nextVocab = await _db.Vocabularies.FindAsync([nextRound.VocabularyId], ct);
                    var allVocabs = await _db.Vocabularies.AsNoTracking()
                        .Where(v => v.CEFRLevel == nextVocab!.CEFRLevel)
                        .ToListAsync(ct);
                    nextQuestion = BuildRoundQuestion(nextRound, nextVocab!, allVocabs);
                }
            }

            await _db.SaveChangesAsync(ct);

            // Build round result
            var roundResult = new RoundResultDto
            {
                RoundIndex = dto.RoundIndex,
                VocabularyId = vocab.Id,
                CorrectAnswer = correctAnswer,
                P1Score = p1Score, P2Score = p2Score,
                P1Correct = p1Correct, P2Correct = botCorrect,
                P1AnswerMs = dto.ElapsedMs, P2AnswerMs = botMs,
                P1Answer = dto.Answer, P2Answer = botCorrect ? correctAnswer : "???",
                DamageDealt = damage, DamagedPlayer = damagedPlayer,
                TypeMultiplier = typeMultiplier, TypeEffectivenessText = typeEffectivenessText,
                P1Pets = petStates.Where(p => p.PlayerIndex == 1).OrderBy(p => p.SlotIndex)
                    .Select(MapPetState).ToList(),
                P2Pets = petStates.Where(p => p.PlayerIndex == 2).OrderBy(p => p.SlotIndex)
                    .Select(MapPetState).ToList(),
                P1TotalScore = session.ChallengerTotalScore,
                P2TotalScore = session.OpponentTotalScore
            };

            return new SubmitAnswerResultWrapper
            {
                RoundResult = roundResult,
                NextQuestion = nextQuestion,
                BattleEnded = battleEnded
            };
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ═══════════════════════════════════════════════════════════

        private static int CalculateMaxHp(PetRarity rarity, int level)
        {
            int baseHp = rarity switch
            {
                PetRarity.Common => 80,
                PetRarity.Uncommon => 100,
                PetRarity.Rare => 120,
                PetRarity.Epic => 150,
                PetRarity.Legendary => 180,
                _ => 100
            };

            int multiplier = rarity switch
            {
                PetRarity.Common => 4,
                PetRarity.Uncommon => 5,
                PetRarity.Rare => 6,
                PetRarity.Epic => 7,
                PetRarity.Legendary => 9,
                _ => 5
            };

            return baseHp + (level * multiplier);
        }

        private static int CalculateScore(int elapsedMs)
        {
            if (elapsedMs >= 10000) return 0;
            // 0ms = 1000, 10000ms = 100
            return Math.Max(100, (int)Math.Round(1000 - (elapsedMs / 10000.0) * 900));
        }

        private static int SimulateBotMs(int avgMs)
        {
            // Bot trả lời trong khoảng avgMs ± 30%
            int variance = (int)(avgMs * 0.3);
            return Math.Clamp(_rng.Next(avgMs - variance, avgMs + variance), 500, 9900);
        }

        private static void ApplyDamage(List<BattlePetState> states, int targetPlayer, int damage)
        {
            var activePet = states
                .Where(p => p.PlayerIndex == targetPlayer && !p.IsFainted)
                .OrderBy(p => p.SlotIndex)
                .FirstOrDefault();

            if (activePet == null) return;

            activePet.CurrentHp = Math.Max(0, activePet.CurrentHp - damage);
            if (activePet.CurrentHp == 0)
            {
                activePet.IsFainted = true;
                activePet.FaintedAt = DateTime.UtcNow;
            }
        }

        private static PetStateDto MapPetState(BattlePetState bps) => new()
        {
            SlotIndex = bps.SlotIndex,
            DisplayName = bps.DisplayName,
            ImageUrl = bps.ImageUrl,
            PetType = bps.PetType.ToString(),
            SecondaryPetType = bps.SecondaryPetType?.ToString(),
            MaxHp = bps.MaxHp,
            CurrentHp = bps.CurrentHp,
            IsFainted = bps.IsFainted
        };

        private static RoundQuestionDto BuildRoundQuestion(
            BattleRound round, Vocabulary vocab, List<Vocabulary> allVocabs)
        {
            bool isFillIn = round.RoundIndex % 2 == 1
                && !string.IsNullOrWhiteSpace(vocab.Description)
                && vocab.Word != null
                && vocab.Description.Contains(vocab.Word, StringComparison.OrdinalIgnoreCase);

            List<string>? options = null;
            string? prompt = null;

            if (!isFillIn)
            {
                var distractors = allVocabs
                    .Where(v => !string.Equals(v.Meaning, vocab.Meaning, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(v.Meaning))
                    .Select(v => v.Meaning!)
                    .OrderBy(_ => _rng.Next())
                    .Take(3)
                    .ToList();
                while (distractors.Count < 3) distractors.Add("—");
                options = distractors.Append(vocab.Meaning ?? "").OrderBy(_ => _rng.Next()).ToList();
            }
            else
            {
                var pat = System.Text.RegularExpressions.Regex.Escape(vocab.Word!);
                prompt = System.Text.RegularExpressions.Regex.Replace(
                    vocab.Description!, pat, "___",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }

            return new RoundQuestionDto
            {
                RoundIndex = round.RoundIndex,
                VocabularyId = vocab.Id,
                Word = vocab.Word,
                Meaning = vocab.Meaning,
                Pronunciation = vocab.Pronunciation,
                QuestionPrompt = prompt,
                QuestionType = isFillIn ? "FillInBlank" : "MultipleChoice",
                Options = options,
                TimeLimitMs = 10000
            };
        }

        private async Task<List<Vocabulary>> SelectBattleVocabsAsync(
            int userId, GymLeader gym, CancellationToken ct)
        {
            int needed = 50; // Dự trữ sẵn 50 câu hỏi cho 1 trận để chạy Endless/Sudden Death
            var priorityStates = new[] { "Learning", "Review", "Mastered" };

            var priority = await _db.UserVocabularyProgresses
                .AsNoTracking()
                .Where(uvp => uvp.UserId == userId
                    && priorityStates.Contains(uvp.MemoryState)
                    && uvp.Vocabulary!.CEFRLevel == gym.RequiredCefrLevel)
                .Include(uvp => uvp.Vocabulary)
                .OrderBy(_ => Guid.NewGuid())
                .Take(needed)
                .Select(uvp => uvp.Vocabulary!)
                .ToListAsync(ct);

            if (priority.Count >= needed) return priority;

            var taken = priority.Select(v => v.Id).ToHashSet();
            var fallback = await _db.Vocabularies
                .AsNoTracking()
                .Where(v => v.CEFRLevel == gym.RequiredCefrLevel
                    && !taken.Contains(v.Id)) // Removed sv.VocabularySet query
                .OrderBy(_ => Guid.NewGuid())
                .Take(needed - priority.Count)
                .ToListAsync(ct);

            priority.AddRange(fallback);
            return priority.OrderBy(_ => Guid.NewGuid()).ToList();
        }

        // ═══════════════════════════════════════════════════════════
        // PVP – Tạo phòng
        // ═══════════════════════════════════════════════════════════

        public async Task<PvpRoomCreatedDto> CreatePvpSessionAsync(
            CreatePvpSessionDto dto, int userId, CancellationToken ct = default)
        {
            if (dto.SelectedPetIds.Count != 3)
                throw new InvalidOperationException("Bạn phải chọn đúng 3 Pokémon.");

            var ownedPets = await _db.UserOwnedPets
                .AsNoTracking()
                .Where(uop => uop.UserId == userId && dto.SelectedPetIds.Contains(uop.PetId))
                .Include(uop => uop.Pet)
                .ToListAsync(ct);

            ownedPets = ownedPets.GroupBy(p => p.PetId).Select(g => g.First()).ToList();
            if (ownedPets.Count != 3)
                throw new InvalidOperationException("Một hoặc nhiều Pokémon không hợp lệ.");

            // Sinh RoomCode 6 ký tự alphanumeric
            string roomCode = GenerateRoomCode();

            var session = new BattleSession
            {
                ChallengerUserId = userId,
                Type = BattleType.PvP,
                Status = BattleStatus.Waiting,
                StartedAt = DateTime.UtcNow,
                RoomCode = roomCode,
                ChallengerPetIds = JsonSerializer.Serialize(ownedPets.Select(p => p.Id).ToList())
            };

            _db.BattleSessions.Add(session);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("PvP room {Code} created by user {U}, sessionId={S}", roomCode, userId, session.Id);
            return new PvpRoomCreatedDto { SessionId = session.Id, RoomCode = roomCode };
        }

        // ═══════════════════════════════════════════════════════════
        // PVP – Join phòng
        // ═══════════════════════════════════════════════════════════

        public async Task<(int sessionId, int opponentUserId)> JoinPvpSessionAsync(
            JoinPvpSessionDto dto, int userId, CancellationToken ct = default)
        {
            if (dto.SelectedPetIds.Count != 3)
                throw new InvalidOperationException("Bạn phải chọn đúng 3 Pokémon.");

            var session = await _db.BattleSessions
                .FirstOrDefaultAsync(s =>
                    s.RoomCode == dto.RoomCode.ToUpper() &&
                    s.Type == BattleType.PvP &&
                    s.Status == BattleStatus.Waiting, ct)
                ?? throw new KeyNotFoundException("Không tìm thấy phòng với mã này hoặc phòng đã đầy.");

            if (session.ChallengerUserId == userId)
                throw new InvalidOperationException("Bạn không thể tự join phòng của mình.");

            var ownedPets = await _db.UserOwnedPets
                .AsNoTracking()
                .Where(uop => uop.UserId == userId && dto.SelectedPetIds.Contains(uop.PetId))
                .Include(uop => uop.Pet)
                .ToListAsync(ct);

            ownedPets = ownedPets.GroupBy(p => p.PetId).Select(g => g.First()).ToList();
            if (ownedPets.Count != 3)
                throw new InvalidOperationException("Một hoặc nhiều Pokémon không hợp lệ.");

            // Chọn vocab: 50% từ P1 + 50% từ P2
            var vocabs = await SelectPvpVocabsAsync(session.ChallengerUserId, userId, ct);
            if (vocabs.Count == 0)
                throw new InvalidOperationException("Không đủ từ vựng để bắt đầu trận đấu.");

            session.OpponentUserId = userId;
            session.Status = BattleStatus.InProgress;
            session.TotalQuestions = vocabs.Count;
            session.OpponentPetIds = JsonSerializer.Serialize(ownedPets.Select(p => p.Id).ToList());

            // Tạo BattleRounds
            for (int i = 0; i < vocabs.Count; i++)
            {
                _db.BattleRounds.Add(new BattleRound
                {
                    BattleSessionId = session.Id,
                    RoundIndex = i,
                    VocabularyId = vocabs[i].Id,
                    StartedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("User {U} joined PvP room {Code} (session {S})", userId, dto.RoomCode, session.Id);
            return (session.Id, session.ChallengerUserId);
        }

        // ═══════════════════════════════════════════════════════════
        // PVP – Start battle (cả 2 vào Hub)
        // ═══════════════════════════════════════════════════════════

        public async Task<BattleStartedDto?> StartPvpBattleAsync(
            int sessionId, int userId, string connectionId, CancellationToken ct = default)
        {
            var session = await _db.BattleSessions
                .Include(s => s.Rounds.OrderBy(r => r.RoundIndex))
                .FirstOrDefaultAsync(s =>
                    s.Id == sessionId &&
                    s.Type == BattleType.PvP &&
                    s.Status == BattleStatus.InProgress &&
                    (s.ChallengerUserId == userId || s.OpponentUserId == userId), ct);

            if (session == null) return null;

            bool isChallenger = session.ChallengerUserId == userId;

            // Cập nhật ConnectionId và Ready flag
            if (isChallenger) { session.P1ConnectionId = connectionId; session.P1Ready = true; }
            else { session.P2ConnectionId = connectionId; session.P2Ready = true; }

            await _db.SaveChangesAsync(ct);

            // Chưa đủ 2 bên ready
            if (!session.P1Ready || !session.P2Ready) return null;

            // Tạo BattlePetStates nếu chưa có
            var existing = await _db.BattlePetStates
                .Where(bps => bps.BattleSessionId == sessionId)
                .ToListAsync(ct);

            if (existing.Count == 0)
            {
                await InitPvpPetStatesAsync(session, sessionId, ct);
            }

            var petStates = await _db.BattlePetStates
                .Where(bps => bps.BattleSessionId == sessionId)
                .ToListAsync(ct);

            var firstRound = session.Rounds.First();
            var firstVocab = await _db.Vocabularies.FindAsync([firstRound.VocabularyId], ct);
            var allVocabs = await _db.Vocabularies.AsNoTracking()
                .Where(v => v.CEFRLevel == firstVocab!.CEFRLevel)
                .ToListAsync(ct);

            // Lấy thông tin opponent
            int opponentId = isChallenger ? session.OpponentUserId!.Value : session.ChallengerUserId;
            var opponent = await _db.Users.FindAsync([opponentId], ct);

            return new BattleStartedDto
            {
                BattleSessionId = sessionId,
                TotalRounds = session.TotalQuestions,
                P1Pets = petStates.Where(p => p.PlayerIndex == 1).OrderBy(p => p.SlotIndex).Select(MapPetState).ToList(),
                P2Pets = petStates.Where(p => p.PlayerIndex == 2).OrderBy(p => p.SlotIndex).Select(MapPetState).ToList(),
                FirstQuestion = BuildRoundQuestion(firstRound, firstVocab!, allVocabs),
                Opponent = new OpponentInfoDto
                {
                    Name = opponent?.Username ?? "",
                    AvatarUrl = opponent?.AvatarUrl,
                    IsBot = false
                }
            };
        }

        // ═══════════════════════════════════════════════════════════
        // PVP – Submit Answer (buffer + resolve khi đủ 2)
        // ═══════════════════════════════════════════════════════════

        private record PvpPendingAnswer(string Answer, int ElapsedMs, DateTime SubmittedAt);

        public async Task<SubmitAnswerResultWrapper?> SubmitPvpAnswerAsync(
            SubmitRoundAnswerDto dto, int userId, CancellationToken ct = default)
        {
            var session = await _db.BattleSessions
                .Include(s => s.Rounds.OrderBy(r => r.RoundIndex))
                .FirstOrDefaultAsync(s =>
                    s.Id == dto.BattleSessionId &&
                    s.Type == BattleType.PvP &&
                    s.Status == BattleStatus.InProgress &&
                    (s.ChallengerUserId == userId || s.OpponentUserId == userId), ct);

            if (session == null) return null;

            var round = session.Rounds.FirstOrDefault(r => r.RoundIndex == dto.RoundIndex);
            if (round == null || round.ResolvedAt.HasValue) return null;

            bool isChallenger = session.ChallengerUserId == userId;
            string cacheKey = $"pvp-{session.Id}-round-{dto.RoundIndex}-{(isChallenger ? "p1" : "p2")}";
            string waitKey  = $"pvp-{session.Id}-round-{dto.RoundIndex}-{(isChallenger ? "p2" : "p1")}";

            // Lưu câu trả lời vào cache (1 giờ TTL)
            _cache.Set(cacheKey, new PvpPendingAnswer(dto.Answer, dto.ElapsedMs, DateTime.UtcNow),
                TimeSpan.FromHours(1));

            // Kiểm tra opponent đã submit chưa
            if (!_cache.TryGetValue(waitKey, out PvpPendingAnswer? opponentAnswer) || opponentAnswer == null)
            {
                // Chưa đủ – báo WaitingOpponent cho caller (Hub sẽ handle)
                return null;
            }

            // Cả 2 đã submit – xóa cache
            _cache.Remove(cacheKey);
            _cache.Remove(waitKey);

            var vocab = await _db.Vocabularies.FindAsync([round.VocabularyId], ct)
                ?? throw new KeyNotFoundException("Vocab không tồn tại.");

            bool isFillIn = round.RoundIndex % 2 == 1
                && !string.IsNullOrWhiteSpace(vocab.Description)
                && vocab.Word != null
                && vocab.Description.Contains(vocab.Word, StringComparison.OrdinalIgnoreCase);

            string correctAnswer = isFillIn ? vocab.Word ?? "" : vocab.Meaning ?? "";

            // P1 = Challenger, P2 = Opponent
            PvpPendingAnswer p1Raw = isChallenger ? new(dto.Answer, dto.ElapsedMs, DateTime.UtcNow) : opponentAnswer;
            PvpPendingAnswer p2Raw = isChallenger ? opponentAnswer : new(dto.Answer, dto.ElapsedMs, DateTime.UtcNow);

            bool p1Correct = string.Equals(p1Raw.Answer.Trim(), correctAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
            bool p2Correct = string.Equals(p2Raw.Answer.Trim(), correctAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
            int p1Score = p1Correct ? CalculateScore(p1Raw.ElapsedMs) : 0;
            int p2Score = p2Correct ? CalculateScore(p2Raw.ElapsedMs) : 0;

            int scoreDiff = Math.Abs(p1Score - p2Score);
            int damageMultiplier = dto.RoundIndex >= 14 ? 5 : (dto.RoundIndex >= 9 ? 2 : 1);
            int baseDamage = scoreDiff == 0 ? 0 : Math.Clamp(scoreDiff / 10, 1, 20) * damageMultiplier;
            int damagedPlayer = p1Score > p2Score ? 2 : (p2Score > p1Score ? 1 : 0);

            // Type Effectiveness
            var petStates = await _db.BattlePetStates
                .Where(bps => bps.BattleSessionId == session.Id)
                .ToListAsync(ct);

            double typeMultiplier = 1.0;
            string? typeEffectivenessText = null;
            int damage = baseDamage;

            if (baseDamage > 0 && damagedPlayer > 0)
            {
                int attackerIdx = damagedPlayer == 1 ? 2 : 1;
                var attackerPet = petStates.Where(p => p.PlayerIndex == attackerIdx && !p.IsFainted).OrderBy(p => p.SlotIndex).FirstOrDefault();
                var defenderPet = petStates.Where(p => p.PlayerIndex == damagedPlayer && !p.IsFainted).OrderBy(p => p.SlotIndex).FirstOrDefault();
                if (attackerPet != null && defenderPet != null)
                {
                    typeMultiplier = WordSoul.Domain.DomainServices.TypeEffectivenessCalculator.Calculate(
                        attackerPet.PetType, defenderPet.PetType, defenderPet.SecondaryPetType);
                    if (typeMultiplier != 1.0)
                        typeEffectivenessText = WordSoul.Domain.DomainServices.TypeEffectivenessCalculator.GetEffectivenessText(typeMultiplier);
                    damage = (int)(baseDamage * typeMultiplier);
                }
                ApplyDamage(petStates, damagedPlayer, damage);
            }

            // Cập nhật Round
            round.P1Score = p1Score; round.P1Answer = p1Raw.Answer; round.P1Correct = p1Correct; round.P1AnswerMs = p1Raw.ElapsedMs;
            round.P2Score = p2Score; round.P2Answer = p2Raw.Answer; round.P2Correct = p2Correct; round.P2AnswerMs = p2Raw.ElapsedMs;
            round.DamageDealt = damage; round.DamagedPlayer = damagedPlayer; round.TypeMultiplier = typeMultiplier;
            round.ResolvedAt = DateTime.UtcNow;

            if (p1Correct) session.ChallengerCorrect++;
            if (p2Correct) session.OpponentCorrect++;
            session.ChallengerTotalScore += p1Score;
            session.OpponentTotalScore += p2Score;
            session.CurrentRound = dto.RoundIndex + 1;

            bool allP1Fainted = petStates.Where(p => p.PlayerIndex == 1).All(p => p.IsFainted);
            bool allP2Fainted = petStates.Where(p => p.PlayerIndex == 2).All(p => p.IsFainted);
            bool noMoreQ = dto.RoundIndex >= session.TotalQuestions - 1;

            BattleEndedDto? battleEnded = null;
            RoundQuestionDto? nextQuestion = null;

            if (allP1Fainted || allP2Fainted || noMoreQ)
            {
                bool p1Won = allP2Fainted && !allP1Fainted ||
                             (!allP1Fainted && !allP2Fainted && session.ChallengerTotalScore >= session.OpponentTotalScore);

                session.ChallengerWon = p1Won;
                session.Status = BattleStatus.Completed;
                session.CompletedAt = DateTime.UtcNow;

                // Apply ELO
                var eloP1 = await ApplyEloAsync(session, p1Won, ct);

                battleEnded = new BattleEndedDto
                {
                    BattleSessionId = session.Id,
                    P1Won = p1Won,
                    P1TotalScore = session.ChallengerTotalScore,
                    P2TotalScore = session.OpponentTotalScore,
                    P1CorrectCount = session.ChallengerCorrect,
                    P2CorrectCount = session.OpponentCorrect,
                    TotalRounds = dto.RoundIndex + 1,
                    XpEarned = 0,
                    EloResult = eloP1
                };
            }
            else
            {
                var nextRound = session.Rounds.ElementAtOrDefault(dto.RoundIndex + 1);
                if (nextRound != null)
                {
                    var nextVocab = await _db.Vocabularies.FindAsync([nextRound.VocabularyId], ct);
                    var allVocabs = await _db.Vocabularies.AsNoTracking()
                        .Where(v => v.CEFRLevel == nextVocab!.CEFRLevel).ToListAsync(ct);
                    nextQuestion = BuildRoundQuestion(nextRound, nextVocab!, allVocabs);
                }
            }

            await _db.SaveChangesAsync(ct);

            return new SubmitAnswerResultWrapper
            {
                RoundResult = new RoundResultDto
                {
                    RoundIndex = dto.RoundIndex,
                    VocabularyId = vocab.Id,
                    CorrectAnswer = correctAnswer,
                    P1Score = p1Score, P2Score = p2Score,
                    P1Correct = p1Correct, P2Correct = p2Correct,
                    P1AnswerMs = p1Raw.ElapsedMs, P2AnswerMs = p2Raw.ElapsedMs,
                    P1Answer = p1Raw.Answer, P2Answer = p2Raw.Answer,
                    DamageDealt = damage, DamagedPlayer = damagedPlayer,
                    TypeMultiplier = typeMultiplier, TypeEffectivenessText = typeEffectivenessText,
                    P1Pets = petStates.Where(p => p.PlayerIndex == 1).OrderBy(p => p.SlotIndex).Select(MapPetState).ToList(),
                    P2Pets = petStates.Where(p => p.PlayerIndex == 2).OrderBy(p => p.SlotIndex).Select(MapPetState).ToList(),
                    P1TotalScore = session.ChallengerTotalScore,
                    P2TotalScore = session.OpponentTotalScore
                },
                NextQuestion = nextQuestion,
                BattleEnded = battleEnded
            };
        }

        // ═══════════════════════════════════════════════════════════
        // PVP – Forfeit (disconnect)
        // ═══════════════════════════════════════════════════════════

        public async Task<BattleEndedDto?> ForfeitPvpBattleAsync(
            string connectionId, CancellationToken ct = default)
        {
            var session = await _db.BattleSessions
                .FirstOrDefaultAsync(s =>
                    s.Type == BattleType.PvP &&
                    s.Status == BattleStatus.InProgress &&
                    (s.P1ConnectionId == connectionId || s.P2ConnectionId == connectionId), ct);

            if (session == null) return null;

            bool p1Forfeited = session.P1ConnectionId == connectionId;
            bool p1Won = !p1Forfeited;

            session.ChallengerWon = p1Won;
            session.Status = BattleStatus.Completed;
            session.CompletedAt = DateTime.UtcNow;

            var eloResult = await ApplyEloAsync(session, p1Won, ct);

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("PvP session {S} forfeited by connection {C}", session.Id, connectionId);

            return new BattleEndedDto
            {
                BattleSessionId = session.Id,
                P1Won = p1Won,
                P1TotalScore = session.ChallengerTotalScore,
                P2TotalScore = session.OpponentTotalScore,
                P1CorrectCount = session.ChallengerCorrect,
                P2CorrectCount = session.OpponentCorrect,
                TotalRounds = session.CurrentRound,
                XpEarned = 0,
                EloResult = eloResult
            };
        }

        // ═══════════════════════════════════════════════════════════
        // PVP – Get Rating
        // ═══════════════════════════════════════════════════════════

        public async Task<PvpRatingDto?> GetPvpRatingAsync(int userId, CancellationToken ct = default)
        {
            var user = await _db.Users.FindAsync([userId], ct);
            if (user == null) return null;
            return new PvpRatingDto
            {
                UserId = user.Id,
                Username = user.Username ?? user.Email,
                PvpRating = user.PvpRating,
                PvpWins = user.PvpWins,
                PvpLosses = user.PvpLosses,
                Tier = GetTier(user.PvpRating)
            };
        }

        // ═══════════════════════════════════════════════════════════
        // PVP Helpers
        // ═══════════════════════════════════════════════════════════

        private static string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            return new string(Enumerable.Range(0, 6).Select(_ => chars[_rng.Next(chars.Length)]).ToArray());
        }

        private static string GetTier(int rating) => rating switch
        {
            < 800  => "Bronze",
            < 1000 => "Silver",
            < 1200 => "Gold",
            < 1400 => "Platinum",
            _      => "Diamond"
        };

        private async Task<PvpEloResultDto> ApplyEloAsync(
            BattleSession session, bool challengerWon, CancellationToken ct)
        {
            const int K = 32;
            var p1 = await _db.Users.FindAsync([session.ChallengerUserId], ct);
            var p2 = await _db.Users.FindAsync([session.OpponentUserId!.Value], ct);
            if (p1 == null || p2 == null) return new PvpEloResultDto();

            double expected1 = 1.0 / (1.0 + Math.Pow(10, (p2.PvpRating - p1.PvpRating) / 400.0));
            double score1 = challengerWon ? 1.0 : 0.0;
            double score2 = 1.0 - score1;
            double expected2 = 1.0 - expected1;

            int change1 = (int)Math.Round(K * (score1 - expected1));
            int change2 = (int)Math.Round(K * (score2 - expected2));

            p1.PvpRating = Math.Max(0, p1.PvpRating + change1);
            p2.PvpRating = Math.Max(0, p2.PvpRating + change2);

            if (challengerWon) { p1.PvpWins++; p2.PvpLosses++; }
            else { p2.PvpWins++; p1.PvpLosses++; }

            // Trả về kết quả ELO cho P1 (Challenger)
            return new PvpEloResultDto
            {
                RatingChange = change1,
                NewRating = p1.PvpRating,
                NewTier = GetTier(p1.PvpRating)
            };
        }

        private async Task InitPvpPetStatesAsync(
            BattleSession session, int sessionId, CancellationToken ct)
        {
            var p1PetIds = JsonSerializer.Deserialize<List<int>>(session.ChallengerPetIds ?? "[]")!;
            var p2PetIds = JsonSerializer.Deserialize<List<int>>(session.OpponentPetIds ?? "[]")!;

            var allPetIds = p1PetIds.Concat(p2PetIds).Distinct().ToList();
            var pets = await _db.UserOwnedPets
                .Include(uop => uop.Pet)
                .Where(uop => allPetIds.Contains(uop.Id))
                .ToListAsync(ct);

            for (int i = 0; i < p1PetIds.Count; i++)
            {
                var pet = pets.First(p => p.Id == p1PetIds[i]);
                int maxHp = CalculateMaxHp(pet.Pet!.Rarity, pet.Level);
                _db.BattlePetStates.Add(new BattlePetState
                {
                    BattleSessionId = sessionId, PlayerIndex = 1, SlotIndex = i,
                    UserOwnedPetId = pet.Id, DisplayName = pet.Pet?.Name ?? "?",
                    ImageUrl = pet.Pet?.ImageUrl, PetType = pet.Pet!.Type,
                    SecondaryPetType = pet.Pet.SecondaryType, MaxHp = maxHp, CurrentHp = maxHp
                });
            }

            for (int i = 0; i < p2PetIds.Count; i++)
            {
                var pet = pets.First(p => p.Id == p2PetIds[i]);
                int maxHp = CalculateMaxHp(pet.Pet!.Rarity, pet.Level);
                _db.BattlePetStates.Add(new BattlePetState
                {
                    BattleSessionId = sessionId, PlayerIndex = 2, SlotIndex = i,
                    UserOwnedPetId = pet.Id, DisplayName = pet.Pet?.Name ?? "?",
                    ImageUrl = pet.Pet?.ImageUrl, PetType = pet.Pet!.Type,
                    SecondaryPetType = pet.Pet.SecondaryType, MaxHp = maxHp, CurrentHp = maxHp
                });
            }

            await _db.SaveChangesAsync(ct);
        }

        private async Task<List<Vocabulary>> SelectPvpVocabsAsync(
            int p1UserId, int p2UserId, CancellationToken ct)
        {
            int half = 15; // 15 từ / bên = 30 tổng
            var states = new[] { "Learning", "Review", "Mastered" };

            var p1Vocabs = await _db.UserVocabularyProgresses
                .AsNoTracking()
                .Where(uvp => uvp.UserId == p1UserId && states.Contains(uvp.MemoryState))
                .Include(uvp => uvp.Vocabulary)
                .OrderBy(_ => Guid.NewGuid())
                .Take(half)
                .Select(uvp => uvp.Vocabulary!)
                .ToListAsync(ct);

            var p2Vocabs = await _db.UserVocabularyProgresses
                .AsNoTracking()
                .Where(uvp => uvp.UserId == p2UserId && states.Contains(uvp.MemoryState))
                .Include(uvp => uvp.Vocabulary)
                .OrderBy(_ => Guid.NewGuid())
                .Take(half)
                .Select(uvp => uvp.Vocabulary!)
                .ToListAsync(ct);

            // Fallback nếu thiếu
            var taken = p1Vocabs.Select(v => v.Id).ToHashSet();
            if (p1Vocabs.Count < half)
            {
                var fill = await _db.Vocabularies.AsNoTracking()
                    .Where(v => !taken.Contains(v.Id))
                    .OrderBy(_ => Guid.NewGuid()).Take(half - p1Vocabs.Count).ToListAsync(ct);
                p1Vocabs.AddRange(fill);
            }

            taken = p2Vocabs.Select(v => v.Id).Union(taken).ToHashSet();
            if (p2Vocabs.Count < half)
            {
                var fill = await _db.Vocabularies.AsNoTracking()
                    .Where(v => !taken.Contains(v.Id))
                    .OrderBy(_ => Guid.NewGuid()).Take(half - p2Vocabs.Count).ToListAsync(ct);
                p2Vocabs.AddRange(fill);
            }

            var combined = p1Vocabs.Concat(p2Vocabs).OrderBy(_ => Guid.NewGuid()).ToList();
            return combined;
        }
    }
}
