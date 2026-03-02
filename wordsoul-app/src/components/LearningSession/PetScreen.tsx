import { motion, AnimatePresence, easeInOut, easeOut } from "framer-motion";
import type { CompleteLearningSessionResponseDto, CompleteReviewSessionResponseDto } from "../../types/LearningSessionDto";

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
  showBattleAnimation: boolean;
  isAnswerCorrect: boolean | null;
}

const PetScreen: React.FC<PetScreenProps> = ({
  showRewardAnimation,
  captureComplete,
  setCaptureComplete,
  encounteredPet,
  userPet,
  sessionData,
  mode,
  petId,
  handleCloseReward,
  catchRate,
  showBattleAnimation,
  isAnswerCorrect,
}) => {
  // Placeholder pet khi chưa có sessionData
  const getPlaceholderPet = () => {
    if (mode === "review") {
      return {
        id: petId || 0,
        name: "Zapdos",
        imageUrl: "https://img.pokemondb.net/sprites/black-white/anim/normal/charizard.gif",
      };
    }

    if (!petId) return null;

    return {
      id: petId,
      name: `Pet #${petId}`,
      imageUrl: `https://via.placeholder.com/120x120/4F46E5/FFFFFF?text=P${petId}`,
    };
  };

  const placeholderPet = getPlaceholderPet();

  // Hàm để định dạng thông báo khi kết thúc phiên học
  const getMessage = () => {
    if (mode === "learning" && sessionData && "isPetAlreadyOwned" in sessionData) {
      if (sessionData.isPetAlreadyOwned) {
        return "Bạn đã sở hữu pet này!";
      }
      if (sessionData.isPetRewardGranted) {
        return `Chúc mừng! Bạn đã bắt được ${sessionData.petName}!`;
      }
      return `${sessionData.petName} đã bỏ trốn!`;
    }
    return sessionData?.message || "Hoàn thành phiên học!";
  };

  // Variants for User Pet
  const userPetVariants = {
    rest: { x: 0, scale: 1, rotate: 0 },
    attack: {
      x: [0, 40, 0],
      scale: [1, 1.2, 1],
      rotate: 0,
      transition: { duration: 0.8, ease: easeInOut },
    },
    damage: {
      rotate: [-5, 5, -3, 3, 0],
      scale: [1, 0.9, 1.1, 0.95, 1],
      x: 0,
      transition: { duration: 0.6, ease: easeOut },
    },
  };

  // Variants for Wild Pet
  const wildPetVariants = {
    rest: { x: 0, scale: 1, rotate: 0 },
    attack: {
      x: [0, -40, 0],
      scale: [1, 1.2, 1],
      rotate: 0,
      transition: { duration: 0.8, ease: easeInOut },
    },
    damage: {
      rotate: [-5, 5, -3, 3, 0],
      scale: [1, 0.9, 1.1, 0.95, 1],
      x: 0,
      transition: { duration: 0.6, ease: easeOut },
    },
  };

  // Determine variant for user and wild pet
  const getUserVariant = () => {
    if (!showBattleAnimation) return "rest";
    return isAnswerCorrect ? "attack" : "damage";
  };

  const getWildVariant = () => {
    if (!showBattleAnimation) return "rest";
    return !isAnswerCorrect ? "attack" : "damage";
  };

  return (
    <div
      className="w-full sm:w-2/12 lg:w-1/2 h-2/4 sm:h-2/4 lg:h-full bg-gray-700 border-4 border-black rounded-lg flex flex-col items-center justify-center overflow-hidden p-4 relative"
      style={{
        backgroundImage: `url("../src/assets/battle-background.png")`,
        backgroundSize: "cover",
        backgroundPosition: "center",
      }}
    >
      <AnimatePresence>
        {/* Session Complete - Reward Screen */}
        {showRewardAnimation && sessionData ? (
          <motion.div
            className="flex flex-col items-center justify-center text-center w-full h-full"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3 }}
          >
            {mode === "review" && !captureComplete ? (
              // Hiệu ứng pet bỏ trốn trong mode review
              <motion.div
                className="flex flex-col items-center"
                initial={{ scale: 1, y: 0, opacity: 1 }}
                animate={{
                  scale: 0.5,
                  y: -200,
                  opacity: 0,
                }}
                transition={{
                  duration: 2,
                  ease: easeInOut,
                }}
                onAnimationComplete={() => setCaptureComplete(true)}
              >
                <img
                  src="https://img.pokemondb.net/sprites/black-white/anim/normal/charizard.gif"
                  alt="Zapdos"
                  className="w-50 h-50 object-contain pixel-art mb-2"
                />
                <p className="text-white text-sm font-pixel bg-black bg-opacity-70 p-2 rounded">
                  Zapdos đã bay đi!
                </p>
                <audio
                  autoPlay
                  src="https://res.cloudinary.com/dqpkxxzaf/video/upload/v1758225103/pokemon-red_blue_yellow-run-away-sound-effect-1_g4iona.mp3"
                />
              </motion.div>
            ) : mode === "learning" &&
              "isPetRewardGranted" in sessionData &&
              sessionData.petId &&
              sessionData.isPetRewardGranted &&
              !captureComplete ? (
              // Hiệu ứng bắt pet trong mode learning
              <motion.div
                className="flex flex-col items-center"
                initial={{ scale: 0 }}
                animate={{
                  scale: 1,
                  rotate: [0, -10, 10, -10, 0, 10, -10, 0],
                }}
                transition={{
                  scale: { duration: 0.6 },
                  rotate: { times: [0, 0.2, 0.4, 0.6, 0.7, 0.8, 0.9, 1], duration: 3.5 },
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
            ) : mode === "learning" &&
              "isPetRewardGranted" in sessionData &&
              sessionData.petId &&
              !sessionData.isPetRewardGranted &&
              !captureComplete ? (
              // Hiệu ứng pet bỏ trốn trong mode learning
              <motion.div
                className="flex flex-col items-center"
                initial={{ scale: 1, y: 0, opacity: 1 }}
                animate={{
                  scale: 0.5,
                  y: -200,
                  opacity: 0,
                }}
                transition={{
                  duration: 2,
                  ease: easeInOut,
                }}
                onAnimationComplete={() => setCaptureComplete(true)}
              >
                <img
                  src={`https://img.pokemondb.net/sprites/black-white/anim/normal/${sessionData.petName}.gif`}
                  alt={sessionData.petName || "Pet"}
                  className="w-50 h-50 object-contain pixel-art mb-2"
                />
                <p className="text-white text-sm font-pixel bg-black bg-opacity-70 p-2 rounded">
                  {sessionData.petName || "Pet"} đã bỏ trốn!
                </p>
                <audio
                  autoPlay
                  src="https://res.cloudinary.com/dqpkxxzaf/video/upload/v1758225103/pokemon-red_blue_yellow-run-away-sound-effect-1_g4iona.mp3"
                />
              </motion.div>
            ) : (
              // Reward Complete Screen (cho cả learning và review sau khi pet bỏ trốn hoặc bắt xong)
              <div className="flex flex-col items-center justify-center text-center bg-opacity-70 p-4 rounded-lg w-3/4 h-3/4">
                {mode === "learning" &&
                  !("isPetRewardGranted" in sessionData && sessionData.isPetRewardGranted) && (
                    <motion.p
                      className="text-red-500 font-pixel mb-4"
                      initial={{ scale: 0.8, opacity: 0 }}
                      animate={{ scale: 1, opacity: 1 }}
                    >
                      {"petName" in sessionData && sessionData.petName ? `${sessionData.petName} đã bỏ trốn!` : "No new pet reward available"}
                    </motion.p>
                  )}
                {mode === "review" && (
                  <motion.p
                    className="text-red-500 font-pixel mb-4"
                    initial={{ scale: 0.8, opacity: 0 }}
                    animate={{ scale: 1, opacity: 1 }}
                  >
                    Zapdos đã bỏ trốn!
                  </motion.p>
                )}

                <motion.h2
                  className="text-xl text-white font-pixel mb-4"
                  initial={{ y: 20, opacity: 0 }}
                  animate={{ y: 0, opacity: 1 }}
                >
                  {getMessage()}
                </motion.h2>

                <div className="space-y-2 mb-4">
                  <motion.p className="text-green-900 font-pixel">
                    💰 XP: +{sessionData.xpEarned}
                  </motion.p>
                  {mode === "review" && "apEarned" in sessionData && (
                    <motion.p className="text-blue-400 font-pixel">
                      💎 AP: +{sessionData.apEarned}
                    </motion.p>
                  )}
                </div>

                <motion.button
                  onClick={handleCloseReward}
                  className="bg-emerald-600 px-6 py-3 rounded-lg text-white font-pixel border-2 border-white hover:bg-emerald-700 transition-colors"
                  whileHover={{ scale: 1.05 }}
                  whileTap={{ scale: 0.95 }}
                  initial={{ y: 20, opacity: 0 }}
                  animate={{ y: 0, opacity: 1 }}
                  transition={{ delay: 0.8 }}
                >
                  🎉 Đóng & Về Dashboard
                </motion.button>
              </div>
            )}
          </motion.div>
        ) : (
          // Session đang diễn ra - Hiển thị pet preview hoặc battle animation
          <>
            {mode === "learning" && !showBattleAnimation && (
              <motion.div
                className="text-center space-y-2 absolute top-10"
                initial={{ y: 20, opacity: 0 }}
                animate={{ y: 0, opacity: 1 }}
                transition={{ delay: 0.2 }}
              >
                <p className="text-yellow-300 font-pixel text-s bg-black bg-opacity-50 px-2 py-1 rounded">
                  Tỉ lệ bắt: {(catchRate * 100).toFixed(0)}%
                </p>
              </motion.div>
            )}

            {/* Persistent User Pet (Bottom Left) */}
            {(encounteredPet || placeholderPet) && userPet && (
              <motion.div
                className="flex flex-col items-center justify-center w-80 h-80 absolute bottom-25 -left-25"
                variants={userPetVariants}
                initial="rest"
                animate={getUserVariant()}
                whileHover={showBattleAnimation ? undefined : { scale: 1.1, rotate: 5 }}
              >
                <motion.img
                  src={`https://img.pokemondb.net/sprites/black-white/anim/back-normal/${userPet.name.toLowerCase()}.gif`}
                  alt={userPet.name}
                  className="w-40 h-40 sm:w-40 sm:h-40 lg:w-100 lg:h-100 object-contain pixel-art rounded-lg mb-3"
                  style={{
                    filter: showBattleAnimation && !isAnswerCorrect ? "brightness(0.7) sepia(1) saturate(3) hue-rotate(300deg)" : "none",
                  }}
                  onError={(e) => {
                    e.currentTarget.src = userPet.imageUrl;
                  }}
                />
                <p className={`text-white font-pixel text-xs ${showBattleAnimation && !isAnswerCorrect ? "text-red-400" : ""}`}>
                  {userPet.name}
                </p>
              </motion.div>
            )}

            {/* Persistent Wild Pet (Top Right) */}
            {(encounteredPet || placeholderPet) && (
              <motion.div
                className="flex flex-col items-center justify-center w-60 h-60 absolute top-40 right-5"
                variants={wildPetVariants}
                initial="rest"
                animate={getWildVariant()}
                whileHover={showBattleAnimation ? undefined : { scale: 1.1, rotate: 5 }}
              >
                {/* Wild Pet Info */}
                <motion.div
                  className="text-center space-y-2 mb-3"
                >
                  <h3 className={`text-white font-pixel text-lg ${showBattleAnimation && isAnswerCorrect ? "text-red-400" : ""}`}>
                    {encounteredPet?.name || placeholderPet?.name}
                  </h3>
                </motion.div>

                <motion.img
                  src={`https://img.pokemondb.net/sprites/black-white/anim/normal/${encounteredPet?.name.toLowerCase() || placeholderPet?.name.toLowerCase()}.gif`}
                  alt={encounteredPet?.name || placeholderPet?.name}
                  className="w-40 h-40 sm:w-40 sm:h-40 lg:w-100 lg:h-100 object-contain pixel-art rounded-lg"
                  style={{
                    filter: showBattleAnimation && isAnswerCorrect ? "brightness(0.7) sepia(1) saturate(3) hue-rotate(300deg)" : "none",
                  }}
                  onError={(e) => {
                    e.currentTarget.src = encounteredPet?.imageUrl ?? placeholderPet?.imageUrl ?? "";
                  }}
                />
              </motion.div>
            )}

            {/* No pet - Placeholder */}
            {!showBattleAnimation && !(encounteredPet || placeholderPet) && (
              <motion.div
                className="flex flex-col items-center justify-center text-center w-full h-full"
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
              >
                <div className="w-24 h-24 bg-gray-600 rounded-full flex items-center justify-center mb-4">
                  <span className="text-gray-400 font-pixel text-2xl">?</span>
                </div>
                <motion.p
                  className="text-white font-pixel mb-2"
                  initial={{ y: 10 }}
                  animate={{ y: 0 }}
                >
                  {mode === "learning" ? "Learning Pet" : "Review Pet"}
                </motion.p>
                <motion.p
                  className="text-gray-400 font-pixel text-sm"
                  initial={{ y: 10 }}
                  animate={{ y: 0 }}
                  transition={{ delay: 0.2 }}
                >
                  Complete session to meet your companion!
                </motion.p>
              </motion.div>
            )}

            {/* Audio triggers during battle */}
            {showBattleAnimation && (
              <>
                {isAnswerCorrect && (
                  <audio autoPlay src="https://res.cloudinary.com/dqpkxxzaf/video/upload/v1758225103/pokemon-red_blue_yellow-battle-sound-effect-1_jw1t0t.mp3" />
                )}
                {!isAnswerCorrect && (
                  <audio autoPlay src="https://res.cloudinary.com/dqpkxxzaf/video/upload/v1758225103/pokemon-red_blue_yellow-battle-sound-effect-1_jw1t0t.mp3" />
                )}
              </>
            )}
          </>
        )}
      </AnimatePresence>
    </div>
  );
};

export default PetScreen;