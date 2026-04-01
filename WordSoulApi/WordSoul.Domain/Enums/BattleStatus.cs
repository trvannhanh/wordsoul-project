namespace WordSoul.Domain.Enums
{
    public enum BattleStatus
    {
        Waiting,    // PvP: phòng đã tạo, chờ người chơi 2 vào
        InProgress, // Đang diễn ra
        Completed,  // Đã hoàn thành (thắng hoặc thua)
        Abandoned   // Người dùng rời bỏ giữa chừng
    }
}
