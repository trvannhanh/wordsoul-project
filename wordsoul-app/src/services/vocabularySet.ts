import { AxiosError } from "axios";
import type { VocabularySet, VocabularySetDetail } from "../types/Dto";
import api, { endpoints } from "./api";

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
  const response = await api.get<VocabularySetDetail>(endpoints.vocabularySet(id));
  return response.data;
};