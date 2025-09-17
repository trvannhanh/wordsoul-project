/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable @typescript-eslint/no-unused-vars */
import { useEffect, useState } from "react";
import { useLocation, useNavigate, useParams, useSearchParams } from "react-router-dom";
import {
  answerQuiz,
  completeLearningSession,
  completeReviewSession,
  fetchQuizOfSession,
  updateProgress,
} from "../../services/learningSession";
import { fetchPetById } from "../../services/pet";
import { type CompleteLearningSessionResponseDto, type CompleteReviewSessionResponseDto, type Pet, type QuizQuestion, QuestionType } from "../../types/Dto";
import BackgroundMusic from "../../components/LearningSession/BackgroundMusic";
import GameScreen from "../../components/LearningSession/GameScreen";
import AnswerScreen from "../../components/LearningSession/AnswerScreen";
import PetScreen from "../../components/LearningSession/PetScreen";

const LearningSession: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [searchParams] = useSearchParams();
  const mode = searchParams.get("mode") as "learning" | "review";
  const sessionId = Number(id);
  const { state } = useLocation();
  const navigate = useNavigate();
  const petId = state?.petId;

  const [questions, setQuestions] = useState<QuizQuestion[]>([]);
  const [pet, setPet] = useState<Pet>();
  const [queue, setQueue] = useState<(QuizQuestion & { isRetry: boolean })[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showRewardAnimation, setShowRewardAnimation] = useState(false);
  const [captureComplete, setCaptureComplete] = useState(false);
  const [encounteredPet, setEncounteredPet] = useState<{ id: number; name: string; imageUrl: string } | null>(null);
  const [sessionData, setSessionData] = useState<CompleteLearningSessionResponseDto | CompleteReviewSessionResponseDto | null>(null);
  const [isBackgroundMusicPlaying, setIsBackgroundMusicPlaying] = useState(true);
  const [backgroundMusicVolume] = useState(0.5);

  // Danh sách các loại câu hỏi
  const typeOrder: QuestionType[] = [QuestionType.Flashcard, QuestionType.FillInBlank, QuestionType.MultipleChoice, QuestionType.Listening];

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      try {
        // Lấy câu hỏi sai trước, nếu không có thì lấy câu hỏi mới
        let data = await fetchQuizOfSession(sessionId, { includeRetries: true });
        if (data.length === 0) {
          data = await fetchQuizOfSession(sessionId, { includeRetries: false });
        }
        console.log("Fetched quiz data:", data);
        setQuestions(data);

        const grouped = new Map<QuestionType, Map<number, QuizQuestion>>();
        data.forEach(q => {
          if (!grouped.has(q.questionType)) grouped.set(q.questionType, new Map());
          grouped.get(q.questionType)!.set(q.vocabularyId, q);
        });

        setQueue(data.map(q => ({ ...q, isRetry: q.isRetry || false })));

        if (mode === "learning" && data.length > 0) {
          const pet = await fetchPetById(petId);
          setPet(pet);
          setEncounteredPet({ id: pet.id, name: pet.name, imageUrl: pet.imageUrl || "https://via.placeholder.com/100" });
        }
        setLoading(false);
      } catch (err) {
        console.error("Error fetching quiz:", err);
        setError("Failed to load quiz questions");
        setLoading(false);
      }
    };
    fetchData();
  }, [sessionId, mode, petId]);

  const handleAnswer = async (question: QuizQuestion, answer: string) => {
    try {
      const currentItem = queue[currentIndex];
      if (!currentItem) {
        console.error("No current item in queue at index:", currentIndex);
        setError("No current question available");
        return false;
      }

      console.log("Sending answer to API:", {
        sessionId,
        vocabularyId: currentItem.vocabularyId,
        questionType: currentItem.questionType,
        answer,
      });

      const res = await answerQuiz(sessionId, {
        vocabularyId: currentItem.vocabularyId,
        questionType: currentItem.questionType,
        answer,
      });

      console.log("API response:", res);

      setTimeout(() => {
        setCurrentIndex((prev) => prev + 1);
      }, 1500);

      return res.isCorrect;
    } catch (error) {
      console.error("Error in handleAnswer:", error);
      setError("Failed to process answer");
      return false;
    }
  };

  useEffect(() => {
    if (currentIndex >= queue.length && queue.length > 0) {
      console.log("Queue completed. Current state:", {
        currentIndex,
        queueLength: queue.length,
      });

      // Lấy câu hỏi sai hoặc câu hỏi mới từ backend
      const fetchNextQuestions = async () => {
        setLoading(true);
        try {
          let nextQuestions = await fetchQuizOfSession(sessionId, { includeRetries: true });
          if (nextQuestions.length === 0) {
            nextQuestions = await fetchQuizOfSession(sessionId, { includeRetries: false });
          }

          if (nextQuestions.length === 0) {
            handleCompleteSession();
          } else {
            console.log("Next questions:", nextQuestions);
            setQueue(nextQuestions.map(q => ({ ...q, isRetry: q.isRetry || false })));
            setCurrentIndex(0);

            const grouped = new Map<QuestionType, Map<number, QuizQuestion>>();
            nextQuestions.forEach(q => {
              if (!grouped.has(q.questionType)) grouped.set(q.questionType, new Map());
              grouped.get(q.questionType)!.set(q.vocabularyId, q);
            });
          }
          setLoading(false);
        } catch (err) {
          console.error("Error fetching next questions:", err);
          setError("Failed to load next questions");
          setLoading(false);
        }
      };

      fetchNextQuestions();
    }
  }, [currentIndex, queue.length, sessionId]);

  async function handleCompleteSession() {
    try {
      let data: CompleteLearningSessionResponseDto | CompleteReviewSessionResponseDto;
      if (mode === "learning") {
        data = await completeLearningSession(sessionId);
      } else {
        data = await completeReviewSession(sessionId);
      }
      setSessionData(data);
      setShowRewardAnimation(true);
    } catch (error) {
      console.error("Error completing session:", error);
      setError("Failed to complete session");
    }
  }

  const toggleBackgroundMusic = () => {
    setIsBackgroundMusicPlaying((prev) => !prev);
  };

  const handleCloseReward = () => {
    setShowRewardAnimation(false);
    setCaptureComplete(false);
    navigate(-1);
  };

  const currentItem = queue[currentIndex];
  const currentQuestion: QuizQuestion | null = currentItem ? { ...currentItem } : null;

  return (
    <div className="h-screen w-screen bg-gray-900 flex items-center justify-between p-4 pixel-background relative">
      <BackgroundMusic
        isPlaying={isBackgroundMusicPlaying}
        volume={backgroundMusicVolume}
        showRewardAnimation={showRewardAnimation}
        toggleMusic={toggleBackgroundMusic}
      />
      <div className="w-1/2 h-3/4 bg-gray-800 border-4 border-black rounded-lg flex flex-col overflow-hidden">
        <div className="flex-1 bg-gray-700 border-b-4 border-black p-4">
          <div className="h-full bg-black border-2 border-white rounded-sm flex items-center justify-center">
            <GameScreen question={currentQuestion} loading={loading} error={error} />
          </div>
        </div>
        <div className="h-4 bg-cyan-900 border-y-2 border-black flex items-center justify-center">
          <div className="w-1/4 h-3/4 bg-black rounded-full border-2 border-white"></div>
        </div>
        <div className="flex-1 bg-gray-700 p-4">
          <div className="h-full bg-black border-2 border-white rounded-sm flex items-center justify-center">
            <AnswerScreen
              question={currentQuestion}
              loading={loading}
              error={error}
              handleAnswer={handleAnswer}
            />
          </div>
        </div>
      </div>
      <PetScreen
        showRewardAnimation={showRewardAnimation}
        captureComplete={captureComplete}
        setCaptureComplete={setCaptureComplete}
        encounteredPet={encounteredPet}
        sessionData={sessionData}
        mode={mode}
        handleCloseReward={handleCloseReward}
      />
    </div>
  );
};

export default LearningSession;