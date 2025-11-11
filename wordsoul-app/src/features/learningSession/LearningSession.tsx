import { useEffect, useState, useCallback } from "react";
import { useParams, useSearchParams, useNavigate, useLocation } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import GameScreen from "../../components/LearningSession/GameScreen";
import AnswerScreen from "../../components/LearningSession/AnswerScreen";
import BackgroundMusic from "../../components/LearningSession/BackgroundMusic";
import PetScreen from "../../components/LearningSession/PetScreen";
import { useQuizSession } from "../../hooks/LearningSession/useQuizSession";
import type { QuizQuestionDto } from "../../types/LearningSessionDto";
import LoadingScreen from "../../components/LearningSession/LoadingScreen";
import { useAuth } from "../../hooks/Auth/useAuth";
import { fetchPetById } from "../../services/pet";

const LearningSession: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [searchParams] = useSearchParams();
  const mode = searchParams.get("mode") as "learning" | "review";
  const sessionId = Number(id);
  const { state } = useLocation();
  const navigate = useNavigate();
  const petId = state?.petId;
  const catchRate = state?.catchRate;
  const [currentCorrectAnswered, setCurrentCorrectAnswered] = useState(state?.currentCorrectAnswered || 0);

  const {
    currentQuestion,
    loading,
    error,
    handleAnswer: originalHandleAnswer,
    sessionData,
    encounteredPet,
    showRewardAnimation,
    captureComplete,
    setCaptureComplete,
    loadNextQuestion,
    catchRate: currentCatchRate,
  } = useQuizSession(sessionId, mode, petId, catchRate, currentCorrectAnswered, setCurrentCorrectAnswered);

  // States for battle animation
  const [isAnswerCorrect, setIsAnswerCorrect] = useState<boolean | null>(null);
  const [showBattleAnimation, setShowBattleAnimation] = useState(false);

  // State for toggling PetScreen
  const [showPetScreen, setShowPetScreen] = useState(true);

  const { user } = useAuth();
  const [isPlaying, setIsPlaying] = useState(true);
  const [showPopup, setShowPopup] = useState(false);
  const [answeredQuestion, setAnsweredQuestion] = useState<QuizQuestionDto | null>(null);
  const [showIntro, setShowIntro] = useState(true);
  const [userPet, setUserPet] = useState<{
    id: number;
    name: string;
    imageUrl: string;
  } | null>(null);

  useEffect(() => {
    const timer = setTimeout(() => setShowIntro(false), 1200);
    return () => clearTimeout(timer);
  }, []);

  useEffect(() => {
    const fetchPet = async () => {
      if (typeof user?.petActiveId === "number") {
        try {
          const pet = await fetchPetById(user.petActiveId);
          setUserPet({
            id: pet.id,
            name: pet.name,
            imageUrl: pet.imageUrl || "https://via.placeholder.com/100",
          });
        } catch (petError) {
          console.warn("Failed to load pet:", petError);
        }
      }
    };
    fetchPet();
  }, [user?.petActiveId])

  const toggleMusic = () => {
    setIsPlaying((prev) => !prev);
  };

  // Toggle PetScreen
  const togglePetScreen = () => {
    setShowPetScreen((prev) => !prev);
  };

  // Handle close reward
  const handleCloseReward = () => {
    navigate(-1);
  };

  // Wrapped handleAnswer to trigger battle animation
  const handleAnswer = useCallback(async (
    question: QuizQuestionDto,
    answer: string,
    onAnswerProcessed: () => void
  ): Promise<boolean> => {
    return originalHandleAnswer(question, answer, onAnswerProcessed, (correct) => {
      setIsAnswerCorrect(correct);
      setShowBattleAnimation(true);
      // Reset after animation (shorter than feedback timeout to allow overlap)
      setTimeout(() => {
        setShowBattleAnimation(false);
        setIsAnswerCorrect(null);
      }, 1500);
    });
  }, [originalHandleAnswer]);

  const handleShowPopup = useCallback((question: QuizQuestionDto) => {
    setAnsweredQuestion(question);
    setShowPopup(true);
    setTimeout(() => {
      setShowPopup(false);
      setAnsweredQuestion(null);
    }, 3000);
  }, []);

  if (showIntro) {
    return <LoadingScreen />;
  }

  // Calculate progress percentage with a maximum of 25 questions
  const MAX_QUESTIONS = 20;
  const progressPercentage = Math.min(Math.max((currentCorrectAnswered / MAX_QUESTIONS) * 100, 0), 100);

  // Dynamic width for Learning Container
  const learningContainerClass = showPetScreen 
    ? "w-full sm:w-10/12 lg:w-3/4" 
    : "w-full sm:w-10/12 lg:w-5/6";

  // Hàm để định dạng thông báo khi kết thúc phiên học
  const getMessage = () => {
    if (mode === "learning" && sessionData && "isPetAlreadyOwned" in sessionData) {
      if (sessionData.isPetAlreadyOwned) {
        return "Bạn đã sở hữu pet này!";
      }
      if (sessionData.isPetRewardGranted) {
        return `Chúc mừng! Bạn đã bắt được ${sessionData.petName}!`;
      }
      return `${sessionData.petName} đã bỏ trốn!`;
    }
    return sessionData?.message || "Hoàn thành phiên học!";
  };

  return (
    <div className="h-screen w-screen bg-gray-900 flex flex-col sm:flex-row items-center justify-between p-4 pixel-background relative">
      <BackgroundMusic
        isPlaying={isPlaying}
        volume={0.5}
        showRewardAnimation={showRewardAnimation}
        toggleMusic={toggleMusic}
      />

      {/* Learning Container */}
      <div className={`${learningContainerClass} h-full sm:h-full lg:h-full bg-gray-800 border-4 border-black rounded-lg flex flex-col overflow-hidden`}>
        {/* Progress Bar */}
        <div className="p-4 flex justify-between items-center">
          <div className="flex-1">
            <div className="w-full bg-gray-700 rounded-full h-6 border-2 border-white overflow-hidden">
              <motion.div
                className="bg-green-500 h-full"
                initial={{ width: 0 }}
                animate={{ width: `${progressPercentage}%` }}
                transition={{ duration: 0.5, ease: "easeInOut" }}
              />
            </div>
            <p className="text-white text-center mt-2 font-pixel">
              Correct Answers: {currentCorrectAnswered} / {MAX_QUESTIONS}
            </p>
          </div>
          {/* Toggle PetScreen Button */}
          <motion.button
            onClick={togglePetScreen}
            className="ml-4 bg-gray-600 hover:bg-gray-500 text-white px-4 py-2 rounded border-2 border-white font-pixel text-sm custom-cursor"
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
            title={showPetScreen ? "Hide Pet" : "Show Pet"}
          >
            {showPetScreen ? "Hide Pet" : "Show Pet"}
          </motion.button>
        </div>

        <div className="flex-1 bg-gray-700 border-b-4 border-black p-4 h-1/2">
          <div className="h-full bg-black border-2 border-white rounded-sm flex items-center justify-center">
            <GameScreen
              question={currentQuestion}
              loading={loading}
              error={error}
            />
          </div>
        </div>

        <div className="flex-1 bg-gray-700 p-4 h-1/2">
          <div className="h-full bg-black border-2 border-white rounded-sm flex items-center justify-center">
            <AnswerScreen
              question={currentQuestion}
              loading={loading}
              error={error}
              handleAnswer={handleAnswer}
              loadNextQuestion={loadNextQuestion}
              showPopup={handleShowPopup}
            />
          </div>
        </div>
      </div>

      {/* Conditionally render PetScreen */}
      {showPetScreen && (
        <PetScreen
          showRewardAnimation={showRewardAnimation}
          captureComplete={captureComplete}
          setCaptureComplete={setCaptureComplete}
          encounteredPet={encounteredPet}
          userPet={userPet}
          sessionData={sessionData}
          mode={mode}
          petId={petId}
          catchRate={currentCatchRate}
          showBattleAnimation={showBattleAnimation}
          isAnswerCorrect={isAnswerCorrect} 
          handleCloseReward={function (): void {
            throw new Error("Function not implemented.");
          } }        />
      )}

      <AnimatePresence>
        {showPopup && answeredQuestion && (
          <motion.div
            className="absolute inset-0 flex items-center justify-center bg-opacity-75 z-50"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3 }}
          >
            <motion.div
              className="bg-gray-800 p-8 rounded-lg border-4 border-white text-white font-pixel text-center w-3/4 max-w-lg"
              initial={{ scale: 0, y: 50 }}
              animate={{ scale: 1, y: 0 }}
              exit={{ scale: 0, y: 50 }}
              transition={{ duration: 0.3 }}
            >
              <h2 className="text-4xl mb-4">{answeredQuestion.word}</h2>
              <p className="text-2xl mb-2">Nghĩa: {answeredQuestion.meaning}</p>
              <p className="text-lg mb-2">Phát âm: {answeredQuestion.pronunciation || "N/A"}</p>
              <p className="text-lg mb-2">Loại từ: {answeredQuestion.partOfSpeech || "N/A"}</p>
              {answeredQuestion.imageUrl && (
                <img
                  src={answeredQuestion.imageUrl}
                  alt={answeredQuestion.word}
                  className="w-48 h-48 object-contain mx-auto mt-4"
                />
              )}
            </motion.div>
          </motion.div>
        )}

        {/* Reward Complete Screen Overlay */}
        {showRewardAnimation && sessionData && captureComplete && (
          <motion.div
            className="absolute inset-0 flex items-center justify-center bg-opacity-75 z-50"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3 }}
          >
            <div className="flex flex-col items-center justify-center text-center bg-opacity-70 p-4 rounded-lg w-3/4 h-3/4 max-w-4xl bg-gray-800 border-4 border-white">
              {mode === "learning" &&
                !("isPetRewardGranted" in sessionData && sessionData.isPetRewardGranted) && (
                  <motion.p
                    className="text-red-500 font-pixel mb-4"
                    initial={{ scale: 0.8, opacity: 0 }}
                    animate={{ scale: 1, opacity: 1 }}
                  >
                    {"petName" in sessionData && sessionData.petName ? `${sessionData.petName} đã bỏ trốn!` : "No new pet reward available"}
                  </motion.p>
                )}
              {mode === "review" && (
                <motion.p
                  className="text-red-500 font-pixel mb-4"
                  initial={{ scale: 0.8, opacity: 0 }}
                  animate={{ scale: 1, opacity: 1 }}
                >
                  Zapdos đã bỏ trốn!
                </motion.p>
              )}

              <motion.h2
                className="text-xl text-white font-pixel mb-4"
                initial={{ y: 20, opacity: 0 }}
                animate={{ y: 0, opacity: 1 }}
              >
                {getMessage()}
              </motion.h2>

              <div className="space-y-2 mb-4">
                <motion.p className="text-green-900 font-pixel">
                  💰 XP: +{sessionData.xpEarned}
                </motion.p>
                {mode === "review" && "apEarned" in sessionData && (
                  <motion.p className="text-blue-400 font-pixel">
                    💎 AP: +{sessionData.apEarned}
                  </motion.p>
                )}
              </div>

              {/* Pet Reward Display (chỉ cho mode learning khi bắt thành công) */}
              {mode === "learning" &&
                "isPetRewardGranted" in sessionData &&
                sessionData.petId &&
                sessionData.isPetRewardGranted && (
                  <motion.div
                    className="flex flex-col items-center space-y-2 mb-6 p-4 bg-yellow-900 bg-opacity-80 rounded-lg"
                    initial={{ scale: 0, rotate: 180 }}
                    animate={{ scale: 1, rotate: 0 }}
                    transition={{ delay: 0.5, type: "spring" }}
                  >
                    <img
                      src={`https://img.pokemondb.net/sprites/black-white/anim/normal/${encounteredPet?.name.toLowerCase()}.gif`}
                      alt={sessionData.petName}
                      className="w-50 h-50 object-contain pixel-art rounded-lg border-2 border-yellow-400"
                      onError={(e) => {
                        e.currentTarget.src = encounteredPet?.imageUrl ?? "";
                      }}
                    />
                    <h3 className="text-yellow-300 font-pixel text-lg">
                      {sessionData.petName}
                    </h3>
                    <div className="text-xs text-yellow-200 space-y-1">
                      <p>Type: {sessionData.petType}</p>
                      <p>Rarity: {sessionData.petRarity}</p>
                    </div>
                  </motion.div>
                )}

              <motion.button
                onClick={handleCloseReward}
                className="bg-emerald-600 px-6 py-3 rounded-lg text-white font-pixel border-2 border-white hover:bg-emerald-700 transition-colors custom-cursor"
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                initial={{ y: 20, opacity: 0 }}
                animate={{ y: 0, opacity: 1 }}
                transition={{ delay: 0.8 }}
              >
                🎉 Đóng & Về Dashboard
              </motion.button>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default LearningSession;