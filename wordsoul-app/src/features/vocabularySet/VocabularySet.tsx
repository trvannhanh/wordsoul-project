import { useEffect, useRef, useState } from 'react';
import { useDebounce } from 'use-debounce';
import { deleteVocabularySet, fetchGroupedVocabularySets, fetchUserVocabularySets, fetchVocabularySets } from '../../services/vocabularySet';
import Card from '../../components/Card';
import Skeleton from '../../components/Skeleton';
import { useNavigate } from 'react-router-dom';
import type { VocabularySetDto } from '../../types/VocabularySetDto';
import { useAuth } from '../../hooks/Auth/useAuth';
import WorldMap, { HOTSPOTS } from './WorldMap';

type ViewTab = 'list' | 'map';

const Spinner = () => (
    <div className="absolute right-2 top-1/2 -translate-y-1/2">
        <svg className="animate-spin h-5 w-5 sm:h-6 sm:w-6 text-blue-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
        </svg>
    </div>
);

// Horizontal fade-in slider — flex layout, always-visible nav buttons
const CardSlider = ({ items, onDelete, prefixSlot, currentUserId }: {
    items: VocabularySetDto[];
    onDelete?: (id: number) => void;
    prefixSlot?: React.ReactNode;
    currentUserId?: number;
}) => {
    const trackRef = useRef<HTMLDivElement>(null);
    const [canPrev, setCanPrev] = useState(false);
    const [canNext, setCanNext] = useState(false);

    const CARD_W = 288; // card width

    const scroll = (dir: 1 | -1) => {
        trackRef.current?.scrollBy({ left: dir * CARD_W * 2, behavior: 'smooth' });
    };

    const checkScroll = () => {
        const el = trackRef.current;
        if (!el) return;
        setCanPrev(el.scrollLeft > 4);
        setCanNext(el.scrollLeft + el.clientWidth < el.scrollWidth - 4);
    };

    useEffect(() => {
        const el = trackRef.current;
        if (!el) return;
        // defer so layout is complete
        const t = setTimeout(checkScroll, 50);
        el.addEventListener('scroll', checkScroll, { passive: true });
        return () => { clearTimeout(t); el.removeEventListener('scroll', checkScroll); };
    }, [items]);

    const NavBtn = ({ dir }: { dir: 1 | -1 }) => {
        const active = dir === -1 ? canPrev : canNext;
        return (
            <button
                onClick={() => scroll(dir)}
                disabled={!active}
                className={`flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center border transition-all self-center
                    ${active
                        ? 'bg-gray-700 hover:bg-gray-600 border-gray-500 text-white cursor-pointer shadow'
                        : 'bg-gray-900 border-gray-700 text-gray-600 cursor-default'}`}
            >
                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                        d={dir === -1 ? 'M15 19l-7-7 7-7' : 'M9 5l7 7-7 7'} />
                </svg>
            </button>
        );
    };

    return (
        <div className="flex items-center gap-2">
            <NavBtn dir={-1} />
            {/* Track */}
            <div
                ref={trackRef}
                className="flex-1 flex gap-4 overflow-x-auto pb-2 items-stretch"
                style={{ scrollSnapType: 'x mandatory', scrollbarWidth: 'none', msOverflowStyle: 'none' }}
            >
                {prefixSlot}
                {items.map((item, idx) => (
                    <div
                        key={item.id}
                        className="flex-shrink-0 w-64 sm:w-72 self-stretch"
                        style={{
                            scrollSnapAlign: 'start',
                            animation: `fadeSlideIn 0.3s ease both`,
                            animationDelay: `${idx * 0.05}s`,
                        }}
                    >
                        <Card
                            title={item.title}
                            description={item.description || 'Không có mô tả'}
                            theme={item.theme}
                            difficultyLevel={item.difficultyLevel}
                            image={item.imageUrl || ''}
                            vocabularySetid={item.id}
                            isPublic={item.isPublic}
                            isOwned={item.isOwned}
                            createdByUsername={item.createdByUsername || 'Unknown'}
                            onDelete={onDelete && item.createdById != null && item.createdById === currentUserId ? () => onDelete(item.id) : undefined}
                        />
                    </div>
                ))}
            </div>
            <NavBtn dir={1} />
            <style>{`
                @keyframes fadeSlideIn {
                    from { opacity: 0; transform: translateX(20px); }
                    to   { opacity: 1; transform: translateX(0); }
                }
            `}</style>
        </div>
    );
};

// Label map for themes
const THEME_LABELS: Record<string, string> = {
    DailyLife: 'Đời sống hằng ngày', Nature: 'Thiên Nhiên, cây cỏ, động vật', Food: 'Ẩm thực & Nấu ăn',
    Weather: 'Thời tiết', Technology: 'Công nghệ, điện tử', Travel: 'Địa lý, du lịch',
    Health: 'Sức khỏe, y tế', Sports: 'Thể thao', Business: 'Kinh doanh, công nghiệp, tài chính',
    Science: 'Khoa học', Art: 'Nghệ thuật', Communication: 'Giao tiếp, mạng lưới xã hội',
    Mystery: 'Bí ẩn, tâm linh, truyền thuyết', Dark: 'Góc tối xã hội', Academic: 'Học thuật, chuyên ngành',
    Challenge: 'Luyện thi, từ vựng khó', TrapWords: 'Từ dễ gây nhầm lẫn', System: 'Hệ thống vĩ mô, chính trị, luật lệ cổ đại',
    Custom: 'Khác',
};

