import * as signalR from '@microsoft/signalr';
import type {
    BattleStartedDto,
    RoundResultDto,
    RoundQuestionDto,
    BattleEndedDto,
} from '../types/BattleArenaTypes';

type EventCallbacks = {
    onBattleStarted: (data: BattleStartedDto) => void;
    onRoundResult: (data: RoundResultDto) => void;
    onNextQuestion: (data: RoundQuestionDto) => void;
    onBattleEnded: (data: BattleEndedDto) => void;
    onError: (msg: string) => void;
    // PvP events
    onOpponentJoined?: (data: { opponentName: string; avatarUrl?: string; opponentRating: number }) => void;
    onWaitingOpponent?: (data: { message: string }) => void;
    onOpponentForfeited?: (data: BattleEndedDto) => void;
};

let connection: signalR.HubConnection | null = null;

export async function connectBattleHub(
    token: string,
    callbacks: EventCallbacks
): Promise<signalR.HubConnection> {
    if (connection) await connection.stop();

    const apiBase = (import.meta.env.VITE_API_URL || '').replace(/\/api$/, '');

    connection = new signalR.HubConnectionBuilder()
        .withUrl(`${apiBase}/battleHub`, {
            accessTokenFactory: () => token,
            transport: signalR.HttpTransportType.WebSockets,
            skipNegotiation: false,
        })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    connection.on('BattleStarted', callbacks.onBattleStarted);
    connection.on('RoundResult', callbacks.onRoundResult);
    connection.on('NextQuestion', callbacks.onNextQuestion);
    connection.on('BattleEnded', callbacks.onBattleEnded);
    connection.onclose(() => callbacks.onError('Connection closed.'));

    // PvP events (optional)
    if (callbacks.onOpponentJoined)
        connection.on('OpponentJoined', callbacks.onOpponentJoined);
    if (callbacks.onWaitingOpponent)
        connection.on('WaitingOpponent', callbacks.onWaitingOpponent);
    if (callbacks.onOpponentForfeited)
        connection.on('OpponentForfeited', callbacks.onOpponentForfeited);

    await connection.start();
    return connection;
}

export async function disconnectBattleHub() {
    if (connection) {
        await connection.stop();
        connection = null;
    }
}

export async function sendPlayerReady(conn: signalR.HubConnection, sessionId: number) {
    await conn.invoke('PlayerReady', sessionId);
}

export async function sendSubmitAnswer(
    conn: signalR.HubConnection,
    payload: {
        battleSessionId: number;
        roundIndex: number;
        vocabularyId: number;
        answer: string;
        elapsedMs: number;
    }
) {
    await conn.invoke('SubmitAnswer', payload);
}

// ── PvP Hub methods ─────────────────────────────────────────────────────────

export async function sendPlayerReadyPvP(conn: signalR.HubConnection, sessionId: number) {
    await conn.invoke('PlayerReadyPvP', sessionId);
}

export async function sendSubmitPvpAnswer(
    conn: signalR.HubConnection,
    payload: {
        battleSessionId: number;
        roundIndex: number;
        vocabularyId: number;
        answer: string;
        elapsedMs: number;
    }
) {
    await conn.invoke('SubmitPvpAnswer', payload);
}
