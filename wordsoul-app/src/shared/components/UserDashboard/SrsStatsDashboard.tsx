import React from 'react';
import { Link } from 'react-router-dom';
import { Doughnut, Bar } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  type TooltipItem,
} from 'chart.js';
import type { UserProgressDto } from '../../../types/UserDto';

ChartJS.register(CategoryScale, LinearScale, BarElement, ArcElement, Title, Tooltip, Legend);

interface SrsStatsDashboardProps {
  progress: UserProgressDto | null;
}

const STATE_LABELS: Record<string, string> = {
  New: 'Mới học',
  Learning: 'Làm quen',
  Review: 'Ghi nhớ',
  Mastered: 'Khắc sâu',
  Relearning: 'Cần ôn lại',
};

const STATE_COLORS: Record<string, string> = {
  New: 'rgba(148, 163, 184, 0.8)',
  Learning: 'rgba(96, 165, 250, 0.8)',
  Review: 'rgba(251, 146, 60, 0.8)',
  Mastered: 'rgba(52, 211, 153, 0.8)',
  Relearning: 'rgba(248, 113, 113, 0.8)',
};

const LEVEL_LABELS: Record<number, string> = {
  0: 'Mới học',
  1: 'Làm quen',
  2: 'Ghi nhớ',
  3: 'Thành thạo',
  4: 'Khắc sâu',
};

