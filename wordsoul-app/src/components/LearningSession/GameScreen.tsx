import { motion } from "framer-motion";
import type { QuizQuestionDto } from "../../types/LearningSessionDto";
import { QuestionTypeEnum } from "../../types/LearningSessionDto";

interface GameScreenProps {
  question: QuizQuestionDto | null;
  loading: boolean;
  error: string | null;
  mode?: "learning" | "review";
}

const ContextSentence: React.FC<{ sentence: string }> = ({ sentence }) => {
  const parts = sentence.split("___");
  return (
    <p className="text-xl font-pixel text-white leading-relaxed text-center px-4">
      {parts.map((part, i) => (
        <span key={i}>
          {part}
          {i < parts.length - 1 && (
            <span className="text-yellow-300 bg-yellow-900 bg-opacity-40 px-2 rounded font-bold border border-yellow-700">
              ___
            </span>
          )}
        </span>
      ))}
    </p>
  );
};

const TypeBadge: React.FC<{ label: string; variant: "green" | "blue" | "purple" | "gray" }> = ({ label, variant }) => {
  const styles = {
    green: "text-emerald-300 border-emerald-700 bg-emerald-950",
    blue: "text-blue-300 border-blue-700 bg-blue-950",
    purple: "text-purple-300 border-purple-700 bg-purple-950",
    gray: "text-gray-400 border-gray-700 bg-gray-900",
  };
  return (
    <span className={`text-xs font-pixel px-3 py-1 rounded-full border uppercase tracking-widest ${styles[variant]}`}>
      {label}
    </span>
  );
};

const MetaRow: React.FC<{ pronunciation?: string; partOfSpeech?: string }> = ({ pronunciation, partOfSpeech }) => {
  if (!pronunciation && !partOfSpeech) return null;
  return (
    <div className="flex items-center gap-3 flex-wrap justify-center">
      {pronunciation && (
        <span className="text-xs font-pixel text-blue-300 bg-blue-950 border border-blue-800 px-2 py-0.5 rounded">
          /{pronunciation}/
        </span>
      )}
      {partOfSpeech && (
        <span className="text-xs font-pixel text-amber-300 bg-amber-950 border border-amber-800 px-2 py-0.5 rounded">
          {partOfSpeech}
        </span>
      )}
    </div>
  );
};

