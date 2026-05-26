using System;
using System.Collections.Generic;

namespace WordSoul.Application.DTOs.Battle
{
    public class BattleHistoryEntryDto
    {
        public int SessionId { get; set; }
        public string Type { get; set; } = ""; // "GymBattle" or "PvP"
        public string Status { get; set; } = ""; // "Waiting", "InProgress", "Completed", "Abandoned"
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TotalQuestions { get; set; }
        public int CurrentRound { get; set; }
        
        // P1 (Challenger)
        public int ChallengerUserId { get; set; }
        public string ChallengerUsername { get; set; } = "";
        public int ChallengerCorrect { get; set; }
        public int ChallengerTotalScore { get; set; }
        
        // P2 (Opponent / Bot)
        public int? OpponentUserId { get; set; }
        public string OpponentName { get; set; } = "";
        public int OpponentCorrect { get; set; }
        public int OpponentTotalScore { get; set; }

        public bool? ChallengerWon { get; set; }
        public bool IsCurrentUserP1 { get; set; }
        public bool CurrentUserWon { get; set; }
    }

    public class BattleHistoryDetailDto
    {
        public int SessionId { get; set; }
        public string Type { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool? ChallengerWon { get; set; }
        public bool IsCurrentUserP1 { get; set; }
        public bool CurrentUserWon { get; set; }

        public PlayerHistoryDetailDto P1 { get; set; } = null!;
        public PlayerHistoryDetailDto P2 { get; set; } = null!;

        public List<RoundHistoryDetailDto> Rounds { get; set; } = [];
    }

    public class PlayerHistoryDetailDto
    {
        public int? UserId { get; set; }
        public string Name { get; set; } = "";
        public int CorrectCount { get; set; }
        public int TotalScore { get; set; }
        public List<PetHistoryDto> SelectedPets { get; set; } = [];
    }

    public class PetHistoryDto
    {
        public string DisplayName { get; set; } = "";
        public string? ImageUrl { get; set; }
        public string PetType { get; set; } = "";
        public string? SecondaryPetType { get; set; }
        public int MaxHp { get; set; }
        public int CurrentHp { get; set; }
        public bool IsFainted { get; set; }
    }

    public class RoundHistoryDetailDto
    {
        public int RoundIndex { get; set; }
        public string Word { get; set; } = "";
        public string Meaning { get; set; } = "";
        public string? Pronunciation { get; set; }

        // Challenger response
        public string? P1Answer { get; set; }
        public bool P1Correct { get; set; }
        public int? P1AnswerMs { get; set; }
        public int? P1Score { get; set; }

        // Opponent response
        public string? P2Answer { get; set; }
        public bool P2Correct { get; set; }
        public int? P2AnswerMs { get; set; }
        public int? P2Score { get; set; }

        // Result
        public int DamageDealt { get; set; }
        public int DamagedPlayer { get; set; } // 1=P1, 2=P2, 0=none
        public double TypeMultiplier { get; set; }
    }

    public class PvpLobbyRoomDto
    {
        public int SessionId { get; set; }
        public string RoomCode { get; set; } = "";
        public int HostUserId { get; set; }
        public string HostUsername { get; set; } = "";
        public int HostRating { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PvpLeaderboardEntryDto
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public int PvpRating { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int TotalGames => Wins + Losses;
        public double WinRate => TotalGames == 0 ? 0 : Math.Round((double)Wins / TotalGames * 100, 1);
    }

    public class BattleHistoryPageDto
    {
        public List<BattleHistoryEntryDto> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
