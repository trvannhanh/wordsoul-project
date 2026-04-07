import { useState, useEffect, useRef, useCallback } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { connectMatchmakingHub, disconnectBattleHub } from '../../services/battleHub';
import { joinMatchmakingQueue, leaveMatchmakingQueue } from '../../services/pvp';
import { ACCESS_TOKEN_KEY, getToken } from '../../helpers/authHelpers';

// ─── Particle background for ambiance ───────────────────────────────────────
function ParticleBg() {
    return (
        <div className="absolute inset-0 overflow-hidden pointer-events-none">
            {[...Array(20)].map((_, i) => (
                <div key={i}
                    className="absolute rounded-full opacity-20 animate-pulse"
                    style={{
                        width: `${2 + (i % 4)}px`,
                        height: `${2 + (i % 4)}px`,
                        background: i % 3 === 0 ? '#a855f7' : i % 3 === 1 ? '#3b82f6' : '#ec4899',
                        left: `${(i * 17 + 5) % 100}%`,
                        top: `${(i * 23 + 10) % 100}%`,
                        animationDelay: `${i * 0.3}s`,
                        animationDuration: `${2 + (i % 3)}s`,
                    }}
                />
            ))}
        </div>
    );
}

type MatchState = 'connecting' | 'searching' | 'found' | 'error' | 'timeout' | 'cancelled';

