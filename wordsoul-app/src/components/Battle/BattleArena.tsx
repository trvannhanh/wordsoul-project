import { useEffect, useRef, useState, useCallback } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import AnswerScreen from '../LearningSession/AnswerScreen';
import type { BattleQuizQuestion, StartBattleResponseDto } from '../../types/GymTypes';
import type { QuizQuestionDto, QuestionTypeEnum } from '../../types/LearningSessionDto';

// ─── Constants ────────────────────────────────────────────────────────────────
const PLAYER_MAX_HP = 100;
const ENEMY_MAX_HP = 100;
const PLAYER_DAMAGE_PER_WRONG = 25; // each wrong answer costs player 25 HP
const COMBO_THRESHOLDS = [3, 5, 7];

const COMBO_LABELS: Record<number, string> = {
  3: 'COMBO!',
  5: 'ON FIRE!',
  7: 'UNSTOPPABLE!',
};

const GYM_EMOJIS: Record<number, string> = {
  1: '⚔️', 2: '🌿', 3: '🪨', 4: '🌊',
  5: '⚡', 6: '🍃', 7: '🌸', 8: '🔥',
};

const GYM_THEME_COLORS: Record<number, string> = {
  1: '#a8a8a8', 2: '#48bb78', 3: '#90cdf4', 4: '#4299e1',
  5: '#f6e05e', 6: '#68d391', 7: '#f687b3', 8: '#fc8181',
};

// ─── Types ────────────────────────────────────────────────────────────────────
interface HitParticle {
  id: number;
  text: string;
  color: string;
  side: 'player' | 'enemy';
}

interface BattleArenaProps {
  session: StartBattleResponseDto;
  currentIdx: number;
  onAnswer: (
    question: QuizQuestionDto,
    answer: string,
    onProcessed: () => void,
    onResult?: (isCorrect: boolean) => void,
    responseTimeSeconds?: number,
    usedHintCount?: number
  ) => Promise<boolean>;
  onNext: () => void;
  isLastQuestion: boolean;
  isSubmitting: boolean;
  hintBalance?: number;
  setHintBalance?: (v: number) => void;
}

// ─── Adapter: BattleQuizQuestion → QuizQuestionDto ───────────────────────────
function adaptQuestion(q: BattleQuizQuestion): QuizQuestionDto {
  const typeMap: Record<string, number> = {
    Flashcard: 0, FillInBlank: 1, MultipleChoice: 2, Listening: 3,
  };
  return {
    vocabularyId: q.vocabularyId,
    questionType: (typeMap[q.questionType] ?? 2) as QuestionTypeEnum,
    word: q.word ?? '',
    meaning: q.meaning,
    partOfSpeech: q.partOfSpeech,
    cefrLevel: q.cefrLevel,
    pronunciation: q.pronunciation,
    imageUrl: q.imageUrl,
    description: q.description,
    options: q.options,
    pronunciationUrl: q.pronunciationUrl,
    isRetry: q.isRetry,
    questionPrompt: q.questionPrompt,
  };
}

// ─── Sub-components ───────────────────────────────────────────────────────────

/** Dual HP bar row */
function HpBars({
  playerHp, enemyHp, themeColor, enemyName,
}: {
  playerHp: number; enemyHp: number;
  themeColor: string; enemyName: string;
}) {
  const playerPct = Math.max(0, (playerHp / PLAYER_MAX_HP) * 100);
  const enemyPct  = Math.max(0, (enemyHp  / ENEMY_MAX_HP)  * 100);

  const playerBarColor =
    playerPct > 50 ? '#4ade80' : playerPct > 25 ? '#facc15' : '#f87171';

  return (
    <div className="grid grid-cols-2 gap-3 px-3 pt-3">
      {/* Player */}
      <div className="bg-white/[.04] border border-white/[.07] rounded-xl px-3 py-2.5 border-l-2"
        style={{ borderLeftColor: '#4ade80' }}>
        <p className="font-pixel text-[9px] tracking-widest text-gray-400 uppercase mb-1">You</p>
        <div className="h-2 bg-white/[.07] rounded-full overflow-hidden mb-1">
          <motion.div
            className="h-full rounded-full"
            style={{ background: playerBarColor }}
            animate={{ width: `${playerPct}%` }}
            transition={{ duration: 0.5, ease: 'easeOut' }}
          />
        </div>
        <p className="font-pixel text-[9px] text-gray-500">
          {Math.round(Math.max(0, playerHp))} / {PLAYER_MAX_HP}
        </p>
      </div>

      {/* Enemy */}
      <div className="bg-white/[.04] border border-white/[.07] rounded-xl px-3 py-2.5 border-r-2 text-right"
        style={{ borderRightColor: themeColor }}>
        <p className="font-pixel text-[9px] tracking-widest text-gray-400 uppercase mb-1 truncate">
          {enemyName}
        </p>
        <div className="h-2 bg-white/[.07] rounded-full overflow-hidden mb-1">
          <motion.div
            className="h-full rounded-full ml-auto"
            style={{ background: themeColor }}
            animate={{ width: `${enemyPct}%` }}
            transition={{ duration: 0.5, ease: 'easeOut' }}
          />
        </div>
        <p className="font-pixel text-[9px] text-gray-500">
          {Math.round(Math.max(0, enemyHp))} / {ENEMY_MAX_HP}
        </p>
      </div>
    </div>
  );
}

