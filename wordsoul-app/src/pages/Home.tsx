import { useState, useEffect } from 'react';
import Card from '../components/Card';
import HeroSection from '../components/HeroSection';
import { fetchVocabularySets } from '../services/vocabularySet';
import Skeleton from '../components/Skeleton';
import type { VocabularySetDto } from '../types/VocabularySetDto';
import OnboardingFlow from '../features/onboarding/OnboardingFlow';
import { useAuth } from '../hooks/Auth/useAuth';

const Home: React.FC = () => {
  const { user } = useAuth();
  const [vocabularySets, setVocabularySets] = useState<VocabularySetDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [showOnboarding, setShowOnboarding] = useState(false);

  useEffect(() => {
    setLoading(true);
    fetchVocabularySets()
      .then((data) => {
        setVocabularySets(data);
        setLoading(false);
      })
      .catch((error) => {
        setError(error.message || 'Failed to load vocabulary sets');
        setLoading(false);
        console.error('Error fetching vocabulary sets:', error);
      });
  }, []);

  const heroButtonProps = user
    ? {} // Logged-in users: button navigates to /register as usual (or can be hidden with hidden=true if desired)
    : { onButtonClick: () => setShowOnboarding(true) };

  if (loading) {
    return (
      <>
        <HeroSection
          title="Chào mừng bạn đến với Eralis"
          description="Hành trình giải mã những văn tự cố, giải thoát những sinh vật bí ẩn, xây dựng kiến thức lâu dài."
          textButton="Bắt đầu hành trình"
          height="29rem"
          hidden={false}
          {...heroButtonProps}
        />
        <div className="background-color text-white h-screen px-10 py-3">
          <Skeleton type="cards" />
        </div>
      </>
    );
  }
  if (error) {
    return (
      <>
        <HeroSection
          title="Chào mừng bạn đến với Eralis"
          description="Hành trình giải mã những văn tự cố, giải thoát những sinh vật bí ẩn, xây dựng kiến thức lâu dài."
          textButton="Bắt đầu hành trình"
          height="29rem"
          hidden={false}
          {...heroButtonProps}
        />
        <div className="background-color h-screen px-10 text-center py-8 text-red-500">
          {error}
        </div>
      </>
    );
  }

  return (
    <>
      <HeroSection
        title="Chào mừng bạn đến với Eralis"
        description="Hành trình giải mã những văn tự cố, giải thoát những sinh vật bí ẩn, xây dựng kiến thức lâu dài."
        textButton="Bắt đầu hành trình"
        height="35rem"
        hidden={false}
        {...heroButtonProps}
      />

      <div className="home-background-color text-white min-h-100 flex justify-center items-center">
        <div className="px-10 py-20 flex flex-col justify-center items-center w-full max-w-2xl">
          <div className="mb-5 text-2xl font-press w-2/3">
            All things you can learn
          </div>
          <p className="mb-5 text-lg font-extralight text-center">
            Hành trình giải mã những văn tự cố, giải thoát những sinh vật bí ẩn, xây dựng kiến thức lâu dài.
          </p>
        </div>
      </div>

      <div className="home-background-color text-white min-h-screen px-4 sm:px-6 lg:px-10 py-3 overflow-auto">
        <div className="container mx-auto w-full sm:w-10/12 lg:w-7/12 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {vocabularySets.map((vocabularySet) => (
            <Card
              key={vocabularySet.id}
              title={vocabularySet.title}
              description={vocabularySet.description || "Không có mô tả"}
              theme={vocabularySet.theme}
              difficultyLevel={vocabularySet.difficultyLevel}
              image={vocabularySet.imageUrl || ""}
              vocabularySetid={vocabularySet.id}
              isPublic={vocabularySet.isPublic}
              isOwned={vocabularySet.isOwned}
              createdByUsername={vocabularySet.createdByUsername || "Unknown"}
            />
          ))}
        </div>
      </div>

      {/* Onboarding full-screen overlay — triggered when guest presses "Bắt đầu" */}
      {showOnboarding && (
        <OnboardingFlow onClose={() => setShowOnboarding(false)} />
      )}
    </>
  );
};

export default Home;