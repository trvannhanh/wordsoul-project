export interface UserAchievementDto {
    achievementId: number;
    name: string;
    description?: string;
    progressValue: number;
    targetValue: number;
    progressPercent: number;
    remaining: number;
    isCompleted: boolean;
}
