export interface PetDto {
  isOwned: boolean;
  id: number;
  name: string;
  description: string;
  imageUrl: string;
  rarity: string;
  type: string;
  order: number
}

export interface PetDetailDto {
  id: number;
  name: string;
  description: string;
  imageUrl: string;
  rarity: string;
  type: string;
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