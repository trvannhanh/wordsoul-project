import { useEffect, useState, useCallback, useRef } from "react";
import { useParams, useSearchParams, useNavigate, useLocation } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import GameScreen from "../../components/LearningSession/GameScreen";
import AnswerScreen from "../../components/LearningSession/AnswerScreen";
import ReviewLayout, { type ReviewLayoutHandle } from "../../components/LearningSession/ReviewLayout";
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
    confirmAndNext: originalConfirmAndNext,
    sessionData,
    encounteredPet,
    showRewardAnimation,
    captureComplete,
    setCaptureComplete,
    catchRate: currentCatchRate,
    hintBalance,
    setHintBalance,
    comboCount,
    buffPetId,
    buffName,
    buffDescription,
    buffIcon,
    petXpMultiplier,
    petCatchBonus,
    petHintShield,
    petReducePenalty,
  } = useQuizSession(
    sessionId, mode, petId, catchRate,
    currentCorrectAnswered, setCurrentCorrectAnswered,
    initialBuffPetId,
    initialBuffName,
    initialBuffDescription,
    initialBuffIcon,
    initialPetXpMultiplier,
    initialPetCatchBonus,
    initialPetHintShield,
    initialPetReducePenalty
  );

  const { user } = useAuth();
  const [showPopup, setShowPopup] = useState(false);
  const [answeredQuestion, setAnsweredQuestion] = useState<QuizQuestionDto | null>(null);
  const [showIntro, setShowIntro] = useState(true);

  // audio ref for auto-play in vocabulary card
  const cardAudioRef = useRef<HTMLAudioElement | null>(null);
  const audioTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const [isPlayingWordAudio, setIsPlayingWordAudio] = useState(false);
  const [isPlayingExampleAudio, setIsPlayingExampleAudio] = useState(false);

  const [showEncounterIntro, setShowEncounterIntro] = useState(false);
  const encounterShownRef = useRef(false);

  const [activeMilestone, setActiveMilestone] = useState<25 | 50 | 75 | null>(null);
  const reachedMilestones = useRef<Set<number>>(new Set());

  const [userPet, setUserPet] = useState<{
    id: number;
    name: string;
    imageUrl: string;
  } | null>(null);

  const [buffPet, setBuffPet] = useState<PetDto | null>(null);
  const reviewLayoutRef = useRef<ReviewLayoutHandle | null>(null);

  // Cleanup speech synthesis and timeouts on unmount
  useEffect(() => {
    return () => {
      window.speechSynthesis.cancel();
      if (audioTimeoutRef.current) {
        clearTimeout(audioTimeoutRef.current);
      }
    };
  }, []);

  const playWordAudio = useCallback((word: string, pronunciationUrl?: string) => {
    if (isPlayingWordAudio) return;
    setIsPlayingWordAudio(true);
    if (pronunciationUrl) {
      const audio = new Audio(pronunciationUrl);
      audio.onended = () => setIsPlayingWordAudio(false);
      audio.onerror = () => {
        const utterance = new SpeechSynthesisUtterance(word);
        utterance.lang = "en-US";
        utterance.onend = () => setIsPlayingWordAudio(false);
        window.speechSynthesis.speak(utterance);
      };
      audio.play().catch(() => {
        const utterance = new SpeechSynthesisUtterance(word);
        utterance.lang = "en-US";
        utterance.onend = () => setIsPlayingWordAudio(false);
        window.speechSynthesis.speak(utterance);
      });
    } else {
      const utterance = new SpeechSynthesisUtterance(word);
      utterance.lang = "en-US";
      utterance.onend = () => setIsPlayingWordAudio(false);
      window.speechSynthesis.speak(utterance);
    }
  }, [isPlayingWordAudio]);

  const playExampleAudio = useCallback((text: string, audioUrl?: string) => {
    if (isPlayingExampleAudio) return;
    setIsPlayingExampleAudio(true);
    if (audioUrl) {
      const audio = new Audio(audioUrl);
      audio.onended = () => setIsPlayingExampleAudio(false);
      audio.onerror = () => {
        const utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = "en-US";
        utterance.onend = () => setIsPlayingExampleAudio(false);
        window.speechSynthesis.speak(utterance);
      };
      audio.play().catch(() => {
        const utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = "en-US";
        utterance.onend = () => setIsPlayingExampleAudio(false);
        window.speechSynthesis.speak(utterance);
      });
    } else {
      const utterance = new SpeechSynthesisUtterance(text);
      utterance.lang = "en-US";
      utterance.onend = () => setIsPlayingExampleAudio(false);
      window.speechSynthesis.speak(utterance);
    }
  }, [isPlayingExampleAudio]);

  useEffect(() => {
    const timer = setTimeout(() => setShowIntro(false), 2000);
    return () => clearTimeout(timer);
  }, []);

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

  useEffect(() => {
    const fetchBuffPet = async () => {
      if (typeof buffPetId === "number") {
        try {
          const pet = await fetchPetById(buffPetId);
          setBuffPet(pet as unknown as PetDto);
        } catch (err) {
          console.warn("Failed to load buff pet:", err);
        }
      }
    };
    fetchBuffPet();
  }, [buffPetId]);

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
      onResult?: (isCorrect: boolean) => void,
      responseTimeSeconds?: number,
      usedHintCount?: number
    ): Promise<boolean> => {
      return originalHandleAnswer(question, answer, onResult, responseTimeSeconds, usedHintCount);
    },
    [originalHandleAnswer]
  );

  const confirmAndNext = useCallback(() => {
    setShowPopup(false);
    setAnsweredQuestion(null);
    if (audioTimeoutRef.current) {
      clearTimeout(audioTimeoutRef.current);
      audioTimeoutRef.current = null;
    }
    if (cardAudioRef.current) {
      cardAudioRef.current.pause();
      cardAudioRef.current.currentTime = 0;
    }
    // Cancel SpeechSynthesis and reset playing flags
    window.speechSynthesis.cancel();
    setIsPlayingWordAudio(false);
    setIsPlayingExampleAudio(false);

    if (mode === "review" && reviewLayoutRef.current) {
      reviewLayoutRef.current.triggerBerryDrop();
    }

    originalConfirmAndNext();
  }, [originalConfirmAndNext, mode]);

  const handleShowPopup = useCallback((question: QuizQuestionDto) => {
    setAnsweredQuestion(question);
    setShowPopup(true);

    if (audioTimeoutRef.current) {
      clearTimeout(audioTimeoutRef.current);
    }

    // Auto-play pronunciation audio with a 0.6-second delay to avoid clashing with feedback sounds
    audioTimeoutRef.current = setTimeout(() => {
      if (question.pronunciationUrl) {
        const audio = new Audio(question.pronunciationUrl);
        cardAudioRef.current = audio;
        audio.play().catch(() => { /* autoplay may be blocked on some browsers */ });
      }
    }, 600);
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

  // ─── Render gates ────────────────────────────────────────────────
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

      {/* ── Buff Badge ── */}
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

      {/* ── Main container ── */}
      <div className="w-full h-full bg-gray-800 border-4 border-black rounded-lg flex flex-col overflow-hidden">

        {/* Progress bar — always shown */}
        <PokemonProgressBar
          currentCorrectAnswered={currentCorrectAnswered}
          maxQuestions={MAX_QUESTIONS}
          catchRate={currentCatchRate}
          encounteredPet={mode === "learning" ? encounteredPet : null}
        />

        {/* ── LEARNING MODE ── */}
        {mode === "learning" && (
          <>
            <div className="flex-1 bg-gray-700 border-b-4 border-black p-4 h-1/2">
              <div className="h-full bg-black border-2 border-white rounded-sm flex items-center justify-center">
                <GameScreen
                  question={currentQuestion}
                  loading={loading}
                  error={error}
                  mode="learning"
                />
              </div>
            </div>

            <div className="flex-1 bg-gray-700 p-4 h-1/2">
              <div className="h-full bg-black border-2 border-white rounded-sm flex items-center justify-center overflow-hidden">
                <AnswerScreen
                  question={currentQuestion}
                  loading={loading}
                  error={error}
                  handleAnswer={handleAnswer}
                  confirmAndNext={confirmAndNext}
                  showPopup={handleShowPopup}
                  hintBalance={hintBalance}
                  setHintBalance={setHintBalance}
                  comboCount={comboCount}
                />
              </div>
            </div>
          </>
        )}

        {/* ── REVIEW MODE ── */}
        {mode === "review" && (
          <div className="flex-1 overflow-hidden">
            <ReviewLayout
              ref={reviewLayoutRef}
              question={currentQuestion}
              loading={loading}
              error={error}
              handleAnswer={handleAnswer}
              confirmAndNext={confirmAndNext}
              showPopup={handleShowPopup}
              hintBalance={hintBalance}
              setHintBalance={setHintBalance}
              userPet={userPet}
              currentCorrectAnswered={currentCorrectAnswered}
              maxQuestions={MAX_QUESTIONS}
              comboCount={comboCount}
            />
          </div>
        )}
      </div>

      {/* ── Milestone Overlay ── */}
      {activeMilestone && (
        <MilestoneOverlay
          milestone={activeMilestone}
          encounteredPet={mode === "learning" ? encounteredPet : null}
          onClose={() => setActiveMilestone(null)}
        />
      )}

      {/* ── End-of-session PetScreen ── */}
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

      {/* ── Word detail popup — stays until user confirms "Tiếp theo" ── */}
      <AnimatePresence>
        {showPopup && answeredQuestion && (
          <motion.div
            className="absolute inset-0 flex items-center justify-center bg-transparent z-50"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.25 }}
          >
            <motion.div
              className="bg-gray-800 p-8 rounded-xl border-4 border-indigo-500 text-white font-pixel text-center w-11/12 max-w-lg shadow-2xl"
              initial={{ scale: 0.85, y: 40 }}
              animate={{ scale: 1, y: 0 }}
              exit={{ scale: 0.85, y: 40 }}
              transition={{ type: "spring", stiffness: 280, damping: 22 }}
            >
              {/* Word Header with Speaker and CEFR Level */}
              <div className="flex items-center justify-center gap-3 mb-3 flex-wrap">
                <h2 className="text-4xl text-yellow-300 font-bold">{answeredQuestion.word}</h2>
                {answeredQuestion.cefrLevel && (
                  <span className="text-xs bg-indigo-900 border border-indigo-600 px-2 py-0.5 rounded text-indigo-300 font-pixel font-bold uppercase">
                    {answeredQuestion.cefrLevel}
                  </span>
                )}
                <button
                  onClick={() => playWordAudio(answeredQuestion.word, answeredQuestion.pronunciationUrl)}
                  disabled={isPlayingWordAudio}
                  className="text-yellow-400 hover:text-yellow-300 disabled:opacity-50 transition-colors p-1"
                  title="Phát âm từ"
                >
                  {isPlayingWordAudio ? (
                    <svg className="w-6 h-6 animate-pulse" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M9.383 3.076A1 1 0 0110 4v12a1 1 0 01-1.617.784L4.586 13H2a1 1 0 01-1-1V8a1 1 0 011-1h2.586l3.797-3.784a1 1 0 011 .076zM12.293 7.293a1 1 0 011.414 0A5.003 5.003 0 0115 10a5.003 5.003 0 01-1.293 2.707 1 1 0 01-1.414-1.414A3.003 3.003 0 0013 10a3.003 3.003 0 00-.707-1.293 1 1 0 010-1.414z" clipRule="evenodd" />
                    </svg>
                  ) : (
                    <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M9.383 3.076A1 1 0 0110 4v12a1 1 0 01-1.617.784L4.586 13H2a1 1 0 01-1-1V8a1 1 0 011-1h2.586l3.797-3.784a1 1 0 011 .076z" clipRule="evenodd" />
                    </svg>
                  )}
                </button>
              </div>

              <p className="text-xl mb-2 text-gray-200">Nghĩa: {answeredQuestion.meaning}</p>

              <div className="flex justify-center gap-4 text-xs mb-4 text-gray-400">
                {answeredQuestion.pronunciation && (
                  <span className="text-blue-300 font-mono">
                    [{answeredQuestion.pronunciation}]
                  </span>
                )}
                {answeredQuestion.partOfSpeech && (
                  <span className="text-amber-300 italic">
                    ({answeredQuestion.partOfSpeech})
                  </span>
                )}
              </div>

              {answeredQuestion.imageUrl && (
                <img
                  src={answeredQuestion.imageUrl}
                  alt={answeredQuestion.word}
                  className="w-32 h-32 object-contain mx-auto mb-4 rounded border border-gray-600 shadow"
                />
              )}

              {/* Description */}
              {answeredQuestion.description && (
                <div className="border-l-4 border-emerald-500 pl-3 bg-gray-900 bg-opacity-40 p-3 rounded mb-3 text-left">
                  <p className="text-xs text-emerald-400 font-pixel uppercase tracking-wider mb-1">Định nghĩa</p>
                  <p className="text-sm text-gray-200 leading-relaxed font-sans">
                    {answeredQuestion.description}
                  </p>
                </div>
              )}

              {/* Example sentence */}
              {answeredQuestion.exampleSentence && (
                <div className="flex items-start gap-2 border-l-4 border-indigo-500 pl-3 bg-gray-900 bg-opacity-40 p-3 rounded mb-4 text-left">
                  <div className="flex-1">
                    <p className="text-xs text-indigo-400 font-pixel uppercase tracking-wider mb-1">Ví dụ</p>
                    <p className="text-sm italic text-gray-300 leading-relaxed font-sans">
                      "{answeredQuestion.exampleSentence}"
                    </p>
                  </div>
                  <button
                    onClick={() => playExampleAudio(
                      answeredQuestion.exampleSentence!,
                      answeredQuestion.exampleSentenceAudioUrl
                    )}
                    disabled={isPlayingExampleAudio}
                    className="text-teal-400 hover:text-teal-300 disabled:opacity-50 transition-colors p-1 flex-shrink-0 self-center"
                    title="Phát câu ví dụ"
                  >
                    {isPlayingExampleAudio ? (
                      <svg className="w-5 h-5 animate-pulse" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M9.383 3.076A1 1 0 0110 4v12a1 1 0 01-1.617.784L4.586 13H2a1 1 0 01-1-1V8a1 1 0 011-1h2.586l3.797-3.784a1 1 0 011 .076zM12.293 7.293a1 1 0 011.414 0A5.003 5.003 0 0115 10a5.003 5.003 0 01-1.293 2.707 1 1 0 01-1.414-1.414A3.003 3.003 0 0013 10a3.003 3.003 0 00-.707-1.293 1 1 0 010-1.414z" clipRule="evenodd" />
                      </svg>
                    ) : (
                      <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                        <path fillRule="evenodd" d="M9.383 3.076A1 1 0 0110 4v12a1 1 0 01-1.617.784L4.586 13H2a1 1 0 01-1-1V8a1 1 0 011-1h2.586l3.797-3.784a1 1 0 011 .076z" clipRule="evenodd" />
                      </svg>
                    )}
                  </button>
                </div>
              )}

              <motion.button
                onClick={confirmAndNext}
                className="mt-2 px-8 py-3 bg-indigo-700 border-2 border-indigo-400 rounded-xl font-pixel text-white text-base hover:bg-indigo-600 transition-colors custom-cursor shadow-lg w-full"
                whileHover={{ scale: 1.03 }}
                whileTap={{ scale: 0.96 }}
                initial={{ opacity: 0, y: 8 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.2 }}
              >
                Tiếp theo ▶
              </motion.button>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Reward Complete overlay */}
      <AnimatePresence>
        {showRewardAnimation && sessionData && captureComplete && (
          <motion.div
            className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-80 z-50"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.3 }}
          >
            <div className="flex flex-col items-center justify-center text-center bg-opacity-70 p-4 rounded-lg w-3/4 h-3/4 max-w-4xl bg-gray-800 border-4 border-white">
              {mode === "learning" &&
                !("isPetRewardGranted" in sessionData && sessionData.isPetRewardGranted) && (
                  <motion.p
                    className="text-red-400 font-pixel mb-4"
                    initial={{ scale: 0.8, opacity: 0 }}
                    animate={{ scale: 1, opacity: 1 }}
                  >
                    {"petName" in sessionData && sessionData.petName
                      ? `${sessionData.petName} đã bỏ trốn!`
                      : "No new pet reward available"}
                  </motion.p>
                )}
              {/* Review mode: friendly "pet is full" message */}
              {mode === "review" && (
                <motion.div
                  className="flex flex-col items-center gap-2 mb-4"
                  initial={{ scale: 0.8, opacity: 0 }}
                  animate={{ scale: 1, opacity: 1 }}
                >
                  {userPet ? (
                    <>
                      <motion.img
                        src={`https://img.pokemondb.net/sprites/black-white/anim/normal/${userPet.name.toLowerCase()}.gif`}
                        alt={userPet.name}
                        className="w-24 h-24 object-contain"
                        style={{ imageRendering: "pixelated" }}
                        animate={{ y: [0, -6, 0] }}
                        transition={{ duration: 1.8, repeat: Infinity, ease: "easeInOut" }}
                        onError={(e) => { e.currentTarget.src = userPet.imageUrl; }}
                      />
                      <motion.p
                        className="text-green-300 font-pixel text-lg"
                        animate={{ scale: [1, 1.05, 1] }}
                        transition={{ duration: 2, repeat: Infinity }}
                      >
                        {userPet.name} đã no rồi!
                      </motion.p>
                    </>
                  ) : (
                    <motion.p
                      className="text-green-300 font-pixel text-2xl"
                      animate={{ scale: [1, 1.05, 1] }}
                      transition={{ duration: 2, repeat: Infinity }}
                    >
                      Phiên ôn tập hoàn thành!
                    </motion.p>
                  )}
                </motion.div>
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
                  Kinh nghiệm: +{sessionData.xpEarned}
                </motion.p>
                {mode === "review" && "apEarned" in sessionData && (
                  <motion.p className="text-blue-400 font-pixel">
                    Điểm nâng cấp: +{sessionData.apEarned}
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
                Đóng
              </motion.button>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default LearningSession;