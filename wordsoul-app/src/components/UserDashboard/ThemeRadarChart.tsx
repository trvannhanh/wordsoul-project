import {
    Chart as ChartJS,
    RadialLinearScale,
    PointElement,
    LineElement,
    Filler,
    Tooltip,
    Legend,
} from 'chart.js';
import { Radar } from 'react-chartjs-2';
import type { ThemePreferenceDto } from '../../types/UserDto';

ChartJS.register(RadialLinearScale, PointElement, LineElement, Filler, Tooltip, Legend);

// Emoji map để hiển thị cảm giác "Pokémon type" cho mỗi chủ đề
const themeEmoji: Record<string, string> = {
    DailyLife: '🏠',
    Nature: '🌿',
    Weather: '❄️',
    Food: '🍜',
    Technology: '⚡',
    Travel: '✈️',
    Health: '💖',
    Sports: '🥊',
    Business: '⚙️',
    Science: '🔮',
    Art: '🐉',
    Mystery: '👻',
    Dark: '🌑',
    Custom: '🔥',
    Challenge: '🪨',
    Poison: '☠️',
};

interface ThemeRadarChartProps {
    preferences: ThemePreferenceDto[];
}

const ThemeRadarChart: React.FC<ThemeRadarChartProps> = ({ preferences }) => {
    if (preferences.length < 2) {
        return (
            <div className="pokemon-background pixel-border rounded-xl p-6 mt-6">
                <h3 className="font-pixel text-xl mb-3 text-yellow-300">🗺️ Sở thích chủ đề</h3>
                <p className="font-pixel text-gray-400 text-xs text-center py-6 leading-relaxed">
                    Hãy hoàn thành thêm phiên học để<br />
                    biểu đồ sở thích của bạn hiện ra! ✨
                </p>
            </div>
        );
    }

    const labels = preferences.map(
        (p) => `${themeEmoji[p.theme] ?? '📚'} ${p.theme}`
    );
    const values = preferences.map((p) => p.completedSessionsCount);
    const maxVal = Math.max(...values, 1);

    const data = {
        labels,
        datasets: [
            {
                label: 'Phiên đã hoàn thành',
                data: values,
                backgroundColor: 'rgba(255, 204, 0, 0.18)',
                borderColor: 'rgba(255, 204, 0, 0.9)',
                pointBackgroundColor: 'rgba(255, 204, 0, 1)',
                pointBorderColor: '#fff',
                pointHoverBackgroundColor: '#fff',
                pointHoverBorderColor: 'rgba(255, 204, 0, 1)',
                borderWidth: 2,
            },
        ],
    };

    const options = {
        responsive: true,
        maintainAspectRatio: true,
        plugins: {
            legend: { display: false },
            tooltip: {
                callbacks: {
                    label: (ctx: { parsed: { r: number } }) =>
                        ` ${ctx.parsed.r} phiên hoàn thành`,
                },
            },
        },
        scales: {
            r: {
                min: 0,
                max: maxVal + 1,
                ticks: {
                    stepSize: 1,
                    color: 'rgba(255,255,255,0.4)',
                    font: { size: 9 },
                    backdropColor: 'transparent',
                },
                grid: { color: 'rgba(255,255,255,0.12)' },
                angleLines: { color: 'rgba(255,255,255,0.12)' },
                pointLabels: {
                    color: '#fde68a',
                    font: { size: 11 },
                },
            },
        },
    };

    return (
        <div className="pokemon-background pixel-border rounded-xl p-6 mt-6">
            <h3 className="font-pixel text-xl mb-1 text-yellow-300">🗺️ Sở thích chủ đề</h3>
            <p className="font-pixel text-gray-400 text-xs mb-4">
                Dựa trên {values.reduce((a, b) => a + b, 0)} phiên học đã hoàn thành
            </p>
            <div className="max-w-xs mx-auto">
                <Radar data={data} options={options} />
            </div>
        </div>
    );
};

export default ThemeRadarChart;
