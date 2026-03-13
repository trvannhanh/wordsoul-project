import { useState, useCallback, useRef } from "react";
import { motion, AnimatePresence } from "framer-motion";
import type { Variants } from "framer-motion";
import GameScreen from "./GameScreen";
import AnswerScreen from "./AnswerScreen";
import type { QuizQuestionDto } from "../../types/LearningSessionDto";

// ─────────────────────────────────────────────────────────────────────────────
// Gen 5 sprite URL helpers
// pokemondb hosts: animated (front/back) + static (front/back)
// ─────────────────────────────────────────────────────────────────────────────
const SPRITE_BASE = "https://img.pokemondb.net/sprites/black-white";

/**
 * Build a Gen-5 sprite URL from a pokemon name.
 * @param name   e.g. "bulbasaur"
 * @param facing "front" | "back"
 * @param animated  true = animated GIF, false = static PNG
 */
function gen5Sprite(name: string, facing: "front" | "back" = "front", animated = true): string {
    const slug = name.toLowerCase().replace(/[^a-z0-9-]/g, "-");
    if (animated) {
        return `${SPRITE_BASE}/anim/${facing === "back" ? "back/normal" : "normal"}/${slug}.gif`;
    }
    return `${SPRITE_BASE}/${facing === "back" ? "back" : "normal"}/${slug}.png`;
}

// ─────────────────────────────────────────────────────────────────────────────
// Three berry SVG variants (Oran = blue, Sitrus = yellow, Pecha = pink)
// Swap src props to real image paths once assets are ready.
// ─────────────────────────────────────────────────────────────────────────────
type BerryType = "oran" | "sitrus" | "pecha";

const BERRY_SRCS: Record<BerryType, string> = {
    oran: "https://res.cloudinary.com/dqpkxxzaf/image/upload/v1773418354/Grid_Oran_Berry_sfv4dd.png",
    sitrus: "https://res.cloudinary.com/dqpkxxzaf/image/upload/v1773418324/Grid_Sitrus_Berry_hu699y.png",
    pecha: "https://res.cloudinary.com/dqpkxxzaf/image/upload/v1773418354/Grid_Occa_Berry_drgsnw.png",
};

const BerrySvg: React.FC<{ type: BerryType; size?: number }> = ({ type, size = 28 }) => {
    const cfg: Record<BerryType, { body: string; shine: string; leaf: string }> = {
        oran: { body: "#4a9edd", shine: "#93c7f4", leaf: "#3bb34a" },
        sitrus: { body: "#f5c518", shine: "#fae07a", leaf: "#5acc5a" },
        pecha: { body: "#e85d75", shine: "#f4a0b0", leaf: "#3bb34a" },
    };
    const { body, shine, leaf } = cfg[type];
    return (
        <svg width={size} height={size} viewBox="0 0 28 28" fill="none">
            <ellipse cx="14" cy="17" rx="9" ry="9" fill={body} />
            <ellipse cx="10.5" cy="13.5" rx="3" ry="2" fill={shine} opacity="0.7" />
            <ellipse cx="13" cy="8" rx="3.5" ry="5.5" fill={leaf} transform="rotate(-18 13 8)" />
            <ellipse cx="16" cy="7.5" rx="2.8" ry="5" fill="#5acc5a" transform="rotate(14 16 7.5)" />
            <line x1="14" y1="10.5" x2="14" y2="6.5" stroke="#5a3a1a" strokeWidth="1.5" strokeLinecap="round" />
        </svg>
    );
};

// ─────────────────────────────────────────────────────────────────────────────
// Types
// ─────────────────────────────────────────────────────────────────────────────
export type PetMood = "idle" | "happy" | "sad" | "excited" | "eating";

interface BerryParticle {
    id: number;
    x: number;       // % from left in drop-zone
    type: BerryType;
}

interface ReviewLayoutProps {
    question: QuizQuestionDto | null;
    loading: boolean;
    error: string | null;
    handleAnswer: (
        question: QuizQuestionDto,
        answer: string,
        onAnswerProcessed: () => void,
        onResult?: (isCorrect: boolean) => void,
        responseTimeSeconds?: number,
        usedHintCount?: number
    ) => Promise<boolean>;
    loadNextQuestion: () => void;
    showPopup: (question: QuizQuestionDto) => void;
    hintBalance?: number;
    setHintBalance?: (value: number) => void;
    userPet: { id: number; name: string; imageUrl: string } | null;
    currentCorrectAnswered: number;
    maxQuestions: number;
}

