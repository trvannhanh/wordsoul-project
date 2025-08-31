import { Link } from "react-router-dom";

interface CardProps {
    title: string;
    description: string | null; // Cho phép null
    theme: string;
    difficultyLevel: string;
    image?: string;
    vocabularySetid?: number; // Đổi thành number
}

const Card: React.FC<CardProps> = ({ title, description, theme, difficultyLevel, image, vocabularySetid }) => {
    return (
        <div className="my-1 rounded-xl shadow-md overflow-hidden border-white border-2 hover:scale-105 transition-transform duration-300">
            {/* truyền vào id để link đến trang chi tiết */}
            <Link to={`/vocabularySet/${vocabularySetid}`} className="custom-cursor">
                <img
                    className="w-90 h-30 object-cover"
                    src={image}
                    alt={title}
                />
                <div className="p-4">
                    <div className="flex items-center justify-between mb-2">
                        <h2 className="text-xl font-pixel">{title}</h2>
                        <span className="bg-purple-600 text-white text-xs font-medium px-2 py-1 rounded-full">
                            NEW
                        </span>
                    </div>
                    <p className="text-gray-600 mb-3">
                        {description}
                    </p>
                    <div className="flex gap-2 justify-end">
                        <span className="border border-gray-300 text-gray-600 text-xs px-2 py-1 rounded-full">
                            {theme}
                        </span>
                        <span className="border border-gray-300 text-gray-600 text-xs px-2 py-1 rounded-full">
                            {difficultyLevel}
                        </span>
                    </div>
                </div>
            </Link>
        </div>
    );
};

export default Card;