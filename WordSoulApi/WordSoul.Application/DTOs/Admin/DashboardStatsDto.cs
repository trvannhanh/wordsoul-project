namespace WordSoul.Application.DTOs.Admin
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsersToday { get; set; }
        public int TotalVocabularySets { get; set; }
        public int TotalLearningSessions { get; set; }
        public int NewUsersThisWeek { get; set; }
        public List<TopUserDto> TopXpUsers { get; set; } = [];
    }

    public class TopUserDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalXP { get; set; }
        public int TotalAP { get; set; }
    }
}
