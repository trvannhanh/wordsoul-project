import type { AnswerResponse, LearningSession, QuizQuestion, AnswerRequest, UpdateProgressResponse, CompleteSessionResponseDto } from "../types/Dto";
import { authApi, endpoints } from "./api";


export const createLearningSession = async (id: number): Promise<LearningSession> => {
  const response = await authApi.post<LearningSession>(endpoints.learningSession(id));
  return response.data;
};

export const fetchQuizOfSession = async (id: number): Promise<QuizQuestion> => {
  const response = await authApi.get<QuizQuestion>(endpoints.quizQuestions(id));
  return response.data;
};

export const answerQuiz = async (id: number, AnswerRequest: AnswerRequest): Promise<AnswerResponse> => {
  const response = await authApi.post<AnswerResponse>(endpoints.answerRecord(id), AnswerRequest);
  return response.data;
};

export const updateProgress = async (sessionId: number, vocabId: number): Promise<UpdateProgressResponse> => {
  const response = await authApi.post<UpdateProgressResponse>(endpoints.learningProgress(sessionId, vocabId));
  return response.data;
};

export const completeSession = async (sessionId: number): Promise<CompleteSessionResponseDto> => {
  const response = await authApi.post<CompleteSessionResponseDto>(endpoints.completeSession(sessionId));
  return response.data;
};