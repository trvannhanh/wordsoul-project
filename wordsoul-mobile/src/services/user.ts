import { authApi, endpoints } from './api';
import type { UserDto, UserProgressDto } from '../types/UserDto';

export const getCurrentUser = async (): Promise<UserDto> => {
  const response = await authApi.get<UserDto>(endpoints.currentUser);
  return response.data;
};

export const getUserProgress = async (): Promise<UserProgressDto> => {
  const response = await authApi.get<UserProgressDto>(endpoints.userProgress);
  return response.data;
};

export const getLeaderboard = async (): Promise<UserDto[]> => {
  const response = await authApi.get<UserDto[]>(endpoints.leaderBoard);
  return response.data;
};

export const registerPushToken = async (token: string): Promise<void> => {
  await authApi.post(endpoints.pushToken, { token });
};
