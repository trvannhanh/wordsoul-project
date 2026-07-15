import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { useAuth } from '../../../hooks/Auth/useAuth';
import { fetchPetDetailById } from '../../../services/pet';
import { typeBackgrounds, rarityBorders, type PetDetailDto } from '../../../types/PetDto';

const ActivePetWidget: React.FC = () => {
  const { user } = useAuth();
  const [pet, setPet] = useState<PetDetailDto | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [imgSrc, setImgSrc] = useState<string>('');

  useEffect(() => {
    const loadActivePet = async () => {
      if (user?.petActiveId) {
        setLoading(true);
        try {
          const petData = await fetchPetDetailById(user.petActiveId);
          setPet(petData);
          setImgSrc(
            `https://img.pokemondb.net/sprites/black-white/anim/normal/${petData.name.toLowerCase()}.gif`
          );
        } catch (error) {
          console.error('Failed to load active pet:', error);
        } finally {
          setLoading(false);
        }
      } else {
        setPet(null);
      }
    };

    loadActivePet();
  }, [user?.petActiveId]);

  if (loading) {
    return (
      <div className="bg-black/60 p-5 rounded-xl border-2 border-yellow-400 min-h-[160px] animate-pulse flex flex-col justify-center items-center">
        <span className="font-pixel text-xs text-yellow-300">Đang tải đồng hành...</span>
      </div>
    );
  }

  // Case: No active pet chosen
  if (!pet) {
    return (
      <motion.div
        className="bg-black/80 rounded-xl border-2 border-dashed border-gray-600 p-6 flex flex-col items-center justify-center text-center relative overflow-hidden group hover:border-yellow-400 transition-colors"
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
      >
        <div className="w-16 h-16 mb-3 relative flex items-center justify-center opacity-40 group-hover:opacity-100 transition-opacity">
          <img
            src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759222012/egg-logo_pflvdz.png"
            alt="No companion"
            className="w-12 h-12 object-contain pixelated grayscale"
          />
        </div>
        <h3 className="font-pixel text-yellow-500 text-sm mb-1">CHƯA CÓ BẠN ĐỒNG HÀNH</h3>
        <p className="font-pixel text-[10px] text-gray-400 mb-4 leading-relaxed">
          Kích hoạt một Vocamon làm bạn đồng hành để cùng nhận XP khi học tập!
        </p>
        <Link to="/pets">
          <motion.button
            className="bg-yellow-500 hover:bg-yellow-400 text-black px-4 py-2 rounded font-pixel text-xs border-2 border-black active:translate-y-0.5 transition-all"
            whileHover={{ scale: 1.05 }}
            whileTap={{ scale: 0.95 }}
          >
            CHỌN VOCAMON ➝
          </motion.button>
        </Link>
      </motion.div>
    );
  }

  const backgroundClass = typeBackgrounds[pet.type] || 'bg-gray-900';
  const borderClass = rarityBorders[pet.rarity] || 'border-gray-500';
  const petLevel = pet.level ?? 1;
  const petExperience = pet.experience ?? 0;
  const xpPercentage = Math.min(100, Math.max(0, petExperience));

  return (
    <motion.div
      className={`bg-black/80 rounded-xl border-2 ${borderClass} p-5 relative overflow-hidden group hover:shadow-[0_0_15px_rgba(250,204,21,0.3)] transition-all`}
      initial={{ opacity: 0, y: 15 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.4 }}
    >
      <div className="absolute -inset-1 bg-gradient-to-r from-yellow-400/10 via-transparent to-yellow-400/10 opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none" />

      {/* Header section with Pet name, rarity and link */}
      <div className="flex justify-between items-start mb-3 relative z-10">
        <div>
          <h2 className="font-press text-yellow-400 text-xs sm:text-sm drop-shadow-md tracking-wider">
            {pet.name.toUpperCase()}
          </h2>
          <div className="flex gap-2 mt-1">
            <span className="font-pixel text-[9px] text-gray-400 uppercase tracking-widest">
              {pet.rarity}
            </span>
            <span className="font-pixel text-[9px] text-yellow-500">
              ⚡ {pet.type}
            </span>
          </div>
        </div>
        <Link to={`/pets/${pet.id}`}>
          <button className="bg-yellow-500 hover:bg-yellow-400 text-black px-2.5 py-1 rounded font-pixel text-[9px] border border-black transition-transform hover:scale-105">
            CHI TIẾT ➝
          </button>
        </Link>
      </div>

      {/* Pet body showing Avatar & Stats */}
      <div className="flex gap-4 items-center relative z-10">
        {/* Animated avatar screen */}
        <div
          className={`${backgroundClass} w-24 h-24 rounded-lg border-2 border-double border-white/50 flex items-center justify-center overflow-hidden bg-cover bg-center bg-no-repeat`}
        >
          <img
            src={imgSrc}
            alt={pet.name}
            className="w-16 h-16 object-contain pixel-art transform hover:scale-110 transition-transform duration-300"
            onError={() => {
              // Fallback to static cloud image if anim gif is missing
              if (imgSrc !== pet.imageUrl) {
                setImgSrc(pet.imageUrl);
              }
            }}
          />
        </div>

        {/* Level and XP progress */}
        <div className="flex-1 space-y-2">
          <div className="flex justify-between items-baseline">
            <span className="font-pixel text-xs text-white">LEVEL</span>
            <span className="font-press text-base text-yellow-300">{petLevel}</span>
          </div>

          <div className="space-y-1">
            <div className="flex justify-between text-[9px] text-gray-400 font-pixel">
              <span>XP</span>
              <span>{petExperience} / 100</span>
            </div>
            {/* Custom Pixel Progress Bar */}
            <div className="w-full bg-gray-900 border border-gray-700 h-3 p-[1px] rounded-sm overflow-hidden">
              <motion.div
                className="bg-yellow-400 h-full rounded-sm shadow-[inset_-2px_0_0_rgba(0,0,0,0.2)]"
                initial={{ width: 0 }}
                animate={{ width: `${xpPercentage}%` }}
                transition={{ duration: 0.6, ease: 'easeOut' }}
              />
            </div>
          </div>

          <p className="font-pixel text-[10px] text-gray-400 italic line-clamp-1 leading-normal">
            "{pet.description}"
          </p>
        </div>
      </div>
    </motion.div>
  );
};

export default ActivePetWidget;
