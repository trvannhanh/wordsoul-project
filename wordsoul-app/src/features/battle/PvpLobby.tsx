import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getMyPvpRating } from '../../services/pvp';
import type { PvpRatingDto } from '../../services/pvp';

const TIER_COLORS: Record<string, { text: string; glow: string; bg: string; border: string }> = {
    Bronze: { text: 'text-orange-400', glow: 'rgba(251,146,60,0.6)', bg: 'bg-orange-900/20', border: 'border-orange-500/40' },
    Silver: { text: 'text-gray-300', glow: 'rgba(156,163,175,0.6)', bg: 'bg-gray-700/30', border: 'border-gray-400/40' },
    Gold:   { text: 'text-yellow-400', glow: 'rgba(250,204,21,0.6)', bg: 'bg-yellow-900/20', border: 'border-yellow-500/40' },
    Platinum: { text: 'text-blue-300', glow: 'rgba(147,197,253,0.6)', bg: 'bg-blue-900/20', border: 'border-blue-400/40' },
    Diamond: { text: 'text-cyan-300', glow: 'rgba(103,232,249,0.6)', bg: 'bg-cyan-900/20', border: 'border-cyan-400/40' },
};

const TIER_ICONS: Record<string, string> = {
    Bronze: '🥉', Silver: '🥈', Gold: '🥇', Platinum: '💎', Diamond: '👑',
};

