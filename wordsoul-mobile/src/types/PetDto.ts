export interface PetDto {
  isOwned: boolean;
  id: number;
  name: string;
  description: string;
  imageUrl: string;
  rarity: string;
  type: string;
  secondaryType?: string | null;
  order: number;
}

export interface PetDetailDto {
  id: number;
  name: string;
  description: string;
  imageUrl: string;
  rarity: string;
  type: string;
  secondaryType?: string | null;
  level: number | null;
  experience: number | null;
  isFavorite: boolean | null;
  isActive: boolean;
  acquiredAt: string | null;
  baseFormId: number | null;
  nextEvolutionId: number | null;
  requiredLevel: number | null;
}

export interface UpgradePetResponseDto {
  petId: number;
  experience: number;
  level: number;
  isLevelUp: boolean;
  isEvolved: boolean;
  ap: number;
}

// Màu background theo PetType
export const typeColors: Record<string, string> = {
  Nomadica: '#0ea5e9',
  Dynamora: '#ef4444',
  Adornica: '#ec4899',
  Velocira: '#f59e0b',
  Substitua: '#8b5cf6',
  Connectara: '#22c55e',
  Preposita: '#6366f1',
  Exclamora: '#f97316',
};

export const rarityColors: Record<string, string> = {
  Common: '#6b7280',
  Uncommon: '#22c55e',
  Rare: '#3b82f6',
  Epic: '#8b5cf6',
  Legendary: '#f59e0b',
};
