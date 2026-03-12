import { useEffect, useState } from 'react';

interface PrologueIntroProps {
    dialog: string[];          // ← nhận dialog từ ngoài
    speakerName?: string;
    onStart: () => void;
    onStartLabel?: string;
    onExit?: () => void;
}

const PrologueIntro: React.FC<PrologueIntroProps> = ({
    dialog,
    speakerName = 'Giáo sư Oak',
    onStart,
    onStartLabel = '⚡ Bắt đầu kiểm tra!',
    onExit,
}) => {
    const [lineIndex, setLineIndex] = useState(0);
    const [displayedText, setDisplayedText] = useState('');
    const [charIndex, setCharIndex] = useState(0);
    const [done, setDone] = useState(false);

    // Reset khi dialog thay đổi (intro → outro)
    useEffect(() => {
        setLineIndex(0);
        setDisplayedText('');
        setCharIndex(0);
        setDone(false);
    }, [dialog]);

    useEffect(() => {
        if (lineIndex >= dialog.length) { setDone(true); return; }
        const line = dialog[lineIndex];
        if (charIndex < line.length) {
            const t = setTimeout(() => {
                setDisplayedText(prev => prev + line[charIndex]);
                setCharIndex(c => c + 1);
            }, 35);
            return () => clearTimeout(t);
        }
    }, [charIndex, lineIndex, dialog]);

    const advance = () => {
        if (lineIndex < dialog.length - 1) {
            setLineIndex(l => l + 1);
            setDisplayedText('');
            setCharIndex(0);
        } else {
            setDone(true);
        }
    };

    const skipToEnd = () => {
        setDisplayedText(dialog[dialog.length - 1]);
        setLineIndex(dialog.length - 1);
        setCharIndex(dialog[dialog.length - 1].length);
        setDone(true);
    };

    return (
        <div
            className="relative min-h-screen flex flex-col items-end justify-end pb-16"
            onClick={!done ? advance : undefined}
        >
            <div className="relative z-10 mb-1 top-20 flex flex-col items-center">
                <img
                    className="w-full h-100 object-cover"
                    src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1773293216/OakPro_vmy64u.png"
                    alt="OakProfessor"
                />
            </div>

            <div
                className="relative z-10 w-4xl mx-4 h-50 bg-gray-900 border-4 border-yellow-400 rounded-2xl p-6 shadow-2xl cursor-pointer select-none"
                onClick={(e) => { e.stopPropagation(); advance(); }}
            >
                <div className="absolute -top-3 left-6 bg-yellow-400 px-3 py-0.5 rounded font-pokemon text-xs text-gray-900">
                    {speakerName}
                </div>
                <p className="font-pixel text-white text-sm leading-relaxed min-h-[3rem]">
                    {displayedText}
                    <span className="animate-pulse">|</span>
                </p>
                <div className="mt-3 flex justify-between items-center">
                    <div className="flex gap-1">
                        {dialog.map((_, i) => (
                            <span
                                key={i}
                                className={`block w-2 h-2 rounded-full ${i <= lineIndex ? 'bg-yellow-400' : 'bg-gray-600'}`}
                            />
                        ))}
                    </div>
                    {!done
                        ? <span className="font-pixel text-gray-400 text-xs animate-pulse">Nhấp để tiếp tục ▶</span>
                        : <button
                            onClick={onStart}
                            className="font-pokemon text-xs bg-yellow-400 text-gray-900 px-4 py-1.5 rounded-lg hover:bg-yellow-300 active:scale-95 transition-all"
                        >
                            {onStartLabel}
                        </button>
                    }
                </div>
            </div>

            <button
                onClick={(e) => { e.stopPropagation(); (onExit ?? skipToEnd)(); }}
                className="absolute top-4 right-4 z-20 font-pixel text-xs text-gray-500 hover:text-gray-300"
            >
                Bỏ qua ▶▶
            </button>
        </div>
    );
};

export default PrologueIntro;