const GameScreen: React.FC<GameScreenProps> = ({ question, loading, error, mode }) => {
  if (loading) return <div className="text-white font-pixel text-sm animate-pulse">Loading...</div>;
  if (error) return <div className="text-red-400 font-pixel text-sm">{error}</div>;
  if (!question) return <div className="text-white font-pixel text-sm">Hoàn thành session!</div>;

  const { questionType, questionPrompt, word, meaning, pronunciation, partOfSpeech, imageUrl } = question;

  // ── MultipleChoice ────────────────────────────────────────────────
  if (questionType === QuestionTypeEnum.MultipleChoice) {
    return (
      <motion.div
        className="relative w-full h-full flex flex-col items-center justify-center gap-3 px-6"
        initial={{ opacity: 0, y: 8 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3 }}
        key={word + "mc"}
      >
        <TypeBadge label="Chọn từ đúng" variant="green" />

        {/* Decorative divider */}
        <div className="w-16 h-px bg-gradient-to-r from-transparent via-gray-500 to-transparent" />

        {/* Meaning — the actual question */}
        <p className="text-white font-pixel text-2xl text-center leading-snug max-w-xs">
          {questionPrompt ?? meaning}
        </p>

        <MetaRow pronunciation={pronunciation} partOfSpeech={partOfSpeech} />

        {imageUrl && (
          <motion.img
            src={imageUrl}
            alt={word}
            className="w-16 h-16 object-contain mx-auto rounded border border-gray-700"
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ delay: 0.15 }}
          />
        )}
      </motion.div>
    );
  }

  // ── FillInBlank ───────────────────────────────────────────────────
  if (questionType === QuestionTypeEnum.FillInBlank) {
    return (
      <motion.div
        className="relative w-full h-full flex flex-col items-center justify-center gap-3 px-6"
        initial={{ opacity: 0, y: 8 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3 }}
        key={word + "fib"}
      >
        <TypeBadge label="Điền từ vào chỗ trống" variant="blue" />

        <div className="w-16 h-px bg-gradient-to-r from-transparent via-gray-500 to-transparent" />

        {questionPrompt ? (
          <ContextSentence sentence={questionPrompt} />
        ) : (
          <p className="text-white font-pixel text-2xl text-center">{meaning}</p>
        )}

        <MetaRow pronunciation={pronunciation} partOfSpeech={partOfSpeech} />

        {imageUrl && (
          <motion.img
            src={imageUrl}
            alt={word}
            className="w-16 h-16 object-contain mx-auto rounded border border-gray-700"
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ delay: 0.15 }}
          />
        )}
      </motion.div>
    );
  }

  // ── Flashcard ─────────────────────────────────────────────────────
  if (questionType === QuestionTypeEnum.Flashcard) {
    return (
      <motion.div
        className="relative w-full h-full flex flex-col items-center justify-center gap-3 px-6"
        initial={{ opacity: 0, scale: 0.97 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.35 }}
        key={word + "fc"}
      >
        <TypeBadge label="Ghi nhớ từ mới" variant="gray" />

        <div className="w-16 h-px bg-gradient-to-r from-transparent via-gray-500 to-transparent" />

        {/* Word front-and-center */}
        <h2 className="text-5xl font-pixel text-white text-center tracking-wide">{word}</h2>

        {/* Meaning with subtle background */}
        <div className="bg-gray-800 border border-gray-700 rounded-lg px-4 py-2 max-w-xs text-center">
          <h3 className="text-lg font-pixel text-gray-200">{meaning}</h3>
        </div>

        <MetaRow pronunciation={pronunciation} partOfSpeech={partOfSpeech} />

        {imageUrl && (
          <motion.img
            src={imageUrl}
            alt={word}
            className="w-20 h-20 object-contain mx-auto rounded border border-gray-700 mt-1"
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ delay: 0.2 }}
          />
        )}
      </motion.div>
    );
  }

  // ── Listening ─────────────────────────────────────────────────────
  return (
    <motion.div
      className="relative w-full h-full flex flex-col items-center justify-center gap-3 px-6"
      initial={{ opacity: 0, y: 8 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      key={word + "ls"}
    >
      <TypeBadge label="Nghe và gõ lại từ" variant="purple" />

      <div className="w-16 h-px bg-gradient-to-r from-transparent via-gray-500 to-transparent" />

      {/* Visual hint: sound waves icon */}
      <div className="flex items-end gap-1 h-8">
        {[3, 5, 7, 5, 3].map((h, i) => (
          <motion.div
            key={i}
            className="w-1.5 bg-purple-400 rounded-full"
            style={{ height: `${h * 4}px` }}
            animate={{ scaleY: [1, 1.5, 1] }}
            transition={{ duration: 0.8, repeat: Infinity, delay: i * 0.12 }}
          />
        ))}
      </div>

      {partOfSpeech && (
        <span className="text-xs font-pixel text-amber-300 bg-amber-950 border border-amber-800 px-2 py-0.5 rounded">
          {partOfSpeech}
        </span>
      )}

      {imageUrl && (
        <motion.img
          src={imageUrl}
          alt={word}
          className="w-20 h-20 object-contain mx-auto rounded border border-gray-700"
          initial={{ scale: 0.8, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          transition={{ delay: 0.15 }}
        />
      )}

      <p className="text-xs font-pixel text-gray-500 text-center">
        Nghe kỹ và gõ từ bạn nghe được
      </p>
    </motion.div>
  );
};

export default GameScreen;