import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { aiCreateVocabularySet, aiPreviewVocabularySet } from '../../services/vocabularySet';
import { VocabularySetThemeEnum as themeValues } from '../../types/VocabularySetDto';
import { VocabularyDifficultyLevelEnum as difficultyValues } from '../../types/VocabularyDto';
import type { VocabularyDifficultyLevelEnum } from '../../types/VocabularyDto';
import type { VocabularySetThemeEnum, AiCreateVocabularySetResultDto, VocabularyPreviewDto } from '../../types/VocabularySetDto';
import LoadingScreen from '../../components/LearningSession/LoadingScreen';

const CreateVocabularySet: React.FC = () => {
  const navigate = useNavigate();
  const [step, setStep] = useState<1 | 2 | 3 | 4>(1);

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
  
  // Preview State
  const [previewList, setPreviewList] = useState<VocabularyPreviewDto[]>([]);
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

  const handleNextToStep2 = () => {
    if (!title.trim()) {
      setError('Tiêu đề không được để trống');
      return;
    }
    setError(null);
    setStep(2);
  };

  const handlePreview = async () => {
    const words = wordsInput
      .split(/[\n,]+/)
      .map((word) => word.trim())
      .filter((word) => word.length > 0);

    if (words.length === 0) {
      setError('Vui lòng nhập ít nhất một từ vựng');
      return;
    }
    
    setError(null);
    setLoading(true);
    try {
      const previewResult = await aiPreviewVocabularySet({ words, useAi });
      setPreviewList(previewResult);
      // Mặc định chọn tất cả
      setSelectedWordIndices(previewResult.map((_, i) => i));
      setStep(3);
    } catch (err: any) {
      setError(err.message || 'Lỗi khi lấy dữ liệu xem trước từ AI');
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

  const handleCreateSet = async () => {
    if (selectedWordIndices.length === 0) {
      setError('Vui lòng chọn ít nhất một từ vựng');
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
      setResult(createResult);
      setStep(4);
    } catch (err: any) {
      setError(err.message || 'Lỗi khi tạo bộ từ vựng qua AI');
      console.error('Create error:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="background-color min-h-screen text-white flex justify-center items-start py-10 mt-10">
      {loading && <LoadingScreen disableAudio={true} />}

      <div className={step === 3 ? "w-11/12 max-w-[95%]" : "w-7/12"}>
        <h1 className="text-3xl font-bold mb-6">Tạo bộ từ vựng (AI Hỗ trợ)</h1>

        {error && (
          <div className="mb-4 p-3 bg-red-500/20 border border-red-500 rounded-md">
            {error}
          </div>
        )}

        {/* BƯỚC 1: THÔNG TIN BỘ TỪ VỰNG */}
        {step === 1 && (
          <div className="flex flex-col gap-4 bg-gray-800 p-6 rounded-md border border-gray-700 shadow-xl">
            <h2 className="text-xl font-semibold border-b border-gray-700 pb-2">Bước 1: Thông tin bộ từ vựng</h2>
            
            <div>
              <label className="block text-sm font-medium mb-1">Tiêu đề</label>
              <input
                type="text"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="Nhập tiêu đề bộ từ vựng"
                className="w-full p-2 border rounded-md bg-gray-900 text-white focus:outline-none focus:ring-2 focus:ring-blue-500 border-gray-700"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Mô tả</label>
              <textarea
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Nhập mô tả (không bắt buộc)"
                className="w-full p-2 border rounded-md bg-gray-900 text-white focus:outline-none focus:ring-2 focus:ring-blue-500 border-gray-700"
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
                    <option key={key} value={value}>{key}</option>
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
                  <option value={difficultyValues.Beginner}>{difficultyValues.Beginner}</option>
                  <option value={difficultyValues.Intermediate}>{difficultyValues.Intermediate}</option>
                  <option value={difficultyValues.Advanced}>{difficultyValues.Advanced}</option>
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
            <div className="flex justify-between mt-4">
              <button
                onClick={() => setStep(1)}
                className="bg-gray-600 text-white px-6 py-2 rounded-md hover:bg-gray-500 font-medium transition-colors"
              >
                Quay lại
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
                {useAi ? (
                  <>
                    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" /></svg>
                    Phân tích bằng AI
                  </>
                ) : 'Tiếp tục'}
              </button>
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
                            <span className="font-bold text-white">{item.word}</span>
                            {isExisting && (
                              <span className="text-[10px] bg-green-900/50 text-green-400 border border-green-800 px-1 py-0.5 rounded-sm w-fit whitespace-nowrap">Đã có sẵn</span>
                            )}
                          </div>
                        </td>
                        <td className="p-2">
                          <input
                            type="text"
                            value={item.meaning}
                            onChange={(e) => handleEditPreviewData(index, 'meaning', e.target.value)}
                            disabled={!isSelected || isExisting}
                            className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 focus:ring-1 focus:ring-blue-500 focus:outline-none disabled:opacity-50 text-white"
                          />
                        </td>
                        <td className="p-2">
                          <input
                            type="text"
                            value={item.pronunciation}
                            onChange={(e) => handleEditPreviewData(index, 'pronunciation', e.target.value)}
                            disabled={!isSelected || isExisting}
                            className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 focus:ring-1 focus:ring-blue-500 focus:outline-none disabled:opacity-50 text-blue-300 font-mono text-xs"
                          />
                        </td>
                        <td className="p-2">
                          <input
                            type="text"
                            value={item.partOfSpeech}
                            onChange={(e) => handleEditPreviewData(index, 'partOfSpeech', e.target.value)}
                            disabled={!isSelected || isExisting}
                            className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 focus:ring-1 focus:ring-blue-500 focus:outline-none disabled:opacity-50 text-yellow-200 text-xs"
                          />
                        </td>
                        <td className="p-2">
                          <textarea
                            value={item.description}
                            onChange={(e) => handleEditPreviewData(index, 'description', e.target.value)}
                            disabled={!isSelected || isExisting}
                            rows={2}
                            className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 focus:ring-1 focus:ring-blue-500 focus:outline-none disabled:opacity-50 text-xs custom-scrollbar"
                          />
                        </td>
                        <td className="p-2">
                          <textarea
                            value={item.exampleSentence}
                            onChange={(e) => handleEditPreviewData(index, 'exampleSentence', e.target.value)}
                            disabled={!isSelected || isExisting}
                            rows={2}
                            className="w-full bg-gray-800 border border-gray-700 rounded px-2 py-1 focus:ring-1 focus:ring-blue-500 focus:outline-none disabled:opacity-50 text-xs custom-scrollbar italic"
                          />
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
            
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