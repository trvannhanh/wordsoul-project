
import type { SearchVocabularyDto, VocabularyDto } from "../types/VocabularyDto";
import { authApi, endpoints } from "./api";

export const getAllVocabularies = async (): Promise<VocabularyDto[]> => {
  const response = await authApi.get<VocabularyDto[]>(endpoints.vocabularies);
  return response.data;
};

export const searchVocabularies = async (searchDto: SearchVocabularyDto): Promise<VocabularyDto[]> => {
    const response = await authApi.post<VocabularyDto[]>(endpoints.searchVocabularies, searchDto);
    return response.data;
};

export const createVocabulary = async (formData: FormData): Promise<VocabularyDto> => {
  const response = await authApi.post<VocabularyDto>(endpoints.vocabularies, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return response.data;
};

export const addVocabularyToSet = async (setId: number, formData: FormData): Promise<VocabularyDto> => {
  const response = await authApi.post<VocabularyDto>(endpoints.setVocabulary(setId), formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return response.data;
};

export const removeVocabularyFromSet = async (setId: number, vocabId: number): Promise<void> => {
  await authApi.delete(endpoints.deleteSetVocabulary(setId, vocabId));
};