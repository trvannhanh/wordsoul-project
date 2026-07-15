import React, { useCallback, useEffect, useState } from 'react';
import { getLeaderBoard } from '../../services/user';
import type { LeaderBoardDto } from '../../types/UserDto';

const Community: React.FC = () => {
    const [xpLeaderboard, setXpLeaderboard] = useState<LeaderBoardDto[]>([]);
    const [apLeaderboard, setApLeaderboard] = useState<LeaderBoardDto[]>([]);
    const [activeTab, setActiveTab] = useState<'xp' | 'ap'>('xp');
    const [xpPage, setXpPage] = useState<number>(1);
    const [apPage, setApPage] = useState<number>(1);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const pageSize = 10;

    const loadLeaderboard = useCallback(async () => {
        setLoading(true);
        try {
            if (activeTab === 'xp') {
                const xpData = await getLeaderBoard(true, undefined, xpPage, pageSize);
                setXpLeaderboard(xpData);
            } else {
                const apData = await getLeaderBoard(undefined, true, apPage, pageSize);
                setApLeaderboard(apData);
            }
        } catch {
            setError('Error loading leaderboard');
        } finally {
            setLoading(false);
        }
    }, [activeTab, apPage, pageSize, xpPage]);

    useEffect(() => {
        loadLeaderboard();
    }, [loadLeaderboard]);

    const handleTabSwitch = (tab: 'xp' | 'ap') => {
        setActiveTab(tab);
        setError(null);
    };

    const handlePageChange = (increment: boolean) => {
        if (activeTab === 'xp') {
            setXpPage((prev) => (increment ? prev + 1 : Math.max(1, prev - 1)));
        } else {
            setApPage((prev) => (increment ? prev + 1 : Math.max(1, prev - 1)));
        }
    };

    const renderRankBadge = (rank: number) => {
        if (rank === 1) {
            return (
                <span className="inline-block px-3 py-1 text-xs bg-yellow-500/20 border-2 border-yellow-400 text-yellow-300 rounded font-pixel font-bold shadow-[0_0_8px_rgba(234,179,8,0.3)]">
                    1st
                </span>
            );
        }
        if (rank === 2) {
            return (
                <span className="inline-block px-3 py-1 text-xs bg-slate-300/20 border-2 border-slate-300 text-slate-200 rounded font-pixel font-bold shadow-[0_0_8px_rgba(203,213,225,0.2)]">
                    2nd
                </span>
            );
        }
        if (rank === 3) {
            return (
                <span className="inline-block px-3 py-1 text-xs bg-amber-700/20 border-2 border-amber-600 text-amber-400 rounded font-pixel font-bold shadow-[0_0_8px_rgba(217,119,6,0.2)]">
                    3rd
                </span>
            );
        }
        return (
            <span className="inline-block px-3 py-1 text-xs bg-slate-800 border border-slate-700 text-slate-400 rounded font-pixel">
                {rank}
            </span>
        );
    };

    return (
        <div className="review-box-background font-pixel text-white h-[calc(100vh-56px)] mt-[56px] w-full flex justify-center items-center py-4 px-4 sm:px-6 lg:px-8 overflow-hidden">
            <div className="max-w-4xl w-full h-full flex flex-col justify-between overflow-hidden">
                {/* Tiêu đề bảng xếp hạng */}
                <h1 className="text-3xl mb-4 text-center font-bold tracking-wider text-transparent bg-clip-text bg-gradient-to-r from-yellow-300 via-amber-400 to-yellow-500 drop-shadow-[0_2px_8px_rgba(0,0,0,0.8)] flex-shrink-0">
                    Trainer Leaderboard
                </h1>

                {/* Chuyển đổi tab dạng tay cầm tay chơi game retro */}
                <div className="flex justify-center mb-4 flex-shrink-0">
                    <div className="bg-slate-950/60 p-1.5 rounded-xl border-2 border-slate-800 flex gap-2 backdrop-blur-sm">
                        <button
                            className={`px-6 py-2 rounded-lg text-sm transition-all duration-200 focus:outline-none custom-cursor ${
                                activeTab === 'xp'
                                    ? 'bg-gradient-to-r from-yellow-500 to-amber-500 text-slate-950 font-bold border-b-4 border-amber-700 shadow-[0_0_15px_rgba(245,158,11,0.4)]'
                                    : 'text-slate-400 hover:text-white hover:bg-slate-900/60'
                            }`}
                            onClick={() => handleTabSwitch('xp')}
                        >
                            XP Rankings
                        </button>
                        <button
                            className={`px-6 py-2 rounded-lg text-sm transition-all duration-200 focus:outline-none custom-cursor ${
                                activeTab === 'ap'
                                    ? 'bg-gradient-to-r from-yellow-500 to-amber-500 text-slate-950 font-bold border-b-4 border-amber-700 shadow-[0_0_15px_rgba(245,158,11,0.4)]'
                                    : 'text-slate-400 hover:text-white hover:bg-slate-900/60'
                            }`}
                            onClick={() => handleTabSwitch('ap')}
                        >
                            AP Rankings
                        </button>
                    </div>
                </div>

                {/* Khung xếp hạng kính mờ kết hợp viền pixel */}
                <div className="bg-slate-900/80 backdrop-blur-md p-4 rounded-2xl border-4 border-slate-800 shadow-2xl flex-1 flex flex-col min-h-0 overflow-hidden">
                    <div className="grid grid-cols-3 gap-4 mb-3 pb-2 border-b-2 border-slate-800 text-center font-bold text-xs tracking-wider text-amber-400 flex-shrink-0">
                        <div>Thứ hạng</div>
                        <div>Trainer</div>
                        <div>{activeTab === 'xp' ? 'Kinh nghiệm (XP)' : 'Điểm (AP)'}</div>
                    </div>

                    {loading ? (
                        <div className="flex flex-col items-center justify-center flex-1 space-y-4 py-8">
                            <div className="animate-spin rounded-full h-10 w-10 border-t-2 border-b-2 border-yellow-500"></div>
                            <div className="text-yellow-400/80 text-[10px] tracking-widest animate-pulse">ĐANG TẢI DỮ LIỆU...</div>
                        </div>
                    ) : error ? (
                        <div className="text-center py-8 text-red-500 bg-red-950/20 rounded-xl border border-red-900/50 my-auto flex-shrink-0">
                            {error}
                        </div>
                    ) : (
                        <div className="space-y-1.5 overflow-y-auto flex-1 pr-1">
                            {(activeTab === 'xp' ? xpLeaderboard : apLeaderboard).map((entry, index) => {
                                const rank = ((activeTab === 'xp' ? xpPage - 1 : apPage - 1) * pageSize + index + 1);
                                const isTop3 = rank <= 3;
                                return (
                                    <div
                                        key={entry.userId}
                                        className={`grid grid-cols-3 gap-4 py-2 items-center text-center rounded-xl border transition-all duration-200 hover:scale-[1.01] ${
                                            isTop3
                                                ? rank === 1
                                                    ? 'bg-yellow-500/5 border-yellow-500/20 hover:bg-yellow-500/10'
                                                    : rank === 2
                                                    ? 'bg-slate-300/5 border-slate-300/15 hover:bg-slate-300/10'
                                                    : 'bg-amber-700/5 border-amber-700/15 hover:bg-amber-700/10'
                                                : 'bg-slate-950/30 border-transparent hover:bg-slate-950/50 hover:border-slate-800'
                                        }`}
                                    >
                                        <div className="flex justify-center">{renderRankBadge(rank)}</div>
                                        <div className={`font-semibold tracking-wide text-xs ${isTop3 ? 'text-white' : 'text-slate-300'}`}>
                                            {entry.userName}
                                        </div>
                                        <div className={`font-mono font-bold text-xs ${isTop3 ? 'text-yellow-400' : 'text-slate-400'}`}>
                                            {activeTab === 'xp' ? entry.totalXP : entry.totalAP}
                                        </div>
                                    </div>
                                );
                            })}
                            
                            {/* Empty State */}
                            {((activeTab === 'xp' ? xpLeaderboard : apLeaderboard).length === 0) && (
                                <div className="text-center py-8 text-slate-500 my-auto">
                                    Không có dữ liệu xếp hạng.
                                </div>
                            )}
                        </div>
                    )}
                </div>

                {/* Nút phân trang cổ điển phong cách máy game retro */}
                <div className="flex justify-between items-center mt-4 px-2 flex-shrink-0">
                    <button
                        className="px-5 py-2 bg-slate-800 border-2 border-slate-700 text-white rounded-lg transition-all duration-150 active:scale-95 disabled:opacity-30 disabled:pointer-events-none custom-cursor hover:bg-slate-750 text-xs"
                        onClick={() => handlePageChange(false)}
                        disabled={(activeTab === 'xp' ? xpPage : apPage) === 1}
                    >
                        ◀ Previous
                    </button>
                    <span className="text-slate-400 text-xs tracking-widest font-bold">
                        Trang {activeTab === 'xp' ? xpPage : apPage}
                    </span>
                    <button
                        className="px-5 py-2 bg-slate-800 border-2 border-slate-700 text-white rounded-lg transition-all duration-150 active:scale-95 disabled:opacity-30 disabled:pointer-events-none custom-cursor hover:bg-slate-750 text-xs"
                        onClick={() => handlePageChange(true)}
                        disabled={(activeTab === 'xp' ? xpLeaderboard : apLeaderboard).length < pageSize}
                    >
                        Next ▶
                    </button>
                </div>
            </div>
        </div>
    );
};

export default Community;