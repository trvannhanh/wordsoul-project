/* eslint-disable @typescript-eslint/no-unused-vars */
import { useEffect, useState, useCallback } from "react";
import {
  answerQuiz,
  completeLearningSession,
  completeReviewSession,
  fetchQuizOfSession,
} from "../../services/learningSession";
import { fetchPetById } from "../../services/pet";
import { QuestionTypeEnum, type AnswerResponseDto, type CompleteLearningSessionResponseDto, type CompleteReviewSessionResponseDto, type QuizQuestionDto } from "../../types/LearningSessionDto";
import type { AnswerRequestDto } from "../../types/LearningSessionDto";

export const useQuizSession = (
  sessionId: number,
  mode: "learning" | "review",
  petId?: number,
  initialCatchRate?: number,
  currentCorrectAnswered?: number,
  setCurrentCorrectAnswered?: (value: number) => void,

  initialBuffPetId?: number,
  initialBuffName?: string,
  initialBuffDescription?: string,
  initialBuffIcon?: string,
  initialPetXpMultiplier?: number,
  initialPetCatchBonus?: number,
  initialPetHintShield?: boolean,
  initialPetReducePenalty?: boolean,
) => {
  const buffPetId = initialBuffPetId;
  const buffName = initialBuffName;
  const buffDescription = initialBuffDescription;
  const buffIcon = initialBuffIcon;
  const petXpMultiplier = initialPetXpMultiplier;
  const petCatchBonus = initialPetCatchBonus;
  const petHintShield = initialPetHintShield;
  const petReducePenalty = initialPetReducePenalty;
  const [questionsBatch, setQuestionsBatch] = useState<QuizQuestionDto[]>([]);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [currentQuestion, setCurrentQuestion] = useState<QuizQuestionDto | null>(null);
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
  const [catchRate, setCatchRate] = useState<number>(initialCatchRate || 0);

  const levelToType: Record<number, QuestionTypeEnum> = {
    0: QuestionTypeEnum.Flashcard,
    1: QuestionTypeEnum.FillInBlank,
    2: QuestionTypeEnum.MultipleChoice,
    3: QuestionTypeEnum.Listening,
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
    question: QuizQuestionDto,
    answer: string,
    onAnswerProcessed: () => void,
    onResult?: (isCorrect: boolean) => void,
    responseTimeSeconds?: number
  ): Promise<boolean> => {
    try {
      setLoading(true);
      setError(null);

      console.log(`💭 Answering question ${currentQuestionIndex + 1}/${questionsBatch.length}`);

      const requestPayload: AnswerRequestDto = {
        vocabularyId: question.vocabularyId,
        questionType: question.questionType,
        answer,
        responseTimeSeconds: responseTimeSeconds ?? 0,
        hintCount: 0,
      };

      // ── Debug log ──
      console.group(`📤 [AnswerRequest] Q${currentQuestionIndex + 1}/${questionsBatch.length}`);
      console.log("🔤 Word:              ", question.word);
      console.log("❓ QuestionType:      ", question.questionType);
      console.log("✏️  Answer:            ", answer);
      console.log("⏱️  ResponseTime (s):  ", requestPayload.responseTimeSeconds);
      console.log("💡 HintCount:         ", requestPayload.hintCount);
      console.log("🆔 VocabularyId:      ", requestPayload.vocabularyId);
      console.log("📦 Full payload:      ", requestPayload);
      console.groupEnd();

      const response: AnswerResponseDto = await answerQuiz(sessionId, {
        vocabularyId: question.vocabularyId,
        questionType: question.questionType,
        answer,
        responseTimeSeconds: responseTimeSeconds ?? 0,
        hintCount: 0,
      });

      if (setCurrentCorrectAnswered) {
        setCurrentCorrectAnswered(
          Math.max(0, Math.min(25, (currentCorrectAnswered || 0) + (response.isCorrect ? 1 : -1)))
        );
      }

      if (response.isCorrect) {
        const nextLevelType = levelToType[response.newLevel] || QuestionTypeEnum.Listening;
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
        // Only apply catch rate penalty if buff does NOT reduce penalty
        if (!petReducePenalty) {
          setCatchRate((prev) => Math.max(0, prev - 0.05));
        }
      }

      onResult?.(response.isCorrect);

      setTimeout(() => {
        setLevelFeedback(null);
        onAnswerProcessed();
      }, 3000);

      return response.isCorrect;
    } catch (err) {
      setError("Failed to process answer");
      console.error("Answer error:", err);
      onResult?.(false);
      return false;
    } finally {
      setLoading(false);
    }
  }, [sessionId, currentQuestionIndex, questionsBatch.length, currentCorrectAnswered, setCurrentCorrectAnswered, petReducePenalty]);

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
    loadNextQuestion,
    catchRate,
    // ── Buff fields ──
    buffPetId,
    buffName,
    buffDescription,
    buffIcon,
    petXpMultiplier,
    petCatchBonus,
    petHintShield,
    petReducePenalty,
  };
};