export default function PvpLobby() {
    const navigate = useNavigate();
    const [joinCode, setJoinCode] = useState('');
    const [ratingInfo, setRatingInfo] = useState<PvpRatingDto | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        getMyPvpRating()
            .then(data => setRatingInfo(data))
            .catch(err => console.error('Failed to load rating', err))
            .finally(() => setLoading(false));
    }, []);

    const tierStyle = ratingInfo ? (TIER_COLORS[ratingInfo.tier] ?? TIER_COLORS.Bronze) : null;
    const winRate = ratingInfo && (ratingInfo.pvpWins + ratingInfo.pvpLosses) > 0
        ? Math.round((ratingInfo.pvpWins / (ratingInfo.pvpWins + ratingInfo.pvpLosses)) * 100)
        : null;

    return (
        <div className="min-h-screen text-white flex flex-col items-center py-10 px-4 relative overflow-hidden"
            style={{ background: 'linear-gradient(160deg, rgb(2,6,23) 0%, rgba(88,28,135,0.1) 50%, rgb(2,6,23) 100%)' }}>

            {/* Background glow */}
            <div className="absolute top-0 left-1/2 -translate-x-1/2 w-[600px] h-[300px] rounded-full opacity-10 blur-3xl pointer-events-none"
                style={{ background: 'radial-gradient(ellipse, #7c3aed, transparent)' }} />

            {/* Back */}
            <button onClick={() => navigate('/home')}
                className="self-start text-gray-400 hover:text-white font-pixel text-xs mb-8 flex items-center gap-1 transition-colors z-10">
                ← Back to Home
            </button>

            {/* Header */}
            <div className="flex flex-col items-center mb-8 z-10">
                <h1 className="font-press text-4xl text-purple-400 mb-2"
                    style={{ textShadow: '0 0 30px rgba(168,85,247,0.7)' }}>
                    PvP ARENA
                </h1>
                <p className="font-noto text-gray-400 text-sm">Battle players in real-time · ELO matchmaking</p>
            </div>

            {/* Rating Card */}
            {!loading && ratingInfo && tierStyle && (
                <div className={`z-10 mb-8 w-full max-w-sm rounded-2xl ${tierStyle.bg} border ${tierStyle.border} p-6`}
                    style={{ boxShadow: `0 0 20px ${tierStyle.glow}30` }}>
                    <div className="flex items-center gap-4 mb-4">
                        <div className={`w-14 h-14 rounded-full flex items-center justify-center text-3xl border ${tierStyle.border}`}
                            style={{ background: 'rgba(0,0,0,0.4)', boxShadow: `0 0 15px ${tierStyle.glow}` }}>
                            {TIER_ICONS[ratingInfo.tier] ?? '🏆'}
                        </div>
                        <div>
                            <div className="font-pixel text-[10px] text-gray-500 mb-0.5">YOUR TIER</div>
                            <div className={`font-press text-xl ${tierStyle.text}`}
                                style={{ textShadow: `0 0 10px ${tierStyle.glow}` }}>
                                {ratingInfo.tier.toUpperCase()}
                            </div>
                        </div>
                        <div className="ml-auto text-right">
                            <div className="font-pixel text-[10px] text-gray-500 mb-0.5">RATING</div>
                            <div className="font-press text-2xl text-white">{ratingInfo.pvpRating}</div>
                        </div>
                    </div>

                    <div className="grid grid-cols-3 gap-2">
                        <div className="text-center bg-black/30 rounded-lg py-2 border border-gray-800">
                            <div className="font-pixel text-[9px] text-green-400 mb-0.5">WINS</div>
                            <div className="font-press text-base text-white">{ratingInfo.pvpWins}</div>
                        </div>
                        <div className="text-center bg-black/30 rounded-lg py-2 border border-gray-800">
                            <div className="font-pixel text-[9px] text-red-400 mb-0.5">LOSSES</div>
                            <div className="font-press text-base text-white">{ratingInfo.pvpLosses}</div>
                        </div>
                        <div className="text-center bg-black/30 rounded-lg py-2 border border-gray-800">
                            <div className="font-pixel text-[9px] text-blue-400 mb-0.5">WIN RATE</div>
                            <div className="font-press text-base text-white">
                                {winRate !== null ? `${winRate}%` : '-'}
                            </div>
                        </div>
                    </div>
                </div>
            )}
            {loading && (
                <div className="mb-8 w-full max-w-sm h-36 rounded-2xl bg-gray-800/30 border border-gray-700/30 animate-pulse" />
            )}

            {/* Action Cards */}
            <div className="z-10 w-full max-w-sm space-y-4">

                {/* Find Match — PRIMARY */}
                <div className="relative overflow-hidden rounded-2xl border border-purple-500/40 bg-purple-950/30 p-6"
                    style={{ boxShadow: '0 0 30px rgba(168,85,247,0.15)' }}>
                    {/* Animated gradient bar on top */}
                    <div className="absolute top-0 left-0 right-0 h-0.5"
                        style={{ background: 'linear-gradient(90deg, transparent, #a855f7, #3b82f6, transparent)' }} />

                    <div className="flex items-center gap-3 mb-3">
                        <span className="text-2xl">🔍</span>
                        <div>
                            <h2 className="font-press text-sm text-purple-300">FIND A MATCH</h2>
                            <p className="font-noto text-gray-500 text-xs mt-0.5">ELO-based · Auto-matched instantly</p>
                        </div>
                        <span className="ml-auto font-pixel text-[9px] text-green-400 bg-green-900/30 border border-green-500/30 rounded-full px-2 py-0.5">
                            RECOMMENDED
                        </span>
                    </div>
                    <button
                        id="btn-find-match"
                        onClick={() => navigate('/pvp/pets?mode=matchmaking')}
                        className="w-full py-4 rounded-xl font-press text-sm text-white transition-all duration-200
                            bg-gradient-to-r from-purple-600 to-indigo-600
                            hover:from-purple-500 hover:to-indigo-500 hover:scale-[1.02]
                            shadow-[0_4px_20px_rgba(168,85,247,0.4)] hover:shadow-[0_4px_28px_rgba(168,85,247,0.6)]">
                        FIND MATCH ⚔️
                    </button>
                </div>

                {/* Create Room */}
                <div className="rounded-2xl border border-gray-700/60 bg-gray-800/30 p-5 flex flex-col gap-3">
                    <div className="flex items-center gap-3">
                        <span className="text-xl">🏠</span>
                        <div>
                            <h2 className="font-pixel text-xs text-gray-300">HOST A ROOM</h2>
                            <p className="font-noto text-gray-500 text-xs mt-0.5">Share code with a friend</p>
                        </div>
                    </div>
                    <button
                        id="btn-create-room"
                        onClick={() => navigate('/pvp/pets?mode=create')}
                        className="w-full py-3 rounded-xl font-press text-sm bg-gray-700 hover:bg-gray-600 text-white transition-all hover:scale-[1.01]">
                        CREATE ROOM
                    </button>
                </div>

                {/* Join Room */}
                <div className="rounded-2xl border border-gray-700/60 bg-gray-800/30 p-5 flex flex-col gap-3">
                    <div className="flex items-center gap-3">
                        <span className="text-xl">🎮</span>
                        <div>
                            <h2 className="font-pixel text-xs text-gray-300">JOIN A ROOM</h2>
                            <p className="font-noto text-gray-500 text-xs mt-0.5">Enter your friend's code</p>
                        </div>
                    </div>
                    <input
                        id="input-room-code"
                        type="text"
                        value={joinCode}
                        onChange={e => setJoinCode(e.target.value.toUpperCase())}
                        placeholder="ENTER CODE"
                        maxLength={6}
                        className="w-full bg-gray-900 border-2 border-gray-600 rounded-xl px-4 py-3
                            font-press text-center text-lg text-yellow-400 uppercase tracking-widest
                            focus:border-purple-500 focus:outline-none focus:shadow-[0_0_12px_rgba(168,85,247,0.3)]
                            transition-all placeholder:text-gray-700"
                    />
                    <button
                        id="btn-join-room"
                        onClick={() => {
                            if (joinCode.trim().length === 6)
                                navigate(`/pvp/pets?mode=join&code=${joinCode.trim()}`);
                        }}
                        disabled={joinCode.trim().length !== 6}
                        className={`w-full py-3 rounded-xl font-press text-sm transition-all
                            ${joinCode.trim().length === 6
                                ? 'bg-blue-600 hover:bg-blue-500 text-white hover:scale-[1.01] shadow-[0_0_15px_rgba(37,99,235,0.3)]'
                                : 'bg-gray-700/50 text-gray-500 cursor-not-allowed border border-gray-700'}`}>
                        JOIN ROOM
                    </button>
                </div>
            </div>
        </div>
    );
}
