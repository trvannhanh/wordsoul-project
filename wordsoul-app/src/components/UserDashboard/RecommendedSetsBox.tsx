import { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { registerVocabularySet } from '../../services/user';
import type { RecommendedSetDto } from '../../types/UserDto';

const themeEmoji: Record<string, string> = {
    DailyLife: '🏠', Nature: '🌿', Weather: '❄️', Food: '🍜',
    Technology: '⚡', Travel: '✈️', Health: '💖', Sports: '🥊',
    Business: '⚙️', Science: '🔮', Art: '🐉', Mystery: '👻',
    Dark: '🌑', Custom: '🔥', Challenge: '🪨', Poison: '☠️',
};

const difficultyColor: Record<string, string> = {
    Beginner: 'text-green-400 border-green-600',
    Elementary: 'text-yellow-300 border-yellow-600',
    Intermediate: 'text-orange-400 border-orange-600',
    Advanced: 'text-red-400 border-red-600',
    Expert: 'text-purple-400 border-purple-600',
};

interface RecommendedSetsBoxProps {
    recommendedSets: RecommendedSetDto[];
    onAdded?: () => void;
}

const RecommendedSetsBox: React.FC<RecommendedSetsBoxProps> = ({ recommendedSets, onAdded }) => {
    const [addingId, setAddingId] = useState<number | null>(null);
    const [addedIds, setAddedIds] = useState<Set<number>>(new Set());
    const [error, setError] = useState<string | null>(null);

    if (!recommendedSets || recommendedSets.length === 0) return null;

    const handleAdd = async (setId: number) => {
        if (addedIds.has(setId) || addingId === setId) return;
        setAddingId(setId);
        setError(null);
        try {
            await registerVocabularySet(setId);
            setAddedIds(prev => new Set([...prev, setId]));
            onAdded?.();
        } catch {
            setError('Không thể thêm bộ từ, hãy thử lại!');
        } finally {
            setAddingId(null);
        }
    };

    return (
        <div className="pokemon-background pixel-border rounded-xl p-6 mt-6">
            <h3 className="font-pokemon text-xl mb-1 text-yellow-300">✨ Gợi ý cho bạn</h3>
            <p className="font-pixel text-gray-400 text-xs mb-4">
                Dựa trên sở thích chủ đề của bạn
            </p>

            {error && (
                <p className="font-pixel text-red-400 text-xs mb-3">{error}</p>
            )}

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <AnimatePresence>
                    {recommendedSets.map((set, i) => {
                        const emoji = themeEmoji[set.theme] ?? '📚';
                        const isAdded = addedIds.has(set.id);
                        const isAdding = addingId === set.id;
                        const diffClass = difficultyColor[set.difficultyLevel] ?? 'text-gray-400 border-gray-600';

                        return (
                            <motion.div
                                key={set.id}
                                className="bg-gray-800 border border-gray-700 rounded-lg p-3 flex flex-col gap-2 hover:border-yellow-500 transition-colors"
                                initial={{ opacity: 0, y: 12 }}
                                animate={{ opacity: 1, y: 0 }}
                                transition={{ delay: i * 0.07 }}
                            >
                                {/* Thumbnail */}
                                <div className="h-20 rounded-md overflow-hidden bg-gray-900 flex items-center justify-center">
                                    {set.imageUrl ? (
                                        <img
                                            src={set.imageUrl}
                                            alt={set.title}
                                            className="w-full h-full object-cover opacity-80"
                                        />
                                    ) : (
                                        <span className="text-4xl">{emoji}</span>
                                    )}
                                </div>

                                {/* Meta */}
                                <div className="flex items-start justify-between gap-2">
                                    <div className="flex-1 min-w-0">
                                        <p className="font-pokemon text-white text-xs truncate">{set.title}</p>
                                        <p className="font-pixel text-gray-400 text-xs mt-0.5">
                                            {emoji} {set.theme}
                                        </p>
                                    </div>
                                    <span className={`font-pixel text-xs border px-1.5 py-0.5 rounded shrink-0 ${diffClass}`}>
                                        {set.difficultyLevel}
                                    </span>
                                </div>

                                {set.description && (
                                    <p className="font-pixel text-gray-500 text-xs line-clamp-2 leading-relaxed">
                                        {set.description}
                                    </p>
                                )}

                                {/* Add button */}
                                <motion.button
                                    onClick={() => handleAdd(set.id)}
                                    disabled={isAdded || isAdding}
                                    className={`mt-auto font-pokemon text-xs rounded px-3 py-1.5 transition-all border ${isAdded
                                            ? 'bg-green-900 border-green-600 text-green-300 cursor-default'
                                            : 'bg-yellow-600 border-yellow-500 text-black hover:bg-yellow-400 disabled:opacity-50'
                                        }`}
                                    whileTap={isAdded ? {} : { scale: 0.96 }}
                                >
                                    {isAdding ? '...' : isAdded ? '✅ Đã thêm' : '+ Thêm vào thư viện'}
                                </motion.button>
                            </motion.div>
                        );
                    })}
                </AnimatePresence>
            </div>
        </div>
    );
};

export default RecommendedSetsBox;
