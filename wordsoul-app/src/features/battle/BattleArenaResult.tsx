import { useLocation, useNavigate } from 'react-router-dom';
import type { BattleEndedDto } from '../../types/BattleArenaTypes';

export default function BattleArenaResult() {
    const navigate = useNavigate();
    const location = useLocation();

    // Result passed via navigate state
    const result: BattleEndedDto | undefined = location.state?.result;

    if (!result) {
        return (
            <div className="min-h-screen flex items-center justify-center flex-col gap-4 text-white font-pixel"
                style={{ background: 'rgb(2,6,23)' }}>
                <p>Result not found.</p>
                <button onClick={() => navigate('/gym')} className="text-gray-400 hover:text-white">
                    ← Back to Gym
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
        xpEarned,
        badgeEarned,
        badgeName,
        badgeImageUrl
    } = result;

    const mainColor = p1Won ? '#facc15' : '#f87171'; // yellow for win, red for lose

    return (
        <div className="min-h-screen text-white flex flex-col items-center justify-center py-12 px-4"
            style={{ background: `linear-gradient(135deg, rgb(2,6,23) 0%, ${mainColor}22 50%, rgb(10,2,30) 100%)` }}>

            <div className={`w-full max-w-md rounded-3xl p-8 border backdrop-blur text-center
                ${p1Won ? 'border-yellow-400/40 bg-yellow-900/10' : 'border-red-500/40 bg-red-900/10'}`}
                style={{ boxShadow: `0 0 40px ${mainColor}44` }}>

                {/* Title */}
                <h1 className="font-press text-3xl mb-2"
                    style={{ color: mainColor, textShadow: `0 0 15px ${mainColor}88` }}>
                    {p1Won ? 'VICTORY!' : 'DEFEATED'}
                </h1>
                <p className="font-noto text-gray-400 text-sm mb-8">
                    {p1Won ? 'You have proven your strength.' : 'Train harder and try again.'}
                </p>

                {/* Score vs Score */}
                <div className="flex items-center justify-center gap-6 mb-8">
                    <div className="text-right">
                        <div className="font-pixel text-[10px] text-blue-300 mb-1">YOU</div>
                        <div className="font-press text-2xl text-blue-400">{p1TotalScore}</div>
                    </div>
                    <div className="font-pixel text-gray-600 text-xl">VS</div>
                    <div className="text-left">
                        <div className="font-pixel text-[10px] text-red-300 mb-1">GYM</div>
                        <div className="font-press text-2xl text-red-400">{p2TotalScore}</div>
                    </div>
                </div>

                {/* Stats */}
                <div className="grid grid-cols-2 gap-4 mb-8">
                    <div className="rounded-xl p-3 border border-gray-700/50 bg-gray-800/40">
                        <p className="font-pixel text-[9px] text-gray-500 mb-1">ACCURACY</p>
                        <p className="font-press text-white">{Math.round((p1CorrectCount / totalRounds) * 100)}%</p>
                    </div>
                    <div className="rounded-xl p-3 border border-gray-700/50 bg-gray-800/40">
                        <p className="font-pixel text-[9px] text-gray-500 mb-1">CORRECT</p>
                        <p className="font-press text-white">{p1CorrectCount}/{totalRounds}</p>
                    </div>
                </div>

                {/* Rewards */}
                {(xpEarned > 0 || badgeEarned) && (
                    <div className="mb-8 p-4 rounded-xl border border-yellow-500/30 bg-yellow-500/10 inline-block text-left relative">
                        <h3 className="absolute -top-3 left-1/2 transform -translate-x-1/2 bg-yellow-900/80 px-2 font-pixel text-[10px] text-yellow-400 border border-yellow-500/50 rounded-full">
                            REWARDS
                        </h3>
                        {xpEarned > 0 && (
                            <div className="flex items-center gap-2 mt-2">
                                <span className="text-xl">✨</span>
                                <span className="font-press text-sm text-yellow-300">+{xpEarned} XP</span>
                            </div>
                        )}
                        {badgeEarned && (
                            <div className="flex items-center gap-3 mt-3">
                                {badgeImageUrl ? (
                                    <img src={badgeImageUrl} alt="Badge" className="w-10 h-10 pixel-art" />
                                ) : (
                                    <div className="w-10 h-10 rounded-full bg-yellow-400 flex items-center justify-center text-xl shadow-[0_0_10px_#facc15]">🏅</div>
                                )}
                                <div>
                                    <div className="font-pixel text-[10px] text-gray-400">BADGE EARNED</div>
                                    <div className="font-noto text-yellow-400 text-sm">{badgeName}</div>
                                </div>
                            </div>
                        )}
                    </div>
                )}

                {/* Actions */}
                <div className="flex flex-col gap-3">
                    <button onClick={() => navigate('/gym')}
                        className={`w-full py-4 rounded-xl font-press text-sm transition-all text-black
                            ${p1Won ? 'bg-yellow-400 hover:bg-yellow-300 hover:scale-105' : 'bg-gray-300 hover:bg-white'}
                        `}>
                        CONTINUE
                    </button>
                    {!p1Won && (
                        <button onClick={() => navigate(-1)} // Go back to pet selector ideally
                            className="w-full py-3 rounded-xl font-pixel text-xs text-gray-400 hover:text-white border border-gray-700 hover:bg-gray-800">
                            RETRY BATTLE
                        </button>
                    )}
                </div>

            </div>
        </div>
    );
}
