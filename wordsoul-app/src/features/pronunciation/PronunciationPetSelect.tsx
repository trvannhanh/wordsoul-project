import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetchPets } from '../../services/pet';

interface OwnedPet {
  id: number;
  petId: number;
  petName: string;
  petImageUrl?: string;
  level: number;
  xpMultiplier?: number;
}

const ACCENT = '#a78bfa'; // violet-400 — màu chủ đạo cho pronunciation

export default function PronunciationPetSelect() {
  const navigate = useNavigate();
  const [pets, setPets] = useState<OwnedPet[]>([]);
  const [selected, setSelected] = useState<number | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchPets({ isOwned: true })
      .then((data) => {
        const mapped: OwnedPet[] = data.map((p) => ({
          id: p.id,
          petId: p.id,
          petName: p.name,
          petImageUrl: p.imageUrl,
          level: 1,
        }));
        setPets(mapped);
      })
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const handleStart = () => {
    // Pass petId qua state — không cần thay đổi URL phức tạp
    navigate('/pronunciation/session', { state: { petId: selected } });
  };

  const handleSkip = () => {
    navigate('/pronunciation/session', { state: { petId: null } });
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center" style={{ background: 'rgb(2,6,23)' }}>
        <div className="w-8 h-8 border-4 border-t-transparent rounded-full animate-spin" style={{ borderColor: ACCENT, borderTopColor: 'transparent' }} />
      </div>
    );
  }

  return (
    <div className="min-h-screen text-white flex flex-col items-center py-12 px-4" style={{ background: 'rgb(2,6,23)' }}>
      {/* Back button */}
      <button
        onClick={() => navigate(-1)}
        className="self-start text-gray-400 hover:text-white font-pixel text-xs mb-8 flex items-center gap-1"
      >
        ← Quay lại
      </button>

      {/* Header */}
      <div className="text-center mb-10">
        <div
          className="mx-auto mb-4 w-20 h-20 rounded-full flex items-center justify-center text-4xl"
          style={{ background: `${ACCENT}22`, border: `2px solid ${ACCENT}66`, boxShadow: `0 0 30px ${ACCENT}44` }}
        >
          🎙️
        </div>
        <h1 className="font-press text-xl mb-2" style={{ color: ACCENT, textShadow: `0 0 20px ${ACCENT}88` }}>
          LUYỆN PHÁT ÂM
        </h1>
        <p className="font-noto text-gray-400 text-sm max-w-xs">
          Chọn Thú Ảo đồng hành để nhận thêm XP khi phát âm chuẩn!
        </p>
      </div>

      {/* Pet grid */}
      {pets.length === 0 ? (
        <div className="mb-8 px-6 py-5 rounded-2xl border text-center" style={{ background: `${ACCENT}08`, borderColor: `${ACCENT}33` }}>
          <p className="text-gray-400 font-noto text-sm">Bạn chưa sở hữu thú ảo nào.</p>
          <p className="text-gray-500 font-pixel text-[10px] mt-1">Bắt đầu phiên luyện mà không cần thú ảo.</p>
        </div>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-3 gap-4 w-full max-w-md mb-8">
          {pets.map((pet) => {
            const isSelected = selected === pet.id;
            return (
              <button
                key={pet.id}
                onClick={() => setSelected(isSelected ? null : pet.id)}
                className={`relative rounded-2xl p-4 transition-all duration-200 border-2 flex flex-col items-center gap-2 ${
                  isSelected
                    ? 'scale-105'
                    : 'border-gray-700/60 bg-gray-800/40 hover:border-gray-500'
                }`}
                style={
                  isSelected
                    ? {
                        borderColor: ACCENT,
                        background: `${ACCENT}15`,
                        boxShadow: `0 0 20px ${ACCENT}44`,
                      }
                    : {}
                }
              >
                {isSelected && (
                  <div
                    className="absolute top-2 right-2 w-5 h-5 rounded-full text-black text-[10px] font-press flex items-center justify-center"
                    style={{ background: ACCENT }}
                  >
                    ✓
                  </div>
                )}
                {pet.petImageUrl ? (
                  <img src={pet.petImageUrl} alt={pet.petName} className="w-14 h-14 object-contain pixel-art" />
                ) : (
                  <div className="w-14 h-14 rounded-full bg-gray-700 flex items-center justify-center text-3xl">🎯</div>
                )}
                <span className="font-pixel text-[11px] text-white text-center leading-tight">{pet.petName}</span>
                <span className="font-noto text-gray-400 text-[10px]">Lv. {pet.level}</span>
              </button>
            );
          })}
        </div>
      )}

      {/* XP bonus preview */}
      {selected !== null && (
        <div
          className="mb-6 px-5 py-3 rounded-xl border text-center"
          style={{ background: `${ACCENT}10`, borderColor: `${ACCENT}44` }}
        >
          <p className="font-pixel text-[11px]" style={{ color: ACCENT }}>
            ✨ Thú Ảo đang active sẽ nhân bonus XP cho mỗi lần phát âm!
          </p>
        </div>
      )}

      {/* Action buttons */}
      <div className="flex flex-col gap-3 w-full max-w-xs">
        <button
          onClick={handleStart}
          className="w-full py-4 rounded-2xl font-press text-sm transition-all duration-200 text-black hover:scale-105"
          style={{ background: ACCENT, boxShadow: `0 0 20px ${ACCENT}66` }}
        >
          🎙️ BẮT ĐẦU LUYỆN TẬP
        </button>
        <button
          onClick={handleSkip}
          className="w-full py-3 rounded-2xl font-pixel text-xs text-gray-400 hover:text-gray-200 border border-gray-700 hover:border-gray-500 transition-all"
        >
          Bỏ qua, không chọn Thú Ảo
        </button>
      </div>
    </div>
  );
}
