import type { ActivityLogDto, UserProgressDto, UserDto, UserVocabularySetDto, LeaderBoardDto } from "../types/Dto";
import api, { authApi, endpoints } from "./api";

export const getUserProgress= async (): Promise<UserProgressDto> => {
  const res = await authApi.get<UserProgressDto>(endpoints.userProgress);
  return res.data;
};

export const getAllUsers = async (): Promise<UserDto[]> => {
  const response = await authApi.get<UserDto[]>(endpoints.users);
  return response.data;
};

export const getUserById = async (userId: number): Promise<UserDto> => {
  const response = await authApi.get<UserDto>(endpoints.user(userId));
  return response.data;
};

export const updateUser = async (userId: number, userDto: UserDto): Promise<void> => {
  await authApi.put(endpoints.user(userId), userDto);
};

export const assignRoleToUser = async (userId: number, roleName: string): Promise<void> => {
  await authApi.put(endpoints.userRole(userId), roleName);
};

export const removeRoleFromUser = async (userId: number, roleName: string): Promise<void> => {
  await authApi.delete(endpoints.userRole(userId) + `/${roleName}`);
};

export const getUserActivities = async (userId: number, pageNumber = 1, pageSize = 10): Promise<ActivityLogDto[]> => {
  const response = await authApi.get<ActivityLogDto[]>(endpoints.userActivities(userId), {
    params: { pageNumber, pageSize },
  });
  return response.data;
};

export const getAllActivities = async (action?: string, fromDate?: string, pageNumber = 1, pageSize = 10): Promise<ActivityLogDto[]> => {
  const response = await authApi.get<ActivityLogDto[]>(endpoints.AllUserActivities, {
    params: { action, fromDate, pageNumber, pageSize },
  });
  return response.data;
};

export const getUserVocabularySets = async (vocabularySetId: number): Promise<UserVocabularySetDto> => {
  const response = await authApi.get<UserVocabularySetDto>(endpoints.userVocabularySets(vocabularySetId));
  return response.data;
}

export const registerVocabularySet = async (vocabularySetId: number): Promise<UserVocabularySetDto | null> => {
    const response = await authApi.post<UserVocabularySetDto>(endpoints.vocabularySet(vocabularySetId));
    return response.data;
};

export const getLeaderBoard = async (topXP?: boolean, topAP?:boolean, pageNumber = 1, pageSize = 10): Promise<LeaderBoardDto[]> => {
  const response = await api.get<LeaderBoardDto[]>(endpoints.leaderBoard, {
    params: { topXP, topAP, pageNumber, pageSize },
  });
  return response.data;
};