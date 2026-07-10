import { useState, useEffect, useCallback } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { fetchPronunciationWords, assessPronunciation } from '../../services/pronunciation';
import type { PronunciationWordDto, PronunciationAssessResponse } from '../../services/pronunciation';
import { useAudioRecorder } from '../../hooks/useAudioRecorder';

const ACCENT = '#a78bfa';

// ── Helpers ─────────────────────────────────────────────────────────────────

function scoreColor(score: number) {
  if (score >= 80) return '#4ade80'; // green
  if (score >= 50) return '#facc15'; // yellow
  return '#f87171'; // red
}

function resultEmoji(result: string) {
  if (result === 'Perfect') return '✅';
  if (result === 'NearMiss') return '⚠️';
  return '❌';
}

function resultLabel(result: string) {
  if (result === 'Perfect') return 'Chuẩn!';
  if (result === 'NearMiss') return 'Gần đúng';
  return 'Cần luyện thêm';
}

// ── Recording button ─────────────────────────────────────────────────────────

interface RecordButtonProps {
  state: 'idle' | 'requesting' | 'recording' | 'processing' | 'done' | 'error';
  onStart: () => void;
  onStop: () => void;
}

function RecordButton({ state, onStart, onStop }: RecordButtonProps) {
  const isRecording = state === 'recording';
  const isProcessing = state === 'processing' || state === 'requesting';

  return (
    <button
      onClick={isRecording ? onStop : onStart}
      disabled={isProcessing}
      className="relative w-20 h-20 rounded-full flex items-center justify-center transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
      style={{
        background: isRecording ? '#ef4444' : ACCENT,
        boxShadow: isRecording
          ? '0 0 0 0 rgba(239,68,68,0.7)'
          : `0 0 25px ${ACCENT}88`,
        animation: isRecording ? 'pulse-ring 1.2s cubic-bezier(0.4,0,0.6,1) infinite' : undefined,
      }}
    >
      {isProcessing ? (
        <div className="w-7 h-7 border-3 border-t-transparent border-white rounded-full animate-spin" style={{ borderWidth: 3, borderTopColor: 'transparent' }} />
      ) : isRecording ? (
        <div className="w-5 h-5 bg-white rounded-sm" />
      ) : (
        <svg className="w-8 h-8 text-black" fill="currentColor" viewBox="0 0 24 24">
          <path d="M12 14c1.66 0 3-1.34 3-3V5c0-1.66-1.34-3-3-3S9 3.34 9 5v6c0 1.66 1.34 3 3 3z" />
          <path d="M17 11c0 2.76-2.24 5-5 5s-5-2.24-5-5H5c0 3.53 2.61 6.43 6 6.92V21h2v-3.08c3.39-.49 6-3.39 6-6.92h-2z" />
        </svg>
      )}
    </button>
  );
}

// ── Phoneme Highlight ────────────────────────────────────────────────────────

interface PhonemeBarProps {
  phoneme: string;
  score: number;
  resultLabel: string;
}

function PhonemeBar({ phoneme, score, resultLabel: rl }: PhonemeBarProps) {
  const color = scoreColor(score);
  return (
    <div className="flex flex-col items-center gap-1">
      <div
        className="w-8 h-8 rounded-lg flex items-center justify-center text-xs font-bold"
        style={{ background: `${color}22`, border: `1px solid ${color}66`, color }}
      >
        {phoneme}
      </div>
      <div className="w-8 h-1 rounded-full bg-gray-800 overflow-hidden">
        <div
          className="h-full rounded-full transition-all duration-700"
          style={{ width: `${score}%`, background: color }}
        />
      </div>
      <span className="text-[9px] font-pixel" style={{ color }}>
        {rl === 'Perfect' ? '✓' : rl === 'NearMiss' ? '~' : '✗'}
      </span>
    </div>
  );
}

// ── Result Card ──────────────────────────────────────────────────────────────

interface ResultCardProps {
  assessment: PronunciationAssessResponse;
  word: PronunciationWordDto;
  onNext: () => void;
  onRetry: () => void;
}

