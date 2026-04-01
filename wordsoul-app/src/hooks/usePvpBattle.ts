import { useCallback, useEffect, useRef, useState } from 'react';
import type { HubConnection } from '@microsoft/signalr';
import {
    connectBattleHub,
    disconnectBattleHub,
    sendPlayerReadyPvP,
    sendSubmitPvpAnswer,
} from '../services/battleHub';
import type {
    BattleStartedDto,
    BattleEndedDto,
    PetStateDto,
    PvpEloResultDto,
    RoundQuestionDto,
    RoundResultDto,
    OpponentInfoDto
} from '../types/BattleArenaTypes';
import { ACCESS_TOKEN_KEY, getToken } from '../helpers/authHelpers';

type PvpPhase = 'connecting' | 'waiting' | 'battle' | 'roundResult' | 'ended' | 'error' | 'opponentLeft';

interface PvpBattleState {
    phase: PvpPhase;
    sessionId: number;
    totalRounds: number;
    currentQuestion: RoundQuestionDto | null;
    lastRoundResult: RoundResultDto | null;
    // p1Pets = MY SIDE (bottom), p2Pets = OPPONENT SIDE (top) — luôn từ góc nhìn client hiện tại
    p1Pets: PetStateDto[];
    p2Pets: PetStateDto[];
    p1TotalScore: number;
    p2TotalScore: number;
    battleResult: BattleEndedDto | null;
    errorMsg: string;
    answered: boolean;
    opponent: OpponentInfoDto | null;
    waitingMessage: string;
    isP1: boolean;  // true = client này là Challenger trong DB
}

// Swap damagedPlayer 1↔2 cho góc nhìn P2
function flipDamagedPlayer(d: number): number {
    if (d === 1) return 2;
    if (d === 2) return 1;
    return 0;
}

// Build RoundResultDto từ góc nhìn của client hiện tại (swap nếu cần)
function perspectiveRoundResult(data: RoundResultDto, isP1: boolean): RoundResultDto {
    if (isP1) return data;
    return {
        ...data,
        p1Score: data.p2Score,
        p2Score: data.p1Score,
        p1Correct: data.p2Correct,
        p2Correct: data.p1Correct,
        p1AnswerMs: data.p2AnswerMs,
        p2AnswerMs: data.p1AnswerMs,
        p1Answer: data.p2Answer,
        p2Answer: data.p1Answer,
        p1Pets: data.p2Pets,
        p2Pets: data.p1Pets,
        p1TotalScore: data.p2TotalScore,
        p2TotalScore: data.p1TotalScore,
        damagedPlayer: flipDamagedPlayer(data.damagedPlayer),
    };
}

