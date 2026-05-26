import { authApi, endpoints } from './api';
import type { GymLeaderDto } from '../types/GymTypes';

export const fetchGyms = async (): Promise<GymLeaderDto[]> => {
  const response = await authApi.get<GymLeaderDto[]>(endpoints.gyms);
  return response.data;
};

export const fetchGymDetail = async (gymId: number): Promise<GymLeaderDto> => {
  const response = await authApi.get<GymLeaderDto>(endpoints.gymDetail(gymId));
  return response.data;
};
