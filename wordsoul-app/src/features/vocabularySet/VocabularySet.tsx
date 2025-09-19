
import { useEffect, useState } from 'react';
import { useDebounce } from 'use-debounce';
import type { VocabularySet } from '../../types/Dto';
import { fetchUserVocabularySets, fetchVocabularySets } from '../../services/vocabularySet';
import Card from '../../components/Card';
import Skeleton from '../../components/Skeleton';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../store/AuthContext';

const VocabularySetsPage = () => {
    const { user } = useAuth();
    const navigate = useNavigate();
    const [dailyLearningSets, setDailyLearningSets] = useState<VocabularySet[]>([]);
    const [advancedTopicsSets, setAdvancedTopicsSets] = useState<VocabularySet[]>([]);
    const [mySets, setMySets] = useState<VocabularySet[]>([]);
    const [searchTitle, setSearchTitle] = useState<string>('');
    const [debouncedSearchTitle] = useDebounce(searchTitle, 500);
    const [loading, setLoading] = useState<boolean>(true);
    const [isSearching, setIsSearching] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const loadData = async () => {
            setIsSearching(true);
            try {
                // Gọi API cho theme DailyLearning
                const dailyData = await fetchVocabularySets(
                    debouncedSearchTitle,
                    'DailyLearning',
                    undefined,
                    undefined
                );
                setDailyLearningSets(dailyData);

                // Gọi API cho theme AdvancedTopics
                const advancedData = await fetchVocabularySets(
                    debouncedSearchTitle,
                    'AdvancedTopics',
                    undefined,
                    undefined
                );
                setAdvancedTopicsSets(advancedData);

                // Gọi API cho My Vocabulary Sets (chỉ khi đăng nhập)
                if (user) {
                    const mySetsData = await fetchUserVocabularySets(
                        debouncedSearchTitle,
                        undefined,
                        undefined,
                        undefined,
                        true // Lấy các bộ sở hữu
                    );
                    setMySets(mySetsData);
                } else {
                    setMySets([]);
                }
            } catch (err) {
                setError(err instanceof Error ? err.message : 'An unexpected error occurred');
            } finally {
                setIsSearching(false);
                setLoading(false);
            }
        };

        loadData();
    }, [debouncedSearchTitle, user]);

    const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setSearchTitle(e.target.value);
    };

    const handleCreateSet = () => {
        navigate('/vocabulary-sets/create');
    };

    if (loading && !dailyLearningSets.length && !advancedTopicsSets.length && !mySets.length) {
        return (
            <div className='background-color mt-13 text-white'>
                <div className="container mx-auto p-4 w-7/12">
                    <div className="mb-8 flex flex-col sm:flex-row sm:items-center sm:space-x-4">
                        <div className="relative flex-grow">
                            <input
                                type="text"
                                value={searchTitle}
                                onChange={handleSearchChange}
                                placeholder="Tìm kiếm bộ từ vựng theo tiêu đề..."
                                className="w-full p-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                            />
                            {isSearching && (
                                <div className="absolute right-2 top-1/2 transform -translate-y-1/2">
                                    <svg
                                        className="animate-spin h-5 w-5 text-blue-500"
                                        xmlns="http://www.w3.org/2000/svg"
                                        fill="none"
                                        viewBox="0 0 24 24"
                                    >
                                        <circle
                                            className="opacity-25"
                                            cx="12"
                                            cy="12"
                                            r="10"
                                            stroke="currentColor"
                                            strokeWidth="4"
                                        ></circle>
                                        <path
                                            className="opacity-75"
                                            fill="currentColor"
                                            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                                        ></path>
                                    </svg>
                                </div>
                            )}
                        </div>
                    </div>
                    <Skeleton type="cards" />
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className='background-color mt-13 text-white'>
                <div className="container mx-auto p-4 w-7/12 text-center py-8 text-red-500">
                    {error} <button onClick={() => window.location.reload()}>Retry</button>
                </div>
            </div>
        );
    }

    return (
        <div className='background-color mt-13 text-white'>
            <div className="container mx-auto p-4 w-7/12">
                <div className="mb-8 flex flex-col sm:flex-row sm:items-center sm:space-x-4">
                    <div className="relative flex-grow">
                        <input
                            type="text"
                            value={searchTitle}
                            onChange={handleSearchChange}
                            placeholder="Tìm kiếm bộ từ vựng theo tiêu đề..."
                            className="w-full p-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                        />
                        {isSearching && (
                            <div className="absolute right-2 top-1/2 transform -translate-y-1/2">
                                <svg
                                    className="animate-spin h-5 w-5 text-blue-500"
                                    xmlns="http://www.w3.org/2000/svg"
                                    fill="none"
                                    viewBox="0 0 24 24"
                                >
                                    <circle
                                        className="opacity-25"
                                        cx="12"
                                        cy="12"
                                        r="10"
                                        stroke="currentColor"
                                        strokeWidth="4"
                                    ></circle>
                                    <path
                                        className="opacity-75"
                                        fill="currentColor"
                                        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                                        ></path>
                                    </svg>
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Section My Vocabulary Sets */}
                    {user && (
                        <section className="mb-12">
                            <h2 className="text-2xl font-bold mb-4">Bộ từ vựng của tôi</h2>
                            {mySets.length === 0 ? (
                                <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
                                    <div
                                        onClick={handleCreateSet}
                                        className="border border-gray-300 rounded-md p-4 bg-transparent flex items-center justify-center cursor-pointer hover:border-blue-500 transition-colors"
                                        style={{ minHeight: '200px' }} // Đảm bảo kích thước tương tự Card
                                    >
                                        <span className="text-4xl text-white">+</span>
                                    </div>
                                    <p className="text-gray-500 col-span-full">Bạn chưa sở hữu bộ từ vựng nào.</p>
                                </div>
                            ) : (
                                <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
                                    <div
                                        onClick={handleCreateSet}
                                        className="border border-gray-300 rounded-md p-4 bg-transparent flex items-center justify-center cursor-pointer hover:border-blue-500 transition-colors"
                                        style={{ minHeight: '200px' }}
                                    >
                                        <span className="text-4xl text-white">+</span>
                                    </div>
                                    {mySets.map((item) => (
                                        <Card
                                            key={item.id}
                                            title={item.title}
                                            description={item.description || 'Không có mô tả'}
                                            theme={item.theme}
                                            difficultyLevel={item.difficultyLevel}
                                            image={item.imageUrl || ''}
                                            vocabularySetid={item.id}
                                            isPublic={item.isPublic}
                                            isOwned={item.isOwned}
                                            createdByUsername={item.createdByUsername || 'Unknown'}
                                        />
                                    ))}
                                </div>
                            )}
                        </section>
                    )}

                    {/* Section DailyLearning */}
                    <section className="mb-12">
                        <h2 className="text-2xl font-bold mb-4">Daily Learning</h2>
                        {dailyLearningSets.length === 0 ? (
                            <p className="text-gray-500">Không tìm thấy bộ từ vựng.</p>
                        ) : (
                            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
                                {dailyLearningSets.map((item) => (
                                    <Card
                                        key={item.id}
                                        title={item.title}
                                        description={item.description || 'Không có mô tả'}
                                        theme={item.theme}
                                        difficultyLevel={item.difficultyLevel}
                                        image={item.imageUrl || ''}
                                        vocabularySetid={item.id}
                                        isPublic={item.isPublic}
                                        isOwned={item.isOwned}
                                        createdByUsername={item.createdByUsername || 'Unknown'}
                                    />
                                ))}
                            </div>
                        )}
                    </section>

                    {/* Section AdvancedTopics */}
                    <section>
                        <h2 className="text-2xl font-bold mb-4">Advanced Topics</h2>
                        {advancedTopicsSets.length === 0 ? (
                            <p className="text-gray-500">Không tìm thấy bộ từ vựng.</p>
                        ) : (
                            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
                                {advancedTopicsSets.map((item) => (
                                    <Card
                                        key={item.id}
                                        title={item.title}
                                        description={item.description || 'Không có mô tả'}
                                        theme={item.theme}
                                        difficultyLevel={item.difficultyLevel}
                                        image={item.imageUrl || ''}
                                        vocabularySetid={item.id}
                                        isPublic={item.isPublic}
                                        isOwned={item.isOwned}
                                        createdByUsername={item.createdByUsername || 'Unknown'}
                                    />
                                ))}
                            </div>
                        )}
                    </section>
                </div>
            </div>
        );
};

export default VocabularySetsPage;