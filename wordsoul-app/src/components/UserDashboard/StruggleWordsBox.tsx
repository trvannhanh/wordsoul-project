import { motion } from 'framer-motion';
import type { StruggleWordDto } from '../../types/UserDto';

interface StruggleWordsBoxProps {
    struggleWords: StruggleWordDto[];
}

const StruggleWordsBox: React.FC<StruggleWordsBoxProps> = ({ struggleWords }) => {
    if (!struggleWords || struggleWords.length === 0) {
        return null; // Không hiển thị nếu không có từ vựng nào
    }

    return (
        <motion.div
            className="bg-gray-800 bg-opacity-80 rounded-xl p-4 mt-6 border-2 border-red-500 shadow-lg"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5 }}
        >
            <div className="flex items-center mb-4">
                <span className="text-2xl mr-2">⚠️</span>
                <h3 className="font-pixel text-xl text-red-400">Từ vựng cần rèn luyện</h3>
            </div>

            <div className="space-y-3 max-h-60 overflow-y-auto custom-scrollbar pr-2">
                {struggleWords.map((item, index) => (
                    <div
                        key={`${item.vocabularyId}-${index}`}
                        className="flex items-center justify-between p-3 bg-black bg-opacity-40 rounded-lg border border-red-900 hover:border-red-500 transition-colors"
                    >
                        <div className="flex flex-col">
                            <span className="font-pixel text-lg text-white">{item.word}</span>
                            <span className="text-sm text-gray-400 font-pixel mt-1 opacity-80 truncate max-w-[150px] sm:max-w-xs">{item.meaning}</span>
                        </div>

                        <div className="flex items-center bg-red-900 bg-opacity-50 px-3 py-1 rounded-full">
                            <span className="text-xs text-red-200 font-pixel mr-1">Sai:</span>
                            <span className="text-sm font-bold text-red-400">{item.wrongCount}</span>
                        </div>
                    </div>
                ))}
            </div>
        </motion.div>
    );
};

export default StruggleWordsBox;
