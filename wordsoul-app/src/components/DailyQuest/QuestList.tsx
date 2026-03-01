import { useEffect, useState, useCallback } from 'react';
import { motion } from 'framer-motion';
import type { UserDailyQuestDto, ClaimQuestRewardResponseDto } from '../../types/DailyQuestDto';
import { getTodayQuests } from '../../services/dailyQuest';
import QuestCard from './QuestCard';

// Returns seconds until midnight (00:00:00 next day)
const getSecondsUntilMidnight = (): number => {
    const now = new Date();
    const midnight = new Date(now);
    midnight.setHours(24, 0, 0, 0);
    return Math.floor((midnight.getTime() - now.getTime()) / 1000);
};

const formatCountdown = (totalSeconds: number): string => {
    const h = Math.floor(totalSeconds / 3600);
    const m = Math.floor((totalSeconds % 3600) / 60);
    const s = totalSeconds % 60;
    return [h, m, s].map((v) => String(v).padStart(2, '0')).join(':');
};

const QuestList: React.FC = () => {
    const [quests, setQuests] = useState<UserDailyQuestDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [countdown, setCountdown] = useState(getSecondsUntilMidnight);

    // Fetch quests on mount
    useEffect(() => {
        const fetchQuests = async () => {
            setLoading(true);
            setError(null);
            try {
                const data = await getTodayQuests();
                setQuests(data);
            } catch {
                setError('Không thể tải nhiệm vụ hằng ngày.');
            } finally {
                setLoading(false);
            }
        };
        fetchQuests();
    }, []);

    // Countdown timer — tick every second
    useEffect(() => {
        const id = setInterval(() => {
            setCountdown((prev) => (prev > 0 ? prev - 1 : 0));
        }, 1000);
        return () => clearInterval(id);
    }, []);

    const handleClaimed = useCallback(
        (questId: number, _result: ClaimQuestRewardResponseDto) => {
            setQuests((prev) =>
                prev.map((q) => (q.id === questId ? { ...q, isClaimed: true } : q))
            );
        },
        []
    );

    const completedCount = quests.filter((q) => q.isCompleted).length;
    const totalCount = quests.length;

    return (
        <motion.div
            className="pixel-border rounded-xl p-5 background-color"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5, delay: 0.1 }}
        >
            {/* Header */}
            <div className="flex items-center justify-between mb-4">
                <div className="flex items-center gap-2">
                    <span className="text-2xl">⚔️</span>
                    <h2 className="font-pixel text-sm text-yellow-400">Nhiệm Vụ Hằng Ngày</h2>
                </div>
                {!loading && totalCount > 0 && (
                    <span className="font-pixel text-xs text-gray-400">
                        {completedCount}/{totalCount} xong
                    </span>
                )}
            </div>

            {/* Daily Reset Countdown */}
            <div className="flex items-center justify-between bg-gray-800/60 rounded-lg px-3 py-2 mb-4 border border-gray-700">
                <span className="font-pixel text-xs text-gray-400">Reset sau</span>
                <span className="font-vt323 text-xl text-purple-300 tracking-widest">
                    {formatCountdown(countdown)}
                </span>
            </div>

            {/* Loading skeletons */}
            {loading && (
                <div className="space-y-3">
                    {[1, 2, 3].map((i) => (
                        <div
                            key={i}
                            className="h-28 rounded-xl bg-gray-700/50 animate-pulse border border-gray-700"
                        />
                    ))}
                </div>
            )}

            {/* Error state */}
            {!loading && error && (
                <p className="font-pixel text-xs text-red-400 text-center py-4">{error}</p>
            )}

            {/* Empty state */}
            {!loading && !error && quests.length === 0 && (
                <div className="text-center py-6">
                    <div className="text-4xl mb-2">🌙</div>
                    <p className="font-pixel text-xs text-gray-400">Không có nhiệm vụ hôm nay.</p>
                </div>
            )}

            {/* Quest cards */}
            {!loading && !error && quests.length > 0 && (
                <div className="space-y-3">
                    {quests.map((quest) => (
                        <QuestCard key={quest.id} quest={quest} onClaimed={handleClaimed} />
                    ))}
                </div>
            )}
        </motion.div>
    );
};

export default QuestList;
