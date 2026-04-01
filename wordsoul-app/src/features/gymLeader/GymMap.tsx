import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetchGyms } from '../../services/gym';
import type { GymLeaderDto } from '../../types/GymTypes';
import { GymStatus } from '../../types/GymTypes';

// ── Johto & Kanto Gym theme colors & type icons ─────────────────────────────────
const GYM_THEMES: Record<number, { color: string; glow: string; type: string; emoji: string }> = {
  // Johto (1-8)
  1: { color: '#a0c8f0', glow: 'shadow-blue-300', type: 'Flying', emoji: '🦅' },
  2: { color: '#a8b820', glow: 'shadow-lime-500', type: 'Bug', emoji: '🐛' },
  3: { color: '#a8a878', glow: 'shadow-gray-400', type: 'Normal', emoji: '⭐' },
  4: { color: '#705898', glow: 'shadow-purple-500', type: 'Ghost', emoji: '👻' },
  5: { color: '#c03028', glow: 'shadow-red-500', type: 'Fighting', emoji: '🥋' },
  6: { color: '#b8b8d0', glow: 'shadow-gray-300', type: 'Steel', emoji: '⚙️' },
  7: { color: '#98d8d8', glow: 'shadow-cyan-300', type: 'Ice', emoji: '❄️' },
  8: { color: '#7038f8', glow: 'shadow-indigo-500', type: 'Dragon', emoji: '🐉' },
  // Kanto (9-16)
  9: { color: '#b8a038', glow: 'shadow-yellow-600', type: 'Rock', emoji: '🪨' },
  10: { color: '#6890f0', glow: 'shadow-blue-500', type: 'Water', emoji: '💧' },
  11: { color: '#f8d030', glow: 'shadow-yellow-400', type: 'Electric', emoji: '⚡' },
  12: { color: '#78c850', glow: 'shadow-green-500', type: 'Grass', emoji: '🌿' },
  13: { color: '#a040a0', glow: 'shadow-purple-400', type: 'Poison', emoji: '☠️' },
  14: { color: '#f85888', glow: 'shadow-pink-400', type: 'Psychic', emoji: '🔮' },
  15: { color: '#f08030', glow: 'shadow-orange-500', type: 'Fire', emoji: '🔥' },
  16: { color: '#f85888', glow: 'shadow-red-400', type: 'Champion', emoji: '👑' },
};

const STATUS_STYLE: Record<GymStatus, { badge: string; border: string; opacity: string }> = {
  [GymStatus.Defeated]: { badge: 'bg-green-500 text-white', border: 'border-green-400', opacity: 'opacity-100' },
  [GymStatus.Unlocked]: { badge: 'bg-yellow-400 text-black', border: 'border-yellow-300', opacity: 'opacity-100' },
  [GymStatus.Locked]: { badge: 'bg-gray-600 text-gray-300', border: 'border-gray-600', opacity: 'opacity-40' },
};

const STATUS_LABEL: Record<GymStatus, string> = {
  [GymStatus.Defeated]: '✅ Defeated',
  [GymStatus.Unlocked]: '⚔️ Challenge',
  [GymStatus.Locked]: '🔒 Locked',
};

// ── Badge images (placeholder SVG data-uris for now) ───────────────
const BADGE_ICONS: Record<number, string> = {
  1: '🌪️', 2: '🐞', 3: '🥛', 4: '🌫️',
  5: '🌪️', 6: '🔩', 7: '🧊', 8: '🐉',
  9: '🏅', 10: '💧', 11: '⚡', 12: '🌸',
  13: '💜', 14: '👁️', 15: '🌋', 16: '🌍',
};

