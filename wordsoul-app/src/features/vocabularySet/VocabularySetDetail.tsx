/* eslint-disable @typescript-eslint/no-explicit-any */
import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import HeroSection from "../../components/HeroSection";
import { fetchVocabularySetDetail } from "../../services/vocabularySet";
import { createLearningSession } from "../../services/learningSession";
import ProfileCard from "../../components/UserProfile/ProfileCard";
import VocabularyList from "../../components/Vocabulary/VocabularyList";
import { getUserVocabularySets, registerVocabularySet } from "../../services/user";
import type { UserVocabularySetDto } from "../../types/UserDto";

interface VocabularySetMeta {
  id: number;
  title: string;
  description: string | null;
  theme: string;
  difficultyLevel: string;
  imageUrl?: string;
}

const VocabularySetDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [meta, setMeta] = useState<VocabularySetMeta | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const [userSetInfo, setUserSetInfo] = useState<UserVocabularySetDto | null>(null);
  const [showBackToTop, setShowBackToTop] = useState<boolean>(false);
  const navigate = useNavigate();

  const userId = 1; // TODO: lấy từ auth context hoặc localStorage

  const handleCreateLearningSession = async () => {
    setError(null);
    setIsLoading(true);
    try {
      const session = await createLearningSession(Number(id));
      navigate(`/learningSession/${session.id}?mode=learning`, {
        state: { petId: session.petId, catchRate: session.catchRate },
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
      if (userVocabSet) {
        setUserSetInfo(userVocabSet);
      }
    } catch (err: any) {
      setError(err?.message || "Lỗi đăng ký bộ từ vựng");
    } finally {
      setIsLoading(false);
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
    if (id && userId) fetchUserSetInfo();
  }, [userId, id]);

  // Theo dõi cuộn để hiển thị nút Back to Top
  useEffect(() => {
    const handleScroll = () => {
      setShowBackToTop(window.scrollY > 300);
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  // Hàm cuộn lên đầu trang
  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  if (loading) return <div className="text-center py-8">Loading set info...</div>;
  if (error) return <div className="text-center py-8 text-red-500">{error}</div>;
  if (!meta) return <div className="text-center py-8">No data available</div>;

  return (
    <>
      <HeroSection
        title={meta.title}
        description={meta.description || "No description available"}
        textButton="Bắt đầu"
        image={meta.imageUrl || "https://via.placeholder.com/600x200"}
        bottomImage="./src/assets/grass.gif"
        hidden={true}
      />

      <div className="background-color flex justify-center items-center py-4">
        <button
          className="w-full sm:w-1/5 max-w-xs px-2 py-1.5 bg-yellow-300 text-black rounded-xs hover:bg-yellow-200 custom-cursor"
          onClick={userSetInfo ? handleCreateLearningSession : handleRegisterSet}
          disabled={isLoading || (userSetInfo?.isCompleted ?? false)}
        >
          <span className="mx-1 text-xs font-bold font-sans">
            {userSetInfo
              ? userSetInfo.isCompleted
                ? "Đã hoàn thành"
                : "Học"
              : "Đăng ký"}
          </span>
        </button>
      </div>

      <div className="pixel2-background text-white min-h-screen w-full flex justify-center items-start px-4 sm:px-6 lg:px-8 py-6 sm:py-10 overflow-auto">
        <div className="w-full sm:w-10/12 lg:w-7/12 flex flex-col sm:flex-row items-start gap-4 sm:gap-5">
          {/* Bên trái */}
          <div className="w-full sm:w-8/12">
            <VocabularyList setId={meta.id} pageSize={5} />
          </div>

          {/* Bên phải */}
          <div className="w-full sm:w-4/12 flex flex-col gap-3">
            <div className="flex flex-col gap-3 items-center rounded-lg">
              <div className="w-full">
                <ProfileCard />
              </div>
            </div>
            <div className="flex flex-col gap-3 items-center border-2 border-white rounded-lg p-3">
              <div className="text-lg font-bold">Progress</div>
              {userSetInfo ? (
                <div className="text-sm">
                  <p>Số phiên đã hoàn thành: {userSetInfo.totalCompletedSession}</p>
                  <p>Ngày bắt đầu: {new Date(userSetInfo.createdAt).toLocaleDateString()}</p>
                  <p>Trạng thái: {userSetInfo.isCompleted ? "Hoàn thành" : "Chưa hoàn thành"}</p>
                </div>
              ) : (
                <p>Chưa đăng ký bộ từ vựng này</p>
              )}
            </div>
          </div>
        </div>

        {/* Nút Back to Top (chỉ hiển thị trên mobile) */}
        {showBackToTop && (
          <button
            onClick={scrollToTop}
            className="fixed bottom-4 right-4 sm:hidden bg-blue-500 text-white p-3 rounded-full shadow-lg hover:bg-blue-600 transition-opacity duration-300 z-50"
          >
            <svg
              className="w-6 h-6"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              xmlns="http://www.w3.org/2000/svg"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 10l7-7m0 0l7 7m-7-7v18" />
            </svg>
          </button>
        )}
      </div>
    </>
  );
};

export default VocabularySetDetail;