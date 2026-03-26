import { useCallback, useEffect, useRef, useState } from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';



import { submitBattle } from '../../services/gym';

import type {
  StartBattleResponseDto,
  BattleAnswerDto,
  BattleResultDto,
} from '../../types/GymTypes';
import type { QuizQuestionDto } from '../../types/LearningSessionDto';
import BattleIntro from '../../components/Battle/BattleIntro';
import BattleArena from '../../components/Battle/BattleArena';
import BattleResult from '../../components/Battle/BattleResult';

// ─── Location state ───────────────────────────────────────────────────────────
interface LocationState {
  session: StartBattleResponseDto;
}

// ─── Phase machine ────────────────────────────────────────────────────────────
//  intro  ──▶  battle  ──▶  submitting  ──▶  result
//                │
//             (error)  ──▶  /gym  (redirect)
type Phase = 'intro' | 'battle' | 'submitting' | 'result';

// ─── Transition overlay between phases ───────────────────────────────────────
function PhaseTransition({ visible, color }: { visible: boolean; color: string }) {
  return (
    <AnimatePresence>
      {visible && (
        <motion.div
          className="fixed inset-0 z-[60] pointer-events-none"
          style={{ background: color }}
          initial={{ opacity: 0 }}
          animate={{ opacity: [0, 0.7, 0.7, 0] }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.55, times: [0, 0.25, 0.65, 1] }}
        />
      )}
    </AnimatePresence>
  );
}

// ─── Submitting screen ────────────────────────────────────────────────────────
function SubmittingScreen({ color }: { color: string }) {
  return (
    <div
      className="fixed inset-0 z-50 flex flex-col items-center justify-center gap-5"
      style={{ background: 'rgb(2,6,23)' }}
    >
      {/* Spinner ring */}
      <div className="relative w-16 h-16">
        <motion.div
          className="absolute inset-0 rounded-full border-2 border-t-transparent"
          style={{ borderColor: `${color}40`, borderTopColor: color }}
          animate={{ rotate: 360 }}
          transition={{ duration: 0.9, repeat: Infinity, ease: 'linear' }}
        />
      </div>
      <p className="font-pixel text-[11px] tracking-widest text-gray-500 uppercase">
        Calculating results...
      </p>
    </div>
  );
}

// ─── Error screen ─────────────────────────────────────────────────────────────
function ErrorScreen({ onBack }: { onBack: () => void }) {
  return (
    <div
      className="min-h-screen flex flex-col items-center justify-center gap-6 px-6"
      style={{ background: 'rgb(2,6,23)' }}
    >
      <span className="text-5xl">⚠️</span>
      <p className="font-pixel text-sm text-red-400 text-center leading-relaxed">
        Something went wrong<br />submitting your battle.
      </p>
      <button
        onClick={onBack}
        className="font-pixel text-[11px] tracking-widest uppercase px-6 py-3 rounded-xl
          border border-white/10 text-gray-400 hover:text-white hover:bg-white/[.06]
          transition-all"
      >
        ← Back to gym
      </button>
    </div>
  );
}

// ─── GymBattle ────────────────────────────────────────────────────────────────
const GYM_COLORS: Record<number, string> = {
  1: '#a0c8f0', 2: '#a8b820', 3: '#a8a878', 4: '#705898',
  5: '#c03028', 6: '#b8b8d0', 7: '#98d8d8', 8: '#7038f8',
  9: '#b8a038', 10: '#6890f0', 11: '#f8d030', 12: '#78c850',
  13: '#a040a0', 14: '#f85888', 15: '#f08030', 16: '#f85888',
};

