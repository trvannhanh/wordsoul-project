import { useEffect, useRef, useState } from 'react';
import { motion, AnimatePresence, useTransform, useMotionValue } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import type { BattleResultDto, BattleAnswerResultDto } from '../../types/GymTypes';

// ─── Theme ────────────────────────────────────────────────────────────────────
const GYM_THEME: Record<number, { color: string; dimBg: string; emoji: string }> = {
  1: { color: '#c0c0c0', dimBg: 'rgba(192,192,192,0.06)', emoji: '⚔️'  },
  2: { color: '#48bb78', dimBg: 'rgba(72,187,120,0.06)',  emoji: '🌿'  },
  3: { color: '#90cdf4', dimBg: 'rgba(144,205,244,0.06)', emoji: '🪨'  },
  4: { color: '#4299e1', dimBg: 'rgba(66,153,225,0.06)',  emoji: '🌊'  },
  5: { color: '#f6e05e', dimBg: 'rgba(246,224,94,0.06)',  emoji: '⚡'  },
  6: { color: '#68d391', dimBg: 'rgba(104,211,145,0.06)', emoji: '🍃'  },
  7: { color: '#f687b3', dimBg: 'rgba(246,135,179,0.06)', emoji: '🌸'  },
  8: { color: '#fc8181', dimBg: 'rgba(252,129,129,0.06)', emoji: '🔥'  },
};

// ─── Cooldown hook ────────────────────────────────────────────────────────────
function useCooldown(cooldownEndsAt?: string) {
  const [remaining, setRemaining] = useState('');
  const [ready, setReady] = useState(false);

  useEffect(() => {
    if (!cooldownEndsAt) return;
    const tick = () => {
      const diff = new Date(cooldownEndsAt).getTime() - Date.now();
      if (diff <= 0) { setRemaining(''); setReady(true); return; }
      const h = Math.floor(diff / 3_600_000);
      const m = Math.floor((diff % 3_600_000) / 60_000);
      const s = Math.floor((diff % 60_000) / 1_000);
      setRemaining(
        h > 0
          ? `${h}h ${String(m).padStart(2, '0')}m ${String(s).padStart(2, '0')}s`
          : `${String(m).padStart(2, '0')}m ${String(s).padStart(2, '0')}s`
      );
    };
    tick();
    const id = setInterval(tick, 1000);
    return () => clearInterval(id);
  }, [cooldownEndsAt]);

  return { remaining, ready };
}

// ─── Animated counter ─────────────────────────────────────────────────────────
function AnimatedNumber({ target, suffix = '' }: { target: number; suffix?: string }) {
  const mv = useMotionValue(0);
  const rounded = useTransform(mv, v => Math.round(v) + suffix);

  useEffect(() => {
    const timeout = setTimeout(() => { mv.set(target); }, 300);
    return () => clearTimeout(timeout);
  }, [target, mv]);

  return <motion.span>{rounded}</motion.span>;
}

// ─── Score ring SVG ───────────────────────────────────────────────────────────
function ScoreRing({
  score,
  correct,
  total,
  color,
  isVictory,
}: {
  score: number;
  correct: number;
  total: number;
  color: string;
  isVictory: boolean;
}) {
  const r = 48;
  const circ = 2 * Math.PI * r;
  const [offset, setOffset] = useState(circ);

  useEffect(() => {
    const t = setTimeout(() => setOffset(circ * (1 - score / 100)), 200);
    return () => clearTimeout(t);
  }, [score, circ]);

  const ringColor = isVictory ? color : '#f87171';

  return (
    <div className="relative mx-auto" style={{ width: 120, height: 120 }}>
      <svg width="120" height="120" viewBox="0 0 120 120" className="-rotate-90">
        {/* Track */}
        <circle cx="60" cy="60" r={r} fill="none"
          stroke="rgba(255,255,255,0.06)" strokeWidth="9" />
        {/* Filled arc */}
        <circle cx="60" cy="60" r={r} fill="none"
          stroke={ringColor} strokeWidth="9"
          strokeLinecap="round"
          strokeDasharray={circ}
          strokeDashoffset={offset}
          style={{ transition: 'stroke-dashoffset 1.4s cubic-bezier(.4,0,.2,1)', filter: `drop-shadow(0 0 6px ${ringColor}80)` }}
        />
      </svg>
      {/* Center text */}
      <div className="absolute inset-0 flex flex-col items-center justify-center gap-0.5">
        <span className="font-pixel text-xl leading-none" style={{ color: ringColor }}>
          <AnimatedNumber target={score} suffix="%" />
        </span>
        <span className="font-pixel text-[10px] text-gray-600">
          {correct}/{total}
        </span>
      </div>
    </div>
  );
}

