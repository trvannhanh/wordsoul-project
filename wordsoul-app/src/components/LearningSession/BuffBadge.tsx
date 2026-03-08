import { useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import type { PetDto } from "../../types/PetDto";

interface BuffBadgeProps {
  buffPet: PetDto | null;
  buffName?: string;
  buffDescription?: string;
  buffIcon?: string;
  petXpMultiplier?: number;
  petCatchBonus?: number;
  petHintShield?: boolean;
  petReducePenalty?: boolean;
}

const rarityColors: Record<string, { border: string; text: string; bg: string; glow: string }> = {
  Common: { border: "border-gray-400", text: "text-gray-300", bg: "bg-gray-700", glow: "shadow-gray-500/40" },
  Uncommon: { border: "border-green-400", text: "text-green-300", bg: "bg-green-900", glow: "shadow-green-500/40" },
  Rare: { border: "border-blue-400", text: "text-blue-300", bg: "bg-blue-900", glow: "shadow-blue-500/40" },
  Epic: { border: "border-purple-400", text: "text-purple-300", bg: "bg-purple-900", glow: "shadow-purple-500/40" },
  Legendary: { border: "border-yellow-400", text: "text-yellow-300", bg: "bg-yellow-900", glow: "shadow-yellow-500/40" },
};

const typeColors: Record<string, { bg: string; text: string }> = {
  Fire: { bg: "bg-orange-600", text: "text-orange-100" },
  Water: { bg: "bg-blue-600", text: "text-blue-100" },
  Electric: { bg: "bg-yellow-500", text: "text-yellow-900" },
  Grass: { bg: "bg-green-600", text: "text-green-100" },
  Psychic: { bg: "bg-pink-600", text: "text-pink-100" },
  Rock: { bg: "bg-stone-600", text: "text-stone-100" },
  Dragon: { bg: "bg-indigo-700", text: "text-indigo-100" },
  Fairy: { bg: "bg-pink-400", text: "text-pink-900" },
  Normal: { bg: "bg-gray-500", text: "text-gray-100" },
};

const BuffBadge: React.FC<BuffBadgeProps> = ({
  buffPet,
  buffName,
  buffDescription,
  buffIcon,
  petXpMultiplier,
  petCatchBonus,
  petHintShield,
  petReducePenalty,
}) => {
  const [expanded, setExpanded] = useState(false);

  // Don't render if no buff data at all
  if (!buffName && !buffPet) return null;

  const rarity = buffPet?.rarity ?? "Common";
  const type = buffPet?.type ?? "";
  const colors = rarityColors[rarity] ?? rarityColors["Common"];
  const typeColor = typeColors[type] ?? typeColors["Normal"];

  const xpBonus = petXpMultiplier && petXpMultiplier > 1
    ? `+${Math.round((petXpMultiplier - 1) * 100)}% XP`
    : null;
  const catchBonus = petCatchBonus && petCatchBonus > 0
    ? `+${Math.round(petCatchBonus * 100)}% Catch`
    : null;

  return (
    <div className="absolute top-15 right-2 z-40 font-pixel select-none">
      {/* Collapsed pill — always visible */}
      <motion.button
        onClick={() => setExpanded((v) => !v)}
        className={`flex items-center gap-2 px-3 py-1.5 rounded-sm border-2 ${colors.border} bg-gray-900 bg-opacity-90 shadow-lg ${colors.glow} cursor-pointer`}
        whileHover={{ scale: 1.04 }}
        whileTap={{ scale: 0.97 }}
        style={{ imageRendering: "pixelated" }}
      >
        {buffPet?.imageUrl && (
          <img
            src={buffPet.imageUrl}
            alt={buffPet.name}
            className="w-7 h-7 object-contain"
            style={{ imageRendering: "pixelated" }}
          />
        )}
        <span className="text-base">{buffIcon ?? "⭐"}</span>
        <span className={`text-xs ${colors.text} uppercase tracking-widest`}>
          {buffName ?? "Buff"}
        </span>
        <span className="text-gray-500 text-xs">{expanded ? "▲" : "▼"}</span>
      </motion.button>

      {/* Expanded panel */}
      <AnimatePresence>
        {expanded && (
          <motion.div
            className={`mt-1 w-64 rounded-sm border-2 ${colors.border} bg-gray-900 bg-opacity-95 shadow-2xl ${colors.glow} overflow-hidden`}
            initial={{ opacity: 0, y: -8, scaleY: 0.9 }}
            animate={{ opacity: 1, y: 0, scaleY: 1 }}
            exit={{ opacity: 0, y: -8, scaleY: 0.9 }}
            transition={{ duration: 0.15 }}
            style={{ transformOrigin: "top right" }}
          >
            {/* Pet info header */}
            <div className={`flex items-center gap-3 p-3 border-b-2 ${colors.border} bg-gray-800`}>
              {buffPet?.imageUrl ? (
                <img
                  src={buffPet.imageUrl}
                  alt={buffPet.name}
                  className="w-12 h-12 object-contain border-2 border-gray-600 rounded-sm bg-gray-950"
                  style={{ imageRendering: "pixelated" }}
                />
              ) : (
                <div className="w-12 h-12 bg-gray-800 border-2 border-gray-600 rounded-sm flex items-center justify-center text-2xl">
                  {buffIcon ?? "⭐"}
                </div>
              )}

              <div className="flex-1 min-w-0">
                <p className="text-white text-sm truncate">
                  {buffPet?.name ?? "???"}
                </p>

                {/* Type & Rarity badges */}
                <div className="flex gap-1 mt-1 flex-wrap">
                  {type && (
                    <span className={`text-xs px-1.5 py-0.5 rounded-sm font-pixel ${typeColor.bg} ${typeColor.text}`}>
                      {type}
                    </span>
                  )}
                  <span className={`text-xs px-1.5 py-0.5 rounded-sm font-pixel ${colors.bg} ${colors.text} border ${colors.border}`}>
                    {rarity}
                  </span>
                </div>
              </div>
            </div>

            {/* Buff info body */}
            <div className="p-3 space-y-2">
              {/* Buff name + icon */}
              <div className="flex items-center gap-2">
                <span className="text-lg">{buffIcon ?? "⭐"}</span>
                <span className={`text-sm font-pixel uppercase tracking-wider ${colors.text}`}>
                  {buffName}
                </span>
              </div>

              {/* Buff description */}
              {buffDescription && (
                <p className="text-gray-300 text-xs leading-relaxed border-l-2 border-gray-600 pl-2">
                  {buffDescription}
                </p>
              )}

              {/* Stat pills */}
              <div className="flex flex-wrap gap-1 pt-1">
                {xpBonus && (
                  <span className="text-xs bg-amber-900 border border-amber-600 text-amber-300 px-2 py-0.5 rounded-sm">
                    💰 {xpBonus}
                  </span>
                )}
                {catchBonus && (
                  <span className="text-xs bg-blue-900 border border-blue-600 text-blue-300 px-2 py-0.5 rounded-sm">
                    🎯 {catchBonus}
                  </span>
                )}
                {petHintShield && (
                  <span className="text-xs bg-purple-900 border border-purple-600 text-purple-300 px-2 py-0.5 rounded-sm">
                    🔮 1 Hint miễn phí
                  </span>
                )}
                {petReducePenalty && (
                  <span className="text-xs bg-stone-800 border border-stone-500 text-stone-300 px-2 py-0.5 rounded-sm">
                    🪨 Không giảm catch khi sai
                  </span>
                )}
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default BuffBadge;