export default function GymBattle() {
  const { sessionId } = useParams<{ sessionId: string }>();
  const location      = useLocation();
  const navigate      = useNavigate();

  const state   = (location.state as LocationState | undefined);
  const session = state?.session;

  // ── Guard: no session → back to gym ──────────────────────────────
  useEffect(() => {
    if (!session) navigate('/gym', { replace: true });
  }, [session, navigate]);

  // ── Derived constants ─────────────────────────────────────────────
  const gymLeaderId = session?.gymLeaderId ?? 1;
  const themeColor  = GYM_COLORS[gymLeaderId] ?? '#f6e05e';
  const questions   = session?.questions ?? [];
  const totalQ      = questions.length;

  // ── Phase & question state ─────────────────────────────────────────
  const [phase,      setPhase]      = useState<Phase>('intro');
  const [currentIdx, setCurrentIdx] = useState(0);
  const [answers,    setAnswers]     = useState<BattleAnswerDto[]>([]);
  const [result,     setResult]      = useState<BattleResultDto | null>(null);
  const [hasError,   setHasError]    = useState(false);

  // Transition flash between phases
  const [transitioning, setTransitioning] = useState(false);

  // Per-question start time (ms) tracked in a ref to avoid stale closures
  const questionStartRef = useRef<number>(Date.now());

  useEffect(() => {
    questionStartRef.current = Date.now();
  }, [currentIdx]);

  // ── Intro → battle ────────────────────────────────────────────────
  const handleIntroComplete = useCallback(() => {
    setTransitioning(true);
    setTimeout(() => {
      setPhase('battle');
      setTransitioning(false);
    }, 280);
  }, []);

  // ── Answer handler (called by BattleArena → AnswerScreen) ─────────
  //
  //   In the original GymBattle, answers were accumulated locally and
  //   submitted in bulk at the end.  We keep the same pattern here:
  //   this callback *records* the answer into local state and returns
  //   a mock isCorrect so AnswerScreen can show feedback immediately,
  //   without making a per-answer API round-trip.
  //
  //   The real correctness check happens server-side on submitBattle.
  //   For instant feedback we compare locally against question.word.
  //
  const handleAnswer = useCallback(async (
    question: QuizQuestionDto,
    answer: string,
    onAnswerProcessed: () => void,
    onResult?: (isCorrect: boolean) => void,
    responseTimeSeconds?: number,
  ): Promise<boolean> => {
    const isCorrect =
      answer.trim().toLowerCase() === (question.word ?? '').toLowerCase();

    const ms = responseTimeSeconds != null
      ? Math.round(responseTimeSeconds * 1000)
      : Date.now() - questionStartRef.current;

    const dto: BattleAnswerDto = {
      vocabularyId:  question.vocabularyId,
      answer:        answer.trim(),
      questionOrder: currentIdx + 1,
      responseTimeMs: ms,
    };

    setAnswers(prev => {
      // Prevent duplicate recordings if called twice (strict-mode safe)
      const alreadyRecorded = prev.some(a => a.questionOrder === dto.questionOrder);
      return alreadyRecorded ? prev : [...prev, dto];
    });

    onAnswerProcessed();
    onResult?.(isCorrect);
    return isCorrect;
  }, [currentIdx]);

  // ── Next question / finish ────────────────────────────────────────
  const handleNext = useCallback(async () => {
    const isLast = currentIdx + 1 >= totalQ;

    if (!isLast) {
      setCurrentIdx(i => i + 1);
      return;
    }

    // ── Final submit ──────────────────────────────────────────────
    setPhase('submitting');
    try {
      // Collect final answer list (answers state may not include the
      // very last answer yet due to React batching – use a stable snapshot)
      const finalAnswers = answers;

      const battleResult = await submitBattle(
        Number(sessionId),
        finalAnswers,
      );

      setResult(battleResult);

      // Flash transition then show result
      setTransitioning(true);
      setTimeout(() => {
        setPhase('result');
        setTransitioning(false);
      }, 300);

    } catch (err) {
      console.error('[GymBattle] submitBattle failed:', err);
      setHasError(true);
      setPhase('battle'); // let error screen render in-place
    }
  }, [currentIdx, totalQ, answers, sessionId]);

  // ── Render guards ─────────────────────────────────────────────────
  if (!session) return null;

  if (hasError) {
    return <ErrorScreen onBack={() => navigate(`/gym/${gymLeaderId}`)} />;
  }

  // ─────────────────────────────────────────────────────────────────
  return (
    <>
      {/* Phase-change flash overlay */}
      <PhaseTransition visible={transitioning} color={themeColor} />

      {/* ── INTRO ── */}
      <AnimatePresence mode="wait">
        {phase === 'intro' && (
          <motion.div
            key="intro"
            initial={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.2 }}
          >
            <BattleIntro
              session={session}
              onComplete={handleIntroComplete}
            />
          </motion.div>
        )}
      </AnimatePresence>

      {/* ── BATTLE ── */}
      <AnimatePresence mode="wait">
        {phase === 'battle' && (
          <motion.div
            key="battle"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.25 }}
          >
            <BattleArena
              session={session}
              currentIdx={currentIdx}
              onAnswer={handleAnswer}
              onNext={handleNext}
              isLastQuestion={currentIdx + 1 >= totalQ}
              isSubmitting={false}
            />
          </motion.div>
        )}
      </AnimatePresence>

      {/* ── SUBMITTING ── */}
      <AnimatePresence>
        {phase === 'submitting' && (
          <motion.div
            key="submitting"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.2 }}
          >
            <SubmittingScreen color={themeColor} />
          </motion.div>
        )}
      </AnimatePresence>

      {/* ── RESULT ── */}
      <AnimatePresence>
        {phase === 'result' && result && (
          <motion.div
            key="result"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ duration: 0.3 }}
          >
            <BattleResult
              result={result}
              gymLeaderId={gymLeaderId}
            />
          </motion.div>
        )}
      </AnimatePresence>
    </>
  );
}