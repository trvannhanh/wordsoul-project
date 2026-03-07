import { motion } from "framer-motion";

interface PokemonProgressBarProps {
    currentCorrectAnswered: number;
    maxQuestions: number;
    catchRate: number;
    encounteredPet: { id: number; name: string; imageUrl: string } | null;
}

const MILESTONES = [25, 50, 75];

const PokemonProgressBar: React.FC<PokemonProgressBarProps> = ({
    currentCorrectAnswered,
    maxQuestions,
    catchRate,
    encounteredPet,
}) => {
    const progressPercent = Math.min(
        Math.max((currentCorrectAnswered / maxQuestions) * 100, 0),
        100
    );

    const petImageUrl = encounteredPet
        ? `https://img.pokemondb.net/sprites/black-white/anim/normal/${encounteredPet.name.toLowerCase()}.gif`
        : null;

    const catchRatePercent = Math.round(catchRate * 100);
    const catchRateColor =
        catchRatePercent >= 70
            ? "text-green-400"
            : catchRatePercent >= 40
                ? "text-yellow-400"
                : "text-red-400";

    return (
        <div className="w-full px-4 pt-3 pb-2">
            {/* Header row: label + catch rate */}
            <div className="flex items-center justify-between mb-1">
                <p className="text-white font-pixel text-xs">
                    ✅ {currentCorrectAnswered} / {maxQuestions}
                </p>
                {encounteredPet && (
                    <div className="flex items-center gap-1">
                        <span className="font-pixel text-gray-400 text-xs">Catch:</span>
                        <motion.span
                            key={catchRatePercent}
                            className={`font-pixel text-xs font-bold ${catchRateColor}`}
                            initial={{ scale: 1.4, opacity: 0.6 }}
                            animate={{ scale: 1, opacity: 1 }}
                            transition={{ duration: 0.3 }}
                        >
                            {catchRatePercent}%
                        </motion.span>
                    </div>
                )}
            </div>

            {/* Progress bar */}
            <div className="relative w-full bg-gray-700 rounded-full h-5 border-2 border-gray-600 overflow-visible">
                {/* Filled bar */}
                <motion.div
                    className="absolute top-0 left-0 h-full rounded-full bg-gradient-to-r from-green-600 to-green-400"
                    initial={{ width: "0%" }}
                    animate={{ width: `${progressPercent}%` }}
                    transition={{ duration: 0.5, ease: "easeInOut" }}
                />

                {/* Milestone markers */}
                {MILESTONES.map((pct) => {
                    const reached = progressPercent >= pct;
                    return (
                        <div
                            key={pct}
                            className="absolute top-0 h-full flex flex-col items-center"
                            style={{ left: `${pct}%`, transform: "translateX(-50%)" }}
                        >
                            <div
                                className={`w-0.5 h-full ${reached ? "bg-green-200" : "bg-gray-500"}`}
                            />
                        </div>
                    );
                })}

                {/* Pokemon mini-icon riding the bar */}
                {petImageUrl && (
                    <motion.div
                        className="absolute top-1/2 -translate-y-1/2 pointer-events-none"
                        animate={{ left: `${Math.max(progressPercent - 2, 0)}%` }}
                        transition={{ duration: 0.5, ease: "easeInOut" }}
                    >
                        <motion.img
                            src={petImageUrl}
                            alt={encounteredPet?.name}
                            className="w-8 h-8 object-contain pixel-art"
                            style={{ marginTop: "-18px" }}
                            onError={(e) => {
                                e.currentTarget.src = encounteredPet?.imageUrl ?? "";
                            }}
                            animate={{ y: [0, -3, 0] }}
                            transition={{ duration: 1.2, repeat: Infinity, ease: "easeInOut" }}
                        />
                    </motion.div>
                )}
            </div>

            {/* Milestone labels */}
            <div className="relative w-full mt-1">
                {MILESTONES.map((pct) => {
                    const reached = progressPercent >= pct;
                    return (
                        <span
                            key={pct}
                            className={`absolute font-pixel text-[9px] -translate-x-1/2 ${reached ? "text-green-300" : "text-gray-500"
                                }`}
                            style={{ left: `${pct}%` }}
                        >
                            {reached ? "✓" : `${pct}%`}
                        </span>
                    );
                })}
            </div>
        </div>
    );
};

export default PokemonProgressBar;
