import type { QuizQuestionDto } from "../../types/LearningSessionDto";


interface GameScreenProps {
  question: QuizQuestionDto | null;
  loading: boolean;
  error: string | null;
}

const GameScreen: React.FC<GameScreenProps> = ({ question, loading, error }) => {
  if (loading) return <div className="text-white font-pixel">Đang tải...</div>;
  if (error) return <div className="text-red-500 font-pixel">{error}</div>;
  if (!question) return <div className="text-white font-pixel">Hoàn thành session!</div>;

  return (
    <div className="relative w-full h-full flex items-center justify-center">
      <div className="text-white font-pixel text-center">

        
        
        <h2 className="text-5xl">{question.word}</h2>
       
        <h2 className="text-4xl">{question.meaning}</h2>

        <p className="text-sm">{question.pronunciation || "N/A"}</p>
        <p className="text-sm">{question.partOfSpeech || "N/A"}</p>
        {question.imageUrl && <img src={question.imageUrl} alt={question.word} className="w-42 h-42 object-contain mx-auto mt-2" />}
      </div>
    </div>
  );
};

export default GameScreen;