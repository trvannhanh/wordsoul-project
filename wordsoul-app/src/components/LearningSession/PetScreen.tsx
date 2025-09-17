import { motion, AnimatePresence } from "framer-motion";
import { type CompleteLearningSessionResponseDto, type CompleteReviewSessionResponseDto } from "../../types/Dto";

interface PetScreenProps {
    showRewardAnimation: boolean;
    captureComplete: boolean;
    setCaptureComplete: (value: boolean) => void; // Thêm prop setCaptureComplete
    encounteredPet: { id: number; name: string; imageUrl: string } | null;
    sessionData: CompleteLearningSessionResponseDto | CompleteReviewSessionResponseDto | null;
    mode: "learning" | "review";
    handleCloseReward: () => void;
}

const PetScreen: React.FC<PetScreenProps> = ({
    showRewardAnimation,
    captureComplete,
    setCaptureComplete, // Nhận setCaptureComplete
    encounteredPet,
    sessionData,
    mode,
    handleCloseReward,
}) => {
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
                {showRewardAnimation && sessionData ? (
                    <motion.div
                        className="flex flex-col items-center justify-center text-center w-full"
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        transition={{ duration: 0.3 }}
                    >
                        {mode === "learning" && "isPetRewardGranted" in sessionData && sessionData.petId && sessionData.isPetRewardGranted && !captureComplete ? (
                            <motion.div
                                className="flex flex-col items-center"
                                initial={{ scale: 0 }}
                                animate={{
                                    scale: 1,
                                    rotate: [0, -10, 10, -10, 0, 10, -10, 0],
                                }}
                                transition={{
                                    scale: { duration: 0.4 },
                                    rotate: { times: [0, 0.2, 0.4, 0.6, 0.7, 0.8, 0.9, 1], duration: 3 },
                                }}
                                onAnimationComplete={() => setCaptureComplete(true)} // Cập nhật trạng thái captureComplete
                            >
                                <img
                                    src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1757509182/PikPng.com_pokeball-sprite-png_4945371_nm8b89.png"
                                    alt="Poké Ball"
                                    className="w-20 h-20 pixel-art"
                                />
                                <p className="text-white text-sm font-pixel mt-2 bg-black bg-opacity-70 p-2 rounded">
                                    Catching {sessionData.petName}...
                                </p>
                                <audio
                                    autoPlay
                                    src="https://res.cloudinary.com/dqpkxxzaf/video/upload/v1757509871/06-caught-a-pokemon_a3r9h1.mp3"
                                />
                            </motion.div>
                        ) : (
                            <div className="flex flex-col items-center justify-center text-center bg-opacity-70 p-4 rounded-lg w-3/4">
                                {mode === "learning" && !("isPetRewardGranted" in sessionData && sessionData.petId && sessionData.isPetRewardGranted) && (
                                    <p className="text-red-500 font-pixel mb-2">No new pet reward available</p>
                                )}
                                <h2 className="text-xl text-white font-pixel mb-2">{sessionData.message}</h2>
                                <p className="text-white font-pixel">XP: {sessionData.xpEarned}</p>
                                {mode === "review" && "apEarned" in sessionData && (
                                    <p className="text-white font-pixel">AP: {sessionData.apEarned}</p>
                                )}
                                {mode === "learning" && "isPetRewardGranted" in sessionData && sessionData.petId && sessionData.isPetRewardGranted && (
                                    <div className="text-center mt-4">
                                        <img
                                            src={sessionData.imageUrl || "https://via.placeholder.com/100"}
                                            alt={sessionData.petName}
                                            className="object-contain max-w-70 mx-auto pixel-art"
                                        />
                                        <h3 className="text-white font-pixel mt-2">Caught {sessionData.petName}!</h3>
                                        <p className="text-white font-pixel">Type: {sessionData.petType}</p>
                                        <p className="text-white font-pixel">Rarity: {sessionData.petRarity}</p>
                                    </div>
                                )}
                                <button
                                    onClick={handleCloseReward}
                                    className="mt-4 bg-emerald-600 px-4 py-2 rounded-lg text-white font-pixel border-2 border-white"
                                >
                                    Đóng
                                </button>
                            </div>
                        )}
                    </motion.div>
                ) : encounteredPet ? (
                    <motion.div
                        className="flex flex-col items-center"
                        initial={{ y: 100, opacity: 0 }}
                        animate={{ y: 0, opacity: 1 }}
                        transition={{ duration: 0.8, type: "spring" }}
                    >
                        <img src={encounteredPet.imageUrl} alt={encounteredPet.name} className="object-contain max-w-70 pixel-art" />
                        <p className="text-white text-sm font-pixel mt-2 bg-black bg-opacity-70 p-2 rounded">Wild {encounteredPet.name} appeared!</p>
                    </motion.div>
                ) : (
                    <div className="text-white font-pixel bg-black bg-opacity-70 p-2 rounded">No wild pet encountered</div>
                )}
            </AnimatePresence>
        </div>
    );
};

export default PetScreen;