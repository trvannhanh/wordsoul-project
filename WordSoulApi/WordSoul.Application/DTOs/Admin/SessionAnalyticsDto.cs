namespace WordSoul.Application.DTOs.Admin
{
    public class SessionAnalyticsDto
    {
        // Aggregate totals
        public int TotalSessions { get; set; }
        public int LearningSessions { get; set; }
        public int ReviewSessions { get; set; }
        public int CompletedSessions { get; set; }
        public double CompletionRate { get; set; }

        // Answer quality
        public int TotalAnswers { get; set; }
        public int CorrectAnswers { get; set; }
        public double OverallAccuracy { get; set; }
        public double AvgResponseTimeSeconds { get; set; }

        // Hints
        public int TotalHintsUsed { get; set; }

        // Daily breakdown (for chart)
        public List<DailySessionStatDto> DailyStats { get; set; } = [];

        // Top active users
        public List<ActiveUserStatDto> TopActiveUsers { get; set; } = [];
    }

    public class DailySessionStatDto
    {
        public string Date { get; set; } = string.Empty;   // "yyyy-MM-dd"
        public int LearningSessions { get; set; }
        public int ReviewSessions { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalAnswers { get; set; }
    }

    public class ActiveUserStatDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int SessionCount { get; set; }
        public int CorrectAnswers { get; set; }
    }
}
