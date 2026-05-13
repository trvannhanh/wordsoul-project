namespace WordSoul.Application.DTOs.Gym
{
    public class GymUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int XpThreshold { get; set; }
        public int PassRatePercent { get; set; }
        public int AiReactionTimeMs { get; set; }
    }
}
