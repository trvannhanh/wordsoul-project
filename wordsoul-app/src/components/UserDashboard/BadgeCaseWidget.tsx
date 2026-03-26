import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetchGyms } from '../../services/gym';
import type { GymLeaderDto } from '../../types/GymTypes';
import { GymStatus } from '../../types/GymTypes';

const BADGE_ICONS: Record<number, string> = {
  1: '🏅', 2: '🎖️', 3: '🥇', 4: '🏆',
  5: '⚡', 6: '🦅', 7: '💫', 8: '🥊',
};

export default function BadgeCaseWidget() {
  const [gyms, setGyms] = useState<GymLeaderDto[]>([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    fetchGyms().then(data => {
      setGyms(data);
    }).catch(err => {
      console.error(err);
    }).finally(() => {
      setLoading(false);
    });
  }, []);

  if (loading) return (
    <div className="bg-black/60 p-4 rounded-xl border-2 border-yellow-400 mt-6 animate-pulse min-h-[140px]"></div>
  );

  const defeatedCount = gyms.filter(g => g.status === GymStatus.Defeated).length;

  return (
    <div className="bg-black/80 rounded-xl border-2 border-yellow-400 p-5 mt-6 relative overflow-hidden group hover:shadow-[0_0_15px_rgba(250,204,21,0.5)] transition-shadow">
      <div className="absolute -inset-1 bg-gradient-to-r from-yellow-400/20 via-transparent to-yellow-400/20 opacity-0 group-hover:opacity-100 transition-opacity"></div>
      
      <div className="flex justify-between items-center mb-4 relative z-10">
        <div>
          <h2 className="font-press text-yellow-500 text-sm drop-shadow-md">KANTO BADGE CASE</h2>
          <p className="font-pixel text-[10px] text-gray-400 mt-1">{defeatedCount} / 8 BADGES EARNED</p>
        </div>
        <button 
          onClick={() => navigate('/gym')}
          className="bg-yellow-500 hover:bg-yellow-400 text-black px-3 py-1.5 rounded font-pixel text-[10px] transition-transform hover:scale-105"
        >
          GYM CIRCUIT ➝
        </button>
      </div>

      <div className="flex gap-2 justify-between bg-black/50 p-3 rounded-lg border border-gray-700 relative z-10">
        {[1, 2, 3, 4, 5, 6, 7, 8].map(order => {
          const gym = gyms.find(g => g.gymOrder === order);
          const isDefeated = gym?.status === GymStatus.Defeated;
          return (
            <div 
              key={order} 
              className={`text-xl sm:text-2xl transition-all duration-300 ${isDefeated ? 'scale-110 drop-shadow-[0_0_8px_rgba(250,204,21,0.8)]' : 'grayscale opacity-30'}`}
              title={gym?.badgeName || `Gym ${order}`}
            >
              {BADGE_ICONS[order]}
            </div>
          );
        })}
      </div>
    </div>
  );
}
