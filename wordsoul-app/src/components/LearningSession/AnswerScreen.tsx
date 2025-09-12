import { motion, AnimatePresence } from "framer-motion";
import { type QuizQuestion, QuestionType } from "../../types/Dto";

interface AnswerScreenProps {
  question: QuizQuestion | null;
  loading: boolean;
  error: string | null;
  showFeedback: boolean;
  answerFeedback: "correct" | "wrong" | null;
  handleAnswer: (question: QuizQuestion, answer: string) => void;
}

const AnswerScreen: React.FC<AnswerScreenProps> = ({
  question,
  loading,
  error,
  showFeedback,
  answerFeedback,
  handleAnswer,
}) => {
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
            case QuestionType.Flashcard:
              return (
                <button
                  onClick={() => handleAnswer(question, "viewed")}
                  className="bg-emerald-600 w-3/4 h-1/4 border-2 border-white rounded-lg font-pixel text-white hover:bg-emerald-700"
                  disabled={showFeedback}
                >
                  Đã Xem
                </button>
              );
            case QuestionType.FillInBlank:
              return (
                <input
                  type="text"
                  className="bg-white p-2 rounded w-3/4 text-black font-pixel"
                  onKeyDown={(e) => {
                    if (e.key === "Enter" && !showFeedback) {
                      handleAnswer(question, (e.target as HTMLInputElement).value);
                      (e.target as HTMLInputElement).value = "";
                    }
                  }}
                  disabled={showFeedback}
                />
              );
            case QuestionType.MultipleChoice:
              return (
                <div className="grid grid-cols-2 gap-2 w-full">
                  {question.options?.map((opt) => (
                    <button
                      key={opt}
                      onClick={() => handleAnswer(question, opt)}
                      className="bg-emerald-600 p-2 rounded-lg font-pixel text-white hover:bg-emerald-700"
                      disabled={showFeedback}
                    >
                      {opt}
                    </button>
                  ))}
                </div>
              );
            case QuestionType.Listening:
              return (
                <div className="flex flex-col items-center">
                  <audio controls src={question.pronunciationUrl ?? ""} className="mb-2" />
                  <input
                    type="text"
                    className="bg-white p-2 rounded w-3/4 text-black font-pixel"
                    onKeyDown={(e) => {
                      if (e.key === "Enter" && !showFeedback) {
                        handleAnswer(question, (e.target as HTMLInputElement).value);
                        (e.target as HTMLInputElement).value = "";
                      }
                    }}
                    disabled={showFeedback}
                  />
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