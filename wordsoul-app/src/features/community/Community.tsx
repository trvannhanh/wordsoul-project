import React, { useEffect, useState } from 'react';

import type { LeaderBoardDto } from '../../types/Dto';
import { getLeaderBoard } from '../../services/user';

const Community: React.FC = () => {
    const [xpLeaderboard, setXpLeaderboard] = useState<LeaderBoardDto[]>([]);
    const [apLeaderboard, setApLeaderboard] = useState<LeaderBoardDto[]>([]);
    const [activeTab, setActiveTab] = useState<'xp' | 'ap'>('xp');
    const [xpPage, setXpPage] = useState<number>(1);
    const [apPage, setApPage] = useState<number>(1);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const pageSize = 10;

    const loadLeaderboard = async () => {
        setLoading(true);
        try {
            if (activeTab === 'xp') {
                const xpData = await getLeaderBoard(true, undefined, xpPage, pageSize);
                setXpLeaderboard(xpData);
            } else {
                const apData = await getLeaderBoard(undefined, true, apPage, pageSize);
                setApLeaderboard(apData);
            }
            // eslint-disable-next-line @typescript-eslint/no-unused-vars
        } catch (err) {
            setError('Error loading leaderboard');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadLeaderboard();
    }, [activeTab, xpPage, apPage]);

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

    if (loading) {
        return (
            <div className="pixel-background font-pixel text-white mt-12 min-h-screen">
                <div className="container mx-auto p-4 w-7/12">
                    <h1 className="text-3xl mb-6 text-center">Trainer Leaderboard</h1>
                    <div className="flex justify-center mb-6">
                        <button
                            className={` px-4 py-2 mr-2 rounded-md ${activeTab === 'xp' ? 'bg-blue-500 text-white' : 'bg-gray-400 text-black'
                                } focus:outline-none`}
                        >
                            XP Rankings
                        </button>
                        <button
                            className={`px-4 py-2 rounded-md ${activeTab === 'ap' ? 'bg-blue-500 text-white' : 'bg-gray-400 text-black'
                                } focus:outline-none`}
                        >
                            AP Rankings
                        </button>
                    </div>

                    <div className="bg-gray-800 bg-opacity-80 p-4 rounded-md shadow-md">
                        <div className="grid grid-cols-3 gap-4 mb-4 text-center font-bold">
                            <div>Rank</div>
                            <div>Trainer</div>
                            <div>{activeTab === 'xp' ? 'XP' : 'AP'}</div>
                        </div>
                        <div className="text-center py-8">Loading...</div>
                    </div>

                    <div className="flex justify-between mt-6">
                        <button
                            className="px-4 py-2 bg-gray-400 text-black rounded-md disabled:opacity-50"
                            disabled={(activeTab === 'xp' ? xpPage : apPage) === 1}
                        >
                            Previous
                        </button>
                        <span>Page {activeTab === 'xp' ? xpPage : apPage}</span>
                        <button
                            className="px-4 py-2 bg-gray-400 text-black rounded-md"
                        >
                            Next
                        </button>
                    </div>
                </div>
            </div>
        )

    }

    if (error) {
        return <div className="text-center py-8 text-red-500 font-pixel">{error}</div>;
    }

    return (
        <div className="pixel-background font-pixel text-white min-h-screen">
            <div className="container mx-auto p-4 w-7/12">
                <h1 className="text-3xl mb-6 text-center">Trainer Leaderboard</h1>
                <div className="flex justify-center mb-6">
                    <button
                        className={`px-4 py-2 mr-2 rounded-md ${activeTab === 'xp' ? 'bg-blue-500 text-white' : 'bg-gray-400 text-black'
                            } focus:outline-none`}
                        onClick={() => handleTabSwitch('xp')}
                    >
                        XP Rankings
                    </button>
                    <button
                        className={`px-4 py-2 rounded-md ${activeTab === 'ap' ? 'bg-blue-500 text-white' : 'bg-gray-400 text-black'
                            } focus:outline-none`}
                        onClick={() => handleTabSwitch('ap')}
                    >
                        AP Rankings
                    </button>
                </div>

                <div className="bg-gray-800 bg-opacity-80 p-4 rounded-md shadow-md">
                    <div className="grid grid-cols-3 gap-4 mb-4 text-center font-bold">
                        <div>Rank</div>
                        <div>Trainer</div>
                        <div>{activeTab === 'xp' ? 'XP' : 'AP'}</div>
                    </div>
                    {(activeTab === 'xp' ? xpLeaderboard : apLeaderboard).map((entry, index) => (
                        <div
                            key={entry.userId}
                            className="grid grid-cols-3 gap-4 py-2 border-b border-gray-600 text-center"
                        >
                            <div>{((activeTab === 'xp' ? xpPage - 1 : apPage - 1) * pageSize + index + 1)}</div>
                            <div>{entry.userName}</div>
                            <div>{activeTab === 'xp' ? entry.totalXP : entry.totalAP}</div>
                        </div>
                    ))}
                </div>

                <div className="flex justify-between mt-6">
                    <button
                        className="px-4 py-2 bg-gray-400 text-black rounded-md disabled:opacity-50"
                        onClick={() => handlePageChange(false)}
                        disabled={(activeTab === 'xp' ? xpPage : apPage) === 1}
                    >
                        Previous
                    </button>
                    <span>Page {activeTab === 'xp' ? xpPage : apPage}</span>
                    <button
                        className="px-4 py-2 bg-gray-400 text-black rounded-md"
                        onClick={() => handlePageChange(true)}
                    >
                        Next
                    </button>
                </div>
            </div>
        </div>
    );
};

export default Community;