const VocabularySetsPage = () => {
    const { user } = useAuth();
    const navigate = useNavigate();

    const [activeTab, setActiveTab] = useState<ViewTab>('list');

    // --- List tab state ---
    const [tierSets, setTierSets] = useState<Record<string, VocabularySetDto[]>>({});
    const [mySets, setMySets] = useState<VocabularySetDto[]>([]);
    const [refreshKey, setRefreshKey] = useState(0);
    const [mySetFilter, setMySetFilter] = useState<'all' | 'owned'>('all');

    const handleDeleteSet = async (id: number) => {
        try {
            await deleteVocabularySet(id);
            setMySets(prev => prev.filter(s => s.id !== id));
            setTierSets(prev => {
                const updated: Record<string, VocabularySetDto[]> = {};
                for (const key of Object.keys(prev)) updated[key] = prev[key].filter(s => s.id !== id);
                return updated;
            });
            // Signal other tabs/windows to refresh
            localStorage.setItem('vocabSetListDirty', Date.now().toString());
        } catch { /* silently fail */ }
    };

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
    }, [debouncedSearchTitle, user, refreshKey]);

    // Listen for external "dirty" signal (after create / delete from other pages)
    useEffect(() => {
        const onStorage = (e: StorageEvent) => {
            if (e.key === 'vocabSetListDirty') setRefreshKey(k => k + 1);
        };
        window.addEventListener('storage', onStorage);
        return () => window.removeEventListener('storage', onStorage);
    }, []);

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
                            {tab === 'list' ? 'Danh sách' : 'Bản đồ'}
                        </button>
                    ))}
                </div>

                {/* ── LIST TAB ─────────────────────────────────── */}
                {activeTab === 'list' && (
                    <>
                        {/* My Sets */}
                        {user && (
                            <section className="mb-10">
                                <div className="flex items-center gap-3 mb-4 flex-wrap">
                                    <h2 className="pixel-text-outline font-pixel text-xl sm:text-2xl font-bold">Bộ từ vựng của tôi</h2>
                                    <div className="flex rounded-lg font-pixel overflow-hidden border border-gray-600 text-xs">
                                        <button
                                            onClick={() => setMySetFilter('all')}
                                            className={`px-3 py-1 transition-colors ${mySetFilter === 'all' ? 'bg-blue-600 text-white' : 'bg-gray-800 text-gray-400 hover:bg-gray-700'}`}
                                        >Tất cả</button>
                                        <button
                                            onClick={() => setMySetFilter('owned')}
                                            className={`px-3 py-1 transition-colors ${mySetFilter === 'owned' ? 'bg-blue-600 text-white' : 'bg-gray-800 text-gray-400 hover:bg-gray-700'}`}
                                        >Do tôi tạo</button>
                                    </div>
                                </div>
                                {(() => {
                                    const displayedSets = mySetFilter === 'owned'
                                        ? mySets.filter(s => s.createdById != null && s.createdById === user.id)
                                        : mySets;
                                    return displayedSets.length === 0 ? (
                                        <div
                                            onClick={() => navigate('/vocabulary-sets/create')}
                                            className="inline-flex items-center gap-2 border border-dashed border-gray-500 rounded-xl px-6 py-4 cursor-pointer hover:border-blue-500 hover:text-blue-400 transition-all"
                                        >
                                            <span className="text-2xl">+</span>
                                            <span className="text-sm text-gray-400">Tạo bộ từ vựng mới</span>
                                        </div>
                                    ) : (
                                        <CardSlider
                                            items={displayedSets}
                                            currentUserId={user.id}
                                            onDelete={(id) => handleDeleteSet(id)}
                                            prefixSlot={
                                                <div
                                                    onClick={() => navigate('/vocabulary-sets/create')}
                                                    className="flex-shrink-0 w-64 sm:w-72 border border-dashed border-gray-500 rounded-xl flex items-center justify-center gap-2 cursor-pointer hover:border-blue-500 hover:text-blue-400 transition-all min-h-[160px]"
                                                    style={{ scrollSnapAlign: 'start' }}
                                                >
                                                    <span className="text-2xl">+</span>
                                                </div>
                                            }
                                        />
                                    );
                                })()}
                            </section>
                        )}

                        {/* Per-theme sections */}
                        {Object.keys(tierSets).map(theme => {
                            const sets = tierSets[theme] ?? [];
                            if (sets.length === 0) return null;
                            return (
                                <section key={theme} className="mb-10">
                                    <h2 className="pixel-text-outline font-pixel text-lg sm:text-xl font-bold mb-3">
                                        {THEME_LABELS[theme] ?? theme}
                                    </h2>
                                    <CardSlider
                                        items={sets}
                                        currentUserId={user?.id}
                                        onDelete={user ? (id) => handleDeleteSet(id) : undefined}
                                    />
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
                                            : <CardSlider items={themeSets} />
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