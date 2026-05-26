namespace WordSoul.Application.DTOs.Admin
{
    public class PvpLeaderboardEntryDto
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public int PvpRating { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int TotalGames => Wins + Losses;
        public double WinRate => TotalGames == 0 ? 0 : Math.Round((double)Wins / TotalGames * 100, 1);
    }

    public class PvpLeaderboardDto
    {
        public List<PvpLeaderboardEntryDto> Entries { get; set; } = [];
        public int TotalActivePlayers { get; set; }
        public double AverageRating { get; set; }
        public int HighestRating { get; set; }
    }
}