function ResultCard({ assessment, word, onNext, onRetry }: ResultCardProps) {
  const color = scoreColor(assessment.pronunciationScore);
  const isPerfect = assessment.result === 'Perfect';

  return (
    <div
      className="rounded-3xl p-6 border space-y-5 animate-fade-in"
      style={{ background: `${color}0a`, borderColor: `${color}44` }}
    >
      {/* Score circle */}
      <div className="flex flex-col items-center gap-2">
        <div
          className="w-20 h-20 rounded-full flex items-center justify-center text-3xl font-press"
          style={{
            background: `${color}22`,
            border: `3px solid ${color}`,
            boxShadow: `0 0 20px ${color}66`,
            color,
          }}
        >
          {Math.round(assessment.pronunciationScore)}
        </div>
        <span className="font-pixel text-sm" style={{ color }}>
          {resultEmoji(assessment.result)} {resultLabel(assessment.result)}
        </span>
      </div>

      {/* Score breakdown */}
      <div className="grid grid-cols-3 gap-2">
        {[
          { label: 'Chính xác', val: assessment.accuracyScore },
          { label: 'Lưu loát', val: assessment.fluencyScore },
          { label: 'Đầy đủ', val: assessment.completenessScore },
        ].map(({ label, val }) => (
          <div
            key={label}
            className="rounded-xl p-2 text-center border border-white/5"
            style={{ background: 'rgba(255,255,255,0.03)' }}
          >
            <p className="text-gray-500 font-noto text-[10px] mb-1">{label}</p>
            <p className="font-pixel text-xs" style={{ color: scoreColor(val) }}>
              {Math.round(val)}
            </p>
          </div>
        ))}
      </div>

      {/* Phoneme detail */}
      {assessment.phonemes.length > 0 && (
        <div>
          <p className="text-gray-500 font-pixel text-[10px] mb-3 text-center uppercase tracking-widest">Chi tiết Phoneme</p>
          <div className="flex flex-wrap gap-3 justify-center">
            {assessment.phonemes.map((ph, i) => (
              <PhonemeBar key={i} phoneme={ph.phoneme} score={ph.accuracyScore} resultLabel={ph.resultLabel} />
            ))}
          </div>
        </div>
      )}

      {/* XP reward */}
      {assessment.xpAwarded > 0 && (
        <div
          className="rounded-xl px-4 py-2 text-center border"
          style={{ background: '#facc1510', borderColor: '#facc1540' }}
        >
          <span className="font-pixel text-xs text-yellow-400">
            +{assessment.xpAwarded} XP
            {assessment.petXpMultiplier > 1 && (
              <span className="text-yellow-500 text-[10px] ml-1">(×{assessment.petXpMultiplier.toFixed(1)} Pet Buff)</span>
            )}
          </span>
        </div>
      )}

      {/* Action buttons */}
      <div className="flex gap-3">
        {!isPerfect && (
          <button
            onClick={onRetry}
            className="flex-1 py-3 rounded-xl font-pixel text-xs border border-gray-600 text-gray-300 hover:border-gray-400 hover:text-white transition-all"
          >
            🔄 Thử lại
          </button>
        )}
        <button
          onClick={onNext}
          className="flex-1 py-3 rounded-xl font-pixel text-sm text-black transition-all hover:scale-105"
          style={{ background: color, boxShadow: `0 0 15px ${color}66` }}
        >
          {isPerfect ? '✓ Hoàn thành' : '→ Tiếp theo'}
        </button>
      </div>

      {/* IPA & meaning */}
      <div className="text-center text-gray-400 font-noto text-xs space-y-1">
        {word.ipaTranscription && <p>{word.ipaTranscription}</p>}
        <p className="text-gray-500">{word.meaning}</p>
      </div>
    </div>
  );
}

// ── Main Component ───────────────────────────────────────────────────────────

