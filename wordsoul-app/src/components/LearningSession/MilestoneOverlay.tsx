import { useEffect } from "react";
import { motion, AnimatePresence } from "framer-motion";

interface MilestoneOverlayProps {
    milestone: 25 | 50 | 75;
    encounteredPet: { id: number; name: string; imageUrl: string } | null;
    onClose: () => void;
}

const milestoneConfig = {
    25: {
        label: "25%",
        emoji: "🌱",
        text: "Good start! Keep going!",
        subText: "¼ of the way there!",
        color: "from-green-900/90 to-gray-900/90",
        border: "border-green-400",
        textColor: "text-green-400",
    },
    50: {
        label: "50%",
        emoji: "⚡",
        text: "Halfway there!",
        subText: "You're doing great!",
        color: "from-yellow-900/90 to-gray-900/90",
        border: "border-yellow-400",
        textColor: "text-yellow-400",
    },
    75: {
        label: "75%",
        emoji: "🔥",
        text: "Almost there!",
        subText: "Keep it up — nearly caught!",
        color: "from-orange-900/90 to-gray-900/90",
        border: "border-orange-400",
        textColor: "text-orange-400",
    },
};

const MilestoneOverlay: React.FC<MilestoneOverlayProps> = ({
    milestone,
    encounteredPet,
    onClose,
}) => {
    const config = milestoneConfig[milestone];

    useEffect(() => {
        const timer = setTimeout(onClose, 2500);
        return () => clearTimeout(timer);
    }, [onClose]);

    const petImageUrl = encounteredPet
        ? `https://img.pokemondb.net/sprites/black-white/anim/normal/${encounteredPet.name.toLowerCase()}.gif`
        : null;

    return (
        <AnimatePresence>
            <motion.div
                className="fixed inset-0 z-[70] flex items-center justify-center"
                style={{ backdropFilter: "blur(4px)", backgroundColor: "rgba(0,0,0,0.55)" }}
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                transition={{ duration: 0.3 }}
            >
                <motion.div
                    className={`bg-gradient-to-b ${config.color} border-2 ${config.border} rounded-2xl px-10 py-8 flex flex-col items-center text-center max-w-xs w-full mx-4 shadow-2xl`}
                    initial={{ scale: 0.5, y: 40, opacity: 0 }}
                    animate={{ scale: 1, y: 0, opacity: 1 }}
                    exit={{ scale: 0.8, y: -20, opacity: 0 }}
                    transition={{ type: "spring", stiffness: 260, damping: 22 }}
                >
                    {/* Milestone badge */}
                    <motion.div
                        className={`font-pixel text-4xl font-bold ${config.textColor} mb-2`}
                        initial={{ scale: 0 }}
                        animate={{ scale: [0, 1.3, 1] }}
                        transition={{ delay: 0.15, duration: 0.5 }}
                    >
                        {config.emoji} {config.label}
                    </motion.div>

                    {/* Pokemon sprite */}
                    {petImageUrl && (
                        <motion.img
                            src={petImageUrl}
                            alt={encounteredPet?.name}
                            className="w-28 h-28 object-contain pixel-art my-3"
                            onError={(e) => {
                                e.currentTarget.src = encounteredPet?.imageUrl ?? "";
                            }}
                            initial={{ x: -60, opacity: 0 }}
                            animate={{
                                x: 0,
                                opacity: 1,
                                rotate: [0, -8, 8, -5, 5, 0],
                            }}
                            transition={{
                                x: { delay: 0.2, duration: 0.4, type: "spring" },
                                opacity: { delay: 0.2, duration: 0.3 },
                                rotate: { delay: 0.55, duration: 0.7 },
                            }}
                        />
                    )}

                    {/* Message */}
                    <motion.p
                        className="font-pixel text-white text-base mb-1"
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ delay: 0.3 }}
                    >
                        {config.text}
                    </motion.p>
                    <motion.p
                        className={`font-pixel ${config.textColor} text-xs`}
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        transition={{ delay: 0.45 }}
                    >
                        {config.subText}
                    </motion.p>

                    {/* Auto-close progress bar */}
                    <div className="w-full bg-gray-700 rounded-full h-1 mt-5 overflow-hidden">
                        <motion.div
                            className={`h-full rounded-full bg-gradient-to-r ${milestone === 25
                                ? "from-green-400 to-green-300"
                                : milestone === 50
                                    ? "from-yellow-400 to-yellow-300"
                                    : "from-orange-400 to-orange-300"
                                }`}
                            initial={{ width: "100%" }}
                            animate={{ width: "0%" }}
                            transition={{ duration: 2.5, ease: "linear" }}
                        />
                    </div>
                </motion.div>
            </motion.div>
        </AnimatePresence>
    );
};

export default MilestoneOverlay;
