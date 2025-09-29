/* eslint-disable @typescript-eslint/no-explicit-any */
import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { activePet, fetchPetDetailById, upgradePet } from '../../services/pet';
import { motion } from 'framer-motion';
import Particles from 'react-particles';
import { loadFull } from 'tsparticles';
import ProfileCard from '../../components/UserProfile/ProfileCard';
import { useAuth } from '../../store/AuthContext';
import type { PetDetailDto, UpgradePetResponseDto } from '../../types/PetDto';
import type { UserDto } from '../../types/UserDto';

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

  // Khởi tạo particles
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const particlesInit = async (main: any) => {
    await loadFull(main);
  };

  useEffect(() => {
    const loadPet = async () => {
      setIsLoading(true);
      try {
        const data = await fetchPetDetailById(Number(id));
        setPet(data);
        setCurrentImage(data.imageUrl);
      } catch (err: any) {
        setError(`Không thể tải thông tin thú cưng: ${err.message}`);
      } finally {
        setIsLoading(false);
      }
    };
    if (id) {
      loadPet();
    }
  }, [id]);

  const handleUpgrade = async () => {
    if (!pet) return;
    setIsUpgrading(true);
    try {
      const response: UpgradePetResponseDto = await upgradePet(pet.id);
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
    } catch (err: any) {
      console.error('Lỗi khi nâng cấp thú cưng:', err);
      setError(err.response?.data?.message || 'Không thể nâng cấp thú cưng');
    } finally {
      setIsUpgrading(false);
    }
  };

  const handleActive = async () => {
    if (!pet) return;
    setIsActive(true);
    try {
      const response: PetDetailDto = await activePet(pet.id);
      setUser({ ...user, avatarUrl: response.imageUrl } as UserDto);
    } catch (err: any) {
      console.error('Lỗi khi kích hoạt thú cưng:', err);
      setError(err.response?.data?.message || 'Không thể kích hoạt thú cưng');
    } finally {
      setIsActive(false);
    }
  };

  const isOwned = pet && pet.acquiredAt !== null;

  if (isLoading) {
    return (
      <div className="pixel-background text-white min-h-screen w-full flex justify-center items-center py-6">
        <div>Đang tải...</div>
      </div>
    );
  }

  if (error || !pet) {
    return (
      <div className="pixel-background text-white min-h-screen w-full flex justify-center items-center py-6">
        <div className="text-red-500">{error || 'Không tìm thấy thú cưng'}</div>
      </div>
    );
  }

  return (
    <div className="pixel-background text-white min-h-screen w-full flex justify-center items-center py-6 px-4 sm:px-6 lg:px-8">
      <div className="w-full max-w-5xl flex flex-col md:flex-row items-start gap-4 sm:gap-6 bg-gradient-to-br from-blue-200 to-blue-400 border-4 border-black rounded-lg p-4 sm:p-6 shadow-lg">
        {/* Bên trái: Hình ảnh pet hoặc dấu chấm hỏi */}
        <div className="w-full md:w-1/2">
          <div className="relative h-64 sm:h-80 lg:h-96 rounded-lg overflow-hidden p-4 pet-background">
            {isOwned ? (
              <>
                {evolveAnimation && (
                  <motion.div
                    className="absolute top-0 left-0 right-0 bg-black bg-opacity-75 text-white text-center py-2 z-10"
                    initial={{ y: -50, opacity: 0 }}
                    animate={{ y: 0, opacity: 1 }}
                    exit={{ y: -50, opacity: 0 }}
                    transition={{ duration: 0.5 }}
                  >
                    What? {pet.name} is evolving!
                  </motion.div>
                )}
                <motion.div
                  animate={
                    levelUpAnimation
                      ? { scale: [1, 1.1, 1], opacity: [1, 0.8, 1] }
                      : evolveAnimation
                        ? {
                            opacity: [1, 0.3, 0.3, 1],
                            scale: [1, 1.2, 1.2, 1],
                            rotate: [0, 180, 360, 0],
                          }
                        : { scale: 1, opacity: 1, rotate: 0 }
                  }
                  transition={
                    evolveAnimation
                      ? { duration: 3, times: [0, 0.4, 0.8, 1], ease: 'easeInOut' }
                      : { duration: 1 }
                  }
                >
                  <img
                    src={currentImage}
                    alt={pet.name}
                    className="w-full h-full max-h-80 sm:max-h-96 object-contain transform hover:scale-105 transition-transform duration-300 rounded-lg"
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
                    <Particles
                      id="tsparticles"
                      init={particlesInit}
                      options={{
                        particles: {
                          number: { value: window.innerWidth < 768 ? 20 : 50 },
                          size: { value: { min: 1, max: 5 } },
                          move: { enable: true, speed: window.innerWidth < 768 ? 3 : 6, direction: 'none', random: true },
                          opacity: { value: { min: 0.3, max: 0.7 } },
                        },
                        interactivity: { events: { onHover: { enable: false } } },
                      }}
                      className="absolute inset-0 md:block hidden"
                    />
                  </>
                )}
              </>
            ) : (
              <img
                src={pet.imageUrl}
                alt={pet.name}
                className="w-full h-full max-h-80 sm:max-h-96 object-contain transform hover:scale-105 transition-transform duration-300 rounded-lg"
                style={{ filter: 'brightness(0)' }}
              />
            )}
          </div>
          {isOwned && (
            <>
              <div className="mt-4 text-center">
                <span className="font-bold text-base sm:text-lg">Cấp độ: </span>
                <span className="text-lg sm:text-xl font-semibold">{pet.level ?? 'N/A'}</span>
              </div>
              <div className="mt-2 px-4">
                <div className="w-full bg-gray-300 rounded-full h-3 relative overflow-hidden">
                  <motion.div
                    className="bg-blue-500 h-3 rounded-full"
                    initial={{ width: 0 }}
                    animate={{ width: pet.experience != null ? `${(pet.experience / 100) * 100}%` : '0%' }}
                    transition={{ duration: 0.5 }}
                  />
                </div>
                <p className="text-center mt-1 text-sm sm:text-base">
                  Kinh nghiệm: {pet.experience != null ? pet.experience : 0}/100
                </p>
              </div>
            </>
          )}
        </div>
        {/* Bên phải: Thông tin và nút thăng cấp */}
        <div className="w-full md:w-1/2 flex flex-col gap-4 sm:gap-6">
          <div className="border-2 border-white rounded-lg p-4 sm:p-5 bg-gray-800 bg-opacity-80">
            <h2 className="text-xl sm:text-2xl lg:text-3xl font-bold mb-3 text-yellow-300">{pet.name}</h2>
            <p className="text-gray-200 mb-3 text-sm sm:text-base">{pet.description}</p>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 text-sm sm:text-base">
              <div>
                <span className="font-bold">Độ hiếm: </span>
                <span className="text-yellow-200">{pet.rarity}</span>
              </div>
              <div>
                <span className="font-bold">Loại: </span>
                <span className="text-yellow-200">{pet.type}</span>
              </div>
              {isOwned && (
                <>
                  <div>
                    <span className="font-bold">Yêu thích: </span>
                    <span className="text-yellow-200">{pet.isFavorite ? 'Có' : 'Không'}</span>
                  </div>
                  <div>
                    <span className="font-bold">Sở hữu: </span>
                    <span className="text-yellow-200">
                      {pet.acquiredAt ? new Date(pet.acquiredAt).toLocaleDateString() : 'N/A'}
                    </span>
                  </div>
                </>
              )}
              {pet.baseFormId && (
                <div>
                  <span className="font-bold">ID dạng gốc: </span>
                  <span className="text-yellow-200">{pet.baseFormId}</span>
                </div>
              )}
              {pet.nextEvolutionId && (
                <div>
                  <span className="font-bold">ID tiến hóa tiếp theo: </span>
                  <span className="text-yellow-200">{pet.nextEvolutionId}</span>
                </div>
              )}
              {pet.requiredLevel && (
                <div>
                  <span className="font-bold">Cấp độ yêu cầu: </span>
                  <span className="text-yellow-200">{pet.requiredLevel}</span>
                </div>
              )}
            </div>
          </div>
          <div className="w-full">
            <ProfileCard />
          </div>
          {isOwned ? (
            <>
              <motion.button
                onClick={handleUpgrade}
                disabled={isUpgrading}
                className="relative flex items-center justify-center w-full px-4 sm:px-6 py-2 sm:py-3 bg-yellow-300 text-black rounded-lg hover:bg-yellow-400 hover:shadow-lg hover:shadow-yellow-300/50 transition-all duration-300 text-sm sm:text-base font-bold"
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
              >
                <span className="absolute left-0 w-0.5 h-full bg-yellow-500" />
                <span>Thăng cấp</span>
                <span className="absolute right-0 w-0.5 h-full bg-yellow-500" />
                {isUpgrading && (
                  <svg
                    className="animate-spin h-5 w-5 ml-2 text-black"
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
                )}
              </motion.button>
              <motion.button
                onClick={handleActive}
                disabled={isActive}
                className="relative flex items-center justify-center w-full px-4 sm:px-6 py-2 sm:py-3 bg-yellow-300 text-black rounded-lg hover:bg-yellow-400 hover:shadow-lg hover:shadow-yellow-300/50 transition-all duration-300 text-sm sm:text-base font-bold"
                whileHover={{ scale: 1.05 }}
                whileTap={{ scale: 0.95 }}
              >
                <span className="absolute left-0 w-0.5 h-full bg-yellow-500" />
                <span>Mang theo</span>
                <span className="absolute right-0 w-0.5 h-full bg-yellow-500" />
                {isActive && (
                  <svg
                    className="animate-spin h-5 w-5 ml-2 text-black"
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
                )}
              </motion.button>
            </>
          ) : (
            <p className="text-red-500 text-center font-semibold text-sm sm:text-base">
              Bạn chưa sở hữu thú cưng này!
            </p>
          )}
        </div>
      </div>
    </div>
  );
};

export default PetDetailPage;