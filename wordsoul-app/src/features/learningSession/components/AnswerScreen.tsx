import { motion, AnimatePresence } from "framer-motion";
import { useState, useRef, useEffect } from "react";
import { QuestionTypeEnum, type QuizQuestionDto } from "../../../types/LearningSessionDto";
import { consumeHint } from "../../../services/user";

interface AnswerScreenProps {
  question: QuizQuestionDto | null;
  loading: boolean;
  error: string | null;
  handleAnswer: (
    question: QuizQuestionDto,
    answer: string,
    onResult?: (isCorrect: boolean) => void,
    responseTimeSeconds?: number,
    usedHintCount?: number
  ) => Promise<boolean>;
  /** Called when user explicitly confirms to go to next question */
  confirmAndNext?: () => void;
  showPopup: (question: QuizQuestionDto) => void;
  hintBalance?: number;
  setHintBalance?: (value: number) => void;
  /** Called with result so parent (ReviewLayout) can react */
  onAnswerResult?: (isCorrect: boolean) => void;
  comboCount?: number;
}

const AnswerScreen: React.FC<AnswerScreenProps> = ({
  question,
  loading,
  error,
  handleAnswer,
  showPopup,
  hintBalance,
  setHintBalance,
  onAnswerResult,
  comboCount = 0,
}) => {
  const [answerFeedback, setAnswerFeedback] = useState<"correct" | "wrong" | null>(null);
  const [showFeedback, setShowFeedback] = useState(false);
  /** true = waiting for user to click "Tiếp theo" before loading next question */
  const [waitingConfirm, setWaitingConfirm] = useState(false);
  const [userAnswer, setUserAnswer] = useState("");
  const inputRef = useRef<HTMLInputElement>(null);

  const audioRef = useRef<HTMLAudioElement>(null);
  const [isPlaying, setIsPlaying] = useState(false);
  const [audioProgress, setAudioProgress] = useState(0);
  const [audioDuration, setAudioDuration] = useState(0);

  // XP floaty text
  const [showXpFloaty, setShowXpFloaty] = useState(false);

  const startTimeRef = useRef<number>(Date.now());

  const [usedHint, setUsedHint] = useState(false);
  const [eliminatedOptions, setEliminatedOptions] = useState<string[]>([]);
  const [isConsumingHint, setIsConsumingHint] = useState(false);

  // Reset per question
  useEffect(() => {
    if (question) {
      startTimeRef.current = Date.now();
      setUsedHint(false);
      setEliminatedOptions([]);
      setWaitingConfirm(false);
      setShowFeedback(false);
      setAnswerFeedback(null);
      setShowXpFloaty(false);
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
    if (!question || waitingConfirm) return;
    const responseTimeSeconds = (Date.now() - startTimeRef.current) / 1000;
    const isCorrect = await handleAnswer(
      question,
      answer,
      (result) => { onAnswerResult?.(result); },
      responseTimeSeconds,
      usedHint ? 1 : 0
    );
    setUserAnswer("");
    setShowFeedback(true);
    setAnswerFeedback(isCorrect ? "correct" : "wrong");
    if (isCorrect) {
      setShowXpFloaty(true);
      setTimeout(() => setShowXpFloaty(false), 1400);
    }
    showPopup(question);
    // Enter "waiting for confirm" mode — user must press "Tiếp theo"
    setWaitingConfirm(true);
  };

  const handlePlayAudio = () => {
    if (audioRef.current && question?.pronunciationUrl) {
      audioRef.current.currentTime = 0;
      audioRef.current.play().then(() => setIsPlaying(true)).catch(console.error);
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
    audio.addEventListener("timeupdate", updateProgress);
    audio.addEventListener("loadedmetadata", updateProgress);
    audio.addEventListener("ended", handleAudioEnded);
    return () => {
      audio.removeEventListener("timeupdate", updateProgress);
      audio.removeEventListener("loadedmetadata", updateProgress);
      audio.removeEventListener("ended", handleAudioEnded);
    };
  }, [question]);

  useEffect(() => {
    if (question?.questionType === QuestionTypeEnum.Listening && question.pronunciationUrl) {
      const audio = audioRef.current;
      if (!audio) return;
      audio.currentTime = 0;
      const handleReady = () => {
        audio.play().then(() => setIsPlaying(true)).catch(console.warn);
      };
      if (audio.readyState >= 3) {
        handleReady();
      } else {
        audio.addEventListener("canplaythrough", handleReady, { once: true });
      }
      return () => audio.removeEventListener("canplaythrough", handleReady);
    }
  }, [question]);

  if (loading) return <div className="text-white font-pixel text-sm" />;
  if (error) return <div className="text-red-400 font-pixel text-sm">{error}</div>;
  if (!question) return <div className="text-white font-pixel text-sm">Hoàn thành session!</div>;

  const showHintButton = question.questionType !== QuestionTypeEnum.Flashcard;

  return (
    <motion.div
      className="h-full w-full flex flex-col"
      animate={{
        borderColor: showFeedback
          ? answerFeedback === "correct" ? "#22c55e" : "#ef4444"
          : "transparent",
        borderWidth: showFeedback ? 2 : 0,
      }}
      transition={{ duration: 0.2 }}
    >
      {/* Feedback sound */}
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

      {/* Answer content area */}
      <div className="flex-1 flex items-center justify-center relative">
        {/* Correct/Wrong banner */}
        <AnimatePresence>
          {showFeedback && (
            <motion.div
              className={`absolute top-2 left-1/2 -translate-x-1/2 z-10 font-pixel text-lg px-4 py-1 rounded-full border ${answerFeedback === "correct"
                ? "text-green-400 border-green-600 bg-green-950"
                : "text-red-400 border-red-600 bg-red-950"
                }`}
              initial={{ scale: 0, opacity: 0, y: -10 }}
              animate={{ scale: 1, opacity: 1, y: 0 }}
              exit={{ scale: 0, opacity: 0 }}
              transition={{ duration: 0.25 }}
            >
              {answerFeedback === "correct" ? "✓ Correct!" : "✗ Wrong!"}
            </motion.div>
          )}
        </AnimatePresence>

        {/* +XP floaty */}
        <AnimatePresence>
          {showXpFloaty && (
            <motion.div
              className="absolute top-10 right-6 z-20 font-pixel text-yellow-300 text-sm pointer-events-none select-none"
              initial={{ opacity: 1, y: 0, scale: 1 }}
              animate={{ opacity: 0, y: -36, scale: 1.2 }}
              exit={{ opacity: 0 }}
              transition={{ duration: 1.2, ease: "easeOut" }}
            >
              +XP ✨
            </motion.div>
          )}
        </AnimatePresence>

        {/* Combo flash */}
        <AnimatePresence>
          {comboCount >= 2 && showFeedback && answerFeedback === "correct" && (
            <motion.div
              key={comboCount}
              className="absolute bottom-10 left-1/2 -translate-x-1/2 z-20 font-pixel text-amber-300 bg-amber-950 border border-amber-600 px-3 py-1 rounded-full text-xs pointer-events-none"
              initial={{ scale: 0.5, opacity: 0, y: 10 }}
              animate={{ scale: 1, opacity: 1, y: 0 }}
              exit={{ scale: 0.5, opacity: 0 }}
              transition={{ duration: 0.3, type: "spring" }}
            >
              🔥 ×{comboCount} Combo!
            </motion.div>
          )}
        </AnimatePresence>

        {/* Retry badge */}
        {/* <AnimatePresence>
          {question.isRetry && !showFeedback && (
            <motion.div
              className="absolute top-2 right-3 z-10 font-pixel text-orange-300 bg-orange-950 border border-orange-700 px-2 py-0.5 rounded-full text-xs"
              initial={{ opacity: 0, x: 10 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0 }}
            >
              🔄 Thử lại
            </motion.div>
          )}
        </AnimatePresence> */}

        {/* ── Question UI ── */}
        {(() => {
          switch (question.questionType) {
            case QuestionTypeEnum.Flashcard:
              return (
                <motion.button
                  onClick={() => handleSubmitAnswer("viewed")}
                  className="bg-emerald-700 w-3/4 h-2/4 border-2 border-emerald-500 rounded-lg font-pixel text-white text-lg custom-cursor hover:bg-emerald-600 disabled:opacity-50 transition-colors"
                  disabled={showFeedback}
                  whileHover={{ scale: 1.02 }}
                  whileTap={{ scale: 0.98 }}
                >
                  Đã Xem
                </motion.button>
              );

            case QuestionTypeEnum.FillInBlank:
              return (
                <div className="flex flex-col items-center gap-3 w-4/5">
                  <AnimatePresence>
                    {usedHint && (
                      <motion.div
                        initial={{ opacity: 0, y: -8 }}
                        animate={{ opacity: 1, y: 0 }}
                        exit={{ opacity: 0 }}
                        className="bg-amber-950 border border-amber-700 text-amber-300 font-pixel text-sm px-4 py-2 rounded-lg text-center"
                      >
                        Bắt đầu: <strong>{question.word.charAt(0).toUpperCase()}</strong>
                        {" "}... Cuối: <strong>{question.word.charAt(question.word.length - 1).toUpperCase()}</strong>
                      </motion.div>
                    )}
                  </AnimatePresence>
                  <input
                    ref={inputRef}
                    type="text"
                    value={userAnswer}
                    className="bg-gray-900 border-2 border-gray-600 focus:border-blue-500 p-3 rounded-lg w-full text-xl text-white font-pixel text-center focus:outline-none transition-colors"
                    autoFocus
                    onChange={(e) => setUserAnswer(e.target.value)}
                    onKeyDown={(e) => {
                      if (e.key === "Enter" && !showFeedback && !waitingConfirm && userAnswer.trim()) {
                        handleSubmitAnswer(userAnswer);
                      }
                    }}
                    disabled={showFeedback || waitingConfirm}
                    placeholder="Gõ từ vựng..."
                  />
                  <motion.button
                    onClick={() => userAnswer.trim() && handleSubmitAnswer(userAnswer)}
                    disabled={showFeedback || waitingConfirm || !userAnswer.trim()}
                    className="w-full py-2 bg-emerald-700 border border-emerald-500 rounded-lg font-pixel text-white text-sm hover:bg-emerald-600 disabled:opacity-40 transition-colors custom-cursor"
                    whileHover={{ scale: 1.01 }}
                    whileTap={{ scale: 0.98 }}
                  >
                    Xác nhận
                  </motion.button>
                </div>
              );

            case QuestionTypeEnum.MultipleChoice:
              return (
                <div className="grid grid-cols-2 gap-4 w-4/5">
                  {question.options?.map((opt) => {
                    const isEliminated = eliminatedOptions.includes(opt);
                    return (
                      <motion.button
                        key={opt}
                        onClick={() => !isEliminated && handleSubmitAnswer(opt)}
                        className={`p-4 rounded-lg font-pixel text-xl text-center transition-colors custom-cursor border ${isEliminated
                          ? "bg-gray-800 border-gray-700 text-gray-600 cursor-not-allowed"
                          : "bg-gray-800 border-gray-600 text-white hover:bg-emerald-800 hover:border-emerald-500 disabled:opacity-50"
                          }`}
                        disabled={showFeedback || waitingConfirm || isEliminated}
                        whileHover={!isEliminated ? { scale: 1.03, y: -1 } : {}}
                        whileTap={!isEliminated ? { scale: 0.97 } : {}}
                      >
                        {isEliminated ? (
                          <span className="opacity-30 line-through">{opt}</span>
                        ) : opt}
                      </motion.button>
                    );
                  })}
                </div>
              );

            case QuestionTypeEnum.Listening:
              return (
                <div className="flex flex-col items-center gap-3 w-4/5 max-w-md">
                  {/* Audio player */}
                  <div className="w-full bg-gray-900 rounded-lg p-3 border border-gray-700">
                    <div className="flex items-center justify-center gap-3 mb-2">
                      <motion.button
                        onClick={handlePlayAudio}
                        disabled={isPlaying || showFeedback}
                        className={`px-5 py-2 rounded-full font-pixel text-sm border-2 disabled:opacity-50 transition-all ${isPlaying
                          ? "bg-red-900 border-red-600 text-red-300"
                          : "bg-blue-900 border-blue-600 text-blue-300 hover:bg-blue-800"
                          }`}
                        whileHover={{ scale: 1.04 }}
                        whileTap={{ scale: 0.96 }}
                      >
                        {isPlaying ? "⏸ Playing..." : "▶ Play"}
                      </motion.button>
                    </div>

                    {audioDuration > 0 && (
                      <div className="space-y-1">
                        <div className="w-full bg-gray-700 rounded-full h-1.5">
                          <motion.div
                            className="bg-blue-500 h-1.5 rounded-full"
                            animate={{ width: `${audioProgress}%` }}
                            transition={{ duration: 0.1 }}
                          />
                        </div>
                        <div className="flex justify-between text-xs text-gray-500 font-pixel">
                          <span>{Math.floor(audioProgress)}%</span>
                          <span>{Math.floor(audioDuration)}s</span>
                        </div>
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
                  </div>

                  <AnimatePresence>
                    {usedHint && (
                      <motion.div
                        initial={{ opacity: 0, y: -8 }}
                        animate={{ opacity: 1, y: 0 }}
                        exit={{ opacity: 0 }}
                        className="bg-amber-950 border border-amber-700 text-amber-300 font-pixel text-sm px-4 py-2 rounded-lg tracking-widest"
                      >
                        {Array(question.word.length).fill("_").join(" ")} ({question.word.length} chữ)
                      </motion.div>
                    )}
                  </AnimatePresence>

                  <input
                    type="text"
                    value={userAnswer}
                    placeholder="Type what you hear..."
                    className="bg-gray-900 border-2 border-gray-600 focus:border-purple-500 p-3 rounded-lg w-full text-white font-pixel text-center text-lg focus:outline-none transition-colors disabled:opacity-50"
                    autoFocus
                    disabled={showFeedback || waitingConfirm}
                    onChange={(e) => setUserAnswer(e.target.value)}
                    onKeyDown={(e) => {
                      if (e.key === "Enter" && !showFeedback && !waitingConfirm && userAnswer.trim()) {
                        handleSubmitAnswer(userAnswer);
                      }
                    }}
                  />
                  <motion.button
                    onClick={() => userAnswer.trim() && handleSubmitAnswer(userAnswer)}
                    disabled={showFeedback || waitingConfirm || !userAnswer.trim()}
                    className="w-full py-2 bg-purple-800 border border-purple-600 rounded-lg font-pixel text-purple-200 text-sm hover:bg-purple-700 disabled:opacity-40 transition-colors custom-cursor"
                    whileHover={{ scale: 1.01 }}
                    whileTap={{ scale: 0.98 }}
                  >
                    Xác nhận
                  </motion.button>
                </div>
              );

            default:
              return <div className="text-white font-pixel">Lỗi loại câu hỏi</div>;
          }
        })()}
      </div>

      {/* ── Hint Footer ── */}
      {showHintButton && !waitingConfirm && (
        <div className="flex items-center justify-between px-4 py-2 border-t border-gray-700 bg-gray-900 bg-opacity-60">
          <button
            onClick={handleUseHint}
            disabled={usedHint || isConsumingHint || !hintBalance || hintBalance <= 0 || showFeedback}
            className={`flex items-center gap-2 px-3 py-1.5 rounded-full font-pixel text-xs transition-colors border ${usedHint
              ? "bg-gray-800 text-gray-500 border-gray-700 opacity-50 cursor-not-allowed"
              : !hintBalance || hintBalance <= 0
                ? "bg-red-950 text-red-400 border-red-800 opacity-50 cursor-not-allowed"
                : "bg-amber-950 text-amber-300 border-amber-700 hover:bg-amber-900 custom-cursor"
              }`}
          >
            <span>💡 Gợi ý</span>
            <span className="bg-black bg-opacity-40 px-1.5 py-0.5 rounded-full text-xs">
              {hintBalance ?? 0}
            </span>
          </button>
          <span className="text-xs text-gray-600 font-pixel">
            {question.questionType === QuestionTypeEnum.Flashcard
              ? ""
              : question.questionType === QuestionTypeEnum.MultipleChoice
                ? "Nhấn để trả lời"
                : "Nhấn enter để xác nhận"}
          </span>
        </div>
      )}
    </motion.div>
  );
};

export default AnswerScreen;