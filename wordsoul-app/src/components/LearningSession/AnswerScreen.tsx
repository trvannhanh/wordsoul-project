import { motion, AnimatePresence } from "framer-motion";
import { useState, useRef, useEffect } from "react";
import { QuestionTypeEnum, type QuizQuestionDto } from "../../types/LearningSessionDto";

interface AnswerScreenProps {
  question: QuizQuestionDto | null;
  loading: boolean;
  error: string | null;
  handleAnswer: (question: QuizQuestionDto, answer: string, onAnswerProcessed: () => void) => Promise<boolean>;
  loadNextQuestion: () => void;
  showPopup: (question: QuizQuestionDto) => void; // Callback để hiển thị pop-up
}

const AnswerScreen: React.FC<AnswerScreenProps> = ({
  question,
  loading,
  error,
  handleAnswer,
  loadNextQuestion,
  showPopup,
}) => {
  const [answerFeedback, setAnswerFeedback] = useState<"correct" | "wrong" | null>(null);
  const [showFeedback, setShowFeedback] = useState(false);
  const [userAnswer, setUserAnswer] = useState("");

  const audioRef = useRef<HTMLAudioElement>(null);
  const [isPlaying, setIsPlaying] = useState(false);
  const [audioProgress, setAudioProgress] = useState(0);
  const [audioDuration, setAudioDuration] = useState(0);

  const handleSubmitAnswer = async (answer: string) => {
    if (question) {
      const isCorrect = await handleAnswer(question, answer, () => {
        loadNextQuestion(); // Chuyển câu hỏi sau khi pop-up đóng
      });
      setUserAnswer("");
      setShowFeedback(true);
      setAnswerFeedback(isCorrect ? "correct" : "wrong");
      showPopup(question); // Gọi callback để hiển thị pop-up
      setTimeout(() => {
        setShowFeedback(false);
        setAnswerFeedback(null);
      }, 3000);
    }
  };

  const handlePlayAudio = () => {
    if (audioRef.current && question?.pronunciationUrl) {
      audioRef.current.currentTime = 0;
      audioRef.current.play().then(() => {
        setIsPlaying(true);
      }).catch(err => {
        console.error("Error playing audio:", err);
      });
    }
  };

  const handleAudioEnded = () => {
    setIsPlaying(false);
    setAudioProgress(0);
  };

  useEffect(() => {
    const audio = audioRef.current;
    if (!audio) return;

    const updateProgress = () => {
      if (audio.duration) {
        setAudioProgress((audio.currentTime / audio.duration) * 100);
        setAudioDuration(audio.duration);
      }
    };

    const handleTimeUpdate = () => updateProgress();
    const handleLoadedMetadata = () => {
      setAudioDuration(audio.duration);
      updateProgress();
    };

    audio.addEventListener('timeupdate', handleTimeUpdate);
    audio.addEventListener('loadedmetadata', handleLoadedMetadata);
    audio.addEventListener('ended', handleAudioEnded);

    return () => {
      audio.removeEventListener('timeupdate', handleTimeUpdate);
      audio.removeEventListener('loadedmetadata', handleLoadedMetadata);
      audio.removeEventListener('ended', handleAudioEnded);
    };
  }, [question]);

  useEffect(() => {
    if (question && question.questionType === QuestionTypeEnum.Listening) {
      const timer = setTimeout(() => {
        const input = document.querySelector('input[type="text"]') as HTMLInputElement;
        input?.focus();
      }, 100);
      return () => clearTimeout(timer);
    }
  }, [question]);

  useEffect(() => {
    if (question?.questionType === QuestionTypeEnum.Listening && question.pronunciationUrl) {
      const audio = audioRef.current;
      if (!audio) return;

      audio.currentTime = 0;
      const handleReady = () => {
        audio.play()
          .then(() => setIsPlaying(true))
          .catch(err => console.warn("Autoplay bị chặn:", err));
      };

      if (audio.readyState >= 3) {
        handleReady();
      } else {
        audio.addEventListener("canplaythrough", handleReady, { once: true });
      }

      return () => {
        audio.removeEventListener("canplaythrough", handleReady);
      };
    }
  }, [question]);

  if (loading) return <div className="text-white font-pixel">Đang tải...</div>;
  if (error) return <div className="text-red-500 font-pixel">{error}</div>;
  if (!question) return <div className="text-white font-pixel">Hoàn thành session!</div>;

  return (
    <motion.div
      className="h-full w-full flex items-center justify-center"
      animate={{
        borderColor: showFeedback
          ? answerFeedback === "correct"
            ? "#00FF00"
            : "#FF0000"
          : "#FFFFFF",
        borderWidth: showFeedback ? 4 : 2,
        scale: showFeedback && answerFeedback === "wrong" ? [1, 1.05, 1] : 1,
      }}
      transition={{ duration: 0.3 }}
    >
      <div className="relative w-full h-full flex items-center justify-center">
        <AnimatePresence>
          {showFeedback && (
            <motion.div
              className={`absolute top-0 text-2xl font-pixel ${
                answerFeedback === "correct" ? "text-green-500" : "text-red-500"
              }`}
              initial={{ scale: 0, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0, opacity: 0 }}
              transition={{ duration: 0.5 }}
            >
              {answerFeedback === "correct" ? "Correct!" : "Wrong!"}
            </motion.div>
          )}
        </AnimatePresence>

        {showFeedback && (
          <audio
            autoPlay
            src={
              answerFeedback === "correct"
                ? "https://res.cloudinary.com/dqpkxxzaf/video/upload/v1757509870/correct-choice-43861_gjqbjp.mp3"
                : "https://res.cloudinary.com/dqpkxxzaf/video/upload/v1757509870/wrong-47985_mr3adc.mp3"
            }
          />
        )}

        {(() => {
          switch (question.questionType) {
            case QuestionTypeEnum.Flashcard:
              return (
                <button
                  onClick={() => handleSubmitAnswer("viewed")}
                  className="bg-emerald-600 w-3/4 h-1/4 border-2 border-white rounded-lg font-pixel text-white hover:bg-emerald-700 disabled:opacity-50"
                  disabled={showFeedback}
                >
                  Đã Xem
                </button>
              );
              
            case QuestionTypeEnum.FillInBlank:
              return (
                <input
                  type="text"
                  value={userAnswer}
                  className="bg-white p-4 rounded w-3/4 text-2xl text-black font-pixel text-center focus:outline-none focus:ring-2 focus:ring-blue-500"
                  autoFocus
                  onChange={(e) => setUserAnswer(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" && !showFeedback && userAnswer.trim()) {
                      handleSubmitAnswer(userAnswer);
                    }
                  }}
                  disabled={showFeedback}
                  placeholder="Type the word..."
                />
              );
              
            case QuestionTypeEnum.MultipleChoice:
              return (
                <div className="grid grid-cols-2 gap-6 w-4/5">
                  {question.options?.map((opt) => (
                    <motion.button
                      key={opt}
                      onClick={() => handleSubmitAnswer(opt)}
                      className="bg-emerald-600 p-5 rounded-lg font-pixel text-white text-2xl hover:bg-emerald-700 disabled:opacity-50 transition-colors"
                      disabled={showFeedback}
                      whileHover={{ scale: 1.05 }}
                      whileTap={{ scale: 0.95 }}
                    >
                      {opt}
                    </motion.button>
                  ))}
                </div>
              );

            case QuestionTypeEnum.Listening:
              return (
                <div className="flex flex-col items-center space-y-4 w-4/5 max-w-md">
                  <motion.h3 
                    className="text-white font-pixel text-lg text-center"
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                  >
                  </motion.h3>

                  <motion.div 
                    className="w-full bg-gray-800 rounded-lg p-4 border-2 border-gray-600"
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.1 }}
                  >
                    <div className="flex items-center justify-center space-x-3 mb-3">
                      <motion.button
                        onClick={handlePlayAudio}
                        disabled={isPlaying || showFeedback}
                        className={`
                          px-4 py-2 rounded-full font-pixel text-white border-2 border-white
                          disabled:opacity-50 disabled:cursor-not-allowed transition-all
                          ${isPlaying 
                            ? 'bg-red-600 hover:bg-red-700' 
                            : 'bg-blue-600 hover:bg-blue-700'
                          }
                        `}
                        whileHover={{ scale: 1.05 }}
                        whileTap={{ scale: 0.95 }}
                      >
                        {isPlaying ? (
                          <>
                            ⏸️ <span className="ml-1">Playing</span>
                          </>
                        ) : (
                          <>
                            ▶️ <span className="ml-1">Play</span>
                          </>
                        )}
                      </motion.button>
                    </div>

                    {audioDuration > 0 && (
                      <div className="space-y-2">
                        <div className="w-full bg-gray-600 rounded-full h-2">
                          <motion.div 
                            className="bg-blue-500 h-2 rounded-full relative overflow-hidden"
                            initial={{ width: 0 }}
                            animate={{ width: `${audioProgress}%` }}
                            transition={{ duration: 0.1 }}
                          >
                            {isPlaying && (
                              <motion.div
                                className="absolute inset-0 bg-gradient-to-r from-transparent via-white to-transparent opacity-30"
                                animate={{ 
                                  x: ["0%", "100%", "0%"],
                                  opacity: [0.3, 0.5, 0.3]
                                }}
                                transition={{ 
                                  duration: 1.5, 
                                  repeat: Infinity,
                                  ease: "linear"
                                }}
                              />
                            )}
                          </motion.div>
                        </div>
                        
                        <div className="flex justify-between text-xs text-gray-400 font-pixel">
                          <span>{Math.floor(audioProgress)}%</span>
                          <span>{Math.floor(audioDuration)}s</span>
                        </div>
                      </div>
                    )}

                    {question.pronunciation && (
                      <div className="text-center mt-2">
                        <motion.p 
                          className="text-blue-300 text-sm font-mono bg-black bg-opacity-30 px-2 py-1 rounded"
                          initial={{ opacity: 0 }}
                          animate={{ opacity: 1 }}
                          transition={{ delay: 0.2 }}
                        >
                          /{question.pronunciation}/
                        </motion.p>
                      </div>
                    )}

                    {question.pronunciationUrl && (
                      <audio
                        ref={audioRef}
                        src={question.pronunciationUrl}
                        preload="auto"
                        onPlay={() => setIsPlaying(true)}
                        onPause={() => setIsPlaying(false)}
                        onEnded={handleAudioEnded}
                        className="hidden"
                      />
                    )}
                  </motion.div>

                  <motion.div 
                    className="w-full"
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.2 }}
                  >
                    <input
                      type="text"
                      value={userAnswer}
                      placeholder="Type what you hear..."
                      className="bg-white p-3 rounded-lg w-full text-black font-pixel text-center text-lg focus:outline-none focus:ring-2 focus:ring-purple-500 disabled:opacity-50 transition-all"
                      autoFocus
                      disabled={showFeedback}
                      onChange={(e) => setUserAnswer(e.target.value)}
                      onKeyDown={(e) => {
                        if (e.key === "Enter" && !showFeedback && userAnswer.trim()) {
                          handleSubmitAnswer(userAnswer);
                        }
                      }}
                    />
                  </motion.div>
                </div>
              );

            default:
              return <div className="text-white font-pixel">Unknown question type</div>;
          }
        })()}
      </div>
    </motion.div>
  );
};

export default AnswerScreen;