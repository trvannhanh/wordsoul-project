import { useState } from 'react';
import { motion } from 'framer-motion';

// IDs must match actual DB entries (confirmed by user: 1=Bulbasaur, 4=Charmander, 7=Squirtle)
const STARTERS = [
    {
        id: 1,
        name: 'Bulbasaur',
        imageUrl: 'https://img.pokemondb.net/sprites/black-white/anim/normal/bulbasaur.gif',
        type: 'Grass 🌿',
        typeColor: 'from-green-800 to-green-600',
        borderColor: 'border-green-500',
        badgeBg: 'bg-green-700',
        description: 'Trầm tĩnh và kiên nhẫn — dành cho người học có chiều sâu.',
        flavorText: '"Cỏ của trí tuệ, mầm của sự kiên định."',
    },
    {
        id: 4,
        name: 'Charmander',
        imageUrl: 'https://img.pokemondb.net/sprites/black-white/anim/normal/charmander.gif',
        type: 'Fire 🔥',
        typeColor: 'from-orange-900 to-orange-700',
        borderColor: 'border-orange-500',
        badgeBg: 'bg-orange-700',
        description: 'Nhiệt huyết và không ngừng tiến lên — dành cho người học không bao giờ bỏ cuộc.',
        flavorText: '"Ngọn lửa đuôi cháy sáng, nghị lực không bao giờ tắt."',
    },
    {
        id: 7,
        name: 'Squirtle',
        imageUrl: 'https://img.pokemondb.net/sprites/black-white/anim/normal/squirtle.gif',
        type: 'Water 💧',
        typeColor: 'from-blue-900 to-blue-700',
        borderColor: 'border-blue-500',
        badgeBg: 'bg-blue-700',
        description: 'Linh hoạt và thích nghi — dành cho người học thông minh và quan sát.',
        flavorText: '"Nước chảy đá mòn — kiên trì sẽ thắng mọi thử thách."',
    },
];

interface StarterPickerProps {
    onPicked: (petId: number) => void;
}

const StarterPicker: React.FC<StarterPickerProps> = ({ onPicked }) => {
    const [hoverId, setHoverId] = useState<number | null>(null);
    const [pickedId, setPickedId] = useState<number | null>(null);

    const handlePick = (id: number) => {
        if (pickedId) return;
        setPickedId(id);
        // Small delay for the "caught" animation before moving on
        setTimeout(() => onPicked(id), 1400);
    };

    return (
        <div className="min-h-screen flex flex-col items-center justify-center bg-cover bg-center px-4 py-12">
            {/* Header */}
            <motion.div
                className="text-center mb-10"
                initial={{ opacity: 0, y: -20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5 }}
            >
                <p className="font-pixel text-gray-400 text-xs mb-2">Prof. Lexi:</p>
                <h1 className="font-pokemon text-2xl md:text-3xl text-yellow-300 leading-snug mb-3">
                    Chọn người bạn đồng hành!
                </h1>
                <p className="font-pixel text-gray-400 text-sm max-w-sm mx-auto leading-relaxed">
                    Người bạn này sẽ cùng bạn chinh phục hàng nghìn từ vựng. Hãy chọn thật cẩn thận!
                </p>
            </motion.div>

            {/* Cards */}
            <div className="flex flex-col sm:flex-row gap-6 max-w-4xl w-full justify-center">
                {STARTERS.map((starter, i) => {
                    const isPicked = pickedId === starter.id;
                    const isDimmed = pickedId !== null && !isPicked;

                    return (
                        <motion.div
                            key={starter.id}
                            className={`relative flex-1 max-w-xs rounded-2xl border-2 overflow-hidden cursor-pointer transition-all duration-300
                ${starter.borderColor} bg-gradient-to-b ${starter.typeColor}
                ${isDimmed ? 'opacity-30 scale-95 cursor-default' : ''}
                ${isPicked ? 'ring-4 ring-white ring-offset-2 ring-offset-transparent scale-105' : ''}
              `}
                            initial={{ opacity: 0, y: 40 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ delay: i * 0.12, duration: 0.4 }}
                            whileHover={!pickedId ? { y: -8, scale: 1.03 } : {}}
                            onHoverStart={() => setHoverId(starter.id)}
                            onHoverEnd={() => setHoverId(null)}
                            onClick={() => handlePick(starter.id)}
                        >
                            {/* Pokébole decoration (top right) */}
                            <div className={`absolute -top-6 -right-6 w-20 h-20 rounded-full border-4 border-white opacity-10 ${isPicked ? 'animate-spin-slow' : ''}`} />

                            {/* Caught banner */}
                            {isPicked && (
                                <motion.div
                                    className="absolute inset-0 flex items-center justify-center z-20 bg-black/40"
                                    initial={{ opacity: 0 }}
                                    animate={{ opacity: 1 }}
                                >
                                </motion.div>
                            )}

                            <div className="p-5">
                                {/* Type badge */}
                                <span className={`font-pixel text-xs text-white px-2 py-0.5 rounded-full ${starter.badgeBg}`}>
                                    {starter.type}
                                </span>

                                {/* Sprite */}
                                <div className="flex justify-center my-4">
                                    <motion.img
                                        src={starter.imageUrl}
                                        alt={starter.name}
                                        className="h-20 w-20 object-contain drop-shadow-2xl"
                                        animate={hoverId === starter.id && !pickedId ? { y: [0, -8, 0] } : { y: 0 }}
                                        transition={{ repeat: Infinity, duration: 1.6, ease: 'easeInOut' }}
                                    />
                                </div>

                                <h3 className="font-pixel text-xl text-white text-center mb-2">{starter.name}</h3>
                                <p className="font-pixel text-gray-300 text-xs text-center leading-relaxed mb-3">
                                    {starter.description}
                                </p>
                                <p className="font-pixel text-yellow-300 text-xs text-center italic">
                                    {starter.flavorText}
                                </p>
                            </div>

                            {/* Choose button */}
                            {!pickedId && (
                                <div className="px-5 pb-5">
                                    <button className="w-full font-pixel text-xs bg-white text-gray-900 rounded-lg py-2 hover:bg-yellow-300 active:scale-95 transition-all">
                                        Chọn {starter.name}!
                                    </button>
                                </div>
                            )}
                        </motion.div>
                    );
                })}
            </div>
        </div>
    );
};

export default StarterPicker;