// ─── Confetti burst (victory only) ───────────────────────────────────────────
function ConfettiBurst({ active, color }: { active: boolean; color: string }) {
  const pieces = Array.from({ length: 20 }, (_, i) => ({
    id: i,
    angle: (i / 20) * 360,
    dist: 60 + (i % 4) * 25,
    size: 3 + (i % 3),
    delay: (i % 5) * 0.04,
    col: i % 3 === 0 ? color : i % 3 === 1 ? '#facc15' : '#fff',
  }));

  if (!active) return null;
  return (
    <div className="absolute inset-0 pointer-events-none overflow-hidden">
      {pieces.map(p => {
        const rad = (p.angle * Math.PI) / 180;
        const tx = Math.cos(rad) * p.dist;
        const ty = Math.sin(rad) * p.dist;
        return (
          <motion.div key={p.id}
            className="absolute rounded-sm"
            style={{
              width: p.size, height: p.size * 2,
              background: p.col,
              top: '50%', left: '50%',
              marginTop: -p.size, marginLeft: -p.size / 2,
            }}
            initial={{ x: 0, y: 0, opacity: 1, rotate: 0, scale: 1 }}
            animate={{ x: tx, y: ty, opacity: 0, rotate: 360 + p.angle, scale: 0.3 }}
            transition={{ duration: 0.9, delay: p.delay, ease: 'easeOut' }}
          />
        );
      })}
    </div>
  );
}

// ─── Victory / Defeat headline ────────────────────────────────────────────────
function ResultHeadline({ isVictory, gymLeaderName, color }: { isVictory: boolean; gymLeaderName: string; color: string }) {
  const victoryColor = color;
  const defeatColor = '#f87171';

  return (
    <div className="text-center relative">
      {/* Glow backdrop */}
      <motion.div
        className="absolute inset-0 blur-2xl opacity-20 rounded-full pointer-events-none"
        style={{ background: isVictory ? victoryColor : defeatColor }}
        animate={{ scale: [1, 1.08, 1] }}
        transition={{ duration: 2.4, repeat: Infinity, ease: 'easeInOut' }}
      />

      {/* Icon */}
      <motion.div
        className="text-5xl mb-3 relative z-10"
        initial={{ scale: 0, rotate: -20 }}
        animate={{ scale: 1, rotate: 0 }}
        transition={{ delay: 0.1, type: 'spring', stiffness: 220, damping: 14 }}
      >
        {isVictory ? '🏆' : '💀'}
      </motion.div>

      {/* Title */}
      <motion.h1
        className="font-pixel text-3xl tracking-widest relative z-10 mb-1"
        style={{ color: isVictory ? victoryColor : defeatColor }}
        initial={{ opacity: 0, y: 12 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.25, duration: 0.4 }}
      >
        {isVictory ? 'VICTORY!' : 'DEFEATED'}
      </motion.h1>

      {/* Sub */}
      <motion.p
        className="font-noto text-xs text-gray-500 relative z-10"
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 0.4 }}
      >
        {isVictory
          ? `You conquered ${gymLeaderName}!`
          : `${gymLeaderName} was too strong...`}
      </motion.p>
    </div>
  );
}

// ─── Stat row ─────────────────────────────────────────────────────────────────
function StatRow({ label, value, delay, accent }: { label: string; value: string | number; delay: number; accent?: string }) {
  return (
    <motion.div
      className="flex items-center justify-between py-2 border-b border-white/[.05]"
      initial={{ opacity: 0, x: -8 }}
      animate={{ opacity: 1, x: 0 }}
      transition={{ delay, duration: 0.3 }}
    >
      <span className="font-pixel text-[10px] tracking-widest text-gray-600 uppercase">{label}</span>
      <span className="font-pixel text-[11px]" style={{ color: accent ?? '#fff' }}>{value}</span>
    </motion.div>
  );
}

// ─── Reward pill ──────────────────────────────────────────────────────────────
function RewardPill({ icon, text, color, delay }: { icon: string; text: string; color: string; delay: number }) {
  return (
    <motion.div
      className="flex items-center gap-2 px-4 py-2.5 rounded-xl border"
      style={{ background: `${color}12`, borderColor: `${color}30` }}
      initial={{ opacity: 0, scale: 0.85, y: 6 }}
      animate={{ opacity: 1, scale: 1, y: 0 }}
      transition={{ delay, type: 'spring', stiffness: 200, damping: 16 }}
    >
      <span className="text-base leading-none">{icon}</span>
      <span className="font-pixel text-xs" style={{ color }}>{text}</span>
    </motion.div>
  );
}

