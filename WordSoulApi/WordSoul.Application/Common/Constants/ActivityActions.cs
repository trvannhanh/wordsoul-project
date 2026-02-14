

namespace WordSoul.Application.Common.Constants
{
    public static class ActivityActions
    {
        // Authentication
        public const string UserLogin = "UserLogin";
        public const string UserLogout = "UserLogout";
        public const string UserRegister = "UserRegister";

        // Learning
        public const string StartLearningSession = "StartLearningSession";
        public const string FinishLearningSession = "FinishLearningSession";
        public const string AnswerQuestion = "AnswerQuestion";
        public const string VocabularyReviewed = "VocabularyReviewed";

        // Gamification
        public const string PetUnlocked = "PetUnlocked";
        public const string PetUpgraded = "PetUpgraded";
        public const string RewardClaimed = "RewardClaimed";
        public const string QuestClaimed = "QuestClaimed";
        public const string AchievementUnlocked = "AchievementUnlocked";
        public const string DailyStreakIncreased = "DailyStreakIncreased";
        public const string DailyStreakBroken = "DailyStreakBroken";
    }
}
