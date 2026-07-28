import api, { authApi, endpoints } from "./api";
import type { VocabularySetDetailDto, VocabularySetDto, AiCreateVocabularySetResultDto, VocabularyPreviewDto, UpdateVocabularyInSetDto, VocabularySetProgressDto, VocabularyDetailDto, UpdateVocabularyCoreDto } from "../types/VocabularySetDto";


export const fetchVocabularySets = async (
  title?: string,
  theme?: string,
  difficulty?: string,
  createdAfter?: string,
  isOwned?: boolean, // Mới: Thêm tham số isOwned
  pageNumber: number = 1,
  pageSize: number = 10
): Promise<VocabularySetDto[]> => {
  const response = await api.get<VocabularySetDto[]>(endpoints.vocabularySets, {
    params: {
      title: title || undefined,
      theme: theme || undefined,
      difficulty: difficulty || undefined,
      createdAfter: createdAfter || undefined,
      isOwned: isOwned !== undefined ? isOwned : undefined,
      pageNumber,
      pageSize,
    },
    errorHandling: { suppressToast: true },
  });
  return response.data;
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
    errorHandling: { suppressToast: true },
  });
  return response.data;
};

export const fetchGroupedVocabularySets = async (
  title?: string,
  limitPerTheme: number = 6
): Promise<Record<string, VocabularySetDto[]>> => {
  const response = await api.get<Record<string, VocabularySetDto[]>>(`${endpoints.vocabularySets}/grouped`, {
    params: {
      title: title || undefined,
      limitPerTheme,
    },
    errorHandling: { suppressToast: true },
  });
  return response.data;
};

export const fetchVocabularySetDetail = async (id: number, page = 1, pageSize = 10): Promise<VocabularySetDetailDto> => {
  const response = await api.get<VocabularySetDetailDto>(endpoints.vocabularySetDetail(id), {
    params: {
      page,
      pageSize,
    },
    errorHandling: { suppressToast: true },
  });
  return response.data;
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
  const response = await authApi.post<AiCreateVocabularySetResultDto>(endpoints.aiCreateVocabularySet, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
    timeout: 300000,
  });
  return response.data;
};

export const updateVocabularySet = async (id: number, data: VocabularySetDto): Promise<void> => {
  await authApi.put(endpoints.vocabularySet(id), data);
};

export const deleteVocabularySet = async (id: number): Promise<void> => {
  await authApi.delete(endpoints.vocabularySet(id));
};

export const publishVocabularySet = async (id: number): Promise<VocabularySetDto> => {
  const response = await authApi.put<VocabularySetDto>(endpoints.vocabularySetPublish(id));
  return response.data;
};

export const updateVocabularyInSet = async (
  setId: number,
  vocabId: number,
  dto: UpdateVocabularyInSetDto
): Promise<VocabularyDetailDto> => {
  const response = await authApi.put<VocabularyDetailDto>(endpoints.vocabularySetVocabOverride(setId, vocabId), dto);
  return response.data;
};

export const fetchMySetProgress = async (id: number): Promise<VocabularySetProgressDto> => {
  const response = await authApi.get<VocabularySetProgressDto>(endpoints.vocabularySetMyProgress(id));
  return response.data;
};

export const unregisterVocabularySet = async (id: number): Promise<void> => {
  await authApi.delete(endpoints.vocabularySetUnregister(id));
};

export const addExistingVocabToSet = async (setId: number, vocabId: number): Promise<void> => {
  await authApi.post(`/vocabulary-sets/${setId}/vocabularies/${vocabId}`);
};

export const addNewVocabToSet = async (setId: number, dto: VocabularyPreviewDto): Promise<void> => {
  await authApi.post(`/vocabulary-sets/${setId}/vocabularies/new`, dto);
};

export const removeVocabFromSet = async (setId: number, vocabId: number): Promise<void> => {
  await authApi.delete(`/vocabulary-sets/${setId}/vocabularies/${vocabId}`);
};

export const updateVocabCore = async (
  setId: number,
  vocabId: number,
  dto: UpdateVocabularyCoreDto,
  imageFile?: File | null
): Promise<VocabularyDetailDto> => {
  const form = new FormData();
  if (dto.word !== undefined) form.append('word', dto.word);
  if (dto.meaning !== undefined) form.append('meaning', dto.meaning);
  if (dto.pronunciation !== undefined) form.append('pronunciation', dto.pronunciation);
  if (dto.exampleSentence !== undefined) form.append('exampleSentence', dto.exampleSentence);
  if (dto.description !== undefined) form.append('description', dto.description);
  if (imageFile) form.append('imageFile', imageFile);
  const res = await authApi.patch<VocabularyDetailDto>(
    `/vocabulary-sets/${setId}/vocabularies/${vocabId}/core`,
    form,
    { headers: { 'Content-Type': 'multipart/form-data' } }
  );
  return res.data;
};