// ─── Cooldown block ───────────────────────────────────────────────────────────
function CooldownBlock({ cooldownEndsAt }: { cooldownEndsAt?: string }) {
  const { remaining, ready } = useCooldown(cooldownEndsAt);
  if (!cooldownEndsAt) return null;

  return (
    <motion.div
      className="rounded-xl border border-red-500/20 bg-red-500/[.07] px-4 py-3 text-center"
      initial={{ opacity: 0, y: 8 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: 0.9 }}
    >
      <p className="font-pixel text-[9px] tracking-widest text-red-500 mb-1.5">⏳ COOLDOWN</p>
      {ready ? (
        <p className="font-pixel text-sm text-green-400">READY TO REMATCH!</p>
      ) : (
        <p className="font-pixel text-lg text-red-300 tabular-nums">{remaining}</p>
      )}
    </motion.div>
  );
}

// ─── Answer review row ────────────────────────────────────────────────────────
function ReviewRow({ item, delay }: { item: BattleAnswerResultDto; delay: number }) {
  return (
    <motion.div
      className={`flex items-center gap-3 px-3 py-2.5 rounded-xl border text-xs ${
        item.isCorrect
          ? 'border-green-500/15 bg-green-500/[.05]'
          : 'border-red-500/15 bg-red-500/[.05]'
      }`}
      initial={{ opacity: 0, x: -10 }}
      animate={{ opacity: 1, x: 0 }}
      transition={{ delay, duration: 0.25 }}
    >
      <span className={`font-pixel text-sm flex-shrink-0 ${item.isCorrect ? 'text-green-400' : 'text-red-400'}`}>
        {item.isCorrect ? '✓' : '✗'}
      </span>
      <div className="flex-1 min-w-0">
        <p className="font-pixel text-[11px] text-white truncate">{item.word}</p>
        {item.meaning && (
          <p className="font-noto text-[10px] text-gray-600 truncate mt-0.5">{item.meaning}</p>
        )}
      </div>
      {!item.isCorrect && item.userAnswer && (
        <span className="font-pixel text-[10px] text-red-400/70 flex-shrink-0 truncate max-w-[80px]">
          "{item.userAnswer}"
        </span>
      )}
    </motion.div>
  );
}

// ─── Action button ────────────────────────────────────────────────────────────
function ActionBtn({
  children, onClick, variant = 'ghost', delay = 0, disabled = false,
}: {
  children: React.ReactNode;
  onClick: () => void;
  variant?: 'primary' | 'ghost' | 'dim';
  delay?: number;
  disabled?: boolean;
}) {
  const styles = {
    primary: 'bg-[--ac] text-black border-transparent font-pixel',
    ghost:   'bg-white/[.05] text-white border-white/10 font-pixel hover:bg-white/[.09]',
    dim:     'bg-transparent text-gray-500 border-white/[.06] font-pixel hover:text-gray-300',
  };

  return (
    <motion.button
      onClick={onClick}
      disabled={disabled}
      className={`w-full py-3.5 rounded-xl border text-[11px] tracking-widest uppercase
        transition-all hover:scale-[1.02] active:scale-[.98]
        disabled:opacity-40 disabled:cursor-not-allowed disabled:scale-100
        ${styles[variant]}`}
      initial={{ opacity: 0, y: 8 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay, duration: 0.3 }}
    >
      {children}
    </motion.button>
  );
}

// ─── BattleResult ─────────────────────────────────────────────────────────────
interface BattleResultProps {
  result: BattleResultDto;
  gymLeaderId: number;
}

