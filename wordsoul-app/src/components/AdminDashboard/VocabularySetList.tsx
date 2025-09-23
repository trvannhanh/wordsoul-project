import { useState, useEffect } from 'react';

import {
  fetchVocabularySets,
  createVocabularySet,
  fetchVocabularySetDetail,
  updateVocabularySet,
  deleteVocabularySet,
} from '../../services/vocabularySet';
import type { VocabularySetDetailDto, VocabularySetDto } from '../../types/VocabularySetDto';
import { addVocabularyToSet, removeVocabularyFromSet } from '../../services/vocabulary';

const VocabularySetList = () => {
  const [sets, setSets] = useState<VocabularySetDto[]>([]);
  const [selectedSet, setSelectedSet] = useState<VocabularySetDetailDto | null>(null);
  const [formData, setFormData] = useState({ title: '', description: '', imageFile: null as File | null });
  const [vocabForm, setVocabForm] = useState({ word: '', meaning: '', imageFile: null as File | null });
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [setToDelete, setSetToDelete] = useState<number | null>(null);

  useEffect(() => {
    const fetchSets = async () => {
      try {
        const data = await fetchVocabularySets();
        setSets(data);
      } catch (error) {
        console.error('Error fetching sets:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchSets();
  }, []);

  const handleCreateSet = async () => {
    const fd = new FormData();
    fd.append('title', formData.title);
    fd.append('description', formData.description || '');
    if (formData.imageFile) fd.append('imageFile', formData.imageFile);

    try {
      const newSet = await createVocabularySet(fd);
      setSets((prev) => [...prev, newSet]);
      setFormData({ title: '', description: '', imageFile: null }); // Reset form
    } catch (error) {
      console.error('Error creating set:', error);
    }
  };

  const handleSelectSet = async (id: number) => {
    try {
      const detail = await fetchVocabularySetDetail(id);
      setSelectedSet(detail);
    } catch (error) {
      console.error('Error fetching set detail:', error);
    }
  };

  const handleUpdateSet = async () => {
    if (selectedSet) {
      const fd = new FormData();
      fd.append('title', selectedSet.title);
      fd.append('description', selectedSet.description || '');
      if (formData.imageFile) fd.append('imageFile', formData.imageFile);

      try {
        await updateVocabularySet(selectedSet.id, { ...selectedSet, imageUrl: formData.imageFile ? URL.createObjectURL(formData.imageFile) : selectedSet.imageUrl });
        setSelectedSet(null); // Reset selected set after update
        const updatedSets = await fetchVocabularySets();
        setSets(updatedSets);
      } catch (error) {
        console.error('Error updating set:', error);
      }
    }
  };

  const handleDeleteSet = async (id: number) => {
    try {
      await deleteVocabularySet(id);
      setSets(sets.filter((set) => set.id !== id));
      setSelectedSet(null);
      setIsModalOpen(false);
    } catch (error) {
      console.error('Error deleting set:', error);
    }
  };

  const handleAddVocab = async () => {
    if (selectedSet) {
      const fd = new FormData();
      fd.append('word', vocabForm.word);
      fd.append('meaning', vocabForm.meaning);
      if (vocabForm.imageFile) fd.append('imageFile', vocabForm.imageFile);

      try {
        const newVocab = await addVocabularyToSet(selectedSet.id, fd);
        setSelectedSet({ ...selectedSet, vocabularies: [...selectedSet.vocabularies, newVocab] });
        setVocabForm({ word: '', meaning: '', imageFile: null }); // Reset form
      } catch (error) {
        console.error('Error adding vocab:', error);
      }
    }
  };

  const handleRemoveVocab = async (vocabId: number) => {
    if (selectedSet) {
      await removeVocabularyFromSet(selectedSet.id, vocabId);
      setSelectedSet({
        ...selectedSet,
        vocabularies: selectedSet.vocabularies.filter((v) => v.id !== vocabId),
      });
    }
  };

  if (loading) return <div className="text-center text-gray-500">Loading...</div>;

  return (
    <div className="bg-white p-6 rounded-lg shadow">
      <h2 className="text-2xl font-semibold mb-6 text-gray-800">Vocabulary Sets</h2>
      {/* Form create/update set */}
      <div className="mb-6 grid grid-cols-1 md:grid-cols-3 gap-4">
        <input
          placeholder="Title"
          value={formData.title}
          onChange={(e) => setFormData({ ...formData, title: e.target.value })}
          className="border p-2 rounded-md focus:ring-2 focus:ring-blue-500"
        />
        <input
          placeholder="Description"
          value={formData.description}
          onChange={(e) => setFormData({ ...formData, description: e.target.value })}
          className="border p-2 rounded-md focus:ring-2 focus:ring-blue-500"
        />
        <div className="flex items-end gap-2">
          <input
            type="file"
            onChange={(e) => setFormData({ ...formData, imageFile: e.target.files?.[0] || null })}
            className="border p-2 rounded-md w-full"
          />
          <button
            onClick={handleCreateSet}
            className="bg-blue-500 text-white p-2 rounded-md hover:bg-blue-600 transition"
          >
            Create Set
          </button>
        </div>
      </div>
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-50">
          <tr>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Title</th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {sets.map((set) => (
            <tr key={set.id}>
              <td className="px-6 py-4 whitespace-nowrap">{set.title}</td>
              <td className="px-6 py-4 whitespace-nowrap flex gap-2">
                <button
                  onClick={() => handleSelectSet(set.id)}
                  className="text-blue-500 hover:text-blue-700"
                >
                  View Details
                </button>
                <button
                  onClick={() => {
                    setSetToDelete(set.id);
                    setIsModalOpen(true);
                  }}
                  className="text-red-500 hover:text-red-700"
                >
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {selectedSet && (
        <div className="mt-6">
          <h3 className="text-xl font-semibold mb-4 text-gray-800">Details for {selectedSet.title}</h3>
          <div className="mb-4 grid grid-cols-1 md:grid-cols-3 gap-4">
            <input
              placeholder="Word"
              value={vocabForm.word}
              onChange={(e) => setVocabForm({ ...vocabForm, word: e.target.value })}
              className="border p-2 rounded-md focus:ring-2 focus:ring-green-500"
            />
            <input
              placeholder="Meaning"
              value={vocabForm.meaning}
              onChange={(e) => setVocabForm({ ...vocabForm, meaning: e.target.value })}
              className="border p-2 rounded-md focus:ring-2 focus:ring-green-500"
            />
            <div className="flex items-end gap-2">
              <input
                type="file"
                onChange={(e) => setVocabForm({ ...vocabForm, imageFile: e.target.files?.[0] || null })}
                className="border p-2 rounded-md w-full"
              />
              <button
                onClick={handleAddVocab}
                className="bg-green-500 text-white p-2 rounded-md hover:bg-green-600 transition"
              >
                Add Vocab
              </button>
            </div>
          </div>
          <table className="min-w-full divide-y divide-gray-200 mt-4">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Word</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Meaning</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {selectedSet.vocabularies.map((vocab) => (
                <tr key={vocab.id}>
                  <td className="px-6 py-4 whitespace-nowrap">{vocab.word}</td>
                  <td className="px-6 py-4 whitespace-nowrap">{vocab.meaning}</td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <button
                      onClick={() => handleRemoveVocab(vocab.id)}
                      className="text-red-500 hover:text-red-700"
                    >
                      Remove
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <button
            onClick={handleUpdateSet}
            className="mt-4 bg-yellow-500 text-white p-2 rounded-md hover:bg-yellow-600 transition"
          >
            Update Set
          </button>
        </div>
      )}

      {/* Modal Delete Confirmation */}
      {isModalOpen && setToDelete && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded-lg shadow-lg">
            <h3 className="text-lg font-semibold mb-4">Confirm Delete</h3>
            <p>Are you sure you want to delete this vocabulary set?</p>
            <div className="mt-4 flex justify-end gap-2">
              <button
                onClick={() => setIsModalOpen(false)}
                className="bg-gray-500 text-white p-2 rounded-md hover:bg-gray-600"
              >
                Cancel
              </button>
              <button
                onClick={() => handleDeleteSet(setToDelete)}
                className="bg-red-500 text-white p-2 rounded-md hover:bg-red-600"
              >
                Delete
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default VocabularySetList;