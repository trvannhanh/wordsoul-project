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

// ── Query DTOs ────────────────────────────────────────────────────────────────

export interface BattleHistoryEntryDto {
    sessionId: number;
    type: string;
    status: string;
    startedAt: string;
    completedAt?: string;
    totalQuestions: number;
    currentRound: number;
    challengerUserId: number;
    challengerUsername: string;
    challengerCorrect: number;
    challengerTotalScore: number;
    opponentUserId?: number;
    opponentName: string;
    opponentCorrect: number;
    opponentTotalScore: number;
    challengerWon?: boolean;
    isCurrentUserP1: boolean;
    currentUserWon: boolean;
}

export interface PetHistoryDto {
    displayName: string;
    imageUrl?: string;
    petType: string;
    secondaryPetType?: string;
    maxHp: number;
    currentHp: number;
    isFainted: boolean;
}

export interface PlayerHistoryDetailDto {
    userId?: number;
    name: string;
    correctCount: number;
    totalScore: number;
    selectedPets: PetHistoryDto[];
}

export interface RoundHistoryDetailDto {
    roundIndex: number;
    word: string;
    meaning: string;
    pronunciation?: string;
    p1Answer?: string;
    p1Correct: boolean;
    p1AnswerMs?: number;
    p1Score?: number;
    p2Answer?: string;
    p2Correct: boolean;
    p2AnswerMs?: number;
    p2Score?: number;
    damageDealt: number;
    damagedPlayer: number;
    typeMultiplier: number;
}

export interface BattleHistoryDetailDto {
    sessionId: number;
    type: string;
    status: string;
    startedAt: string;
    completedAt?: string;
    challengerWon?: boolean;
    isCurrentUserP1: boolean;
    currentUserWon: boolean;
    p1: PlayerHistoryDetailDto;
    p2: PlayerHistoryDetailDto;
    rounds: RoundHistoryDetailDto[];
}

export interface PvpLobbyRoomDto {
    sessionId: number;
    roomCode: string;
    hostUserId: number;
    hostUsername: string;
    hostRating: number;
    createdAt: string;
}

export interface PvpLeaderboardEntryDto {
    rank: number;
    userId: number;
    userName: string;
    avatarUrl?: string;
    pvpRating: number;
    wins: number;
    losses: number;
    totalGames: number;
    winRate: number;
}

export interface BattleHistoryPageDto {
    items: BattleHistoryEntryDto[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
}

// ── Waiting Rooms ─────────────────────────────────────────────────────────────

export const getWaitingPvpRooms = async (): Promise<PvpLobbyRoomDto[]> => {
    const res = await authApi.get<PvpLobbyRoomDto[]>('/pvp/rooms');
    return res.data;
};

// ── PvP Leaderboard ───────────────────────────────────────────────────────────

export const getPvpLeaderboard = async (top: number = 50): Promise<PvpLeaderboardEntryDto[]> => {
    const res = await authApi.get<PvpLeaderboardEntryDto[]>(`/pvp/leaderboard?top=${top}`);
    return res.data;
};

// ── Battle History ────────────────────────────────────────────────────────────

export const getBattleHistory = async (
    type?: string,
    page: number = 1,
    pageSize: number = 20,
    gymLeaderId?: number
): Promise<BattleHistoryPageDto> => {
    let url = `/arena/history?page=${page}&pageSize=${pageSize}`;
    if (type) url += `&type=${type}`;
    if (gymLeaderId) url += `&gymLeaderId=${gymLeaderId}`;
    const res = await authApi.get<BattleHistoryPageDto>(url);
    return res.data;
};

export const getBattleHistoryDetail = async (sessionId: number): Promise<BattleHistoryDetailDto> => {
    const res = await authApi.get<BattleHistoryDetailDto>(`/arena/history/${sessionId}`);
    return res.data;
};
