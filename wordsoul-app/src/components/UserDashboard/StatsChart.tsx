import { Bar } from 'react-chartjs-2';

import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';
import type { UserProgressDto } from '../../types/UserDto';

// Đăng ký ChartJS components
ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

interface StatsChartProps {
  progress: UserProgressDto | null;
}

const StatsChart: React.FC<StatsChartProps> = ({ progress }) => {
  return (
    <div className="pokemon-background pixel-border rounded-xl p-6 mt-6">
      <h3 className="font-pokemon text-xl mb-4 text-yellow-300">Thống kê từ vựng</h3>
      {progress && (
        <Bar
          data={{
            labels: progress.vocabularyStats.map((s) => `Lv ${s.level}`),
            datasets: [
              {
                label: 'Số lượng từ',
                data: progress.vocabularyStats.map((s) => s.count),
                backgroundColor: 'rgba(255, 204, 0, 0.7)', // Màu vàng Pokémon
                borderColor: 'rgba(255, 204, 0, 1)',
                borderWidth: 2,
              },
            ],
          }}
          options={{
            responsive: true,
            plugins: {
              legend: { display: false },
              title: { display: false },
            },
            scales: {
              y: {
                beginAtZero: true,
                ticks: {
                  stepSize: 1,
                  color: '#fff',
                  font: { family: 'PokemonClassic', size: 12 },
                },
                grid: { color: 'rgba(255, 255, 255, 0.2)' },
              },
              x: {
                ticks: {
                  color: '#fff',
                  font: { family: 'PokemonClassic', size: 12 },
                },
                grid: { display: false },
              },
            },
          }}
        />
      )}
    </div>
  );
};

export default StatsChart;