const SrsStatsDashboard: React.FC<SrsStatsDashboardProps> = ({ progress }) => {
  if (!progress) {
    return (
      <div className="pokemon-background pixel-border rounded-xl p-6 mt-6 animate-pulse">
        <p className="font-pixel text-xs text-gray-400 text-center">Đang tải dữ liệu học...</p>
      </div>
    );
  }

  const memoryStateStats = progress.memoryStateStats || [];
  const weeklyActivities = progress.weeklyActivities || [];
  const retentionRate = progress.retentionRate ?? 0;
  const averageRecallSpeed = progress.averageRecallSpeed ?? 0;

  // Hỗ trợ hạ cấp mượt mà (Graceful Degradation): Nếu chưa thể lấy được dữ liệu SRS nâng cao
  // (do DLL backend bị lock chưa khởi động lại), ta đếm tổng số từ và lấy dữ liệu vẽ chart từ vocabularyStats cũ.
  const hasSRSData = memoryStateStats.length > 0;
  const totalWords = hasSRSData
    ? memoryStateStats.reduce((a, b) => a + b.count, 0)
    : (progress.vocabularyStats || []).reduce((a, b) => a + b.count, 0);

  if (totalWords === 0) {
    return (
      <div className="pokemon-background pixel-border rounded-xl p-5 mt-6 text-center space-y-4">
        <h3 className="font-pixel text-lg text-yellow-300 border-b border-gray-700 pb-2 text-left">
          TIẾN TRÌNH HỌC TẬP
        </h3>
        <div className="py-6 flex flex-col items-center">
          <span className="text-4xl mb-3 animate-pulse">📚</span>
          <p className="font-pixel text-xs text-yellow-400">CHƯA CÓ TIẾN TRÌNH HỌC TẬP</p>
          <p className="font-pixel text-[9px] text-gray-400 mt-2 max-w-sm leading-relaxed">
            Hệ thống lặp lại ngắt quãng của Vocamon sẽ phân tích độ khó từ vựng, chuỗi nhớ đúng và tốc độ phản xạ của bạn khi bạn bắt đầu học để vẽ biểu đồ não bộ!
          </p>
          <Link to="/vocabularySet" className="mt-4">
            <button className="bg-yellow-500 hover:bg-yellow-400 text-black px-4 py-2 rounded font-pixel text-xs border border-black hover:shadow-lg transition-transform hover:scale-105 active:translate-y-0.5">
              HỌC TỪ VỰNG NGAY
            </button>
          </Link>
        </div>
      </div>
    );
  }

  // 1. Doughnut chart configuration (Memory States or Level stats fallback)
  const chartLabels = hasSRSData
    ? memoryStateStats.map((s) => STATE_LABELS[s.state] || s.state)
    : (progress.vocabularyStats || []).map((s) => LEVEL_LABELS[s.level] || `Cấp độ ${s.level} 🎓`);

  const chartData = hasSRSData
    ? memoryStateStats.map((s) => s.count)
    : (progress.vocabularyStats || []).map((s) => s.count);

  const chartColors = hasSRSData
    ? memoryStateStats.map((s) => STATE_COLORS[s.state] || '#fff')
    : ['rgba(148, 163, 184, 0.8)', 'rgba(96, 165, 250, 0.8)', 'rgba(251, 146, 60, 0.8)', 'rgba(52, 211, 153, 0.8)', 'rgba(248, 113, 113, 0.8)'];

  const doughnutData = {
    labels: chartLabels,
    datasets: [
      {
        data: chartData,
        backgroundColor: chartColors,
        borderColor: '#000',
        borderWidth: 2,
      },
    ],
  };

  const doughnutOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom' as const,
        labels: {
          color: '#fff',
          font: { family: 'PokemonClassic', size: 8 },
          padding: 10,
        },
      },
      tooltip: {
        callbacks: {
          label: (ctx: TooltipItem<'doughnut'>) => String(ctx.raw) + ' từ',
        },
      },
    },
    cutout: '65%',
  };

  // 2. Bar chart configuration (Weekly Review Activity)
  const barData = {
    labels: weeklyActivities.map((a) => a.dateLabel),
    datasets: [
      {
        label: 'Số từ đã ôn',
        data: weeklyActivities.map((a) => a.count),
        backgroundColor: 'rgba(255, 215, 0, 0.8)',
        borderColor: '#FFD700',
        borderWidth: 1.5,
      },
    ],
  };

  const barOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          stepSize: 1,
          color: '#fff',
          font: { family: 'PokemonClassic', size: 9 },
        },
        grid: { color: 'rgba(255, 255, 255, 0.1)' },
      },
      x: {
        ticks: {
          color: '#fff',
          font: { family: 'PokemonClassic', size: 9 },
        },
        grid: { display: false },
      },
    },
  };

  return (
    <div className="pokemon-background pixel-border rounded-xl p-5 mt-6 space-y-6">
      <h3 className="font-pixel text-lg text-yellow-300 border-b border-gray-700 pb-2">
        TIẾN TRÌNH HỌC TẬP
      </h3>

      {/* Top row: Memory distribution circle vs Statistics text cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 items-center">
        {/* Doughnut Chart representing memory segments */}
        <div className="relative flex flex-col items-center">
          <div className="w-full h-44 sm:h-48">
            <Doughnut data={doughnutData} options={doughnutOptions} />
          </div>
          {/* Centered count label inside the doughnut hole */}
          <div className="absolute top-[38%] left-1/2 -translate-y-1/2 -translate-x-1/2 flex flex-col items-center pointer-events-none">
            <span className="font-press text-xs text-yellow-400">{totalWords}</span>
            <span className="font-pixel text-[8px] text-gray-400 mt-0.5">TỪ HỌC</span>
          </div>
        </div>

        {/* Cognitive Metric Cards */}
        <div className="grid grid-cols-2 md:grid-cols-1 gap-3">
          {/* Retention score card */}
          <div className="bg-black/60 p-3 rounded-lg border border-gray-700">
            <span className="font-pixel text-[9px] text-gray-400 block uppercase">
              Tỉ lệ ghi nhớ
            </span>
            <div className="flex items-baseline gap-2 mt-1">
              <span className="font-press text-sm text-green-400">
                {retentionRate}%
              </span>
              <span className="font-pixel text-[8px] text-gray-500">SM-2</span>
            </div>
            <p className="font-pixel text-[8px] text-gray-400 mt-1 leading-normal">
              {retentionRate > 85
                ? 'Tốt! Độ giữ chân từ ổn định.'
                : 'Cần ôn tập đúng hẹn để cải thiện.'}
            </p>
          </div>

          {/* Recall response time card */}
          <div className="bg-black/60 p-3 rounded-lg border border-gray-700">
            <span className="font-pixel text-[9px] text-gray-400 block uppercase">
              Tốc độ phản xạ
            </span>
            <div className="flex items-baseline gap-2 mt-1">
              <span className="font-press text-sm text-cyan-400">
                {averageRecallSpeed}s
              </span>
              <span className="font-pixel text-[8px] text-gray-500">giây</span>
            </div>
            <p className="font-pixel text-[8px] text-gray-400 mt-1 leading-normal">
              Thời gian trung bình để đưa ra đáp án.
            </p>
          </div>
        </div>
      </div>

      {/* Bottom Section: Weekly review activity frequency */}
      <div className="border-t border-gray-700/60 pt-4">
        <h4 className="font-pixel text-xs text-gray-300 mb-2">
          Tần suất ôn tập (7 ngày qua)
        </h4>
        <div className="w-full h-36">
          <Bar data={barData} options={barOptions} />
        </div>
      </div>
    </div>
  );
};

export default SrsStatsDashboard;
