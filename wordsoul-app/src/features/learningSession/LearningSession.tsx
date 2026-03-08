import { useEffect, useState, useCallback, useRef } from "react";
import { useParams, useSearchParams, useNavigate, useLocation } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import GameScreen from "../../components/LearningSession/GameScreen";
import AnswerScreen from "../../components/LearningSession/AnswerScreen";
import PetScreen from "../../components/LearningSession/PetScreen";
import PokemonEncounterIntro from "../../components/LearningSession/PokemonEncounterIntro";
import MilestoneOverlay from "../../components/LearningSession/MilestoneOverlay";
import PokemonProgressBar from "../../components/LearningSession/PokemonProgressBar";
import BuffBadge from "../../components/LearningSession/BuffBadge";
import { useQuizSession } from "../../hooks/LearningSession/useQuizSession";
import type { QuizQuestionDto } from "../../types/LearningSessionDto";
import type { PetDto } from "../../types/PetDto";
import LoadingScreen from "../../components/LearningSession/LoadingScreen";
import { useAuth } from "../../hooks/Auth/useAuth";
import { fetchPetById } from "../../services/pet";

const MILESTONE_THRESHOLDS = [5, 10, 15] as const;
const MAX_QUESTIONS = 20;

const LearningSession: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [searchParams] = useSearchParams();
  const mode = searchParams.get("mode") as "learning" | "review";
  const sessionId = Number(id);
  const { state } = useLocation();
  const navigate = useNavigate();
  const petId = state?.petId;
  const catchRate = state?.catchRate;
  const initialBuffPetId = state?.buffPetId;
  const initialBuffName = state?.buffName;
  const initialBuffDescription = state?.buffDescription;
  const initialBuffIcon = state?.buffIcon;
  const initialPetXpMultiplier = state?.petXpMultiplier;
  const initialPetCatchBonus = state?.petCatchBonus;
  const initialPetHintShield = state?.petHintShield;
  const initialPetReducePenalty = state?.petReducePenalty;

  const [currentCorrectAnswered, setCurrentCorrectAnswered] = useState(
    state?.currentCorrectAnswered || 0
  );

  const {
    currentQuestion,
    loading,
    error,
    handleAnswer: originalHandleAnswer,
    sessionData,
    encounteredPet,
    showRewardAnimation,
    captureComplete,
    setCaptureComplete,
    loadNextQuestion,
    catchRate: currentCatchRate,
    // ── NEW: buff fields exposed from hook ──
    buffPetId,
    buffName,
    buffDescription,
    buffIcon,
    petXpMultiplier,
    petCatchBonus,
    petHintShield,
    petReducePenalty,
  } = useQuizSession(sessionId, mode, petId, catchRate,
    currentCorrectAnswered, setCurrentCorrectAnswered,
    initialBuffPetId,
    initialBuffName,
    initialBuffDescription,
    initialBuffIcon,
    initialPetXpMultiplier,
    initialPetCatchBonus,
    initialPetHintShield,
    initialPetReducePenalty);

  // console.group("🗺️ [LearningSession] Location State");
  // console.log("🐾 petId:             ", petId);
  // console.log("🎯 catchRate:         ", catchRate);
  // console.log("🦄 initialBuffPetId:  ", initialBuffPetId);
  // console.log("⭐ initialBuffName:   ", initialBuffName);
  // console.log("📝 initialBuffDesc:   ", initialBuffDescription);
  // console.log("🎨 initialBuffIcon:   ", initialBuffIcon);
  // console.log("💰 xpMultiplier:      ", initialPetXpMultiplier);
  // console.log("🎯 catchBonus:        ", initialPetCatchBonus);
  // console.log("🔮 hintShield:        ", initialPetHintShield);
  // console.log("🪨 reducePenalty:     ", initialPetReducePenalty);
  // console.log("📦 Full state:        ", state);
  // console.groupEnd();

  const { user } = useAuth();
  const [showPopup, setShowPopup] = useState(false);
  const [answeredQuestion, setAnsweredQuestion] = useState<QuizQuestionDto | null>(null);
  const [showIntro, setShowIntro] = useState(true);

  // Encounter intro: shown once after loading, only in learning mode with a pet
  const [showEncounterIntro, setShowEncounterIntro] = useState(false);
  const encounterShownRef = useRef(false);

  // Milestone overlay
  const [activeMilestone, setActiveMilestone] = useState<25 | 50 | 75 | null>(null);
  const reachedMilestones = useRef<Set<number>>(new Set());

  // User's active pet
  const [userPet, setUserPet] = useState<{
    id: number;
    name: string;
    imageUrl: string;
  } | null>(null);

  // ── NEW: buff pet data fetched from API ──
  const [buffPet, setBuffPet] = useState<PetDto | null>(null);

  // Loading screen visible for 1.2 s
  useEffect(() => {
    const timer = setTimeout(() => setShowIntro(false), 1200);
    return () => clearTimeout(timer);
  }, []);

  // Fetch user's active pet
  useEffect(() => {
    const fetchPet = async () => {
      if (typeof user?.petActiveId === "number") {
        try {
          const pet = await fetchPetById(user.petActiveId);
          setUserPet({
            id: pet.id,
            name: pet.name,
            imageUrl: pet.imageUrl || "https://via.placeholder.com/100",
          });
        } catch (petError) {
          console.warn("Failed to load pet:", petError);
        }
      }
    };
    fetchPet();
  }, [user?.petActiveId]);

  // ── NEW: Fetch buff pet info when buffPetId is available ──
  useEffect(() => {
    const fetchBuffPet = async () => {
      if (typeof buffPetId === "number") {
        try {
          const pet = await fetchPetById(buffPetId);


          // console.group("🦄 [BuffPet] Fetched");
          // console.log("🆔 ID:      ", pet.id);
          // console.log("📛 Name:    ", pet.name);
          // console.log("🏷️  Type:    ", (pet as any).type);
          // console.log("💎 Rarity:  ", (pet as any).rarity);
          // console.log("🖼️  ImageUrl:", pet.imageUrl);
          // console.log("📦 Full:    ", pet);
          // console.groupEnd();

          setBuffPet(pet as unknown as PetDto);
        } catch (err) {
          console.warn("Failed to load buff pet:", err);
        }
      }
    };
    fetchBuffPet();
  }, [buffPetId]);

  // Show encounter intro once encounteredPet is loaded (learning mode only)
  useEffect(() => {
    if (
      mode === "learning" &&
      encounteredPet &&
      !showIntro &&
      !encounterShownRef.current
    ) {
      encounterShownRef.current = true;
      setShowEncounterIntro(true);
    }
  }, [encounteredPet, showIntro, mode]);

  // Milestone detection
  useEffect(() => {
    if (showEncounterIntro || showIntro) return;
    const thresholdMap: Record<number, 25 | 50 | 75> = { 5: 25, 10: 50, 15: 75 };
    for (const threshold of MILESTONE_THRESHOLDS) {
      if (
        currentCorrectAnswered >= threshold &&
        !reachedMilestones.current.has(threshold) &&
        !showRewardAnimation
      ) {
        reachedMilestones.current.add(threshold);
        setActiveMilestone(thresholdMap[threshold]);
        break;
      }
    }
  }, [currentCorrectAnswered, showEncounterIntro, showIntro, showRewardAnimation]);

  const handleCloseReward = () => navigate(-1);

  const handleAnswer = useCallback(
    async (
      question: QuizQuestionDto,
      answer: string,
      onAnswerProcessed: () => void,
      onResult?: (isCorrect: boolean) => void,
      responseTimeSeconds?: number
    ): Promise<boolean> => {
      return originalHandleAnswer(question, answer, onAnswerProcessed, onResult, responseTimeSeconds);
    },
    [originalHandleAnswer]
  );

  const handleShowPopup = useCallback((question: QuizQuestionDto) => {
    setAnsweredQuestion(question);
    setShowPopup(true);
    setTimeout(() => {
      setShowPopup(false);
      setAnsweredQuestion(null);
    }, 3000);
  }, []);

  const getMessage = () => {
    if (mode === "learning" && sessionData && "isPetAlreadyOwned" in sessionData) {
      if (sessionData.isPetAlreadyOwned) return "Bạn đã sở hữu pet này!";
      if (sessionData.isPetRewardGranted)
        return `Chúc mừng! Bạn đã bắt được ${sessionData.petName}!`;
      return `${sessionData.petName} đã bỏ trốn!`;
    }
    return sessionData?.message || "Hoàn thành phiên học!";
  };

  // ─── Render gates ───────────────────────────────────────────────
  if (showIntro) return <LoadingScreen />;

  if (showEncounterIntro) {
    return (
      <PokemonEncounterIntro
        encounteredPet={encounteredPet}
        onComplete={() => setShowEncounterIntro(false)}
      />
    );
  }

  return (
    <div className="h-screen w-screen bg-gray-900 flex flex-col items-center justify-between pixel-background relative overflow-hidden">

      {/* ── Buff Badge (top-right, always visible during session) ── */}
      {(buffName || buffPet) && (
        <BuffBadge
          buffPet={buffPet}
          buffName={buffName}
          buffDescription={buffDescription}
          buffIcon={buffIcon}
          petXpMultiplier={petXpMultiplier}
          petCatchBonus={petCatchBonus}
          petHintShield={petHintShield}
          petReducePenalty={petReducePenalty}
        />
      )}

      {/* ── Main Learning Container (full width) ── */}
      <div className="w-full h-full bg-gray-800 border-4 border-black rounded-lg flex flex-col overflow-hidden">
        {/* Progress Bar */}
        <PokemonProgressBar
          currentCorrectAnswered={currentCorrectAnswered}
          maxQuestions={MAX_QUESTIONS}
          catchRate={currentCatchRate}
          encounteredPet={encounteredPet}
        />

        {/* Game (question display) */}
        <div className="flex-1 bg-gray-700 border-b-4 border-black p-4 h-1/2">
          <div className="h-full bg-black border-2 border-white rounded-sm flex items-center justify-center">
            <GameScreen
              question={currentQuestion}
              loading={loading}
              error={error}
            />
          </div>
        </div>

        {/* Answer options */}
        <div className="flex-1 bg-gray-700 p-4 h-1/2">
          <div className="h-full bg-black border-2 border-white rounded-sm flex items-center justify-center">
            <AnswerScreen
              question={currentQuestion}
              loading={loading}
              error={error}
              handleAnswer={handleAnswer}
              loadNextQuestion={loadNextQuestion}
              showPopup={handleShowPopup}
            />
          </div>
        </div>
      </div>



      {/* ── Milestone Overlay ── */}
      {activeMilestone && (
        <MilestoneOverlay
          milestone={activeMilestone}
          encounteredPet={encounteredPet}
          onClose={() => setActiveMilestone(null)}
        />
      )}

      {/* ── End-of-session PetScreen (fullscreen overlay) ── */}
      {showRewardAnimation && (
        <PetScreen
          showRewardAnimation={showRewardAnimation}
          captureComplete={captureComplete}
          setCaptureComplete={setCaptureComplete}
          encounteredPet={encounteredPet}
          userPet={userPet}
          sessionData={sessionData}
          mode={mode}
          petId={petId}
          catchRate={currentCatchRate}
          handleCloseReward={handleCloseReward}
          showBattleAnimation={false}
          isAnswerCorrect={null}
        />
      )}

      {/* ── Word detail popup ── */}
      <AnimatePresence>
        {showPopup && answeredQuestion && (
          <motion.div
            className="absolute inset-0 flex items-center justify-center bg-opacity-75 z-50"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3 }}
          >
            <motion.div
              className="bg-gray-800 p-8 rounded-lg border-4 border-white text-white font-pixel text-center w-3/4 max-w-lg"
              initial={{ scale: 0, y: 50 }}
              animate={{ scale: 1, y: 0 }}
              exit={{ scale: 0, y: 50 }}
              transition={{ duration: 0.3 }}
            >
              <h2 className="text-4xl mb-4">{answeredQuestion.word}</h2>
              <p className="text-2xl mb-2">Nghĩa: {answeredQuestion.meaning}</p>
              <p className="text-lg mb-2">
                Phát âm: {answeredQuestion.pronunciation || "N/A"}
              </p>
              <p className="text-lg mb-2">
                Loại từ: {answeredQuestion.partOfSpeech || "N/A"}
              </p>
              {answeredQuestion.imageUrl && (
                <img
                  src={answeredQuestion.imageUrl}
                  alt={answeredQuestion.word}
                  className="w-48 h-48 object-contain mx-auto mt-4"
                />
              )}
            </motion.div>
          </motion.div>
        )}

        {/* Reward Complete Screen Overlay */}
        {showRewardAnimation && sessionData && captureComplete && (
          <motion.div
            className="absolute inset-0 flex items-center justify-center bg-opacity-75 z-50"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3 }}
          >
            <div className="flex flex-col items-center justify-center text-center bg-opacity-70 p-4 rounded-lg w-3/4 h-3/4 max-w-4xl bg-gray-800 border-4 border-white">
              {mode === "learning" &&
                !("isPetRewardGranted" in sessionData && sessionData.isPetRewardGranted) && (
                  <motion.p
                    className="text-red-500 font-pixel mb-4"
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
                <motion.p className="text-green-400 font-pixel">
                  💰 XP: +{sessionData.xpEarned}
                </motion.p>
                {mode === "review" && "apEarned" in sessionData && (
                  <motion.p className="text-blue-400 font-pixel">
                    💎 AP: +{sessionData.apEarned}
                  </motion.p>
                )}
              </div>

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
                      src={`https://img.pokemondb.net/sprites/black-white/anim/normal/${encounteredPet?.name.toLowerCase()}.gif`}
                      alt={sessionData.petName}
                      className="w-50 h-50 object-contain pixel-art rounded-lg border-2 border-yellow-400"
                      onError={(e) => {
                        e.currentTarget.src = encounteredPet?.imageUrl ?? "";
                      }}
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
                className="bg-emerald-600 px-6 py-3 rounded-lg text-white font-pixel border-2 border-white hover:bg-emerald-700 transition-colors custom-cursor"
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
                initial={{ y: 20, opacity: 0 }}
                animate={{ y: 0, opacity: 1 }}
                transition={{ delay: 0.8 }}
              >
                🎉 Đóng & Về Dashboard
              </motion.button>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default LearningSession;