import { useEffect, useState } from "react";
import { motion, AnimatePresence } from "framer-motion";

interface PokemonEncounterIntroProps {
    encounteredPet: { id: number; name: string; imageUrl: string } | null;
    onComplete: () => void;
}

const PokemonEncounterIntro: React.FC<PokemonEncounterIntroProps> = ({
    encounteredPet,
    onComplete,
}) => {
    const [phase, setPhase] = useState<"enter" | "show" | "exit">("enter");
    const [displayedText, setDisplayedText] = useState("");
    const fullText = `A wild ${encounteredPet?.name ?? "Pokémon"} appeared!`;

    // Typewriter effect
    useEffect(() => {
        let index = 0;
        const interval = setInterval(() => {
            setDisplayedText(fullText.slice(0, index + 1));
            index++;
            if (index >= fullText.length) {
                clearInterval(interval);
            }
        }, 45);
        return () => clearInterval(interval);
    }, [fullText]);

    // Phase transitions
    useEffect(() => {
        const enterTimer = setTimeout(() => setPhase("show"), 400);
        const exitTimer = setTimeout(() => setPhase("exit"), 2800);
        const doneTimer = setTimeout(() => onComplete(), 3400);
        return () => {
            clearTimeout(enterTimer);
            clearTimeout(exitTimer);
            clearTimeout(doneTimer);
        };
    }, [onComplete]);

    const petImageUrl = encounteredPet
        ? `https://img.pokemondb.net/sprites/black-white/anim/normal/${encounteredPet.name.toLowerCase()}.gif`
        : null;

    return (
        <AnimatePresence>
            {phase !== "exit" && (
                <motion.div
                    className="fixed inset-0 z-50 flex flex-col items-center justify-center"
                    style={{ backgroundColor: "#0a0a0a" }}
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    exit={{ opacity: 0 }}
                    transition={{ duration: 0.5 }}
                >
                    {/* Grass / wild area flavor text */}
                    <motion.p
                        className="font-pixel text-gray-400 text-sm mb-8 tracking-widest uppercase"
                        initial={{ opacity: 0, y: -10 }}
                        animate={{ opacity: phase === "show" ? 1 : 0, y: 0 }}
                        transition={{ delay: 0.2, duration: 0.4 }}
                    >
                        ▶ Wild area
                    </motion.p>

                    {/* Pokemon sprite */}
                    {petImageUrl && (
                        <motion.div
                            className="relative mb-8"
                            initial={{ x: 200, opacity: 0 }}
                            animate={
                                phase === "show"
                                    ? { x: 0, opacity: 1 }
                                    : { x: 0, opacity: 0 }
                            }
                            transition={{
                                type: "spring",
                                stiffness: 180,
                                damping: 18,
                                delay: 0.1,
                            }}
                        >
                            {/* Glow ring */}
                            <motion.div
                                className="absolute inset-0 rounded-full"
                                style={{
                                    background:
                                        "radial-gradient(circle, rgba(253,224,71,0.35) 0%, transparent 70%)",
                                    filter: "blur(8px)",
                                }}
                                animate={{ scale: [1, 1.1, 1] }}
                                transition={{ duration: 1.8, repeat: Infinity, ease: "easeInOut" }}
                            />
                            <motion.img
                                src={petImageUrl}
                                alt={encounteredPet?.name}
                                className="w-40 h-40 object-contain pixel-art relative z-10"
                                onError={(e) => {
                                    e.currentTarget.src = encounteredPet?.imageUrl ?? "";
                                }}
                                animate={{ y: [0, -6, 0] }}
                                transition={{ duration: 1.6, repeat: Infinity, ease: "easeInOut" }}
                            />
                        </motion.div>
                    )}

                    {/* Typewriter text */}
                    <motion.div
                        className="bg-gray-900 border-2 border-yellow-400 rounded-lg px-8 py-4 max-w-sm w-full mx-4"
                        initial={{ opacity: 0, scaleX: 0 }}
                        animate={
                            phase === "show"
                                ? { opacity: 1, scaleX: 1 }
                                : { opacity: 0, scaleX: 0.9 }
                        }
                        transition={{ delay: 0.15, duration: 0.35 }}
                    >
                        <p className="font-pixel text-white text-lg text-center leading-relaxed min-h-[2rem]">
                            {displayedText}
                            <motion.span
                                className="inline-block ml-1 text-yellow-400"
                                animate={{ opacity: [1, 0, 1] }}
                                transition={{ duration: 0.8, repeat: Infinity }}
                            >
                                |
                            </motion.span>
                        </p>
                    </motion.div>

                    {/* Catch rate hint */}
                    <motion.p
                        className="font-pixel text-gray-500 text-xs mt-6"
                        initial={{ opacity: 0 }}
                        animate={{ opacity: phase === "show" ? 1 : 0 }}
                        transition={{ delay: 0.8 }}
                    >
                        Study well to catch it!
                    </motion.p>

                    {/* Audio: Pokemon battle cry */}
                    {phase === "show" && (
                        <audio
                            autoPlay
                            src="https://res.cloudinary.com/dqpkxxzaf/video/upload/v1758225103/pokemon-red_blue_yellow-battle-sound-effect-1_jw1t0t.mp3"
                        />
                    )}
                </motion.div>
            )}
        </AnimatePresence>
    );
};

export default PokemonEncounterIntro;
