import { useEffect, useRef, useState, useCallback } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import type { StartBattleResponseDto } from '../../types/GymTypes';

// ─── Theme ────────────────────────────────────────────────────────────────────
const GYM_THEME: Record<number, { color: string; bg: string; emoji: string; scanColor: string }> = {
  1: { color: '#c0c0c0', bg: '#0d0d0d',  emoji: '⚔️',  scanColor: 'rgba(192,192,192,' },
  2: { color: '#48bb78', bg: '#021a0b',  emoji: '🌿',  scanColor: 'rgba(72,187,120,'  },
  3: { color: '#90cdf4', bg: '#011829',  emoji: '🪨',  scanColor: 'rgba(144,205,244,' },
  4: { color: '#4299e1', bg: '#01122a',  emoji: '🌊',  scanColor: 'rgba(66,153,225,'  },
  5: { color: '#f6e05e', bg: '#1a1600',  emoji: '⚡',  scanColor: 'rgba(246,224,94,'  },
  6: { color: '#68d391', bg: '#021a09',  emoji: '🍃',  scanColor: 'rgba(104,211,145,' },
  7: { color: '#f687b3', bg: '#1a0011',  emoji: '🌸',  scanColor: 'rgba(246,135,179,' },
  8: { color: '#fc8181', bg: '#1a0000',  emoji: '🔥',  scanColor: 'rgba(252,129,129,' },
};

// ─── Typewriter hook ──────────────────────────────────────────────────────────
function useTypewriter(text: string, speed = 40, startDelay = 0) {
  const [displayed, setDisplayed] = useState('');
  const [done, setDone] = useState(false);

  useEffect(() => {
    setDisplayed('');
    setDone(false);
    let i = 0;
    const start = setTimeout(() => {
      const id = setInterval(() => {
        i++;
        setDisplayed(text.slice(0, i));
        if (i >= text.length) {
          clearInterval(id);
          setDone(true);
        }
      }, speed);
      return () => clearInterval(id);
    }, startDelay);
    return () => clearTimeout(start);
  }, [text, speed, startDelay]);

  return { displayed, done };
}

// ─── Scanline canvas overlay ──────────────────────────────────────────────────
function ScanLines({ color }: { color: string }) {
  return (
    <div
      className="absolute inset-0 pointer-events-none z-10"
      style={{
        backgroundImage: `repeating-linear-gradient(
          0deg,
          transparent,
          transparent 3px,
          ${color}0.04) 3px,
          ${color}0.04) 4px
        )`,
        backgroundSize: '100% 4px',
      }}
    />
  );
}

// ─── Electric sparks that shoot across the divider ───────────────────────────
function ElectricDivider({ color }: { color: string }) {
  const sparks = Array.from({ length: 6 }, (_, i) => i);
  return (
    <div className="relative flex items-center justify-center w-full h-[2px] overflow-visible">
      {/* Base line */}
      <motion.div
        className="absolute inset-0"
        style={{ background: `linear-gradient(90deg, transparent, ${color}, transparent)` }}
        initial={{ scaleX: 0, opacity: 0 }}
        animate={{ scaleX: 1, opacity: 1 }}
        transition={{ duration: 0.4, ease: 'easeOut', delay: 0.6 }}
      />
      {/* Sparks */}
      {sparks.map(i => (
        <motion.div
          key={i}
          className="absolute w-1 h-1 rounded-full pointer-events-none"
          style={{ background: color, top: '-1px' }}
          initial={{ left: '-2%', opacity: 0, scale: 0 }}
          animate={{
            left: ['−2%', '102%'],
            opacity: [0, 1, 1, 0],
            scale: [0, 1.5, 1, 0],
          }}
          transition={{
            duration: 0.6,
            delay: 0.8 + i * 0.08,
            ease: 'easeInOut',
          }}
        />
      ))}
    </div>
  );
}

