import type {
  AnswerResponse,
  LearningSession,
  QuizQuestion,
  AnswerRequest,
  UpdateProgressResponse,
  CompleteLearningSessionResponseDto,
  CompleteReviewSessionResponseDto,
} from "../types/Dto";
import { authApi, endpoints } from "./api";

export const createLearningSession = async (id: number): Promise<LearningSession> => {
  const response = await authApi.post<LearningSession>(endpoints.learningSession(id));
  return response.data;
};

export const createReviewSession = async (): Promise<LearningSession> => {
  const response = await authApi.post<LearningSession>(endpoints.reviewSession);
  return response.data;
};

export const fetchQuizOfSession = async (
  id: number,
  params: { includeRetries: boolean } = { includeRetries: false }
): Promise<QuizQuestion[]> => {
  const response = await authApi.get<QuizQuestion[]>(
    `${endpoints.quizQuestions(id)}?includeRetries=${params.includeRetries}`
  );
  return response.data;
};

export const answerQuiz = async (
  sessionId: number,
  req: AnswerRequest
): Promise<AnswerResponse> => {
  const response = await authApi.post<AnswerResponse>(
    endpoints.answerRecord(sessionId),
    req
  );
  return response.data;
};

export const updateProgress = async (
  sessionId: number,
  vocabId: number
): Promise<UpdateProgressResponse> => {
  const response = await authApi.post<UpdateProgressResponse>(
    endpoints.learningProgress(sessionId, vocabId)
  );
  return response.data;
};

export const completeLearningSession = async (
  sessionId: number
): Promise<CompleteLearningSessionResponseDto> => {
  const response = await authApi.post<CompleteLearningSessionResponseDto>(
    endpoints.completeLearningSession(sessionId)
  );
  return response.data;
};

export const completeReviewSession = async (
  sessionId: number
): Promise<CompleteReviewSessionResponseDto> => {
  const response = await authApi.post<CompleteReviewSessionResponseDto>(
    endpoints.completeReviewSession(sessionId)
  );
  return response.data;
};