export function usePvpBattle(sessionId: number) {
    const connRef = useRef<HubConnection | null>(null);
    const isP1Ref = useRef<boolean>(false); // dùng trong callbacks vì state là stale

    const [state, setState] = useState<PvpBattleState>({
        phase: 'connecting',
        sessionId,
        totalRounds: 0,
        currentQuestion: null,
        lastRoundResult: null,
        p1Pets: [],
        p2Pets: [],
        p1TotalScore: 0,
        p2TotalScore: 0,
        battleResult: null,
        errorMsg: '',
        answered: false,
        opponent: null,
        waitingMessage: 'Connecting to battle server...',
        isP1: false,
    });

    useEffect(() => {
        let mounted = true;
        const token = getToken(ACCESS_TOKEN_KEY) ?? '';

        connectBattleHub(token, {
            onWaitingOpponent: (data: { message: string }) => {
                if (!mounted) return;
                setState(s => ({ ...s, phase: 'waiting', waitingMessage: data.message }));
            },

            onOpponentJoined: (data: { opponentName: string; avatarUrl?: string; opponentRating: number }) => {
                if (!mounted) return;
                setState(s => ({
                    ...s,
                    opponent: { name: data.opponentName, avatarUrl: data.avatarUrl, isBot: false }
                }));
            },

            onBattleStarted: (data: BattleStartedDto) => {
                if (!mounted) return;
                const myIsP1 = data.isP1 ?? false;
                isP1Ref.current = myIsP1;

                // P1: p1Pets = data.p1Pets (my), p2Pets = data.p2Pets (opponent)
                // P2: p1Pets = data.p2Pets (my), p2Pets = data.p1Pets (opponent) — swap!
                const myPets  = myIsP1 ? data.p1Pets : data.p2Pets;
                const theirPets = myIsP1 ? data.p2Pets : data.p1Pets;

                setState(s => ({
                    ...s,
                    phase: 'battle',
                    isP1: myIsP1,
                    totalRounds: data.totalRounds,
                    currentQuestion: data.firstQuestion,
                    p1Pets: myPets,
                    p2Pets: theirPets,
                    answered: false,
                    opponent: data.opponent,
                }));
            },

            onRoundResult: (data: RoundResultDto) => {
                if (!mounted) return;
                // Swap data theo góc nhìn của client
                const result = perspectiveRoundResult(data, isP1Ref.current);
                setState(s => ({
                    ...s,
                    phase: 'roundResult',
                    lastRoundResult: result,
                    p1Pets: result.p1Pets,
                    p2Pets: result.p2Pets,
                    p1TotalScore: result.p1TotalScore,
                    p2TotalScore: result.p2TotalScore,
                    answered: true,
                }));
            },

            onNextQuestion: (data: RoundQuestionDto) => {
                if (!mounted) return;
                setState(s => ({
                    ...s,
                    phase: 'battle',
                    currentQuestion: data,
                    lastRoundResult: null,
                    answered: false,
                }));
            },

            onBattleEnded: (data: BattleEndedDto) => {
                if (!mounted) return;
                // Tạo kết quả từ góc nhìn của client (iWon)
                const myIsP1 = isP1Ref.current;
                const iWon = myIsP1 ? data.p1Won : !data.p1Won;
                const myScore = myIsP1 ? data.p1TotalScore : data.p2TotalScore;
                const theirScore = myIsP1 ? data.p2TotalScore : data.p1TotalScore;
                const myCorrect = myIsP1 ? data.p1CorrectCount : data.p2CorrectCount;
                const myElo: PvpEloResultDto | undefined = myIsP1 ? data.eloResult : data.p2EloResult;

                const adjusted: BattleEndedDto = {
                    ...data,
                    p1Won: iWon,                  // frontend đọc p1Won = "tôi thắng không?"
                    p1TotalScore: myScore,
                    p2TotalScore: theirScore,
                    p1CorrectCount: myCorrect,
                    p2CorrectCount: myIsP1 ? data.p2CorrectCount : data.p1CorrectCount,
                    eloResult: myElo,
                    p2EloResult: undefined,        // không cần ở client
                };
                setState(s => ({ ...s, phase: 'ended', battleResult: adjusted }));
            },

            onOpponentForfeited: (data: BattleEndedDto) => {
                if (!mounted) return;
                // Nếu opponent forfeited thì tôi thắng
                const myIsP1 = isP1Ref.current;
                const iWon = myIsP1 ? data.p1Won : !data.p1Won;
                const myElo: PvpEloResultDto | undefined = myIsP1 ? data.eloResult : data.p2EloResult;

                const adjusted: BattleEndedDto = {
                    ...data,
                    p1Won: iWon,
                    p1TotalScore: myIsP1 ? data.p1TotalScore : data.p2TotalScore,
                    p2TotalScore: myIsP1 ? data.p2TotalScore : data.p1TotalScore,
                    p1CorrectCount: myIsP1 ? data.p1CorrectCount : data.p2CorrectCount,
                    eloResult: myElo,
                    p2EloResult: undefined,
                };
                setState(s => ({
                    ...s,
                    phase: 'opponentLeft',
                    battleResult: adjusted,
                    errorMsg: 'Opponent has disconnected.',
                }));
            },

            onError: (msg: string) => {
                if (!mounted) return;
                setState(s => ({ ...s, phase: 'error', errorMsg: msg }));
            },
        }).then(conn => {
            if (!mounted) return;
            connRef.current = conn;
            sendPlayerReadyPvP(conn, sessionId);
        }).catch(err => {
            if (!mounted) return;
            setState(s => ({ ...s, phase: 'error', errorMsg: String(err) }));
        });

        return () => {
            mounted = false;
            disconnectBattleHub();
        };
    }, [sessionId]);

    const submitAnswer = useCallback((answer: string, elapsedMs: number) => {
        if (!connRef.current || !state.currentQuestion || state.answered) return;
        setState(s => ({ ...s, answered: true, phase: 'waiting', waitingMessage: 'Waiting for opponent to answer...' }));
        sendSubmitPvpAnswer(connRef.current!, {
            battleSessionId: sessionId,
            roundIndex: state.currentQuestion!.roundIndex,
            vocabularyId: state.currentQuestion!.vocabularyId,
            answer,
            elapsedMs,
        });
    }, [sessionId, state.currentQuestion, state.answered]);

    return { state, submitAnswer };
}
