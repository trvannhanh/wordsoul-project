import { authApi, endpoints } from './api';
import type { UserDailyQuestDto, ClaimQuestRewardResponseDto } from '../types/DailyQuestDto';

export const getTodayQuests = async (): Promise<UserDailyQuestDto[]> => {
    const response = await authApi.get<UserDailyQuestDto[]>(endpoints.todayQuests);
    return response.data;
};

export const claimQuestReward = async (
    userDailyQuestId: number
): Promise<ClaimQuestRewardResponseDto> => {
    const response = await authApi.post<ClaimQuestRewardResponseDto>(
        endpoints.claimQuestReward(userDailyQuestId)
    );
    return response.data;
};
