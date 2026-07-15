import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { createReviewSession } from '../services/learningSession';
import { getUserProgress } from '../services/user';
import ReviewBox from '../shared/components/UserDashboard/ReviewBox';
import SrsStatsDashboard from '../shared/components/UserDashboard/SrsStatsDashboard';
import StruggleWordsBox from '../shared/components/UserDashboard/StruggleWordsBox';
import ProfileCard from '../shared/components/UserProfile/ProfileCard';
import QuestList from '../shared/components/DailyQuest/QuestList';
import PronunciationWidget from '../shared/components/UserDashboard/PronunciationWidget';
import type { UserProgressDto } from '../types/UserDto';

const dashboardParticles = Array.from({ length: 20 }, (_, index) => ({
  id: index,
  left: `${(index * 37) % 100}%`,
  top: `${(index * 53) % 100}%`,
  size: 1 + (index % 2),
  delay: `${(index % 7) * 0.45}s`,
  duration: `${3 + (index % 5) * 0.35}s`,
}));

const UserDashboard: React.FC = () => {
  const [dashboard, setDashboard] = useState<UserProgressDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();


  const handleCreateReviewSession = async () => {
    setError(null);
    setLoading(true);
    try {
      const session = await createReviewSession();
      navigate(`/learningSession/${session.id}?mode=review`, {
        state: { currentCorrectAnswered: session.currentCorrectAnswered },
      });
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
    } catch (error: any) {
      setError(error?.response?.data?.message || 'Lỗi tạo phiên ôn tập');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const fetchData = async () => {
      try {
        const data = await getUserProgress();
        const filledStats = Array.from({ length: 5 }, (_, i) => {
          const level = i;
          const found = data.vocabularyStats.find((s) => s.level === level);
          return { level, count: found ? found.count : 0 };
        });
        setDashboard({ ...data, vocabularyStats: filledStats });
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
      } catch (err) {
        setError('Không thể tải dữ liệu tiến trình');
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  if (loading) {
    return (
      <div className="bg-black text-white h-screen flex items-center justify-center">
        <div className="font-pixel text-xl">Đang tải...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-black text-white h-screen flex items-center justify-center">
        <div className="font-pixel text-red-500 text-xl">{error}</div>
      </div>
    );
  }

  return (
    <div className="review-box-background bg-fixed text-white min-h-screen font-pixel relative overflow-auto">
      <div className="pointer-events-none absolute inset-0 overflow-hidden">
        {dashboardParticles.map((particle) => (
          <span
            key={particle.id}
            className="absolute rounded-full bg-yellow-300/70 shadow-[0_0_8px_rgba(250,204,21,0.7)] animate-pulse"
            style={{
              left: particle.left,
              top: particle.top,
              width: particle.size,
              height: particle.size,
              animationDelay: particle.delay,
              animationDuration: particle.duration,
            }}
          />
        ))}
      </div>      <div className="container mx-auto w-full sm:w-10/12 lg:w-8/12 xl:w-7/12 flex flex-col sm:flex-row items-start gap-6 sm:gap-8 pt-16 sm:pt-20 pb-6 sm:pb-10 relative z-10">
        {/* Cột trái: Review, Biểu đồ thống kê và Từ vựng cần rèn luyện */}
        <div className="w-full sm:w-7/12 space-y-6">
          <ReviewBox
            progress={dashboard}
            loading={loading}
            onCreateReviewSession={handleCreateReviewSession}
          />
          <PronunciationWidget />
          <SrsStatsDashboard progress={dashboard} />
          {dashboard && (
            <StruggleWordsBox
              struggleWords={dashboard.struggleWords || []}
              onCreateReviewSession={handleCreateReviewSession}
            />
          )}
        </div>

        {/* Cột phải: Profile Card, Quests */}
        <div className="w-full sm:w-5/12 space-y-6">
          <ProfileCard />
          <QuestList />
        </div>
      </div>
    </div>
  );
};

export default UserDashboard;