import { motion, AnimatePresence } from "framer-motion";
import type { CompleteLearningSessionResponseDto, CompleteReviewSessionResponseDto, QuizQuestionDto } from "../../types/LearningSessionDto";


interface PetScreenProps {
  showRewardAnimation: boolean;
  captureComplete: boolean;
  setCaptureComplete: (value: boolean) => void;
  encounteredPet: { id: number; name: string; imageUrl: string } | null;
  sessionData: CompleteLearningSessionResponseDto | CompleteReviewSessionResponseDto | null;
  mode: "learning" | "review";
  petId?: number;
  handleCloseReward: () => void;
  currentQuestion?: QuizQuestionDto | null;
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
  // Placeholder pet khi chưa có sessionData
  const getPlaceholderPet = () => {
    if (mode === "review") {
      return {
        id: petId || 0, // Sử dụng 0 nếu petId không có
        name: "Zapdos",
        imageUrl: "https://res.cloudinary.com/dqpkxxzaf/image/upload/v1757601990/Zapdos-removebg-preview_ztjvlh.png",
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

  return (
    <div
      className="w-1/2 h-3/4 bg-gray-700 border-4 border-black rounded-lg flex flex-col items-center justify-center overflow-hidden p-4"
      style={{
        backgroundImage: `url('https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756297202/vocabulary_sets/banner4_duprqr.gif')`,
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
                  ease: "easeInOut",
                }}
                onAnimationComplete={() => setCaptureComplete(true)}
              >
                <img
                  src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1757601990/Zapdos-removebg-preview_ztjvlh.png"
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
                      No new pet reward available
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
                  {sessionData.message}
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

                {/* Pet Reward Display (chỉ cho mode learning) */}
                {mode === "learning" &&
                  "isPetRewardGranted" in sessionData &&
                  sessionData.petId &&
                  sessionData.isPetRewardGranted && (
                    <motion.div
                      className="flex flex-col items-center space-y-2 mb-6 p-4 bg-yellow-900 bg-opacity-80 rounded-lg"
                      initial={{ scale: 0, rotate: 180 }}
                      animate={{ scale: 1, rotate: 0 }}
                      transition={{ delay: 0.5, type: "spring" }}
                    >
                      <img
                        src={sessionData.imageUrl || "https://via.placeholder.com/100"}
                        alt={sessionData.petName}
                        className="w-50 h-50 object-contain pixel-art rounded-lg border-2 border-yellow-400"
                      />
                      <h3 className="text-yellow-300 font-pixel text-lg">
                        {sessionData.petName}
                      </h3>
                      <div className="text-xs text-yellow-200 space-y-1">
                        <p>Type: {sessionData.petType}</p>
                        <p>Rarity: {sessionData.petRarity}</p>
                      </div>
                    </motion.div>
                  )}
                
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
        ) : 
        // Session đang diễn ra - Hiển thị pet preview hoặc placeholder
        (encounteredPet || placeholderPet) ? (
          <motion.div
            className="flex flex-col items-center justify-center w-full h-full"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ duration: 0.5 }}
          >
            {/* Pet Image */}
            <motion.img
              src={encounteredPet?.imageUrl || placeholderPet?.imageUrl}
              alt={encounteredPet?.name || placeholderPet?.name}
              className="w-100 h-100 object-contain pixel-art rounded-lg mb-3"
              initial={{ scale: 0.8, rotate: 180 }}
              animate={{ scale: 1, rotate: 0 }}
              whileHover={{ scale: 1.1, rotate: 5 }}
              transition={{ type: "spring", stiffness: 300 }}
            />
            
            {/* Pet Info */}
            <motion.div
              className="text-center space-y-2"
              initial={{ y: 20, opacity: 0 }}
              animate={{ y: 0, opacity: 1 }}
              transition={{ delay: 0.2 }}
            >
              <h3 className="text-white font-pixel text-lg">
                {encounteredPet?.name || placeholderPet?.name}
              </h3>
              <p className="text-gray-300 font-pixel text-sm bg-black bg-opacity-50 px-2 py-1 rounded">
                {mode === "learning"
                  ? "Một sinh vật hoang dã xuất hiện"
                  : "Vị thần bảo hộ trong truyền thuyết đã xuất hiện"}
              </p>
            </motion.div>
          </motion.div>
        ) : (
          // No pet - Placeholder
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
      </AnimatePresence>
    </div>
  );
};

export default PetScreen;