export default function PvpMatchmaking() {
    const navigate = useNavigate();
    const location = useLocation();

    // Passed from PvpPetSelector
    const selectedPetIds: number[] = location.state?.selectedPetIds ?? [];

    const [matchState, setMatchState] = useState<MatchState>('connecting');
    const [elapsedSeconds, setElapsedSeconds] = useState(0);
    const [errorMsg, setErrorMsg] = useState('');

    const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);
    const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
    const queueIdRef = useRef('');
    const matchStateRef = useRef<MatchState>('connecting');

    // Keep ref in sync with state
    const setMatchStateSynced = useCallback((s: MatchState) => {
        matchStateRef.current = s;
        setMatchState(s);
    }, []);

    const stopTimers = useCallback(() => {
        if (timerRef.current) clearInterval(timerRef.current);
        if (timeoutRef.current) clearTimeout(timeoutRef.current);
    }, []);

    const handleCancel = useCallback(async () => {
        stopTimers();
        setMatchStateSynced('cancelled');
        if (queueIdRef.current) {
            try { await leaveMatchmakingQueue(queueIdRef.current); } catch { /* ok */ }
        }
        disconnectBattleHub();
        navigate('/pvp');
    }, [navigate, stopTimers, setMatchStateSynced]);

    useEffect(() => {
        if (selectedPetIds.length !== 3) {
            navigate('/pvp');
            return;
        }

        let mounted = true;
        const token = getToken(ACCESS_TOKEN_KEY) ?? '';

        const start = async () => {
            try {
                // 1) Connect to BattleHub
                const { connectionId } = await connectMatchmakingHub(
                    token,
                    (data) => {
                        // MatchFound — navigate to arena
                        if (!mounted) return;
                        stopTimers();
                        setMatchStateSynced('found');
                        setTimeout(() => navigate(`/pvp/arena/${data.sessionId}`), 1200);
                    },
                    (data) => {
                        if (!mounted) return;
                        stopTimers();
                        setMatchStateSynced('error');
                        setErrorMsg(data.error);
                        disconnectBattleHub();
                    },
                    () => {
                        if (!mounted) return;
                        const cur = matchStateRef.current;
                        if (cur !== 'found' && cur !== 'cancelled') {
                            setMatchStateSynced('error');
                            setErrorMsg('Connection lost. Please try again.');
                        }
                    }
                );

                if (!mounted) return;

                // 2) Join matchmaking queue
                const result = await joinMatchmakingQueue(selectedPetIds, connectionId);
                if (!mounted) return;

                queueIdRef.current = result.queueId;
                setMatchStateSynced('searching');

                // 3) Start elapsed timer
                timerRef.current = setInterval(() => {
                    setElapsedSeconds(s => s + 1);
                }, 1000);

                // 4) 60s timeout
                timeoutRef.current = setTimeout(() => {
                    if (!mounted) return;
                    stopTimers();
                    setMatchStateSynced('timeout');
                    leaveMatchmakingQueue(queueIdRef.current).catch(() => {});
                    disconnectBattleHub();
                }, 60_000);

            } catch (err: unknown) {
                if (!mounted) return;
                const msg = err instanceof Error ? err.message : 'Failed to connect.';
                setMatchStateSynced('error');
                setErrorMsg(msg);
            }
        };

        start();

        return () => {
            mounted = false;
            stopTimers();
        };
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const formatTime = (s: number) =>
        `${String(Math.floor(s / 60)).padStart(2, '0')}:${String(s % 60).padStart(2, '0')}`;

    // ── Searching UI ──────────────────────────────────────────────────────────
    if (matchState === 'searching') {
        return (
            <div className="min-h-screen text-white flex flex-col items-center justify-center relative overflow-hidden"
                style={{ background: 'linear-gradient(135deg, rgb(2,6,23) 0%, rgba(88,28,135,0.15) 50%, rgb(2,6,23) 100%)' }}>
                <ParticleBg />

                <div className="relative z-10 flex flex-col items-center gap-6 px-6">

                    {/* Animated radar rings */}
                    <div className="relative w-40 h-40 flex items-center justify-center">
                        {[0, 1, 2].map(i => (
                            <div key={i} className="absolute rounded-full border-2 border-purple-500/40 animate-ping"
                                style={{
                                    width: `${80 + i * 40}px`,
                                    height: `${80 + i * 40}px`,
                                    animationDelay: `${i * 0.5}s`,
                                    animationDuration: '2s',
                                }}
                            />
                        ))}
                        {/* Center icon */}
                        <div className="w-20 h-20 rounded-full border-2 border-purple-400 bg-purple-900/40 flex items-center justify-center text-4xl shadow-[0_0_30px_rgba(168,85,247,0.6)]">
                            ⚔️
                        </div>
                    </div>

                    {/* Title */}
                    <div className="text-center">
                        <h1 className="font-press text-2xl text-purple-400 mb-2"
                            style={{ textShadow: '0 0 20px rgba(168,85,247,0.8)' }}>
                            FINDING MATCH
                        </h1>
                        <p className="font-pixel text-xs text-gray-400 animate-pulse">
                            Searching for an opponent of similar rating...
                        </p>
                    </div>

                    {/* Timer */}
                    <div className="font-press text-4xl text-white tabular-nums"
                        style={{ textShadow: '0 0 10px rgba(168,85,247,0.5)' }}>
                        {formatTime(elapsedSeconds)}
                    </div>

                    {/* Range expand info */}
                    <div className="text-center bg-gray-900/60 rounded-xl border border-gray-700/50 px-6 py-3">
                        <p className="font-pixel text-[10px] text-gray-500 mb-1">ELO RANGE</p>
                        <p className="font-press text-sm text-yellow-400">
                            ±{200 + Math.floor(elapsedSeconds / 10) * 50}
                        </p>
                        {elapsedSeconds >= 10 && (
                            <p className="font-pixel text-[9px] text-gray-500 mt-1 animate-fade-in">
                                Range expanded every 10 seconds
                            </p>
                        )}
                    </div>

                    {/* Pokemon team preview */}
                    <div className="flex gap-3">
                        {selectedPetIds.map((_, i) => (
                            <div key={i} className="w-10 h-10 rounded-full bg-purple-900/40 border-2 border-purple-500/60
                                flex items-center justify-center text-lg shadow-[0_0_8px_rgba(168,85,247,0.4)]">
                                🎯
                            </div>
                        ))}
                    </div>

                    {/* Cancel */}
                    <button onClick={handleCancel}
                        className="mt-2 px-8 py-3 rounded-xl font-pixel text-xs text-gray-400 border border-gray-700
                            hover:text-white hover:border-red-500/60 hover:bg-red-900/20 transition-all">
                        CANCEL SEARCH
                    </button>
                </div>
            </div>
        );
    }

    // ── Match Found ───────────────────────────────────────────────────────────
    if (matchState === 'found') {
        return (
            <div className="min-h-screen text-white flex flex-col items-center justify-center"
                style={{ background: 'rgb(2,6,23)' }}>
                <ParticleBg />
                <div className="relative z-10 flex flex-col items-center gap-4">
                    <div className="text-7xl animate-bounce">⚔️</div>
                    <h1 className="font-press text-3xl text-green-400"
                        style={{ textShadow: '0 0 20px rgba(74,222,128,0.8)' }}>
                        MATCH FOUND!
                    </h1>
                    <p className="font-pixel text-xs text-gray-400 animate-pulse">
                        Entering arena...
                    </p>
                </div>
            </div>
        );
    }

    // ── Connecting ────────────────────────────────────────────────────────────
    if (matchState === 'connecting') {
        return (
            <div className="min-h-screen text-white flex flex-col items-center justify-center gap-4"
                style={{ background: 'rgb(2,6,23)' }}>
                <div className="w-12 h-12 rounded-full border-4 border-purple-500 border-t-transparent animate-spin" />
                <p className="font-pixel text-sm text-gray-400 animate-pulse">Connecting to arena...</p>
            </div>
        );
    }

    // ── Timeout ───────────────────────────────────────────────────────────────
    if (matchState === 'timeout') {
        return (
            <div className="min-h-screen text-white flex flex-col items-center justify-center gap-6 px-6"
                style={{ background: 'rgb(2,6,23)' }}>
                <div className="text-6xl">⏰</div>
                <h1 className="font-press text-xl text-yellow-400 text-center">MATCHMAKING TIMEOUT</h1>
                <p className="font-noto text-gray-400 text-sm text-center max-w-xs">
                    No opponent found within 60 seconds. Try again or create a room manually.
                </p>
                <div className="flex flex-col gap-3 w-full max-w-xs">
                    <button onClick={() => navigate('/pvp/pets?mode=matchmaking', { state: { selectedPetIds } })}
                        className="w-full py-4 rounded-xl font-press text-sm bg-purple-600 hover:bg-purple-500 text-white transition-all">
                        TRY AGAIN
                    </button>
                    <button onClick={() => navigate('/pvp')}
                        className="w-full py-3 rounded-xl font-pixel text-xs text-gray-400 border border-gray-700 hover:bg-gray-900">
                        BACK TO LOBBY
                    </button>
                </div>
            </div>
        );
    }

    // ── Error ─────────────────────────────────────────────────────────────────
    return (
        <div className="min-h-screen text-white flex flex-col items-center justify-center gap-6 px-6"
            style={{ background: 'rgb(2,6,23)' }}>
            <div className="text-6xl">💥</div>
            <h1 className="font-press text-xl text-red-400 text-center">CONNECTION ERROR</h1>
            <p className="font-noto text-gray-400 text-sm text-center max-w-xs">{errorMsg || 'An unexpected error occurred.'}</p>
            <div className="flex flex-col gap-3 w-full max-w-xs">
                <button onClick={() => navigate('/pvp/pets?mode=matchmaking', { state: { selectedPetIds } })}
                    className="w-full py-4 rounded-xl font-press text-sm bg-red-700 hover:bg-red-600 text-white transition-all">
                    RETRY
                </button>
                <button onClick={() => navigate('/pvp')}
                    className="w-full py-3 rounded-xl font-pixel text-xs text-gray-400 border border-gray-700 hover:bg-gray-900">
                    BACK TO LOBBY
                </button>
            </div>
        </div>
    );
}
