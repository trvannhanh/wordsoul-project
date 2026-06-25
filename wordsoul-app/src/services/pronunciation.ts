import { authApi, endpoints } from './api';

// ── Types ──────────────────────────────────────────────────────────────────

export interface PronunciationWordDto {
  vocabularyId: number;
  word: string;
  meaning: string;
  ipaTranscription?: string;
  pronunciationUrl?: string;
  exampleSentence?: string;
  memoryState: string;
  pronunciationWrongCount: number;
  lastPronunciationAt?: string;
}

export interface PronunciationAssessResponse {
  accuracyScore: number;
  fluencyScore: number;
  completenessScore: number;
  pronunciationScore: number;
  result: 'Perfect' | 'NearMiss' | 'Wrong';
  resultLabel: string;
  xpAwarded: number;
  petXpMultiplier: number;
  phonemes: PhonemeResultDto[];
}

export interface PhonemeResultDto {
  phoneme: string;
  accuracyScore: number;
  resultLabel: string;
}

export interface PronunciationHistoryDto {
  attemptTime: string;
  pronunciationScore: number;
  result: string;
  xpAwarded: number;
}

export interface PronunciationStatsDto {
  totalAttempts: number;
  perfectCount: number;
  nearMissCount: number;
  wrongCount: number;
  distinctWordsPracticed: number;
  perfectRate: number;
}

// ── Service functions ───────────────────────────────────────────────────────

export const fetchPronunciationWords = async (
  limit = 20
): Promise<PronunciationWordDto[]> => {
  const response = await authApi.get<PronunciationWordDto[]>(
    endpoints.pronunciationWords,
    { params: { limit } }
  );
  return response.data;
};

export const assessPronunciation = async (
  audio: Blob,
  vocabularyId: number,
  word: string,
  petId?: number
): Promise<PronunciationAssessResponse> => {
  const formData = new FormData();
  formData.append('audio', audio, 'recording.wav');
  formData.append('vocabularyId', vocabularyId.toString());
  formData.append('word', word);
  if (petId) formData.append('petId', petId.toString());

  const response = await authApi.post<PronunciationAssessResponse>(
    endpoints.pronunciationAssess,
    formData,
    { headers: { 'Content-Type': 'multipart/form-data' } }
  );
  return response.data;
};

export const fetchPronunciationHistory = async (
  vocabId: number
): Promise<PronunciationHistoryDto[]> => {
  const response = await authApi.get<PronunciationHistoryDto[]>(
    endpoints.pronunciationHistory(vocabId)
  );
  return response.data;
};

export const fetchPronunciationStats = async (): Promise<PronunciationStatsDto> => {
  const response = await authApi.get<PronunciationStatsDto>(
    endpoints.pronunciationStats
  );
  return response.data;
};
