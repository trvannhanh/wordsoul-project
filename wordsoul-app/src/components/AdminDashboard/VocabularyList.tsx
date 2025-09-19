// import { useState, useEffect } from 'react';
// import {
//   fetchVocabularies,
//   createVocabulary,
//   updateVocabulary,
//   deleteVocabulary,
// } from '../../services/vocabularyService';
// import type { AdminVocabularyDto, CreateVocabularyDto } from '../../types/Dto';

// const VocabularyList = () => {
//   const [vocabularies, setVocabularies] = useState<AdminVocabularyDto[]>([]);
//   const [formData, setFormData] = useState<CreateVocabularyDto>({
//     word: '',
//     meaning: '',
//     pronunciation: '',
//     partOfSpeech: 'Noun',
//     cEFRLevel: 'A1',
//     description: '',
//     exampleSentence: '',
//     imageFile: null,
//     pronunciationUrl: '',
//   });
//   const [selectedVocab, setSelectedVocab] = useState<AdminVocabularyDto | null>(null);
//   const [loading, setLoading] = useState(true);
//   const [isModalOpen, setIsModalOpen] = useState(false);
//   const [vocabToDelete, setVocabToDelete] = useState<number | null>(null);

//   useEffect(() => {
//     const fetchData = async () => {
//       try {
//         const data = await fetchVocabularies();
//         setVocabularies(data);
//       } catch (error) {
//         console.error('Error fetching vocabularies:', error);
//       } finally {
//         setLoading(false);
//       }
//     };
//     fetchData();
//   }, []);

//   const handleCreateVocab = async () => {
//     const fd = new FormData();
//     fd.append('word', formData.word);
//     fd.append('meaning', formData.meaning);
//     fd.append('partOfSpeech', formData.partOfSpeech);
//     fd.append('cEFRLevel', formData.cEFRLevel);
//     if (formData.pronunciation) fd.append('pronunciation', formData.pronunciation);
//     if (formData.description) fd.append('description', formData.description);
//     if (formData.exampleSentence) fd.append('exampleSentence', formData.exampleSentence);
//     if (formData.imageFile) fd.append('imageFile', formData.imageFile);
//     if (formData.pronunciationUrl) fd.append('pronunciationUrl', formData.pronunciationUrl);

//     try {
//       const newVocab = await createVocabulary(fd);
//       setVocabularies((prev) => [...prev, newVocab]);
//       setFormData({
//         word: '',
//         meaning: '',
//         pronunciation: '',
//         partOfSpeech: 'Noun',
//         cEFRLevel: 'A1',
//         description: '',
//         exampleSentence: '',
//         imageFile: null,
//         pronunciationUrl: '',
//       });
//     } catch (error) {
//       console.error('Error creating vocabulary:', error);
//       alert('Failed to create vocabulary.');
//     }
//   };

//   const handleSelectVocab = (vocab: AdminVocabularyDto) => {
//     setSelectedVocab(vocab);
//     setFormData({
//       word: vocab.word || '',
//       meaning: vocab.meaning || '',
//       pronunciation: vocab.pronunciation || '',
//       partOfSpeech: vocab.partOfSpeech || 'Noun',
//       cEFRLevel: vocab.cEFRLevel || 'A1',
//       description: vocab.description || '',
//       exampleSentence: vocab.exampleSentence || '',
//       imageFile: null,
//       pronunciationUrl: vocab.pronunciationUrl || '',
//     });
//   };

//   const handleUpdateVocab = async () => {
//     if (selectedVocab) {
//       const fd = new FormData();
//       fd.append('word', formData.word);
//       fd.append('meaning', formData.meaning);
//       fd.append('partOfSpeech', formData.partOfSpeech);
//       fd.append('cEFRLevel', formData.cEFRLevel);
//       if (formData.pronunciation) fd.append('pronunciation', formData.pronunciation);
//       if (formData.description) fd.append('description', formData.description);
//       if (formData.exampleSentence) fd.append('exampleSentence', formData.exampleSentence);
//       if (formData.imageFile) fd.append('imageFile', formData.imageFile);
//       if (formData.pronunciationUrl) fd.append('pronunciationUrl', formData.pronunciationUrl);

