import { useEffect, useRef, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useBattleArena } from '../../hooks/useBattleArena';
import type { PetStateDto, RoundResultDto } from '../../types/BattleArenaTypes';

// ── Sub-components ─────────────────────────────────────────────────────────────

function HpBar({ current, max }: { current: number; max: number }) {
    const pct = Math.max(0, Math.round((current / max) * 100));
    const barColor = pct > 50 ? '#4ade80' : pct > 25 ? '#facc15' : '#f87171';
    return (
        <div className="w-full">
            <div className="flex justify-between mb-0.5">
                <span className="text-[9px] font-pixel text-gray-400">HP</span>
                <span className="text-[9px] font-pixel" style={{ color: barColor }}>{current}/{max}</span>
            </div>
            <div className="h-2 rounded-full bg-gray-800 overflow-hidden border border-gray-700">
                <div className="h-full rounded-full transition-all duration-500"
                    style={{ width: `${pct}%`, background: barColor, boxShadow: `0 0 6px ${barColor}88` }} />
            </div>
        </div>
    );
}

function PetCard({ pet, flipped = false }: { pet: PetStateDto; flipped?: boolean }) {
    return (
        <div className={`flex flex-col ${flipped ? 'items-end' : 'items-start'} gap-1 w-28`}>
            <div className={`w-20 h-20 rounded-xl flex items-center justify-center relative
        ${pet.isFainted ? 'grayscale opacity-30' : ''}
        bg-gray-800/60 border border-gray-700`}
                style={{ transform: flipped ? 'scaleX(-1)' : undefined }}>
                {pet.imageUrl
                    ? <img src={pet.imageUrl} alt={pet.displayName} className="w-16 h-16 object-contain pixel-art" />
                    : <span className="text-4xl">🐾</span>}
                {pet.isFainted && (
                    <div className="absolute inset-0 flex items-center justify-center">
                        <span className="text-red-400 text-2xl">💀</span>
                    </div>
                )}
            </div>
            <span className="text-[10px] font-pixel text-gray-300 truncate w-full">{pet.displayName}</span>
            <HpBar current={pet.currentHp} max={pet.maxHp} />
        </div>
    );
}

function PartyRack({ pets, flipped = false }: { pets: PetStateDto[]; flipped?: boolean }) {
    return (
        <div className={`flex gap-1 ${flipped ? 'flex-row-reverse' : ''}`}>
            {pets.map((p, i) => (
                <div key={i} title={p.displayName}
                    className={`w-5 h-5 rounded-full border-2 transition-all
            ${p.isFainted
                            ? 'border-gray-700 bg-gray-800'
                            : 'border-green-400 bg-green-400/20 shadow-[0_0_6px_rgba(74,222,128,0.5)]'}`} />
            ))}
        </div>
    );
}

function ScoreTimer({ timeLimitMs, onElapsed, answered }: {
    timeLimitMs: number;
    onElapsed: (ms: number) => void;
    answered: boolean;
}) {
    const [elapsed, setElapsed] = useState(0);
    const startRef = useRef(Date.now());
    const calledRef = useRef(false);

    useEffect(() => {
        startRef.current = Date.now();
        setElapsed(0);
        calledRef.current = false;
    }, [timeLimitMs]);

    useEffect(() => {
        if (answered) return;
        const id = setInterval(() => {
            const e = Date.now() - startRef.current;
            if (e >= timeLimitMs) {
                clearInterval(id);
                setElapsed(timeLimitMs);
                if (!calledRef.current) {
                    calledRef.current = true;
                    onElapsed(timeLimitMs);
                }
            } else {
                setElapsed(e);
            }
        }, 50);
        return () => clearInterval(id);
    }, [timeLimitMs, onElapsed, answered]);

    const score = Math.max(0, Math.round(1000 - (elapsed / timeLimitMs) * 900));
    const pct = Math.max(0, 100 - (elapsed / timeLimitMs) * 100);
    const barColor = pct > 50 ? '#4ade80' : pct > 25 ? '#facc15' : '#f87171';

    return (
        <div className="w-full">
            <div className="flex justify-between mb-1">
                <span className="font-pixel text-xs text-gray-400">SCORE</span>
                <span className="font-press text-sm" style={{ color: barColor }}>{score}</span>
            </div>
            <div className="h-3 rounded-full bg-gray-800 overflow-hidden border border-gray-700">
                <div className="h-full rounded-full transition-none"
                    style={{ width: `${pct}%`, background: barColor, boxShadow: `0 0 8px ${barColor}66` }} />
            </div>
        </div>
    );
}

