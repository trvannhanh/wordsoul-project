import { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import Bar from '../../components/Bar';
import HeroSection from '../../components/HeroSection';
import { fetchVocabularySetDetail } from '../../services/vocabularySet';
import { createLearningSession } from '../../services/learningSession';


interface Vocabulary {
    id: number;
    word: string;
    meaning: string;
    imageUrl: string | null;
    pronunciation: string | null;
    partOfSpeech: string;
}

interface VocabularySetDetail {
    id: number;
    title: string;
    description: string | null;
    theme: string;
    difficultyLevel: string;
    imageUrl?: string;
    isActive?: boolean;
    createdAt?: string;
    vocabularies: Vocabulary[];
    totalVocabularies: number;
    currentPage: number;
    pageSize: number;
    totalPages: number;
}

const VocabularySetDetail: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const [vocabularySet, setVocabularySet] = useState<VocabularySetDetail | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(false);
    const navigate = useNavigate();

    const handleCreateLearningSession = async () => {
        setError(null);
        setIsLoading(true);
        try {
            const session = await createLearningSession(Number(id));
            navigate(`/learningSession/${session.id}?mode=learning`, {
            state: { petId: session.petId }, // Truyền PetId qua state
        });
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        } catch (err: any) {
            setError(err?.response?.data?.message || "Lỗi tạo phiên học");
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
            try {
                const data = await fetchVocabularySetDetail(Number(id));
                setVocabularySet(data);
                setLoading(false);
            } catch (error) {
                setError('Failed to load vocabulary set details');
                setLoading(false);
                console.error('Error fetching vocabulary set details:', error);
            }
        };
        fetchData();
    }, [id]);

    if (loading) return <div>Loading...</div>;
    if (error) return <div>{error}</div>;
    if (!vocabularySet) return <div>No data available</div>;

    return (
        <>

            <HeroSection
                title={vocabularySet.title}
                description={vocabularySet.description || 'No description available'}
                textButton="Bắt đầu"
                image={vocabularySet.imageUrl || 'https://via.placeholder.com/600x200'}
                bottomImage="./src/assets/grass.gif"
                hidden={true}
            />

            {/* <div className="flex gap-4 mb-6">
                <span className="border border-gray-300 text-gray-300 text-sm px-3 py-1 rounded-full">
                    Theme: {vocabularySet.theme}
                </span>
                <span className="border border-gray-300 text-gray-300 text-sm px-3 py-1 rounded-full">
                    Difficulty: {vocabularySet.difficultyLevel}
                </span>
                <span className="border border-gray-300 text-gray-300 text-sm px-3 py-1 rounded-full">
                    Total Vocabularies: {vocabularySet.totalVocabularies}
                </span>
            </div> */}
            <div >
                <button className="relative flex items-center justify-center w-full px-2 py-1.5 bg-yellow-300 text-black rounded-xs hover:bg-yellow-200 custom-cursor " onClick={handleCreateLearningSession} disabled={isLoading}>
                    <span className="absolute left-0 w-0.5 h-full bg-yellow-500" />
                    <span className="mx-1 text-xs font-bold font-sans">Học</span>
                    <span className="absolute right-0 w-0.5 h-full bg-yellow-500" />
                    <span className="absolute top-6.5 right-0 w-full h-1 bg-yellow-500" />
                    <span className="absolute bottom-6.5 right-0 w-full h-0.5 bg-yellow-500" />
                </button>
            </div>
            <div className="background-color text-white h-auto w-full flex justify-center items-center">
                <div className="w-7/12 flex items-start gap-5 py-10">
                    {/* Bên trái  */}
                    <div className="w-9/12 border-white border-2 rounded-lg p-3">
                        {vocabularySet.vocabularies.map((vocab) => (
                            <Bar
                                id={vocab.id}
                                word={vocab.word}
                                meaning={vocab.meaning}
                                pronunciation={vocab.pronunciation || 'N/A'}
                                partOfSpeech={vocab.partOfSpeech}
                                image={vocab.imageUrl || 'https://via.placeholder.com/150'}
                            />
                        ))}

                        <div className="mt-6 text-gray-400">
                            Page {vocabularySet.currentPage} of {vocabularySet.totalPages} (Showing {vocabularySet.vocabularies.length} of {vocabularySet.totalVocabularies} vocabularies)
                        </div>
                    </div>
                    {/* Bên phải */}
                    <div className="w-3/12 flex flex-col gap-3">
                        {/* Profile */}
                        <div className="flex flex-col gap-3 items-center border-2 border-white rounded-lg p-3">
                            {/* profile box */}
                            <div>
                                <div className="flex gap-2 mb-3">
                                    <div className="w-20 h-20">
                                        <img
                                            src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756453095/boy_c1k3lt.gif"
                                            alt="minh hoa"
                                            className="w-full h-full object-cover"
                                        />
                                    </div>
                                    <div>
                                        <div className='font-pixel'>Giidavibe</div>
                                        <div className='font-extralight'>level 1</div>
                                    </div>
                                </div>
                                <Link to="/signup" className="no-underline">
                                    <button className="relative flex items-center justify-center w-full px-2 py-1.5 bg-yellow-300 text-black rounded-xs hover:bg-yellow-200 custom-cursor">
                                        <span className="absolute left-0 w-0.5 h-full bg-yellow-500" />
                                        <span className="mx-1 text-xs font-bold font-sans">View Profile</span>
                                        <span className="absolute right-0 w-0.5 h-full bg-yellow-500" />
                                        <span className="absolute top-6.5 right-0 w-full h-1 bg-yellow-500" />
                                        <span className="absolute bottom-6.5 right-0 w-full h-0.5 bg-yellow-500" />
                                    </button>
                                </Link>
                            </div>

                        </div>
                        {/* Progress */}
                        <div className="flex flex-col gap-3 items-center border-2 border-white rounded-lg p-3">
                            <div>Progress</div>
                            <div>Hello</div>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
};

export default VocabularySetDetail;