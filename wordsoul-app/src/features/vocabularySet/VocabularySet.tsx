import { useEffect, useState } from 'react';
import { useDebounce } from 'use-debounce';
import { fetchGroupedVocabularySets, fetchUserVocabularySets, fetchVocabularySets } from '../../services/vocabularySet';
import Card from '../../components/Card';
import Skeleton from '../../components/Skeleton';
import { useNavigate } from 'react-router-dom';
import type { VocabularySetDto } from '../../types/VocabularySetDto';
import { useAuth } from '../../hooks/Auth/useAuth';
import WorldMap, { HOTSPOTS } from './WorldMap';

type ViewTab = 'list' | 'map';

// Theme tier groups for the list view
const THEME_TIERS = [
    { label: '🌱 Daily Learning', subtitle: 'Cuộc sống · Thiên nhiên · Thời tiết · Ẩm thực', themes: ['DailyLife', 'Nature', 'Weather', 'Food'] },
    { label: '⚡ Intermediate', subtitle: 'Công nghệ · Du lịch · Sức khỏe · Thể thao', themes: ['Technology', 'Travel', 'Health', 'Sports'] },
    { label: '🔥 Advanced & Specialized', subtitle: 'Kinh doanh · Khoa học · Nghệ thuật · Bí ẩn · ...', themes: ['Business', 'Science', 'Art', 'Mystery', 'Dark', 'Custom', 'Challenge', 'Poison'] },
] as const;

