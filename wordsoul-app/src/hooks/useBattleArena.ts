import { useCallback, useEffect, useRef, useState } from 'react';
import type { HubConnection } from '@microsoft/signalr';
import {
    connectBattleHub,
    disconnectBattleHub,
    sendPlayerReady,
    sendSubmitAnswer,
} from '../services/battleHub';
import type {
    BattleStartedDto,
    BattleEndedDto,
    PetStateDto,
    RoundQuestionDto,
    RoundResultDto,
} from '../types/BattleArenaTypes';
import { ACCESS_TOKEN_KEY, getToken } from '../helpers/authHelpers';

type Phase = 'connecting' | 'battle' | 'roundResult' | 'ended' | 'error';

interface BattleState {
    phase: Phase;
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
}

export function useBattleArena(sessionId: number) {
    const connRef = useRef<HubConnection | null>(null);

    const [state, setState] = useState<BattleState>({
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
    });

    useEffect(() => {
        let mounted = true;
        const token = getToken(ACCESS_TOKEN_KEY) ?? '';

        connectBattleHub(token, {
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

            onError: (msg: string) => {
                if (!mounted) return;
                setState(s => ({ ...s, phase: 'error', errorMsg: msg }));
            },
        }).then(conn => {
            if (!mounted) return;
            connRef.current = conn;
            sendPlayerReady(conn, sessionId);
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
        setState(s => ({ ...s, answered: true }));
        sendSubmitAnswer(connRef.current!, {
            battleSessionId: sessionId,
            roundIndex: state.currentQuestion!.roundIndex,
            vocabularyId: state.currentQuestion!.vocabularyId,
            answer,
            elapsedMs,
        });
    }, [sessionId, state.currentQuestion, state.answered]);

    return { state, submitAnswer };
}
