/* eslint-disable @typescript-eslint/no-explicit-any */
import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import VocabSetHeroSection from "../../components/VocabSetHeroSection";
import { fetchVocabularySetDetail, updateVocabularySet, publishVocabularySet, fetchMySetProgress, unregisterVocabularySet, updateVocabCore } from "../../services/vocabularySet";
import { createLearningSession } from "../../services/learningSession";
import ProfileCard from "../../components/UserProfile/ProfileCard";
import VocabularyList from "../../components/Vocabulary/VocabularyList";
import { getUserVocabularySets, registerVocabularySet } from "../../services/user";
import { fetchPets } from "../../services/pet";
import PetCard from "../../components/Pet/PetCard";
import type { UserVocabularySetDto } from "../../types/UserDto";
import type { PetDto } from "../../types/PetDto";
import type { VocabularySetProgressDto, VocabularyDetailDto, UpdateVocabularyCoreDto } from "../../types/VocabularySetDto";
import { useAuth } from "../../hooks/Auth/useAuth";

interface VocabularySetMeta {
  id: number;
  title: string;
  description: string | null;
  theme: string;
  difficultyLevel: string;
  imageUrl?: string;
  isPublic: boolean;
  createdById?: number;
}

const VocabularySetDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [meta, setMeta] = useState<VocabularySetMeta | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const [userSetInfo, setUserSetInfo] = useState<UserVocabularySetDto | null>(null);
  const [showBackToTop, setShowBackToTop] = useState<boolean>(false);
  const [pets, setPets] = useState<PetDto[]>([]);
  const [showPetsModal, setShowPetsModal] = useState<boolean>(false);
  const [petsLoading, setPetsLoading] = useState<boolean>(false);
  const [petsError, setPetsError] = useState<string | null>(null);
  const navigate = useNavigate();
  const { user } = useAuth();

  // ── Unregister ─────────────────────────────────────────────────────────────
  const [unregistering, setUnregistering] = useState(false);

  // ── Edit set modal ─────────────────────────────────────────────────────────
  const [showEditModal, setShowEditModal] = useState(false);
  const [editTitle, setEditTitle] = useState('');
  const [editDescription, setEditDescription] = useState('');
  const [editSaving, setEditSaving] = useState(false);
  const [editError, setEditError] = useState('');

  // ── Publish ────────────────────────────────────────────────────────────────
  const [publishing, setPublishing] = useState(false);

  // ── Vocab detail popup ─────────────────────────────────────────────────────
  const [detailVocab, setDetailVocab] = useState<VocabularyDetailDto | null>(null);
  const [isPlayingAudio, setIsPlayingAudio] = useState(false);
  const [isPlayingExample, setIsPlayingExample] = useState(false);

  // ── Edit core vocab (inside detail popup) ─────────────────────────────────
  const [isEditingDetail, setIsEditingDetail] = useState(false);
  const [editCoreWord, setEditCoreWord] = useState('');
  const [editCoreMeaning, setEditCoreMeaning] = useState('');
  const [editCorePronunciation, setEditCorePronunciation] = useState('');
  const [editCoreExample, setEditCoreExample] = useState('');
  const [editCoreDesc, setEditCoreDesc] = useState('');
  const [editCoreSaving, setEditCoreSaving] = useState(false);
  const [editCoreError, setEditCoreError] = useState('');
  const [vocabListKey, setVocabListKey] = useState(0);

  // ── Progress ───────────────────────────────────────────────────────────────
  const [progress, setProgress] = useState<VocabularySetProgressDto | null>(null);
  const [showProgress, setShowProgress] = useState(true);

  const isOwner = !!user && !!meta?.createdById && user.id === meta.createdById;

  const handleCreateLearningSession = async () => {
    setError(null);
    setIsLoading(true);
    try {
      const session = await createLearningSession(Number(id));
      navigate(`/learningSession/${session.id}?mode=learning`, {
        state: {
          petId: session.petId,
          catchRate: session.catchRate,
          currentCorrectAnswered: session.currentCorrectAnswered,
          buffPetId: session.buffPetId,
          buffName: session.buffName,
          buffDescription: session.buffDescription,
          buffIcon: session.buffIcon,
          petXpMultiplier: session.petXpMultiplier,
          petCatchBonus: session.petCatchBonus,
          petHintShield: session.petHintShield,
          petReducePenalty: session.petReducePenalty,
        },
      });
    } catch (err: any) {
      setError(err?.response?.data?.message || "Lỗi tạo phiên học");
    } finally {
      setIsLoading(false);
    }
  };

  const handleRegisterSet = async () => {
    setError(null);
    setIsLoading(true);
    try {
      const userVocabSet = await registerVocabularySet(Number(id));
      if (userVocabSet) setUserSetInfo(userVocabSet);
    } catch (err: any) {
      setError(err?.message || "Lỗi đăng ký bộ từ vựng");
    } finally {
      setIsLoading(false);
    }
  };

  // Only open pets modal when logged in
  const handleShowPets = async () => {
    if (!user) return;
    setPetsLoading(true);
    setPetsError(null);
    setShowPetsModal(true);
    try {
      const filters = { vocabularySetId: Number(id), pageSize: 20 };
      const data = await fetchPets(filters);
      setPets(data);
    } catch {
      setPetsError("Lỗi tải danh sách pet");
    } finally {
      setPetsLoading(false);
    }
  };

  // ── Unregister handler ────────────────────────────────────────────────────
  const handleUnregister = async () => {
    if (!window.confirm('Bạn có chắc muốn hủy đăng ký bộ từ vựng này?')) return;
    setUnregistering(true);
    try {
      await unregisterVocabularySet(Number(id));
      setUserSetInfo(null);
      setProgress(null);
    } catch {
      // ignore
    } finally {
      setUnregistering(false);
    }
  };

  // ── Edit set handlers ──────────────────────────────────────────────────────
  const openEditModal = () => {
    if (!meta) return;
    setEditTitle(meta.title);
    setEditDescription(meta.description || '');
    setEditError('');
    setShowEditModal(true);
  };

  const handleSaveEdit = async () => {
    if (!meta) return;
    if (!editTitle.trim()) { setEditError('Tiêu đề không được để trống'); return; }
    if (editDescription.length > 300) { setEditError('Mô tả tối đa 300 ký tự'); return; }
    setEditError('');
    setEditSaving(true);
    try {
      await updateVocabularySet(meta.id, {
        ...meta,
        title: editTitle.trim(),
        description: editDescription || null,
        isActive: true,
        vocabularyIds: [],
      } as any);
      setMeta(prev => prev ? { ...prev, title: editTitle.trim(), description: editDescription || null } : prev);
      setShowEditModal(false);
    } catch {
      // ignore — keep modal open
    } finally {
      setEditSaving(false);
    }
  };

  // ── Publish handler ────────────────────────────────────────────────────────
  const handlePublish = async () => {
    if (!meta) return;
    setPublishing(true);
    try {
      await publishVocabularySet(meta.id);
      setMeta(prev => prev ? { ...prev, isPublic: true } : prev);
    } catch {
      // ignore
    } finally {
      setPublishing(false);
    }
  };

  // ── Vocab detail popup ─────────────────────────────────────────────────────
  const [editCoreImageFile, setEditCoreImageFile] = useState<File | null>(null);
  const [editCoreImagePreview, setEditCoreImagePreview] = useState<string | null>(null);

  const openDetailEdit = (vocab: VocabularyDetailDto) => {
    setEditCoreWord(vocab.word ?? '');
    setEditCoreMeaning(vocab.meaning ?? '');
    setEditCorePronunciation(vocab.pronunciation ?? '');
    setEditCoreExample(vocab.exampleSentence ?? '');
    setEditCoreDesc(vocab.description ?? '');
    setEditCoreImageFile(null);
    setEditCoreImagePreview(null);
    setEditCoreError('');
    setIsEditingDetail(true);
  };

  const handleSaveCoreEdit = async () => {
    if (!detailVocab || !meta) return;
    if (!editCoreWord.trim()) { setEditCoreError('Từ vựng không được để trống.'); return; }
    setEditCoreSaving(true);
    setEditCoreError('');
    try {
      const dto: UpdateVocabularyCoreDto = {
        word: editCoreWord.trim(),
        meaning: editCoreMeaning,
        pronunciation: editCorePronunciation,
        exampleSentence: editCoreExample,
        description: editCoreDesc,
      };
      const updated = await updateVocabCore(meta.id, detailVocab.id, dto, editCoreImageFile);
      // Merge updated fields back into detailVocab for immediate display
      setDetailVocab(prev => prev ? {
        ...prev,
        word: (updated as unknown as Record<string, string>).word ?? prev.word,
        meaning: (updated as unknown as Record<string, string>).meaning ?? prev.meaning,
        pronunciation: (updated as unknown as Record<string, string>).pronunciation ?? prev.pronunciation,
        exampleSentence: (updated as unknown as Record<string, string>).exampleSentence ?? prev.exampleSentence,
        description: (updated as unknown as Record<string, string>).description ?? prev.description,
        imageUrl: (updated as unknown as Record<string, string>).imageUrl ?? prev.imageUrl,
      } : prev);
      setEditCoreImageFile(null);
      setEditCoreImagePreview(null);
      setIsEditingDetail(false);
      setVocabListKey(k => k + 1); // force VocabularyList remount + re-fetch
    } catch (err: unknown) {
      const e = err as Record<string, unknown>;
      const data = (e?.response as Record<string, unknown> | undefined)?.data;
      setEditCoreError((typeof data === 'string' ? data : (data as Record<string, unknown> | undefined)?.message as string) || (e?.message as string) || 'Lỗi lưu thay đổi');
    } finally {
      setEditCoreSaving(false);
    }
  };

  const handlePlayAudio = (vocab: VocabularyDetailDto) => {
    if (vocab.pronunciationUrl) {
      setIsPlayingAudio(true);
      const audio = new Audio(vocab.pronunciationUrl);
      audio.onended = () => setIsPlayingAudio(false);
      audio.onerror = () => {
        // fallback to speechSynthesis
        const utterance = new SpeechSynthesisUtterance(vocab.word ?? '');
        utterance.lang = 'en-US';
        utterance.onend = () => setIsPlayingAudio(false);
        window.speechSynthesis.speak(utterance);
      };
      audio.play().catch(() => {
        const utterance = new SpeechSynthesisUtterance(vocab.word ?? '');
        utterance.lang = 'en-US';
        utterance.onend = () => setIsPlayingAudio(false);
        window.speechSynthesis.speak(utterance);
      });
    } else {
      setIsPlayingAudio(true);
      const utterance = new SpeechSynthesisUtterance(vocab.word ?? '');
      utterance.lang = 'en-US';
      utterance.onend = () => setIsPlayingAudio(false);
      window.speechSynthesis.speak(utterance);
    }
  };

  useEffect(() => {
    const fetchMeta = async () => {
      try {
        const data = await fetchVocabularySetDetail(Number(id), 1, 1);
        setMeta({
          id: data.id,
          title: data.title,
          description: data.description,
          theme: data.theme,
          difficultyLevel: data.difficultyLevel,
          imageUrl: data.imageUrl,
          isPublic: data.isPublic,
          createdById: data.createdById,
        });
      } catch {
        setError("Failed to load set metadata");
      } finally {
        setLoading(false);
      }
    };
    if (id) fetchMeta();
  }, [id]);

  useEffect(() => {
    const fetchUserSetInfo = async () => {
      const userSets = await getUserVocabularySets(Number(id));
      setUserSetInfo(userSets);
    };
    if (id && user) fetchUserSetInfo();
  }, [user, id]);

  useEffect(() => {
    const fetchProgress = async () => {
      try {
        const data = await fetchMySetProgress(Number(id));
        setProgress(data);
      } catch { /* not registered or error — ignore */ }
    };
    if (id && user && userSetInfo) fetchProgress();
  }, [id, user, userSetInfo]);

  useEffect(() => {
    const handleScroll = () => setShowBackToTop(window.scrollY > 300);
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  const scrollToTop = () => window.scrollTo({ top: 0, behavior: 'smooth' });

  if (loading) return <div className="text-center py-8">Loading set info...</div>;
  if (error) return <div className="text-center py-8 text-red-500">{error}</div>;
  if (!meta) return <div className="text-center py-8">No data available</div>;

  const masteredPct = progress ? Math.round((progress.masteredCount / Math.max(progress.totalVocabularies, 1)) * 100) : 0;

  return (
    <>
      <VocabSetHeroSection
        title={meta.title}
        description={meta.description}
        isPublic={meta.isPublic}
        isLoggedIn={!!user}
        userSetInfo={userSetInfo}
        isLoading={isLoading}
        onLearnOrRegister={userSetInfo ? handleCreateLearningSession : handleRegisterSet}
        onNavigateToLogin={() => navigate('/login')}
        petsLoading={petsLoading}
        onShowPets={handleShowPets}
        isOwner={isOwner}
        unregistering={unregistering}
        onUnregister={handleUnregister}
        publishing={publishing}
        onPublish={handlePublish}
        onEdit={openEditModal}
        progress={progress}
        showProgress={showProgress}
        onToggleProgress={() => setShowProgress(p => !p)}
      />

      <div className="pixel2-background text-white min-h-screen w-full flex justify-center items-start px-4 sm:px-6 lg:px-8 py-6 sm:py-10 overflow-auto">
        <div className="w-full sm:w-10/12 lg:w-8/12 flex flex-col gap-5">

          {/* ── Progress section (full width, above vocab list) ─────────────── */}
          {user && userSetInfo && progress && showProgress && (
            <div className="flex flex-col gap-4 text-color border border-indigo-800/50 rounded-xl p-4 bg-gray-900/60">
              <div className="flex items-center justify-between">
                <span className="text-base font-bold">Tiến trình học</span>
                <span className="text-xs text-gray-400">{progress.totalVocabularies} từ vựng</span>
              </div>
              {/* Top row: donut + breakdown bars */}
              <div className="flex flex-col sm:flex-row gap-5 items-center sm:items-start">
                {/* Donut chart */}
                {(() => {
                  const total = progress.totalVocabularies;
                  const r = 40; const cx = 50; const cy = 50;
                  const circ = 2 * Math.PI * r;
                  const segs = [
                    { v: progress.masteredCount, color: '#22c55e' },
                    { v: progress.reviewCount,   color: '#eab308' },
                    { v: progress.learningCount, color: '#3b82f6' },
                    { v: progress.newCount,      color: '#4b5563' },
                  ];
                  let cum = 0;
                  const arcs = segs.map(s => {
                    const arc = total > 0 ? (s.v / total) * circ : 0;
                    const offset = -cum;
                    cum += arc;
                    return { ...s, arc, offset };
                  });
                  return (
                    <svg viewBox="0 0 100 100" className="w-28 h-28 flex-shrink-0">
                      {total === 0 && <circle cx={cx} cy={cy} r={r} fill="none" stroke="#374151" strokeWidth="16" />}
                      {arcs.map((a, i) => a.arc > 0 && (
                        <circle key={i} cx={cx} cy={cy} r={r} fill="none" stroke={a.color}
                          strokeWidth="16" strokeDasharray={`${a.arc} ${circ}`}
                          strokeDashoffset={a.offset} transform="rotate(-90 50 50)" />
                      ))}
                      <text x="50" y="47" textAnchor="middle" fill="white" fontSize="14" fontWeight="bold">{masteredPct}%</text>
                      <text x="50" y="60" textAnchor="middle" fill="#9ca3af" fontSize="8">thành thạo</text>
                    </svg>
                  );
                })()}
                {/* Breakdown bars */}
                <div className="flex-1 flex flex-col gap-2 w-full">
                  {[
                    { label: 'Đã thuộc',  count: progress.masteredCount, color: 'bg-green-500',  text: 'text-green-400' },
                    { label: 'Cần ôn',    count: progress.reviewCount,   color: 'bg-yellow-500', text: 'text-yellow-400' },
                    { label: 'Đang học',  count: progress.learningCount, color: 'bg-blue-500',   text: 'text-blue-400' },
                    { label: 'Chưa học',  count: progress.newCount,      color: 'bg-gray-600',   text: 'text-gray-400' },
                  ].map(s => (
                    <div key={s.label} className="flex items-center gap-2 text-xs">
                      <span className={`w-16 ${s.text} font-semibold`}>{s.label}</span>
                      <div className="flex-1 bg-gray-700 rounded-full h-2">
                        <div className={`${s.color} h-2 rounded-full transition-all`}
                          style={{ width: progress.totalVocabularies > 0 ? `${(s.count / progress.totalVocabularies) * 100}%` : '0%' }} />
                      </div>
                      <span className="w-6 text-right text-gray-300">{s.count}</span>
                    </div>
                  ))}
                  {/* Stats row */}
                  <div className="flex gap-3 mt-1 flex-wrap">
                    <span className="text-[11px] text-blue-300">Độ chính xác {progress.correctRate.toFixed(1)}%</span>
                    <span className="text-[11px] text-purple-300">Mức độ ghi nhớ {progress.overallRetentionScore.toFixed(1)}</span>
                    {/* <span className="text-[11px] text-yellow-300">{progress.totalCompletedSession} phiên học</span> */}
                  </div>
                </div>
              </div>
              {/* Heatmap + weak words */}
              <div className="flex flex-col sm:flex-row gap-4">
                {progress.activityHeatmap.length > 0 && (
                  <div className="flex-1">
                    <div className="text-xs text-gray-400 mb-1.5">Hoạt động 30 ngày</div>
                    <div className="flex flex-wrap gap-0.5">
                      {progress.activityHeatmap.map((day) => {
                        const intensity = Math.min(day.reviewCount / 10, 1);
                        const alpha = 0.2 + intensity * 0.8;
                        return (
                          <div key={day.date} className="w-3 h-3 rounded-sm"
                            style={{ backgroundColor: `rgba(74,222,128,${alpha})` }}
                            title={`${day.date}: ${day.reviewCount}`} />
                        );
                      })}
                    </div>
                  </div>
                )}
                {progress.weakVocabularies.length > 0 && (
                  <div className="min-w-[150px]">
                    <div className="text-xs text-gray-400 mb-1.5">Top từ cần ôn</div>
                    {progress.weakVocabularies.map(w => (
                      <div key={w.id} className="text-xs flex justify-between border-b border-gray-800 py-0.5 gap-2">
                        <span className="text-red-300 truncate">{w.word}</span>
                        <span className="text-gray-400 flex-shrink-0">{Number(w.retentionScore).toFixed(0)}%</span>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          )}

          {/* ── Main content: vocab list + sidebar ─────────────────────────── */}
          <div className="flex flex-col sm:flex-row items-start gap-4 sm:gap-5">
            {/* Left — vocabulary list */}
            <div className="w-full sm:w-8/12 min-w-0">
              <VocabularyList
                key={vocabListKey}
                setId={meta.id}
                pageSize={5}
                isOwner={isOwner}
                vocabMemoryStates={progress?.vocabMemoryStates}
                onClickVocab={(v) => setDetailVocab(v as unknown as VocabularyDetailDto)}
              />
            </div>

            {/* Right — sidebar */}
            <div className="w-full sm:w-4/12 flex flex-col gap-3">
              <div className="flex flex-col gap-3 items-center rounded-lg">
                <div className="w-full"><ProfileCard /></div>
              </div>
            </div>
          </div>
        </div>

        {/* ── Pets modal ─────────────────────────────────────────────────────── */}
        {showPetsModal && (
          <div className="fixed inset-0 bg-black/60 flex justify-center items-center z-50 font-pixel">
            <div className="background-color rounded-lg p-6 w-full max-w-[80vw] max-h-[80vh] overflow-auto">
              <div className="flex justify-between items-center mb-4">
                <h2 className="text-xl font-bold text-color">Danh sách Pet</h2>
                <button className="text-color hover:text-gray-400" onClick={() => setShowPetsModal(false)}>
                  <svg className="w-6 h-6 custom-cursor" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
              {petsError && <div className="text-red-500 mb-4">{petsError}</div>}
              {pets.length === 0 && !petsLoading && !petsError && <div className="text-center text-gray-400">Không có pet nào.</div>}
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-8 gap-4">
                {pets.map(pet => <PetCard key={pet.id} pet={pet} />)}
              </div>
              {petsLoading && (
                <div className="text-center py-4">
                  <svg className="animate-spin h-8 w-8 text-blue-500 mx-auto" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                  </svg>
                </div>
              )}
            </div>
          </div>
        )}

        {/* ── Edit set modal ──────────────────────────────────────────────────── */}
        {showEditModal && (
          <div className="fixed inset-0 bg-black/60 flex justify-center items-center z-50">
            <div className="background-color rounded-lg p-6 w-full max-w-md border border-gray-700">
              <h2 className="text-lg font-bold text-color mb-4">Chỉnh sửa bộ từ vựng</h2>
              <div className="flex flex-col gap-3">
                <div>
                  <label className="text-xs text-gray-400 mb-1 block">Tiêu đề</label>
                  <input
                    value={editTitle}
                    onChange={e => { setEditTitle(e.target.value); if (e.target.value.trim()) setEditError(''); }}
                    className={`w-full bg-gray-900 border rounded px-3 py-2 text-white text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 ${editError && !editTitle.trim() ? 'border-red-500' : 'border-gray-700'}`}
                  />
                  {editError && <p className="text-xs text-red-400 mt-1">{editError}</p>}
                </div>
                <div>
                  <label className="text-xs text-gray-400 mb-1 block">Mô tả <span className="text-gray-600">({editDescription.length}/300)</span></label>
                  <textarea
                    value={editDescription}
                    onChange={e => setEditDescription(e.target.value)}
                    maxLength={300}
                    rows={3}
                    className="w-full bg-gray-900 border border-gray-700 rounded px-3 py-2 text-white text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>
              <div className="flex justify-end gap-3 mt-5">
                <button onClick={() => setShowEditModal(false)} className="px-4 py-2 bg-gray-700 hover:bg-gray-600 rounded text-sm">Huỷ</button>
                <button onClick={handleSaveEdit} disabled={editSaving} className="px-4 py-2 bg-blue-600 hover:bg-blue-500 disabled:opacity-50 rounded text-sm font-bold">
                  {editSaving ? 'Đang lưu...' : 'Lưu'}
                </button>
              </div>
            </div>
          </div>
        )}

        {/* ── Vocab detail popup ──────────────────────────────────────────────── */}
        {detailVocab && (
          <div className="fixed inset-0 bg-black/60 flex justify-center items-center z-50" onClick={() => { setDetailVocab(null); setIsEditingDetail(false); }}>
            <div className="background-color rounded-xl p-6 w-full max-w-md border border-gray-700 shadow-xl" onClick={e => e.stopPropagation()}>

              {/* ── VIEW MODE ─────────────────────────────────────────────── */}
              {!isEditingDetail ? (
                <>
                  {/* Header row: word + pronunciation icon + edit button + close */}
                  <div className="flex items-start justify-between mb-1">
                    <div className="flex items-center gap-2 flex-wrap">
                      <h2 className="text-2xl font-bold text-color">{detailVocab.word}</h2>
                      <button onClick={() => handlePlayAudio(detailVocab)} disabled={isPlayingAudio}
                        className="text-blue-400 hover:text-blue-300 disabled:opacity-50 transition-colors" title="Phát âm từ">
                        {isPlayingAudio
                          ? <svg className="w-5 h-5 animate-pulse" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M9.383 3.076A1 1 0 0110 4v12a1 1 0 01-1.617.784L4.586 13H2a1 1 0 01-1-1V8a1 1 0 011-1h2.586l3.797-3.784a1 1 0 011 .076zM12.293 7.293a1 1 0 011.414 0A5.003 5.003 0 0115 10a5.003 5.003 0 01-1.293 2.707 1 1 0 01-1.414-1.414A3.003 3.003 0 0013 10a3.003 3.003 0 00-.707-1.293 1 1 0 010-1.414z" clipRule="evenodd" /></svg>
                          : <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M9.383 3.076A1 1 0 0110 4v12a1 1 0 01-1.617.784L4.586 13H2a1 1 0 01-1-1V8a1 1 0 011-1h2.586l3.797-3.784a1 1 0 011 .076z" clipRule="evenodd" /></svg>}
                      </button>
                    </div>
                    <div className="flex items-center gap-2 ml-4 flex-shrink-0">
                      {/* Edit button — only for owner */}
                      {isOwner && (
                        <button onClick={() => openDetailEdit(detailVocab)}
                          className="text-yellow-400 hover:text-yellow-300 transition-colors" title="Chỉnh sửa từ vựng">
                          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536M9 13l6.293-6.293a1 1 0 011.414 0l1.586 1.586a1 1 0 010 1.414L12 16H9v-3z"/>
                          </svg>
                        </button>
                      )}
                      <button onClick={() => { setDetailVocab(null); setIsEditingDetail(false); }}
                        className="text-gray-500 hover:text-gray-300">
                        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                      </button>
                    </div>
                  </div>

                  <span className="text-xs text-gray-400 italic mb-3 block">{detailVocab.partOfSpeech}</span>
                  {detailVocab.pronunciation && (
                    <p className="text-blue-300 font-mono text-sm mb-3">{detailVocab.pronunciation}</p>
                  )}
                  <div className="text-lg text-white mb-2">
                    {detailVocab.meaning}
                    {detailVocab.isCustomEdited && (
                      <span className="ml-2 text-xs bg-yellow-600/30 text-yellow-300 border border-yellow-600 px-1.5 py-0.5 rounded-full">Override</span>
                    )}
                  </div>
                  {detailVocab.originalMeaning && (
                    <p className="text-xs text-gray-500 line-through mb-2">{detailVocab.originalMeaning}</p>
                  )}
                  {detailVocab.exampleSentence && (
                    <div className="flex items-start gap-2 border-l-2 border-blue-500 pl-3 mb-3">
                      <p className="text-sm italic text-gray-300 flex-1">"{detailVocab.exampleSentence}"</p>
                      <button
                        onClick={() => {
                          if (!detailVocab.exampleSentence) return;
                          setIsPlayingExample(true);
                          if (detailVocab.exampleSentenceAudioUrl) {
                            const audio = new Audio(detailVocab.exampleSentenceAudioUrl);
                            audio.onended = () => setIsPlayingExample(false);
                            audio.onerror = () => {
                              const utt = new SpeechSynthesisUtterance(detailVocab.exampleSentence!);
                              utt.lang = 'en-US'; utt.onend = () => setIsPlayingExample(false);
                              window.speechSynthesis.speak(utt);
                            };
                            audio.play().catch(() => {
                              const utt = new SpeechSynthesisUtterance(detailVocab.exampleSentence!);
                              utt.lang = 'en-US'; utt.onend = () => setIsPlayingExample(false);
                              window.speechSynthesis.speak(utt);
                            });
                          } else {
                            const utterance = new SpeechSynthesisUtterance(detailVocab.exampleSentence);
                            utterance.lang = 'en-US'; utterance.onend = () => setIsPlayingExample(false);
                            window.speechSynthesis.speak(utterance);
                          }
                        }}
                        disabled={isPlayingExample}
                        className="text-teal-400 hover:text-teal-300 disabled:opacity-50 flex-shrink-0 mt-0.5 transition-colors" title="Phát câu ví dụ">
                        {isPlayingExample
                          ? <svg className="w-4 h-4 animate-pulse" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M9.383 3.076A1 1 0 0110 4v12a1 1 0 01-1.617.784L4.586 13H2a1 1 0 01-1-1V8a1 1 0 011-1h2.586l3.797-3.784a1 1 0 011 .076zM12.293 7.293a1 1 0 011.414 0A5.003 5.003 0 0115 10a5.003 5.003 0 01-1.293 2.707 1 1 0 01-1.414-1.414A3.003 3.003 0 0013 10a3.003 3.003 0 00-.707-1.293 1 1 0 010-1.414z" clipRule="evenodd" /></svg>
                          : <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M9.383 3.076A1 1 0 0110 4v12a1 1 0 01-1.617.784L4.586 13H2a1 1 0 01-1-1V8a1 1 0 011-1h2.586l3.797-3.784a1 1 0 011 .076z" clipRule="evenodd" /></svg>}
                      </button>
                    </div>
                  )}
                  {detailVocab.description && (
                    <p className="text-xs text-gray-400 mb-3">{detailVocab.description}</p>
                  )}
                  {detailVocab.imageUrl && (
                    <img src={detailVocab.imageUrl} alt={detailVocab.word ?? ''} className="w-full max-h-40 object-contain rounded-lg mt-2" />
                  )}
                  <button onClick={() => { setDetailVocab(null); setIsEditingDetail(false); }} className="mt-4 w-full py-2 bg-gray-700 hover:bg-gray-600 rounded text-sm">Đóng</button>
                </>
              ) : (
                /* ── EDIT MODE ──────────────────────────────────────────────── */
                <>
                  <div className="flex items-center justify-between mb-3">
                    <h3 className="text-base font-bold text-yellow-300">Chỉnh sửa từ vựng</h3>
                    <button onClick={() => setIsEditingDetail(false)} className="text-gray-500 hover:text-gray-300">
                      <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  </div>

                  <div className="flex flex-col gap-2">
                    {[
                      { label: 'Từ vựng', value: editCoreWord, set: setEditCoreWord, required: true },
                      { label: 'Nghĩa', value: editCoreMeaning, set: setEditCoreMeaning },
                      { label: 'Phát âm', value: editCorePronunciation, set: setEditCorePronunciation },
                      { label: 'Câu ví dụ', value: editCoreExample, set: setEditCoreExample },
                      { label: 'Mô tả', value: editCoreDesc, set: setEditCoreDesc },
                    ].map(f => (
                      <div key={f.label}>
                        <label className="text-[10px] text-gray-400 mb-0.5 block">
                          {f.label}{f.required && <span className="text-red-400 ml-0.5">*</span>}
                        </label>
                        <input value={f.value} onChange={e => { f.set(e.target.value); setEditCoreError(''); }}
                          className="w-full bg-gray-800 border border-gray-700 rounded px-3 py-1.5 text-white text-sm focus:outline-none focus:ring-1 focus:ring-yellow-500" />
                      </div>
                    ))}
                  </div>

                  {/* Image upload */}
                  <div className="mt-3">
                    <label className="text-[10px] text-gray-400 mb-1 block">Ảnh minh họa</label>
                    <div className="flex items-center gap-3">
                      {/* Preview: uploaded file > existing imageUrl */}
                      {(editCoreImagePreview || detailVocab.imageUrl) && (
                        <img
                          src={editCoreImagePreview ?? detailVocab.imageUrl ?? ''}
                          alt=""
                          className="w-16 h-16 object-cover rounded-lg border border-gray-700 flex-shrink-0"
                        />
                      )}
                      <label className="cursor-pointer text-xs px-3 py-1.5 bg-gray-700 hover:bg-gray-600 border border-gray-600 rounded text-gray-300 flex items-center gap-1.5">
                        <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"/>
                        </svg>
                        {editCoreImageFile ? editCoreImageFile.name : 'Chọn ảnh mới...'}
                        <input type="file" accept="image/*" className="hidden"
                          onChange={e => {
                            const f = e.target.files?.[0] ?? null;
                            setEditCoreImageFile(f);
                            setEditCoreImagePreview(f ? URL.createObjectURL(f) : null);
                          }}
                        />
                      </label>
                      {editCoreImageFile && (
                        <button onClick={() => { setEditCoreImageFile(null); setEditCoreImagePreview(null); }}
                          className="text-xs text-red-400 hover:text-red-300">Xóa</button>
                      )}
                    </div>
                  </div>

                  {editCoreError && <p className="text-xs text-red-400 mt-2">{editCoreError}</p>}

                  <div className="flex justify-end gap-2 mt-4">
                    <button onClick={() => setIsEditingDetail(false)} disabled={editCoreSaving}
                      className="px-3 py-1.5 text-sm bg-gray-700 hover:bg-gray-600 rounded text-gray-300 disabled:opacity-50">Hủy</button>
                    <button onClick={handleSaveCoreEdit} disabled={editCoreSaving}
                      className="px-4 py-1.5 text-sm bg-yellow-600 hover:bg-yellow-500 disabled:opacity-50 rounded text-white font-bold">
                      {editCoreSaving ? 'Đang lưu...' : 'Lưu'}
                    </button>
                  </div>
                </>
              )}
            </div>
          </div>
        )}

        {/* Back to top */}
        {showBackToTop && (
          <button
            onClick={scrollToTop}
            className="fixed bottom-4 right-4 sm:hidden bg-blue-500 text-white p-3 rounded-full shadow-lg hover:bg-blue-600 transition-opacity duration-300 z-50"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 10l7-7m0 0l7 7m-7-7v18" />
            </svg>
          </button>
        )}
      </div>
    </>
  );
};

export default VocabularySetDetail;