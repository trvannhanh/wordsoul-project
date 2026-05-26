import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { useAuth } from '../../hooks/Auth/useAuth';
import { fetchPetDetailById } from '../../services/pet';
import { typeBackgrounds, type PetDetailDto } from '../../types/PetDto';
import type { UserProgressDto } from '../../types/UserDto';

interface ReviewBoxProps {
  progress: UserProgressDto | null;
  loading: boolean;
  onCreateReviewSession: () => void;
}

const ReviewBox: React.FC<ReviewBoxProps> = ({ progress, loading, onCreateReviewSession }) => {
  const { user } = useAuth();
  const [pet, setPet] = useState<PetDetailDto | null>(null);
  const [petLoading, setPetLoading] = useState<boolean>(false);
  const [imgSrc, setImgSrc] = useState<string>('');

  useEffect(() => {
    const loadActivePet = async () => {
      if (user?.petActiveId) {
        setPetLoading(true);
        try {
          const petData = await fetchPetDetailById(user.petActiveId);
          setPet(petData);
          setImgSrc(
            `https://img.pokemondb.net/sprites/black-white/anim/normal/${petData.name.toLowerCase()}.gif`
          );
        } catch (error) {
          console.error('Failed to load active pet for ReviewBox:', error);
        } finally {
          setPetLoading(false);
        }
      } else {
        setPet(null);
      }
    };
    loadActivePet();
  }, [user?.petActiveId]);

  return (
    <motion.div
      className="review-box-background pixel-border rounded-xl p-6 text-center"
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5 }}
    >
      {petLoading ? (
        <div className="flex justify-center items-center h-20 mb-4 animate-pulse">
          <span className="font-pixel text-[10px] text-yellow-300">Đang tìm đồng hành...</span>
        </div>
      ) : pet ? (
        <div className="flex flex-col items-center mb-4">
          {/* Unified active pet companion container */}
          <div className="flex gap-4 items-center bg-black/50 p-3 rounded-lg border border-gray-700 w-full max-w-md text-left">
            <div
              className={`w-16 h-16 rounded border-2 border-double border-white/35 flex items-center justify-center overflow-hidden bg-cover bg-center ${typeBackgrounds[pet.type] || 'bg-gray-900'
                }`}
            >
              <img
                src={imgSrc}
                alt={pet.name}
                className="w-12 h-12 object-contain pixel-art transform hover:scale-115 transition-transform"
                style={{ imageRendering: 'pixelated' }}
                onError={() => {
                  if (imgSrc !== pet.imageUrl) setImgSrc(pet.imageUrl);
                }}
              />
            </div>
            <div className="flex-1 min-w-0">
              <div className="flex justify-between items-baseline">
                <span className="font-press text-[10px] sm:text-xs text-yellow-300 truncate">
                  {pet.name.toUpperCase()}
                </span>
                <span className="font-pixel text-[10px] text-yellow-500">Lv.{pet.level ?? 1}</span>
              </div>
              {/* Pet XP Bar */}
              <div className="w-full bg-gray-900 border border-gray-800 h-2.5 p-[1px] rounded-sm mt-1 overflow-hidden">
                <div
                  className="bg-yellow-400 h-full rounded-sm"
                  style={{ width: `${Math.min(100, Math.max(0, pet.experience ?? 0))}%` }}
                />
              </div>
              <div className="flex justify-between text-[8px] text-gray-500 font-pixel mt-0.5">
                <span>Kinh nghiệm</span>
                <span>{pet.experience ?? 0} / 100</span>
              </div>
            </div>
          </div>
        </div>
      ) : (
        <div className="flex flex-col items-center mb-4">
          <div className="flex justify-center mb-2">
            <img
              src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756453095/boy_c1k3lt.gif"
              alt="minh hoa"
              className="w-14 h-14 object-cover pixelated"
            />
          </div>
          <p className="font-pixel text-[10px] text-gray-400 mb-2">
            Bạn chưa dắt thú cưng đồng hành theo.
          </p>
          <Link to="/pets" className="mb-2">
            <button className="bg-yellow-600 hover:bg-yellow-500 text-black px-3 py-1.5 rounded font-pixel text-[10px] border border-black animate-pulse transition-transform hover:scale-105 active:translate-y-0.5">
              Chọn thú cưng đồng hành
            </button>
          </Link>
        </div>
      )}

      {progress && progress.reviewWordCount > 0 ? (
        <>
          <p className="font-pixel text-xs sm:text-sm text-yellow-300 mb-4 leading-relaxed">
            {pet ? (
              <span>
                Bạn có {progress.reviewWordCount} từ cần ôn tập để cho <b>{pet.name}</b> ăn!
              </span>
            ) : (
              <span>Bạn có {progress.reviewWordCount} từ cần ôn tập!</span>
            )}
            {progress.nextReviewTime &&
              ` (tiếp theo sau ${new Date(progress.nextReviewTime).toLocaleTimeString()})`}
          </p>
          <div className="flex justify-center">
            <motion.button
              className="relative flex items-center justify-center w-40 px-4 py-2 bg-yellow-400 text-black font-pixel text-sm rounded pixel-border-dark hover:bg-yellow-300 custom-cursor"
              onClick={onCreateReviewSession}
              disabled={loading}
              whileHover={{ scale: 1.05, boxShadow: '0 0 10px rgba(255, 204, 0, 0.7)' }}
              whileTap={{ scale: 0.95 }}
            >
              <span className="absolute left-0 w-1 h-full bg-yellow-600" />
              <span>{pet ? 'Cho ăn (Ôn tập)' : 'Ôn tập'}</span>
              <span className="absolute right-0 w-1 h-full bg-yellow-600" />
            </motion.button>
          </div>
        </>
      ) : (
        <>
          <p className="text-xs sm:text-sm text-gray-200 mb-4 leading-relaxed">
            {pet ? (
              <span>
                <b>{pet.name}</b> đã no rồi! Hãy học thêm bộ từ vựng mới để cùng lớn lên nào! 🌟
              </span>
            ) : (
              <span>Hành trình của bạn chỉ mới bắt đầu, cùng khám phá nào!!</span>
            )}
          </p>
          <div className="flex justify-center">
            <Link to="/vocabularySet" className="no-underline">
              <motion.button
                className="relative flex items-center justify-center w-36 px-4 py-2 bg-yellow-400 text-black font-pixel text-sm rounded pixel-border-dark hover:bg-yellow-300 custom-cursor"
                whileHover={{ scale: 1.05, boxShadow: '0 0 10px rgba(255, 204, 0, 0.7)' }}
                whileTap={{ scale: 0.95 }}
              >
                <span className="absolute left-0 w-1 h-full bg-yellow-600" />
                <span>Học từ mới</span>
                <span className="absolute right-0 w-1 h-full bg-yellow-600" />
              </motion.button>
            </Link>
          </div>
        </>
      )}
    </motion.div>
  );
};

export default ReviewBox;