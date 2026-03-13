import type { QuizQuestionDto } from "../../types/LearningSessionDto";
import { QuestionTypeEnum } from "../../types/LearningSessionDto";

interface GameScreenProps {
  question: QuizQuestionDto | null;
  loading: boolean;
  error: string | null;
}

// Renders a context sentence with ___ highlighted in yellow
const ContextSentence: React.FC<{ sentence: string }> = ({ sentence }) => {
  const parts = sentence.split("___");
  return (
    <p className="text-2xl font-pixel text-white leading-relaxed text-center px-4">
      {parts.map((part, i) => (
        <span key={i}>
          {part}
          {i < parts.length - 1 && (
            <span className="text-yellow-300 bg-yellow-900 bg-opacity-50 px-2 rounded font-bold">
              ___
            </span>
          )}
        </span>
      ))}
    </p>
  );
};

const GameScreen: React.FC<GameScreenProps> = ({ question, loading, error }) => {
  if (loading) return <div className="text-white font-pixel"></div>;
  if (error) return <div className="text-red-500 font-pixel">{error}</div>;
  if (!question) return <div className="text-white font-pixel">Hoàn thành session!</div>;

  const { questionType, questionPrompt, word, meaning, pronunciation, partOfSpeech, imageUrl } = question;

  // ── MultipleChoice (Proposal A): Hiển thị Nghĩa → User chọn Từ đúng ──────
  if (questionType === QuestionTypeEnum.MultipleChoice) {
    return (
      <div className="relative w-full h-full flex flex-col items-center justify-center gap-4 px-6">
        {/* Question type badge */}
        <span className="text-xs font-pixel text-emerald-300 border border-emerald-600 px-3 py-1 rounded-full bg-emerald-900 bg-opacity-50 uppercase tracking-widest">
          Chọn từ đúng
        </span>
        {/* The meaning is the question */}
        <p className="text-white font-pixel text-3xl text-center leading-snug">
          {questionPrompt ?? meaning}
        </p>
        {/* Supporting info */}
        <div className="flex gap-3 text-xs font-pixel text-gray-400">
          {pronunciation && <span>/{pronunciation}/</span>}
          {partOfSpeech && <span className="text-blue-300">{partOfSpeech}</span>}
        </div>
        {imageUrl && (
          <img src={imageUrl} alt={word} className="w-20 h-20 object-contain mx-auto" />
        )}
      </div>
    );
  }

  // ── FillInBlank (Proposal B): Hiển thị câu ví dụ có ___ nếu có, fallback Nghĩa ──
  if (questionType === QuestionTypeEnum.FillInBlank) {
    return (
      <div className="relative w-full h-full flex flex-col items-center justify-center gap-4 px-6">
        <span className="text-xs font-pixel text-blue-300 border border-blue-600 px-3 py-1 rounded-full bg-blue-900 bg-opacity-50 uppercase tracking-widest">
          Điền từ vào chỗ trống
        </span>
        {questionPrompt ? (
          <ContextSentence sentence={questionPrompt} />
        ) : (
          // Fallback: hiển thị nghĩa như cũ
          <p className="text-white font-pixel text-3xl text-center">{meaning}</p>
        )}
        <div className="flex gap-3 text-xs font-pixel text-gray-400">
          {pronunciation && <span>/{pronunciation}/</span>}
          {partOfSpeech && <span className="text-blue-300">{partOfSpeech}</span>}
        </div>
        {imageUrl && (
          <img src={imageUrl} alt={word} className="w-20 h-20 object-contain mx-auto" />
        )}
      </div>
    );
  }

  // ── Flashcard (Level 0): Hiển thị word + meaning như cũ ──────────────────
  if (questionType === QuestionTypeEnum.Flashcard) {
    return (
      <div className="relative w-full h-full flex flex-col items-center justify-center gap-3 px-6">
        <span className="text-xs font-pixel text-gray-400 border border-gray-600 px-3 py-1 rounded-full uppercase tracking-widest">
          Ghi nhớ từ mới
        </span>
        <h2 className="text-5xl font-pixel text-white text-center">{word}</h2>
        <h3 className="text-2xl font-pixel text-gray-300 text-center">{meaning}</h3>
        {pronunciation && <p className="text-sm font-pixel text-gray-400">/{pronunciation}/</p>}
        {partOfSpeech && <p className="text-sm font-pixel text-blue-300">{partOfSpeech}</p>}
        {imageUrl && (
          <img src={imageUrl} alt={word} className="w-20 h-20 sm:w-24 sm:h-24 object-contain mx-auto mt-2" />
        )}
      </div>
    );
  }

  // ── Listening (Level 3): Hiển thị gợi ý nhẹ (giữ nguyên logic AnswerScreen xử lý audio) ──
  return (
    <div className="relative w-full h-full flex flex-col items-center justify-center gap-3 px-6">
      <span className="text-xs font-pixel text-purple-300 border border-purple-600 px-3 py-1 rounded-full bg-purple-900 bg-opacity-50 uppercase tracking-widest">
        Nghe và gõ lại từ
      </span>
      {partOfSpeech && <p className="text-sm font-pixel text-blue-300">{partOfSpeech}</p>}
      {imageUrl && (
        <img src={imageUrl} alt={word} className="w-24 h-24 object-contain mx-auto" />
      )}
    </div>
  );
};

export default GameScreen;