//       try {
//         const updatedVocab = await updateVocabulary(selectedVocab.id, fd);
//         setVocabularies((prev) =>
//           prev.map((vocab) => (vocab.id === selectedVocab.id ? updatedVocab : vocab))
//         );
//         setSelectedVocab(null);
//         setFormData({
//           word: '',
//           meaning: '',
//           pronunciation: '',
//           partOfSpeech: 'Noun',
//           cEFRLevel: 'A1',
//           description: '',
//           exampleSentence: '',
//           imageFile: null,
//           pronunciationUrl: '',
//         });
//       } catch (error) {
//         console.error('Error updating vocabulary:', error);
//         alert('Failed to update vocabulary.');
//       }
//     }
//   };

//   const handleDeleteVocab = async (id: number) => {
//     try {
//       await deleteVocabulary(id);
//       setVocabularies(vocabularies.filter((vocab) => vocab.id !== id));
//       setSelectedVocab(null);
//       setIsModalOpen(false);
//     } catch (error) {
//       console.error('Error deleting vocabulary:', error);
//       alert('Failed to delete vocabulary.');
//     }
//   };

//   if (loading) return <div className="text-center text-gray-500">Loading...</div>;

//   return (
//     <div className="bg-white p-6 rounded-lg shadow">
//       <h2 className="text-2xl font-semibold mb-6 text-gray-800">Vocabularies</h2>
//       {/* Form create/update vocabulary */}
//       <div className="mb-6 grid grid-cols-1 md:grid-cols-2 gap-4">
//         <input
//           placeholder="Word"
//           value={formData.word}
//           onChange={(e) => setFormData({ ...formData, word: e.target.value })}
//           className="border p-2 rounded-md focus:ring-2 focus:ring-blue-500"
//         />
//         <input
//           placeholder="Meaning"
//           value={formData.meaning}
//           onChange={(e) => setFormData({ ...formData, meaning: e.target.value })}
//           className="border p-2 rounded-md focus:ring-2 focus:ring-blue-500"
//         />
//         <input
//           placeholder="Pronunciation (e.g., /wɜːrd/)"
//           value={formData.pronunciation}
//           onChange={(e) => setFormData({ ...formData, pronunciation: e.target.value })}
//           className="border p-2 rounded-md focus:ring-2 focus:ring-blue-500"
//         />
//         <select
//           value={formData.partOfSpeech}
//           onChange={(e) => setFormData({ ...formData, partOfSpeech: e.target.value })}
//           className="border p-2 rounded-md focus:ring-2 focus:ring-blue-500"
//         >
//           <option value="Noun">Noun</option>
//           <option value="Verb">Verb</option>
//           <option value="Adjective">Adjective</option>
//           <option value="Adverb">Adverb</option>
//           <option value="Preposition">Preposition</option>
//           <option value="Conjunction">Conjunction</option>
//           <option value="Pronoun">Pronoun</option>
//           <option value="Interjection">Interjection</option>
//         </select>
//         <select
//           value={formData.cEFRLevel}
//           onChange={(e) => setFormData({ ...formData, cEFRLevel: e.target.value })}
//           className="border p-2 rounded-md focus:ring-2 focus:ring-blue-500"
//         >
//           <option value="A1">A1</option>
//           <option value="A2">A2</option>
//           <option value="B1">B1</option>
//           <option value="B2">B2</option>
//           <option value="C1">C1</option>
//           <option value="C2">C2</option>
//         </select>
//         <input
//           placeholder="Description"
//           value={formData.description}
//           onChange={(e) => setFormData({ ...formData, description: e.target.value })}
//           className="border p-2 rounded-md focus:ring-2 focus:ring-blue-500"
//         />
//         <input
//           placeholder="Example Sentence"
//           value={formData.exampleSentence}
//           onChange={(e) => setFormData({ ...formData, exampleSentence: e.target.value })}
//           className="border p-2 rounded-md focus:ring-2 focus:ring-blue-500"
//         />
//         <input
//           placeholder="Pronunciation URL"
//           value={formData.pronunciationUrl}
//           onChange={(e) => setFormData({ ...formData, pronunciationUrl: e.target.value })}
//           className="border p-2 rounded-md focus:ring-2 focus:ring-blue-500"
//         />
//         <div className="flex items-end gap-2">
//           <input
//             type="file"
//             onChange={(e) => setFormData({ ...formData, imageFile: e.target.files?.[0] || null })}
//             className="border p-2 rounded-md w-full"
//           />
//           <button
//             onClick={selectedVocab ? handleUpdateVocab : handleCreateVocab}
//             className={`p-2 rounded-md text-white transition ${
//               selectedVocab
//                 ? 'bg-yellow-500 hover:bg-yellow-600'
//                 : 'bg-blue-500 hover:bg-blue-600'
//             }`}
//           >
//             {selectedVocab ? 'Update Vocabulary' : 'Create Vocabulary'}
//           </button>
//         </div>
//       </div>

