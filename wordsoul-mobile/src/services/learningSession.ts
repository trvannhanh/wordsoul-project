import { authApi, endpoints } from './api';
import type {
  AnswerRequestDto,
  AnswerResponseDto,
  CompleteLearningSessionResponseDto,
  CompleteReviewSessionResponseDto,
  LearningSessionDto,
  QuizQuestionDto,
} from '../types/LearningSessionDto';

export const createLearningSession = async (
  id: number,
  petId?: number,
): Promise<LearningSessionDto> => {
  const response = await authApi.post<LearningSessionDto>(
    endpoints.learningSession(id),
    petId ? { petId } : undefined,
  );
  return response.data;
};

export const createReviewSession = async (): Promise<LearningSessionDto> => {
  const response = await authApi.post<LearningSessionDto>(
    endpoints.reviewSession,
  );
  return response.data;
};

export const fetchQuizOfSession = async (
  sessionId: number,
): Promise<QuizQuestionDto[]> => {
  const response = await authApi.get<QuizQuestionDto[]>(
    endpoints.quizQuestions(sessionId),
  );
  return response.data;
};

export const answerQuiz = async (
  sessionId: number,
  req: AnswerRequestDto,
): Promise<AnswerResponseDto> => {
  const response = await authApi.post<AnswerResponseDto>(
    endpoints.answerRecord(sessionId),
    req,
  );
  return response.data;
};

export const completeLearningSession = async (
  sessionId: number,
): Promise<CompleteLearningSessionResponseDto> => {
  const response = await authApi.post<CompleteLearningSessionResponseDto>(
    endpoints.completeLearningSession(sessionId),
  );
  return response.data;
};

export const completeReviewSession = async (
  sessionId: number,
): Promise<CompleteReviewSessionResponseDto> => {
  const response = await authApi.post<CompleteReviewSessionResponseDto>(
    endpoints.completeReviewSession(sessionId),
  );
  return response.data;
};
