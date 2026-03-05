import { motion, AnimatePresence, easeInOut } from "framer-motion";
import type {
  CompleteLearningSessionResponseDto,
  CompleteReviewSessionResponseDto,
} from "../../types/LearningSessionDto";

interface PetScreenProps {
  showRewardAnimation: boolean;
  captureComplete: boolean;
  setCaptureComplete: (value: boolean) => void;
  encounteredPet: { id: number; name: string; imageUrl: string } | null;
  userPet: { id: number; name: string; imageUrl: string } | null;
  sessionData: CompleteLearningSessionResponseDto | CompleteReviewSessionResponseDto | null;
  mode: "learning" | "review";
  petId?: number;
  handleCloseReward: () => void;
  catchRate: number;
  // kept for interface compatibility; no longer used for battle
  showBattleAnimation: boolean;
  isAnswerCorrect: boolean | null;
}

const PetScreen: React.FC<PetScreenProps> = ({
  showRewardAnimation,
  captureComplete,
  setCaptureComplete,
  encounteredPet,
  sessionData,
  mode,
  petId,
  handleCloseReward,
}) => {
  // Resolve which pet to show during end animation
  const getEndPet = () => {
    if (encounteredPet) return encounteredPet;
    if (mode === "review") {
      return {
        id: petId || 0,
        name: "Zapdos",
        imageUrl:
          "https://img.pokemondb.net/sprites/black-white/anim/normal/charizard.gif",
      };
    }
    return null;
  };

  const endPet = getEndPet();

  const getMessage = () => {
    if (mode === "learning" && sessionData && "isPetAlreadyOwned" in sessionData) {
      if (sessionData.isPetAlreadyOwned) return "Bạn đã sở hữu pet này!";
      if (sessionData.isPetRewardGranted)
        return `Chúc mừng! Bạn đã bắt được ${sessionData.petName}!`;
      return `${sessionData.petName} đã bỏ trốn!`;
    }
    return sessionData?.message || "Hoàn thành phiên học!";
  };

  if (!showRewardAnimation || !sessionData) return null;

  return (
    <div
      className="fixed inset-0 z-40 flex items-center justify-center"
      style={{
        backgroundImage: `url("../src/assets/battle-background.png")`,
        backgroundSize: "cover",
        backgroundPosition: "center",
        backgroundColor: "rgba(0,0,0,0.75)",
      }}
    >
      <AnimatePresence>
        <motion.div
          className="flex flex-col items-center justify-center text-center w-full h-full"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.4 }}
        >
          {/* ── Catch animation (Pokeball) ── */}
          {mode === "learning" &&
            "isPetRewardGranted" in sessionData &&
            sessionData.petId &&
            sessionData.isPetRewardGranted &&
            !captureComplete ? (
            <motion.div
              className="flex flex-col items-center"
              initial={{ scale: 0 }}
              animate={{
                scale: 1,
                rotate: [0, -10, 10, -10, 0, 10, -10, 0],
              }}
              transition={{
                scale: { duration: 0.6 },
                rotate: {
                  times: [0, 0.2, 0.4, 0.6, 0.7, 0.8, 0.9, 1],
                  duration: 3.5,
                },
              }}
              onAnimationComplete={() => setCaptureComplete(true)}
            >
              <img
                src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1757509182/PikPng.com_pokeball-sprite-png_4945371_nm8b89.png"
                alt="Poké Ball"
                className="w-20 h-20 pixel-art mb-2"
              />
              <p className="text-white text-sm font-pixel bg-black bg-opacity-70 p-2 rounded">
                Catching {sessionData.petName}...
              </p>
              <audio
                autoPlay
                src="https://res.cloudinary.com/dqpkxxzaf/video/upload/v1757509871/06-caught-a-pokemon_a3r9h1.mp3"
              />
            </motion.div>

          ) : /* ── Escape animation (pet flies away) ── */
            (mode === "learning" &&
              "isPetRewardGranted" in sessionData &&
              sessionData.petId &&
              !sessionData.isPetRewardGranted &&
              !captureComplete) ||
              (mode === "review" && !captureComplete) ? (
              <motion.div
                className="flex flex-col items-center"
                initial={{ scale: 1, y: 0, opacity: 1 }}
                animate={{ scale: 0.5, y: -240, opacity: 0 }}
                transition={{ duration: 2, ease: easeInOut }}
                onAnimationComplete={() => setCaptureComplete(true)}
              >
                <img
                  src={
                    mode === "review"
                      ? "https://img.pokemondb.net/sprites/black-white/anim/normal/charizard.gif"
                      : `https://img.pokemondb.net/sprites/black-white/anim/normal/${endPet?.name?.toLowerCase() ?? "pikachu"
                      }.gif`
                  }
                  alt={endPet?.name ?? "Pet"}
                  className="w-36 h-36 object-contain pixel-art mb-2"
                  onError={(e) => {
                    e.currentTarget.src = endPet?.imageUrl ?? "";
                  }}
                />
                <p className="text-white text-sm font-pixel bg-black bg-opacity-70 p-2 rounded">
                  {mode === "review"
                    ? "Zapdos đã bay đi!"
                    : `${endPet?.name ?? "Pet"} đã bỏ trốn!`}
                </p>
                <audio
                  autoPlay
                  src="https://res.cloudinary.com/dqpkxxzaf/video/upload/v1758225103/pokemon-red_blue_yellow-run-away-sound-effect-1_g4iona.mp3"
                />
              </motion.div>

            ) : (
              /* ── Reward Summary ── */
              <motion.div
                className="flex flex-col items-center justify-center text-center bg-gray-900 bg-opacity-90 p-8 rounded-2xl border-4 border-white w-11/12 max-w-lg shadow-2xl"
                initial={{ scale: 0.8, opacity: 0 }}
                animate={{ scale: 1, opacity: 1 }}
                transition={{ duration: 0.4, type: "spring" }}
              >
                {mode === "learning" &&
                  !("isPetRewardGranted" in sessionData && sessionData.isPetRewardGranted) && (
                    <motion.p
                      className="text-red-400 font-pixel mb-3 text-sm"
                      initial={{ scale: 0.8, opacity: 0 }}
                      animate={{ scale: 1, opacity: 1 }}
                    >
                      {"petName" in sessionData && sessionData.petName
                        ? `${sessionData.petName} đã bỏ trốn!`
                        : "No new pet reward available"}
                    </motion.p>
                  )}
                {mode === "review" && (
                  <motion.p
                    className="text-red-400 font-pixel mb-3 text-sm"
                    initial={{ scale: 0.8, opacity: 0 }}
                    animate={{ scale: 1, opacity: 1 }}
                  >
                    Zapdos đã bỏ trốn!
                  </motion.p>
                )}

                <motion.h2
                  className="text-2xl text-white font-pixel mb-4"
                  initial={{ y: 20, opacity: 0 }}
                  animate={{ y: 0, opacity: 1 }}
                >
                  {getMessage()}
                </motion.h2>

                {/* Pet reward display */}
                {mode === "learning" &&
                  "isPetRewardGranted" in sessionData &&
                  sessionData.petId &&
                  sessionData.isPetRewardGranted && (
                    <motion.div
                      className="flex flex-col items-center space-y-2 mb-5 p-4 bg-yellow-900 bg-opacity-80 rounded-xl"
                      initial={{ scale: 0, rotate: 180 }}
                      animate={{ scale: 1, rotate: 0 }}
                      transition={{ delay: 0.4, type: "spring" }}
                    >
                      <img
                        src={`https://img.pokemondb.net/sprites/black-white/anim/normal/${endPet?.name?.toLowerCase()}.gif`}
                        alt={sessionData.petName}
                        className="w-28 h-28 object-contain pixel-art rounded-lg border-2 border-yellow-400"
                        onError={(e) => {
                          e.currentTarget.src = endPet?.imageUrl ?? "";
                        }}
                      />
                      <h3 className="text-yellow-300 font-pixel text-base">
                        {sessionData.petName}
                      </h3>
                      <div className="text-xs text-yellow-200 space-y-1 text-center">
                        <p>Type: {sessionData.petType}</p>
                        <p>Rarity: {sessionData.petRarity}</p>
                      </div>
                    </motion.div>
                  )}

                {/* Rewards */}
                <div className="space-y-1 mb-5">
                  <motion.p
                    className="text-green-400 font-pixel"
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ delay: 0.2 }}
                  >
                    💰 XP: +{sessionData.xpEarned}
                  </motion.p>
                  {mode === "review" && "apEarned" in sessionData && (
                    <motion.p
                      className="text-blue-400 font-pixel"
                      initial={{ opacity: 0 }}
                      animate={{ opacity: 1 }}
                      transition={{ delay: 0.3 }}
                    >
                      💎 AP: +{sessionData.apEarned}
                    </motion.p>
                  )}
                </div>

                <motion.button
                  onClick={handleCloseReward}
                  className="bg-emerald-600 px-8 py-3 rounded-lg text-white font-pixel border-2 border-white hover:bg-emerald-700 transition-colors custom-cursor"
                  whileHover={{ scale: 1.05 }}
                  whileTap={{ scale: 0.95 }}
                  initial={{ y: 20, opacity: 0 }}
                  animate={{ y: 0, opacity: 1 }}
                  transition={{ delay: 0.7 }}
                >
                  🎉 Đóng & Về Dashboard
                </motion.button>
              </motion.div>
            )}
        </motion.div>
      </AnimatePresence>
    </div>
  );
};

export default PetScreen;