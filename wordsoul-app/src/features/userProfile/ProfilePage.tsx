import React, { useEffect, useState } from 'react';
import { isAxiosError } from 'axios';
import { Link } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import StatCard from '../../shared/components/UserProfile/StatCard';
import SrsStatsDashboard from '../../shared/components/UserDashboard/SrsStatsDashboard';
import ThemeRadarChart from '../../shared/components/UserDashboard/ThemeRadarChart';
import AchievementGrid from '../../shared/components/Achievement/AchievementGrid';
import { useAuth } from '../../hooks/Auth/useAuth';
import { getUserProgress, updateUserProfile, uploadUserAvatar } from '../../services/user';
import type { UserProgressDto, UserDto } from '../../types/UserDto';

type ProfileTab = 'info' | 'stats' | 'achievements';
type AvatarType = 'default' | 'upload' | 'pet';

const DEFAULT_AVATARS = [
  { id: 'evee', name: 'Eevee', url: 'https://res.cloudinary.com/dqpkxxzaf/image/upload/v1779778116/701889416_1310365154555281_7941044098437819807_n_naesig.jpg' },
  { id: 'gengar', name: 'Gengar', url: 'https://res.cloudinary.com/dqpkxxzaf/image/upload/v1779778116/701581874_1310365194555277_7627812307086915054_n_wzzgzd.jpg' },
  { id: 'porygon', name: 'Porygon', url: 'https://res.cloudinary.com/dqpkxxzaf/image/upload/v1779778115/701538282_1310365321221931_2191742099393852665_n_fprtin.jpg' },
  { id: 'margikarp', name: 'Magikarp', url: 'https://res.cloudinary.com/dqpkxxzaf/image/upload/v1779778115/700235700_1310365191221944_5607345117887374664_n_el7afj.jpg' },
  { id: 'magnemite', name: 'Magnemite', url: 'https://res.cloudinary.com/dqpkxxzaf/image/upload/v1779778115/700971038_1310365187888611_6763571970404301036_n_frlhhr.jpg' },
  { id: 'mew', name: 'Mew', url: 'https://res.cloudinary.com/dqpkxxzaf/image/upload/v1779778115/700762888_1310365111221952_5462810914521868195_n_a6mb2l.jpg' },
  { id: 'sandshrew', name: 'Sandshrew', url: 'https://res.cloudinary.com/dqpkxxzaf/image/upload/v1779778114/700224246_1310365231221940_4512420955309024739_n_o6hvim.jpg' },
  { id: 'mewtwo', name: 'Mewtwo', url: 'https://res.cloudinary.com/dqpkxxzaf/image/upload/v1779778114/702525314_1310365281221935_8878335436240873076_n_esjy0g.jpg' },
  { id: 'jigglypuff', name: 'Jigglypuff', url: 'https://res.cloudinary.com/dqpkxxzaf/image/upload/v1779778114/702134182_1310365394555257_8894057767693200629_n_tytccw.jpg' }
];