export default function PronunciationSession() {
  const navigate = useNavigate();
  const location = useLocation();
  const petId: number | null = (location.state as { petId: number | null })?.petId ?? null;

  const [stack, setStack] = useState<PronunciationWordDto[]>([]);
  const [completed, setCompleted] = useState<PronunciationWordDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [assessment, setAssessment] = useState<PronunciationAssessResponse | null>(null);
  // Lưu riêng từ đang được đánh giá để tránh stale reference khi stack thay đổi
  const [assessedWord, setAssessedWord] = useState<PronunciationWordDto | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const { state: recState, audioBlob, startRecording, stopRecording, reset: resetRecorder } = useAudioRecorder(10000);

  // Load từ luyện tập
  useEffect(() => {
    fetchPronunciationWords(20)
      .then((words) => {
        if (words.length === 0) {
          setError('Bạn chưa có từ nào đã học. Hãy học từ mới trước!');
        } else {
          setStack(words);
        }
      })
      .catch(() => setError('Không thể tải danh sách từ. Vui lòng thử lại.'))
      .finally(() => setLoading(false));
  }, []);

  // Khi có audio blob → tự động gửi đánh giá
  useEffect(() => {
    if (recState !== 'done' || !audioBlob || !stack[0]) return;

    const wordBeingAssessed = stack[0];
    setAssessedWord(wordBeingAssessed); // Snapshot trước khi gọi API
    setSubmitting(true);

    assessPronunciation(audioBlob, wordBeingAssessed.vocabularyId, wordBeingAssessed.word, petId ?? undefined)
      .then(setAssessment)
      .catch(() => setError('Đánh giá phát âm thất bại. Vui lòng thử lại.'))
      .finally(() => setSubmitting(false));
  }, [recState, audioBlob, stack, petId]);

  const handleNext = useCallback(() => {
    if (!assessment || !assessedWord) return;

    if (assessment.result === 'Perfect') {
      setCompleted((prev) => [...prev, assessedWord]);
      setStack((prev) => prev.filter(w => w.vocabularyId !== assessedWord.vocabularyId));
    } else {
      // NearMiss hoặc Wrong — xuống cuối stack
      setStack((prev) => {
        const rest = prev.filter(w => w.vocabularyId !== assessedWord.vocabularyId);
        return [...rest, assessedWord];
      });
    }

    setAssessment(null);
    setAssessedWord(null);
    resetRecorder();
  }, [assessment, assessedWord, resetRecorder]);

  const handleRetry = useCallback(() => {
    setAssessment(null);
    setAssessedWord(null);
    resetRecorder();
  }, [resetRecorder]);

  const currentWord = stack[0];
  const displayWord = assessedWord ?? currentWord; // dùng snapshot khi có assessment
  const totalWords = stack.length + completed.length;
  const progressPct = totalWords > 0 ? Math.round((completed.length / totalWords) * 100) : 0;

  // ── Render states ──────────────────────────────────────────────────────────

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center" style={{ background: 'rgb(2,6,23)' }}>
        <div className="text-center space-y-4">
          <div className="w-10 h-10 border-4 border-t-transparent rounded-full animate-spin mx-auto" style={{ borderColor: ACCENT, borderTopColor: 'transparent' }} />
          <p className="font-pixel text-[11px]" style={{ color: ACCENT }}>Đang tải từ luyện tập...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center px-6" style={{ background: 'rgb(2,6,23)' }}>
        <div className="text-center space-y-4">
          <p className="text-5xl">😕</p>
          <p className="font-pixel text-xs text-red-400">{error}</p>
          <button
            onClick={() => navigate('/home')}
            className="px-6 py-3 rounded-xl font-pixel text-xs border border-gray-600 text-gray-300 hover:border-gray-400"
          >
            ← Về trang chủ
          </button>
        </div>
      </div>
    );
  }

  // Hoàn thành tất cả từ
  if (stack.length === 0 && completed.length > 0) {
    return (
      <div className="min-h-screen text-white flex flex-col items-center justify-center px-6 text-center" style={{ background: 'rgb(2,6,23)' }}>
        <div
          className="w-24 h-24 rounded-full flex items-center justify-center text-5xl mb-6"
          style={{ background: `${ACCENT}22`, border: `3px solid ${ACCENT}`, boxShadow: `0 0 40px ${ACCENT}66` }}
        >
          🎉
        </div>
        <h1 className="font-pixel text-2xl mb-2" style={{ color: ACCENT }}>HOÀN THÀNH!</h1>
        <p className="font-noto text-gray-400 text-sm mb-2">
          Bạn đã phát âm chuẩn {completed.length} từ trong phiên này.
        </p>
        <p className="font-pixel text-[10px] text-gray-500 mb-8">
          Hãy ôn luyện thêm để giữ vững kỹ năng!
        </p>
        <div className="flex flex-col gap-3 w-full max-w-xs">
          <button
            onClick={() => navigate('/pronunciation')}
            className="w-full py-4 rounded-2xl font-pixel text-sm text-black hover:scale-105 transition-all"
            style={{ background: ACCENT, boxShadow: `0 0 20px ${ACCENT}66` }}
          >
            🔁 Luyện lại
          </button>
          <button
            onClick={() => navigate('/home')}
            className="w-full py-3 rounded-2xl font-pixel text-xs text-gray-400 hover:text-gray-200 border border-gray-700 hover:border-gray-500 transition-all"
          >
            ← Về trang chủ
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen text-white flex flex-col" style={{ background: 'rgb(2,6,23)' }}>
      {/* Header */}
      <div className="px-4 pt-6 pb-4">
        <div className="flex items-center justify-between mb-4">
          <button
            onClick={() => navigate(-1)}
            className="text-gray-400 hover:text-white font-pixel text-xs flex items-center gap-1"
          >
            ← Thoát
          </button>
          <span className="font-pixel text-[10px]" style={{ color: ACCENT }}>
            🎙️ PRONUNCIATION
          </span>
          <span className="font-pixel text-xs text-gray-400">
            {completed.length}/{totalWords}
          </span>
        </div>

        {/* Progress bar */}
        <div className="h-2 bg-gray-800 rounded-full overflow-hidden">
          <div
            className="h-full rounded-full transition-all duration-700"
            style={{ width: `${progressPct}%`, background: ACCENT, boxShadow: `0 0 8px ${ACCENT}88` }}
          />
        </div>
        <p className="text-gray-600 font-noto text-[10px] mt-1 text-right">
          Còn {stack.length} từ
        </p>
      </div>

      {/* Main content */}
      <div className="flex-1 px-4 pb-8 flex flex-col gap-6 max-w-lg mx-auto w-full">

        {/* Word card */}
        {displayWord && (
          <div
            className="rounded-3xl p-6 border relative overflow-hidden"
            style={{
              background: `linear-gradient(135deg, rgb(2,6,23) 0%, ${ACCENT}0d 100%)`,
              borderColor: `${ACCENT}33`,
            }}
          >
            {/* Stack indicator — lớp thẻ phía sau */}
            {stack.length > 1 && (
              <>
                <div
                  className="absolute inset-x-4 bottom-0 h-2 rounded-b-3xl opacity-20"
                  style={{ background: ACCENT, transform: 'translateY(4px)' }}
                />
                <div
                  className="absolute inset-x-6 bottom-0 h-2 rounded-b-3xl opacity-10"
                  style={{ background: ACCENT, transform: 'translateY(8px)' }}
                />
              </>
            )}

            {/* Wrong count badge */}
            {currentWord.pronunciationWrongCount > 0 && (
              <div className="absolute top-4 right-4 px-2 py-0.5 rounded-full bg-red-900/40 border border-red-500/30">
                <span className="text-red-400 font-pixel text-[9px]">
                  {currentWord.pronunciationWrongCount}× sai
                </span>
              </div>
            )}

            {/* Memory state badge */}
            <div className="absolute top-4 left-4">
              <span
                className="px-2 py-0.5 rounded-full font-pixel text-[9px] border"
                style={{ borderColor: `${ACCENT}44`, color: ACCENT, background: `${ACCENT}15` }}
              >
                {currentWord.memoryState}
              </span>
            </div>

            <div className="text-center mt-6">
              <h2 className="font-press text-3xl text-white mb-2 tracking-wide">
                {currentWord.word}
              </h2>
              {currentWord.ipaTranscription && (
                <p className="text-gray-400 font-noto text-base mb-1">
                  {currentWord.ipaTranscription}
                </p>
              )}
              <p className="text-gray-500 font-noto text-sm">{currentWord.meaning}</p>

              {/* Audio hint button */}
              {currentWord.pronunciationUrl && (
                <button
                  onClick={() => new Audio(currentWord.pronunciationUrl!).play()}
                  className="mt-3 inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg border text-gray-400 hover:text-white hover:border-gray-500 transition-all font-pixel text-[10px]"
                  style={{ borderColor: 'rgba(255,255,255,0.1)' }}
                >
                  🔊 Nghe mẫu
                </button>
              )}

              {/* Example sentence */}
              {currentWord.exampleSentence && (
                <p className="mt-4 text-gray-600 font-noto text-xs italic leading-relaxed">
                  "{currentWord.exampleSentence}"
                </p>
              )}
            </div>
          </div>
        )}

        {/* Assessment result or recorder */}
        {assessment && assessedWord ? (
          <ResultCard
            assessment={assessment}
            word={assessedWord}
            onNext={handleNext}
            onRetry={handleRetry}
          />
        ) : (
          <div className="flex flex-col items-center gap-5">
            {/* Recorder status */}
            <div className="text-center h-8 flex items-center justify-center">
              {recState === 'idle' && (
                <p className="font-noto text-gray-500 text-sm">Nhấn micro để bắt đầu ghi âm</p>
              )}
              {recState === 'requesting' && (
                <p className="font-pixel text-[11px] animate-pulse" style={{ color: ACCENT }}>
                  Đang xin quyền micro...
                </p>
              )}
              {recState === 'recording' && (
                <div className="flex items-center gap-2">
                  <div className="w-2.5 h-2.5 rounded-full bg-red-500 animate-pulse" />
                  <p className="font-pixel text-[11px] text-red-400">Đang ghi âm... (nhấn để dừng)</p>
                </div>
              )}
              {(recState === 'done' || submitting) && (
                <p className="font-pixel text-[11px] animate-pulse" style={{ color: ACCENT }}>
                  Đang phân tích phát âm...
                </p>
              )}
              {recState === 'error' && (
                <p className="font-pixel text-[11px] text-red-400">
                  Lỗi micro. Kiểm tra quyền và thử lại.
                </p>
              )}
            </div>

            {/* Record button */}
            <RecordButton
              state={submitting ? 'processing' : recState}
              onStart={startRecording}
              onStop={stopRecording}
            />

            {/* Max duration hint */}
            <p className="font-noto text-gray-600 text-xs">Tối đa 10 giây</p>

            {/* Skip word */}
            <button
              onClick={() => {
                const top = stack[0];
                if (!top) return;
                setStack((prev) => [...prev.slice(1), top]);
                resetRecorder();
              }}
              className="font-pixel text-[10px] text-gray-600 hover:text-gray-400 transition-colors"
            >
              Bỏ qua từ này →
            </button>
          </div>
        )}
      </div>

      {/* Inline styles for pulse-ring animation */}
      <style>{`
        @keyframes pulse-ring {
          0% { box-shadow: 0 0 0 0 rgba(239,68,68,0.7); }
          70% { box-shadow: 0 0 0 12px rgba(239,68,68,0); }
          100% { box-shadow: 0 0 0 0 rgba(239,68,68,0); }
        }
        @keyframes fade-in {
          from { opacity: 0; transform: translateY(8px); }
          to   { opacity: 1; transform: translateY(0); }
        }
        .animate-fade-in { animation: fade-in 0.35s ease; }
      `}</style>
    </div>
  );
}
