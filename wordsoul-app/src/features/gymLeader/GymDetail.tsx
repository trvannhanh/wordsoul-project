import { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { fetchGymDetail } from '../../services/gym';
import type { GymLeaderDto } from '../../types/GymTypes';
import { GymStatus } from '../../types/GymTypes';

const THEME_COLORS: Record<string, string> = {
  DailyLife: '#a8a878', Nature: '#78c850', Food: '#f08030', Weather: '#a0c8f0',
  Technology: '#f8d030', Travel: '#e0c068', Health: '#ee99ac', Sports: '#c03028',
  Business: '#b8b8d0', Science: '#f85888', Art: '#a8b820', Communication: '#6890f0',
  Mystery: '#705898', Dark: '#705848', Academic: '#98d8d8',
  Challenge: '#b8a038', TrapWords: '#a040a0', System: '#7038f8', Custom: '#a8a8a8',
};
const THEME_EMOJI: Record<string, string> = {
  DailyLife: '⭐', Nature: '🌿', Food: '🔥', Weather: '🦅',
  Technology: '⚡', Travel: '🏜️', Health: '✨', Sports: '🥋',
  Business: '⚙️', Science: '🔮', Art: '🐛', Communication: '💧',
  Mystery: '👻', Dark: '🌙', Academic: '❄️',
  Challenge: '🪨', TrapWords: '☠️', System: '🐉', Custom: '👑',
};

function useCooldownTimer(cooldownEndsAt?: string) {
  const [remaining, setRemaining] = useState('');
  useEffect(() => {
    if (!cooldownEndsAt) return;
    const tick = () => {
      const diff = new Date(cooldownEndsAt).getTime() - Date.now();
      if (diff <= 0) { setRemaining(''); return; }
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

export default function GymDetail() {
  const { gymId } = useParams<{ gymId: string }>();
  const navigate = useNavigate();
  const [gym, setGym] = useState<GymLeaderDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const cooldownRemaining = useCooldownTimer(gym?.cooldownEndsAt);
  const color = gym ? (THEME_COLORS[gym.theme] ?? '#a8a8a8') : '#a8a8a8';

  const load = useCallback(async () => {
    if (!gymId) return;
    try {
      const data = await fetchGymDetail(Number(gymId));
      setGym(data);
    } catch {
      setError('Không thể tải thông tin Gym.');
    } finally {
      setLoading(false);
    }
  }, [gymId]);

  useEffect(() => { load(); }, [load]);

  const handleStart = () => {
    if (!gym) return;
    navigate(`/gym/${gym.id}/pets`);
  };

  if (loading) return <DetailSkeleton />;
  if (!gym) return (
    <div className="min-h-screen flex items-center justify-center" style={{ background: 'rgb(2,6,23)' }}>
      <p className="text-red-400 font-pixel text-sm">Gym not found.</p>
    </div>
  );

  const xpPct = Math.min(100, Math.round((gym.currentXp / gym.xpThreshold) * 100));
  const vocabPct = Math.min(100, Math.round((gym.currentVocabCount / gym.vocabThreshold) * 100));
  const canBattle = gym.status === GymStatus.Unlocked && !gym.isOnCooldown;

  return (
    <div className="min-h-screen text-white" style={{ background: 'rgb(2,6,23)' }}>
      {/* Banner */}
      <div className="relative overflow-hidden py-16 px-6 text-center"
        style={{ background: `linear-gradient(135deg, rgb(2,6,23) 0%, ${color}22 50%, rgb(2,6,23) 100%)` }}>
        <button onClick={() => navigate('/gym')}
          className="absolute top-4 left-4 text-gray-400 hover:text-white font-pixel text-xs flex items-center gap-1">
          ← Back
        </button>

        {/* Avatar */}
        <div className="mx-auto mb-4 w-28 h-28 rounded-full flex items-center justify-center text-6xl"
          style={{ border: `3px solid ${color}`, boxShadow: `0 0 30px ${color}66`, background: `${color}22` }}>
          {gym.avatarUrl
            ? <img src={gym.avatarUrl} alt={gym.name} className="w-full h-full rounded-full object-cover pixel-art" />
            : THEME_EMOJI[gym.theme] ?? '🏅'}
        </div>

        <h1 className="font-press text-2xl mb-1" style={{ color, textShadow: `0 0 20px ${color}88` }}>
          {gym.name}
        </h1>
        <p className="text-gray-300 font-noto text-base mb-2">{gym.title}</p>
        <span className={`inline-block px-3 py-1 rounded-full text-xs font-pixel ${gym.status === GymStatus.Defeated ? 'bg-green-500/20 text-green-400 border border-green-500/40'
          : gym.status === GymStatus.Unlocked ? 'bg-yellow-500/20 text-yellow-400 border border-yellow-500/40'
            : 'bg-gray-700/50 text-gray-400 border border-gray-600/40'}`}>
          {gym.status === GymStatus.Defeated ? '✅ DEFEATED' : gym.status === GymStatus.Unlocked ? '⚔️ UNLOCKED' : '🔒 LOCKED'}
        </span>
      </div>

      <div className="max-w-2xl mx-auto px-4 py-8 space-y-6">
        {/* Description */}
        <div className="rounded-xl p-5 border"
          style={{ background: `${color}0d`, borderColor: `${color}33` }}>
          <p className="text-gray-300 font-noto text-sm leading-relaxed">{gym.description}</p>
        </div>

        {/* Stats row */}
        <div className="grid grid-cols-3 gap-3">
          {[
            { label: 'Theme', value: gym.theme },
            { label: 'CEFR', value: gym.requiredCefrLevel },
            { label: 'Questions', value: gym.questionCount.toString() },
          ].map(({ label, value }) => (
            <div key={label} className="rounded-lg p-3 text-center border border-white/10"
              style={{ background: 'rgba(255,255,255,0.03)' }}>
              <p className="text-gray-500 font-noto text-xs mb-1">{label}</p>
              <p className="font-pixel text-xs" style={{ color }}>{value}</p>
            </div>
          ))}
        </div>

        {/* Unlock conditions */}
        <div className="rounded-xl p-5 border border-white/10" style={{ background: 'rgba(255,255,255,0.03)' }}>
          <h2 className="font-press text-xs mb-4" style={{ color }}>UNLOCK CONDITIONS</h2>
          <div className="space-y-4">
            <Condition
              label={`XP — ${gym.currentXp.toLocaleString()} / ${gym.xpThreshold.toLocaleString()}`}
              pct={xpPct} color={color} met={gym.currentXp >= gym.xpThreshold} />
            <Condition
              label={`${gym.requiredCefrLevel} Words (≥${gym.requiredMemoryState}) — ${gym.currentVocabCount} / ${gym.vocabThreshold}`}
              pct={vocabPct} color={color} met={gym.currentVocabCount >= gym.vocabThreshold} />
            {gym.gymOrder > 1 && (
              <div className="flex items-center gap-2 text-xs font-noto text-gray-400">
                <span className={gym.status !== GymStatus.Locked ? 'text-green-400' : 'text-red-400'}>
                  {gym.status !== GymStatus.Locked ? '✓' : '✗'}
                </span>
                Previous Gym defeated
              </div>
            )}
          </div>
        </div>

        {/* Battle record */}
        {gym.totalAttempts > 0 && (
          <div className="grid grid-cols-2 gap-3">
            <Stat label="Attempts" value={gym.totalAttempts.toString()} color={color} />
            <Stat label="Best Score" value={`${gym.bestScore}%`} color={color} />
            {gym.defeatedAt && (
              <Stat label="Defeated On" value={new Date(gym.defeatedAt).toLocaleDateString()} color={color} />
            )}
            <Stat label="Pass Rate" value={`${gym.passRatePercent}%`} color={color} />
          </div>
        )}

        {/* Reward preview */}
        <div className="rounded-xl p-4 border border-yellow-500/20 bg-yellow-500/5">
          <p className="font-pixel text-xs text-yellow-400 mb-2">🏆 VICTORY REWARD</p>
          <div className="flex items-center gap-4 text-sm font-noto text-gray-300">
            <span>+{gym.xpReward} XP</span>
            <span className="flex items-center gap-2">
              •
              {gym.badgeImageUrl && <img src={gym.badgeImageUrl} alt={gym.badgeName} className="w-6 h-6 object-contain pixel-art" />}
              {gym.badgeName}
            </span>
          </div>
        </div>

        {/* Cooldown warning */}
        {gym.isOnCooldown && cooldownRemaining && (
          <div className="rounded-xl p-4 border border-red-500/30 bg-red-500/10 text-center">
            <p className="font-pixel text-red-400 text-xs">⏳ COOLDOWN</p>
            <p className="font-press text-xl text-red-300 mt-1">{cooldownRemaining}</p>
            <p className="text-gray-400 text-xs font-noto mt-1">Come back after the cooldown to challenge again</p>
          </div>
        )}

        {/* Error */}
        {error && (
          <div className="rounded-lg p-3 border border-red-500/30 bg-red-500/10">
            <p className="text-red-400 font-noto text-sm text-center">{error}</p>
          </div>
        )}

        {/* Action button */}
        {gym.status === GymStatus.Locked && (
          <button disabled className="w-full py-4 rounded-xl font-pixel text-sm text-gray-500 bg-gray-800 cursor-not-allowed border border-gray-700">
            🔒 LOCKED — Meet the conditions first
          </button>
        )}
        {gym.status === GymStatus.Defeated && (
          <button onClick={handleStart}
            className="w-full py-4 rounded-xl font-pixel text-sm text-white bg-green-700 hover:bg-green-600 transition-colors border border-green-500/30">
            🔁 REMATCH
          </button>
        )}
        {canBattle && (
          <button onClick={handleStart}
            className="w-full py-4 rounded-xl font-pixel text-sm transition-all hover:scale-105 hover:shadow-lg"
            style={{ background: color, color: '#000', boxShadow: `0 0 20px ${color}66` }}>
            ⚔️ CHALLENGE {gym.name.toUpperCase()}!
          </button>
        )}
        {gym.status === GymStatus.Unlocked && gym.isOnCooldown && (
          <button disabled className="w-full py-4 rounded-xl font-pixel text-sm text-gray-500 bg-gray-800 cursor-not-allowed border border-gray-700">
            ⏳ On Cooldown
          </button>
        )}
      </div>
    </div>
  );
}

function Condition({ label, pct, color, met }: { label: string; pct: number; color: string; met: boolean }) {
  return (
    <div>
      <div className="flex justify-between text-xs font-noto text-gray-400 mb-1">
        <span className="flex items-center gap-1">
          <span className={met ? 'text-green-400' : 'text-gray-500'}>{met ? '✓' : '○'}</span>
          {label}
        </span>
        <span>{pct}%</span>
      </div>
      <div className="h-2 bg-gray-800 rounded-full overflow-hidden">
        <div className="h-full rounded-full transition-all duration-700"
          style={{ width: `${pct}%`, background: met ? '#48bb78' : color, boxShadow: `0 0 8px ${met ? '#48bb7888' : color + '88'}` }} />
      </div>
    </div>
  );
}

function Stat({ label, value, color }: { label: string; value: string; color: string }) {
  return (
    <div className="rounded-lg p-3 border border-white/10 text-center" style={{ background: 'rgba(255,255,255,0.03)' }}>
      <p className="text-gray-500 font-noto text-xs mb-1">{label}</p>
      <p className="font-pixel text-sm" style={{ color }}>{value}</p>
    </div>
  );
}

function DetailSkeleton() {
  return (
    <div className="min-h-screen" style={{ background: 'rgb(2,6,23)' }}>
      <div className="h-64 animate-pulse" style={{ background: 'rgba(255,255,255,0.05)' }} />
      <div className="max-w-2xl mx-auto px-4 py-8 space-y-4">
        {[...Array(4)].map((_, i) => (
          <div key={i} className="h-20 rounded-xl animate-pulse" style={{ background: 'rgba(255,255,255,0.04)' }} />
        ))}
      </div>
    </div>
  );
}
