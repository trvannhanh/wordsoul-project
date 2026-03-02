import { useEffect, useState, useCallback } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import type { UserAchievementDto } from '../../types/AchievementDto';
import { getMyAchievements } from '../../services/achievement';
import AchievementCard from './AchievementCard';
import AchievementModal from './AchievementModal';

type FilterTab = 'all' | 'completed' | 'locked';

const TABS: { key: FilterTab; label: string }[] = [
    { key: 'all', label: 'Tất cả' },
    { key: 'completed', label: '✓ Hoàn thành' },
    { key: 'locked', label: '🔒 Chưa đạt' },
];

const AchievementGrid: React.FC = () => {
    const [achievements, setAchievements] = useState<UserAchievementDto[]>([]);
    const [claimedIds, setClaimedIds] = useState<Set<number>>(new Set());
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [activeTab, setActiveTab] = useState<FilterTab>('all');
    const [selectedAchievement, setSelectedAchievement] = useState<UserAchievementDto | null>(null);

    useEffect(() => {
        const fetchAchievements = async () => {
            setLoading(true);
            setError(null);
            try {
                const data = await getMyAchievements();
                setAchievements(data);
            } catch {
                setError('Không thể tải danh sách thành tựu.');
            } finally {
                setLoading(false);
            }
        };
        fetchAchievements();
    }, []);

    const handleClaimed = useCallback((achievementId: number) => {
        setClaimedIds((prev) => new Set(prev).add(achievementId));
    }, []);

    const handleOpenModal = useCallback((achievement: UserAchievementDto) => {
        setSelectedAchievement(achievement);
    }, []);

    const handleCloseModal = useCallback(() => {
        setSelectedAchievement(null);
    }, []);

    const filtered = achievements.filter((a) => {
        if (activeTab === 'completed') return a.isCompleted;
        if (activeTab === 'locked') return !a.isCompleted;
        return true;
    });

    const completedCount = achievements.filter((a) => a.isCompleted).length;
    const totalCount = achievements.length;

    return (
        <>
            <motion.div
                className="pixel-border rounded-xl p-5"
                style={{ background: 'var(--background-color)' }}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5, delay: 0.15 }}
            >
                {/* Header */}
                <div className="flex items-center justify-between mb-4">
                    <div className="flex items-center gap-2">
                        <span className="text-2xl">🏆</span>
                        <h2 className="font-pixel text-sm text-yellow-400">Thành Tựu</h2>
                    </div>
                    {!loading && totalCount > 0 && (
                        <span className="font-pixel text-xs text-gray-400">
                            {completedCount}/{totalCount}
                        </span>
                    )}
                </div>

                {/* Overall progress bar */}
                {!loading && totalCount > 0 && (
                    <div className="mb-4">
                        <div className="w-full h-2 bg-gray-700 rounded-full overflow-hidden border border-gray-600">
                            <motion.div
                                className="h-full rounded-full bg-yellow-400"
                                initial={{ width: 0 }}
                                animate={{ width: `${Math.round((completedCount / totalCount) * 100)}%` }}
                                transition={{ duration: 0.8, ease: 'easeOut' }}
                            />
                        </div>
                    </div>
                )}

                {/* Filter tabs */}
                {!loading && totalCount > 0 && (
                    <div className="flex gap-1.5 mb-4 flex-wrap">
                        {TABS.map((tab) => (
                            <button
                                key={tab.key}
                                onClick={() => setActiveTab(tab.key)}
                                className={
                                    'font-pixel text-xs px-2.5 py-1 rounded-full border transition-colors ' +
                                    (activeTab === tab.key
                                        ? 'bg-yellow-400 text-black border-yellow-400'
                                        : 'bg-transparent text-gray-400 border-gray-600 hover:border-gray-400')
                                }
                            >
                                {tab.label}
                            </button>
                        ))}
                    </div>
                )}

                {/* Loading skeletons */}
                {loading && (
                    <div className="grid grid-cols-2 gap-3">
                        {[1, 2, 3, 4].map((i) => (
                            <div
                                key={i}
                                className="h-32 rounded-xl bg-gray-700/50 animate-pulse border border-gray-700"
                            />
                        ))}
                    </div>
                )}

                {/* Error state */}
                {!loading && error && (
                    <p className="font-pixel text-xs text-red-400 text-center py-6">{error}</p>
                )}

                {/* Empty state */}
                {!loading && !error && filtered.length === 0 && (
                    <div className="text-center py-6">
                        <div className="text-4xl mb-2">🌟</div>
                        <p className="font-pixel text-xs text-gray-400">
                            {activeTab === 'all' ? 'Chưa có thành tựu nào.' : 'Không có thành tựu nào.'}
                        </p>
                    </div>
                )}

                {/* Achievement grid */}
                {!loading && !error && filtered.length > 0 && (
                    <motion.div
                        className="grid grid-cols-2 gap-3"
                        layout
                    >
                        <AnimatePresence>
                            {filtered.map((achievement) => (
                                <AchievementCard
                                    key={achievement.achievementId}
                                    achievement={achievement}
                                    isClaimed={claimedIds.has(achievement.achievementId)}
                                    onOpenModal={handleOpenModal}
                                />
                            ))}
                        </AnimatePresence>
                    </motion.div>
                )}
            </motion.div>

            {/* Modal — rendered outside the grid div to avoid z-index clipping */}
            {selectedAchievement && (
                <AchievementModal
                    achievement={selectedAchievement}
                    isClaimed={claimedIds.has(selectedAchievement.achievementId)}
                    onClose={handleCloseModal}
                    onClaimed={handleClaimed}
                />
            )}
        </>
    );
};

export default AchievementGrid;