// ─────────────────────────────────────────────────────────────────────────────
// Reaction messages
// ─────────────────────────────────────────────────────────────────────────────
const REACTION: Record<PetMood, string[]> = {
    idle: ["Đang chờ...", "Cùng ôn bài nào!", "Sẵn sàng chưa?"],
    happy: ["Tuyệt vời!", "Chính xác!", "Xuất sắc!", "Làm tốt lắm!"],
    sad: ["Cố lên nào...", "Thử lại nhé!", "Đừng bỏ cuộc!"],
    excited: ["Streak! 🔥", "Liên tiếp rồi!", "Không thể dừng!"],
    eating: ["Ngon quá!", "Thêm berry nữa!", "Ăn ngon!", "Yummy!"],
};

const BERRY_TYPES: BerryType[] = ["oran", "sitrus", "pecha"];
function randomBerry(): BerryType {
    return BERRY_TYPES[Math.floor(Math.random() * BERRY_TYPES.length)];
}
function pickMsg(mood: PetMood): string {
    const arr = REACTION[mood];
    return arr[Math.floor(Math.random() * arr.length)];
}

// ─────────────────────────────────────────────────────────────────────────────
// FallingBerry
// ─────────────────────────────────────────────────────────────────────────────
const FallingBerry: React.FC<{
    particle: BerryParticle;
    onDone: (id: number) => void;
}> = ({ particle, onDone }) => {
    const [imgErr, setImgErr] = useState(false);

    return (
        <motion.div
            className="absolute top-0 pointer-events-none z-20"
            style={{ left: `${particle.x}%` }}
            initial={{ y: -36, opacity: 1, rotate: -20, scale: 1.15 }}
            animate={{
                y: ["0px", "50%", "62%", "72%"],
                opacity: [1, 1, 0.7, 0],
                rotate: [-20, 8, -6, 4],
                scale: [1.15, 1, 0.8, 0.45],
            }}
            transition={{ duration: 1.05, ease: "easeIn", times: [0, 0.5, 0.78, 1] }}
            onAnimationComplete={() => onDone(particle.id)}
        >
            {imgErr ? (
                <BerrySvg type={particle.type} size={28} />
            ) : (
                <img
                    src={BERRY_SRCS[particle.type]}
                    alt={particle.type}
                    className="w-12 h-12 sm:w-50 sm:h-50 object-contain"
                    style={{ imageRendering: "pixelated" }}
                    onError={() => setImgErr(true)}
                />
            )}
        </motion.div>
    );
};

// ─────────────────────────────────────────────────────────────────────────────
// EatBurst
// ─────────────────────────────────────────────────────────────────────────────
const EatBurst: React.FC<{ active: boolean }> = ({ active }) => (
    <AnimatePresence>
        {active &&
            [...Array(8)].map((_, i) => {
                const rad = ((i / 8) * 360 * Math.PI) / 180;
                return (
                    <motion.div
                        key={i}
                        className="absolute w-2 h-2 rounded-full bg-pink-400 pointer-events-none z-30"
                        style={{ top: "50%", left: "50%", marginTop: -4, marginLeft: -4 }}
                        initial={{ x: 0, y: 0, opacity: 1, scale: 1 }}
                        animate={{ x: Math.cos(rad) * 36, y: Math.sin(rad) * 36, opacity: 0, scale: 0.15 }}
                        exit={{ opacity: 0 }}
                        transition={{ duration: 0.5, delay: i * 0.025, ease: "easeOut" }}
                    />
                );
            })}
    </AnimatePresence>
);

// ─────────────────────────────────────────────────────────────────────────────
// PetMoodPill
// ─────────────────────────────────────────────────────────────────────────────
const PetMoodPill: React.FC<{ mood: PetMood }> = ({ mood }) => {
    const cfg: Record<PetMood, { pill: string; dot: string; label: string }> = {
        idle: { pill: "bg-gray-700 border-gray-600", dot: "bg-gray-400", label: "chờ" },
        happy: { pill: "bg-emerald-900 border-emerald-600", dot: "bg-emerald-400", label: "vui" },
        sad: { pill: "bg-red-950 border-red-800", dot: "bg-red-400", label: "buồn" },
        excited: { pill: "bg-amber-900 border-amber-600", dot: "bg-amber-400", label: "hứng khởi" },
        eating: { pill: "bg-pink-950 border-pink-700", dot: "bg-pink-400", label: "đang ăn" },
    };
    const { pill, dot, label } = cfg[mood];
    return (
        <div className={`flex items-center gap-1.5 px-2 py-1 rounded-full border font-pixel ${pill}`}
            style={{ fontSize: "10px" }}>
            <motion.div className={`w-1.5 h-1.5 rounded-full ${dot}`}
                animate={{ scale: [1, 1.5, 1] }} transition={{ duration: 1, repeat: Infinity }} />
            <span className="text-gray-300">{label}</span>
        </div>
    );
};

