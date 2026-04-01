// ── Types for Real-time Arena Battle ──────────────────────────────────────────

export interface PetStateDto {
    slotIndex: number;
    displayName: string;
    imageUrl?: string;
    maxHp: number;
    currentHp: number;
    isFainted: boolean;
    petType: string;
    secondaryPetType?: string;
}

export interface OpponentInfoDto {
    name: string;
    avatarUrl?: string;
    isBot: boolean;
}

export interface RoundQuestionDto {
    roundIndex: number;
    vocabularyId: number;
    word?: string;
    meaning?: string;
    pronunciation?: string;
    questionPrompt?: string;      // FillInBlank sentence with ___
    questionType: 'MultipleChoice' | 'FillInBlank';
    options?: string[];            // null for FillInBlank
    timeLimitMs: number;           // 10000
}

export interface BattleStartedDto {
    battleSessionId: number;
    totalRounds: number;
    p1Pets: PetStateDto[];
    p2Pets: PetStateDto[];
    firstQuestion: RoundQuestionDto;
    opponent: OpponentInfoDto;
}

export interface RoundResultDto {
    roundIndex: number;
    vocabularyId: number;
    correctAnswer: string;
    p1Score: number;
    p2Score: number;
    p1Correct: boolean;
    p2Correct: boolean;
    p1AnswerMs: number;
    p2AnswerMs: number;
    p1Answer?: string;
    p2Answer?: string;
    damageDealt: number;
    damagedPlayer: number;   // 1=P1 damaged, 2=P2 damaged, 0=draw
    typeMultiplier: number;
    typeEffectivenessText?: string;
    p1Pets: PetStateDto[];
    p2Pets: PetStateDto[];
    p1TotalScore: number;
    p2TotalScore: number;
}

export interface PvpEloResultDto {
    oldRating: number;
    newRating: number;
    ratingChange: number;
    oldTier: string;
    newTier: string;
}

export interface BattleEndedDto {
    battleSessionId: number;
    p1Won: boolean;
    p1TotalScore: number;
    p2TotalScore: number;
    p1CorrectCount: number;
    p2CorrectCount: number;
    totalRounds: number;
    xpEarned: number;
    badgeEarned: boolean;
    badgeName?: string;
    badgeImageUrl?: string;
    pvpEloResult?: PvpEloResultDto;
}
