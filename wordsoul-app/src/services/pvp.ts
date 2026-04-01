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

export const getMyPvpRating = async (): Promise<PvpRatingDto> => {
    const res = await authApi.get<PvpRatingDto>('/pvp/rating');
    return res.data;
};
