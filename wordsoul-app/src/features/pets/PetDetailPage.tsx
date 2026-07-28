import { useEffect, useState, useRef } from 'react';
import { useParams } from 'react-router-dom';
import { activePet, fetchPetDetailById, upgradePet } from '../../services/pet';
import { motion, AnimatePresence } from 'framer-motion';
import ProfileCard from '../../shared/components/UserProfile/ProfileCard';
import { typeBackgrounds, type PetDetailDto, type UpgradePetResponseDto } from '../../types/PetDto';
import type { UserDto } from '../../types/UserDto';
import { extractApiError } from '../../shared/errors';
import { toast } from '../../shared/toast';
import { useAuth } from '../../hooks/Auth/useAuth';

type BerryType = "oran" | "sitrus" | "pecha";

const BERRY_SRCS: Record<BerryType, string> = {
  oran: "https://res.cloudinary.com/dqpkxxzaf/image/upload/v1773418354/Grid_Oran_Berry_sfv4dd.png",
  sitrus: "https://res.cloudinary.com/dqpkxxzaf/image/upload/v1773418324/Grid_Sitrus_Berry_hu699y.png",
  pecha: "https://res.cloudinary.com/dqpkxxzaf/image/upload/v1773418354/Grid_Occa_Berry_drgsnw.png",
};

interface BerryParticle {
  id: number;
  x: number;       // % from left in drop-zone
  type: BerryType;
}

const BERRY_TYPES: BerryType[] = ["oran", "sitrus", "pecha"];
const EVOLUTION_SPARKLES = Array.from({ length: 36 }, (_, index) => ({
  id: index,
  x: 8 + ((index * 29) % 84),
  y: 10 + ((index * 43) % 78),
  size: 2 + (index % 4),
  delay: (index % 9) * 0.06,
}));
function randomBerry(): BerryType {
  return BERRY_TYPES[Math.floor(Math.random() * BERRY_TYPES.length)];
}

const FallingBerry: React.FC<{
  particle: BerryParticle;
  onDone: (id: number) => void;
}> = ({ particle, onDone }) => {
  return (
    <motion.div
      className="absolute top-0 pointer-events-none z-20"
      style={{ left: `${particle.x}%` }}
      initial={{ y: -36, opacity: 1, rotate: -20, scale: 1.15 }}
      animate={{
        y: ["0%", "70%", "85%", "100%"],
        opacity: [1, 1, 0.7, 0],
        rotate: [-20, 8, -6, 4],
        scale: [1.15, 1, 0.8, 0.45],
      }}
      transition={{ duration: 1.05, ease: "easeIn", times: [0, 0.5, 0.78, 1] }}
      onAnimationComplete={() => onDone(particle.id)}
    >
      <img
        src={BERRY_SRCS[particle.type]}
        alt={particle.type}
        className="w-12 h-12 object-contain"
        style={{ imageRendering: "pixelated" }}
      />
    </motion.div>
  );
};

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

const PetDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [pet, setPet] = useState<PetDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isUpgrading, setIsUpgrading] = useState<boolean>(false);
  const [isActive, setIsActive] = useState<boolean>(false);
  const [levelUpAnimation, setLevelUpAnimation] = useState<boolean>(false);
  const [evolveAnimation, setEvolveAnimation] = useState<boolean>(false);
  const [currentImage, setCurrentImage] = useState<string | undefined>(undefined);
  const { user, setUser } = useAuth();

  const [berries, setBerries] = useState<BerryParticle[]>([]);
  const [eatBurst, setEatBurst] = useState<boolean>(false);
  const [isEating, setIsEating] = useState<boolean>(false);
  const berryIdRef = useRef<number>(0);

  // Chọn background dựa trên pet.type, mặc định là pet-background
  const backgroundClass = pet?.type ? typeBackgrounds[pet.type] || "pet-background" : "pet-background";

  const dropBerries = (count: number) => {
    const fresh: BerryParticle[] = Array.from({ length: count }, () => ({
      id: ++berryIdRef.current,
      x: 15 + Math.random() * 70,
      type: randomBerry(),
    }));
    setBerries((p) => [...p, ...fresh]);
  };

  const removeBerry = (id: number) => {
    setBerries((p) => p.filter((b) => b.id !== id));
  };

  useEffect(() => {
    const loadPet = async () => {
      setIsLoading(true);
      try {
        const data = await fetchPetDetailById(Number(id));
        setPet(data);
        setCurrentImage(data.imageUrl);
      } catch (err: unknown) {
        setError(extractApiError(err).message);
      } finally {
        setIsLoading(false);
      }
    };
    if (id) {
      loadPet();
    }
  }, [id]);

  const handleUpgrade = async () => {
    if (!pet || isUpgrading) return;
    setIsUpgrading(true);

    // 1. Kích hoạt hiệu ứng quả rơi
    dropBerries(2 + Math.floor(Math.random() * 2));

    try {
      const response: UpgradePetResponseDto = await upgradePet(pet.id);

      // 2. Chờ quả rơi chạm pet (920ms) mới cập nhật UI và chạy hiệu ứng tiếp theo
      setTimeout(async () => {
        setIsEating(true);
        setEatBurst(true);

        setTimeout(() => setEatBurst(false), 560);
        setTimeout(() => setIsEating(false), 1200);

        if (response.isEvolved) {
          const updatedPet = await fetchPetDetailById(response.petId);
          setPet(updatedPet);
          setCurrentImage(updatedPet.imageUrl);
          setEvolveAnimation(true);
          const evolveSound = new Audio('https://res.cloudinary.com/dqpkxxzaf/video/upload/v1757584431/pokemon-evolve_vzpzqg.mp3');
          evolveSound.play().catch(() => console.warn('Autoplay âm thanh bị chặn'));
      setTimeout(() => {
            setEvolveAnimation(false);
          }, 3000);
        } else {
          setPet((prevPet) => {
            if (!prevPet) return null;
            return {
              ...prevPet,
              level: response.level,
              experience: response.experience,
            };
          });
        }

        if (response.isLevelUp) {
          setLevelUpAnimation(true);
          const levelUpSound = new Audio('https://res.cloudinary.com/dqpkxxzaf/video/upload/v1757584438/12_3_gdjgqm.mp3');
          levelUpSound.play().catch(() => console.warn('Autoplay âm thanh bị chặn'));
          setTimeout(() => setLevelUpAnimation(false), 1000);
        }

        setUser({ ...user, totalAP: response.ap } as UserDto);
        setIsUpgrading(false);
        toast.success('Nâng cấp thú cưng thành công.');
      }, 920);

    } catch (err: unknown) {
      setError(extractApiError(err).message);
      setIsUpgrading(false);
    }
  };

  const handleActive = async () => {
    if (!pet) return;
    setIsActive(true);
    try {
      await activePet(pet.id);
      setUser({ ...user, petActiveId: pet.id } as UserDto);
      toast.success('Đã chọn thú cưng đồng hành.');
    } catch (err: unknown) {
      setError(extractApiError(err).message);
    } finally {
      setIsActive(false);
    }
  };

  const isOwned = pet && pet.acquiredAt !== null;

  if (isLoading) {
    return (
      <div className="pixel-background text-white min-h-screen w-full flex justify-center items-center py-6">
        <div className="font-pixel">Đang tải...</div>
      </div>
    );
  }

  if (error || !pet) {
    return (
      <div className="pixel-background text-white min-h-screen w-full flex justify-center items-center py-6">
        <div className="text-red-500 font-pixel">{error || 'Không tìm thấy thú cưng'}</div>
      </div>
    );
  }

  const isCurrentActive = user?.petActiveId === pet.id;

  return (
    <div className="review-box-background text-white h-[calc(100vh-56px)] mt-[56px] w-full flex justify-center items-center py-4 px-4 sm:px-6 lg:px-8 overflow-hidden">
      <div className="w-full max-w-5xl h-full flex flex-col md:flex-row items-stretch gap-4 sm:gap-6 bg-slate-950/70 backdrop-blur-md border-4 border-slate-800 rounded-2xl p-4 sm:p-6 shadow-2xl relative overflow-hidden">

        {/* Bên trái: Hình ảnh pet hoặc dấu chấm hỏi */}
        <div className="w-full md:w-5/12 flex flex-col gap-4 max-h-full overflow-hidden">
          <div className="relative flex-1 min-h-[200px] max-h-[300px] lg:max-h-[360px]">
            {/* Rarity Border Glow effect */}
            <div className={`absolute -inset-1 rounded-2xl bg-gradient-to-r from-yellow-500 to-amber-600 opacity-20 blur-sm`} />

            <div className={`relative ${backgroundClass} bg-no-repeat bg-cover bg-center border-4 border-slate-800 h-full rounded-2xl overflow-hidden p-4 flex items-center justify-center shadow-lg`}>
              {isOwned ? (
                <>
                  {/* Falling berries zone */}
                  <div className="absolute inset-0 overflow-hidden pointer-events-none z-10">
                    <AnimatePresence>
                      {berries.map((b) => (
                        <FallingBerry key={b.id} particle={b} onDone={removeBerry} />
                      ))}
                    </AnimatePresence>
                  </div>

                  <EatBurst active={eatBurst} />

                  {evolveAnimation && (
                    <motion.div
                      className="absolute top-0 left-0 right-0 bg-black/85 text-white text-center py-2 z-30 font-pixel text-xs tracking-wider"
                      initial={{ y: -50, opacity: 0 }}
                      animate={{ y: 0, opacity: 1 }}
                      exit={{ y: -50, opacity: 0 }}
                      transition={{ duration: 0.5 }}
                    >
                      Dạng sống của {pet.name} đang tiến hóa!
                    </motion.div>
                  )}
                  <motion.div
                    className="w-full h-full flex items-center justify-center"
                    animate={
                      levelUpAnimation
                        ? { scale: [1, 1.15, 1], rotate: [0, -5, 5, 0] }
                        : evolveAnimation
                          ? {
                            opacity: [1, 0.3, 0.3, 1],
                            scale: [1, 1.25, 1.25, 1],
                            rotate: [0, 180, 360, 0],
                          }
                          : isEating
                            ? { scaleX: [1, 1.13, 0.91, 1.05, 1], scaleY: [1, 0.87, 1.11, 0.96, 1], y: [0, -10, 8, -2, 0] }
                            : { y: [0, -6, 0] } // idle floating animation
                    }
                    transition={
                      evolveAnimation
                        ? { duration: 3, times: [0, 0.4, 0.8, 1], ease: 'easeInOut' }
                        : isEating
                          ? { duration: 0.42 }
                          : { duration: 2.4, repeat: Infinity, ease: 'easeInOut' }
                    }
                  >
                    <img
                      src={currentImage}
                      alt={pet.name}
                      className="w-full h-full object-contain transform transition-transform duration-300 rounded-lg pixel-art"
                    />
                  </motion.div>
                  {evolveAnimation && (
                    <>
                      <motion.div
                        className="absolute inset-0 bg-white"
                        initial={{ opacity: 0 }}
                        animate={{ opacity: [0, 0.8, 0] }}
                        transition={{ duration: 3, times: [0, 0.4, 1], ease: 'easeInOut' }}
                      />
                      <div className="pointer-events-none absolute inset-0 hidden overflow-hidden md:block">
                        {EVOLUTION_SPARKLES.map((sparkle) => (
                          <motion.span
                            key={sparkle.id}
                            className="absolute rounded-full bg-yellow-200 shadow-[0_0_12px_rgba(253,224,71,0.9)]"
                            style={{
                              left: `${sparkle.x}%`,
                              top: `${sparkle.y}%`,
                              width: sparkle.size,
                              height: sparkle.size,
                            }}
                            animate={{ opacity: [0, 0.9, 0], scale: [0.4, 1.8, 0.4], y: [-8, 8] }}
                            transition={{ duration: 1.2, delay: sparkle.delay, repeat: Infinity, ease: 'easeInOut' }}
                          />
                        ))}
                      </div>                    </>
                  )}
                </>
              ) : (
                <img
                  src={pet.imageUrl}
                  alt={pet.name}
                  className="w-full h-full object-contain rounded-lg pixel-art"
                  style={{ filter: 'brightness(0)' }}
                />
              )}
            </div>
          </div>
          {isOwned && (
            <div className="bg-slate-900/80 border-2 border-slate-800 rounded-2xl p-4 flex flex-col gap-2 shadow-md flex-shrink-0">
              <div className="flex justify-between items-center text-xs font-pixel">
                <span className="text-slate-400 font-bold">Cấp độ</span>
                <span className="text-lg font-bold text-yellow-400">{pet.level ?? 'N/A'}</span>
              </div>
              <div className="space-y-1">
                <div className="w-full bg-slate-950 rounded-full h-3 border border-slate-800 relative overflow-hidden p-0.5">
                  <motion.div
                    className="bg-gradient-to-r from-blue-500 to-indigo-500 h-full rounded-full shadow-[0_0_8px_rgba(59,130,246,0.5)]"
                    initial={{ width: 0 }}
                    animate={{ width: pet.experience != null ? `${(pet.experience / 100) * 100}%` : '0%' }}
                    transition={{ duration: 0.5 }}
                  />
                </div>
                <div className="flex justify-between text-[10px] text-slate-500 font-pixel">
                  <span>Kinh nghiệm</span>
                  <span>{pet.experience != null ? pet.experience : 0} / 100</span>
                </div>
              </div>
            </div>
          )}
        </div>

        {/* Bên phải: Thông tin và các nút hành động */}
        <div className="w-full md:w-7/12 flex flex-col gap-4 justify-between max-h-full overflow-hidden">
          <div className="flex flex-col gap-4 overflow-y-auto pr-1 flex-1">
            <div className="bg-slate-900/50 border-2 border-slate-800 rounded-2xl p-5 shadow-md backdrop-blur-sm relative flex-shrink-0">
              <div className="flex justify-between items-start mb-4 flex-wrap gap-2">
                <h2 className="text-3xl font-pixel font-bold text-transparent bg-clip-text bg-gradient-to-r from-yellow-300 via-amber-400 to-yellow-500 drop-shadow-md">
                  {pet.name}
                </h2>
                <div className="flex gap-2">
                  <span className="px-3 py-1 rounded-full text-[10px] font-pixel font-bold bg-amber-950/80 border border-amber-500/50 text-amber-400">
                    {pet.rarity}
                  </span>
                  <span className="px-3 py-1 rounded-full text-[10px] font-pixel font-bold bg-blue-950/80 border border-blue-500/50 text-blue-400">
                    {pet.type}
                  </span>
                </div>
              </div>
              <p className="text-slate-300 font-pixel text-xs leading-relaxed mb-4 border-b border-slate-800/80 pb-3">
                {pet.description}
              </p>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 font-pixel text-[10px]">
                {isOwned && (
                  <>
                    <div className="flex items-center gap-2 bg-slate-950/40 p-2.5 rounded-xl border border-slate-900">
                      <span className="text-slate-500 font-bold">Yêu thích:</span>
                      <span className="text-yellow-400">{pet.isFavorite ? 'Có' : 'Không'}</span>
                    </div>
                    <div className="flex items-center gap-2 bg-slate-950/40 p-2.5 rounded-xl border border-slate-900">
                      <span className="text-slate-500 font-bold">Sở hữu ngày:</span>
                      <span className="text-emerald-400">
                        {pet.acquiredAt ? new Date(pet.acquiredAt).toLocaleDateString() : 'N/A'}
                      </span>
                    </div>
                  </>
                )}
                {pet.requiredLevel && (
                  <div className="flex items-center gap-2 bg-slate-950/40 p-2.5 rounded-xl border border-slate-900">
                    <span className="text-slate-500 font-bold">Cấp độ yêu cầu:</span>
                    <span className="text-yellow-400">Cấp {pet.requiredLevel}</span>
                  </div>
                )}
                {pet.nextEvolutionId && (
                  <div className="flex items-center gap-2 bg-slate-950/40 p-2.5 rounded-xl border border-slate-900">
                    <span className="text-slate-500 font-bold">Dạng tiến hóa:</span>
                    <span className="text-indigo-400">Có dạng tiếp theo</span>
                  </div>
                )}
              </div>
            </div>

            <div className="w-full flex-shrink-0">
              <ProfileCard />
            </div>
          </div>

          {/* Các nút hành động */}
          <div className="flex flex-col sm:flex-row gap-4 mt-2 flex-shrink-0">
            {isOwned ? (
              <>
                <motion.button
                  onClick={handleUpgrade}
                  disabled={isUpgrading}
                  className="flex-1 flex items-center justify-center gap-2 px-6 py-3 bg-gradient-to-r from-amber-500 to-yellow-500 text-slate-950 rounded-xl hover:from-amber-400 hover:to-yellow-400 hover:shadow-lg hover:shadow-yellow-500/20 disabled:opacity-50 transition-all font-pixel font-bold text-xs sm:text-sm border-b-4 border-amber-700 custom-cursor"
                  whileHover={{ scale: 1.03 }}
                  whileTap={{ scale: 0.97 }}
                >
                  {isUpgrading ? (
                    <>
                      <svg
                        className="animate-spin h-5 w-5 mr-2 text-slate-950"
                        xmlns="http://www.w3.org/2000/svg"
                        fill="none"
                        viewBox="0 0 24 24"
                      >
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                        <path
                          className="opacity-75"
                          fill="currentColor"
                          d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                        />
                      </svg>
                      <span>Đang cho ăn...</span>
                    </>
                  ) : (
                    <span>Cho ăn</span>
                  )}
                </motion.button>

                <motion.button
                  onClick={handleActive}
                  disabled={isActive || isCurrentActive}
                  className={`flex-1 flex items-center justify-center gap-2 px-6 py-3 rounded-xl font-pixel font-bold text-xs sm:text-sm transition-all custom-cursor ${isCurrentActive
                      ? 'bg-emerald-950/60 border-2 border-emerald-500/50 text-emerald-400 cursor-not-allowed shadow-[0_0_15px_rgba(16,185,129,0.15)]'
                      : 'bg-slate-800 border-2 border-slate-700 hover:bg-slate-700 text-white'
                    }`}
                  whileHover={isCurrentActive ? {} : { scale: 1.03 }}
                  whileTap={isCurrentActive ? {} : { scale: 0.97 }}
                >
                  {isActive ? (
                    <>
                      <svg
                        className="animate-spin h-5 w-5 mr-2 text-white"
                        xmlns="http://www.w3.org/2000/svg"
                        fill="none"
                        viewBox="0 0 24 24"
                      >
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                        <path
                          className="opacity-75"
                          fill="currentColor"
                          d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                        />
                      </svg>
                      <span>Đang mang...</span>
                    </>
                  ) : isCurrentActive ? (
                    <span>Đang đồng hành</span>
                  ) : (
                    <span>Mang theo</span>
                  )}
                </motion.button>
              </>
            ) : (
              <p className="w-full text-red-500 text-center font-pixel font-semibold text-xs sm:text-sm py-3 bg-red-950/20 border border-red-900/40 rounded-xl">
                Bạn chưa sở hữu thú cưng này!
              </p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default PetDetailPage;
