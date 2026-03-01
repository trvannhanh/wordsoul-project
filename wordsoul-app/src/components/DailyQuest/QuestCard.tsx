import { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import type { UserDailyQuestDto, ClaimQuestRewardResponseDto } from '../../types/DailyQuestDto';
import { claimQuestReward } from '../../services/dailyQuest';

interface QuestCardProps {
    quest: UserDailyQuestDto;
    onClaimed: (questId: number, result: ClaimQuestRewardResponseDto) => void;
}

const QUEST_TYPE_ICONS: Record<string, string> = {
    LearnWords: '📚',
    ReviewWords: '🔄',
    CompleteSession: '🎯',
    AnswerCorrect: '✅',
    default: '⚔️',
};

const REWARD_TYPE_LABELS: Record<string, string> = {
    XP: 'XP',
    AP: 'AP',
    default: 'Quà',
};

const QuestCard: React.FC<QuestCardProps> = ({ quest, onClaimed }) => {
    const [claiming, setClaiming] = useState(false);
    const [showReward, setShowReward] = useState(false);
    const [rewardMsg, setRewardMsg] = useState('');

    const progressPercent = Math.min(
        Math.round((quest.progress / quest.targetValue) * 100),
        100
    );

    const icon = QUEST_TYPE_ICONS[quest.questType] ?? QUEST_TYPE_ICONS.default;
    const rewardLabel = REWARD_TYPE_LABELS[quest.rewardType] ?? REWARD_TYPE_LABELS.default;

    const handleClaim = async () => {
        if (claiming || quest.isClaimed || !quest.isCompleted) return;
        setClaiming(true);
        try {
            const result = await claimQuestReward(quest.id);
            setRewardMsg(`+${result.rewardValue} ${rewardLabel}!`);
            setShowReward(true);
            onClaimed(quest.id, result);
            setTimeout(() => setShowReward(false), 2200);
        } catch {
            // silently ignore; parent can show errors if needed
        } finally {
            setClaiming(false);
        }
    };

    const barColor =
        quest.isClaimed
            ? 'bg-gray-500'
            : quest.isCompleted
                ? 'bg-yellow-400'
                : 'bg-purple-500';

    return (
        <motion.div
            className="relative background-color pixel-border rounded-xl p-4 overflow-hidden"
            initial={{ opacity: 0, y: 12 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.35 }}
            layout
        >
            {/* Reward animation overlay */}
            <AnimatePresence>
                {showReward && (
                    <motion.div
                        className="absolute inset-0 flex flex-col items-center justify-center z-20 pointer-events-none"
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                    >
                        {/* Faded glow bg */}
                        <motion.div
                            className="absolute inset-0 bg-yellow-400/20 rounded-xl"
                            initial={{ scale: 0.8 }}
                            animate={{ scale: 1.1 }}
                            exit={{ scale: 0.8 }}
                            transition={{ duration: 0.4 }}
                        />
                        {/* Flying reward text */}
                        <motion.div
                            className="relative font-pixel text-yellow-300 text-2xl drop-shadow-lg"
                            initial={{ y: 20, opacity: 0, scale: 0.6 }}
                            animate={{ y: -10, opacity: 1, scale: 1.2 }}
                            exit={{ y: -40, opacity: 0 }}
                            transition={{ duration: 0.5, type: 'spring', stiffness: 200 }}
                        >
                            {rewardMsg}
                        </motion.div>
                        {/* Particles */}
                        {[...Array(8)].map((_, i) => (
                            <motion.div
                                key={i}
                                className="absolute w-2 h-2 rounded-full bg-yellow-400"
                                initial={{ x: 0, y: 0, opacity: 1, scale: 1 }}
                                animate={{
                                    x: Math.cos((i / 8) * Math.PI * 2) * 60,
                                    y: Math.sin((i / 8) * Math.PI * 2) * 60,
                                    opacity: 0,
                                    scale: 0.3,
                                }}
                                transition={{ duration: 0.7, delay: 0.1 }}
                            />
                        ))}
                    </motion.div>
                )}
            </AnimatePresence>

            {/* Header */}
            <div className="flex items-start justify-between mb-3 gap-2">
                <div className="flex items-center gap-2 flex-1 min-w-0">
                    <span className="text-xl flex-shrink-0">{icon}</span>
                    <div className="min-w-0">
                        <h3 className="font-pixel text-xs text-color leading-relaxed truncate">
                            {quest.title}
                        </h3>
                        {quest.description && (
                            <p className="text-gray-400 text-xs mt-0.5 line-clamp-1">{quest.description}</p>
                        )}
                    </div>
                </div>

                {/* Status badge */}
                {quest.isClaimed ? (
                    <span className="flex-shrink-0 bg-gray-600 text-gray-300 text-xs font-pixel px-2 py-1 rounded-full whitespace-nowrap">
                        ★ Đã nhận
                    </span>
                ) : quest.isCompleted ? (
                    <span className="flex-shrink-0 bg-yellow-400 text-black text-xs font-pixel px-2 py-1 rounded-full whitespace-nowrap animate-pulse">
                        ✓ Xong!
                    </span>
                ) : null}
            </div>

            {/* Progress bar */}
            <div className="mb-3">
                <div className="flex justify-between items-center mb-1">
                    <span className="text-xs text-gray-400 font-pixel">Tiến trình</span>
                    <span className="text-xs font-pixel text-color">
                        {quest.progress}/{quest.targetValue}
                    </span>
                </div>
                <div className="w-full h-3 bg-gray-700 rounded-full overflow-hidden border border-gray-600">
                    <motion.div
                        className={`h-full rounded-full ${barColor}`}
                        initial={{ width: 0 }}
                        animate={{ width: `${progressPercent}%` }}
                        transition={{ duration: 0.6, ease: 'easeOut', delay: 0.1 }}
                    />
                </div>
            </div>

            {/* Footer: reward badge + claim button */}
            <div className="flex items-center justify-between">
                <span className="bg-purple-700 text-purple-100 text-xs font-pixel px-2 py-1 rounded-full">
                    +{quest.rewardValue} {rewardLabel}
                </span>

                <motion.button
                    onClick={handleClaim}
                    disabled={!quest.isCompleted || quest.isClaimed || claiming}
                    className={
                        'relative font-pixel text-xs px-3 py-1.5 rounded pixel-border transition-opacity ' +
                        (quest.isCompleted && !quest.isClaimed
                            ? 'bg-yellow-400 text-black hover:bg-yellow-300 cursor-pointer'
                            : 'bg-gray-700 text-gray-500 opacity-50 cursor-not-allowed')
                    }
                    whileHover={quest.isCompleted && !quest.isClaimed ? { scale: 1.07 } : {}}
                    whileTap={quest.isCompleted && !quest.isClaimed ? { scale: 0.95 } : {}}
                >
                    {claiming ? '...' : quest.isClaimed ? 'Đã nhận' : 'Nhận thưởng'}
                </motion.button>
            </div>
        </motion.div>
    );
};

export default QuestCard;
