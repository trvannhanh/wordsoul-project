import { authApi, endpoints } from './api';
import type {
  ClaimQuestRewardResponseDto,
  UserDailyQuestDto,
} from '../types/DailyQuestDto';

export const fetchTodayQuests = async (): Promise<UserDailyQuestDto[]> => {
  const response = await authApi.get<UserDailyQuestDto[]>(
    endpoints.todayQuests,
  );
  return response.data;
};

export const claimQuestReward = async (
  questId: number,
): Promise<ClaimQuestRewardResponseDto> => {
  const response = await authApi.post<ClaimQuestRewardResponseDto>(
    endpoints.claimQuestReward(questId),
  );
  return response.data;
};
