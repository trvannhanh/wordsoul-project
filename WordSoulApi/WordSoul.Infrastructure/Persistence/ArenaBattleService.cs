using Microsoft.EntityFrameworkCore;
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
        private static readonly Random _rng = new();

        public ArenaBattleService(WordSoulDbContext db, ILogger<ArenaBattleService> logger)
        {
            _db = db;
            _logger = logger;
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
                .Where(uop => uop.UserId == userId && uop.IsActive
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
                    _db.BattlePetStates.Add(new BattlePetState
                    {
                        BattleSessionId = sessionId,
                        PlayerIndex = 1,
                        SlotIndex = i,
                        UserOwnedPetId = pet.Id,
                        DisplayName = pet.Pet?.Name ?? "?",
                        ImageUrl = pet.Pet?.ImageUrl,
                        MaxHp = 100, CurrentHp = 100
                    });
                }

                // P2: bot (GymLeaderPets)
                var gymPets = session.GymLeader!.GymLeaderPets.OrderBy(p => p.SlotIndex).ToList();
                for (int i = 0; i < gymPets.Count; i++)
                {
                    _db.BattlePetStates.Add(new BattlePetState
                    {
                        BattleSessionId = sessionId,
                        PlayerIndex = 2,
                        SlotIndex = i,
                        GymLeaderPetId = gymPets[i].Id,
                        DisplayName = gymPets[i].Nickname ?? gymPets[i].Pet?.Name ?? "?",
                        ImageUrl = gymPets[i].Pet?.ImageUrl,
                        MaxHp = 100, CurrentHp = 100
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
            int damage = scoreDiff == 0 ? 0 : Math.Clamp(scoreDiff / 10, 1, 20);
            int damagedPlayer = p1Score > p2Score ? 2 : (p2Score > p1Score ? 1 : 0);

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
            round.ResolvedAt = DateTime.UtcNow;

            // 5. Cập nhật HP
            var petStates = await _db.BattlePetStates
                .Where(bps => bps.BattleSessionId == dto.BattleSessionId)
                .ToListAsync(ct);

            if (damage > 0 && damagedPlayer > 0)
            {
                ApplyDamage(petStates, damagedPlayer, damage);
            }

            // 6. Cập nhật session scores
            if (p1Correct) session.ChallengerCorrect++;
            if (botCorrect) session.OpponentCorrect++;
            session.ChallengerTotalScore += p1Score;
            session.OpponentTotalScore += p2Score;
            session.CurrentRound = dto.RoundIndex + 1;

            // 7. Kiểm tra kết thúc
            bool allP1Fainted = petStates.Where(p => p.PlayerIndex == 1).All(p => p.IsFainted);
            bool allP2Fainted = petStates.Where(p => p.PlayerIndex == 2).All(p => p.IsFainted);
            bool lastRound = dto.RoundIndex >= session.TotalQuestions - 1;

            BattleEndedDto? battleEnded = null;
            RoundQuestionDto? nextQuestion = null;

            if (allP1Fainted || allP2Fainted || lastRound)
            {
                // Xác định người thắng
                bool p1Won = allP2Fainted
                    || (!allP1Fainted && session.ChallengerTotalScore >= session.OpponentTotalScore);
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
                    int scoreP = session.TotalQuestions == 0 ? 0
                        : (int)Math.Round((double)session.ChallengerCorrect / session.TotalQuestions * 100);
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
                    TotalRounds = session.TotalQuestions,
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
            int needed = gym.QuestionCount;
            var priorityStates = new[] { "Learning", "Review", "Mastered" };

            var priority = await _db.UserVocabularyProgresses
                .AsNoTracking()
                .Where(uvp => uvp.UserId == userId
                    && priorityStates.Contains(uvp.MemoryState)
                    && uvp.Vocabulary!.CEFRLevel == gym.RequiredCefrLevel
                    && _db.SetVocabularies.Any(sv =>
                        sv.VocabularyId == uvp.VocabularyId
                        && sv.VocabularySet!.Theme == gym.Theme))
                .Include(uvp => uvp.Vocabulary)
                .OrderBy(_ => Guid.NewGuid())
                .Take(needed)
                .Select(uvp => uvp.Vocabulary!)
                .ToListAsync(ct);

            if (priority.Count >= needed) return priority;

            var taken = priority.Select(v => v.Id).ToHashSet();
            var fallback = await _db.SetVocabularies
                .AsNoTracking()
                .Where(sv => sv.VocabularySet!.Theme == gym.Theme
                    && sv.Vocabulary!.CEFRLevel == gym.RequiredCefrLevel
                    && !taken.Contains(sv.VocabularyId))
                .Include(sv => sv.Vocabulary)
                .OrderBy(_ => Guid.NewGuid())
                .Take(needed - priority.Count)
                .Select(sv => sv.Vocabulary!)
                .ToListAsync(ct);

            priority.AddRange(fallback);
            return priority.OrderBy(_ => Guid.NewGuid()).ToList();
        }
    }
}
