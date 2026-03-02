export interface UserDailyQuestDto {
    id: number;
    dailyQuestId: number;
    title: string;
    description?: string;
    questType: string;
    progress: number;
    targetValue: number;
    rewardType: string;
    rewardValue: number;
    isCompleted: boolean;
    isClaimed: boolean;
    questDate: string;
}

export interface ClaimQuestRewardResponseDto {
    rewardType: string;
    rewardValue: number;
    rewardReferenceId?: number;
    message: string;
}