function RoundResultOverlay({ result, onDone }: { result: RoundResultDto; onDone: () => void }) {
    const attacked = result.damagedPlayer === 2 ? 'P2' : result.damagedPlayer === 1 ? 'P1' : 'none';

    useEffect(() => {
        const t = setTimeout(onDone, 2500);
        return () => clearTimeout(t);
    }, [onDone]);

    return (
        <div className="absolute inset-0 z-30 flex flex-col items-center justify-center gap-4 bg-black/60 rounded-2xl p-4">
            <div className="text-center">
                <div className={`text-4xl mb-2 ${result.p1Correct ? (result.damagedPlayer === 2 ? 'animate-bounce' : '') : ''}`}>
                    {result.damagedPlayer === 2 ? '⚔️' : result.damagedPlayer === 1 ? '💥' : '🤝'}
                </div>
                <div className="font-press text-sm text-white mb-1">
                    {result.damagedPlayer === 0 ? 'DRAW!' : attacked === 'P2' ? 'YOU HIT!' : 'TOOK DAMAGE!'}
                </div>
                {result.damageDealt > 0 && (
                    <div className={`font-pixel text-xs ${result.damagedPlayer === 1 ? 'text-red-400' : 'text-green-400'}`}>
                        {result.damageDealt} DMG
                    </div>
                )}
            </div>
            <div className="grid grid-cols-2 gap-4 w-full max-w-xs text-center">
                <div>
                    <div className="text-gray-400 font-pixel text-[9px] mb-1">YOU</div>
                    <div className={`font-press text-lg ${result.p1Correct ? 'text-green-400' : 'text-red-400'}`}>
                        {result.p1Score}
                    </div>
                    <div className="font-noto text-[10px] text-gray-500">{result.p1AnswerMs}ms</div>
                </div>
                <div>
                    <div className="text-gray-400 font-pixel text-[9px] mb-1">BOT</div>
                    <div className={`font-press text-lg ${result.p2Correct ? 'text-green-400' : 'text-red-400'}`}>
                        {result.p2Score}
                    </div>
                    <div className="font-noto text-[10px] text-gray-500">{result.p2AnswerMs}ms</div>
                </div>
            </div>
            <div className="font-noto text-xs text-gray-400">
                ✅ <span className="text-white">{result.correctAnswer}</span>
            </div>
        </div>
    );
}

// ── Main BattleArena ───────────────────────────────────────────────────────────

