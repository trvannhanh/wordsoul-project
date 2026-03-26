import { authApi, endpoints } from './api';
import type {
  GymLeaderDto,
  StartBattleResponseDto,
  BattleAnswerDto,
  BattleResultDto,
} from '../types/GymTypes';

export const fetchGyms = async (): Promise<GymLeaderDto[]> => {
  const res = await authApi.get<GymLeaderDto[]>(endpoints.gyms);
  return res.data;
};

export const fetchGymDetail = async (gymId: number): Promise<GymLeaderDto> => {
  const res = await authApi.get<GymLeaderDto>(endpoints.gymDetail(gymId));
  return res.data;
};

export const startGymBattle = async (gymId: number): Promise<StartBattleResponseDto> => {
  const res = await authApi.post<StartBattleResponseDto>(endpoints.startGymBattle(gymId));
  return res.data;
};

export const submitBattle = async (
  sessionId: number,
  answers: BattleAnswerDto[]
): Promise<BattleResultDto> => {
  const res = await authApi.post<BattleResultDto>(endpoints.submitBattle(sessionId), {
    answers,
  });
  return res.data;
};

/** Tạo arena battle session – trả về sessionId để kết nối SignalR */
export const setupArenaBattle = async (
  gymLeaderId: number,
  selectedPetIds: number[]
): Promise<number> => {
  const res = await authApi.post<{ sessionId: number }>('/arena/setup', {
    gymLeaderId,
    selectedPetIds,
  });
  return res.data.sessionId;
};

