/* eslint-disable @typescript-eslint/no-unused-vars */
import { useEffect, useState, useCallback } from "react";
import {
  QuestionType,
  type CompleteLearningSessionResponseDto,
  type CompleteReviewSessionResponseDto,
  type QuizQuestion,
  type AnswerResponse,
} from "../../types/Dto";
import {
  answerQuiz,
  completeLearningSession,
  completeReviewSession,
  fetchQuizOfSession,
} from "../../services/learningSession";
import { fetchPetById } from "../../services/pet";

export const useQuizSession = (
  sessionId: number,
  mode: "learning" | "review",
  petId?: number
) => {
  const [questionsBatch, setQuestionsBatch] = useState<QuizQuestion[]>([]);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [currentQuestion, setCurrentQuestion] = useState<QuizQuestion | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sessionData, setSessionData] = useState<
    CompleteLearningSessionResponseDto | CompleteReviewSessionResponseDto | null
  >(null);
  const [levelFeedback, setLevelFeedback] = useState<{
    message: string;
    isCompleted?: boolean;
  } | null>(null);
  const [encounteredPet, setEncounteredPet] = useState<{
    id: number;
    name: string;
    imageUrl: string;
  } | null>(null);
  const [showRewardAnimation, setShowRewardAnimation] = useState(false);
  const [captureComplete, setCaptureComplete] = useState(false);

  const levelToType: Record<number, QuestionType> = {
    0: QuestionType.Flashcard,
    1: QuestionType.FillInBlank,
    2: QuestionType.MultipleChoice,
    3: QuestionType.Listening,
  };

  const loadNewQuestionsBatch = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      console.log("🔄 Fetching NEW batch of questions...");
      const questions = await fetchQuizOfSession(sessionId);
      
      if (questions.length === 0) {
        console.log("✅ No more questions, completing session...");
        await handleCompleteSession();
        setCurrentQuestion(null);
        setQuestionsBatch([]);
        return;
      }

      console.log(`✅ Loaded ${questions.length} new questions`);
      setQuestionsBatch(questions);
      setCurrentQuestionIndex(0);
      setCurrentQuestion(questions[0]);
      
      if (mode === "learning" && petId) {
        try {
          const pet = await fetchPetById(petId);
          setEncounteredPet({
            id: pet.id,
            name: pet.name,
            imageUrl: pet.imageUrl || "https://via.placeholder.com/100",
          });
        } catch (petError) {
          console.warn("Failed to load pet:", petError);
        }
      }
    } catch (err) {
      setError("Failed to load quiz questions");
      console.error("Load questions error:", err);
    } finally {
      setLoading(false);
    }
  }, [sessionId, mode, petId]);

  const loadNextQuestion = useCallback(() => {
    if (questionsBatch.length === 0) {
      console.log("⚠️ No questions in batch");
      return;
    }

    const nextIndex = currentQuestionIndex + 1;
    
    if (nextIndex >= questionsBatch.length) {
      console.log("📦 Current batch finished, loading new batch...");
      loadNewQuestionsBatch();
    } else {
      console.log(`📄 Loading question ${nextIndex + 1}/${questionsBatch.length}`);
      setCurrentQuestionIndex(nextIndex);
      setCurrentQuestion(questionsBatch[nextIndex]);
    }
  }, [questionsBatch, currentQuestionIndex, loadNewQuestionsBatch]);

  useEffect(() => {
    loadNewQuestionsBatch();
  }, [loadNewQuestionsBatch]);

  const handleAnswer = useCallback(async (
    question: QuizQuestion, 
    answer: string,
    onAnswerProcessed: () => void // Callback để báo hiệu khi xử lý xong
  ): Promise<boolean> => {
    try {
      setLoading(true);
      setError(null);
      
      console.log(`💭 Answering question ${currentQuestionIndex + 1}/${questionsBatch.length}`);
      
      const response: AnswerResponse = await answerQuiz(sessionId, {
        vocabularyId: question.vocabularyId,
        questionType: question.questionType,
        answer,
      });

      if (response.isCorrect) {
        const nextLevelType = levelToType[response.newLevel] || QuestionType.Listening;
        if (response.isVocabularyCompleted) {
          setLevelFeedback({
            message: `🎉 Mastered "${question.word}"!`,
            isCompleted: true,
          });
        } else {
          setLevelFeedback({
            message: `✅ Level ${response.newLevel + 1}: ${nextLevelType}`,
          });
        }
      } else {
        const prevLevel = Math.max(0, response.newLevel);
        const retryType = levelToType[prevLevel];
        setLevelFeedback({
          message: `🔄 Retry ${retryType} for "${question.word}"`,
        });
      }

      // Gọi callback để báo hiệu xử lý xong
      setTimeout(() => {
        setLevelFeedback(null);
        onAnswerProcessed(); // Chuyển câu hỏi sau khi pop-up đóng
      }, 3000); // Chờ 3 giây để đồng bộ với pop-up

      return response.isCorrect;
    } catch (err) {
      setError("Failed to process answer");
      console.error("Answer error:", err);
      return false;
    } finally {
      setLoading(false);
    }
  }, [sessionId, currentQuestionIndex, questionsBatch.length]);

  const handleCompleteSession = useCallback(async () => {
    try {
      setLoading(true);
      let data;
      
      if (mode === "learning") {
        data = await completeLearningSession(sessionId);
        setShowRewardAnimation(true);
      } else {
        data = await completeReviewSession(sessionId);
        setShowRewardAnimation(true);
      }
      
      setSessionData(data);
      setCurrentQuestion(null);
      setQuestionsBatch([]);
      setCurrentQuestionIndex(0);
    } catch (err) {
      setError("Failed to complete session");
      console.error("Complete session error:", err);
    } finally {
      setLoading(false);
    }
  }, [sessionId, mode]);

  useEffect(() => {
    if (questionsBatch.length === 0 && currentQuestion === null && !loading && !sessionData) {
      handleCompleteSession();
    }
  }, [questionsBatch.length, currentQuestion, loading, sessionData, handleCompleteSession]);

  console.log(`📊 Session State: ${questionsBatch.length} questions, index ${currentQuestionIndex}, current: ${!!currentQuestion}`);

  return {
    currentQuestion,
    loading,
    error,
    handleAnswer,
    sessionData,
    levelFeedback,
    progress: {
      current: currentQuestionIndex + 1,
      total: questionsBatch.length,
    },
    encounteredPet,
    showRewardAnimation,
    captureComplete,
    setCaptureComplete,
    loadNextQuestion, // Xuất hàm này để AnswerScreen gọi
  };
};