const Spinner = () => (
    <div className="absolute right-2 top-1/2 -translate-y-1/2">
        <svg className="animate-spin h-5 w-5 sm:h-6 sm:w-6 text-blue-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
        </svg>
    </div>
);

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

    const [activeTab, setActiveTab] = useState<ViewTab>('list');

    // --- List tab state ---
    const [tierSets, setTierSets] = useState<Record<string, VocabularySetDto[]>>({});
    const [mySets, setMySets] = useState<VocabularySetDto[]>([]);

    // --- Map tab state ---
    const [activeTheme, setActiveTheme] = useState<string | null>(null);
    const [themeSets, setThemeSets] = useState<VocabularySetDto[]>([]);
    const [themeLoading, setThemeLoading] = useState(false);

    const [searchTitle, setSearchTitle] = useState('');
    const [debouncedSearchTitle] = useDebounce(searchTitle, 500);
    const [loading, setLoading] = useState(true);
    const [isSearching, setIsSearching] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [showBackToTop, setShowBackToTop] = useState(false);

    // Load list-view data
    useEffect(() => {
        const loadListData = async () => {
            setIsSearching(true);
            try {
                // Sử dụng API gom nhóm (Grouped API) để giải quyết lỗi N+1
                const groupedData = await fetchGroupedVocabularySets(debouncedSearchTitle, 6);
                setTierSets(groupedData);

                // Fetch user specific sets if logged in (this request uses Authorization automatically)
                if (user) {
                    const mySetsData = await fetchUserVocabularySets(debouncedSearchTitle, undefined, undefined, undefined, true);
                    setMySets(mySetsData);
                } else {
                    setMySets([]);
                }
            } catch (err) {
                setError(err instanceof Error ? err.message : 'Lỗi tải dữ liệu');
            } finally {
                setIsSearching(false);
                setLoading(false);
            }
        };
        loadListData();
    }, [debouncedSearchTitle, user]);

    // Load map-view theme sets when activeTheme changes
    useEffect(() => {
        if (!activeTheme) { setThemeSets([]); return; }
        const load = async () => {
            setThemeLoading(true);
            try {
                const data = await fetchVocabularySets(debouncedSearchTitle, activeTheme, undefined, undefined);
                setThemeSets(data);
            } catch {
                // silently fail; user can retry by clicking again
            } finally {
                setThemeLoading(false);
            }
        };
        load();
    }, [activeTheme, debouncedSearchTitle]);

    useEffect(() => {
        const h = () => setShowBackToTop(window.scrollY > 300);
        window.addEventListener('scroll', h);
        return () => window.removeEventListener('scroll', h);
    }, []);

    const activeSpot = HOTSPOTS.find(h => h.theme === activeTheme);

    const getTierSets = (themes: readonly string[]) =>
        themes.flatMap(t => tierSets[t] ?? []);

    const SearchBar = (
        <div className="mb-6 relative flex-grow">
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

    if (loading) {
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
                    <button onClick={() => window.location.reload()} className="mt-2 text-blue-500 hover:text-blue-400">Retry</button>
                </div>
            </div>
        );
    }

    return (
        <div className="review-box-background bg-fixed pt-13 text-color min-h-screen overflow-auto">
            <div className="container mx-auto p-4 sm:p-6 lg:p-8 w-full sm:w-10/12 lg:w-7/12">
                {SearchBar}

                {/* Tab switcher */}
                <div className="flex gap-1 mb-6 border-b border-gray-600">
                    {(['list', 'map'] as ViewTab[]).map(tab => (
                        <button
                            key={tab}
                            onClick={() => setActiveTab(tab)}
                            className={`px-5 py-2 text-sm font-semibold rounded-t-lg transition-all duration-150 ${activeTab === tab
                                ? 'bg-blue-600 text-white border border-b-0 border-blue-600'
                                : 'text-gray-400 hover:text-white hover:bg-gray-700'
                                }`}
                        >
                            {tab === 'list' ? '📚 Danh sách' : '🗺️ Bản đồ'}
                        </button>
                    ))}
                </div>

                {/* ── LIST TAB ─────────────────────────────────── */}
                {activeTab === 'list' && (
                    <>
                        {/* My Sets */}
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
                                        : <p className="text-sm text-gray-500 col-span-2">Bạn chưa sở hữu bộ từ vựng nào.</p>
                                    }
                                </div>
                            </section>
                        )}

                        {/* Tier sections */}
                        {THEME_TIERS.map(tier => {
                            const sets = getTierSets(tier.themes);
                            return (
                                <section key={tier.label} className="mb-12">
                                    <h2 className="text-xl sm:text-2xl font-bold mb-1">{tier.label}</h2>
                                    <p className="text-sm text-gray-400 mb-4">{tier.subtitle}</p>
                                    {sets.length === 0
                                        ? <p className="text-sm text-gray-500">Không tìm thấy bộ từ vựng.</p>
                                        : <CardGrid items={sets} />
                                    }
                                </section>
                            );
                        })}
                    </>
                )}

                {/* ── MAP TAB ──────────────────────────────────── */}
                {activeTab === 'map' && (
                    <>
                        <WorldMap activeTheme={activeTheme} onSelect={setActiveTheme} />

                        <section className="mt-6">
                            {activeTheme && activeSpot ? (
                                <>
                                    <h2 className="text-xl sm:text-2xl font-bold mb-1 flex items-center gap-2">
                                        <span className="inline-block w-3 h-3 rounded-full flex-shrink-0" style={{ backgroundColor: activeSpot.color }} />
                                        {activeSpot.viLabel}
                                        <span className="text-sm font-normal text-gray-400">— {activeSpot.label}</span>
                                    </h2>
                                    <p className="text-xs text-gray-500 mb-4">Theme: {activeTheme}</p>
                                    {themeLoading
                                        ? <Skeleton type="cards" />
                                        : themeSets.length === 0
                                            ? <p className="text-sm text-gray-500">Chưa có bộ từ vựng nào ở khu vực này.</p>
                                            : <CardGrid items={themeSets} />
                                    }
                                </>
                            ) : (
                                <div className="text-center py-12 text-gray-500">
                                    <p className="text-5xl mb-3">🗺️</p>
                                    <p className="text-base">Chọn một khu vực trên bản đồ để xem bộ từ vựng</p>
                                </div>
                            )}
                        </section>
                    </>
                )}
            </div>

            {showBackToTop && (
                <button
                    onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
                    className="fixed bottom-4 right-4 sm:hidden bg-blue-500 text-white p-3 rounded-full shadow-lg hover:bg-blue-600 z-50"
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