import { motion } from 'framer-motion';
import type { UserAchievementDto } from '../../types/AchievementDto';

interface AchievementCardProps {
    achievement: UserAchievementDto;
    isClaimed: boolean;
    onOpenModal: (achievement: UserAchievementDto) => void;
}

const AchievementCard: React.FC<AchievementCardProps> = ({
    achievement,
    isClaimed,
    onOpenModal,
}) => {
    const isUnlocked = achievement.isCompleted;
    const percent = Math.min(Math.round(achievement.progressPercent), 100);

    const barColor = isClaimed
        ? 'bg-gray-500'
        : isUnlocked
            ? 'bg-yellow-400'
            : 'bg-purple-500';

    return (
        <motion.div
            onClick={() => onOpenModal(achievement)}
            className={
                'relative rounded-xl p-4 pixel-border overflow-hidden cursor-pointer transition-colors ' +
                (isUnlocked && !isClaimed
                    ? 'border-yellow-400/60 shadow-[0_0_12px_rgba(250,204,21,0.25)]'
                    : 'border-gray-700')
            }
            style={{ background: 'var(--background-color)' }}
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            whileHover={{ scale: 1.03 }}
            whileTap={{ scale: 0.97 }}
            transition={{ duration: 0.25 }}
            layout
        >
            {/* Locked overlay */}
            {!isUnlocked && (
                <div className="absolute inset-0 bg-black/40 rounded-xl flex items-center justify-center z-10 pointer-events-none">
                    <span className="text-3xl opacity-60">🔒</span>
                </div>
            )}

            {/* Header row */}
            <div className="flex items-start justify-between gap-2 mb-2">
                <h3
                    className={
                        'font-pixel text-xs leading-relaxed ' +
                        (isUnlocked ? 'text-yellow-300' : 'text-gray-400')
                    }
                >
                    {achievement.name}
                </h3>

                {isClaimed ? (
                    <span className="flex-shrink-0 text-xs font-pixel bg-gray-700 text-gray-400 px-2 py-0.5 rounded-full">
                        ★ Đạt
                    </span>
                ) : isUnlocked ? (
                    <span className="flex-shrink-0 text-xs font-pixel bg-yellow-400 text-black px-2 py-0.5 rounded-full animate-pulse">
                        ✓ Xong!
                    </span>
                ) : null}
            </div>

            {/* Description (truncated) */}
            {achievement.description && (
                <p className="text-gray-400 text-xs mb-3 line-clamp-2">{achievement.description}</p>
            )}

            {/* Progress bar */}
            <div>
                <div className="flex justify-between mb-1">
                    <span className="text-xs text-gray-500 font-pixel">Tiến trình</span>
                    <span className="text-xs font-pixel text-color">
                        {achievement.progressValue}/{achievement.targetValue}
                    </span>
                </div>
                <div className="w-full h-2.5 bg-gray-700 rounded-full overflow-hidden border border-gray-600">
                    <motion.div
                        className={`h-full rounded-full ${barColor}`}
                        initial={{ width: 0 }}
                        animate={{ width: `${percent}%` }}
                        transition={{ duration: 0.7, ease: 'easeOut' }}
                    />
                </div>
            </div>
        </motion.div>
    );
};

export default AchievementCard;
