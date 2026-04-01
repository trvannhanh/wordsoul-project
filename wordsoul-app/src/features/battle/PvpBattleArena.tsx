import { useEffect, useRef, useState, useCallback } from 'react';
import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import { usePvpBattle } from '../../hooks/usePvpBattle';
import { useAuth } from '../../hooks/Auth/useAuth';
import type { PetStateDto } from '../../types/BattleArenaTypes';
import battleBg from '../../assets/battle-background.png';

import {
    Nameplate,
    PokemonSprite,
    TrainerSprite,
    PartyBalls,
    ScoreTimer,
    RoundResultOverlay,
    BattleIntroOverlay,
    FillInBlankInput,
} from '../../components/Battle';
import type { SpriteState } from '../../components/Battle';

export default function PvpBattleArena() {
    const { sessionId: sessionIdStr } = useParams<{ sessionId: string }>();
    const [searchParams] = useSearchParams();
    const roomCode = searchParams.get('code') || '';
    const navigate = useNavigate();
    const sessionId = Number(sessionIdStr);

    const { state, submitAnswer } = usePvpBattle(sessionId);
    const { user } = useAuth();
    
    const [showOverlay, setShowOverlay] = useState(false);
    const [showIntro, setShowIntro] = useState(true);
    const [p1SpriteState, setP1SpriteState] = useState<SpriteState>('idle');
    const [p2SpriteState, setP2SpriteState] = useState<SpriteState>('idle');
    const [trainerHidden, setTrainerHidden] = useState(false);
    const questionStartRef = useRef(Date.now());

    // Hide trainer after intro, trigger pokemon enter
    const handleIntroDone = useCallback(() => {
        setShowIntro(false);
        setTrainerHidden(true);
        setP1SpriteState('enter');
        setP2SpriteState('enter');
        setTimeout(() => {
            setP1SpriteState('idle');
            setP2SpriteState('idle');
        }, 600);
    }, []);

    // Round result → attack animations
    useEffect(() => {
        if (state.phase !== 'roundResult' || !state.lastRoundResult) return;
        const r = state.lastRoundResult;
        if (r.damagedPlayer === 2) {
            setP1SpriteState('attack');
            setTimeout(() => { setP2SpriteState('hit'); }, 300);
            setTimeout(() => { setP1SpriteState('idle'); setP2SpriteState('idle'); }, 700);
        } else if (r.damagedPlayer === 1) {
            setP2SpriteState('attack');
            setTimeout(() => { setP1SpriteState('hit'); }, 300);
            setTimeout(() => { setP1SpriteState('idle'); setP2SpriteState('idle'); }, 700);
        }
        setShowOverlay(true);
    }, [state.phase, state.lastRoundResult]);

    // Navigate to result
    useEffect(() => {
        if ((state.phase === 'ended' || state.phase === 'opponentLeft') && state.battleResult) {
            navigate(`/pvp/arena/${sessionId}/result`, {
                state: { result: state.battleResult }
            });
        }
    }, [state.phase, state.battleResult, navigate, sessionId]);

    // Track question start time
    useEffect(() => {
        if (state.phase === 'battle') questionStartRef.current = Date.now();
    }, [state.currentQuestion, state.phase]);

    const handleTimeup = (ms: number) => submitAnswer('', ms);
    const handleChoice = (choice: string) =>
        submitAnswer(choice, Date.now() - questionStartRef.current);

    const activePet = (pets: PetStateDto[]) =>
        pets.find(p => !p.isFainted) ?? pets[0];

    // ── Connecting / Interstitial states ────────────────────────────────────────

    if (state.phase === 'connecting') {
        return (
            <div className="h-screen w-full overflow-hidden flex items-center justify-center text-white" style={{ background: 'rgb(2,6,23)' }}>
                <div className="text-center">
                    <div className="text-purple-400 text-5xl mb-4 animate-pulse">📡</div>
                    <p className="font-press text-sm animate-pulse">CONNECTING TO SERVER...</p>
                </div>
            </div>
        );
    }

    if (state.phase === 'error') {
        return (
            <div className="h-screen w-full overflow-hidden flex items-center justify-center text-white" style={{ background: 'rgb(2,6,23)' }}>
                <div className="text-center max-w-sm px-4">
                    <div className="text-5xl mb-4">💔</div>
                    <p className="font-pixel text-red-400 text-xs mb-6">{state.errorMsg}</p>
                    <button onClick={() => navigate('/pvp')}
                        className="font-pixel text-xs text-gray-400 hover:text-white border border-gray-700 rounded px-4 py-2">
                        ← Back to Lobby
                    </button>
                </div>
            </div>
        );
    }
    
    if (state.phase === 'waiting' && state.totalRounds === 0) {
        // Only show this big waiting screen if battle hasn't started yet
        return (
            <div className="h-screen w-full overflow-hidden flex flex-col items-center justify-center text-white p-6" style={{ background: 'rgb(2,6,23)' }}>
                <h1 className="font-press text-2xl text-purple-400 mb-6 drop-shadow-[0_0_10px_#a855f7]">WAITING FOR OPPONENT</h1>
                
                {roomCode && (
                    <div className="bg-gray-900 border-2 border-purple-500 rounded-xl p-8 mb-8 text-center max-w-md w-full shadow-[0_0_30px_rgba(168,85,247,0.3)]">
                        <p className="font-pixel text-xs text-gray-400 mb-4">ROOM CODE</p>
                        <p className="font-press text-5xl text-yellow-400 tracking-[0.2em] mb-2">{roomCode}</p>
                        <p className="font-noto text-sm text-gray-500 mt-4">Share this code with your friend</p>
                    </div>
                )}
                
                <div className="flex gap-2 items-center text-gray-400 font-pixel text-xs animate-pulse">
                    <div className="w-2 h-2 bg-purple-500 rounded-full"></div>
                    {state.waitingMessage}
                </div>
                
                <button onClick={() => navigate('/pvp')}
                    className="mt-12 font-pixel text-xs text-gray-500 hover:text-red-400 transition-colors">
                    CANCEL & LEAVE
                </button>
            </div>
        );
    }

    const q = state.currentQuestion;
    const p1Active = state.p1Pets.length ? activePet(state.p1Pets) : null;
    const p2Active = state.p2Pets.length ? activePet(state.p2Pets) : null;

    // ── Main render ─────────────────────────────────────────────────────────────

    return (
        <div
            className="relative h-screen w-full overflow-hidden text-white"
            style={{ background: 'linear-gradient(180deg, rgb(10,2,25) 0%, rgb(15,2,20) 100%)' }}
        >
            {/* Battle Intro */}
            {showIntro && state.phase !== 'waiting' && (
                <BattleIntroOverlay
                    p1Avatar={user?.avatarUrl}
                    p2Avatar={state.opponent?.avatarUrl}
                    p2Name={state.opponent?.name || 'Opponent'}
                    onDone={handleIntroDone}
                />
            )}

            {/* ── BATTLEFIELD ──────────────────────────────────────────────────── */}
            <div
                className="absolute inset-0 w-full h-full overflow-hidden z-0"
                style={{
                    backgroundImage: `url(${battleBg})`,
                    backgroundSize: 'cover',
                    backgroundPosition: 'center bottom',
                    backgroundRepeat: 'no-repeat',
                    filter: 'hue-rotate(-20deg) saturate(1.2)' // Make it look slightly different for PvP
                }}
            >
                {/* ── P2: Nameplate top-left + Pokemon top-right ───────────────── */}
                <div className="absolute top-3 left-3 sm:top-4 sm:left-6 z-10">
                    {p2Active && <Nameplate pet={p2Active} />}
                    <div className="mt-1.5 flex flex-col gap-1">
                        <PartyBalls pets={state.p2Pets} />
                        {state.opponent && (
                            <div className="bg-black/50 border border-purple-500/50 rounded px-2 py-1 mt-1 inline-block">
                                <span className="font-pixel text-[8px] text-purple-300">{state.opponent.name}</span>
                            </div>
                        )}
                    </div>
                </div>

                <div
                    className="absolute z-10 flex flex-col items-center"
                    style={{ right: '8%', top: '10%' }}
                >
                    {!trainerHidden && (
                        <TrainerSprite avatarUrl={state.opponent?.avatarUrl} flipped hidden={trainerHidden} />
                    )}
                    {(trainerHidden && p2Active) && (
                        <PokemonSprite pet={p2Active} state={p2SpriteState} />
                    )}
                </div>

                {/* ── P1: Nameplate bottom-right + Pokemon bottom-left ─────────── */}
                <div className="absolute bottom-3 right-3 sm:bottom-4 sm:right-6 z-10">
                    {p1Active && <Nameplate pet={p1Active} flipped />}
                    <div className="mt-1.5 flex flex-col items-end gap-1">
                        <PartyBalls pets={state.p1Pets} flipped />
                        <div className="bg-black/50 border border-blue-500/50 rounded px-2 py-1 mt-1 inline-block">
                            <span className="font-pixel text-[8px] text-blue-300">{user?.username || 'You'}</span>
                        </div>
                    </div>
                </div>

                <div
                    className="absolute z-10 flex flex-col items-center"
                    style={{ left: '8%', bottom: '8%' }}
                >
                    {!trainerHidden && (
                        <TrainerSprite avatarUrl={user?.avatarUrl} hidden={trainerHidden} />
                    )}
                    {(trainerHidden && p1Active) && (
                        <PokemonSprite pet={p1Active} back state={p1SpriteState} />
                    )}
                </div>
            </div>

            {/* ── BATTLE UI PANEL ──────────────────────────────────────────────── */}
            <div className="absolute z-30 top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-full max-w-2xl px-4 flex flex-col drop-shadow-2xl">
                <div className="bg-[#0a0f18]/80 backdrop-blur-md p-4 rounded-xl border-2 border-purple-900/50 shadow-[0_4px_30px_rgba(168,85,247,0.15)]">
                    
                    {/* Score/timer row */}
                    {q && (
                        <div className="flex items-center gap-4 mb-3">
                            <div className="flex-1">
                                <ScoreTimer
                                    key={q.roundIndex}
                                    timeLimitMs={q.timeLimitMs}
                                    onElapsed={handleTimeup}
                                    answered={state.answered || state.phase === 'waiting'}
                                />
                            </div>
                            <div className={`shrink-0 px-2 py-1 rounded border font-pixel text-[9px]
                                ${(q.roundIndex ?? 0) >= 9
                                    ? 'border-red-500/60 bg-red-900/30 text-red-400 animate-pulse'
                                    : 'border-purple-600 bg-purple-900/40 text-purple-300'}`}>
                                {(q.roundIndex ?? 0) >= 9 ? '🔥 SUDDEN DEATH' : `Round ${(q.roundIndex ?? 0) + 1}`}
                            </div>
                            <div className="shrink-0 flex items-center gap-1.5">
                                <span className="font-press text-[10px] text-blue-400">{state.p1TotalScore}</span>
                                <span className="font-pixel text-[8px] text-gray-600">vs</span>
                                <span className="font-press text-[10px] text-red-400">{state.p2TotalScore}</span>
                            </div>
                        </div>
                    )}

                    {/* Waiting overlay inside UI panel */}
                    {state.phase === 'waiting' && state.totalRounds > 0 ? (
                         <div className="rounded border-2 border-purple-700/50 bg-[#0a0a1e]/90 p-6 my-2 flex flex-col items-center justify-center text-center">
                            <p className="font-pixel text-[10px] text-purple-300 mb-3 uppercase animate-pulse">
                                {state.waitingMessage}
                            </p>
                            <div className="flex gap-1 justify-center">
                                <span className="w-1.5 h-1.5 bg-purple-500 rounded-full animate-bounce"></span>
                                <span className="w-1.5 h-1.5 bg-purple-500 rounded-full animate-bounce" style={{ animationDelay: '0.15s' }}></span>
                                <span className="w-1.5 h-1.5 bg-purple-500 rounded-full animate-bounce" style={{ animationDelay: '0.3s' }}></span>
                            </div>
                        </div>
                    ) : q ? (
                        <>
                            {/* Question box */}
                            <div className="rounded border-2 border-gray-600 bg-[#060614] p-4 mb-3" style={{ boxShadow: '3px 3px 0 #00000066' }}>
                                {q.questionType === 'MultipleChoice' ? (
                                    <div className="text-center">
                                        <p className="font-pixel text-[9px] text-gray-500 uppercase tracking-widest mb-2">
                                            What is the meaning of:
                                        </p>
                                        <p className="font-press text-xl sm:text-2xl text-yellow-300 mb-1">
                                            {q.word}
                                        </p>
                                        {q.pronunciation && (
                                            <p className="font-noto text-gray-500 text-xs">/{q.pronunciation}/</p>
                                        )}
                                    </div>
                                ) : (
                                    <div>
                                        <p className="font-pixel text-[9px] text-gray-500 uppercase tracking-widest mb-2">
                                            Fill in the blank:
                                        </p>
                                        <p className="font-noto text-white text-sm sm:text-base text-center leading-relaxed">
                                            {q.questionPrompt}
                                        </p>
                                    </div>
                                )}
                            </div>

                            {/* Answer options */}
                            {q?.questionType === 'MultipleChoice' && q.options && (
                                <div className="grid grid-cols-2 gap-2 sm:gap-3">
                                    {q.options.map((opt, i) => {
                                        const letters = ['A', 'B', 'C', 'D'];
                                        const colors = [
                                            'hover:border-blue-400/70 hover:bg-blue-400/10',
                                            'hover:border-red-400/70 hover:bg-red-400/10',
                                            'hover:border-green-400/70 hover:bg-green-400/10',
                                            'hover:border-yellow-400/70 hover:bg-yellow-400/10',
                                        ];
                                        return (
                                            <button
                                                key={i}
                                                onClick={() => !state.answered && handleChoice(opt)}
                                                disabled={state.answered}
                                                className={`
                                                    relative p-3 rounded border-2 text-left font-noto text-xs sm:text-sm
                                                    transition-all duration-100 active:scale-95
                                                    ${state.answered
                                                        ? 'border-gray-700/40 text-gray-600 cursor-not-allowed bg-gray-900/40'
                                                        : `border-gray-600 bg-[#0a0a1e] text-white ${colors[i]}`
                                                    }
                                                `}
                                                style={{ boxShadow: state.answered ? 'none' : '2px 2px 0 #00000066' }}
                                            >
                                                <span className={`inline-block w-5 h-5 rounded text-center mr-2 font-pixel text-[9px] leading-5
                                                    ${state.answered ? 'bg-gray-700 text-gray-500' : 'bg-gray-700 text-yellow-300'}`}>
                                                    {letters[i]}
                                                </span>
                                                {opt}
                                            </button>
                                        );
                                    })}
                                </div>
                            )}

                            {q?.questionType === 'FillInBlank' && (
                                <FillInBlankInput
                                    disabled={state.answered}
                                    onSubmit={(ans, ms) => submitAnswer(ans, ms)}
                                />
                            )}
                        </>
                    ) : (
                        <div className="rounded border-2 border-gray-700 bg-[#0a0a1e] p-4 mb-3 flex flex-col items-center justify-center min-h-[120px]">
                            <span className="font-pixel text-xs text-purple-400 animate-pulse mb-3">
                                Preparing next round...
                            </span>
                        </div>
                    )}
                </div>
            </div>

            {/* Round overlay */}
            {showOverlay && state.lastRoundResult && (
                <RoundResultOverlay
                    result={state.lastRoundResult}
                    onDone={() => setShowOverlay(false)}
                    opponentLabel="OPPONENT"
                />
            )}
        </div>
    );
}