// ─── One trainer card (player or leader) ─────────────────────────────────────
function TrainerCard({
  side,
  name,
  title,
  emoji,
  color,
  delay,
}: {
  side: 'left' | 'right';
  name: string;
  title: string;
  emoji: string;
  color: string;
  delay: number;
}) {
  const isLeft = side === 'left';

  return (
    <motion.div
      className={`flex-1 flex flex-col ${isLeft ? 'items-start' : 'items-end'} justify-end pb-10 px-6 relative`}
      initial={{ x: isLeft ? -80 : 80, opacity: 0 }}
      animate={{ x: 0, opacity: 1 }}
      transition={{ duration: 0.55, delay, ease: [0.22, 1, 0.36, 1] }}
    >
      {/* Spotlight cone behind sprite */}
      <motion.div
        className="absolute bottom-0 pointer-events-none"
        style={{
          width: '200px',
          height: '260px',
          left: isLeft ? '0' : 'auto',
          right: isLeft ? 'auto' : '0',
          background: `radial-gradient(ellipse at ${isLeft ? '30%' : '70%'} 100%, ${color}20 0%, transparent 70%)`,
        }}
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: delay + 0.2, duration: 0.6 }}
      />

      {/* Sprite */}
      <motion.div
        className="text-[72px] leading-none select-none mb-4 relative z-10"
        initial={{ y: 20, scale: 0.7, opacity: 0 }}
        animate={{ y: 0, scale: 1, opacity: 1 }}
        transition={{ delay: delay + 0.1, duration: 0.5, type: 'spring', stiffness: 200 }}
        style={{ filter: `drop-shadow(0 0 16px ${color}60)` }}
      >
        {emoji}
      </motion.div>

      {/* Name plate */}
      <motion.div
        className={`relative z-10 ${isLeft ? '' : 'text-right'}`}
        initial={{ opacity: 0, y: 8 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: delay + 0.3, duration: 0.35 }}
      >
        {/* Accent bar */}
        <motion.div
          className={`h-[2px] w-12 mb-2 ${isLeft ? '' : 'ml-auto'}`}
          style={{ background: color }}
          initial={{ scaleX: 0 }}
          animate={{ scaleX: 1 }}
          transition={{ delay: delay + 0.35, duration: 0.3, ease: 'easeOut' }}
        />
        <p
          className="font-pixel text-[10px] tracking-[3px] uppercase mb-1"
          style={{ color }}
        >
          {isLeft ? 'Trainer' : 'Gym Leader'}
        </p>
        <p className="font-pixel text-xl text-white tracking-wide leading-tight">{name}</p>
        {title && (
          <p className="font-noto text-[11px] text-gray-500 mt-1 italic">{title}</p>
        )}
      </motion.div>
    </motion.div>
  );
}

// ─── Central VS badge ─────────────────────────────────────────────────────────
function VsBadge({ color, delay }: { color: string; delay: number }) {
  return (
    <motion.div
      className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 z-30 flex flex-col items-center gap-2"
      initial={{ scale: 0, rotate: -20, opacity: 0 }}
      animate={{ scale: 1, rotate: 0, opacity: 1 }}
      transition={{
        delay,
        duration: 0.6,
        type: 'spring',
        stiffness: 220,
        damping: 14,
      }}
    >
      {/* Outer ring pulse */}
      <motion.div
        className="absolute rounded-full border-2 pointer-events-none"
        style={{ width: 72, height: 72, borderColor: `${color}60` }}
        animate={{ scale: [1, 1.6, 1], opacity: [0.6, 0, 0.6] }}
        transition={{ duration: 1.8, repeat: Infinity, ease: 'easeOut' }}
      />
      {/* Badge circle */}
      <div
        className="w-14 h-14 rounded-full flex items-center justify-center border-2 relative"
        style={{
          background: `radial-gradient(circle, ${color}22 0%, transparent 70%)`,
          borderColor: `${color}80`,
          boxShadow: `0 0 20px ${color}40`,
        }}
      >
        <span
          className="font-pixel text-lg font-black tracking-widest"
          style={{ color }}
        >
          VS
        </span>
      </div>
    </motion.div>
  );
}

