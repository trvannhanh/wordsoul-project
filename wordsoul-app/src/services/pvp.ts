import { authApi } from './api';

export interface PvpRoomCreatedDto {
    sessionId: number;
    roomCode: string;
}

export interface PvpRatingDto {
    userId: number;
    username: string;
    pvpRating: number;
    pvpWins: number;
    pvpLosses: number;
    tier: string;
}

export interface QueueJoinedDto {
    queueId: string;
    status: string;
}

// ── Room Code flow ────────────────────────────────────────────────────────────

export const createPvpSession = async (selectedPetIds: number[]): Promise<PvpRoomCreatedDto> => {
    const res = await authApi.post<PvpRoomCreatedDto>('/pvp/create', { selectedPetIds });
    return res.data;
};

export const joinPvpSession = async (
    roomCode: string,
    selectedPetIds: number[]
): Promise<{ sessionId: number }> => {
    const res = await authApi.post<{ sessionId: number }>('/pvp/join', { roomCode, selectedPetIds });
    return res.data;
};

// ── Matchmaking Queue flow ────────────────────────────────────────────────────

export const joinMatchmakingQueue = async (
    selectedPetIds: number[],
    connectionId: string
): Promise<QueueJoinedDto> => {
    const res = await authApi.post<QueueJoinedDto>('/pvp/queue/join', {
        selectedPetIds,
        connectionId,
    });
    return res.data;
};

export const leaveMatchmakingQueue = async (queueId: string): Promise<void> => {
    await authApi.delete(`/pvp/queue/leave?queueId=${queueId}`);
};

// ── Rating ────────────────────────────────────────────────────────────────────

export const getMyPvpRating = async (): Promise<PvpRatingDto> => {
    const res = await authApi.get<PvpRatingDto>('/pvp/rating');
    return res.data;
};
