

interface BarDetailProps {
    id: number;
    word: string;
    meaning: string;
    pronunciation: string;
    partOfSpeech?: string; 
    image?: string;
}

const Bar: React.FC<BarDetailProps> = ({ id, word, meaning, pronunciation, partOfSpeech, image }) => {
    return (
        <>
            {/* Thanh từ vựng */}
            <div key={id} className="flex justify-between items-start mt-2 border-t-2 text-color pt-3 font-pixel">
                {/* Flex column Word, loai tu, phat am,  */}
                <div className="flex flex-col gap-1 mb-4">
                    {/* Word */}
                    <div className="text-3xl ">{word}</div>
                    {/* Loai tu */}
                    <div className="text-s ">{partOfSpeech}</div>
                    {/* Phat am */}
                    <div className="text-s ">{pronunciation}</div>
                </div>
                {/* Nghĩa */}
                <div className="text-2xl ">{meaning}</div>
                {/* Ảnh minh họa */}
                <div className="w-40 h-30">
                    <img
                        src={image}
                        alt="minh hoa"
                        className="w-full h-full object-contain rounded-lg"
                    />
                </div>
            </div>
        </>
    );
};

export default Bar;