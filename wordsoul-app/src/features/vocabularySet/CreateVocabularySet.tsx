
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { createVocabularySet } from '../../services/vocabularySet';
import { searchVocabularies } from '../../services/vocabulary';
import { VocabularySetThemeEnum as themeValues } from '../../types/VocabularySetDto';
import {  VocabularyDifficultyLevelEnum as difficultyValues } from '../../types/VocabularyDto';
import type { SearchVocabularyDto, VocabularyDifficultyLevelEnum, VocabularyDto } from '../../types/VocabularyDto';
import type { VocabularySetThemeEnum } from '../../types/VocabularySetDto';

const CreateVocabularySet: React.FC = () => {
  const navigate = useNavigate();
  const [step, setStep] = useState<1 | 2>(1);
  const [wordsInput, setWordsInput] = useState<string>('');
  const [vocabularies, setVocabularies] = useState<VocabularyDto[]>([]);
  const [selectedVocabularyIds, setSelectedVocabularyIds] = useState<number[]>([]);
  const [title, setTitle] = useState<string>('');
  const [description, setDescription] = useState<string>('');
  const [theme, setTheme] = useState<VocabularySetThemeEnum>(themeValues.DailyLearning);
  const [difficultyLevel, setDifficultyLevel] = useState<VocabularyDifficultyLevelEnum>(difficultyValues.Beginner);
  const [isPublic, setIsPublic] = useState<boolean>(false);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Xử lý tìm kiếm từ vựng
  const handleSearchVocabularies = async () => {
    setError(null);
    setLoading(true);
    try {
      const words = wordsInput
        .split(/[\n,]+/)
        .map((word) => word.trim())
        .filter((word) => word.length > 0);
      if (words.length === 0) {
        setError('Vui lòng nhập ít nhất một từ');
        return;
      }
      const searchDto: SearchVocabularyDto = { words };
      const results = await searchVocabularies(searchDto);
      setVocabularies(results);
      setSelectedVocabularyIds(results.map((vocab) => vocab.id));
      setStep(2);
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (err: any) {
      setError(err.message || 'Lỗi khi tìm kiếm từ vựng');
      console.error('Search error:', err);
    } finally {
      setLoading(false);
    }
  };

  // Xử lý chọn/bỏ chọn từ vựng
  const handleToggleVocabulary = (id: number) => {
    setSelectedVocabularyIds((prev) =>
      prev.includes(id) ? prev.filter((vid) => vid !== id) : [...prev, id]
    );
  };

  // Xử lý upload file ảnh
  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] || null;
    setImageFile(file);
  };

  // Xử lý tạo bộ từ vựng
  const handleCreateSet = async () => {
    setError(null);
    setLoading(true);
    try {
      if (!title.trim()) {
        setError('Tiêu đề không được để trống');
        return;
      }
      if (selectedVocabularyIds.length === 0) {
        setError('Vui lòng chọn ít nhất một từ vựng');
        return;
      }

      const formData = new FormData();
      formData.append('Title', title);
      formData.append('Description', description || '');
      formData.append('Theme', theme.toString()); // Gửi giá trị số
      formData.append('DifficultyLevel', difficultyLevel.toString()); // Gửi giá trị số
      formData.append('IsPublic', isPublic.toString());
      formData.append('IsActive', 'true');
      selectedVocabularyIds.forEach((id) => {
        formData.append('VocabularyIds', id.toString());
      });
      if (imageFile) {
        formData.append('ImageFile', imageFile);
      }

      const newSet = await createVocabularySet(formData);
      if (newSet && newSet.id) {
        navigate('/vocabulary-sets');
      } else {
        throw new Error('Tạo bộ từ vựng thất bại, không nhận được ID');
      }
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (err: any) {
      setError(err.message || 'Lỗi khi tạo bộ từ vựng');
      console.error('Create error:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="background-color min-h-screen text-white flex justify-center items-start py-10 mt-10">
      <div className="w-7/12">
        <h1 className="text-3xl font-bold mb-6">Tạo bộ từ vựng mới</h1>

        {error && (
          <div className="mb-4 p-3 bg-red-500/20 border border-red-500 rounded-md">
            {error}
          </div>
        )}

        {step === 1 ? (
          <div className="flex flex-col gap-4">
            <h2 className="text-xl font-semibold">Bước 1: Tìm kiếm từ vựng</h2>
            <textarea
              value={wordsInput}
              onChange={(e) => setWordsInput(e.target.value)}
              placeholder="Nhập danh sách từ vựng (cách nhau bởi dấu phẩy hoặc dòng mới, ví dụ: apple, banana hoặc apple\nbanana)"
              className="w-full p-3 border rounded-md bg-gray-800 text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
              rows={5}
            />
            <button
              onClick={handleSearchVocabularies}
              disabled={loading}
              className="bg-blue-500 text-white px-4 py-2 rounded-md hover:bg-blue-600 disabled:bg-blue-300"
            >
              {loading ? 'Đang tìm kiếm...' : 'Tiếp tục'}
            </button>
          </div>
        ) : (
          <div className="flex flex-col gap-6">
            <h2 className="text-xl font-semibold">Bước 2: Nhập thông tin bộ từ vựng</h2>

            {/* Danh sách từ vựng tìm thấy */}
            <div className="border rounded-md p-4 bg-gray-800">
              <h3 className="text-lg font-semibold mb-2">Từ vựng tìm thấy</h3>
              {vocabularies.length === 0 ? (
                <p className="text-gray-500">Không tìm thấy từ vựng nào</p>
              ) : (
                <div className="grid grid-cols-1 gap-2 max-h-64 overflow-y-auto">
                  {vocabularies.map((vocab) => (
                    <div
                      key={vocab.id}
                      className="flex items-center gap-2 p-2 border rounded-md bg-gray-700"
                    >
                      <input
                        type="checkbox"
                        checked={selectedVocabularyIds.includes(vocab.id)}
                        onChange={() => handleToggleVocabulary(vocab.id)}
                        className="h-5 w-5 text-blue-500"
                      />
                      <div>
                        <p className="font-semibold">{vocab.word}</p>
                        <p className="text-sm text-gray-300">Nghĩa: {vocab.meaning}</p>
                        <p className="text-sm text-gray-300">Ví dụ: {vocab.example}</p>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* Form thông tin bộ từ vựng */}
            <div className="flex flex-col gap-4">
              <div>
                <label className="block text-sm font-medium mb-1">Tiêu đề</label>
                <input
                  type="text"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  placeholder="Nhập tiêu đề bộ từ vựng"
                  className="w-full p-2 border rounded-md bg-gray-800 text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Mô tả</label>
                <textarea
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  placeholder="Nhập mô tả (không bắt buộc)"
                  className="w-full p-2 border rounded-md bg-gray-800 text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                  rows={3}
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Ảnh (không bắt buộc)</label>
                <input
                  type="file"
                  accept="image/*"
                  onChange={handleImageChange}
                  className="w-full p-2 border rounded-md bg-gray-800 text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Chủ đề</label>
                <select
                  value={theme}
                  onChange={(e) => setTheme(Number(e.target.value) as VocabularySetThemeEnum)}
                  className="w-full p-2 border rounded-md bg-gray-800 text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value={themeValues.DailyLearning}>{themeValues.DailyLearning}</option>
                  <option value={themeValues.AdvancedTopics}>{themeValues.AdvancedTopics}</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Độ khó</label>
                <select
                  value={difficultyLevel}
                  onChange={(e) => setDifficultyLevel(Number(e.target.value) as VocabularyDifficultyLevelEnum)}
                  className="w-full p-2 border rounded-md bg-gray-800 text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value={difficultyValues.Beginner}>{difficultyValues.Beginner}</option>
                  <option value={difficultyValues.Intermediate}>{difficultyValues.Intermediate}</option>
                  <option value={difficultyValues.Advanced}>{difficultyValues.Advanced}</option>
                </select>
              </div>
              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  checked={isPublic}
                  onChange={(e) => setIsPublic(e.target.checked)}
                  className="h-5 w-5 text-blue-500"
                />
                <label className="text-sm font-medium">Công khai</label>
              </div>
              <div className="flex gap-4">
                <button
                  onClick={() => setStep(1)}
                  className="bg-gray-500 text-white px-4 py-2 rounded-md hover:bg-gray-600"
                >
                  Quay lại
                </button>
                <button
                  onClick={handleCreateSet}
                  disabled={loading}
                  className="bg-blue-500 text-white px-4 py-2 rounded-md hover:bg-blue-600 disabled:bg-blue-300"
                >
                  {loading ? 'Đang tạo...' : 'Tạo'}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default CreateVocabularySet;