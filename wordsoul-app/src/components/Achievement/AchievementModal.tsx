import { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import type { UserAchievementDto } from '../../types/AchievementDto';
import { claimAchievement } from '../../services/achievement';

interface AchievementModalProps {
    achievement: UserAchievementDto;
    isClaimed: boolean;
    onClose: () => void;
    onClaimed: (achievementId: number) => void;
}

const AchievementModal: React.FC<AchievementModalProps> = ({
    achievement,
    isClaimed,
    onClose,
    onClaimed,
}) => {
    const [claiming, setClaiming] = useState(false);
    const [showSuccess, setShowSuccess] = useState(false);
    const [errorMsg, setErrorMsg] = useState<string | null>(null);

    const percent = Math.min(Math.round(achievement.progressPercent), 100);
    const canClaim = achievement.isCompleted && !isClaimed;

    const handleClaim = async () => {
        if (!canClaim || claiming) return;
        setClaiming(true);
        setErrorMsg(null);
        try {
            await claimAchievement(achievement.achievementId);
            setShowSuccess(true);
            onClaimed(achievement.achievementId);
            setTimeout(() => {
                setShowSuccess(false);
                onClose();
            }, 1800);
        } catch {
            setErrorMsg('Không thể nhận phần thưởng. Vui lòng thử lại.');
        } finally {
            setClaiming(false);
        }
    };

    return (
        <AnimatePresence>
            {/* Backdrop */}
            <motion.div
                className="fixed inset-0 bg-black/70 z-40 flex items-center justify-center p-4"
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                onClick={onClose}
            >
                {/* Modal panel */}
                <motion.div
                    className="relative pixel-border rounded-2xl p-6 w-full max-w-sm z-50 overflow-hidden"
                    style={{ background: 'var(--background-color)' }}
                    initial={{ y: 40, opacity: 0, scale: 0.9 }}
                    animate={{ y: 0, opacity: 1, scale: 1 }}
                    exit={{ y: 40, opacity: 0, scale: 0.9 }}
                    transition={{ type: 'spring', stiffness: 280, damping: 24 }}
                    onClick={(e) => e.stopPropagation()}
                >
                    {/* Success burst overlay */}
                    <AnimatePresence>
                        {showSuccess && (
                            <motion.div
                                className="absolute inset-0 flex flex-col items-center justify-center bg-yellow-400/20 rounded-2xl z-20"
                                initial={{ opacity: 0 }}
                                animate={{ opacity: 1 }}
                                exit={{ opacity: 0 }}
                            >
                                {/* Particles */}
                                {[...Array(10)].map((_, i) => (
                                    <motion.div
                                        key={i}
                                        className="absolute w-2 h-2 rounded-full"
                                        style={{
                                            background: i % 2 === 0 ? '#facc15' : '#a855f7',
                                            top: '50%',
                                            left: '50%',
                                        }}
                                        initial={{ x: 0, y: 0, opacity: 1 }}
                                        animate={{
                                            x: Math.cos((i / 10) * Math.PI * 2) * 80,
                                            y: Math.sin((i / 10) * Math.PI * 2) * 80,
                                            opacity: 0,
                                        }}
                                        transition={{ duration: 0.7, ease: 'easeOut' }}
                                    />
                                ))}
                                <motion.div
                                    className="font-pixel text-yellow-300 text-xl drop-shadow-lg"
                                    initial={{ scale: 0.5, opacity: 0 }}
                                    animate={{ scale: 1.2, opacity: 1 }}
                                    exit={{ opacity: 0 }}
                                    transition={{ type: 'spring', stiffness: 300 }}
                                >
                                    🏆 Nhận thành công!
                                </motion.div>
                            </motion.div>
                        )}
                    </AnimatePresence>

                    {/* Close button */}
                    <button
                        onClick={onClose}
                        className="absolute top-3 right-4 text-gray-400 hover:text-white font-pixel text-lg leading-none"
                    >
                        ✕
                    </button>

                    {/* Lock / trophy icon */}
                    <div className="text-center mb-4">
                        <span className="text-5xl">
                            {achievement.isCompleted ? '🏆' : '🔒'}
                        </span>
                    </div>

                    {/* Title */}
                    <h2
                        className={
                            'font-pixel text-sm text-center mb-2 ' +
                            (achievement.isCompleted ? 'text-yellow-300' : 'text-gray-400')
                        }
                    >
                        {achievement.name}
                    </h2>

                    {/* Description */}
                    {achievement.description && (
                        <p className="text-gray-400 text-xs text-center mb-4 leading-relaxed">
                            {achievement.description}
                        </p>
                    )}

                    {/* Progress */}
                    <div className="mb-4">
                        <div className="flex justify-between mb-1">
                            <span className="text-xs text-gray-500 font-pixel">Tiến trình</span>
                            <span className="text-xs font-pixel text-color">
                                {achievement.progressValue} / {achievement.targetValue}
                            </span>
                        </div>
                        <div className="w-full h-3 bg-gray-700 rounded-full overflow-hidden border border-gray-600">
                            <motion.div
                                className={
                                    'h-full rounded-full ' +
                                    (isClaimed
                                        ? 'bg-gray-500'
                                        : achievement.isCompleted
                                            ? 'bg-yellow-400'
                                            : 'bg-purple-500')
                                }
                                initial={{ width: 0 }}
                                animate={{ width: `${percent}%` }}
                                transition={{ duration: 0.8, ease: 'easeOut' }}
                            />
                        </div>
                        {!achievement.isCompleted && (
                            <p className="text-xs text-gray-500 font-pixel mt-1 text-right">
                                Còn {achievement.remaining} nữa
                            </p>
                        )}
                    </div>

                    {/* Status badges */}
                    {isClaimed && (
                        <div className="text-center mb-3">
                            <span className="inline-block bg-gray-700 text-gray-300 font-pixel text-xs px-3 py-1 rounded-full">
                                ★ Đã nhận phần thưởng
                            </span>
                        </div>
                    )}

                    {/* Error */}
                    {errorMsg && (
                        <p className="text-red-400 text-xs font-pixel text-center mb-2">{errorMsg}</p>
                    )}

                    {/* Claim button */}
                    <motion.button
                        onClick={handleClaim}
                        disabled={!canClaim || claiming}
                        className={
                            'w-full font-pixel text-xs py-2.5 rounded pixel-border transition-opacity ' +
                            (canClaim
                                ? 'bg-yellow-400 text-black hover:bg-yellow-300 cursor-pointer'
                                : 'bg-gray-700 text-gray-500 opacity-50 cursor-not-allowed')
                        }
                        whileHover={canClaim ? { scale: 1.04 } : {}}
                        whileTap={canClaim ? { scale: 0.96 } : {}}
                    >
                        {claiming
                            ? 'Đang xử lý...'
                            : isClaimed
                                ? 'Đã nhận'
                                : achievement.isCompleted
                                    ? 'Nhận phần thưởng 🎁'
                                    : `Chưa hoàn thành`}
                    </motion.button>
                </motion.div>
            </motion.div>
        </AnimatePresence>
    );
};

export default AchievementModal;