export default function BattleArena() {
    const { sessionId: sessionIdStr } = useParams<{ sessionId: string }>();
    const navigate = useNavigate();

    const sessionId = Number(sessionIdStr);
    const { state, submitAnswer } = useBattleArena(sessionId);
    const [showOverlay, setShowOverlay] = useState(false);
    const questionStartRef = useRef<number>(Date.now());  // track when each question started

    useEffect(() => {
        if (state.phase === 'roundResult') setShowOverlay(true);
        else setShowOverlay(false);
    }, [state.phase, state.lastRoundResult]);

    useEffect(() => {
        if (state.phase === 'ended' && state.battleResult) {
            navigate(`/arena/${sessionId}/result`, { state: { result: state.battleResult } });
        }
    }, [state.phase, state.battleResult, navigate, sessionId]);

    useEffect(() => {
        // Reset timer when a new question arrives
        if (state.phase === 'battle') questionStartRef.current = Date.now();
    }, [state.currentQuestion, state.phase]);

    const handleTimeup = (ms: number) => {
        submitAnswer('', ms);
    };

    const handleChoice = (choice: string) => {
        submitAnswer(choice, Date.now() - questionStartRef.current);
    };

    const activePet = (pets: PetStateDto[]) =>
        pets.find(p => !p.isFainted) ?? pets[0];

    if (state.phase === 'connecting') {
        return (
            <div className="min-h-screen flex items-center justify-center" style={{ background: 'rgb(2,6,23)' }}>
                <div className="text-center">
                    <div className="text-yellow-400 text-4xl mb-4 animate-pulse">⚔️</div>
                    <p className="font-press text-sm text-white">CONNECTING TO BATTLE...</p>
                </div>
            </div>
        );
    }

    if (state.phase === 'error') {
        return (
            <div className="min-h-screen flex items-center justify-center" style={{ background: 'rgb(2,6,23)' }}>
                <div className="text-center max-w-sm">
                    <div className="text-4xl mb-4">💔</div>
                    <p className="font-pixel text-red-400 text-sm mb-4">{state.errorMsg}</p>
                    <button onClick={() => navigate('/gym')}
                        className="font-pixel text-xs text-gray-400 hover:text-white">← Back to Gym</button>
                </div>
            </div>
        );
    }

    const q = state.currentQuestion;
    const p1Active = state.p1Pets.length ? activePet(state.p1Pets) : null;
    const p2Active = state.p2Pets.length ? activePet(state.p2Pets) : null;

    return (
        <div className="min-h-screen text-white flex flex-col items-center pt-4 pb-8 px-4"
            style={{ background: 'linear-gradient(180deg, rgb(2,6,23) 0%, rgb(10,2,30) 100%)' }}>

            {/* ── Top bar: avatars + party ─────────────────────────────────── */}
            <div className="w-full max-w-4xl flex justify-between items-center mb-6 px-4">
                {/* P1 */}
                <div className="flex flex-col gap-1 items-start">
                    <div className="flex items-center gap-2">
                        <div className="w-10 h-10 rounded-full bg-blue-600/30 border border-blue-500/40 flex items-center justify-center text-lg">🧑</div>
                        <span className="font-pixel text-[12px] text-blue-300">YOU</span>
                    </div>
                    <PartyRack pets={state.p1Pets} />
                </div>
                {/* Scores */}
                <div className="text-center">
                    <div className="font-press text-xs text-gray-400">TOTAL</div>
                    <div className="flex gap-2 items-center">
                        <span className="font-press text-sm text-blue-400">{state.p1TotalScore}</span>
                        <span className="text-gray-600">vs</span>
                        <span className="font-press text-sm text-red-400">{state.p2TotalScore}</span>
                    </div>
                    {q && (
                        <div className={`font-pixel text-[9px] mt-1 ${(q.roundIndex ?? 0) >= 14 ? 'text-red-500 animate-pulse' : (q.roundIndex ?? 0) >= 9 ? 'text-orange-400' : 'text-gray-500'}`}>
                            {(q.roundIndex ?? 0) >= 9 ? '🔥 SUDDEN DEATH ' : 'Round '}{(q.roundIndex ?? 0) + 1}
                        </div>
                    )}
                </div>
                {/* P2 (Bot) */}
                <div className="flex flex-col gap-1 items-end">
                    <div className="flex items-center gap-2">
                        <span className="font-pixel text-[12px] text-red-300">GYM</span>
                        <div className="w-10 h-10 rounded-full bg-red-600/30 border border-red-500/40 flex items-center justify-center text-lg">🤖</div>
                    </div>
                    <PartyRack pets={state.p2Pets} flipped />
                </div>
            </div>

            {/* ── Pokémon battlefield ─────────────────────────────────────── */}
            <div className="w-full max-w-4xl relative h-64 mb-6 mt-4">
                {/* P2 (top-right, facing left) */}
                {p2Active && (
                    <div className="absolute top-0 right-8 transform scale-125">
                        <PetCard pet={p2Active} flipped={false} />
                    </div>
                )}
                {/* P1 (bottom-left, facing right) */}
                {p1Active && (
                    <div className="absolute bottom-0 left-8 transform scale-125">
                        <PetCard pet={p1Active} flipped={true} />
                    </div>
                )}
                {/* Round result overlay */}
                {showOverlay && state.lastRoundResult && (
                    <RoundResultOverlay
                        result={state.lastRoundResult}
                        onDone={() => setShowOverlay(false)} />
                )}
            </div>

            {/* ── Question panel ──────────────────────────────────────────── */}
            {q && (
                <div className="w-full max-w-3xl">
                    {/* Score timer */}
                    <div className="mb-4">
                        <ScoreTimer
                            key={q.roundIndex}
                            timeLimitMs={q.timeLimitMs}
                            onElapsed={handleTimeup}
                            answered={state.answered} />
                    </div>

                    {/* Question card */}
                    <div className="rounded-2xl border border-gray-700/60 bg-gray-900/80 p-5 mb-3 backdrop-blur">
                        {q.questionType === 'MultipleChoice' ? (
                            <>
                                <p className="font-pixel text-xs text-gray-400 mb-2">WHAT IS THE MEANING OF:</p>
                                <p className="font-press text-xl text-yellow-300 text-center mb-1">{q.word}</p>
                                {q.pronunciation && (
                                    <p className="font-noto text-gray-500 text-xs text-center mb-3">/{q.pronunciation}/</p>
                                )}
                            </>
                        ) : (
                            <>
                                <p className="font-pixel text-xs text-gray-400 mb-2">FILL IN THE BLANK:</p>
                                <p className="font-noto text-white text-base text-center leading-relaxed mb-3">
                                    {q.questionPrompt}
                                </p>
                            </>
                        )}
                    </div>

                    {/* Answer options */}
                    {q.questionType === 'MultipleChoice' && q.options && (
                        <div className="grid grid-cols-2 gap-3">
                            {q.options.map((opt, i) => {
                                const letters = ['A', 'B', 'C', 'D'];
                                return (
                                    <button key={i} onClick={() => !state.answered && handleChoice(opt)}
                                        disabled={state.answered}
                                        className={`p-3 rounded-xl border font-noto text-sm text-left transition-all duration-150
                      ${state.answered
                                                ? 'border-gray-700/40 text-gray-500 cursor-not-allowed bg-gray-800/30'
                                                : 'border-gray-600/60 bg-gray-800/60 hover:border-yellow-400/60 hover:bg-yellow-400/10 hover:scale-105 active:scale-95'}`}>
                                        <span className="font-pixel text-[10px] text-gray-500 mr-2">{letters[i]}</span>
                                        {opt}
                                    </button>
                                );
                            })}
                        </div>
                    )}

                    {q.questionType === 'FillInBlank' && (
                        <FillInBlankInput
                            disabled={state.answered}
                            onSubmit={(ans, ms) => submitAnswer(ans, ms)} />
                    )}
                </div>
            )}
        </div>
    );
}