/** Sprite zone with hit particles */
function SpriteZone({
  gymLeaderId, themeColor, gymLeaderName,
  playerShaking, enemyShaking, particles,
}: {
  gymLeaderId: number; themeColor: string; gymLeaderName: string;
  playerShaking: boolean; enemyShaking: boolean;
  particles: HitParticle[];
}) {
  const leaderEmoji = GYM_EMOJIS[gymLeaderId] ?? '🧙';

  return (
    <div className="relative flex items-center justify-between px-6 py-1 min-h-[80px]">
      {/* Player sprite */}
      <motion.div
        className="flex flex-col items-center gap-1"
        animate={playerShaking ? { x: [-8, 8, -6, 6, 0] } : { x: 0 }}
        transition={{ duration: 0.35 }}
      >
        <span className="text-4xl leading-none select-none">🧑‍💻</span>
        <span className="font-pixel text-[8px] text-gray-600 tracking-widest uppercase">trainer</span>
      </motion.div>

      {/* VS */}
      <span className="font-pixel text-[11px] text-white/10 tracking-widest select-none">VS</span>

      {/* Enemy sprite */}
      <motion.div
        className="flex flex-col items-center gap-1"
        animate={enemyShaking ? { x: [-8, 8, -6, 6, 0] } : { x: 0 }}
        transition={{ duration: 0.35 }}
      >
        <span className="text-4xl leading-none select-none">{leaderEmoji}</span>
        <span className="font-pixel text-[8px] tracking-widest uppercase truncate max-w-[80px]"
          style={{ color: themeColor }}>
          {gymLeaderName}
        </span>
      </motion.div>

      {/* Hit particles */}
      <AnimatePresence>
        {particles.map(p => (
          <motion.div
            key={p.id}
            className="absolute pointer-events-none font-pixel text-xs font-bold"
            style={{
              color: p.color,
              left: p.side === 'enemy' ? '72%' : '10%',
              top: '8px',
              zIndex: 20,
            }}
            initial={{ y: 0, opacity: 1, scale: 0.7 }}
            animate={{ y: -44, opacity: 0, scale: 1.2 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.75, ease: 'easeOut' }}
          >
            {p.text}
          </motion.div>
        ))}
      </AnimatePresence>
    </div>
  );
}



/** Streak badge */
function StreakBadge({ streak }: { streak: number }) {
  if (streak < 2) return null;
  return (
    <motion.div
      key={streak}
      className="flex items-center justify-center"
      initial={{ scale: 0.4, opacity: 0 }}
      animate={{ scale: 1, opacity: 1 }}
      transition={{ type: 'spring', stiffness: 300, damping: 18 }}
    >
      <span className="font-pixel text-[10px] tracking-widest px-3 py-1 rounded-full border"
        style={{
          background: 'rgba(251,146,60,.12)',
          borderColor: 'rgba(251,146,60,.3)',
          color: '#fb923c',
        }}>
        🔥 STREAK ×{streak}
      </span>
    </motion.div>
  );
}

/** Combo flash overlay */
function ComboOverlay({ label, visible }: { label: string; visible: boolean }) {
  return (
    <AnimatePresence>
      {visible && (
        <motion.div
          className="fixed inset-0 z-50 flex items-center justify-center pointer-events-none"
          style={{ backdropFilter: 'blur(2px)' }}
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.2 }}
        >
          <motion.p
            className="font-pixel text-3xl tracking-widest"
            style={{ color: '#facc15', textShadow: '0 0 20px rgba(250,204,21,.5)' }}
            initial={{ scale: 0.4, rotate: -8, opacity: 0 }}
            animate={{ scale: [0.4, 1.3, 1], rotate: [-8, 3, 0], opacity: [0, 1, 1] }}
            exit={{ scale: 0.8, opacity: 0 }}
            transition={{ duration: 0.55, times: [0, 0.5, 1] }}
          >
            {label}
          </motion.p>
        </motion.div>
      )}
    </AnimatePresence>
  );
}

