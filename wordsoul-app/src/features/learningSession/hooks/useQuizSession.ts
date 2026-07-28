import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { fetchQuizOfSession, completeLearningSession, completeReviewSession, answerQuiz } from "../../../services/learningSession";
import { getCurrentUser } from "../../../services/user";
import { fetchPetById } from "../../../services/pet";
import { QuestionTypeEnum, type AnswerResponseDto, type CompleteLearningSessionResponseDto, type CompleteReviewSessionResponseDto, type QuizQuestionDto } from "../../../types/LearningSessionDto";
import type { AnswerRequestDto } from "../../../types/LearningSessionDto";

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
  const [hintBalance, setHintBalance] = useState<number>(0);
  const [comboCount, setComboCount] = useState(0);
  const submissionIdsRef = useRef(new WeakMap<QuizQuestionDto, string>());

  useEffect(() => {
    const fetchUserHints = async () => {
      try {
         const user = await getCurrentUser();
         setHintBalance(user.hintBalance || 0);
      } catch (e) {
         console.warn("Could not fetch user hints", e);
      }
    };
    fetchUserHints();
  }, []);

  const levelToType = useMemo<Record<number, QuestionTypeEnum>>(() => ({
    0: QuestionTypeEnum.Flashcard,
    1: QuestionTypeEnum.FillInBlank,
    2: QuestionTypeEnum.MultipleChoice,
    3: QuestionTypeEnum.Listening,
  }), []);

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

  const loadNewQuestionsBatch = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);

      const questions = await fetchQuizOfSession(sessionId);

      if (questions.length === 0) {
        await handleCompleteSession();
        setCurrentQuestion(null);
        setQuestionsBatch([]);
        return;
      }

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
  }, [sessionId, mode, petId, handleCompleteSession]);

  const loadNextQuestion = useCallback(() => {
    if (questionsBatch.length === 0) {
      return;
    }

    const nextIndex = currentQuestionIndex + 1;

    if (nextIndex >= questionsBatch.length) {
      loadNewQuestionsBatch();
    } else {
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
    onResult?: (isCorrect: boolean) => void,
    responseTimeSeconds?: number,
    usedHintCount = 0
  ): Promise<boolean> => {
    try {
      setLoading(true);
      setError(null);


      let submissionId = submissionIdsRef.current.get(question);
      if (!submissionId) {
        submissionId = crypto.randomUUID();
        submissionIdsRef.current.set(question, submissionId);
      }

      const requestPayload: AnswerRequestDto = {
        submissionId,
        vocabularyId: question.vocabularyId,
        questionType: question.questionType,
        answer,
        responseTimeSeconds: responseTimeSeconds ?? 0,
        hintCount: usedHintCount,
      };

      // ── Debug log ──
      console.group(`📤 [AnswerRequest] Q${currentQuestionIndex + 1}/${questionsBatch.length}`);
      console.groupEnd();

      const response: AnswerResponseDto = await answerQuiz(sessionId, requestPayload);
      submissionIdsRef.current.delete(question);
      const newStageIndex = response.newStageIndex;

      if (setCurrentCorrectAnswered) {
        setCurrentCorrectAnswered(
          Math.max(0, Math.min(25, (currentCorrectAnswered || 0) + (response.isCorrect ? 1 : -1)))
        );
      }

      if (response.isCorrect) {
        setComboCount(c => c + 1);
        const nextLevelType = levelToType[newStageIndex] || QuestionTypeEnum.Listening;
        if (response.isVocabularyCompleted) {
          setLevelFeedback({
            message: `🎉 Mastered "${question.word}"!`,
            isCompleted: true,
          });
        } else {
          setLevelFeedback({
            message: `✅ Stage ${newStageIndex + 1}: ${nextLevelType}`,
          });
        }
      } else {
        setComboCount(0);
        const previousStageIndex = Math.max(0, newStageIndex);
        const retryType = levelToType[previousStageIndex];
        setLevelFeedback({
          message: `🔄 Retry ${retryType} for "${question.word}"`,
        });
        // Only apply catch rate penalty if buff does NOT reduce penalty
        if (!petReducePenalty) {
          setCatchRate((prev) => Math.max(0, prev - 0.05));
        }
      }

      onResult?.(response.isCorrect);

      setTimeout(() => setLevelFeedback(null), 3000);

      return response.isCorrect;
    } catch (err) {
      setError("Failed to process answer");
      console.error("Answer error:", err);
      onResult?.(false);
      return false;
    } finally {
      setLoading(false);
    }
  }, [sessionId, currentQuestionIndex, questionsBatch.length, currentCorrectAnswered, setCurrentCorrectAnswered, petReducePenalty, levelToType]);

  // Called by AnswerScreen when user clicks "Tiếp theo" — loads next question
  const confirmAndNext = useCallback(() => {
    loadNextQuestion();
  }, [loadNextQuestion]);


  useEffect(() => {
    if (questionsBatch.length === 0 && currentQuestion === null && !loading && !sessionData) {
      handleCompleteSession();
    }
  }, [questionsBatch.length, currentQuestion, loading, sessionData, handleCompleteSession]);


  return {
    currentQuestion,
    loading,
    error,
    handleAnswer,
    confirmAndNext,
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
    hintBalance,
    setHintBalance,
    comboCount,
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
