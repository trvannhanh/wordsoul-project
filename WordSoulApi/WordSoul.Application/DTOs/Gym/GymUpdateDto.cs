namespace WordSoul.Application.DTOs.Gym
{
    public class GymUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int XpThreshold { get; set; }
        public int AiReactionTimeMs { get; set; }
        /// <summary>Bot hit-rate (0.0 – 1.0). Applied to all pets in the gym.</summary>
        public double BotAccuracy { get; set; } = 0.6;
    }
}
