

interface BarDetailProps {
    id: number;
    word: string;
    meaning: string;
    pronunciation: string;
    partOfSpeech?: string;
    image?: string;
    isCustomEdited?: boolean;
    originalMeaning?: string | null;
    memoryState?: string;
    onEdit?: () => void;
    onClick?: () => void;
}

const Bar: React.FC<BarDetailProps> = ({ id, word, meaning, pronunciation, partOfSpeech, image, isCustomEdited, originalMeaning, memoryState, onEdit, onClick }) => {
    const memoryDot: Record<string, string> = {
        Mastered: 'bg-green-500',
        Review: 'bg-yellow-400',
        Learning: 'bg-blue-500',
    };
    return (
        <>
            {/* Thanh từ vựng */}
            <div
                key={id}
                className="flex justify-between items-start mt-2 border-t-2 text-color pt-3 font-pixel cursor-pointer hover:bg-white/5 rounded px-1 transition-colors"
                onClick={onClick}
            >
                {/* Flex column Word, loai tu, phat am */}
                <div className="flex flex-col gap-1 mb-4 min-w-0 w-[35%]">
                    {/* Word + badges */}
                    <div className="flex items-center gap-2 flex-wrap">
                        <span className="text-2xl break-all">{word}</span>
                        {memoryState && memoryDot[memoryState] && (
                            <span className={`w-2.5 h-2.5 rounded-full flex-shrink-0 ${memoryDot[memoryState]}`} title={memoryState} />
                        )}
                        {isCustomEdited && (
                            <span className="text-[10px] bg-yellow-600/30 text-yellow-300 border border-yellow-600 px-1.5 py-0.5 rounded-full whitespace-nowrap">Đã chỉnh sửa</span>
                        )}
                    </div>
                    {/* Loai tu */}
                    <div className="text-s">{partOfSpeech}</div>
                    {/* Phat am */}
                    <div className="text-s text-blue-300 font-mono break-all">{pronunciation}</div>
                    {/* Original meaning (mờ, khi bị override) */}
                    {originalMeaning && (
                        <div className="text-xs text-gray-500 line-through break-words">{originalMeaning}</div>
                    )}
                </div>
                {/* Nghĩa */}
                <div className="text-lg mx-3 w-[35%] min-w-0 break-words leading-snug">{meaning}</div>
                {/* Ảnh minh họa + nút sửa */}
                <div className="flex flex-col items-end gap-1 flex-shrink-0 w-[28%]">
                    <div className="w-full max-w-[120px] h-20">
                        <img
                            src={image}
                            alt="minh hoa"
                            className="w-full h-full object-contain rounded-lg"
                        />
                    </div>
                    {onEdit && (
                        <button
                            onClick={(e) => { e.stopPropagation(); onEdit(); }}
                            className="text-xs text-gray-400 hover:text-yellow-300 flex items-center gap-1 transition-colors"
                            title="Chỉnh sửa override"
                        >
                            <svg className="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                            </svg>
                            Sửa
                        </button>
                    )}
                </div>
            </div>
        </>
    );
};

export default Bar;