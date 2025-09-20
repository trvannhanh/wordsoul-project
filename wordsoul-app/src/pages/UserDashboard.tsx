import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Particles from 'react-particles';
import { loadFull } from 'tsparticles';
import { createReviewSession } from '../services/learningSession';
import { getUserProgress } from '../services/user';
import ReviewBox from '../components/UserDashboard/ReviewBox';
import StatsChart from '../components/UserDashboard/StatsChart';
import ProfileCard from '../components/UserProfile/ProfileCard';
import type { UserProgressDto } from '../types/UserDto';


const UserDashboard: React.FC = () => {
  const [dashboard, setDashboard] = useState<UserProgressDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const particlesInit = async (main: any) => {
    await loadFull(main);
  };

  const handleCreateReviewSession = async () => {
    setError(null);
    setLoading(true);
    try {
      const session = await createReviewSession();
      navigate(`/learningSession/${session.id}?mode=review`);
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
          const level = i + 1;
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
        <div className="font-pokemon text-xl">Đang tải...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-black text-white h-screen flex items-center justify-center">
        <div className="font-pokemon text-red-500 text-xl">{error}</div>
      </div>
    );
  }

  return (
    <div className="pixel-background text-white h-screen font-pokemon relative overflow-hidden">
      <Particles
        id="tsparticles"
        init={particlesInit}
        options={{
          particles: {
            number: { value: 30 },
            size: { value: { min: 1, max: 3 } },
            move: { enable: true, speed: 1, direction: 'none', random: true },
            opacity: { value: { min: 0.2, max: 0.5 } },
            color: { value: '#FFD700' },
          },
          interactivity: { events: { onHover: { enable: false } } },
        }}
        className="absolute inset-0"
      />
      <div className="container mx-auto w-7/12 flex items-start gap-8 pt-20 pb-10 relative z-10">
        <div className="w-8/12 space-y-6">
          <ReviewBox
            progress={dashboard}
            loading={loading}
            onCreateReviewSession={handleCreateReviewSession}
          />
          <StatsChart progress={dashboard} />
        </div>
        <div className="w-4/12">
          <ProfileCard />
        </div>
      </div>
    </div>
  );
};

export default UserDashboard;