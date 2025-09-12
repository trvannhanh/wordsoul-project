import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { fetchPetDetailById, upgradePet } from '../../services/pet';
import type { PetDetail, UpgradePetResponse } from '../../types/Dto';
import { motion } from 'framer-motion';
import Particles from 'react-particles';
import { loadFull } from 'tsparticles';

const PetDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const navigate = useNavigate();
  const [pet, setPet] = useState<PetDetail | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isUpgrading, setIsUpgrading] = useState<boolean>(false);
  const [levelUpAnimation, setLevelUpAnimation] = useState<boolean>(false);
  const [evolveAnimation, setEvolveAnimation] = useState<boolean>(false);
  const [currentImage, setCurrentImage] = useState<string | undefined>(undefined);

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
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
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
      const response: UpgradePetResponse = await upgradePet(pet.id);
      if (response.isEvolved) {
        const updatedPet = await fetchPetDetailById(response.petId);
        setPet(updatedPet);
        setCurrentImage(updatedPet.imageUrl); // Cập nhật ảnh ngay lập tức
        setEvolveAnimation(true);
        const evolveSound = new Audio('https://res.cloudinary.com/dqpkxxzaf/video/upload/v1757584431/pokemon-evolve_vzpzqg.mp3');
        evolveSound.play();
        setTimeout(() => {
          setEvolveAnimation(false); // Kết thúc hiệu ứng sau 3 giây
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
        levelUpSound.play();
        setTimeout(() => setLevelUpAnimation(false), 1000);
      }
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (err: any) {
      console.error('Lỗi khi nâng cấp thú cưng:', err);
      setError(err.response?.data?.message || 'Không thể nâng cấp thú cưng');
    } finally {
      setIsUpgrading(false);
    }
  };

  const isOwned = pet && pet.acquiredAt !== null;

  if (isLoading) {
    return (
      <div className="pixel-background text-white h-screen w-full flex justify-center items-center py-10">
        <div>Đang tải...</div>
      </div>
    );
  }

  if (error || !pet) {
    return (
      <div className="pixel-background text-white h-screen w-full flex justify-center items-center py-10">
        <div className="text-red-500">{error || 'Không tìm thấy thú cưng'}</div>
      </div>
    );
  }

  return (
    <div className="pixel-background text-white h-screen w-full flex justify-center items-center py-10">
      <div className="w-7/12 h-10/12 flex items-start gap-5 bg-gradient-to-br from-blue-200 to-blue-400 border-4 border-black rounded-lg p-5 shadow-lg">
        {/* Bên trái: Hình ảnh pet hoặc dấu chấm hỏi */}
        <div className="w-1/2 h-full">
          <div className="relative h-2/3 rounded-lg overflow-hidden p-5 pet-background">
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
                          opacity: [1, 0.3, 0.3, 1], // Mờ dần rồi rõ lại
                          scale: [1, 1.2, 1.2, 1], // Phóng to rồi trở lại
                          rotate: [0, 180, 360, 0], // Xoay 1 vòng, kết thúc ở 0°
                        }
                      : { scale: 1, opacity: 1, rotate: 0 } // Trạng thái bình thường
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
                    className="w-full h-full object-cover transform hover:scale-105 transition-transform duration-300 rounded-lg"
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
                          number: { value: 50 },
                          size: { value: { min: 1, max: 5 } },
                          move: { enable: true, speed: 6, direction: 'none', random: true },
                          opacity: { value: { min: 0.3, max: 0.7 } },
                        },
                        interactivity: { events: { onHover: { enable: false } } },
                      }}
                      className="absolute inset-0"
                    />
                  </>
                )}
              </>
            ) : (
              <div className="w-full h-full flex items-center justify-center bg-gray-700 text-6xl text-gray-400 rounded-lg">
                ?
              </div>
            )}
          </div>
          {isOwned && (
            <>
              <div className="mt-4 text-center">
                <span className="font-bold text-lg">Cấp độ: </span>
                <span className="text-xl font-semibold">{pet.level ?? 'N/A'}</span>
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
                <p className="text-center mt-1 text-sm">
                  Kinh nghiệm: {pet.experience != null ? pet.experience : 0}/100
                </p>
              </div>
            </>
          )}
        </div>
        {/* Bên phải: Thông tin và nút thăng cấp */}
        <div className="w-1/2 flex flex-col gap-4">
          <div className="border-2 border-white rounded-lg p-5 bg-gray-800 bg-opacity-80">
            <h2 className="text-3xl font-bold mb-3 text-yellow-300">{pet.name}</h2>
            <p className="text-gray-200 mb-3 text-sm">{pet.description}</p>
            <div className="grid grid-cols-2 gap-3 text-sm">
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
          {isOwned ? (
            <motion.button
              onClick={handleUpgrade}
              disabled={isUpgrading}
              className="relative flex items-center justify-center w-full px-4 py-2 bg-yellow-300 text-black rounded-lg hover:bg-yellow-400 hover:shadow-lg hover:shadow-yellow-300/50 transition-all duration-300"
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
            >
              <span className="absolute left-0 w-0.5 h-full bg-yellow-500" />
              <span className="font-bold">Thăng cấp</span>
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
          ) : (
            <p className="text-red-500 text-center font-semibold">Bạn chưa sở hữu thú cưng này!</p>
          )}
        </div>
      </div>
    </div>
  );
};

export default PetDetailPage;