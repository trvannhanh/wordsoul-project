import { useEffect, useState } from "react";
import { useParams, useSearchParams, useNavigate, useLocation } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion"; // Thêm framer-motion
import GameScreen from "../../components/LearningSession/GameScreen";
import AnswerScreen from "../../components/LearningSession/AnswerScreen";
import BackgroundMusic from "../../components/LearningSession/BackgroundMusic";
import PetScreen from "../../components/LearningSession/PetScreen";
import { useQuizSession } from "../../hooks/LearningSession/useQuizSession";
import type { QuizQuestionDto } from "../../types/LearningSessionDto";
import LoadingScreen from "../../components/LearningSession/LoadingScreen";


const LearningSession: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [searchParams] = useSearchParams();
  const mode = searchParams.get("mode") as "learning" | "review";
  const sessionId = Number(id);
  const { state } = useLocation();
  const navigate = useNavigate();
  const petId = state?.petId;
  const catchRate = state?.catchRate;

  const {
    currentQuestion,
    loading,
    error,
    handleAnswer,
    sessionData,
    encounteredPet,
    showRewardAnimation,
    captureComplete,
    setCaptureComplete,
    loadNextQuestion,
    catchRate: currentCatchRate,
  } = useQuizSession(sessionId, mode, petId, catchRate);

  const [isPlaying, setIsPlaying] = useState(true);
  const [showPopup, setShowPopup] = useState(false); // Trạng thái pop-up
  const [answeredQuestion, setAnsweredQuestion] = useState<QuizQuestionDto | null>(null); // Câu hỏi vừa trả lời

  // 👇 intro animation state
  const [showIntro, setShowIntro] = useState(true);

  useEffect(() => {
    const timer = setTimeout(() => setShowIntro(false), 1200); // chạy 1.5s rồi tắt
    return () => clearTimeout(timer);
  }, []);

  const toggleMusic = () => {
    setIsPlaying((prev) => !prev);
  };

  // Callback để hiển thị pop-up từ AnswerScreen
  const handleShowPopup = (question: QuizQuestionDto) => {
    setAnsweredQuestion(question);
    setShowPopup(true);
    setTimeout(() => {
      setShowPopup(false);
      setAnsweredQuestion(null);
    }, 3000); // Đóng pop-up sau 3 giây
  };

  if (showIntro) {
    return <LoadingScreen />;
  }

  return (
    <div className="h-screen w-screen bg-gray-900 flex flex-col sm:flex-row items-center justify-between p-4 pixel-background relative">
      <BackgroundMusic 
        isPlaying={isPlaying} 
        volume={0.5} 
        showRewardAnimation={showRewardAnimation}
        toggleMusic={toggleMusic}
      />
      
      <div className="w-full sm:w-10/12 lg:w-3/4 h-full sm:h-full lg:h-full bg-gray-800 border-4 border-black rounded-lg flex flex-col overflow-hidden">
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
              showPopup={handleShowPopup} // Truyền callback để hiển thị pop-up
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
        petId={petId}
        handleCloseReward={() => navigate(-1)}
        catchRate={currentCatchRate}
      />

      {/* Pop-up hiển thị thông tin từ vựng */}
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
      </AnimatePresence>
    </div>
  );
};

export default LearningSession;