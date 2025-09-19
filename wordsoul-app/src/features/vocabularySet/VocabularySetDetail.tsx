import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import HeroSection from "../../components/HeroSection";
import { fetchVocabularySetDetail } from "../../services/vocabularySet";
import { createLearningSession } from "../../services/learningSession";
import ProfileCard from "../../components/UserProfile/ProfileCard";
import VocabularyList from "../../components/Vocabulary/VocabularyList";
import { getUserVocabularySets, registerVocabularySet } from "../../services/userService";
import type { UserVocabularySetDto } from "../../types/Dto";

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
  const navigate = useNavigate();

  const userId = 1; // TODO: lấy từ auth context hoặc localStorage

  const handleCreateLearningSession = async () => {
    setError(null);
    setIsLoading(true);
    try {
      const session = await createLearningSession(Number(id));
      navigate(`/learningSession/${session.id}?mode=learning`, {
        state: { petId: session.petId },
      });
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
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
        setUserSetInfo(userVocabSet); // Cập nhật userSetInfo sau khi đăng ký
      }
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
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
      setUserSetInfo(userSets); // userSets sẽ là null nếu không sở hữu
    };
    if (id && userId) fetchUserSetInfo();
  }, [userId, id]);

  if (loading) return <div>Loading set info...</div>;
  if (error) return <div>{error}</div>;
  if (!meta) return <div>No data available</div>;

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

      <div className="background-color flex justify-center items-center">
        <button
          className="relative flex items-center justify-center w-1/5 px-2 py-1.5 bg-yellow-300 text-black rounded-xs hover:bg-yellow-200 custom-cursor"
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

      <div className="pixel2-background text-white min-h-screen w-full flex justify-center items-start">
        <div className="w-7/12 flex items-start gap-5 py-10">
          {/* Bên trái */}
          <VocabularyList setId={meta.id} pageSize={5} />

          {/* Bên phải */}
          <div className="w-4/12 flex flex-col gap-3">
            <div className="flex flex-col gap-3 items-center rounded-lg">
              <div className="w-full">
                <ProfileCard />
              </div>
            </div>
            <div className="flex flex-col gap-3 items-center border-2 border-white rounded-lg p-3">
              <div className="text-lg font-bold">Progress</div>
              {userSetInfo ? (
                <div className="text-sm">
                  <p>Số phiên đã hoàn thành: {userSetInfo.totalCompletedSessions}</p>
                  <p>Ngày bắt đầu: {new Date(userSetInfo.createdAt).toLocaleDateString()}</p>
                  <p>Trạng thái: {userSetInfo.isCompleted ? "Hoàn thành" : "Chưa hoàn thành"}</p>
                </div>
              ) : (
                <p>Chưa đăng ký bộ từ vựng này</p>
              )}
            </div>
          </div>
        </div>
      </div>
    </>
  );
};

export default VocabularySetDetail;