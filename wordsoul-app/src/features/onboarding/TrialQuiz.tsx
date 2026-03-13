import { useState, useCallback } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { QuestionTypeEnum, type QuizQuestionDto } from '../../types/LearningSessionDto';
import GameScreen from '../../components/LearningSession/GameScreen';
import AnswerScreen from '../../components/LearningSession/AnswerScreen';
import PokemonProgressBar from '../../components/LearningSession/PokemonProgressBar';

// ─── Static trial data (no API needed) ───────────────────────────────────────
const TRIAL_QUESTIONS: QuizQuestionDto[] = [
    {
        vocabularyId: -1,
        questionType: QuestionTypeEnum.Flashcard,
        word: 'Apple',
        meaning: 'Quả táo',
        pronunciation: '/ˈæp.əl/',
        partOfSpeech: 'Noun',
        options: ['Quả táo', 'Con mèo', 'Mặt trời', 'Nước'],
        imageUrl: 'https://upload.wikimedia.org/wikipedia/commons/thumb/1/15/Red_Apple.jpg/320px-Red_Apple.jpg',
    },
    {
        vocabularyId: -2,
        questionType: QuestionTypeEnum.MultipleChoice,
        word: 'Sun',
        meaning: 'Mặt trời',
        pronunciation: '/sʌn/',
        partOfSpeech: 'Noun',
        options: ['Cá', 'Mặt trời', 'Trái đất', 'Ngôi sao'],
    },
    {
        vocabularyId: -3,
        questionType: QuestionTypeEnum.FillInBlank,
        word: 'Water',
        meaning: 'Nước',
        pronunciation: '/ˈwɔː.tər/',
        partOfSpeech: 'Noun',
        options: ['Lửa', 'Gió', 'Nước', 'Đất'],
    },
];

interface TrialQuizProps {
    onComplete: () => void;
    onExit?: () => void;
}

const TrialQuiz: React.FC<TrialQuizProps> = ({ onComplete, onExit }) => {
    const [questionIndex, setQuestionIndex] = useState(0);
    const [correctAnswered, setCorrectAnswered] = useState(0);
    const [showPopup, setShowPopup] = useState(false);
    const [answeredQuestion, setAnsweredQuestion] = useState<QuizQuestionDto | null>(null);

    const currentQuestion = TRIAL_QUESTIONS[questionIndex] ?? null;
    const isFinished = questionIndex >= TRIAL_QUESTIONS.length;

    const handleAnswer = useCallback(async (
        question: QuizQuestionDto,
        answer: string,
        onAnswerProcessed: () => void,
        _onResult?: (isCorrect: boolean) => void,
        _responseTime?: number
    ): Promise<boolean> => {
        const correct = question.questionType === QuestionTypeEnum.Flashcard
            ? answer.trim().toLowerCase() === 'viewed'
            : answer.trim().toLowerCase() === question.meaning?.toLowerCase();

        if (correct) setCorrectAnswered(c => c + 1);
        _onResult?.(correct);

        // Wait for AnswerScreen popup to finish, then move to next
        setTimeout(() => {
            onAnswerProcessed();
        }, 2500);

        return correct;
    }, []);

    const loadNextQuestion = useCallback(() => {
        setQuestionIndex(i => {
            const next = i + 1;
            if (next >= TRIAL_QUESTIONS.length) {
                setTimeout(onComplete, 400);
            }
            return next;
        });
    }, [onComplete]);

    const handleShowPopup = useCallback((q: QuizQuestionDto) => {
        setAnsweredQuestion(q);
        setShowPopup(true);
        setTimeout(() => { setShowPopup(false); setAnsweredQuestion(null); }, 2500);
    }, []);

    return (
        <div className="h-screen w-screen bg-gray-900 flex flex-col items-center justify-between pixel-background relative overflow-hidden">
            {/* Exit button */}
            <button
                onClick={() => onExit?.()}
                className="absolute top-3 right-4 z-50 font-pixel text-xs text-gray-400 hover:text-white"
            >
                ✕ Thoát
            </button>

            {/* Trial banner */}
            <div className="absolute top-3 left-4 z-50 font-pixel text-xs text-yellow-300 bg-gray-900/80 px-2 py-1 rounded-lg border border-yellow-700">
                🎓 Bài kiểm tra tân thủ ({questionIndex + 1}/{TRIAL_QUESTIONS.length})
            </div>

            {/* Main Learning Container — identical layout to real LearningSession */}
            <div className="w-full h-full bg-gray-800 border-4 border-black rounded-lg flex flex-col overflow-hidden">
                {/* Progress Bar */}
                <PokemonProgressBar
                    currentCorrectAnswered={correctAnswered}
                    maxQuestions={TRIAL_QUESTIONS.length}
                    catchRate={50}
                    encounteredPet={null}
                />

                {/* Game (word display) */}
                <div className="flex-1 bg-gray-700 border-b-4 border-black p-4 h-1/2">
                    <div className="h-full bg-black border-2 border-white rounded-sm flex items-center justify-center">
                        {isFinished
                            ? <div className="text-white font-pixel text-center text-xl">🎉 Hoàn thành!</div>
                            : <GameScreen question={currentQuestion} loading={false} error={null} />
                        }
                    </div>
                </div>

                {/* Answer options */}
                <div className="flex-1 bg-gray-700 p-4 h-1/2">
                    <div className="h-full bg-black border-2 border-white rounded-sm flex items-center justify-center">
                        {!isFinished && (
                            <AnswerScreen
                                question={currentQuestion}
                                loading={false}
                                error={null}
                                handleAnswer={handleAnswer}
                                loadNextQuestion={loadNextQuestion}
                                showPopup={handleShowPopup}
                            />
                        )}
                    </div>
                </div>
            </div>

            {/* Word detail popup (same as real session) */}
            <AnimatePresence>
                {showPopup && answeredQuestion && (
                    <motion.div
                        className="absolute inset-0 flex items-center justify-center bg-opacity-75 z-50"
                        initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
                        transition={{ duration: 0.3 }}
                    >
                        <motion.div
                            className="bg-gray-800 p-8 rounded-lg border-4 border-white text-white font-pixel text-center w-3/4 max-w-lg"
                            initial={{ scale: 0, y: 50 }} animate={{ scale: 1, y: 0 }}
                            exit={{ scale: 0, y: 50 }} transition={{ duration: 0.3 }}
                        >
                            <h2 className="text-4xl mb-4">{answeredQuestion.word}</h2>
                            <p className="text-2xl mb-2">Nghĩa: {answeredQuestion.meaning}</p>
                            <p className="text-lg mb-2">Phát âm: {answeredQuestion.pronunciation || 'N/A'}</p>
                            <p className="text-lg">Loại từ: {answeredQuestion.partOfSpeech || 'N/A'}</p>
                        </motion.div>
                    </motion.div>
                )}
            </AnimatePresence>
        </div>
    );
};

export default TrialQuiz;
