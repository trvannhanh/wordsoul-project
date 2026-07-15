import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { aiCreateVocabularySet, aiPreviewVocabularySet, updateVocabularyInSet } from '../../services/vocabularySet';
import { VocabularySetThemeEnum as themeValues } from '../../types/VocabularySetDto';
import { VocabularyDifficultyLevelEnum as difficultyValues } from '../../types/VocabularyDto';
import type { VocabularyDifficultyLevelEnum } from '../../types/VocabularyDto';
import type { VocabularySetThemeEnum, AiCreateVocabularySetResultDto, VocabularyPreviewDto } from '../../types/VocabularySetDto';
import LoadingScreen from '../learningSession/components/LoadingScreen';

const MAX_TITLE_LENGTH = 100;
const MAX_SET_DESC_LENGTH = 300;
const MAX_WORD_LENGTH = 100;
const MAX_MEANING_LENGTH = 200;
const MAX_PRONUNCIATION_LENGTH = 100;
const MAX_VOCAB_DESC_LENGTH = 500;
const MAX_EXAMPLE_LENGTH = 500;
const MAX_WORDS_PER_REQUEST = 50;
// Disallow HTML angle-bracket injection
const UNSAFE_CHARS_RE = /[<>]/;

const CreateVocabularySet: React.FC = () => {
  const navigate = useNavigate();
  const [step, setStep] = useState<1 | 2 | 3 | 4>(1);

  const THEME_VI: Record<string, string> = {
    DailyLife: 'Cuộc sống hàng ngày',
    Nature: 'Thiên nhiên',
    Food: 'Ẩm thực',
    Weather: 'Thời tiết',
    Technology: 'Công nghệ',
    Travel: 'Du lịch',
    Health: 'Sức khỏe',
    Sports: 'Thể thao',
    Business: 'Kinh doanh',
    Science: 'Khoa học',
    Art: 'Nghệ thuật',
    Communication: 'Giao tiếp',
    Mystery: 'Bí ẩn',
    Dark: 'Tối tăm',
    Academic: 'Học thuật',
    Challenge: 'Thử thách',
    TrapWords: 'Từ bẫy',
    System: 'Hệ thống',
    Custom: 'Tùy chỉnh',
  };

  // Set Info
  const [title, setTitle] = useState<string>('');
  const [description, setDescription] = useState<string>('');
  const [theme, setTheme] = useState<VocabularySetThemeEnum>(themeValues.Custom);
  const [difficultyLevel, setDifficultyLevel] = useState<VocabularyDifficultyLevelEnum>(difficultyValues.Beginner);
  const [isPublic, setIsPublic] = useState<boolean>(true);
  const [imageFile, setImageFile] = useState<File | null>(null);

  // Words Input
  const [wordsInput, setWordsInput] = useState<string>('');
  const [useAi, setUseAi] = useState<boolean>(true);
  const [fileError, setFileError] = useState<string | null>(null);

  const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFileError(null);
    const file = e.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (event) => {
      const text = event.target?.result as string;
      let words: string[] = [];

      if (file.name.endsWith('.csv')) {
        words = text.split('\n')
          .map(line => line.split(',')[0].replace(/"/g, '').trim())
          .filter(w => w && isNaN(Number(w)));
      } else {
        words = text.split(/\r?\n/).map(w => w.trim()).filter(Boolean);
      }

      if (words.length > 50) {
        setFileError(`File có ${words.length} từ, vượt quá giới hạn 50 từ. Chỉ lấy 50 từ đầu.`);
        words = words.slice(0, 50);
      }

      setWordsInput(prev => prev ? `${prev}\n${words.join('\n')}` : words.join('\n'));
    };
    reader.readAsText(file, 'utf-8');
    // Reset input để có thể chọn lại cùng file
    e.target.value = '';
  };
  
  // Preview State
  const [previewList, setPreviewList] = useState<VocabularyPreviewDto[]>([]);
  const [originalPreviewList, setOriginalPreviewList] = useState<VocabularyPreviewDto[]>([]);
  const [selectedWordIndices, setSelectedWordIndices] = useState<number[]>([]);

  // Result & State
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<AiCreateVocabularySetResultDto | null>(null);

  // Xử lý upload file ảnh
  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] || null;
    setImageFile(file);
  };

  const validateStep1 = (): string | null => {
    const trimmedTitle = title.trim();
    if (!trimmedTitle) return 'Tiêu đề không được để trống';
    if (trimmedTitle.length < 3) return 'Tiêu đề phải có ít nhất 3 ký tự';
    if (trimmedTitle.length > MAX_TITLE_LENGTH) return `Tiêu đề không được quá ${MAX_TITLE_LENGTH} ký tự`;
    if (UNSAFE_CHARS_RE.test(trimmedTitle)) return 'Tiêu đề không được chứa ký tự < hoặc >';
    if (description.length > MAX_SET_DESC_LENGTH) return `Mô tả không được quá ${MAX_SET_DESC_LENGTH} ký tự`;
    if (description && UNSAFE_CHARS_RE.test(description)) return 'Mô tả không được chứa ký tự < hoặc >';
    if (imageFile && imageFile.size > 5 * 1024 * 1024) return 'Ảnh đại diện không được lớn hơn 5 MB';
    return null;
  };

  const handleNextToStep2 = () => {
    const err = validateStep1();
    if (err) {
      setError(err);
      return;
    }
    setError(null);
    setStep(2);
  };

  const extractErrorMessage = (err: unknown): string => {
    if (err && typeof err === 'object') {
      const e = err as Record<string, unknown>;
      // Axios error: message is in response.data
      const data = (e.response as Record<string, unknown> | undefined)?.data;
      if (typeof data === 'string' && data.length > 0) return data;
      if (data && typeof data === 'object') {
        const d = data as Record<string, unknown>;
        if (typeof d.message === 'string') return d.message;
        if (typeof d.title === 'string') return d.title;
      }
      if (typeof e.message === 'string') return e.message;
    }
    return 'Lỗi không xác định';
  };

  const handleSkipToStep3 = () => {
    setPreviewList([]);
    setOriginalPreviewList([]);
    setSelectedWordIndices([]);
    setError(null);
    setStep(3);
  };

  const handlePreview = async () => {
    // Parse each line: extract only the word part before IPA /.../ or (pos):
    const extractWord = (line: string): string => {
      // Match text before the first / or ( character
      const m = line.match(/^([^/(]+)/);
      return m ? m[1].trim() : line.trim();
    };

    const words = wordsInput
      .split(/[\n,]+/)
      .map((line) => extractWord(line))
      .filter((word) => word.length > 0);

    if (words.length === 0) {
      setError('Vui lòng nhập ít nhất một từ vựng');
      return;
    }
    if (words.length > MAX_WORDS_PER_REQUEST) {
      setError(`Vui lòng nhập tối đa ${MAX_WORDS_PER_REQUEST} từ. Hiện tại có ${words.length} từ.`);
      return;
    }
    const tooLong = words.filter(w => w.length > MAX_WORD_LENGTH);
    if (tooLong.length > 0) {
      setError(`Từ quá dài (tối đa ${MAX_WORD_LENGTH} ký tự): ${tooLong.slice(0, 3).map(w => `"${w}"`).join(', ')}${tooLong.length > 3 ? '...' : ''}`);
      return;
    }

    setError(null);
    setLoading(true);
    try {
      const previewResult = await aiPreviewVocabularySet({ words, useAi });
      setPreviewList(previewResult);
      setOriginalPreviewList(previewResult.map(v => ({ ...v }))); // deep copy for tracking
      // Mặc định chọn tất cả
      setSelectedWordIndices(previewResult.map((_, i) => i));
      setStep(3);
    } catch (err) {
      setError(extractErrorMessage(err));
      console.error('Preview error:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleToggleSelectWord = (index: number) => {
    setSelectedWordIndices(prev => 
      prev.includes(index) ? prev.filter(i => i !== index) : [...prev, index]
    );
  };

  const handleEditPreviewData = (index: number, field: keyof VocabularyPreviewDto, value: string) => {
    setPreviewList(prev => {
      const newList = [...prev];
      newList[index] = { ...newList[index], [field]: value };
      return newList;
    });
  };

  const handleAddNewRow = () => {
    const newVocab: VocabularyPreviewDto = {
      id: null,
      isExisting: false,
      isAiGenerated: false,
      isCustom: true,
      word: '',
      meaning: '',
      pronunciation: '',
      partOfSpeech: '',
      cefrLevel: '',
      description: '',
      exampleSentence: '',
    };
    setPreviewList(prev => [...prev, newVocab]);
    setOriginalPreviewList(prev => [...prev, { ...newVocab }]);
    setSelectedWordIndices(prev => [...prev, previewList.length]);
  };

  const handleDeleteRow = (index: number) => {
    setPreviewList(prev => prev.filter((_, i) => i !== index));
    setOriginalPreviewList(prev => prev.filter((_, i) => i !== index));
    setSelectedWordIndices(prev =>
      prev.filter(i => i !== index).map(i => (i > index ? i - 1 : i))
    );
  };

  const handleCreateSet = async () => {
    if (selectedWordIndices.length === 0) {
      setError('Vui lòng chọn ít nhất một từ vựng');
      return;
    }

    // Validate each selected word before calling the API
    const wordErrors: string[] = [];
    selectedWordIndices.forEach((i) => {
      const v = previewList[i];
      const label = v.word?.trim() ? `"${v.word}"` : `Dòng ${i + 1}`;
      if (!v.word?.trim()) {
        wordErrors.push(`${label}: Từ vựng không được để trống`);
      } else if (v.word.length > MAX_WORD_LENGTH) {
        wordErrors.push(`${label}: Từ vựng quá dài (tối đa ${MAX_WORD_LENGTH} ký tự)`);
      }
      if (!v.isExisting && !v.meaning?.trim()) {
        wordErrors.push(`${label}: Nghĩa tiếng Việt không được để trống`);
      } else if (v.meaning && v.meaning.length > MAX_MEANING_LENGTH) {
        wordErrors.push(`${label}: Nghĩa quá dài (tối đa ${MAX_MEANING_LENGTH} ký tự)`);
      }
      if (v.pronunciation && v.pronunciation.length > MAX_PRONUNCIATION_LENGTH) {
        wordErrors.push(`${label}: Phát âm quá dài (tối đa ${MAX_PRONUNCIATION_LENGTH} ký tự)`);
      }
      if (v.description && v.description.length > MAX_VOCAB_DESC_LENGTH) {
        wordErrors.push(`${label}: Mô tả quá dài (tối đa ${MAX_VOCAB_DESC_LENGTH} ký tự)`);
      }
      if (v.exampleSentence && v.exampleSentence.length > MAX_EXAMPLE_LENGTH) {
        wordErrors.push(`${label}: Câu ví dụ quá dài (tối đa ${MAX_EXAMPLE_LENGTH} ký tự)`);
      }
    });
    if (wordErrors.length > 0) {
      setError(wordErrors.join('\n'));
      return;
    }
    
    setError(null);
    setLoading(true);

    try {
      const formData = new FormData();
      formData.append('Title', title);
      formData.append('Description', description || '');
      formData.append('Theme', theme.toString());
      formData.append('DifficultyLevel', difficultyLevel.toString());
      formData.append('IsPublic', isPublic.toString());
      formData.append('IsActive', 'true');
      
      const vocabulariesToCreate = selectedWordIndices.map(i => previewList[i]);
      
      vocabulariesToCreate.forEach((v, idx) => {
        if (v.id) formData.append(`Vocabularies[${idx}].Id`, v.id.toString());
        formData.append(`Vocabularies[${idx}].IsExisting`, v.isExisting.toString());
        formData.append(`Vocabularies[${idx}].IsAiGenerated`, (v.isAiGenerated ?? false).toString());
        formData.append(`Vocabularies[${idx}].IsCustom`, (v.isCustom ?? false).toString());
        formData.append(`Vocabularies[${idx}].Word`, v.word || '');
        formData.append(`Vocabularies[${idx}].Meaning`, v.meaning || '');
        formData.append(`Vocabularies[${idx}].Pronunciation`, v.pronunciation || '');
        formData.append(`Vocabularies[${idx}].PartOfSpeech`, v.partOfSpeech || '');
        formData.append(`Vocabularies[${idx}].CefrLevel`, v.cefrLevel || '');
        formData.append(`Vocabularies[${idx}].Description`, v.description || '');
        formData.append(`Vocabularies[${idx}].ExampleSentence`, v.exampleSentence || '');
      });
      
      if (imageFile) {
        formData.append('ImageFile', imageFile);
      }

      const createResult = await aiCreateVocabularySet(formData);

      // Apply overrides for existing words that were edited
      const setId = createResult.vocabularySet.id;
      const overridePromises = selectedWordIndices
        .map(i => previewList[i])
        .filter(v => v.isExisting && v.id != null)
        .filter(v => {
          const orig = originalPreviewList.find(o => o.id === v.id);
          if (!orig) return false;
          return v.meaning !== orig.meaning || v.pronunciation !== orig.pronunciation
            || v.exampleSentence !== orig.exampleSentence || v.description !== orig.description;
        })
        .map(v => {
          const orig = originalPreviewList.find(o => o.id === v.id)!;
          return updateVocabularyInSet(setId, v.id!, {
            overrideMeaning: v.meaning !== orig.meaning ? v.meaning : null,
            overridePronunciation: v.pronunciation !== orig.pronunciation ? v.pronunciation : null,
            overrideExampleSentence: v.exampleSentence !== orig.exampleSentence ? v.exampleSentence : null,
            overrideDescription: v.description !== orig.description ? v.description : null,
          });
        });
      if (overridePromises.length > 0) {
        await Promise.allSettled(overridePromises);
      }

      setResult(createResult);
      setStep(4);
      // Signal the list page to refresh after creation
      localStorage.setItem('vocabSetListDirty', Date.now().toString());
    } catch (err) {
      setError(extractErrorMessage(err));
      console.error('Create error:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="background-color min-h-screen text-white flex justify-center items-start py-10 mt-10">
      {loading && <LoadingScreen disableAudio={true} />}

      <div className={step === 3 ? "w-11/12 max-w-[95%]" : "w-7/12"}>
        <h1 className="text-3xl font-bold mb-6">Tạo bộ từ vựng</h1>

        {error && (
          <div className="mb-4 p-3 bg-red-500/20 border border-red-500 rounded-md">
            {error.includes('\n') ? (
              <ul className="list-disc list-inside space-y-0.5 text-sm">
                {error.split('\n').map((line, i) => <li key={i}>{line}</li>)}
              </ul>
            ) : error}
          </div>
        )}

        {/* BƯỚC 1: THÔNG TIN BỘ TỪ VỰNG */}
        {step === 1 && (
          <div className="flex flex-col gap-4 bg-gray-800 p-6 rounded-md border border-gray-700 shadow-xl">
            <h2 className="text-xl font-semibold border-b border-gray-700 pb-2">Bước 1: Thông tin bộ từ vựng</h2>
            
            <div>
              <div className="flex justify-between items-baseline mb-1">
                <label className="text-sm font-medium">Tiêu đề <span className="text-red-400">*</span></label>
                <span className={`text-xs ${title.length > MAX_TITLE_LENGTH ? 'text-red-400 font-semibold' : 'text-gray-500'}`}>
                  {title.length}/{MAX_TITLE_LENGTH}
                </span>
              </div>
              <input
                type="text"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="Nhập tiêu đề bộ từ vựng"
                maxLength={MAX_TITLE_LENGTH}
                className={`w-full p-2 border rounded-md bg-gray-900 text-white focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                  title.length > 0 && title.trim().length < 3 ? 'border-yellow-500' : 'border-gray-700'
                }`}
              />
              {title.length > 0 && title.trim().length < 3 && (
                <p className="text-xs text-yellow-400 mt-1">Tiêu đề phải có ít nhất 3 ký tự</p>
              )}
            </div>
            <div>
              <div className="flex justify-between items-baseline mb-1">
                <label className="text-sm font-medium">Mô tả</label>
                <span className={`text-xs ${description.length > MAX_SET_DESC_LENGTH ? 'text-red-400 font-semibold' : 'text-gray-500'}`}>
                  {description.length}/{MAX_SET_DESC_LENGTH}
                </span>
              </div>
              <textarea
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Nhập mô tả (không bắt buộc)"
                maxLength={MAX_SET_DESC_LENGTH}
                className={`w-full p-2 border rounded-md bg-gray-900 text-white focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                  description.length > MAX_SET_DESC_LENGTH ? 'border-red-500' : 'border-gray-700'
                }`}
                rows={3}
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Ảnh đại diện</label>
              <input
                type="file"
                accept="image/*"
                onChange={handleImageChange}
                className="w-full p-2 border rounded-md bg-gray-900 text-white focus:outline-none focus:ring-2 focus:ring-blue-500 border-gray-700"
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium mb-1">Chủ đề</label>
                <select
                  value={theme}
                  onChange={(e) => setTheme(Number(e.target.value) as VocabularySetThemeEnum)}
                  className="w-full p-2 border rounded-md bg-gray-900 text-white focus:outline-none focus:ring-2 focus:ring-blue-500 border-gray-700"
                >
                  {Object.entries(themeValues).map(([key, value]) => (
                    <option key={key} value={value}>{THEME_VI[key] ?? key}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Độ khó</label>
                <select
                  value={difficultyLevel}
                  onChange={(e) => setDifficultyLevel(Number(e.target.value) as VocabularyDifficultyLevelEnum)}
                  className="w-full p-2 border rounded-md bg-gray-900 text-white focus:outline-none focus:ring-2 focus:ring-blue-500 border-gray-700"
                >
                  <option value={difficultyValues.Beginner}>Dễ</option>
                  <option value={difficultyValues.Intermediate}>Vừa</option>
                  <option value={difficultyValues.Advanced}>Khó</option>
                </select>
              </div>
            </div>
            <div className="flex items-center gap-2 mt-2">
              <input
                type="checkbox"
                checked={isPublic}
                onChange={(e) => setIsPublic(e.target.checked)}
                className="h-5 w-5 text-blue-500"
              />
              <label className="text-sm font-medium">Công khai</label>
            </div>
            <div className="flex justify-end mt-4">
              <button
                onClick={handleNextToStep2}
                className="bg-blue-500 text-white px-6 py-2 rounded-md hover:bg-blue-600 font-medium transition-colors"
              >
                Tiếp tục
              </button>
            </div>
          </div>
        )}

        {/* BƯỚC 2: NHẬP TỪ VỰNG */}
        {step === 2 && (
          <div className="flex flex-col gap-4 bg-gray-800 p-6 rounded-md border border-gray-700 shadow-xl">
            <h2 className="text-xl font-semibold border-b border-gray-700 pb-2">Bước 2: Danh sách từ vựng</h2>
            <p className="text-sm text-gray-300">
              Nhập danh sách từ vựng. {useAi ? 'Hệ thống AI sẽ tự động phân tích và sinh nghĩa, IPA, định nghĩa và ví dụ cho các từ chưa có trong hệ thống.' : 'Các từ chưa có trong hệ thống sẽ được để trống để bạn tự điền.'}
            </p>

            {/* Toggle AI */}
            <div className="flex items-center justify-between p-3 bg-gray-900 rounded-md border border-gray-700">
              <div className="flex flex-col">
                <span className="text-sm font-semibold text-white">Sử dụng AI sinh thông tin</span>
                <span className="text-xs text-gray-400 mt-0.5">
                  {useAi ? 'AI sẽ tự động điền nghĩa, IPA, định nghĩa và câu ví dụ' : 'Bạn sẽ tự điền thông tin cho từng từ vựng'}
                </span>
              </div>
              <button
                type="button"
                onClick={() => setUseAi(prev => !prev)}
                className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 focus:ring-offset-gray-900 ${
                  useAi ? 'bg-blue-600' : 'bg-gray-600'
                }`}
              >
                <span
                  className={`inline-block h-4 w-4 transform rounded-full bg-white shadow-md transition-transform ${
                    useAi ? 'translate-x-6' : 'translate-x-1'
                  }`}
                />
              </button>
            </div>

            <textarea
              value={wordsInput}
              onChange={(e) => setWordsInput(e.target.value)}
              placeholder="Nhập danh sách từ vựng (cách nhau bởi dấu phẩy hoặc dòng mới, ví dụ: apple, banana)"
              className="w-full p-3 border rounded-md bg-gray-900 text-white focus:outline-none focus:ring-2 focus:ring-blue-500 font-mono border-gray-700"
              rows={8}
            />
            {(() => {
              const count = wordsInput.split(/[\n,]+/).map(l => l.trim()).filter(l => l.length > 0).length;
              return count > 0 ? (
                <p className={`text-xs mt-1 ${count > MAX_WORDS_PER_REQUEST ? 'text-red-400 font-semibold' : 'text-gray-400'}`}>
                  {count} từ{count > MAX_WORDS_PER_REQUEST ? ` — vượt giới hạn ${MAX_WORDS_PER_REQUEST} từ` : ` / tối đa ${MAX_WORDS_PER_REQUEST}`}
                </p>
              ) : null;
            })()}

            {/* File upload */}
            <div className="flex flex-col gap-2">
              <div className="flex items-center gap-3">
                <div className="flex-1 border-t border-gray-700" />
                <span className="text-xs text-gray-500">hoặc tải lên file</span>
                <div className="flex-1 border-t border-gray-700" />
              </div>
              <label className="flex items-center gap-2 cursor-pointer w-fit px-4 py-2 bg-gray-700 hover:bg-gray-600 border border-gray-600 rounded-md text-sm text-white transition-colors">
                <svg className="w-4 h-4 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" />
                </svg>
                Chọn file .txt / .csv
                <input
                  type="file"
                  accept=".txt,.csv"
                  onChange={handleFileUpload}
                  className="hidden"
                />
              </label>
              <p className="text-xs text-gray-500">.txt: mỗi dòng 1 từ &nbsp;|&nbsp; .csv: cột đầu tiên là từ (tối đa 50 từ)</p>
              {fileError && <p className="text-xs text-yellow-400">{fileError}</p>}
            </div>
            <div className="flex justify-between mt-4">
              <button
                onClick={() => setStep(1)}
                className="bg-gray-600 text-white px-6 py-2 rounded-md hover:bg-gray-500 font-medium transition-colors"
              >
                Quay lại
              </button>
              <div className="flex items-center gap-2">
                <button
                  onClick={handleSkipToStep3}
                  className="bg-gray-700 text-gray-300 px-4 py-2 rounded-md hover:bg-gray-600 font-medium transition-colors text-sm"
                  title="Bỏ qua bước nhập từ, thêm từ thủ công ở bước sau"
                >
                  Bỏ qua
                </button>
                <button
                  onClick={handlePreview}
                  className={`text-white px-6 py-2 rounded-md font-medium transition-colors flex items-center gap-2 ${
                    useAi
                      ? 'bg-blue-500 hover:bg-blue-600'
                      : 'bg-gray-500 hover:bg-gray-400'
                  }`}
                  disabled={loading}
                >
                  {useAi ? 'Phân tích bằng AI' : 'Tiếp tục'}
                </button>
              </div>
            </div>
          </div>
        )}

        {/* BƯỚC 3: LỌC & CHỈNH SỬA TỪ VỰNG */}
        {step === 3 && (
          <div className="flex flex-col gap-4 bg-gray-800 p-6 rounded-md border border-gray-700 shadow-xl">
            <div className="flex justify-between items-center border-b border-gray-700 pb-2">
              <h2 className="text-xl font-semibold">Bước 3: Lọc & Chỉnh sửa từ vựng</h2>
              <span className="text-pink-400 font-semibold bg-gray-900 px-3 py-1 rounded-full border border-gray-700">
                Đã chọn: {selectedWordIndices.length} / {previewList.length} từ
              </span>
            </div>
            <p className="text-sm text-gray-300 mb-2">
              Dưới đây là kết quả AI tạo ra. Bạn có thể chỉnh sửa trực tiếp các nội dung trước khi lưu, hoặc bỏ chọn các từ không mong muốn.
              Từ đánh dấu "Đã có sẵn" sẽ được tái sử dụng từ dữ liệu hệ thống.
            </p>

            <div className="overflow-x-auto bg-gray-900 rounded-md border border-gray-700 max-h-[60vh] overflow-y-auto custom-scrollbar">
              <table className="w-full text-left text-sm text-gray-300">
                <thead className="text-xs text-gray-400 uppercase bg-gray-800 sticky top-0 z-10 shadow-sm border-b border-gray-700">
                  <tr>
                    <th scope="col" className="p-3 w-12 text-center">
                      <input
                        type="checkbox"
                        checked={selectedWordIndices.length === previewList.length && previewList.length > 0}
                        onChange={(e) => {
                          if (e.target.checked) {
                            setSelectedWordIndices(previewList.map((_, i) => i));
                          } else {
                            setSelectedWordIndices([]);
                          }
                        }}
                        className="h-4 w-4 text-blue-500 rounded bg-gray-700 border-gray-600 focus:ring-blue-500"
                      />
                    </th>
                    <th scope="col" className="p-3 w-32">Từ vựng</th>
                    <th scope="col" className="p-3 w-40">Nghĩa VN</th>
                    <th scope="col" className="p-3 w-32">Phát âm</th>
                    <th scope="col" className="p-3 w-28">Loại từ</th>
                    <th scope="col" className="p-3 min-w-[200px]">Mô tả (EN)</th>
                    <th scope="col" className="p-3 min-w-[250px]">Câu ví dụ</th>
                    <th scope="col" className="p-3 w-10"></th>
                  </tr>
                </thead>
                <tbody>
                  {previewList.map((item, index) => {
                    const isSelected = selectedWordIndices.includes(index);
                    const isExisting = item.isExisting;
                    
                    return (
                      <tr key={index} className={`border-b border-gray-800 hover:bg-gray-800/50 transition-colors ${!isSelected ? 'opacity-50' : ''}`}>
                        <td className="p-3 text-center">
                          <input
                            type="checkbox"
                            checked={isSelected}
                            onChange={() => handleToggleSelectWord(index)}
                            className="h-4 w-4 text-blue-500 rounded bg-gray-700 border-gray-600"
                          />
                        </td>
                        <td className="p-2">
                          <div className="flex flex-col gap-1">
                            {!isExisting ? (
                              <input
                                type="text"
                                value={item.word}
                                onChange={(e) => handleEditPreviewData(index, 'word', e.target.value)}
                                disabled={!isSelected}
                                placeholder="Nhập từ..."
                                className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 focus:ring-1 focus:ring-blue-500 focus:outline-none disabled:opacity-50 text-white font-bold"
                              />
                            ) : (
                              <>
                                <span className="font-bold text-white">{item.word}</span>
                                <span className="text-[10px] bg-green-900/50 text-green-400 border border-green-800 px-1 py-0.5 rounded-sm w-fit whitespace-nowrap">Đã có sẵn</span>
                              </>
                            )}
                          </div>
                        </td>
                        <td className="p-2">
                          <input
                            type="text"
                            value={item.meaning}
                            onChange={(e) => handleEditPreviewData(index, 'meaning', e.target.value)}
                            disabled={!isSelected}
                            className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 focus:ring-1 focus:ring-blue-500 focus:outline-none disabled:opacity-50 text-white"
                          />
                        </td>
                        <td className="p-2">
                          <input
                            type="text"
                            value={item.pronunciation}
                            onChange={(e) => handleEditPreviewData(index, 'pronunciation', e.target.value)}
                            disabled={!isSelected}
                            className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 focus:ring-1 focus:ring-blue-500 focus:outline-none disabled:opacity-50 text-blue-300 font-mono text-xs"
                          />
                        </td>
                        <td className="p-2">
                            {isExisting ? (
                              <span className="inline-block px-2 py-0.5 bg-gray-700 border border-gray-600 rounded text-yellow-200 text-xs">
                                {item.partOfSpeech || '—'}
                              </span>
                            ) : (
                              <select
                                value={item.partOfSpeech}
                                onChange={(e) => handleEditPreviewData(index, 'partOfSpeech', e.target.value)}
                                disabled={!isSelected}
                                className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 focus:ring-1 focus:ring-blue-500 focus:outline-none disabled:opacity-50 text-yellow-200 text-xs"
                              >
                                <option value="">-- Chọn --</option>
                                <option value="noun">Noun</option>
                                <option value="verb">Verb</option>
                                <option value="adjective">Adjective</option>
                                <option value="adverb">Adverb</option>
                                <option value="pronoun">Pronoun</option>
                                <option value="preposition">Preposition</option>
                                <option value="conjunction">Conjunction</option>
                                <option value="interjection">Interjection</option>
                                <option value="article">Article</option>
                                <option value="phrasal verb">Phrasal Verb</option>
                                <option value="idiom">Idiom</option>
                              </select>
                            )}
                        </td>
                        <td className="p-2">
                          <textarea
                            value={item.description}
                            onChange={(e) => handleEditPreviewData(index, 'description', e.target.value)}
                            disabled={!isSelected}
                            rows={2}
                            className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 focus:ring-1 focus:ring-blue-500 focus:outline-none disabled:opacity-50 text-xs custom-scrollbar"
                          />
                        </td>
                        <td className="p-2">
                          <textarea
                            value={item.exampleSentence}
                            onChange={(e) => handleEditPreviewData(index, 'exampleSentence', e.target.value)}
                            disabled={!isSelected}
                            rows={2}
                            className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 focus:ring-1 focus:ring-blue-500 focus:outline-none disabled:opacity-50 text-xs custom-scrollbar italic"
                          />
                        </td>
                        <td className="p-2 text-center">
                          <button
                            type="button"
                            onClick={() => handleDeleteRow(index)}
                            className="text-gray-600 hover:text-red-400 transition-colors"
                            title="Xóa dòng này"
                          >
                            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                            </svg>
                          </button>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>

            {/* Add new blank row */}
            <button
              type="button"
              onClick={handleAddNewRow}
              className="flex items-center gap-2 text-sm text-blue-400 hover:text-blue-300 mt-2 transition-colors"
            >
              <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              Thêm từ mới
            </button>
            
            <div className="flex justify-between mt-4">
              <button
                onClick={() => setStep(2)}
                className="bg-gray-600 text-white px-6 py-2 rounded-md hover:bg-gray-500 font-medium transition-colors"
                disabled={loading}
              >
                Quay lại
              </button>
              <button
                onClick={handleCreateSet}
                disabled={loading || selectedWordIndices.length === 0}
                className="bg-green-600 text-white px-6 py-2 rounded-md hover:bg-green-500 disabled:bg-gray-600 font-bold transition-colors shadow-lg shadow-green-600/20"
              >
                Hoàn tất & Lưu bộ từ vựng
              </button>
            </div>
          </div>
        )}

        {/* BƯỚC 4: KẾT QUẢ */}
        {step === 4 && result && (
          <div className="flex flex-col gap-6 bg-gray-800 p-8 rounded-md border border-green-500 shadow-xl shadow-green-500/10">
            <div className="text-center">
              <div className="w-20 h-20 bg-green-500 rounded-full flex items-center justify-center mx-auto mb-4 shadow-lg shadow-green-500/50">
                <svg className="w-10 h-10 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="3" d="M5 13l4 4L19 7"></path></svg>
              </div>
              <h2 className="text-2xl font-bold text-green-400">Tạo bộ từ vựng thành công!</h2>
              <p className="text-gray-300 mt-2">
                Bộ từ vựng <span className="font-bold text-white">{result.vocabularySet.title}</span> đã được lưu vào hệ thống.
              </p>
            </div>

            <div className="grid grid-cols-3 gap-4 text-center mt-4">
              <div className="bg-gray-900 p-4 rounded-md border border-gray-700">
                <p className="text-3xl font-bold text-blue-400">{result.totalWords}</p>
                <p className="text-sm text-gray-400 uppercase tracking-wide mt-1">Tổng số từ</p>
              </div>
              <div className="bg-gray-900 p-4 rounded-md border border-gray-700">
                <p className="text-3xl font-bold text-green-400">{result.newlyCreated}</p>
                <p className="text-sm text-gray-400 uppercase tracking-wide mt-1">Từ mới tạo (AI)</p>
              </div>
              <div className="bg-gray-900 p-4 rounded-md border border-gray-700">
                <p className="text-3xl font-bold text-yellow-400">{result.alreadyExisted}</p>
                <p className="text-sm text-gray-400 uppercase tracking-wide mt-1">Từ tái sử dụng</p>
              </div>
            </div>

            {result.failedWords.length > 0 && (
              <div className="mt-4 p-4 bg-red-900/30 border border-red-500 rounded-md">
                <h3 className="text-red-400 font-bold mb-2">Từ bị lỗi không xử lý được ({result.failedWords.length}):</h3>
                <div className="flex flex-wrap gap-2">
                  {result.failedWords.map((word, i) => (
                    <span key={i} className="bg-red-800 text-white text-xs px-2 py-1 rounded border border-red-700">{word}</span>
                  ))}
                </div>
              </div>
            )}

            <div className="flex justify-center mt-6">
              <button
                onClick={() => navigate(`/vocabularySet/${result.vocabularySet.id}`)}
                className="bg-blue-500 text-white px-8 py-3 rounded-md hover:bg-blue-600 font-bold transition-colors shadow-lg shadow-blue-500/30"
              >
                Khám phá bộ từ vựng ngay
              </button>
            </div>
          </div>
        )}

      </div>
    </div>
  );
};

export default CreateVocabularySet;