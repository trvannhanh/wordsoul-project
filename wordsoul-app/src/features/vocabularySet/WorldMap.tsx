import { useState } from 'react';
import worldmap from '../../assets/worldmap.png';

interface Hotspot {
    theme: string;
    label: string;
    viLabel: string;
    top: string;
    left: string;
    color: string;
}

const HOTSPOTS: Hotspot[] = [
    { theme: 'DailyLife', label: 'Starting Village', viLabel: 'Làng Khởi Đầu', top: '88%', left: '33%', color: '#a8d8a8' },
    { theme: 'Nature', label: 'Green Forest', viLabel: 'Rừng Xanh', top: '63%', left: '33%', color: '#4caf50' },
    { theme: 'Weather', label: 'Snowy Region', viLabel: 'Vùng Tuyết', top: '16%', left: '23%', color: '#90caf9' },
    { theme: 'Food', label: 'Seaport', viLabel: 'Cảng Biển', top: '40%', left: '89%', color: '#ffca28' },
    { theme: 'Technology', label: 'Electric City', viLabel: 'Thành Phố Điện', top: '41%', left: '34%', color: '#ce93d8' },
    { theme: 'Travel', label: 'Sky Region', viLabel: 'Vùng Trời Cao', top: '10%', left: '51%', color: '#29b6f6' },
    { theme: 'Health', label: 'Wisdom Region', viLabel: 'Thung Lũng Huyền Ảo', top: '72%', left: '61%', color: '#f48fb1' },
    { theme: 'Sports', label: 'Arena', viLabel: 'Đấu Trường', top: '53%', left: '51%', color: '#ff7043' },
    { theme: 'Business', label: 'Iron City', viLabel: 'Thành Thép', top: '45%', left: '14%', color: '#90a4ae' },
    { theme: 'Science', label: 'Knowledge Tower', viLabel: 'Tháp Tri Thức', top: '75%', left: '72%', color: '#7986cb' },
    { theme: 'Art', label: 'Dragon Mountain', viLabel: 'Núi Rồng', top: '25%', left: '37%', color: '#ef5350' },
    { theme: 'Mystery', label: 'Ghost Castle', viLabel: 'Lâu Đài Ma', top: '36%', left: '74%', color: '#9575cd' },
    { theme: 'Dark', label: 'Mystery Region', viLabel: 'Vùng Tối', top: '15%', left: '90%', color: '#546e7a' },
    { theme: 'Custom', label: 'Volcano', viLabel: 'Núi Lửa', top: '21%', left: '67%', color: '#ff5722' },
    { theme: 'Challenge', label: 'Cave', viLabel: 'Hang Đá', top: '25%', left: '22%', color: '#a1887f' },
    { theme: 'Poison', label: 'Swamp', viLabel: 'Đầm Lầy', top: '88%', left: '84%', color: '#aed581' },
];

interface WorldMapProps {
    activeTheme: string | null;
    onSelect: (theme: string | null) => void;
}

const WorldMap: React.FC<WorldMapProps> = ({ activeTheme, onSelect }) => {
    const [hoveredTheme, setHoveredTheme] = useState<string | null>(null);

    const handleClick = (theme: string) => {
        onSelect(activeTheme === theme ? null : theme);
    };

    return (
        <div className="w-full mb-6">
            {/* Map label */}
            <div className="flex items-center gap-2 mb-3">
                <span className="text-lg font-bold">🗺️ Bản đồ thế giới</span>
                {activeTheme && (
                    <span
                        className="text-xs px-2 py-0.5 rounded-full font-semibold"
                        style={{
                            backgroundColor: HOTSPOTS.find(h => h.theme === activeTheme)?.color + '33',
                            color: HOTSPOTS.find(h => h.theme === activeTheme)?.color,
                            border: `1px solid ${HOTSPOTS.find(h => h.theme === activeTheme)?.color}`,
                        }}
                    >
                        {HOTSPOTS.find(h => h.theme === activeTheme)?.viLabel}
                    </span>
                )}
                {activeTheme && (
                    <button
                        onClick={() => onSelect(null)}
                        className="text-xs text-gray-400 hover:text-gray-200 underline ml-1"
                    >
                        Bỏ chọn
                    </button>
                )}
            </div>

            {/* Map container */}
            <div
                className="relative w-full overflow-hidden rounded-xl border border-gray-600 shadow-2xl"
                style={{ aspectRatio: '1050 / 640' }}
            >
                {/* Map image */}
                <img
                    src={worldmap}
                    alt="World Map"
                    className="w-full h-full object-cover select-none"
                    draggable={false}
                />

                {/* Hotspots */}
                {HOTSPOTS.map((spot) => {
                    const isActive = activeTheme === spot.theme;
                    const isHovered = hoveredTheme === spot.theme;

                    return (
                        <button
                            key={spot.theme}
                            onClick={() => handleClick(spot.theme)}
                            onMouseEnter={() => setHoveredTheme(spot.theme)}
                            onMouseLeave={() => setHoveredTheme(null)}
                            className="absolute transform -translate-x-1/2 -translate-y-1/2 transition-all duration-200 group"
                            style={{ top: spot.top, left: spot.left }}
                            title={spot.viLabel}
                        >
                            {/* Pulse ring when active */}
                            {isActive && (
                                <span
                                    className="absolute inset-0 rounded-full animate-ping opacity-60"
                                    style={{ backgroundColor: spot.color, transform: 'scale(1.8)' }}
                                />
                            )}

                            {/* Dot */}
                            <span
                                className="relative flex items-center justify-center rounded-full transition-all duration-200"
                                style={{
                                    width: isActive || isHovered ? '18px' : '12px',
                                    height: isActive || isHovered ? '18px' : '12px',
                                    backgroundColor: spot.color,
                                    boxShadow: isActive
                                        ? `0 0 12px 4px ${spot.color}`
                                        : isHovered
                                            ? `0 0 8px 2px ${spot.color}`
                                            : `0 0 4px 1px ${spot.color}88`,
                                    border: '2px solid white',
                                }}
                            />

                            {/* Tooltip */}
                            {isHovered && (
                                <span
                                    className="absolute z-20 bottom-full mb-2 left-1/2 -translate-x-1/2 whitespace-nowrap text-white text-xs font-bold px-2 py-1 rounded shadow-lg pointer-events-none"
                                    style={{ backgroundColor: spot.color + 'ee' }}
                                >
                                    {spot.viLabel}
                                    <span className="block text-center text-white/70 font-normal">{spot.label}</span>
                                </span>
                            )}
                        </button>
                    );
                })}
            </div>

            {/* Mobile: scrollable theme chips as fallback */}
            <div className="flex gap-2 mt-3 overflow-x-auto pb-1 sm:hidden">
                {HOTSPOTS.map((spot) => (
                    <button
                        key={spot.theme}
                        onClick={() => handleClick(spot.theme)}
                        className="flex-shrink-0 text-xs px-3 py-1 rounded-full font-semibold transition-all duration-150"
                        style={{
                            backgroundColor: activeTheme === spot.theme ? spot.color : spot.color + '22',
                            color: activeTheme === spot.theme ? '#fff' : spot.color,
                            border: `1px solid ${spot.color}`,
                        }}
                    >
                        {spot.viLabel}
                    </button>
                ))}
            </div>
        </div>
    );
};

export default WorldMap;
export { HOTSPOTS };
