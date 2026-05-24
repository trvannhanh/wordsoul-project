import api, { authApi, endpoints } from './api';
import type { VocabularySetDto, VocabularySetDetailDto, VocabularySetProgressDto } from '../types/VocabularySetDto';

export const fetchVocabularySets = async (params?: {
  title?: string;
  theme?: string;
  difficulty?: string;
  isOwned?: boolean;
  pageNumber?: number;
  pageSize?: number;
}): Promise<VocabularySetDto[]> => {
  const response = await api.get<VocabularySetDto[]>(endpoints.vocabularySets, {
    params: { pageNumber: 1, pageSize: 20, ...params },
  });
  return response.data;
};

export const fetchUserVocabularySets = async (params?: {
  title?: string;
  theme?: string;
  difficulty?: string;
  isOwned?: boolean;
  pageNumber?: number;
  pageSize?: number;
}): Promise<VocabularySetDto[]> => {
  const response = await authApi.get<VocabularySetDto[]>(endpoints.vocabularySets, {
    params: { pageNumber: 1, pageSize: 20, ...params },
  });
  return response.data;
};

export const fetchGroupedVocabularySets = async (
  title?: string,
  limitPerTheme = 6,
): Promise<Record<string, VocabularySetDto[]>> => {
  const response = await api.get<Record<string, VocabularySetDto[]>>(
    endpoints.fetchGroupedVocabularySets,
    { params: { title, limitPerTheme } },
  );
  return response.data;
};

export const fetchVocabularySetDetail = async (
  id: number,
  pageNumber = 1,
  pageSize = 20,
): Promise<VocabularySetDetailDto> => {
  const response = await authApi.get<VocabularySetDetailDto>(
    endpoints.vocabularySetDetail(id),
    { params: { pageNumber, pageSize } },
  );
  return response.data;
};

export const fetchMyProgress = async (
  id: number,
): Promise<VocabularySetProgressDto> => {
  const response = await authApi.get<VocabularySetProgressDto>(
    endpoints.vocabularySetMyProgress(id),
  );
  return response.data;
};

export const registerVocabularySet = async (id: number): Promise<void> => {
  await authApi.post(endpoints.userVocabularySets(id));
};

export const unregisterVocabularySet = async (id: number): Promise<void> => {
  await authApi.delete(endpoints.vocabularySetUnregister(id));
};