export default function BattleResult({ result, gymLeaderId }: BattleResultProps) {
  const navigate = useNavigate();
  const theme = GYM_THEME[gymLeaderId] ?? GYM_THEME[1];

  const {
    isVictory, scorePercent, correctAnswers, totalQuestions,
    passRatePercent, bestScore, xpEarned, badgeEarned, badgeName,
    gymLeaderName, answerResults, isOnCooldown, cooldownEndsAt,
  } = result;

  const [showReview, setShowReview] = useState(false);
  const [confettiDone, setConfettiDone] = useState(false);
  const confettiFired = useRef(false);

  // Fire confetti once on victory mount
  useEffect(() => {
    if (isVictory && !confettiFired.current) {
      confettiFired.current = true;
      setTimeout(() => setConfettiDone(true), 1200);
    }
  }, [isVictory]);

  // css var for primary button accent
  const accentStyle = { '--ac': isVictory ? theme.color : '#f87171' } as React.CSSProperties;

  return (
    <div
      className="min-h-screen flex flex-col items-center"
      style={{ background: 'rgb(2,6,23)', ...accentStyle }}
    >
      {/* ── Decorative top glow bar ── */}
      <div
        className="w-full h-[3px] flex-shrink-0"
        style={{
          background: isVictory
            ? `linear-gradient(90deg, transparent, ${theme.color}, transparent)`
            : 'linear-gradient(90deg, transparent, #f87171, transparent)',
        }}
      />

      <div className="w-full max-w-md px-4 py-6 flex flex-col gap-5">

        {/* ── Hero card ── */}
        <motion.div
          className="relative rounded-2xl border overflow-hidden"
          style={{
            borderColor: isVictory ? `${theme.color}30` : 'rgba(248,113,113,.2)',
            background: isVictory ? theme.dimBg : 'rgba(248,113,113,0.05)',
          }}
          initial={{ opacity: 0, y: 24 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5, ease: [0.22, 1, 0.36, 1] }}
        >
          {/* Confetti layer */}
          <div className="relative">
            {isVictory && !confettiDone && (
              <ConfettiBurst active color={theme.color} />
            )}
          </div>

          {/* Inner content */}
          <div className="px-6 pt-7 pb-5 flex flex-col items-center gap-5">
            {/* Headline */}
            <ResultHeadline
              isVictory={isVictory}
              gymLeaderName={gymLeaderName}
              color={theme.color}
            />

            {/* Score ring */}
            <motion.div
              initial={{ scale: 0.6, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              transition={{ delay: 0.35, type: 'spring', stiffness: 160, damping: 14 }}
            >
              <ScoreRing
                score={scorePercent}
                correct={correctAnswers}
                total={totalQuestions}
                color={theme.color}
                isVictory={isVictory}
              />
            </motion.div>

            {/* Stats */}
            <div className="w-full">
              <StatRow label="Pass rate" value={`${passRatePercent}%`} delay={0.5} />
              <StatRow label="Best score" value={`${bestScore}%`}      delay={0.55} />
              <StatRow label="Questions"  value={`${correctAnswers} / ${totalQuestions} correct`} delay={0.6} />
            </div>
          </div>
        </motion.div>

        {/* ── Rewards ── */}
        {isVictory && (
          <div className="flex flex-col gap-2">
            {xpEarned > 0 && (
              <RewardPill icon="⚡" text={`+${xpEarned} XP earned`} color="#facc15" delay={0.7} />
            )}
            {badgeEarned && badgeName && (
              <RewardPill icon="🏅" text={`${badgeName} earned!`} color={theme.color} delay={0.78} />
            )}
          </div>
        )}

        {/* ── Cooldown ── */}
        {!isVictory && isOnCooldown && (
          <CooldownBlock cooldownEndsAt={cooldownEndsAt} />
        )}

        {/* ── Actions ── */}
        <div className="flex flex-col gap-2.5">
          <ActionBtn
            variant="ghost"
            onClick={() => setShowReview(v => !v)}
            delay={1.0}
          >
            {showReview ? '▲ Hide review' : '▼ Review answers'}
          </ActionBtn>

          <ActionBtn
            variant="primary"
            onClick={() => navigate(`/gym/${gymLeaderId}`)}
            delay={1.05}
          >
            ← Back to gym
          </ActionBtn>

          <ActionBtn
            variant="dim"
            onClick={() => navigate('/gym')}
            delay={1.1}
          >
            🗺️ Gym map
          </ActionBtn>
        </div>

        {/* ── Answer review ── */}
        <AnimatePresence>
          {showReview && (
            <motion.div
              className="flex flex-col gap-2 overflow-hidden"
              initial={{ opacity: 0, height: 0 }}
              animate={{ opacity: 1, height: 'auto' }}
              exit={{ opacity: 0, height: 0 }}
              transition={{ duration: 0.35, ease: 'easeInOut' }}
            >
              <p className="font-pixel text-[9px] tracking-widest text-gray-600 uppercase mb-1">
                Answer Review
              </p>

              {/* Summary bar */}
              <motion.div
                className="flex rounded-xl overflow-hidden h-2 mb-1"
                initial={{ scaleX: 0 }}
                animate={{ scaleX: 1 }}
                transition={{ duration: 0.5, ease: 'easeOut' }}
                style={{ transformOrigin: 'left' }}
              >
                <div
                  className="bg-green-500 h-full"
                  style={{ width: `${(correctAnswers / totalQuestions) * 100}%`, transition: 'width .6s ease' }}
                />
                <div className="bg-red-500/40 h-full flex-1" />
              </motion.div>

              {answerResults.map((ar, i) => (
                <ReviewRow key={ar.vocabularyId} item={ar} delay={i * 0.04} />
              ))}
            </motion.div>
          )}
        </AnimatePresence>

        {/* Bottom spacer */}
        <div className="h-2" />
      </div>
    </div>
  );
}