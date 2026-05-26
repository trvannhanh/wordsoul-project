using System;
using System.Collections.Generic;

namespace WordSoul.Application.DTOs.Gym
{
    public class AdminGymLeaderDto
    {
        public int Id { get; set; }
        public int GymOrder { get; set; }
        public string Name { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public string BadgeName { get; set; } = "";
        public string? BadgeImageUrl { get; set; }
        public string Theme { get; set; } = "";
        public string RequiredCefrLevel { get; set; } = "";
        public int XpThreshold { get; set; }
        public int VocabThreshold { get; set; }
        public int QuestionCount { get; set; }
        public int XpReward { get; set; }
        public List<AdminGymLeaderPetDto> GymLeaderPets { get; set; } = new();
    }

    public class AdminGymLeaderPetDto
    {
        public int Id { get; set; }
        public int SlotIndex { get; set; }
        public string? Nickname { get; set; }
        public double BotAccuracy { get; set; }
        public int BotAvgResponseMs { get; set; }
        public int Level { get; set; }
        public AdminPetDto? Pet { get; set; }
    }

    public class AdminPetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? ImageUrl { get; set; }
    }
}
