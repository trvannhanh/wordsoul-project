import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import StatCard from '../../components/UserProfile/StatCard';
import SrsStatsDashboard from '../../components/UserDashboard/SrsStatsDashboard';
import ThemeRadarChart from '../../components/UserDashboard/ThemeRadarChart';
import AchievementGrid from '../../components/Achievement/AchievementGrid';
import { useAuth } from '../../hooks/Auth/useAuth';
import { getUserProgress } from '../../services/user';
import type { UserProgressDto } from '../../types/UserDto';

type ProfileTab = 'info' | 'stats' | 'achievements';

const ProfilePage: React.FC = () => {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<ProfileTab>('info');
  const [progress, setProgress] = useState<UserProgressDto | null>(null);
  const [loadingProgress, setLoadingProgress] = useState<boolean>(false);

  useEffect(() => {
    if (!user) return;

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
          <div className="flex flex-col sm:flex-row items-center gap-6 pb-6 border-b border-gray-700">
            <motion.div
              className="w-24 h-24 flex-shrink-0"
              whileHover={{ scale: 1.1 }}
              transition={{ duration: 0.3 }}
            >
              <img
                src={
                  user.avatarUrl ??
                  'https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756453095/boy_c1k3lt.gif'
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
    </div>
  );
};

export default ProfilePage;