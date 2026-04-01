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
    p1Pets: PetStateDto[];
    p2Pets: PetStateDto[];
    p1TotalScore: number;
    p2TotalScore: number;
    battleResult: BattleEndedDto | null;
    errorMsg: string;
    answered: boolean;
    opponent: OpponentInfoDto | null;
    waitingMessage: string;
}

export function usePvpBattle(sessionId: number) {
    const connRef = useRef<HubConnection | null>(null);

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
        waitingMessage: 'Connecting to battle server...'
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
                    opponent: {
                        name: data.opponentName,
                        avatarUrl: data.avatarUrl || '',
                        isBot: false
                    }
                }));
            },

            onBattleStarted: (data: BattleStartedDto) => {
                if (!mounted) return;
                setState(s => ({
                    ...s,
                    phase: 'battle',
                    totalRounds: data.totalRounds,
                    currentQuestion: data.firstQuestion,
                    p1Pets: data.p1Pets,
                    p2Pets: data.p2Pets,
                    answered: false,
                    opponent: data.opponent,
                }));
            },

            onRoundResult: (data: RoundResultDto) => {
                if (!mounted) return;
                setState(s => ({
                    ...s,
                    phase: 'roundResult',
                    lastRoundResult: data,
                    p1Pets: data.p1Pets,
                    p2Pets: data.p2Pets,
                    p1TotalScore: data.p1TotalScore,
                    p2TotalScore: data.p2TotalScore,
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
                setState(s => ({ ...s, phase: 'ended', battleResult: data }));
            },

            onOpponentForfeited: (data: BattleEndedDto) => {
                if (!mounted) return;
                setState(s => ({
                    ...s,
                    phase: 'opponentLeft',
                    battleResult: data,
                    errorMsg: 'Opponent has disconnected.'
                }));
            },

            onError: (msg: string) => {
                if (!mounted) return;
                setState(s => ({ ...s, phase: 'error', errorMsg: msg }));
            },
        }).then(conn => {
            if (!mounted) return;
            connRef.current = conn;
            // Join PvP room
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
