import { useEffect, useState } from "react";
import { useParams, useSearchParams } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import {
  answerQuiz,
  completeLearningSession,
  completeReviewSession,
  fetchQuizOfSession,
  updateProgress,
} from "../../services/learningSession";
import { QuestionType, type CompleteLearningSessionResponseDto, type CompleteReviewSessionResponseDto, type QuizQuestion } from "../../types/Dto";


export default function LearningSession() {
  const { id } = useParams<{ id: string }>();
  const [searchParams] = useSearchParams();
  const mode = searchParams.get("mode") as "learning" | "review";
  const sessionId = Number(id);

  const [questions, setQuestions] = useState<QuizQuestion[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [retryQueue, setRetryQueue] = useState<QuizQuestion[]>([]);
  const [remainingByVocab, setRemainingByVocab] = useState<Map<number, number>>(
    new Map()
  );
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showCompleteModal, setShowCompleteModal] = useState(false);
  const [sessionData, setSessionData] = useState<CompleteLearningSessionResponseDto | CompleteReviewSessionResponseDto | null>(null);
  const [isCardFlipped, setIsCardFlipped] = useState(false);

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      try {
        const data = await fetchQuizOfSession(sessionId);
        setQuestions(data);

        const map = new Map<number, number>();
        data.forEach((q) =>
          map.set(q.vocabularyId, (map.get(q.vocabularyId) || 0) + 1)
        );
        setRemainingByVocab(map);

        setLoading(false);
      } catch {
        setError("Failed to load quiz questions");
        setLoading(false);
      }
    };
    fetchData();
  }, [sessionId]);

  async function handleAnswer(question: QuizQuestion, answer: string) {
    const res = await answerQuiz(sessionId, {
      vocabularyId: question.vocabularyId,
      questionType: question.questionType,
      answer,
    });

    if (res.isCorrect) {
      const newMap = new Map(remainingByVocab);
      newMap.set(
        question.vocabularyId,
        (newMap.get(question.vocabularyId) || 1) - 1
      );
      setRemainingByVocab(newMap);

      if ((newMap.get(question.vocabularyId) || 0) === 0) {
        await updateProgress(sessionId, question.vocabularyId);
      }
    } else {
      setRetryQueue((prev) => [...prev, question]);
    }

    setCurrentIndex((prev) => prev + 1);
  }

  const currentQuestion =
    currentIndex < questions.length
      ? questions[currentIndex]
      : retryQueue.length > 0
        ? retryQueue[0]
        : null;

  useEffect(() => {
    if (!currentQuestion && questions.length > 0) {
      const unfinished = Array.from(remainingByVocab.values()).some(
        (v) => v > 0
      );
      if (!unfinished) handleCompleteSession();
    }
  }, [currentQuestion]);

  async function handleCompleteSession() {
    try {
      let data;
      if (mode === "learning") {
        data = await completeLearningSession(sessionId);
      } else {
        data = await completeReviewSession(sessionId);
      }
      setSessionData(data);
      setShowCompleteModal(true);
    } catch (error) {
      console.error("Error completing session:", error);
    }
  }

  const handleFlipCard = () => {
    setIsCardFlipped(true);
  };

  const handleCloseModal = () => {
    setShowCompleteModal(false);
    setIsCardFlipped(false);
  };

  const renderQuestion = (q: QuizQuestion) => {
    switch (q.questionType) {
      case QuestionType.Flashcard:
        return (
          <div className="bg-black h-15/16 w-15/16 border-4 border-black flex items-center justify-center">
            <button
              onClick={() => handleAnswer(q, "viewed")}
              className="bg-emerald-600 w-4/12 h-5/12 border-2 border-white rounded-2xl font-pixel custom-cursor"
            >
              Đã Xem
            </button>
          </div>
        );
      case QuestionType.FillInBlank:
        return (
          <div className="bg-black h-15/16 w-15/16 border-4 border-black flex items-center justify-center">
            <input
              type="text"
              className="border p-2 rounded bg-white w-1/2"
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  handleAnswer(q, (e.target as HTMLInputElement).value);
                  (e.target as HTMLInputElement).value = "";
                }
              }}
            />
          </div>
        );
      case QuestionType.MultipleChoice:
        return (
          <div className="bg-black h-15/16 w-15/16 border-4 border-black flex items-center justify-center">
            <div className="grid grid-cols-2 items-center h-full gap-y-2 gap-x-30">
              {q.options?.map((opt) => (
                <button
                  key={opt}
                  onClick={() => handleAnswer(q, opt)}
                  className="bg-emerald-600 w-40 h-20 border-2 border-white rounded-2xl font-pixel custom-cursor"
                >
                  {opt}
                </button>
              ))}
            </div>
          </div>
        );
      case QuestionType.Listening:
        return (
          <div className="bg-black h-15/16 w-15/16 border-4 border-black flex items-center justify-center">
            <input
              type="text"
              className="border p-2 rounded bg-white w-1/2"
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  handleAnswer(q, (e.target as HTMLInputElement).value);
                  (e.target as HTMLInputElement).value = "";
                }
              }}
            />
          </div>
        );
      default:
        return <div>Unknown question type</div>;
    }
  };

  const renderTopScreen = (q: QuizQuestion) => {
    switch (q.questionType) {
      case QuestionType.Flashcard:
        return (
          <div className="bg-black h-15/16 w-15/16 border-4 border-black flex items-center justify-center text-white font-pixel">
            <div className="flex items-start justify-evenly gap-2">
              <div className="flex-col w-1/3">
                <h2 className="text-4xl">{q.word}</h2>
                <p className="text-s">{q.pronunciation || "N/A"}</p>
                <p className="text-s">{q.partOfSpeech || "N/A"}</p>
                <p className="text-s">{q.cefrLevel || "N/A"}</p>
              </div>
              <div className="flex-col w-1/3">
                <p className="text-4xl">{q.meaning}</p>
                <p className="text-s">{q.description || "N/A"}</p>
              </div>
              <div className="w-1/3">
                {q.imageUrl && (
                  <img src={q.imageUrl} alt={q.word} className="w-32 h-32" />
                )}
              </div>
            </div>
          </div>
        );
      case QuestionType.FillInBlank:
        return (
          <div className="bg-black h-15/16 w-15/16 border-4 border-black flex items-center justify-center text-white font-pixel">
            <div className="flex items-start gap-2">
              <div className="flex-col w-1/3">
                <h2 className="text-4xl">{q.meaning}</h2>
                <p className="text-s">{q.pronunciation || "N/A"}</p>
              </div>
            </div>
          </div>
        );
      case QuestionType.MultipleChoice:
        return (
          <div className="bg-black h-15/16 w-15/16 border-4 border-black flex items-center justify-center text-white font-pixel">
            <div className="flex items-start justify-evenly gap-2">
              <div className="flex-col w-1/2">
                <h2 className="text-4xl">{q.meaning}</h2>
                <p className="text-s">{q.pronunciation || "N/A"}</p>
                <p className="text-s">{q.partOfSpeech || "N/A"}</p>
                <p className="text-s">{q.cefrLevel || "N/A"}</p>
              </div>
              <div className="w-1/2">
                {q.imageUrl && (
                  <img src={q.imageUrl} alt={q.meaning} className="w-32 h-32" />
                )}
              </div>
            </div>
          </div>
        );
      case QuestionType.Listening:
        return (
          <div className="bg-black h-15/16 w-15/16 border-4 border-black flex items-center justify-center text-white font-pixel">
            <div className="flex items-start justify-evenly gap-2">
              <audio controls src={q.pronunciationUrl ?? ""}></audio>
            </div>
          </div>
        );
      default:
        return <div>Unknown question type</div>;
    }
  };

  // Hiển thị giao diện chính ngay cả khi không có câu hỏi
  return (
    <div className="h-screen w-screen flex">
      {/* Máy chơi game DS Bên trái */}
      <div className="w-9/12 h-15/16 mt-12 flex-col justify-center items-center rounded-4xl border-2 border-white">
        <div className="h-6/12 w-full flex align-middle justify-center items-center border-2 rounded-2xl bg-gray-900">
          {/* Màn hình trên */}
          <div className="bg-gray-800 h-10/12 w-8/12 border-4 border-black flex items-center justify-center rounded-xl">
            <AnimatePresence mode="wait">
              {loading ? (
                <div>Đang tải...</div>
              ) : error ? (
                <div>{error}</div>
              ) : currentQuestion ? (
                <motion.div
                  key={`top-${currentQuestion.vocabularyId}-${currentQuestion.questionType}`}
                  initial={{ opacity: 0, rotateX: -90 }}
                  animate={{ opacity: 1, rotateX: 0 }}
                  exit={{ opacity: 0, rotateX: 90 }}
                  transition={{ duration: 0.5 }}
                  className="w-full h-full flex justify-center items-center"
                >
                  {renderTopScreen(currentQuestion)}
                </motion.div>
              ) : (
                <div>Hoàn thành session!</div>
              )}
            </AnimatePresence>
          </div>
        </div>
        <div className="bg-cyan-950 h-1/16 w-full rounded-2xl flex justify-center border-4 border-black">
          <div className="bg-cyan-950 h-full w-9/12 rounded-s border-x-4 border-black flex items-center justify-center">
            <div className="bg-black w-1/16 h-full rounded-4xl border-1 border-white"></div>
          </div>
        </div>
        <div className="h-7/16 w-full flex align-middle items-center justify-evenly bg-gray-900 border-2 rounded-2xl">
          {/* Nút điều hướng */}
          <div className="w-2/16 h-5/12 relative">
            <div className="absolute bg-gray-800 w-4/12 h-4/12 left-4/12 border-4 border-black">
              <div className="absolute w-1/16 h-8/12 bg-white top-1 left-7/16"></div>
            </div>
            <div className="absolute bg-gray-800 w-4/12 h-4/12 top-4/12 border-4 border-black">
              <div className="absolute w-8/12 h-1/16 bg-white top-7/16 left-1"></div>
            </div>
            <div className="absolute bg-gray-800 w-4/12 h-4/12 top-4/12 right-0 border-4 border-black">
              <div className="absolute w-8/12 h-1/16 bg-white top-7/16 left-1"></div>
            </div>
            <div className="absolute bg-gray-800 w-4/12 h-4/12 left-4/12 bottom-0 border-4 border-black">
              <div className="absolute w-1/16 h-8/12 bg-white top-1 left-7/16"></div>
            </div>
          </div>
          {/* Màn hình dưới */}
          <div className="bg-gray-800 h-10/12 w-8/12 border-4 border-black flex items-center justify-center rounded-xl">
            <AnimatePresence mode="wait">
              {loading ? (
                <div>Đang tải...</div>
              ) : error ? (
                <div>{error}</div>
              ) : currentQuestion ? (
                <motion.div
                  key={`bottom-${currentQuestion.vocabularyId}-${currentQuestion.questionType}`}
                  initial={{ opacity: 0, rotateX: 90 }}
                  animate={{ opacity: 1, rotateX: 0 }}
                  exit={{ opacity: 0, rotateX: -90 }}
                  transition={{ duration: 0.5 }}
                  className="w-full h-full"
                >
                  {renderQuestion(currentQuestion)}
                </motion.div>
              ) : (
                <div>Hoàn thành session!</div>
              )}
            </AnimatePresence>
          </div>
          {/* Nút bấm */}
          <div className="w-2/16 h-5/12 relative">
            <div className="absolute bg-gray-800 w-4/12 h-4/12 left-4/12 rounded-3xl text-center text-white border-4 border-black">
              A
            </div>
            <div className="absolute bg-gray-800 w-4/12 h-4/12 top-4/12 rounded-3xl text-center text-white border-4 border-black">
              B
            </div>
            <div className="absolute bg-gray-800 w-4/12 h-4/12 top-4/12 right-0 rounded-3xl text-center text-white border-4 border-black">
              X
            </div>
            <div className="absolute bg-gray-800 w-4/12 h-4/12 left-4/12 bottom-0 rounded-3xl text-center text-white border-4 border-black">
              Y
            </div>
          </div>
        </div>
      </div>

      {/* Modal hiển thị khi hoàn thành session */}
      <AnimatePresence>
        {showCompleteModal && sessionData && (
          <motion.div
            className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3 }}
          >
            <motion.div
              className="bg-gray-800 p-6 rounded-xl border-4 border-black flex flex-col items-center w-96"
              initial={{ scale: 0 }}
              animate={{ scale: 1 }}
              exit={{ scale: 0 }}
              transition={{ duration: 0.3 }}
            >
              {/* Thông báo chung */}
              <h2 className="text-2xl text-white font-pixel mb-4">
                {sessionData?.message ?? "Hoàn thành session!"}
              </h2>
              <p className="text-lg text-white font-pixel mb-4">
                Bạn nhận được {sessionData?.xpEarned ?? 0} XP!
              </p>

              {/* Nếu là review thì hiển thị thêm AP */}
              {mode === "review" && "apEarned" in sessionData && (
                <p className="text-lg text-white font-pixel mb-4">
                  Bạn nhận được {sessionData.apEarned ?? 0} AP!
                </p>
              )}

              {/* Nếu là learning thì hiển thị Pet reward */}
              {mode === "learning" &&
                "isPetRewardGranted" in sessionData &&
                sessionData.petId &&
                !sessionData.isPetRewardGranted && (
                  <>
                    <motion.div
                      className="w-64 h-96 bg-gray-900 border-4 border-white rounded-xl flex items-center justify-center"
                      animate={{ rotateY: isCardFlipped ? 180 : 0 }}
                      transition={{ duration: 0.5 }}
                      style={{ transformStyle: "preserve-3d" }}
                    >
                      {/* Mặt sau của card (úp) */}
                      {!isCardFlipped && (
                        <div className="absolute w-full h-full bg-gray-900 flex items-center justify-center text-white font-pixel text-2xl">
                          ???
                        </div>
                      )}
                      {/* Mặt trước của card (thông tin Pet) */}
                      {isCardFlipped && (
                        <div className="absolute w-full h-full bg-white flex flex-col items-center justify-center p-4">
                          <img
                            src={sessionData.imageUrl || "https://via.placeholder.com/150"}
                            alt={sessionData.petName}
                            className="w-32 h-32 mb-4"
                          />
                          <h2 className="text-2xl font-pixel text-black">
                            {sessionData.petName}
                          </h2>
                          <p className="text-sm text-black">
                            Type: {sessionData.petType || "N/A"}
                          </p>
                          <p className="text-sm text-black">
                            Rarity: {sessionData.petRarity || "N/A"}
                          </p>
                          <p className="text-sm text-black">
                            Description: {sessionData.description || "N/A"}
                          </p>
                        </div>
                      )}
                    </motion.div>

                    {/* Nút lật card hoặc đóng modal */}
                    {!isCardFlipped ? (
                      <button
                        onClick={handleFlipCard}
                        className="mt-4 bg-emerald-600 px-4 py-2 rounded-xl text-white font-pixel border-2 border-white"
                      >
                        Lật Card
                      </button>
                    ) : (
                      <button
                        onClick={handleCloseModal}
                        className="mt-4 bg-red-600 px-4 py-2 rounded-xl text-white font-pixel border-2 border-white"
                      >
                        Đóng
                      </button>
                    )}
                  </>
                )}

              {/* Nút đóng nếu không có Pet reward hoặc đang ở review */}
              {((mode === "learning" &&
                "isPetRewardGranted" in sessionData &&
                (!sessionData.petId || sessionData.isPetRewardGranted)) ||
                mode === "review") && (
                  <button
                    onClick={handleCloseModal}
                    className="mt-4 bg-red-600 px-4 py-2 rounded-xl text-white font-pixel border-2 border-white"
                  >
                    Đóng
                  </button>
                )}
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Cột trạng thái Bên phải */}
      <div className="w-3/12 h-full bg-black flex justify-center items-end">
        <div className="h-10/12 w-5/12 bg-green-600"></div>
      </div>
    </div>
  );
}