function FillInBlankInput({
    disabled,
    onSubmit,
}: {
    disabled: boolean;
    onSubmit: (ans: string, ms: number) => void;
}) {
    const [val, setVal] = useState('');
    const startRef = useRef(Date.now());

    useEffect(() => {
        startRef.current = Date.now();
        setVal('');
    }, [disabled]); // reset when new question

    const submit = () => {
        if (disabled || !val.trim()) return;
        onSubmit(val.trim(), Date.now() - startRef.current);
    };

    return (
        <div className="flex gap-2">
            <input
                type="text"
                value={val}
                onChange={e => setVal(e.target.value)}
                onKeyDown={e => e.key === 'Enter' && submit()}
                disabled={disabled}
                placeholder="Type the missing word..."
                className="flex-1 px-4 py-3 rounded-xl bg-gray-800/80 border border-gray-600/60 text-white font-noto text-sm
          focus:outline-none focus:border-yellow-400/60 placeholder-gray-600 disabled:opacity-40" />
            <button onClick={submit} disabled={disabled || !val.trim()}
                className="px-5 py-3 rounded-xl font-pixel text-xs bg-yellow-400 text-black
          hover:bg-yellow-300 disabled:bg-gray-700 disabled:text-gray-500 disabled:cursor-not-allowed transition-all">
                GO
            </button>
        </div>
    );
}
