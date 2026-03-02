import { authApi, endpoints } from './api';
import type { UserAchievementDto } from '../types/AchievementDto';

export const getMyAchievements = async (): Promise<UserAchievementDto[]> => {
    const response = await authApi.get<UserAchievementDto[]>(endpoints.myAchievements);
    return response.data;
};

export const claimAchievement = async (achievementId: number): Promise<void> => {
    await authApi.post(endpoints.claimAchievement(achievementId));
};
