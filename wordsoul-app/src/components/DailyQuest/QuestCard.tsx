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

    const handleClaim = async (e: React.MouseEvent) => {
        e.stopPropagation();
        if (claiming || quest.isClaimed || !quest.isCompleted) return;
        setClaiming(true);
        try {
            const result = await claimQuestReward(quest.id);
            setRewardMsg(`+${result.rewardValue} ${rewardLabel}!`);
            setShowReward(true);
            onClaimed(quest.id, result);
            setTimeout(() => setShowReward(false), 2200);
        } catch {
            // silently ignore
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
            className={`relative background-color border border-gray-700/80 hover:border-gray-500 rounded-lg p-2.5 flex items-center justify-between gap-3 overflow-hidden select-none transition-colors ${quest.isClaimed ? 'opacity-60' : 'opacity-100'
                }`}
            initial={{ opacity: 0, y: 8 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.25 }}
            layout
        >
            {/* Reward animation overlay */}
            <AnimatePresence>
                {showReward && (
                    <motion.div
                        className="absolute inset-0 flex items-center justify-center bg-yellow-400/20 z-20 pointer-events-none"
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                    >
                        <motion.span
                            className="font-pixel text-yellow-300 text-[11px] drop-shadow-md font-bold"
                            initial={{ y: 10, scale: 0.8 }}
                            animate={{ y: -5, scale: 1.1 }}
                            exit={{ y: -20, opacity: 0 }}
                            transition={{ duration: 0.6 }}
                        >
                            {rewardMsg}
                        </motion.span>
                    </motion.div>
                )}
            </AnimatePresence>

            {/* Left Section: Quest Icon, Title & Progress Text */}
            <div className="flex items-center gap-2 min-w-0 flex-1">
                <span className="text-lg flex-shrink-0">{icon}</span>
                <div className="min-w-0 flex-1">
                    <div className="flex justify-between items-baseline gap-2">
                        <h4 className="font-pixel text-[11px] text-white truncate" title={quest.title}>
                            {quest.title}
                        </h4>
                        <span className="font-pixel text-[9px] text-gray-400 flex-shrink-0">
                            {quest.progress}/{quest.targetValue}
                        </span>
                    </div>
                    {quest.description && (
                        <p className="text-gray-500 text-[9px] truncate mt-0.5">
                            {quest.description}
                        </p>
                    )}
                </div>
            </div>

            {/* Right Section: Reward bubble and Claim button */}
            <div className="flex items-center gap-2 flex-shrink-0">
                <span className="bg-purple-900/60 border border-purple-800 text-purple-200 text-[8px] font-pixel px-1.5 py-0.5 rounded-full">
                    +{quest.rewardValue}{rewardLabel}
                </span>

                <button
                    onClick={handleClaim}
                    disabled={!quest.isCompleted || quest.isClaimed || claiming}
                    className={`font-pixel text-[9px] px-2.5 py-1 rounded border active:translate-y-0.5 transition-all ${quest.isClaimed
                            ? 'bg-gray-800 border-gray-700 text-gray-500 cursor-default'
                            : quest.isCompleted
                                ? 'bg-yellow-500 border-yellow-400 text-black hover:bg-yellow-400 cursor-pointer shadow-[0_0_5px_rgba(234,179,8,0.3)] animate-pulse'
                                : 'bg-gray-800 border-gray-700 text-gray-400 cursor-default opacity-40'
                        }`}
                >
                    {claiming ? '...' : quest.isClaimed ? 'Đã nhận' : 'Nhận'}
                </button>
            </div>

            {/* Progress Bar running at the bottom */}
            <div className="absolute bottom-0 left-0 w-full h-[3px] bg-gray-900">
                <motion.div
                    className={`h-full ${barColor}`}
                    initial={{ width: 0 }}
                    animate={{ width: `${progressPercent}%` }}
                    transition={{ duration: 0.4, ease: 'easeOut' }}
                />
            </div>
        </motion.div>
    );
};

export default QuestCard;
