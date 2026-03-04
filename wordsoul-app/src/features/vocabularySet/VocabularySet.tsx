import { useEffect, useState } from 'react';
import { useDebounce } from 'use-debounce';
import { fetchUserVocabularySets, fetchVocabularySets } from '../../services/vocabularySet';
import Card from '../../components/Card';
import Skeleton from '../../components/Skeleton';
import { useNavigate } from 'react-router-dom';
import type { VocabularySetDto } from '../../types/VocabularySetDto';
import { useAuth } from '../../hooks/Auth/useAuth';

// Theme groups theo VocabularySetThemeEnum
const DAILY_THEMES = ['DailyLife', 'Nature', 'Weather', 'Food'] as const;
const ADVANCED_THEMES = ['Business', 'Science', 'Art', 'Mystery', 'Dark'] as const;

// Spinner tái sử dụng
const Spinner = () => (
    <div className="absolute right-2 top-1/2 -translate-y-1/2">
        <svg className="animate-spin h-5 w-5 sm:h-6 sm:w-6 text-blue-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
        </svg>
    </div>
);

// Card grid tái sử dụng
const CardGrid = ({ items }: { items: VocabularySetDto[] }) => (
    <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4 sm:gap-6">
        {items.map((item) => (
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
);

const VocabularySetsPage = () => {
    const { user } = useAuth();
    const navigate = useNavigate();
    const [dailySets, setDailySets] = useState<VocabularySetDto[]>([]);
    const [advancedSets, setAdvancedSets] = useState<VocabularySetDto[]>([]);
    const [mySets, setMySets] = useState<VocabularySetDto[]>([]);
    const [searchTitle, setSearchTitle] = useState('');
    const [debouncedSearchTitle] = useDebounce(searchTitle, 500);
    const [loading, setLoading] = useState(true);
    const [isSearching, setIsSearching] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [showBackToTop, setShowBackToTop] = useState(false);

    useEffect(() => {
        const loadData = async () => {
            setIsSearching(true);
            try {
                // Fetch tất cả theme trong từng nhóm song song
                const [dailyResults, advancedResults, mySetsData] = await Promise.all([
                    Promise.all(DAILY_THEMES.map(theme =>
                        fetchVocabularySets(debouncedSearchTitle, theme, undefined, undefined)
                    )),
                    Promise.all(ADVANCED_THEMES.map(theme =>
                        fetchVocabularySets(debouncedSearchTitle, theme, undefined, undefined)
                    )),
                    user
                        ? fetchUserVocabularySets(debouncedSearchTitle, undefined, undefined, undefined, true)
                        : Promise.resolve([]),
                ]);

                setDailySets(dailyResults.flat());
                setAdvancedSets(advancedResults.flat());
                setMySets(mySetsData);
            } catch (err) {
                setError(err instanceof Error ? err.message : 'An unexpected error occurred');
            } finally {
                setIsSearching(false);
                setLoading(false);
            }
        };
        loadData();
    }, [debouncedSearchTitle, user]);

    useEffect(() => {
        const handleScroll = () => setShowBackToTop(window.scrollY > 300);
        window.addEventListener('scroll', handleScroll);
        return () => window.removeEventListener('scroll', handleScroll);
    }, []);

    const SearchBar = (
        <div className="mb-8 relative flex-grow">
            <input
                type="text"
                value={searchTitle}
                onChange={e => setSearchTitle(e.target.value)}
                placeholder="Tìm kiếm bộ từ vựng theo tiêu đề..."
                className="w-full p-2 sm:p-3 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {isSearching && <Spinner />}
        </div>
    );

    if (loading && !dailySets.length && !advancedSets.length && !mySets.length) {
        return (
            <div className="background-color pt-13 text-color min-h-screen overflow-auto">
                <div className="container mx-auto p-4 sm:p-6 lg:p-8 w-full sm:w-10/12 lg:w-7/12">
                    {SearchBar}
                    <Skeleton type="cards" />
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="background-color pt-13 text-white min-h-screen overflow-auto">
                <div className="container mx-auto p-4 sm:p-6 lg:p-8 w-full sm:w-10/12 lg:w-7/12 text-center py-8">
                    <p className="text-base sm:text-lg text-red-500">{error}</p>
                    <button onClick={() => window.location.reload()} className="mt-2 text-blue-500 hover:text-blue-400">
                        Retry
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="review-box-background bg-fixed pt-13 text-color min-h-screen overflow-auto">
            <div className="container mx-auto p-4 sm:p-6 lg:p-8 w-full sm:w-10/12 lg:w-7/12">
                {SearchBar}

                {/* My Vocabulary Sets */}
                {user && (
                    <section className="mb-12">
                        <h2 className="text-xl sm:text-2xl font-bold mb-4">Bộ từ vựng của tôi</h2>
                        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4 sm:gap-6">
                            <div
                                onClick={() => navigate('/vocabulary-sets/create')}
                                className="border border-gray-300 rounded-md p-4 bg-transparent flex items-center justify-center cursor-pointer hover:border-blue-500 hover:scale-105 transition-all duration-300"
                            >
                                <span className="text-3xl sm:text-4xl">+</span>
                            </div>
                            {mySets.length > 0
                                ? mySets.map((item) => (
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
                                ))
                                : <p className="text-sm sm:text-base text-gray-500 col-span-2">Bạn chưa sở hữu bộ từ vựng nào.</p>
                            }
                        </div>
                    </section>
                )}

                {/* Daily Learning — DailyLife · Nature · Weather · Food */}
                <section className="mb-12">
                    <h2 className="text-xl sm:text-2xl font-bold mb-4">🌱 Daily Learning</h2>
                    <p className="text-sm text-gray-400 mb-4">Cuộc sống · Thiên nhiên · Thời tiết · Ẩm thực</p>
                    {dailySets.length === 0
                        ? <p className="text-sm sm:text-base text-gray-500">Không tìm thấy bộ từ vựng.</p>
                        : <CardGrid items={dailySets} />
                    }
                </section>

                {/* Advanced Topics — Business · Science · Art · Mystery · Dark */}
                <section className="mb-12">
                    <h2 className="text-xl sm:text-2xl font-bold mb-4">🔬 Advanced Topics</h2>
                    <p className="text-sm text-gray-400 mb-4">Kinh doanh · Khoa học · Nghệ thuật · Bí ẩn · Tối tăm</p>
                    {advancedSets.length === 0
                        ? <p className="text-sm sm:text-base text-gray-500">Không tìm thấy bộ từ vựng.</p>
                        : <CardGrid items={advancedSets} />
                    }
                </section>
            </div>

            {/* Back to Top (mobile only) */}
            {showBackToTop && (
                <button
                    onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
                    className="fixed bottom-4 right-4 sm:hidden bg-blue-500 text-white p-3 rounded-full shadow-lg hover:bg-blue-600 transition-opacity duration-300 z-50"
                >
                    <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 10l7-7m0 0l7 7m-7-7v18" />
                    </svg>
                </button>
            )}
        </div>
    );
};

export default VocabularySetsPage;