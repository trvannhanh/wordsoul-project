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

// Ánh xạ PetType với các class background
export const typeBackgrounds: Record<string, string> = {
  Nomadica: "bg-[url('https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759167166/8-bit-graphics-pixels-scene-with-ocean-waves_p0rkwl.jpg')]",
  Dynamora: "bg-[url('https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759167169/8-bit-graphics-pixels-scene-with-mountains_1_xo5aqr.jpg')]",
  Adornica: "bg-[url('https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759167174/8-bit-graphics-pixels-scene-with-mountains_r1e9j2.jpg')]",
  Velocira: "bg-[url('https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759167229/9259344_smrvda.jpg')]",
  Substitua: "bg-[url('https://res.cloudinary.com/dqpkxxzaf/image/upload/v1757662431/wp7159979-anime-pixel-art-wallpapers_kxhy41.png')]",
  Connectara: "bg-[url('https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759167169/jungle-landscape-pixel-art-style_nbiexd.jpg')]",
  Preposita: "bg-[url('https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759168054/R_3_v6gi8m.jpg')]",
  Exclamora: "bg-[url('https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759168052/OIP_11_k6a52o.webp')]",
};

// Ánh xạ PetRarity với các class border
export const rarityBorders: Record<string, string> = {
  Common: "border-gray-500",
  Uncommon: "border-green-500",
  Rare: "border-blue-500",
  Epic: "border-purple-500",
  Legendary: "border-yellow-500",
};