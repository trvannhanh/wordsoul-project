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
import { type CompleteLearningSessionResponseDto, type CompleteReviewSessionResponseDto, type Pet, type QuizQuestion } from "../../types/Dto";
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
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [pet, setPet] = useState<Pet>();
  const [currentIndex, setCurrentIndex] = useState(0);
  const [retryQueue, setRetryQueue] = useState<QuizQuestion[]>([]);
  const [remainingByVocab, setRemainingByVocab] = useState<Map<number, number>>(new Map());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showRewardAnimation, setShowRewardAnimation] = useState(false);
  const [captureComplete, setCaptureComplete] = useState(false);
  const [encounteredPet, setEncounteredPet] = useState<{ id: number; name: string; imageUrl: string } | null>(null);
  const [sessionData, setSessionData] = useState<CompleteLearningSessionResponseDto | CompleteReviewSessionResponseDto | null>(null);
  const [answerFeedback, setAnswerFeedback] = useState<"correct" | "wrong" | null>(null);
  const [showFeedback, setShowFeedback] = useState(false);
  const [isBackgroundMusicPlaying, setIsBackgroundMusicPlaying] = useState(true);
  const [backgroundMusicVolume] = useState(0.5);

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      try {
        const data = await fetchQuizOfSession(sessionId);
        setQuestions(data);

        const pet = await fetchPetById(petId);
        setPet(pet);

        const map = new Map<number, number>();
        data.forEach((q) => map.set(q.vocabularyId, (map.get(q.vocabularyId) || 0) + 1));
        setRemainingByVocab(map);

        if (mode === "learning" && data.length > 0) {
          setEncounteredPet({ id: pet.id, name: pet.name, imageUrl: pet.imageUrl || "https://via.placeholder.com/100" });
        }
        setLoading(false);
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      } catch (err) {
        setError("Failed to load quiz questions");
        setLoading(false);
      }
    };
    fetchData();
  }, [sessionId, mode, petId]);

  async function handleAnswer(question: QuizQuestion, answer: string) {
    try {
      const res = await answerQuiz(sessionId, {
        vocabularyId: question.vocabularyId,
        questionType: question.questionType,
        answer,
      });

      setAnswerFeedback(res.isCorrect ? "correct" : "wrong");
      setShowFeedback(true);

      if (res.isCorrect) {
        const newMap = new Map(remainingByVocab);
        newMap.set(question.vocabularyId, (newMap.get(question.vocabularyId) || 1) - 1);
        setRemainingByVocab(newMap);

        if ((newMap.get(question.vocabularyId) || 0) === 0) {
          await updateProgress(sessionId, question.vocabularyId);
        }
      } else {
        setRetryQueue((prev) => [...prev, question]);
      }

      setTimeout(() => {
        setShowFeedback(false);
        setAnswerFeedback(null);
        setCurrentIndex((prev) => prev + 1);
      }, 1000);
    } catch (error) {
      console.error("Error in handleAnswer:", error);
      setError("Failed to process answer");
    }
  }

  const currentQuestion = currentIndex < questions.length
    ? questions[currentIndex]
    : retryQueue.length > 0
      ? retryQueue[0]
      : null;

  useEffect(() => {
    if (!currentQuestion && questions.length > 0) {
      const unfinished = Array.from(remainingByVocab.values()).some((v) => v > 0);
      if (!unfinished) handleCompleteSession();
    }
  }, [currentQuestion, questions]);

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
              showFeedback={showFeedback}
              answerFeedback={answerFeedback}
              handleAnswer={handleAnswer}
            />
          </div>
        </div>
      </div>
      <PetScreen
        showRewardAnimation={showRewardAnimation}
        captureComplete={captureComplete}
        setCaptureComplete={setCaptureComplete} // Truyền setCaptureComplete
        encounteredPet={encounteredPet}
        sessionData={sessionData}
        mode={mode}
        handleCloseReward={handleCloseReward}
      />
    </div>
  );
};

export default LearningSession;