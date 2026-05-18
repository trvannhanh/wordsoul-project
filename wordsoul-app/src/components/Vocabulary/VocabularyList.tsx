import { useEffect, useState } from "react";
import { fetchVocabularySetDetail, removeVocabFromSet, updateVocabularyInSet, addNewVocabToSet } from "../../services/vocabularySet";
import type { VocabularyPreviewDto, UpdateVocabularyInSetDto } from "../../types/VocabularySetDto";
import Bar from "../Bar";

interface Vocabulary {
  id: number;
  word: string;
  meaning: string;
  imageUrl: string | null;
  pronunciation: string | null;
  pronunciationUrl: string | null;
  exampleSentenceAudioUrl: string | null;
  partOfSpeech: string;
  exampleSentence: string | null;
  description: string | null;
  isCustomEdited: boolean;
  originalMeaning: string | null;
}

interface VocabularyListProps {
  setId: number;
  pageSize?: number;
  isOwner?: boolean;
  vocabMemoryStates?: Record<number, string>;
  onClickVocab?: (vocab: Vocabulary) => void;
}

const POS_OPTIONS = [
  { value: 'noun', label: 'Noun' },
  { value: 'verb', label: 'Verb' },
  { value: 'adjective', label: 'Adjective' },
  { value: 'adverb', label: 'Adverb' },
  { value: 'pronoun', label: 'Pronoun' },
  { value: 'preposition', label: 'Preposition' },
  { value: 'conjunction', label: 'Conjunction' },
  { value: 'interjection', label: 'Interjection' },
  { value: 'phrasal verb', label: 'Phrasal Verb' },
  { value: 'idiom', label: 'Idiom' },
];

