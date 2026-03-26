
import { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import type { BattleResultDto } from '../../types/GymTypes';

const THEME_COLORS: Record<number, string> = {
  1: '#a0c8f0', 2: '#a8b820', 3: '#a8a878', 4: '#705898',
  5: '#c03028', 6: '#b8b8d0', 7: '#98d8d8', 8: '#7038f8',
  9: '#b8a038', 10: '#6890f0', 11: '#f8d030', 12: '#78c850',
  13: '#a040a0', 14: '#f85888', 15: '#f08030', 16: '#f85888',
};

interface LocationState { result: BattleResultDto; gymLeaderId: number }

function useCooldownTimer(cooldownEndsAt?: string) {
  const [remaining, setRemaining] = useState('');
  useEffect(() => {
    if (!cooldownEndsAt) return;
    const tick = () => {
      const diff = new Date(cooldownEndsAt).getTime() - Date.now();
      if (diff <= 0) { setRemaining('READY'); return; }
      const h = Math.floor(diff / 3600000);
      const m = Math.floor((diff % 3600000) / 60000);
      const s = Math.floor((diff % 60000) / 1000);
      setRemaining(`${h}h ${m}m ${s}s`);
    };
    tick();
    const id = setInterval(tick, 1000);
    return () => clearInterval(id);
  }, [cooldownEndsAt]);
  return remaining;
}

export default function GymResult() {
  const location = useLocation();
  const navigate = useNavigate();
  const state = location.state as LocationState | undefined;

  const result = state?.result;
  const gymLeaderId = state?.gymLeaderId ?? 1;
  const color = THEME_COLORS[gymLeaderId] ?? '#f6e05e';
  const cooldown = useCooldownTimer(result?.cooldownEndsAt);

  const [showReview, setShowReview] = useState(false);
  const [animate, setAnimate] = useState(false);

  useEffect(() => {
    if (!result) navigate('/gym');
    else setTimeout(() => setAnimate(true), 100);
  }, [result, navigate]);

  if (!result) return null;

  const { isVictory, scorePercent, correctAnswers, totalQuestions,
          passRatePercent, xpEarned, badgeEarned, badgeName, badgeImageUrl, gymLeaderName,
          answerResults, bestScore } = result;

  return (
    <div className="min-h-screen flex flex-col items-center justify-start py-10 px-4 text-white"
      style={{ background: 'rgb(2,6,23)' }}>

      {/* Victory / Defeat card */}
      <div className={`w-full max-w-md rounded-2xl border p-8 text-center transition-all duration-700 ${animate ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`}
        style={{
          borderColor: isVictory ? `${color}66` : '#ff444466',
          background: isVictory ? `${color}0d` : 'rgba(255,68,68,0.07)',
          boxShadow: isVictory ? `0 0 50px ${color}44` : '0 0 50px #ff444422',
        }}>

        {/* Result headline */}
        <div className="text-6xl mb-4">{isVictory ? '🏆' : '💀'}</div>
        <h1 className="font-press text-xl mb-1" style={{ color: isVictory ? color : '#fc8181' }}>
          {isVictory ? 'VICTORY!' : 'DEFEATED'}
        </h1>
        <p className="text-gray-400 font-noto text-sm mb-6">
          {isVictory ? `You conquered ${gymLeaderName}!` : `${gymLeaderName} was too strong...`}
        </p>

        {/* Score ring */}
        <div className="relative mx-auto w-28 h-28 mb-6">
          <svg className="w-full h-full -rotate-90" viewBox="0 0 100 100">
            <circle cx="50" cy="50" r="44" fill="none" stroke="rgba(255,255,255,0.07)" strokeWidth="8" />
            <circle cx="50" cy="50" r="44" fill="none"
              stroke={isVictory ? color : '#fc8181'} strokeWidth="8"
              strokeDasharray={`${2 * Math.PI * 44}`}
              strokeDashoffset={`${2 * Math.PI * 44 * (1 - scorePercent / 100)}`}
              strokeLinecap="round"
              style={{ transition: 'stroke-dashoffset 1.5s ease', filter: `drop-shadow(0 0 6px ${isVictory ? color : '#fc8181'})` }} />
          </svg>
          <div className="absolute inset-0 flex flex-col items-center justify-center">
            <span className="font-press text-xl" style={{ color: isVictory ? color : '#fc8181' }}>{scorePercent}%</span>
            <span className="text-gray-500 text-xs font-noto">{correctAnswers}/{totalQuestions}</span>
          </div>
        </div>

        <p className="text-gray-500 text-xs font-noto mb-6">Pass rate: {passRatePercent}% — Best: {bestScore}%</p>

        {/* Rewards */}
        {isVictory && (
          <div className="space-y-3 mb-6">
            {xpEarned > 0 && (
              <div className="flex items-center justify-center gap-2 py-2 px-4 rounded-lg bg-yellow-500/10 border border-yellow-500/20">
                <span className="text-yellow-400 font-pixel text-sm">+{xpEarned} XP</span>
              </div>
            )}
            {badgeEarned && (
              <div className="flex items-center justify-center gap-2 py-2 px-4 rounded-lg border"
                style={{ background: `${color}15`, borderColor: `${color}40` }}>
                {badgeImageUrl ? (
                    <img src={badgeImageUrl} alt={badgeName} className="w-6 h-6 object-contain pixel-art" />
                ) : (
                    <span className="text-2xl">🏅</span>
                )}
                <span className="font-pixel text-xs" style={{ color }}>{badgeName} earned!</span>
              </div>
            )}
          </div>
        )}

        {/* Cooldown */}
        {!isVictory && result.isOnCooldown && cooldown && (
          <div className="mb-6 py-3 px-4 rounded-lg bg-red-500/10 border border-red-500/20">
            <p className="text-red-400 font-pixel text-xs mb-1">⏳ COOLDOWN</p>
            <p className="font-press text-lg text-red-300">{cooldown}</p>
          </div>
        )}
      </div>

      {/* Actions */}
      <div className="mt-6 w-full max-w-md flex flex-col gap-3">
        <button onClick={() => setShowReview(r => !r)}
          className="w-full py-3 rounded-xl border border-white/10 font-pixel text-xs text-gray-300 hover:bg-white/5 transition-colors">
          {showReview ? '▲ Hide Review' : '▼ Review Answers'}
        </button>
        <button onClick={() => navigate(`/gym/${gymLeaderId}`)}
          className="w-full py-3 rounded-xl font-pixel text-xs transition-all hover:scale-105"
          style={{ background: color, color: '#000' }}>
          ← Back to Gym
        </button>
        <button onClick={() => navigate('/gym')}
          className="w-full py-3 rounded-xl border border-white/10 font-pixel text-xs text-gray-400 hover:bg-white/5 transition-colors">
          🗺️ Gym Map
        </button>
      </div>

      {/* Answer review */}
      {showReview && (
        <div className="mt-6 w-full max-w-md space-y-2">
          <h2 className="font-pixel text-xs text-gray-400 mb-3">ANSWER REVIEW</h2>
          {answerResults.map((ar) => (
            <div key={ar.vocabularyId}
              className={`flex items-center gap-3 p-3 rounded-lg border text-sm font-noto ${
                ar.isCorrect ? 'border-green-500/20 bg-green-500/5' : 'border-red-500/20 bg-red-500/5'}`}>
              <span className={ar.isCorrect ? 'text-green-400' : 'text-red-400'}>
                {ar.isCorrect ? '✓' : '✗'}
              </span>
              <div className="flex-1 min-w-0">
                <p className="font-pixel text-xs text-white truncate">{ar.word}</p>
                <p className="text-gray-500 text-xs truncate">{ar.meaning}</p>
              </div>
              {!ar.isCorrect && ar.userAnswer && (
                <span className="text-red-400 text-xs truncate ml-1">"{ar.userAnswer}"</span>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