// ─────────────────────────────────────────────────────────────────────────────
// PetSprite — Gen5 animated front sprite, fallback → imageUrl from DB
// ─────────────────────────────────────────────────────────────────────────────
const PET_VARIANTS: Variants = {
    idle: {
        y: [0, -6, 0],
        transition: { duration: 2.4, repeat: Infinity, ease: "easeInOut" },
    },
    happy: {
        y: [0, -14, 0, -9, 0],
        rotate: [-4, 4, -4, 4, 0],
        transition: { duration: 0.55 },
    },
    sad: {
        rotate: [-3, 3, -3, 0],
        y: [0, 5, 0],
        transition: { duration: 0.5 },
    },
    excited: {
        y: [0, -20, 2, -13, 0, -7, 0],
        rotate: [-6, 6, -5, 5, 0],
        scale: [1, 1.14, 1, 1.08, 1],
        transition: { duration: 0.72 },
    },
    eating: {
        scaleX: [1, 1.13, 0.91, 1.05, 1],
        scaleY: [1, 0.87, 1.11, 0.96, 1],
        y: [0, -5, 4, -1, 0],
        transition: { duration: 0.42 },
    },
};

const PetSprite: React.FC<{
    petName: string;
    fallbackUrl: string;
    mood: PetMood;
}> = ({ petName, fallbackUrl, mood }) => {
    // Stage 0: animated GIF from pokemondb
    // Stage 1: fallback to DB imageUrl
    const [stage, setStage] = useState<0 | 1>(0);
    const animSrc = gen5Sprite(petName, "front", true);

    const handleError = () => {
        if (stage === 0) setStage(1);
    };

    const src = stage === 0 ? animSrc : fallbackUrl;

    return (
        <motion.img
            key={`${petName}-${stage}-${mood}`}
            src={src}
            alt={petName}
            // Larger: mobile w-28 h-28, sm+ w-36 h-36
            className="w-28 h-28 sm:w-100 sm:h-130 object-contain"
            style={{ imageRendering: "pixelated" }}
            animate={mood}
            variants={PET_VARIANTS}
            onError={handleError}
        />
    );
};