// ─── Main BattleArena ─────────────────────────────────────────────────────────
export default function BattleArena({
  session,
  currentIdx,
  onAnswer,
  onNext,
  isLastQuestion,
  isSubmitting,
  hintBalance,
  setHintBalance,
}: BattleArenaProps) {
  const { gymLeaderId, gymLeaderName, gymLeaderTitle, questions } = session;

  const themeColor = GYM_THEME_COLORS[gymLeaderId] ?? '#f6e05e';
  const totalQ = questions.length;

  // ── State ──────────────────────────────────────────────────────────
  const [playerHp, setPlayerHp] = useState(PLAYER_MAX_HP);
  const [enemyHp,  setEnemyHp]  = useState(ENEMY_MAX_HP);
  const [streak, setStreak]       = useState(0);
  const [playerShaking, setPlayerShaking] = useState(false);
  const [enemyShaking,  setEnemyShaking]  = useState(false);
  const [particles, setParticles] = useState<HitParticle[]>([]);
  const [comboLabel,  setComboLabel]  = useState('');
  const [comboVisible, setComboVisible] = useState(false);
  const [confirmed, setConfirmed] = useState(false);

  const particleIdRef = useRef(0);
  const dmgPerRight = ENEMY_MAX_HP / totalQ;

  // ── Reset confirmed state on question change ───────────────────────
  useEffect(() => {
    setConfirmed(false);
  }, [currentIdx]);

  // ── Spawn a hit particle ───────────────────────────────────────────
  const spawnParticle = useCallback((side: 'player' | 'enemy', text: string, color: string) => {
    const id = ++particleIdRef.current;
    setParticles(prev => [...prev, { id, text, color, side }]);
    setTimeout(() => setParticles(prev => prev.filter(p => p.id !== id)), 800);
  }, []);

  // ── Shake helper ──────────────────────────────────────────────────
  const shake = useCallback((side: 'player' | 'enemy') => {
    if (side === 'player') {
      setPlayerShaking(true);
      setTimeout(() => setPlayerShaking(false), 400);
    } else {
      setEnemyShaking(true);
      setTimeout(() => setEnemyShaking(false), 400);
    }
  }, []);

  // ── Handle answer result ──────────────────────────────────────────
  const handleAnswerResult = useCallback((isCorrect: boolean) => {
    setConfirmed(true);

    if (isCorrect) {
      // Damage enemy
      setEnemyHp(prev => Math.max(0, prev - dmgPerRight));
      shake('enemy');
      spawnParticle('enemy', `⚡ -${Math.round(dmgPerRight)}`, '#4ade80');

      // Streak
      setStreak(prev => {
        const next = prev + 1;
        if (COMBO_THRESHOLDS.includes(next)) {
          setComboLabel(COMBO_LABELS[next] ?? 'COMBO!');
          setComboVisible(true);
          setTimeout(() => setComboVisible(false), 800);
        }
        return next;
      });
    } else {
      // Damage player
      setPlayerHp(prev => Math.max(0, prev - PLAYER_DAMAGE_PER_WRONG));
      shake('player');
      spawnParticle('player', `💥 -${PLAYER_DAMAGE_PER_WRONG}`, '#f87171');
      setStreak(0);
    }
  }, [dmgPerRight, shake, spawnParticle]);

  // ── Wrapped onAnswer to hook result ──────────────────────────────
  const wrappedOnAnswer = useCallback(async (
    question: QuizQuestionDto,
    answer: string,
    onProcessed: () => void,
    onResult?: (isCorrect: boolean) => void,
    responseTimeSeconds?: number,
    usedHintCount?: number,
  ): Promise<boolean> => {
    return onAnswer(
      question,
      answer,
      onProcessed,
      (isCorrect) => {
        handleAnswerResult(isCorrect);
        onResult?.(isCorrect);
      },
      responseTimeSeconds,
      usedHintCount,
    );
  }, [onAnswer, handleAnswerResult]);

  // ── Current question adapted ──────────────────────────────────────
  const rawQuestion = questions[currentIdx];
  const adaptedQuestion: QuizQuestionDto | null = rawQuestion ? adaptQuestion(rawQuestion) : null;

  return (
    <>
      {/* Combo overlay — fixed, above everything */}
      <ComboOverlay label={comboLabel} visible={comboVisible} />

      {/* ── Main battle screen ── */}
      <div
        className="min-h-screen flex flex-col"
        style={{ background: 'rgb(2,6,23)' }}
      >
        {/* Top: progress */}
        <div
          className="px-4 pt-3 pb-2 flex items-center justify-between border-b border-white/[.05]"
          style={{ background: 'rgba(255,255,255,.02)' }}
        >
          <div>
            <p className="font-pixel text-[10px] tracking-widest uppercase"
              style={{ color: themeColor }}>
              VS {gymLeaderName.toUpperCase()}
            </p>
            <p className="font-noto text-[10px] text-gray-600 mt-0.5">{gymLeaderTitle}</p>
          </div>
          <div className="text-right">
            <p className="font-pixel text-[10px] text-gray-500">
              Pass: {session.passRatePercent}%
            </p>
          </div>
        </div>

        {/* Thin global progress line */}
        <div className="h-[2px] bg-white/[.04]">
          <motion.div
            className="h-full"
            style={{ background: themeColor, boxShadow: `0 0 6px ${themeColor}` }}
            animate={{ width: `${((currentIdx) / totalQ) * 100}%` }}
            transition={{ duration: 0.4 }}
          />
        </div>

        {/* HP bars */}
        <HpBars
          playerHp={playerHp}
          enemyHp={enemyHp}
          themeColor={themeColor}
          enemyName={gymLeaderName}
        />

        {/* Sprite zone */}
        <SpriteZone
          gymLeaderId={gymLeaderId}
          themeColor={themeColor}
          gymLeaderName={gymLeaderName}
          playerShaking={playerShaking}
          enemyShaking={enemyShaking}
          particles={particles}
        />

        {/* Streak badge */}
        <div className="px-4 min-h-[28px] flex items-center justify-center">
          <StreakBadge streak={streak} />
        </div>

        {/* ── Question card ── */}
        <div className="flex-1 flex flex-col px-4 pb-4 gap-3 min-h-0">
          <div
            className="rounded-2xl border flex-1 flex flex-col overflow-hidden"
            style={{
              borderColor: `${themeColor}33`,
              background: `${themeColor}08`,
              boxShadow: `0 0 24px ${themeColor}18`,
            }}
          >
            {/* Question prompt */}
            <div className="px-5 pt-5 pb-3 border-b border-white/[.05]">
              <div className="flex items-center justify-between mb-3">
                <span
                  className="font-pixel text-[9px] tracking-widest uppercase px-2.5 py-1 rounded-full border"
                  style={{
                    background: `${themeColor}18`,
                    borderColor: `${themeColor}44`,
                    color: themeColor,
                  }}
                >
                  {rawQuestion?.questionType === 'MultipleChoice' ? 'Choose the word'
                    : rawQuestion?.questionType === 'FillInBlank' ? 'Fill in the blank'
                    : rawQuestion?.questionType === 'Listening' ? 'Listen & type'
                    : 'Flashcard'}
                </span>
                <span className="font-pixel text-[9px] text-gray-600">
                  {currentIdx + 1} / {totalQ}
                </span>
              </div>

              {rawQuestion?.imageUrl && (
                <img
                  src={rawQuestion.imageUrl}
                  alt=""
                  className="w-16 h-16 mx-auto mb-3 rounded-lg object-cover"
                />
              )}

              <p className="font-pixel text-sm leading-relaxed text-white text-center">
                {rawQuestion?.questionPrompt ?? rawQuestion?.meaning ?? rawQuestion?.word}
              </p>
            </div>

            {/* Answer zone — reuse AnswerScreen */}
            <div className="flex-1 flex items-center justify-center overflow-hidden">
              {adaptedQuestion && (
                <AnswerScreen
                  question={adaptedQuestion}
                  loading={false}
                  error={null}
                  handleAnswer={wrappedOnAnswer}
                  loadNextQuestion={() => {}} // controlled by onNext button
                  showPopup={() => {}}        // no popup in battle mode
                  hintBalance={hintBalance}
                  setHintBalance={setHintBalance}
                />
              )}
            </div>

            {/* Confirmed: mini vocab info */}
            <AnimatePresence>
              {confirmed && (
                <motion.div
                  className="px-4 pb-3 text-center border-t border-white/[.05] pt-2"
                  initial={{ opacity: 0, y: 6 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0 }}
                >
                  {rawQuestion?.meaning && (
                    <p className="font-noto text-[11px] text-gray-500">{rawQuestion.meaning}</p>
                  )}
                  {rawQuestion?.partOfSpeech && (
                    <p className="font-noto text-[10px] text-gray-700 italic mt-0.5">
                      {rawQuestion.partOfSpeech}
                    </p>
                  )}
                </motion.div>
              )}
            </AnimatePresence>
          </div>

          {/* Next / Finish button — only shown after confirming */}
          <AnimatePresence>
            {confirmed && (
              <motion.button
                onClick={onNext}
                disabled={isSubmitting}
                className="w-full py-3.5 rounded-xl font-pixel text-sm tracking-widest uppercase transition-all hover:scale-[1.02] active:scale-[.98] border border-white/10 disabled:opacity-50"
                style={{ background: 'rgba(255,255,255,.07)', color: '#fff' }}
                initial={{ opacity: 0, y: 8 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0 }}
                transition={{ duration: 0.2 }}
              >
                {isSubmitting
                  ? 'Submitting...'
                  : isLastQuestion
                  ? 'FINISH BATTLE ⚔️'
                  : 'NEXT →'}
              </motion.button>
            )}
          </AnimatePresence>
        </div>
      </div>
    </>
  );
}