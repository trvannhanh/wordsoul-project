import { AxiosError } from "axios";
import api, { authApi, endpoints } from "./api";
import type { VocabularySetDetailDto, VocabularySetDto, AiCreateVocabularySetResultDto, VocabularyPreviewDto } from "../types/VocabularySetDto";


export const fetchVocabularySets = async (
  title?: string,
  theme?: string,
  difficulty?: string,
  createdAfter?: string,
  isOwned?: boolean, // Mới: Thêm tham số isOwned
  pageNumber: number = 1,
  pageSize: number = 10
): Promise<VocabularySetDto[]> => {
  try {
    const response = await api.get<VocabularySetDto[]>(endpoints.vocabularySets, {
      params: {
        title: title || undefined,
        theme: theme || undefined,
        difficulty: difficulty || undefined,
        createdAfter: createdAfter || undefined,
        isOwned: isOwned !== undefined ? isOwned : undefined, // Chỉ gửi nếu isOwned được xác định
        pageNumber,
        pageSize,
      },
    });
    return response.data;
  } catch (error) {
    const errorMessage = error instanceof AxiosError
      ? `Failed to fetch vocabulary sets: ${error.response?.data?.message || error.message}`
      : 'An unexpected error occurred while fetching vocabulary sets';
    throw new Error(errorMessage);
  }
};

export const fetchUserVocabularySets = async (
  title?: string,
  theme?: string,
  difficulty?: string,
  createdAfter?: string,
  isOwned?: boolean,
  pageNumber: number = 1,
  pageSize: number = 10
): Promise<VocabularySetDto[]> => {
  try {
    const response = await authApi.get<VocabularySetDto[]>(endpoints.vocabularySets, {
      params: {
        title: title || undefined,
        theme: theme || undefined,
        difficulty: difficulty || undefined,
        createdAfter: createdAfter || undefined,
        isOwned: isOwned !== undefined ? isOwned : undefined,
        pageNumber,
        pageSize,
      },
    });
    return response.data;
  } catch (error) {
    const errorMessage = error instanceof AxiosError
      ? `Failed to fetch vocabulary sets: ${error.response?.data?.message || error.message}`
      : 'An unexpected error occurred while fetching vocabulary sets';
    throw new Error(errorMessage);
  }
};

export const fetchGroupedVocabularySets = async (
  title?: string,
  limitPerTheme: number = 6
): Promise<Record<string, VocabularySetDto[]>> => {
  try {
    const response = await api.get<Record<string, VocabularySetDto[]>>(`${endpoints.vocabularySets}/grouped`, {
      params: {
        title: title || undefined,
        limitPerTheme,
      },
    });
    return response.data;
  } catch (error) {
    const errorMessage = error instanceof AxiosError
      ? `Failed to fetch grouped vocabulary sets: ${error.response?.data?.message || error.message}`
      : 'An unexpected error occurred while fetching grouped vocabulary sets';
    throw new Error(errorMessage);
  }
};

export const fetchVocabularySetDetail = async (id: number, page = 1, pageSize = 10): Promise<VocabularySetDetailDto> => {
  try {
    const response = await api.get<VocabularySetDetailDto>(endpoints.vocabularySetDetail(id), {
      params: {
        page,
        pageSize,
      },
    });
    return response.data;
  } catch (error) {
    const errorMessage = error instanceof AxiosError
      ? `Failed to fetch vocabulary sets: ${error.response?.data?.message || error.message}`
      : 'An unexpected error occurred while fetching vocabulary sets';
    throw new Error(errorMessage);
  }
};

export const createVocabularySet = async (formData: FormData): Promise<VocabularySetDto> => {
  const response = await authApi.post<VocabularySetDto>(endpoints.vocabularySets, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return response.data;
};

export const aiPreviewVocabularySet = async (dto: { words: string[]; useAi?: boolean }): Promise<VocabularyPreviewDto[]> => {
  const response = await authApi.post<VocabularyPreviewDto[]>(endpoints.aiPreviewVocabularySet, dto);
  return response.data;
};

export const aiCreateVocabularySet = async (formData: FormData): Promise<AiCreateVocabularySetResultDto> => {
  try {
    const response = await authApi.post<AiCreateVocabularySetResultDto>(endpoints.aiCreateVocabularySet, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      timeout: 300000, // Timeout dài (5 phút) vì AI xử lý batch có delay
    });
    return response.data;
  } catch (error) {
    if (error instanceof AxiosError && error.response) {
      const serverMessage = error.response.data?.message || error.response.data || error.message;
      throw new Error(typeof serverMessage === 'string' ? serverMessage : JSON.stringify(serverMessage));
    }
    throw error;
  }
};

export const updateVocabularySet = async (id: number, data: VocabularySetDto): Promise<void> => {
  await authApi.put(endpoints.vocabularySet(id), data);
};

export const deleteVocabularySet = async (id: number): Promise<void> => {
  await authApi.delete(endpoints.vocabularySet(id));
};