export default function GymMap() {
  const [gyms, setGyms] = useState<GymLeaderDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    fetchGyms()
      .then(setGyms)
      .catch(() => setError('Không thể tải danh sách Gym. Hãy thử lại!'))
      .finally(() => setLoading(false));
  }, []);

  const defeatedCount = gyms.filter(g => g.status === GymStatus.Defeated).length;

  if (loading) return <GymMapSkeleton />;
  if (error) return (
    <div className="min-h-screen flex items-center justify-center" style={{ background: 'rgb(2,6,23)' }}>
      <p className="text-red-400 font-pixel text-sm">{error}</p>
    </div>
  );

  return (
    <div className="min-h-screen text-white mt-12" style={{ background: 'rgb(2,6,23)' }}>
      {/* ── Header ── */}
      <div className="relative overflow-hidden py-12 px-6 text-center"
        style={{ background: 'linear-gradient(135deg, #0f0c29, #302b63, #24243e)' }}>
        <div className="absolute inset-0 opacity-10"
          style={{ backgroundImage: 'repeating-linear-gradient(0deg,transparent,transparent 30px,rgba(255,255,255,.1) 30px,rgba(255,255,255,.1) 31px),repeating-linear-gradient(90deg,transparent,transparent 30px,rgba(255,255,255,.1) 30px,rgba(255,255,255,.1) 31px)' }} />
        <h1 className="font-press text-2xl md:text-3xl mb-2" style={{ color: '#f6e05e', textShadow: '0 0 20px #f6e05e88' }}>
          JOHTO & KANTO GYM CIRCUIT
        </h1>
        <p className="text-gray-300 font-noto text-base mb-4">Challenge 16 Gym Leaders to prove your vocabulary mastery</p>
        {/* Badge case */}
        <div className="inline-flex flex-wrap justify-center gap-3 bg-black/40 rounded-xl px-6 py-3 border border-yellow-400/30">
          {gyms.map(gym => (
            <span key={gym.id} title={gym.badgeName}
              className={`w-8 h-8 flex items-center justify-center text-xl transition-all duration-300 ${gym.status === GymStatus.Defeated ? 'scale-110' : 'grayscale opacity-30'}`}>
              {gym.badgeImageUrl ? (
                <img src={gym.badgeImageUrl} alt={gym.badgeName} className="w-full h-full object-contain pixel-art" />
              ) : (
                BADGE_ICONS[gym.gymOrder] || '🛡️'
              )}
            </span>
          ))}
        </div>
        <p className="text-yellow-400 font-pixel text-xs mt-3">{defeatedCount} / 16 BADGES</p>
      </div>

      {/* ── Gym Grid ── */}
      <div className="max-w-5xl mx-auto px-4 py-10 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
        {gyms.map(gym => {
          const theme = GYM_THEMES[gym.gymOrder];
          const style = STATUS_STYLE[gym.status] ?? STATUS_STYLE[GymStatus.Locked];
          const xpPct = Math.min(100, Math.round((gym.currentXp / gym.xpThreshold) * 100));
          const vocabPct = Math.min(100, Math.round((gym.currentVocabCount / gym.vocabThreshold) * 100));

          return (
            <div key={gym.id}
              className={`relative rounded-xl border-2 ${style.border} ${style.opacity} custom-cursor transition-all duration-300 hover:scale-105 hover:shadow-lg overflow-hidden`}
              style={{ background: 'rgba(15,23,42,0.95)', boxShadow: gym.status !== GymStatus.Locked ? `0 0 20px ${theme.color}44` : undefined }}
              onClick={() => gym.status !== GymStatus.Locked && navigate(`/gym/${gym.id}`)}>

              {/* Gym # badge */}
              <div className="absolute top-2 left-2 font-pixel text-xs px-2 py-1 rounded"
                style={{ background: theme.color + '33', color: theme.color, border: `1px solid ${theme.color}66` }}>
                #{gym.gymOrder}
              </div>

              {/* Status badge */}
              <div className={`absolute top-2 right-2 text-xs font-pixel px-2 py-1 rounded ${style.badge}`}>
                {gym.status === GymStatus.Defeated ? '✓' : gym.status === GymStatus.Unlocked ? '!' : '🔒'}
              </div>

              {/* Avatar area */}
              <div className="pt-10 pb-4 flex flex-col items-center gap-2">
                <div className="w-16 h-16 rounded-full flex items-center justify-center text-4xl"
                  style={{ background: `${theme.color}22`, border: `2px solid ${theme.color}66` }}>
                  {gym.avatarUrl
                    ? <img src={gym.avatarUrl} alt={gym.name} className="w-full h-full rounded-full object-cover pixel-art" />
                    : theme.emoji}
                </div>
                <div className="text-center">
                  <p className="font-press text-xs" style={{ color: theme.color }}>{gym.name}</p>
                  <p className="text-gray-400 text-xs font-noto mt-1">{gym.title}</p>
                </div>
              </div>

              {/* Progress bars */}
              {gym.status !== GymStatus.Defeated && (
                <div className="px-4 pb-4 space-y-2">
                  <ProgressBar label="XP" value={xpPct} color={theme.color}
                    tooltip={`${gym.currentXp.toLocaleString()} / ${gym.xpThreshold.toLocaleString()} XP`} />
                  <ProgressBar label="Words" value={vocabPct} color={theme.color}
                    tooltip={`${gym.currentVocabCount} / ${gym.vocabThreshold} words`} />
                </div>
              )}

              {/* Action button */}
              <div className="px-4 pb-4">
                <button
                  disabled={gym.status === GymStatus.Locked}
                  onClick={e => { e.stopPropagation(); navigate(`/gym/${gym.id}`); }}
                  className={`w-full py-2 rounded font-pixel text-xs transition-all ${gym.status === GymStatus.Locked
                    ? 'bg-gray-700 text-gray-500 cursor-not-allowed'
                    : gym.status === GymStatus.Defeated
                      ? 'bg-green-700 hover:bg-green-600 text-white'
                      : 'text-black hover:opacity-90 custom-cursor'
                    }`}
                  style={gym.status === GymStatus.Unlocked ? { background: theme.color } : undefined}>
                  {STATUS_LABEL[gym.status]}
                </button>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

function ProgressBar({ label, value, color, tooltip }: { label: string; value: number; color: string; tooltip: string }) {
  return (
    <div title={tooltip}>
      <div className="flex justify-between text-xs text-gray-400 mb-1 font-noto">
        <span>{label}</span><span>{value}%</span>
      </div>
      <div className="h-1.5 bg-gray-700 rounded-full overflow-hidden">
        <div className="h-full rounded-full transition-all duration-500"
          style={{ width: `${value}%`, background: color, boxShadow: `0 0 6px ${color}88` }} />
      </div>
    </div>
  );
}

function GymMapSkeleton() {
  return (
    <div className="min-h-screen" style={{ background: 'rgb(2,6,23)' }}>
      <div className="h-48 animate-pulse" style={{ background: 'rgba(48,43,99,0.5)' }} />
      <div className="max-w-5xl mx-auto px-4 py-10 grid grid-cols-2 lg:grid-cols-4 gap-5">
        {[...Array(16)].map((_, i) => (
          <div key={i} className="h-64 rounded-xl animate-pulse" style={{ background: 'rgba(255,255,255,0.05)' }} />
        ))}
      </div>
    </div>
  );
}
