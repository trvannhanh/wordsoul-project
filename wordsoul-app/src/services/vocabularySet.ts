import { AxiosError } from "axios";
import type { VocabularySet, VocabularySetDetail } from "../types/Dto";
import api, { endpoints } from "./api";

export const fetchVocabularySets = async (): Promise<VocabularySet[]> => {
  try {
    const response = await api.get<VocabularySet[]>(endpoints.vocabularySets);
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