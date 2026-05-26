import { motion } from 'framer-motion';
import type { StruggleWordDto } from '../../types/UserDto';

interface StruggleWordsBoxProps {
    struggleWords: StruggleWordDto[];
    onCreateReviewSession?: () => void;
}

const StruggleWordsBox: React.FC<StruggleWordsBoxProps> = ({ struggleWords, onCreateReviewSession }) => {
    if (!struggleWords || struggleWords.length === 0) {
        return (
            <motion.div
                className="bg-gray-800 bg-opacity-80 rounded-xl p-5 mt-6 border-2 border-green-500 shadow-lg text-center"
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5 }}
            >
                <div className="text-3xl mb-2">🎉</div>
                <h3 className="font-pixel text-sm text-green-400">Không có từ vựng yếu</h3>
                <p className="font-pixel text-[10px] text-gray-300 mt-1 leading-relaxed">
                    Tuyệt vời! Bạn đã trả lời đúng tất cả các từ vựng đã học. Hãy tiếp tục duy trì phong độ này nhé!
                </p>
            </motion.div>
        );
    }

    // Chỉ hiển thị tối đa 5 từ yếu nhất
    const displayedWords = struggleWords.slice(0, 5);

    return (
        <motion.div
            className="bg-gray-800 bg-opacity-80 rounded-xl p-4 mt-6 border-2 border-red-500 shadow-lg"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5 }}
        >
            <div className="flex items-center justify-between mb-4">
                <div className="flex items-center">
                    <span className="text-2xl mr-2">⚠️</span>
                    <h3 className="font-pixel text-lg text-red-400">Từ vựng cần rèn luyện</h3>
                </div>
                <span className="font-pixel text-[10px] text-gray-400 bg-red-950 px-2 py-0.5 border border-red-800 rounded">
                    Top {displayedWords.length}
                </span>
            </div>

            <div className="space-y-2 max-h-60 overflow-y-auto custom-scrollbar pr-1">
                {displayedWords.map((item, index) => (
                    <div
                        key={`${item.vocabularyId}-${index}`}
                        className="flex items-center justify-between p-2.5 bg-black bg-opacity-40 rounded-lg border border-red-900/50 hover:border-red-500 transition-colors"
                    >
                        <div className="flex flex-col min-w-0">
                            <span className="font-pixel text-sm text-white truncate">{item.word}</span>
                            <span className="text-[10px] text-gray-400 font-pixel mt-0.5 truncate max-w-[150px] sm:max-w-xs">{item.meaning}</span>
                        </div>

                        <div className="flex items-center bg-red-900 bg-opacity-30 px-2 py-0.5 rounded border border-red-800">
                            <span className="text-[9px] text-red-300 font-pixel mr-1">Sai:</span>
                            <span className="text-xs font-bold text-red-400">{item.wrongCount}</span>
                        </div>
                    </div>
                ))}
            </div>

            {onCreateReviewSession && (
                <div className="mt-4">
                    <button
                        onClick={onCreateReviewSession}
                        className="w-full py-2 bg-red-600 hover:bg-red-500 text-white font-pixel text-xs rounded border border-red-800 hover:shadow-[0_0_8px_rgba(239,68,68,0.5)] active:translate-y-0.5 transition-all"
                    >
                        RÈN LUYỆN TỪ YẾU ⚔️
                    </button>
                </div>
            )}
        </motion.div>
    );
};

export default StruggleWordsBox;
