import { AxiosError } from "axios";
import type { Vocabulary, VocabularySet, VocabularySetDetail } from "../types/Dto";
import api, { authApi, endpoints } from "./api";

export const fetchVocabularySets = async (
  title?: string,
  theme?: string,
  difficulty?: string,
  createdAfter?: string,
  pageNumber: number = 1,
  pageSize: number = 10
): Promise<VocabularySet[]> => {
  try {
    const response = await api.get<VocabularySet[]>(endpoints.vocabularySets, {
      params: {
        title: title || undefined,
        theme: theme || undefined,
        difficulty: difficulty || undefined,
        createdAfter: createdAfter || undefined,
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

export const fetchVocabularySetDetail = async (id: number): Promise<VocabularySetDetail> => {
  const response = await api.get<VocabularySetDetail>(endpoints.vocabularySetDetail(id));
  return response.data;
};

export const createVocabularySet = async (formData: FormData): Promise<VocabularySet> => {
  const response = await authApi.post<VocabularySet>(endpoints.vocabularySets, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return response.data;
};

export const updateVocabularySet = async (id: number, data: VocabularySet): Promise<void> => {
  await authApi.put(endpoints.vocabularySet(id), data);
};

export const deleteVocabularySet = async (id: number): Promise<void> => {
  await authApi.delete(endpoints.vocabularySet(id));
};

export const getAllVocabularies = async (): Promise<Vocabulary[]> => {
  const response = await authApi.get<Vocabulary[]>(endpoints.vocabularies);
  return response.data;
};

export const createVocabulary = async (formData: FormData): Promise<Vocabulary> => {
  const response = await authApi.post<Vocabulary>(endpoints.vocabularies, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return response.data;
};

export const addVocabularyToSet = async (setId: number, formData: FormData): Promise<Vocabulary> => {
  const response = await authApi.post<Vocabulary>(endpoints.setVocabulary(setId), formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return response.data;
};

export const removeVocabularyFromSet = async (setId: number, vocabId: number): Promise<void> => {
  await authApi.delete(endpoints.deleteSetVocabulary(setId, vocabId));
};