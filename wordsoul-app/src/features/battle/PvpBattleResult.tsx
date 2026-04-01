import { useLocation, useNavigate } from 'react-router-dom';
import type { BattleEndedDto } from '../../types/BattleArenaTypes';

export default function PvpBattleResult() {
    const navigate = useNavigate();
    const location = useLocation();

    // Result passed via navigate state
    const result: BattleEndedDto | undefined = location.state?.result;

    if (!result) {
        return (
            <div className="min-h-screen flex items-center justify-center flex-col gap-4 text-white font-pixel"
                style={{ background: 'rgb(2,6,23)' }}>
                <p>Result not found.</p>
                <button onClick={() => navigate('/pvp')} className="text-gray-400 hover:text-white">
                    ← Back to Lobby
                </button>
            </div>
        );
    }

    const {
        p1Won,
        p1TotalScore,
        p2TotalScore,
        p1CorrectCount,
        totalRounds,
        eloResult
    } = result;

    const mainColor = p1Won ? '#a855f7' : '#ef4444'; // purple for win, red for lose
    const shadowColor = p1Won ? 'rgba(168,85,247,0.5)' : 'rgba(239,68,68,0.5)';

    return (
        <div className="min-h-screen text-white flex flex-col items-center justify-center py-12 px-4"
            style={{ background: `linear-gradient(135deg, rgb(2,6,23) 0%, ${mainColor}22 50%, rgb(10,2,30) 100%)` }}>

            <div className={`w-full max-w-md rounded-3xl p-8 border backdrop-blur text-center
                ${p1Won ? 'border-purple-500/40 bg-purple-900/10' : 'border-red-500/40 bg-red-900/10'}`}
                style={{ boxShadow: `0 0 40px ${shadowColor}` }}>

                {/* Title */}
                <h1 className="font-press text-3xl mb-2"
                    style={{ color: mainColor, textShadow: `0 0 15px ${shadowColor}` }}>
                    {p1Won ? 'VICTORY!' : 'DEFEATED'}
                </h1>
                <p className="font-noto text-gray-400 text-sm mb-6">
                    {p1Won ? 'You dominated the match.' : 'Better luck next time.'}
                </p>

                {/* ELO Rating Changes card */}
                {eloResult && (
                    <div className="mb-6 p-4 rounded-xl border border-gray-700/50 bg-gray-800/60 flex justify-around items-center">
                        <div className="text-center">
                            <p className="font-pixel text-[8px] text-gray-500 mb-1">RATING</p>
                            <p className="font-press text-xl text-yellow-400">{eloResult.newRating}</p>
                            <p className={`font-pixel text-xs mt-1 ${eloResult.ratingChange >= 0 ? 'text-green-400' : 'text-red-400'}`}>
                                {eloResult.ratingChange >= 0 ? '+' : ''}{eloResult.ratingChange}
                            </p>
                        </div>

                        <div className="w-px h-12 bg-gray-700"></div>

                        <div className="text-center">
                            <p className="font-pixel text-[8px] text-gray-500 mb-1">TIER</p>
                            <p className={`font-press text-sm px-2 py-1 rounded inline-block
                                ${eloResult.newTier === 'Diamond' ? 'text-cyan-300 bg-cyan-900/30 border border-cyan-500/50' :
                                  eloResult.newTier === 'Platinum' ? 'text-blue-300 bg-blue-900/30 border border-blue-500/50' :
                                  eloResult.newTier === 'Gold' ? 'text-yellow-400 bg-yellow-900/30 border border-yellow-500/50' :
                                  eloResult.newTier === 'Silver' ? 'text-gray-300 bg-gray-700/50 border border-gray-500/50' :
                                  'text-orange-400 bg-orange-900/30 border border-orange-500/50'}`}>
                                {eloResult.newTier}
                            </p>
                            {eloResult.oldTier !== eloResult.newTier && (
                                <p className="font-pixel text-[8px] text-green-400 animate-pulse mt-2">TIER UP!</p>
                            )}
                        </div>
                    </div>
                )}

                {/* Score vs Score */}
                <div className="flex items-center justify-center gap-6 mb-8 mt-2">
                    <div className="text-right">
                        <div className="font-pixel text-[10px] text-blue-300 mb-1">YOU</div>
                        <div className="font-press text-2xl text-blue-400">{p1TotalScore}</div>
                    </div>
                    <div className="font-pixel text-gray-600 text-xl">VS</div>
                    <div className="text-left">
                        <div className="font-pixel text-[10px] text-red-300 mb-1">OPPONENT</div>
                        <div className="font-press text-2xl text-red-400">{p2TotalScore}</div>
                    </div>
                </div>

                {/* Stats */}
                <div className="grid grid-cols-2 gap-4 mb-8">
                    <div className="rounded-xl p-3 border border-gray-700/50 bg-gray-800/40">
                        <p className="font-pixel text-[9px] text-gray-500 mb-1">ACCURACY</p>
                        <p className="font-press text-white">{totalRounds > 0 ? Math.round((p1CorrectCount / totalRounds) * 100) : 0}%</p>
                    </div>
                    <div className="rounded-xl p-3 border border-gray-700/50 bg-gray-800/40">
                        <p className="font-pixel text-[9px] text-gray-500 mb-1">CORRECT</p>
                        <p className="font-press text-white">{p1CorrectCount}/{totalRounds}</p>
                    </div>
                </div>

                {/* Actions */}
                <div className="flex flex-col gap-3">
                    <button onClick={() => navigate('/pvp')}
                        className={`w-full py-4 rounded-xl font-press text-sm transition-all text-white
                            ${p1Won ? 'bg-purple-600 hover:bg-purple-500 hover:scale-105' : 'bg-gray-700 hover:bg-gray-600 border border-gray-600'}
                        `}>
                        BACK TO LOBBY
                    </button>
                    <button onClick={() => navigate('/home')}
                        className="w-full py-3 rounded-xl font-pixel text-xs text-gray-400 hover:text-white border border-gray-700 hover:bg-gray-900">
                        RETURN HOME
                    </button>
                </div>

            </div>
        </div>
    );
}
