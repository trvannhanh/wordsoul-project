import { useEffect, useState } from "react";
import SpriteCharacter from "../../components/SpriteCharacter";
import { answerQuiz, completeSession, fetchQuizOfSession, updateProgress } from "../../services/learningSession";
import type { QuizQuestion } from "../../types/Dto";
import { useParams } from "react-router-dom";



export default function LearningSession() {
  const { id } = useParams<{ id: string }>();
  const sessionId = Number(id);
  const [questions, setQuestions] = useState<QuizQuestion[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [retryQueue, setRetryQueue] = useState<QuizQuestion[]>([]);
  const [remainingByVocab, setRemainingByVocab] = useState<Map<number, number>>(new Map());
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Load all questions when component mounts
  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      try {
        const data = await fetchQuizOfSession(sessionId);
        setQuestions(data);
        const map = new Map<number, number>();
        data.forEach(q => map.set(q.vocabId, (map.get(q.vocabId) || 0) + 1));
        setRemainingByVocab(map);
        setLoading(false);
      } catch (error) {
        setError('Failed to load quiz questions');
        setLoading(false);
        console.error('Error fetching quiz questions:', error);
      }
    };
    fetchData();
  }, [sessionId]);

  async function handleAnswer(question: QuizQuestion, answer: string) {
  const data = await answerQuiz(sessionId, { questionId: question.id, answer });

  if (data.isCorrect) {
    const newMap = new Map(remainingByVocab);
    newMap.set(question.vocabId, (newMap.get(question.vocabId) || 1) - 1);
    setRemainingByVocab(newMap);

    if ((newMap.get(question.vocabId) || 0) === 0) {
      await updateProgress(sessionId, question.vocabId);
      console.log(`Vocab ${question.vocabId} hoàn thành!`);
    }
  } else {
    setRetryQueue(prev => [...prev, question]); // không mutate
  }

  setCurrentIndex(prev => prev + 1);
}

const currentQuestion =
  currentIndex < questions.length ? questions[currentIndex] : retryQueue[0];

useEffect(() => {
  if (!currentQuestion && questions.length > 0) {
    const unfinished = Array.from(remainingByVocab.values()).some(v => v > 0);
    if (!unfinished) handleCompleteSession();
  }
}, [currentQuestion]);


  async function handleCompleteSession() {
    const data = await completeSession(sessionId);
    console.log(data);
    alert("Bạn đã hoàn thành session!");
  }

  if (!currentQuestion) return <div>Đang tải câu hỏi...</div>;
  if (loading) return <div>Loading...</div>;
  if (error) return <div>{error}</div>;

  return (
    <>
      <div className="h-screen">
        {/* Bên trên */}
        <div className="relative w-full min-h-[15rem] max-h-[50rem] h-2/5 flex items-center justify-center top-[3.25rem]">
          {/* Layer 1 (dưới cùng) */}
          <div className="absolute w-full inset-0 bg-[url('./src/assets/background_layer_1.png')] bg-contain bg-center  z-0"></div>
          {/* Layer 2 (giữa) */}
          <div className="absolute w-full inset-0 bg-[url('./src/assets/background_layer_2.png')] bg-contain bg-center  z-10"></div>
          {/* Layer 3 (trên cùng) */}
          <div className="absolute w-full inset-0 bg-[url('./src/assets/background_layer_3.png')] bg-contain bg-center  z-20"></div>

          {/* Nội dung trên cùng */}
          <div className="relative z-30 text-white text-4xl font-pixel">
            <p>{currentQuestion.prompt}</p>

          </div>


          <div className="absolute bottom-5 left-1/2 -translate-x-1/2 z-30">
            <SpriteCharacter />
          </div>
        </div>
        {/* Bên dưới */}

        <div className="bg-amber-300 h-3/5 flex items-center">
          <div className="container h-1/2 mx-auto w-7/12 grid grid-cols-2 gap-10 bg-amber-600 text-center">
            {currentQuestion.options ? (
              currentQuestion.options.map(opt => (
                <div className="bg-white">
                  <button className="bg-amber-200 w-full h-full text-3xl font-pixel custom-cursor" key={opt}
                    onClick={() => handleAnswer(currentQuestion, opt)}>{opt}</button>
                </div>
              ))
            ) : (
              <input
                type="text"
                onKeyDown={e => {
                  if (e.key === "Enter") {
                    handleAnswer(currentQuestion, (e.target as HTMLInputElement).value);
                    (e.target as HTMLInputElement).value = "";
                  }
                }}
              />
            )}
          </div>
        </div>
      </div>
    </>
  );
};
