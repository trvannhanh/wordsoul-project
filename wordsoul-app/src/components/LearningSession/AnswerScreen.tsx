import { motion, AnimatePresence } from "framer-motion";
import { useState, useRef, useEffect } from "react";
import { QuestionTypeEnum, type QuizQuestionDto } from "../../types/LearningSessionDto";

import { consumeHint } from "../../services/user";

interface AnswerScreenProps {
  question: QuizQuestionDto | null;
  loading: boolean;
  error: string | null;
  handleAnswer: (question: QuizQuestionDto, answer: string, onAnswerProcessed: () => void, onResult?: (isCorrect: boolean) => void, responseTimeSeconds?: number, usedHintCount?: number) => Promise<boolean>;
  loadNextQuestion: () => void;
  showPopup: (question: QuizQuestionDto) => void; // Callback để hiển thị pop-up
  hintBalance?: number;
  setHintBalance?: (value: number) => void;
}

const AnswerScreen: React.FC<AnswerScreenProps> = ({
  question,
  loading,
  error,
  handleAnswer,
  loadNextQuestion,
  showPopup,
  hintBalance,
  setHintBalance,
}) => {
  const [answerFeedback, setAnswerFeedback] = useState<"correct" | "wrong" | null>(null);
  const [showFeedback, setShowFeedback] = useState(false);
  const [userAnswer, setUserAnswer] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);

  const audioRef = useRef<HTMLAudioElement>(null);
  const [isPlaying, setIsPlaying] = useState(false);
  const [audioProgress, setAudioProgress] = useState(0);
  const [audioDuration, setAudioDuration] = useState(0);

  const startTimeRef = useRef<number>(Date.now());

  const [usedHint, setUsedHint] = useState(false);
  const [eliminatedOptions, setEliminatedOptions] = useState<string[]>([]);
  const [isConsumingHint, setIsConsumingHint] = useState(false);

  useEffect(() => {
    if (question) {
      startTimeRef.current = Date.now();
      setUsedHint(false);
      setEliminatedOptions([]);
    }
  }, [question]);

  const handleUseHint = async () => {
    if (usedHint || !hintBalance || hintBalance <= 0 || !question || isConsumingHint) return;

    setIsConsumingHint(true);
    try {
      await consumeHint();
      setHintBalance?.(hintBalance - 1);
      setUsedHint(true);

      if (question.questionType === QuestionTypeEnum.MultipleChoice && question.options) {
        const wrongOptions = question.options.filter((opt) => opt !== question.word);
        const toEliminate = wrongOptions.sort(() => 0.5 - Math.random()).slice(0, 2);
        setEliminatedOptions(toEliminate);
      }
    } catch (e) {
      console.error("Failed to use hint", e);
    } finally {
      setIsConsumingHint(false);
    }
  };

  const handleSubmitAnswer = async (answer: string) => {
    if (question) {
      const capturedStartTime = startTimeRef.current;
      const responseTimeSeconds = (Date.now() - capturedStartTime) / 1000;

      console.log("⏱️ Start time:", capturedStartTime);
      console.log("⏱️ Now:", Date.now());
      console.log("⏱️ ResponseTime calculated:", responseTimeSeconds);

      const isCorrect = await handleAnswer(
        question,
        answer,
        () => {
          loadNextQuestion(); // Chuyển câu hỏi sau khi pop-up đóng
        },
        undefined,
        responseTimeSeconds,
        usedHint ? 1 : 0
      );
      setUserAnswer("");
      setShowFeedback(true);
      setAnswerFeedback(isCorrect ? "correct" : "wrong");
      showPopup(question); // Gọi callback để hiển thị pop-up
      setTimeout(() => {
        setShowFeedback(false);
        setAnswerFeedback(null);
        console.log("Response time:", responseTimeSeconds);
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
    if (question && question.questionType === QuestionTypeEnum.Listening && inputRef.current) {
      inputRef.current.focus();
    }
  }, [question]);

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

  if (loading) return <div className="text-white font-pixel"></div>;
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
              className={`absolute top-0 text-2xl font-pixel ${answerFeedback === "correct" ? "text-green-500" : "text-red-500"
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
                  className="bg-emerald-600 w-3/4 h-2/4 border-2 border-white rounded-lg font-pixel text-white custom-cursor hover:bg-emerald-700 disabled:opacity-50"
                  disabled={showFeedback}
                >
                  Đã Xem
                </button>
              );

            case QuestionTypeEnum.FillInBlank:
              return (
                <div className="flex flex-col items-center space-y-4 w-3/4">
                  {usedHint && (
                    <motion.div
                      initial={{ opacity: 0, y: -10 }}
                      animate={{ opacity: 1, y: 0 }}
                      className="bg-yellow-900 border-2 border-yellow-500 text-yellow-300 font-pixel px-4 py-2 rounded shadow-lg"
                    >
                      Bắt đầu: {question.word.charAt(0).toUpperCase()} ... Cuối: {question.word.charAt(question.word.length - 1).toUpperCase()}
                    </motion.div>
                  )}
                  <input
                    ref={inputRef}
                    type="text"
                    value={userAnswer}
                    className="bg-white p-4 rounded w-full text-2xl text-black font-pixel text-center focus:outline-none focus:ring-2 focus:ring-blue-500"
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
                </div>
              );

            case QuestionTypeEnum.MultipleChoice:
              return (
                <div className="grid grid-cols-2 gap-6 w-4/5">
                  {question.options?.map((opt) => {
                    const isEliminated = eliminatedOptions.includes(opt);
                    return (
                      <motion.button
                        key={opt}
                        onClick={() => handleSubmitAnswer(opt)}
                        className={`p-5 rounded-lg font-pixel text-white text-2xl transition-colors custom-cursor ${
                          isEliminated
                            ? "bg-gray-600 opacity-30 cursor-not-allowed"
                            : "bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50"
                        }`}
                        disabled={showFeedback || isEliminated}
                        whileHover={!isEliminated ? { scale: 1.05 } : {}}
                        whileTap={!isEliminated ? { scale: 0.95 } : {}}
                      >
                        {isEliminated ? "" : opt}
                      </motion.button>
                    );
                  })}
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
                    className="w-full flex flex-col items-center"
                    initial={{ opacity: 0, y: 10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.2 }}
                  >
                    {usedHint && (
                      <motion.div
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="mb-3 bg-yellow-900 border-2 border-yellow-500 text-yellow-300 font-pixel text-xl px-4 py-2 rounded shadow-lg tracking-widest"
                      >
                        {Array(question.word.length).fill("_").join(" ")} ({question.word.length} chữ cái)
                      </motion.div>
                    )}
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

        {/* Nút Hint: Gắn cố định góc trái dưới box câu hỏi */}
        {question.questionType !== QuestionTypeEnum.Flashcard && (
          <div className="absolute bottom-4 left-4">
            <button
              onClick={handleUseHint}
              disabled={usedHint || isConsumingHint || !hintBalance || hintBalance <= 0 || showFeedback}
              className={`flex items-center space-x-2 px-4 py-2 rounded-full font-pixel transition-colors border-2
                ${usedHint
                  ? "bg-gray-700 text-gray-400 border-gray-600 opacity-50 cursor-not-allowed"
                  : (!hintBalance || hintBalance <= 0)
                  ? "bg-red-900 text-red-300 border-red-700 opacity-50 cursor-not-allowed"
                  : "bg-yellow-600 text-white border-yellow-400 hover:bg-yellow-500 custom-cursor"
                }
              `}
            >
              <span>💡 Hint</span>
              <span className="bg-black bg-opacity-50 px-2 py-0.5 rounded-full text-sm">
                {hintBalance || 0}
              </span>
            </button>
          </div>
        )}
      </div>
    </motion.div>
  );
};

export default AnswerScreen;