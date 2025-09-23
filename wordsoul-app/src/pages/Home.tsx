import { useState, useEffect } from 'react';
import Card from '../components/Card';
import HeroSection from '../components/HeroSection';
import { fetchVocabularySets } from '../services/vocabularySet';
import Skeleton from '../components/Skeleton';
import type { VocabularySetDto } from '../types/VocabularySetDto';




const Home: React.FC = () => {
  const [vocabularySets, setVocabularySets] = useState<VocabularySetDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

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

  if (loading) {
    return (
      <>
        <HeroSection
          title="Chào mừng bạn đến với Eralis"
          description="Hành trình giải mã những văn tự cố, giải thoát những sinh vật bí ẩn, xây dựng kiến thức lâu dài."
          textButton="Bắt đầu"
          image="./src/assets/thumb.gif"
          bottomImage="./src/assets/grass.gif"
          height="29rem"
          hidden={false}
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
          textButton="Bắt đầu"
          image="./src/assets/thumb.gif"
          bottomImage="./src/assets/grass.gif"
          height="29rem"
          hidden={false}
        />
        <div className="background-color h-screen px-10 text-center py-8 text-red-500">
          {error}  // Hoặc thêm nút retry: <button onClick={() => window.location.reload()}>Retry</button>
        </div>
      </>
    );
  }

  return (
    <>
      <HeroSection
        title="Chào mừng bạn đến với Eralis"
        description="Hành trình giải mã những văn tự cố, giải thoát những sinh vật bí ẩn, xây dựng kiến thức lâu dài."
        textButton="Bắt đầu"
        image="./src/assets/thumb.gif"
        bottomImage="./src/assets/grass.gif"
        height="29rem"
        hidden={false}
      />

      <div className="background-color text-white min-h-100 flex justify-center items-center">
        <div className="px-10 py-20 flex flex-col justify-center items-center w-full max-w-2xl">
          <div className="mb-5 text-2xl font-press w-2/3">
            All things you can learn
          </div>
          <p className="mb-5 text-lg font-extralight text-center">
            Hành trình giải mã những văn tự cố, giải thoát những sinh vật bí ẩn, xây dựng kiến thức lâu dài.
          </p>
        </div>
      </div>

      <div className="background-color text-white h-screen px-10 py-3">
        <div className="container mx-auto w-7/12 grid grid-cols-3 gap-4">
          {vocabularySets.map((vocabularySet) => (
            <Card
              key={vocabularySet.id}
              title={vocabularySet.title}
              description={vocabularySet.description || 'Không có mô tả'}
              theme={vocabularySet.theme}
              difficultyLevel={vocabularySet.difficultyLevel}
              image={vocabularySet.imageUrl || ''}
              vocabularySetid={vocabularySet.id}
              isPublic={vocabularySet.isPublic}
              isOwned={vocabularySet.isOwned}
              createdByUsername={vocabularySet.createdByUsername || 'Unknown'}
            />
          ))}
        </div>
      </div>
    </>
  );
};

export default Home;