const ProfilePage: React.FC = () => {
  const { user, setUser } = useAuth();
  const [activeTab, setActiveTab] = useState<ProfileTab>('info');
  const [progress, setProgress] = useState<UserProgressDto | null>(null);
  const [loadingProgress, setLoadingProgress] = useState<boolean>(false);

  // Edit profile states
  const [isEditModalOpen, setIsEditModalOpen] = useState<boolean>(false);
  const [newUsername, setNewUsername] = useState<string>('');
  const [selectedAvatarType, setSelectedAvatarType] = useState<AvatarType>('default');
  const [selectedDefaultAvatarUrl, setSelectedDefaultAvatarUrl] = useState<string>(DEFAULT_AVATARS[0].url);
  const [uploadFile, setUploadFile] = useState<File | null>(null);
  const [uploadPreview, setUploadPreview] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState<boolean>(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  useEffect(() => {
    if (!user) return;

    // Prefill form values
    setNewUsername(user.username);

    // Determine avatar type
    const isDefault = DEFAULT_AVATARS.some(av => av.url === user.avatarUrl);
    if (isDefault && user.avatarUrl) {
      setSelectedAvatarType('default');
      setSelectedDefaultAvatarUrl(user.avatarUrl);
    } else if (user.avatarUrl) {
      // It's a custom avatar URL
      setSelectedAvatarType('upload');
      setUploadPreview(user.avatarUrl);
    } else {
      // No custom avatar, falls back to pet
      setSelectedAvatarType('pet');
    }

    const fetchProgress = async () => {
      setLoadingProgress(true);
      try {
        const data = await getUserProgress();
        const filledStats = Array.from({ length: 5 }, (_, i) => {
          const level = i;
          const found = data.vocabularyStats.find((s) => s.level === level);
          return { level, count: found ? found.count : 0 };
        });
        setProgress({ ...data, vocabularyStats: filledStats });
      } catch (err) {
        console.error('Failed to load user progress for profile page:', err);
      } finally {
        setLoadingProgress(false);
      }
    };

    fetchProgress();
  }, [user]);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const file = e.target.files[0];
      if (file.size > 5 * 1024 * 1024) {
        setErrorMsg('Kích thước ảnh không được vượt quá 5MB.');
        return;
      }
      setUploadFile(file);
      setUploadPreview(URL.createObjectURL(file));
      setErrorMsg(null);
    }
  };

  const handleSaveProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) return;

    if (!newUsername.trim()) {
      setErrorMsg('Tên Trainer không được để trống.');
      return;
    }

    setIsSaving(true);
    setErrorMsg(null);
    setSuccessMsg(null);

    try {
      let finalAvatarUrl = user.avatarUrl;

      // Handle avatar updates
      if (selectedAvatarType === 'upload') {
        if (uploadFile) {
          const uploadRes = await uploadUserAvatar(uploadFile);
          finalAvatarUrl = uploadRes.avatarUrl;
        } else {
          finalAvatarUrl = uploadPreview || undefined;
        }
      } else if (selectedAvatarType === 'default') {
        finalAvatarUrl = selectedDefaultAvatarUrl;
      } else if (selectedAvatarType === 'pet') {
        finalAvatarUrl = 'clear'; // triggers fallback on backend
      }

      // Update full profile
      const updatedUser = await updateUserProfile(user.id, {
        username: newUsername.trim(),
        avatarUrl: finalAvatarUrl
      });

      // Update auth context state
      setUser(updatedUser as UserDto);
      setSuccessMsg('Cập nhật hồ sơ thành công! ✨');
      setTimeout(() => {
        setIsEditModalOpen(false);
        setSuccessMsg(null);
      }, 1500);
    } catch (err: unknown) {
      console.error('Error saving profile:', err);
      const responseMessage = isAxiosError<{ message?: string }>(err)
        ? err.response?.data?.message
        : undefined;
      const message = err instanceof Error ? err.message : undefined;
      setErrorMsg(responseMessage || message || 'Có lỗi xảy ra khi lưu hồ sơ.');
    } finally {
      setIsSaving(false);
    }
  };

  if (!user) {
    return (
      <div className="review-box-background font-pixel text-white min-h-screen flex items-center justify-center">
        <div className="text-center text-red-500">Please log in to view your profile.</div>
      </div>
    );
  }

  return (
    <div className="review-box-background font-pixel text-color min-h-screen w-full flex justify-center items-start pt-20 pb-12">
      <div className="container mx-auto p-4 w-full sm:w-10/12 lg:w-8/12 xl:w-7/12">
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
          className="background-color pixel-border rounded-xl p-6 shadow-lg space-y-6"
        >
          {/* Header with Avatar and Username */}
          <div className="flex flex-col sm:flex-row items-center gap-6 pb-6 border-b border-gray-700 w-full justify-between">
            <div className="flex flex-col sm:flex-row items-center gap-6">
              <motion.div
                className="w-24 h-24 flex-shrink-0"
                whileHover={{ scale: 1.1 }}
                transition={{ duration: 0.3 }}
              >
                <img
                  src={
                    user.avatarUrl ??
                    'https://res.cloudinary.com/dqpkxxzaf/image/upload/v1779778616/subtitute_jg49qb.jpg'
                  }
                  alt="avatar"
                  className="w-full h-full object-cover pixelated rounded-md border-2 border-yellow-300 shadow-md"
                />
              </motion.div>
              <div className="text-center sm:text-left">
                <h1 className="text-3xl font-pixel text-yellow-500 tracking-wide drop-shadow-md">
                  {user.username}
                </h1>
                <div className="text-base text-gray-300 mt-1">Trainer Cấp {user.level}</div>
                <div className="text-xs text-gray-400 mt-0.5">{user.email}</div>
              </div>
            </div>

            <motion.button
              onClick={() => {
                setErrorMsg(null);
                setSuccessMsg(null);
                setNewUsername(user.username);
                setIsEditModalOpen(true);
              }}
              className="px-4 py-2 font-pixel text-xs bg-yellow-400 hover:bg-yellow-300 text-black rounded border-2 border-yellow-500 hover:shadow-[0_0_8px_rgba(234,179,8,0.5)] transition-all custom-cursor"
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
            >
              CHỈNH SỬA HỒ SƠ
            </motion.button>
          </div>

          {/* Retro Pixel Tab Selector */}
          <div className="flex gap-1.5 border-b border-gray-700 pb-3 flex-wrap">
            <button
              onClick={() => setActiveTab('info')}
              className={`px-4 py-2 font-pixel text-xs sm:text-sm rounded border transition-all ${activeTab === 'info'
                ? 'bg-blue-600 border-blue-500 text-white shadow-[0_0_8px_rgba(37,99,235,0.4)]'
                : 'bg-transparent border-gray-600 text-gray-400 hover:border-gray-400 hover:text-white custom-cursor'
                }`}
            >
              HỒ SƠ
            </button>
            <button
              onClick={() => setActiveTab('stats')}
              className={`px-4 py-2 font-pixel text-xs sm:text-sm rounded border transition-all ${activeTab === 'stats'
                ? 'bg-blue-600 border-blue-500 text-white shadow-[0_0_8px_rgba(37,99,235,0.4)]'
                : 'bg-transparent border-gray-600 text-gray-400 hover:border-gray-400 hover:text-white custom-cursor'
                }`}
            >
              THỐNG KÊ
            </button>
            <button
              onClick={() => setActiveTab('achievements')}
              className={`px-4 py-2 font-pixel text-xs sm:text-sm rounded border transition-all ${activeTab === 'achievements'
                ? 'bg-blue-600 border-blue-500 text-white shadow-[0_0_8px_rgba(37,99,235,0.4)]'
                : 'bg-transparent border-gray-600 text-gray-400 hover:border-gray-400 hover:text-white custom-cursor'
                }`}
            >
              THÀNH TỰU
            </button>
          </div>

          {/* Tab Content Areas */}
          <div>
            {/* 1. Trainer Info Tab */}
            {activeTab === 'info' && (
              <motion.div
                initial={{ opacity: 0, x: -10 }}
                animate={{ opacity: 1, x: 0 }}
                className="space-y-6"
              >
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="background-color bg-opacity-80 p-4 rounded-md border border-gray-700">
                    <h2 className="text-lg font-pixel text-blue-400 mb-3 border-b border-gray-700 pb-1.5">
                      Chi tiết tài khoản
                    </h2>
                    <div className="text-xs space-y-2 leading-relaxed">
                      <p>
                        <span className="font-bold text-gray-400">Vai trò:</span>{' '}
                        {user.role.charAt(0).toUpperCase() + user.role.slice(1)}
                      </p>
                      <p>
                        <span className="font-bold text-gray-400">Ngày tham gia:</span>{' '}
                        {new Date(user.createdAt).toLocaleDateString()}
                      </p>
                      <p>
                        <span className="font-bold text-gray-400">Trạng thái:</span>{' '}
                        <span className={user.isActive ? 'text-green-400' : 'text-red-400'}>
                          {user.isActive ? 'Đang hoạt động' : 'Vô hiệu hóa'}
                        </span>
                      </p>
                    </div>
                  </div>

                  <div className="background-color bg-opacity-80 p-4 rounded-md border border-gray-700">
                    <h2 className="text-lg font-pixel text-blue-400 mb-3 border-b border-gray-700 pb-1.5">
                      Chỉ số Trainer
                    </h2>
                    <div className="grid grid-cols-2 gap-2">
                      <StatCard label="Tổng kinh nghiệm" value={user.totalXP} />
                      <StatCard label="Tổng điểm" value={user.totalAP} />
                      <StatCard label="Thú cưng" value={user.petCount ?? 0} />
                      <StatCard label="Chuỗi ngày học" value={user.streakDays} />
                    </div>
                  </div>
                </div>

                {/* Navigation shortcuts */}
                <div className="flex justify-center gap-4 pt-2">
                  <Link to="/community" className="no-underline">
                    <motion.button
                      className="px-5 py-2.5 bg-blue-600 hover:bg-blue-500 text-white font-pixel text-xs rounded border border-blue-500 hover:shadow-lg custom-cursor"
                      whileHover={{ scale: 1.05 }}
                      whileTap={{ scale: 0.95 }}
                    >
                      BẢNG XẾP HẠNG
                    </motion.button>
                  </Link>
                  <Link to="/pets" className="no-underline">
                    <motion.button
                      className="px-5 py-2.5 bg-green-600 hover:bg-green-500 text-white font-pixel text-xs rounded border border-green-500 hover:shadow-lg custom-cursor"
                      whileHover={{ scale: 1.05 }}
                      whileTap={{ scale: 0.95 }}
                    >
                      TẤT CẢ THÚ CƯNG
                    </motion.button>
                  </Link>
                </div>
              </motion.div>
            )}

            {/* 2. Analytics & Progress Tab */}
            {activeTab === 'stats' && (
              <motion.div
                initial={{ opacity: 0, x: -10 }}
                animate={{ opacity: 1, x: 0 }}
                className="space-y-6"
              >
                {loadingProgress ? (
                  <div className="text-center py-12 font-pixel text-xs text-gray-400 animate-pulse">
                    Đang tải dữ liệu tiến trình...
                  </div>
                ) : (
                  <div className="space-y-6">
                    <SrsStatsDashboard progress={progress} />
                    {progress?.themePreferences && progress.themePreferences.length > 0 ? (
                      <ThemeRadarChart preferences={progress.themePreferences} />
                    ) : (
                      <div className="pokemon-background pixel-border rounded-xl p-6">
                        <h3 className="font-pixel text-xl mb-3 text-yellow-300">🗺️ Sở thích chủ đề</h3>
                        <p className="font-pixel text-gray-400 text-xs text-center py-6">
                          Hãy hoàn thành các phiên học để cập nhật biểu đồ sở thích chủ đề! ✨
                        </p>
                      </div>
                    )}
                  </div>
                )}
              </motion.div>
            )}

            {/* 3. Achievements Tab */}
            {activeTab === 'achievements' && (
              <motion.div
                initial={{ opacity: 0, x: -10 }}
                animate={{ opacity: 1, x: 0 }}
              >
                <AchievementGrid />
              </motion.div>
            )}
          </div>
        </motion.div>
      </div>

      {/* Edit Profile Modal */}
      <AnimatePresence>
        {isEditModalOpen && (
          <div className="fixed inset-0 bg-black bg-opacity-80 flex items-center justify-center z-50 p-4 font-pixel">
            <motion.div
              initial={{ scale: 0.9, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0.9, opacity: 0 }}
              className="bg-[#181a21] border-4 border-double border-yellow-400 rounded-xl p-6 w-full max-w-lg shadow-[0_0_20px_rgba(234,179,8,0.3)] text-white relative max-h-[90vh] overflow-y-auto"
            >
              <div className="text-center mb-6">
                <h2 className="text-xl text-yellow-400 tracking-wider font-bold">
                  CHỈNH SỬA HỒ SƠ TRAINER
                </h2>
                <div className="w-12 h-1 bg-yellow-400 mx-auto mt-2 rounded"></div>
              </div>

              <form onSubmit={handleSaveProfile} className="space-y-5">
                {/* Error/Success alerts */}
                {errorMsg && (
                  <div className="p-3 bg-red-950 border border-red-500 text-red-400 rounded text-xs">
                    ⚠️ {errorMsg}
                  </div>
                )}
                {successMsg && (
                  <div className="p-3 bg-green-950 border border-green-500 text-green-400 rounded text-xs">
                    {successMsg}
                  </div>
                )}

                {/* Username Input */}
                <div className="space-y-1.5">
                  <label className="text-xs text-yellow-300 font-bold block">TÊN TRAINER</label>
                  <input
                    type="text"
                    value={newUsername}
                    onChange={(e) => setNewUsername(e.target.value)}
                    maxLength={50}
                    className="w-full text-black px-3 py-2 rounded border-2 border-gray-700 bg-white focus:border-yellow-400 outline-none text-sm font-pixel"
                    placeholder="Nhập tên Trainer..."
                    disabled={isSaving}
                  />
                  <p className="text-[10px] text-gray-400">Tên của bạn sẽ được hiển thị trên bảng xếp hạng và các trận đấu PvP.</p>
                </div>

                {/* Avatar Section */}
                <div className="space-y-2">
                  <label className="text-xs text-yellow-300 font-bold block">ẢNH ĐẠI DIỆN</label>

                  {/* Source selector */}
                  <div className="flex gap-1 bg-gray-900 p-1 rounded-md border border-gray-800">
                    <button
                      type="button"
                      onClick={() => setSelectedAvatarType('default')}
                      className={`flex-1 py-1.5 text-center rounded text-[10px] sm:text-xs transition-all ${selectedAvatarType === 'default'
                        ? 'bg-yellow-400 text-black font-bold shadow-md'
                        : 'bg-transparent text-gray-400 hover:text-white'
                        }`}
                      disabled={isSaving}
                    >
                      Mặc định
                    </button>
                    <button
                      type="button"
                      onClick={() => setSelectedAvatarType('upload')}
                      className={`flex-1 py-1.5 text-center rounded text-[10px] sm:text-xs transition-all ${selectedAvatarType === 'upload'
                        ? 'bg-yellow-400 text-black font-bold shadow-md'
                        : 'bg-transparent text-gray-400 hover:text-white'
                        }`}
                      disabled={isSaving}
                    >
                      Tải ảnh lên
                    </button>
                    <button
                      type="button"
                      onClick={() => setSelectedAvatarType('pet')}
                      className={`flex-1 py-1.5 text-center rounded text-[10px] sm:text-xs transition-all ${selectedAvatarType === 'pet'
                        ? 'bg-yellow-400 text-black font-bold shadow-md'
                        : 'bg-transparent text-gray-400 hover:text-white'
                        }`}
                      disabled={isSaving}
                    >
                      Pokémon Active
                    </button>
                  </div>

                  {/* 1. Default Avatars Grid */}
                  {selectedAvatarType === 'default' && (
                    <div className="grid grid-cols-3 gap-3 p-3 bg-gray-900 rounded-md border border-gray-800">
                      {DEFAULT_AVATARS.map((av) => (
                        <div
                          key={av.id}
                          onClick={() => {
                            if (!isSaving) setSelectedDefaultAvatarUrl(av.url);
                          }}
                          className={`cursor-pointer p-2 border-2 rounded flex flex-col items-center justify-center transition-all bg-gray-800 hover:bg-gray-700 ${selectedDefaultAvatarUrl === av.url
                            ? 'border-yellow-400 shadow-[0_0_8px_rgba(234,179,8,0.4)] bg-gray-700'
                            : 'border-transparent'
                            }`}
                        >
                          <img src={av.url} alt={av.name} className="w-12 h-12 object-cover pixelated rounded mb-1" />
                          <span className="text-[8px] text-gray-300 text-center leading-tight truncate w-full">{av.name}</span>
                        </div>
                      ))}
                    </div>
                  )}

                  {/* 2. Upload Custom Image */}
                  {selectedAvatarType === 'upload' && (
                    <div className="flex flex-col items-center justify-center border-2 border-dashed border-gray-700 rounded-md p-4 bg-gray-900">
                      <input
                        type="file"
                        accept="image/*"
                        onChange={handleFileChange}
                        className="hidden"
                        id="avatar-upload-file"
                        disabled={isSaving}
                      />
                      <label
                        htmlFor="avatar-upload-file"
                        className="cursor-pointer px-3 py-2 bg-blue-600 hover:bg-blue-500 font-pixel text-[10px] sm:text-xs rounded border border-blue-500 hover:shadow-lg transition-all custom-cursor mb-2 text-white text-center"
                      >
                        CHỌN ẢNH TỪ MÁY
                      </label>
                      {uploadPreview ? (
                        <div className="text-center">
                          <img src={uploadPreview} alt="Preview" className="w-16 h-16 object-cover rounded border-2 border-yellow-400 pixelated mx-auto" />
                          <p className="text-[9px] text-gray-400 mt-1">Ảnh xem trước</p>
                        </div>
                      ) : (
                        <p className="text-[9px] text-gray-400">PNG, JPG, GIF (Tối đa 5MB)</p>
                      )}
                    </div>
                  )}

                  {/* 3. Pokémon Active Fallback */}
                  {selectedAvatarType === 'pet' && (
                    <div className="flex flex-col items-center justify-center border-2 border-gray-800 rounded-md p-4 bg-gray-900 text-center space-y-1.5">
                      <span className="text-2xl animate-bounce">🐾</span>
                      <p className="text-[10px] text-gray-300 leading-normal max-w-xs mx-auto">
                        Ảnh đại diện sẽ tự động đồng bộ theo Pokémon đang mang theo của bạn.
                      </p>
                    </div>
                  )}
                </div>

                {/* Footer buttons */}
                <div className="flex justify-end gap-3 pt-3 border-t border-gray-800">
                  <button
                    type="button"
                    onClick={() => {
                      if (!isSaving) setIsEditModalOpen(false);
                    }}
                    className="px-4 py-2 border border-gray-700 bg-transparent text-gray-400 hover:text-white rounded text-xs transition-all custom-cursor"
                    disabled={isSaving}
                  >
                    HỦY
                  </button>
                  <button
                    type="submit"
                    className="px-4 py-2 bg-yellow-400 hover:bg-yellow-300 text-black rounded border border-yellow-500 font-bold text-xs hover:shadow-[0_0_8px_rgba(234,179,8,0.5)] transition-all flex items-center justify-center custom-cursor"
                    disabled={isSaving}
                  >
                    {isSaving ? (
                      <span className="flex items-center gap-1.5">
                        <svg className="animate-spin h-3 w-3 text-black" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                        </svg>
                        ĐANG LƯU...
                      </span>
                    ) : (
                      'LƯU THAY ĐỔI'
                    )}
                  </button>
                </div>
              </form>
            </motion.div>
          </div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default ProfilePage;