// ─── Dialog box (bottom) ──────────────────────────────────────────────────────
function DialogBox({
  text,
  color,
  delay,
  onSkip,
}: {
  text: string;
  color: string;
  delay: number;
  onSkip: () => void;
}) {
  const { displayed, done } = useTypewriter(text, 38, delay * 1000);

  return (
    <motion.div
      className="mx-4 mb-4 rounded-xl border px-5 py-4 relative cursor-pointer select-none"
      style={{
        background: 'rgba(0,0,0,.85)',
        borderColor: `${color}44`,
        backdropFilter: 'blur(8px)',
      }}
      initial={{ opacity: 0, y: 16 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay, duration: 0.4, ease: 'easeOut' }}
      onClick={onSkip}
    >
      {/* Corner accent */}
      <div
        className="absolute top-0 left-0 w-3 h-3 border-t-2 border-l-2 rounded-tl-xl"
        style={{ borderColor: color }}
      />
      <div
        className="absolute bottom-0 right-0 w-3 h-3 border-b-2 border-r-2 rounded-br-xl"
        style={{ borderColor: color }}
      />

      <p className="font-pixel text-[13px] text-white leading-relaxed min-h-[20px]">
        {displayed}
        {/* Blinking cursor */}
        {!done && (
          <motion.span
            className="inline-block ml-0.5 w-[7px] h-[13px] align-middle"
            style={{ background: color }}
            animate={{ opacity: [1, 0, 1] }}
            transition={{ duration: 0.7, repeat: Infinity }}
          />
        )}
      </p>

      {/* Skip hint */}
      {done && (
        <motion.p
          className="font-pixel text-[9px] text-right mt-2 tracking-widest"
          style={{ color: `${color}80` }}
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ duration: 0.3 }}
        >
          TAP TO CONTINUE ▶
        </motion.p>
      )}
    </motion.div>
  );
}

// ─── Flash transition out ─────────────────────────────────────────────────────
function FlashOut({ active, color }: { active: boolean; color: string }) {
  return (
    <AnimatePresence>
      {active && (
        <motion.div
          className="fixed inset-0 z-50 pointer-events-none"
          style={{ background: color }}
          initial={{ opacity: 0 }}
          animate={{ opacity: [0, 1, 1, 0] }}
          transition={{ duration: 0.5, times: [0, 0.2, 0.7, 1] }}
        />
      )}
    </AnimatePresence>
  );
}

// ─── Floating particles ───────────────────────────────────────────────────────
function AmbientParticles({ color }: { color: string }) {
  const particles = Array.from({ length: 12 }, (_, i) => ({
    id: i,
    x: 5 + ((i * 37 + 13) % 90),
    delay: (i * 0.3) % 2.4,
    duration: 3 + (i % 4) * 0.7,
    size: 1 + (i % 3),
  }));

  return (
    <div className="absolute inset-0 pointer-events-none overflow-hidden z-0">
      {particles.map(p => (
        <motion.div
          key={p.id}
          className="absolute rounded-full"
          style={{
            width: p.size,
            height: p.size,
            background: color,
            left: `${p.x}%`,
            bottom: '-4px',
          }}
          animate={{
            y: [0, -300 - p.size * 40],
            opacity: [0, 0.6, 0.6, 0],
          }}
          transition={{
            duration: p.duration,
            delay: p.delay,
            repeat: Infinity,
            ease: 'easeOut',
          }}
        />
      ))}
    </div>
  );
}

// ─── BattleIntro ─────────────────────────────────────────────────────────────
interface BattleIntroProps {
  session: StartBattleResponseDto;
  onComplete: () => void;
}

type Phase = 'enter' | 'dialog' | 'flash' | 'done';

