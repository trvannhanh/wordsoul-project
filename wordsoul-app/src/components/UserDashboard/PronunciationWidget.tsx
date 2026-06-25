import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetchPronunciationStats } from '../../services/pronunciation';
import type { PronunciationStatsDto } from '../../services/pronunciation';

const ACCENT = '#a78bfa'; // violet-400

export default function PronunciationWidget() {
  const navigate = useNavigate();
  const [stats, setStats] = useState<PronunciationStatsDto | null>(null);

  useEffect(() => {
    fetchPronunciationStats()
      .then(setStats)
      .catch(() => {
        // Không hiển thị lỗi — widget ẩn nhẹ nhàng
      });
  }, []);

  return (
    <div
      className="rounded-2xl border p-5 space-y-4 relative overflow-hidden"
      style={{
        background: `linear-gradient(135deg, rgba(2,6,23,0.95) 0%, ${ACCENT}0d 100%)`,
        borderColor: `${ACCENT}33`,
      }}
    >
      {/* Ambient glow */}
      <div
        className="absolute -top-8 -right-8 w-32 h-32 rounded-full blur-3xl pointer-events-none"
        style={{ background: `${ACCENT}18` }}
      />

      {/* Header */}
      <div className="flex items-center justify-between relative z-10">
        <div className="flex items-center gap-2">
          <div
            className="w-8 h-8 rounded-lg flex items-center justify-center text-base"
            style={{ background: `${ACCENT}22`, border: `1px solid ${ACCENT}55` }}
          >
            🎙️
          </div>
          <div>
            <h3 className="font-pixel text-lg" style={{ color: ACCENT }}>LUYỆN PHÁT ÂM</h3>
            <p className="font-noto text-[10px] text-gray-500">Pronunciation Practice</p>
          </div>
        </div>
      </div>

      {/* Stats row */}
      {stats && stats.totalAttempts > 0 ? (
        <div className="grid grid-cols-3 gap-2 relative z-10">
          {[
            { label: 'Tổng lần', val: stats.totalAttempts.toString() },
            { label: 'Chuẩn', val: stats.perfectCount.toString() },
            { label: 'Tỉ lệ', val: `${stats.perfectRate.toFixed(0)}%` },
          ].map(({ label, val }) => (
            <div
              key={label}
              className="rounded-xl p-2 text-center border border-white/5"
              style={{ background: 'rgba(255,255,255,0.03)' }}
            >
              <p className="text-gray-500 font-noto text-[9px] mb-0.5">{label}</p>
              <p className="font-press text-xs" style={{ color: ACCENT }}>{val}</p>
            </div>
          ))}
        </div>
      ) : (
        <p className="font-noto text-gray-500 text-xs relative z-10">
          Bạn chưa luyện phát âm lần nào. Bắt đầu ngay hôm nay!
        </p>
      )}

      {/* Perfect rate bar */}
      {stats && stats.totalAttempts > 0 && (
        <div className="relative z-10">
          <div className="flex justify-between font-pixel text-[9px] text-gray-500 mb-1.5">
            <span>Tỉ lệ Chuẩn</span>
            <span style={{ color: ACCENT }}>{stats.perfectRate.toFixed(1)}%</span>
          </div>
          <div className="h-1.5 bg-gray-800 rounded-full overflow-hidden">
            <div
              className="h-full rounded-full transition-all duration-700"
              style={{
                width: `${stats.perfectRate}%`,
                background: ACCENT,
                boxShadow: `0 0 6px ${ACCENT}88`,
              }}
            />
          </div>
        </div>
      )}

      {/* CTA Button */}
      <button
        onClick={() => navigate('/pronunciation')}
        className="relative z-10 w-full py-3 rounded-xl font-pixel text-sm text-black transition-all duration-200 hover:scale-[1.02] active:scale-[0.98] custom-cursor"
        style={{
          background: ACCENT,
          boxShadow: `0 0 18px ${ACCENT}55`,
        }}
      >
        🎙️ {stats && stats.totalAttempts > 0 ? 'Luyện tiếp' : 'Bắt đầu luyện tập'}
      </button>
    </div>
  );
}