// ─────────────────────────────────────────────────────────────────────────────
// ReviewLayout
// ─────────────────────────────────────────────────────────────────────────────
const ReviewLayout: React.FC<ReviewLayoutProps> = ({
    question,
    loading,
    error,
    handleAnswer,
    loadNextQuestion,
    showPopup,
    hintBalance,
    setHintBalance,
    userPet,
}) => {
    const [petMood, setPetMood] = useState<PetMood>("idle");
    const [reactionText, setReactionText] = useState("Cùng ôn bài nào!");
    const [streak, setStreak] = useState(0);
    const [, setTotal] = useState(0);
    const [berries, setBerries] = useState<BerryParticle[]>([]);
    const [eatBurst, setEatBurst] = useState(false);
    const [, setIdleIdx] = useState(0);

    const berryIdRef = useRef(0);

    const dropBerries = useCallback((count: number) => {
        const fresh: BerryParticle[] = Array.from({ length: count }, () => ({
            id: ++berryIdRef.current,
            x: 12 + Math.random() * 76,
            type: randomBerry(),
        }));
        setBerries((p) => [...p, ...fresh]);
    }, []);

    const removeBerry = useCallback((id: number) =>
        setBerries((p) => p.filter((b) => b.id !== id)), []);

    const handleAnswerResult = useCallback((isCorrect: boolean) => {
        setTotal((t) => t + 1);

        if (isCorrect) {
            const ns = streak + 1;
            setStreak(ns);
            const mood: PetMood = ns >= 3 ? "excited" : "happy";
            setPetMood(mood);
            setReactionText(pickMsg(mood));
            dropBerries(ns >= 5 ? 3 : ns >= 3 ? 2 : 1);

            // After berry lands → eating
            setTimeout(() => {
                setPetMood("eating");
                setReactionText(pickMsg("eating"));
                setEatBurst(true);
                setTimeout(() => setEatBurst(false), 560);
            }, 920);
        } else {
            setStreak(0);
            setPetMood("sad");
            setReactionText(pickMsg("sad"));
        }

        // Back to idle
        setTimeout(() => {
            setPetMood("idle");
            setIdleIdx((i) => {
                const msgs = REACTION["idle"];
                setReactionText(msgs[i % msgs.length]);
                return i + 1;
            });
        }, 2700);
    }, [streak, dropBerries]);

    // Mood → style tokens
    const borderColor: Record<PetMood, string> = {
        idle: "border-gray-600",
        happy: "border-emerald-500",
        sad: "border-red-700",
        excited: "border-amber-500",
        eating: "border-pink-500",
    };
    const cardBg: Record<PetMood, string> = {
        idle: "bg-gray-900",
        happy: "bg-emerald-950",
        sad: "bg-red-950",
        excited: "bg-amber-950",
        eating: "bg-pink-950",
    };
    const reactionCls: Record<PetMood, string> = {
        idle: "text-gray-400 border-gray-700 bg-gray-900",
        happy: "text-emerald-300 border-emerald-700 bg-emerald-950",
        sad: "text-red-300 border-red-800 bg-red-950",
        excited: "text-amber-300 border-amber-700 bg-amber-950",
        eating: "text-pink-300 border-pink-700 bg-pink-950",
    };

    return (
        /**
         * RESPONSIVE:
         * - Mobile (<640 sm):  stacked — pet panel on TOP (compact), Q+A below
         * - Tablet+ (sm+):     side-by-side — pet 40% left, Q+A 60% right
         */
        <div className="w-full h-full flex flex-col sm:flex-row overflow-hidden">

            {/* ════════════════════════════════════════════════════════
          PET PANEL
          — mobile: horizontal strip at top (fixed height)
          — sm+:    left column (40% width, full height)
      ════════════════════════════════════════════════════════ */}
            <div
                className={[
                    // shared
                    "flex items-center bg-gray-800 transition-colors duration-500",
                    borderColor[petMood],
                    // mobile: row layout, compact
                    "flex-row gap-3 px-3 py-2 border-b-2",
                    "sm:flex-col sm:justify-between sm:py-4 sm:px-3 sm:border-b-0 sm:border-r-2",
                    // width / height
                    "w-full sm:w-2/5",
                    "h-auto sm:h-full",
                ].join(" ")}
            >
                {/* ── TOP ROW (mood pill + streak) ── */}
                <div className="hidden sm:flex items-center justify-between w-full">
                    <PetMoodPill mood={petMood} />
                    <AnimatePresence>
                        {streak >= 2 && (
                            <motion.div
                                key={streak}
                                className="font-pixel text-amber-300 bg-amber-950 border border-amber-700 px-2 py-0.5 rounded-full"
                                style={{ fontSize: "10px" }}
                                initial={{ scale: 0, opacity: 0 }}
                                animate={{ scale: 1, opacity: 1 }}
                                exit={{ scale: 0, opacity: 0 }}
                            >
                                🔥 ×{streak}
                            </motion.div>
                        )}
                    </AnimatePresence>
                </div>

                {/* ── PET SPRITE (center) ── */}
                <div className="relative  flex justify-center items-center flex-shrink-0">
                    {/* Berry drop zone */}
                    <div className="absolute inset-0 overflow-hidden pointer-events-none z-10">
                        <AnimatePresence>
                            {berries.map((b) => (
                                <FallingBerry key={b.id} particle={b} onDone={removeBerry} />
                            ))}
                        </AnimatePresence>
                    </div>

                    {userPet ? (
                        <div className={[
                            "relative rounded-xl border-2 transition-colors duration-400",
                            borderColor[petMood], cardBg[petMood],
                            // mobile: smaller card
                            "p-2 sm:p-3",
                        ].join(" ")}>
                            <EatBurst active={eatBurst} />

                            {/* Excited confetti */}
                            <AnimatePresence>
                                {petMood === "excited" &&
                                    [...Array(5)].map((_, i) => (
                                        <motion.div
                                            key={i}
                                            className="absolute w-1 h-1 rounded-full bg-amber-400 pointer-events-none"
                                            style={{ top: "50%", left: "50%" }}
                                            initial={{ x: 0, y: 0, opacity: 1, scale: 1 }}
                                            animate={{
                                                x: (i % 2 === 0 ? 1 : -1) * (22 + i * 9),
                                                y: -(20 + i * 7),
                                                opacity: 0,
                                                scale: 0.3,
                                            }}
                                            exit={{ opacity: 0 }}
                                            transition={{ duration: 0.55, delay: i * 0.07 }}
                                        />
                                    ))}
                            </AnimatePresence>

                            {/* Sprite */}
                            <PetSprite
                                petName={userPet.name}
                                fallbackUrl={userPet.imageUrl}
                                mood={petMood}
                            />
                        </div>
                    ) : (
                        <div className="w-28 h-28 sm:w-36 sm:h-36 rounded-xl bg-gray-900 border-2 border-gray-700 flex items-center justify-center">
                            <span style={{ fontSize: "40px" }}>🐾</span>
                        </div>
                    )}
                </div>

                {/* ── PET NAME + REACTION + CONTROLS (right of pet on mobile, below on sm+) ── */}
                <div className="flex flex-col gap-1.5 flex-1 sm:items-center sm:w-full min-w-0">
                    {/* Name */}
                    {userPet && (
                        <div className="flex items-center gap-2 flex-wrap">
                            <p className="font-pixel text-white" style={{ fontSize: "12px" }}>{userPet.name}</p>
                            {/* Mobile: mood pill inline */}
                            <div className="sm:hidden">
                                <PetMoodPill mood={petMood} />
                            </div>
                            {/* Mobile: streak badge */}
                            {streak >= 2 && (
                                <span
                                    className="font-pixel text-amber-300 bg-amber-950 border border-amber-700 px-1.5 py-0.5 rounded-full sm:hidden"
                                    style={{ fontSize: "10px" }}
                                >
                                    🔥 ×{streak}
                                </span>
                            )}
                        </div>
                    )}

                    {/* Reaction bubble */}
                    <AnimatePresence mode="wait">
                        <motion.div
                            key={reactionText}
                            className={`font-pixel px-2 py-0.5 rounded-full border text-center w-fit max-w-full ${reactionCls[petMood]}`}
                            style={{ fontSize: "30px" }}
                            initial={{ opacity: 0, y: 4, scale: 0.88 }}
                            animate={{ opacity: 1, y: 0, scale: 1 }}
                            exit={{ opacity: 0, y: -4, scale: 0.88 }}
                            transition={{ duration: 0.2 }}
                        >
                            {reactionText}
                        </motion.div>
                    </AnimatePresence>

                    {/* In-flight berry hint */}
                    <AnimatePresence>
                        {berries.length > 0 && (
                            <motion.div
                                className="flex items-center gap-1 text-pink-400 font-pixel"
                                style={{ fontSize: "10px" }}
                                initial={{ opacity: 0 }}
                                animate={{ opacity: 0.85 }}
                                exit={{ opacity: 0 }}
                            >
                                <BerrySvg type="pecha" size={11} />
                                <span>berry!</span>
                            </motion.div>
                        )}
                    </AnimatePresence>

                </div>
            </div>

            {/* ════════════════════════════════════════════════════════
          QUESTION + ANSWER PANEL
          — mobile: fills remaining height below pet strip
          — sm+:    60% right column
      ════════════════════════════════════════════════════════ */}
            <div className="flex-1 flex flex-col overflow-hidden min-h-0">
                {/* Question zone */}
                <div className="flex-1 border-b-2 border-gray-700 p-2 sm:p-3 min-h-0">
                    <div className="h-full bg-gray-900 border border-gray-700 rounded-lg flex items-center justify-center">
                        <GameScreen
                            question={question}
                            loading={loading}
                            error={error}
                            mode="review"
                        />
                    </div>
                </div>

                {/* Answer zone */}
                <div className="flex-1 p-2 sm:p-3 bg-gray-800 min-h-0">
                    <div className="h-full bg-gray-900 border border-gray-700 rounded-lg flex items-center justify-center overflow-hidden">
                        <AnswerScreen
                            question={question}
                            loading={loading}
                            error={error}
                            handleAnswer={handleAnswer}
                            loadNextQuestion={loadNextQuestion}
                            showPopup={showPopup}
                            hintBalance={hintBalance}
                            setHintBalance={setHintBalance}
                            onAnswerResult={handleAnswerResult}
                        />
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ReviewLayout;