export default function BattleIntro({ session, onComplete }: BattleIntroProps) {
  const { gymLeaderId, gymLeaderName, gymLeaderTitle } = session;
  const theme = GYM_THEME[gymLeaderId] ?? GYM_THEME[1];

  const [phase, setPhase] = useState<Phase>('enter');
  const completedRef = useRef(false);

  const dialogText = `${gymLeaderName}: "So you've come this far... Let's see if your vocabulary can match your courage!"`;

  // Auto-advance: enter → dialog after 1.4s
  useEffect(() => {
    const t = setTimeout(() => setPhase('dialog'), 1400);
    return () => clearTimeout(t);
  }, []);

  // Memoized function to handle flash transition
  const triggerFlash = useCallback(() => {
    if (completedRef.current) return;
    completedRef.current = true;
    setPhase('flash');
    setTimeout(onComplete, 500);
  }, [onComplete]);

  // Auto-advance: dialog → flash after 5.5s (typewriter + read time)
  useEffect(() => {
    if (phase !== 'dialog') return;
    const t = setTimeout(() => triggerFlash(), 5500);
    return () => clearTimeout(t);
  }, [phase, triggerFlash]);

  function handleSkip() {
    triggerFlash();
  }

  return (
    <div
      className="fixed inset-0 z-40 flex flex-col overflow-hidden"
      style={{ background: theme.bg }}
      onClick={phase === 'dialog' ? handleSkip : undefined}
    >
      {/* Scan lines */}
      <ScanLines color={theme.scanColor} />

      {/* Ambient particles */}
      <AmbientParticles color={theme.color} />

      {/* Top bar: gym info */}
      <motion.div
        className="relative z-20 px-5 pt-5 pb-2 flex items-center gap-3"
        initial={{ opacity: 0, y: -12 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.4, ease: 'easeOut' }}
      >
        <div
          className="w-1 h-8 rounded-full flex-shrink-0"
          style={{ background: theme.color }}
        />
        <div>
          <p
            className="font-pixel text-[9px] tracking-[3px] uppercase"
            style={{ color: theme.color }}
          >
            Gym Battle
          </p>
          <p className="font-pixel text-[11px] text-white mt-0.5">
            {gymLeaderTitle}
          </p>
        </div>
        {/* Pass rate badge */}
        <div
          className="ml-auto font-pixel text-[9px] tracking-widest px-3 py-1 rounded-full border"
          style={{
            borderColor: `${theme.color}44`,
            background: `${theme.color}10`,
            color: theme.color,
          }}
        >
          PASS {session.passRatePercent}%
        </div>
      </motion.div>

      {/* ── Arena: two trainers + VS ── */}
      <div className="relative z-10 flex-1 flex">
        {/* Left: player */}
        <TrainerCard
          side="left"
          name="You"
          title="Challenger"
          emoji="🧑‍💻"
          color="#4ade80"
          delay={0.25}
        />

        {/* Vertical divider line */}
        <div className="absolute inset-y-0 left-1/2 -translate-x-px w-[1px] flex flex-col justify-center">
          <motion.div
            className="flex-1"
            style={{ background: `linear-gradient(to bottom, transparent, ${theme.color}30, transparent)` }}
            initial={{ scaleY: 0, opacity: 0 }}
            animate={{ scaleY: 1, opacity: 1 }}
            transition={{ delay: 0.5, duration: 0.5 }}
          />
        </div>

        {/* VS badge center */}
        <VsBadge color={theme.color} delay={0.7} />

        {/* Right: gym leader */}
        <TrainerCard
          side="right"
          name={gymLeaderName}
          title={gymLeaderTitle}
          emoji={theme.emoji}
          color={theme.color}
          delay={0.4}
        />
      </div>

      {/* Electric divider */}
      <div className="relative z-20 px-4 my-1">
        <ElectricDivider color={theme.color} />
      </div>

      {/* ── Dialog box ── */}
      <div className="relative z-20">
        <AnimatePresence>
          {phase === 'dialog' && (
            <DialogBox
              text={dialogText}
              color={theme.color}
              delay={0.1}
              onSkip={handleSkip}
            />
          )}
        </AnimatePresence>

        {/* Placeholder height when dialog not yet shown */}
        {phase === 'enter' && <div className="h-[80px]" />}
      </div>

      {/* Skip hint while in enter phase */}
      <AnimatePresence>
        {phase === 'enter' && (
          <motion.p
            className="font-pixel text-[9px] text-center pb-4 tracking-widest relative z-20"
            style={{ color: `${theme.color}40` }}
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ delay: 0.8 }}
          >
            TAP ANYWHERE TO SKIP
          </motion.p>
        )}
      </AnimatePresence>

      {/* Flash-out overlay */}
      <FlashOut active={phase === 'flash'} color={theme.color} />
    </div>
  );
}