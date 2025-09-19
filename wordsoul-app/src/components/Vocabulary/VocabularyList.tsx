import { useEffect, useState } from "react";
import { fetchVocabularySetDetail } from "../../services/vocabularySet";
import Bar from "../../components/Bar";

interface Vocabulary {
  id: number;
  word: string;
  meaning: string;
  imageUrl: string | null;
  pronunciation: string | null;
  partOfSpeech: string;
}

interface VocabularyListProps {
  setId: number;
  pageSize?: number;
}

const VocabularyList: React.FC<VocabularyListProps> = ({ setId, pageSize = 5 }) => {
  const [vocabularies, setVocabularies] = useState<Vocabulary[]>([]);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalVocabularies, setTotalVocabularies] = useState(0);
  const [loading, setLoading] = useState(false);

  const fetchData = async (pageNumber: number) => {
    setLoading(true);
    try {
      const data = await fetchVocabularySetDetail(setId, pageNumber, pageSize);
      setVocabularies(data.vocabularies);
      setTotalPages(data.totalPages);
      setTotalVocabularies(data.totalVocabularies);
    } catch (err) {
      console.error("Error fetching vocabularies:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData(page);
  }, [page, setId]);

  return (
    <div className="w-full border-white border-2 rounded-lg p-3">
      {/* Pagination */}
      <div className="mb-3 flex justify-between items-center text-gray-400">
        <button
          disabled={page <= 1}
          onClick={() => setPage((prev) => prev - 1)}
          className="px-3 py-1 background-color text-white rounded disabled:opacity-50 custom-cursor"
        >
          Previous
        </button>

        <span>
          Page {page} of {totalPages} (Showing {vocabularies.length} / {totalVocabularies})
        </span>

        <button
          disabled={page >= totalPages}
          onClick={() => setPage((prev) => prev + 1)}
          className="px-3 py-1 background-color text-white rounded disabled:opacity-50 custom-cursor"
        >
          Next
        </button>
      </div>

      {/* Danh sách từ */}
      {loading ? (
        <div className="text-center text-gray-400">Loading words...</div>
      ) : (
        vocabularies.map((vocab) => (
          <Bar
            key={vocab.id}
            id={vocab.id}
            word={vocab.word}
            meaning={vocab.meaning}
            pronunciation={vocab.pronunciation || "N/A"}
            partOfSpeech={vocab.partOfSpeech}
            image={vocab.imageUrl || "https://via.placeholder.com/150"}
          />
        ))
      )}
    </div>
  );
};

export default VocabularyList;