//       {/* Vocabulary Table */}
//       <div className="overflow-x-auto">
//         <table className="min-w-full divide-y divide-gray-200">
//           <thead className="bg-gray-50">
//             <tr>
//               <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Word</th>
//               <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Meaning</th>
//               <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Part of Speech</th>
//               <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">CEFR Level</th>
//               <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
//             </tr>
//           </thead>
//           <tbody className="bg-white divide-y divide-gray-200">
//             {vocabularies.map((vocab) => (
//               <tr key={vocab.id}>
//                 <td className="px-6 py-4 whitespace-nowrap">{vocab.word}</td>
//                 <td className="px-6 py-4 whitespace-nowrap">{vocab.meaning}</td>
//                 <td className="px-6 py-4 whitespace-nowrap">{vocab.partOfSpeech}</td>
//                 <td className="px-6 py-4 whitespace-nowrap">{vocab.cEFRLevel}</td>
//                 <td className="px-6 py-4 whitespace-nowrap flex gap-2">
//                   <button
//                     onClick={() => handleSelectVocab(vocab)}
//                     className="text-blue-500 hover:text-blue-700"
//                   >
//                     Edit
//                   </button>
//                   <button
//                     onClick={() => {
//                       setVocabToDelete(vocab.id);
//                       setIsModalOpen(true);
//                     }}
//                     className="text-red-500 hover:text-red-700"
//                   >
//                     Delete
//                   </button>
//                 </td>
//               </tr>
//             ))}
//           </tbody>
//         </table>
//       </div>

//       {/* Modal Delete Confirmation */}
//       {isModalOpen && vocabToDelete && (
//         <div className="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center z-50">
//           <div className="bg-white p-6 rounded-lg shadow-lg">
//             <h3 className="text-lg font-semibold mb-4">Confirm Delete</h3>
//             <p>Are you sure you want to delete this vocabulary?</p>
//             <div className="mt-4 flex justify-end gap-2">
//               <button
//                 onClick={() => setIsModalOpen(false)}
//                 className="bg-gray-500 text-white p-2 rounded-md hover:bg-gray-600"
//               >
//                 Cancel
//               </button>
//               <button
//                 onClick={() => handleDeleteVocab(vocabToDelete)}
//                 className="bg-red-500 text-white p-2 rounded-md hover:bg-red-600"
//               >
//                 Delete
//               </button>
//             </div>
//           </div>
//         </div>
//       )}
//     </div>
//   );
// };

// export default VocabularyList;