const VocabularyList: React.FC<VocabularyListProps> = ({ setId, pageSize = 5, isOwner = false, vocabMemoryStates, onClickVocab }) => {
  const [vocabularies, setVocabularies] = useState<Vocabulary[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalVocabularies, setTotalVocabularies] = useState(0);
  const [loading, setLoading] = useState(false);
  const [studyFilter, setStudyFilter] = useState<'all' | 'studied' | 'new'>('all');

  // ── Inline add form ────────────────────────────────────────────────────────
  const [showAddForm, setShowAddForm] = useState(false);
  const [addWord, setAddWord] = useState('');
  const [addMeaning, setAddMeaning] = useState('');
  const [addPronunciation, setAddPronunciation] = useState('');
  const [addPartOfSpeech, setAddPartOfSpeech] = useState('noun');
  const [addExample, setAddExample] = useState('');
  const [addDesc, setAddDesc] = useState('');
  const [addSaving, setAddSaving] = useState(false);
  const [addFormError, setAddFormError] = useState('');

  // ── Inline edit ────────────────────────────────────────────────────────────
  const [editingVocabId, setEditingVocabId] = useState<number | null>(null);
  const [editMeaning, setEditMeaning] = useState('');
  const [editPronunciation, setEditPronunciation] = useState('');
  const [editExample, setEditExample] = useState('');
  const [editDesc, setEditDesc] = useState('');
  const [editSaving, setEditSaving] = useState(false);

  // ── Remove ─────────────────────────────────────────────────────────────────
  const [removingId, setRemovingId] = useState<number | null>(null);

  const fetchData = async (pageNumber: number) => {
    setLoading(true);
    try {
      const data = await fetchVocabularySetDetail(setId, pageNumber, pageSize);
      setVocabularies(data.vocabularies as unknown as Vocabulary[]);
      setTotalPages(data.totalPages);
      setTotalVocabularies(data.totalVocabularies);
    } catch (err) {
      console.error("Error fetching vocabularies:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData(page);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, setId]);

  // Search for vocabs to add — removed; using manual entry now

  const handleAddNewVocab = async () => {
    if (!addWord.trim()) { setAddFormError('Từ vựng không được để trống'); return; }
    if (!addMeaning.trim()) { setAddFormError('Nghĩa không được để trống'); return; }
    setAddFormError('');
    setAddSaving(true);
    try {
      const dto: VocabularyPreviewDto = {
        id: null,
        isExisting: false,
        isAiGenerated: false,
        isCustom: true,
        word: addWord.trim(),
        meaning: addMeaning.trim(),
        pronunciation: addPronunciation,
        partOfSpeech: addPartOfSpeech,
        cefrLevel: '',
        description: addDesc,
        exampleSentence: addExample,
      };
      await addNewVocabToSet(setId, dto);
      setAddWord(''); setAddMeaning(''); setAddPronunciation('');
      setAddPartOfSpeech('noun'); setAddExample(''); setAddDesc('');
      setShowAddForm(false);
      setTotalVocabularies(t => t + 1);
      await fetchData(page);
    } catch (err: unknown) {
      const e = err as Record<string, unknown>;
      const data = (e?.response as Record<string, unknown> | undefined)?.data;
      setAddFormError((typeof data === 'string' ? data : (data as Record<string, unknown> | undefined)?.message as string) || (e?.message as string) || 'Lỗi thêm từ vựng');
    } finally {
      setAddSaving(false);
    }
  };

  const handleOpenEdit = (vocab: Vocabulary) => {
    setEditingVocabId(vocab.id);
    setEditMeaning(vocab.meaning ?? '');
    setEditPronunciation(vocab.pronunciation ?? '');
    setEditExample(vocab.exampleSentence ?? '');
    setEditDesc(vocab.description ?? '');
  };

  const handleSaveEdit = async (vocabId: number) => {
    setEditSaving(true);
    try {
      const dto: UpdateVocabularyInSetDto = {
        overrideMeaning: editMeaning || null,
        overridePronunciation: editPronunciation || null,
        overrideExampleSentence: editExample || null,
        overrideDescription: editDesc || null,
      };
      await updateVocabularyInSet(setId, vocabId, dto);
      setEditingVocabId(null);
      await fetchData(page);
    } catch { /* silently ignore */ } finally { setEditSaving(false); }
  };

  const handleResetEdit = async (vocabId: number) => {
    setEditSaving(true);
    try {
      await updateVocabularyInSet(setId, vocabId, {
        overrideMeaning: null, overridePronunciation: null,
        overrideExampleSentence: null, overrideDescription: null,
      });
      setEditingVocabId(null);
      await fetchData(page);
    } catch { /* silently ignore */ } finally { setEditSaving(false); }
  };

  const handleRemoveVocab = async (vocabId: number) => {
    setRemovingId(vocabId);
    try {
      await removeVocabFromSet(setId, vocabId);
      setVocabularies(prev => prev.filter(v => v.id !== vocabId));
      setTotalVocabularies(t => t - 1);
    } catch { /* silently ignore */ } finally { setRemovingId(null); }
  };

  const filteredVocabs = vocabMemoryStates
    ? vocabularies.filter(v => {
        if (studyFilter === 'studied') return vocabMemoryStates[v.id] !== undefined;
        if (studyFilter === 'new')     return vocabMemoryStates[v.id] === undefined;
        return true;
      })
    : vocabularies;

  return (
    <div className="w-full border rounded-lg p-3">
      {/* Filter + Pagination row */}
      <div className="mb-3 flex flex-wrap justify-between items-center gap-2 text-gray-400">
        {vocabMemoryStates ? (
          <select
            value={studyFilter}
            onChange={e => setStudyFilter(e.target.value as 'all' | 'studied' | 'new')}
            className="text-xs px-2 py-1 rounded background-color text-color border border-gray-600 focus:outline-none focus:ring-1 focus:ring-blue-500"
          >
            <option value="all">Tất cả</option>
            <option value="studied">Đã học</option>
            <option value="new">Chưa học</option>
          </select>
        ) : <span />}
        <div className="flex items-center gap-2">
          <button disabled={page <= 1} onClick={() => setPage(p => p - 1)}
            className="px-3 py-1 background-color text-color rounded disabled:opacity-50 custom-cursor">←</button>
          <span className="text-xs">Trang {page}/{totalPages} ({totalVocabularies} từ)</span>
          <button disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}
            className="px-3 py-1 background-color text-color rounded disabled:opacity-50 custom-cursor">→</button>
        </div>
      </div>

      {/* ── Add new vocab form (owner only) ──────────────────────────────────── */}
      {isOwner && (
        <div className="mb-3 border-b border-gray-700 pb-3">
          {!showAddForm ? (
            <button
              onClick={() => setShowAddForm(true)}
              className="flex items-center gap-1.5 text-xs text-blue-400 hover:text-blue-300 transition-colors"
            >
              <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4"/>
              </svg>
              Thêm từ mới vào bộ
            </button>
          ) : (
            <div className="bg-gray-900/60 border border-gray-700 rounded-lg p-3 flex flex-col gap-2">
              <div className="text-xs font-semibold text-gray-300 mb-1">
                Thêm từ mới — ảnh &amp; âm thanh sẽ được tạo tự động
              </div>
              <div className="grid grid-cols-2 gap-2">
                <div>
                  <label className="text-[10px] text-gray-400 mb-0.5 block">Từ vựng <span className="text-red-400">*</span></label>
                  <input type="text" value={addWord}
                    onChange={e => { setAddWord(e.target.value); setAddFormError(''); }}
                    placeholder="e.g. serendipity"
                    className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 text-white text-sm focus:outline-none focus:ring-1 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="text-[10px] text-gray-400 mb-0.5 block">Nghĩa tiếng Việt <span className="text-red-400">*</span></label>
                  <input type="text" value={addMeaning}
                    onChange={e => { setAddMeaning(e.target.value); setAddFormError(''); }}
                    placeholder="e.g. sự tình cờ may mắn"
                    className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 text-white text-sm focus:outline-none focus:ring-1 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="text-[10px] text-gray-400 mb-0.5 block">Phát âm</label>
                  <input type="text" value={addPronunciation}
                    onChange={e => setAddPronunciation(e.target.value)}
                    placeholder="/ˌser.ənˈdɪp.ɪ.ti/"
                    className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 text-blue-300 font-mono text-xs focus:outline-none focus:ring-1 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="text-[10px] text-gray-400 mb-0.5 block">Loại từ</label>
                  <select value={addPartOfSpeech} onChange={e => setAddPartOfSpeech(e.target.value)}
                    className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 text-yellow-200 text-xs focus:outline-none focus:ring-1 focus:ring-blue-500">
                    {POS_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
                  </select>
                </div>
              </div>
              <div>
                <label className="text-[10px] text-gray-400 mb-0.5 block">Câu ví dụ</label>
                <input type="text" value={addExample}
                  onChange={e => setAddExample(e.target.value)}
                  placeholder="It was pure serendipity that we met."
                  className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 text-white text-sm italic focus:outline-none focus:ring-1 focus:ring-blue-500" />
              </div>
              <div>
                <label className="text-[10px] text-gray-400 mb-0.5 block">Mô tả</label>
                <input type="text" value={addDesc}
                  onChange={e => setAddDesc(e.target.value)}
                  placeholder="The act of finding something valuable by chance..."
                  className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 text-white text-sm focus:outline-none focus:ring-1 focus:ring-blue-500" />
              </div>
              {addFormError && <p className="text-xs text-red-400">{addFormError}</p>}
              <div className="flex justify-end gap-2 mt-1">
                <button onClick={() => { setShowAddForm(false); setAddFormError(''); }} disabled={addSaving}
                  className="px-3 py-1 text-xs bg-gray-700 hover:bg-gray-600 rounded text-gray-300 disabled:opacity-50">
                  Hủy
                </button>
                <button onClick={handleAddNewVocab} disabled={addSaving}
                  className="px-3 py-1 text-xs bg-blue-600 hover:bg-blue-500 disabled:opacity-50 rounded text-white font-semibold flex items-center gap-1">
                  {addSaving ? (
                    <><svg className="w-3 h-3 animate-spin" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
                    </svg>Đang tạo...</>
                  ) : 'Tạo từ vựng'}
                </button>
              </div>
            </div>
          )}
        </div>
      )}

      {/* ── Vocabulary list ───────────────────────────────────────────────────── */}
      {loading ? (
        <div className="text-center text-gray-400">Loading words...</div>
      ) : filteredVocabs.length === 0 ? (
        <div className="text-center text-gray-500 text-sm py-4">Không có từ nào</div>
      ) : (
        filteredVocabs.map((vocab) => (
          <div key={vocab.id}>
            <div className="flex items-center gap-1">
              <div className="flex-1 min-w-0">
                <Bar
                  id={vocab.id}
                  word={vocab.word}
                  meaning={vocab.meaning}
                  pronunciation={vocab.pronunciation || "N/A"}
                  partOfSpeech={vocab.partOfSpeech}
                  image={vocab.imageUrl || "https://via.placeholder.com/150"}
                  isCustomEdited={vocab.isCustomEdited}
                  originalMeaning={vocab.originalMeaning}
                  memoryState={vocabMemoryStates ? vocabMemoryStates[vocab.id] : undefined}
                  onEdit={isOwner ? () => {
                    if (editingVocabId === vocab.id) setEditingVocabId(null);
                    else handleOpenEdit(vocab);
                  } : undefined}
                  onClick={onClickVocab ? () => onClickVocab(vocab) : undefined}
                />
              </div>
              {isOwner && (
                <button onClick={() => handleRemoveVocab(vocab.id)} disabled={removingId === vocab.id}
                  className="flex-shrink-0 p-1 text-gray-600 hover:text-red-400 disabled:opacity-40 transition-colors" title="Gỡ khỏi bộ">
                  {removingId === vocab.id
                    ? <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/></svg>
                    : <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12"/></svg>}
                </button>
              )}
            </div>

            {/* Inline edit panel */}
            {isOwner && editingVocabId === vocab.id && (
              <div className="ml-1 mb-2 bg-gray-900/70 border border-yellow-700/40 rounded-lg p-3 flex flex-col gap-2">
                <div className="text-[10px] font-semibold text-yellow-400 mb-0.5">
                  Chỉnh sửa override — chỉ ảnh hưởng trong bộ này
                </div>
                {[
                  { label: 'Nghĩa', value: editMeaning, set: setEditMeaning },
                  { label: 'Phát âm', value: editPronunciation, set: setEditPronunciation },
                  { label: 'Câu ví dụ', value: editExample, set: setEditExample },
                  { label: 'Mô tả', value: editDesc, set: setEditDesc },
                ].map(f => (
                  <div key={f.label} className="flex items-center gap-2">
                    <label className="text-[10px] text-gray-400 w-16 flex-shrink-0">{f.label}</label>
                    <input value={f.value} onChange={e => f.set(e.target.value)}
                      placeholder="Để trống = dùng giá trị gốc"
                      className="flex-1 bg-gray-800 border border-gray-700 rounded px-2 py-1 text-white text-xs focus:outline-none focus:ring-1 focus:ring-yellow-500" />
                  </div>
                ))}
                <div className="flex justify-between mt-1">
                  <button onClick={() => handleResetEdit(vocab.id)} disabled={editSaving}
                    className="px-2 py-1 text-[10px] bg-red-900/40 hover:bg-red-800/60 border border-red-700/50 rounded text-red-300 disabled:opacity-50">
                    Reset mặc định
                  </button>
                  <div className="flex gap-2">
                    <button onClick={() => setEditingVocabId(null)} disabled={editSaving}
                      className="px-2 py-1 text-[10px] bg-gray-700 hover:bg-gray-600 rounded text-gray-300 disabled:opacity-50">Hủy</button>
                    <button onClick={() => handleSaveEdit(vocab.id)} disabled={editSaving}
                      className="px-2 py-1 text-[10px] bg-yellow-600 hover:bg-yellow-500 disabled:opacity-50 rounded text-white font-bold">
                      {editSaving ? 'Lưu...' : 'Lưu'}
                    </button>
                  </div>
                </div>
              </div>
            )}
          </div>
        ))
      )}
    </div>
  );
};

export default VocabularyList;

