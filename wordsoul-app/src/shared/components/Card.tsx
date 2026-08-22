import { useState } from "react";
import { Link } from "react-router-dom";

const THEME_VI: Record<string, string> = {
  DailyLife: 'Cuộc sống', Nature: 'Thiên nhiên', Food: 'Ẩm thực', Weather: 'Thời tiết',
  Technology: 'Công nghệ', Travel: 'Du lịch', Health: 'Sức khỏe', Sports: 'Thể thao',
  Business: 'Kinh doanh', Science: 'Khoa học', Art: 'Nghệ thuật', Communication: 'Giao tiếp',
  Mystery: 'Bí ẩn', Dark: 'Tối tăm', Academic: 'Học thuật',
  Challenge: 'Thử thách', TrapWords: 'Từ bẫy', System: 'Hệ thống',
};
const DIFF_VI: Record<string, string> = {
  Beginner: 'Dễ', Intermediate: 'Vừa', Advanced: 'Khó',
  Easy: 'Dễ', Medium: 'Vừa', Hard: 'Khó',
};

interface CardProps {
  title: string;
  description: string;
  theme: string;
  difficultyLevel: string;
  image: string;
  vocabularySetid: number;
  isPublic: boolean;
  isOwned: boolean;
  createdByUsername: string;
  onDelete?: () => void;
}

const Card: React.FC<CardProps> = ({ title, description, theme, difficultyLevel, image, vocabularySetid, createdByUsername, isOwned, onDelete }) => {
    const [isPeeling, setIsPeeling] = useState(false);
    const [hoverCorner, setHoverCorner] = useState(false);

    const handleDeleteClick = (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        if (isPeeling) return;
        setIsPeeling(true);
        setTimeout(() => onDelete?.(), 380);
    };

    return (
        <div
            className="background-color my-1 rounded-xl shadow-md overflow-hidden border hover:scale-105 transition-transform duration-300 relative h-full flex flex-col"
            style={{
                animation: isPeeling ? 'peelOut 0.4s ease-in forwards' : undefined,
            }}
        >
            {/* Peel corner — chỉ hiện khi owner */}
            {isOwned && onDelete && (
                <div
                    className="absolute top-0 right-0 z-10 cursor-pointer"
                    style={{ width: 44, height: 44 }}
                    onMouseEnter={() => setHoverCorner(true)}
                    onMouseLeave={() => setHoverCorner(false)}
                    onClick={handleDeleteClick}
                    title="Xóa bộ từ vựng"
                >
                    {/* Tam giác nền (góc giấy hở) */}
                    <svg
                        width="44" height="44"
                        viewBox="0 0 44 44"
                        className="absolute top-0 right-0"
                        style={{ transition: 'transform 0.2s', transform: hoverCorner ? 'scale(1.25)' : 'scale(1)' }}
                    >
                        <polygon
                            points="22,0 44,0 44,22"
                            fill={hoverCorner ? '#ef4444' : '#b91c1c'}
                            opacity="0.85"
                        />
                        <polygon
                            points="22,0 44,22 22,22"
                            fill="rgba(0,0,0,0.15)"
                        />
                    </svg>
                    {/* Trash icon */}
                    <svg
                        xmlns="http://www.w3.org/2000/svg"
                        className="absolute"
                        style={{ top: 4, right: 4, width: 14, height: 14, color: 'white', opacity: hoverCorner ? 1 : 0.7, transition: 'opacity 0.2s' }}
                        fill="none" viewBox="0 0 24 24" stroke="currentColor"
                    >
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5}
                            d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                </div>
            )}

            <Link to={`/vocabularySet/${vocabularySetid}`} className="custom-cursor block flex flex-col flex-1">
                <img
                    className="w-full h-36 object-cover flex-shrink-0"
                    src={image}
                    alt={title}
                />
                <div className="p-4 flex flex-col flex-1">
                    <div className="flex items-center justify-between mb-2">
                        <h2 className="text-xl text-color font-pixel">{title}</h2>
                        <span className="bg-purple-600 text-white text-xs font-medium min-w-18 text-center px-2 py-1 rounded-full">
                            {createdByUsername ? `${createdByUsername}` : 'Unknown'}
                        </span>
                    </div>
                    <p className="text-gray-600 mb-3 text-sm line-clamp-2 flex-1">
                        {description}
                    </p>
                    <div className="flex gap-2 justify-end">
                        <span className="border border-gray-300 text-gray-600 text-xs px-2 py-1 rounded-full">
                            {THEME_VI[theme] ?? theme}
                        </span>
                        <span className="border border-gray-300 text-gray-600 text-xs px-2 py-1 rounded-full">
                            {DIFF_VI[difficultyLevel] ?? difficultyLevel}
                        </span>
                    </div>
                </div>
            </Link>

            <style>{`
                @keyframes peelOut {
                    0%   { opacity: 1; transform: scale(1) rotate(0deg) translateX(0); }
                    40%  { opacity: 0.8; transform: scale(0.9) rotate(-3deg) translateX(-10px); }
                    100% { opacity: 0; transform: scale(0.4) rotate(-15deg) translateX(-80px) translateY(40px); }
                }
            `}</style>
        </div>
    );
};

export default Card;