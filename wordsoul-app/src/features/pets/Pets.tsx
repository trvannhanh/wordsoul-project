// src/pages/PetsPage.tsx
import React, { useEffect, useState } from 'react';
import { useDebounce } from 'use-debounce';
import type { Pet } from '../../types/Dto';
import { fetchPets } from '../../services/pet';
import PetCard from '../../components/PetCard';



const rarityOptions = ['Common', 'Uncommon', 'Rare', 'Epic', 'Legendary'];
const typeOptions = ['Nomadica', 'Dynamora', 'Adornica', 'Velocira', 'Substitua', 'Connectara', 'Preposita', 'Exclamora'];

const Pets: React.FC = () => {
  const [pets, setPets] = useState<Pet[]>([]);
  const [searchName, setSearchName] = useState<string>('');
  const [debouncedSearchName] = useDebounce(searchName, 500);
  const [rarityFilter, setRarityFilter] = useState<string>('');
  const [typeFilter, setTypeFilter] = useState<string>('');
  const [ownedOnly, setOwnedOnly] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(true);
  const [isSearching, setIsSearching] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const loadPets = async () => {
    setIsSearching(true);
    try {
      const filters = {
        name: debouncedSearchName || undefined,
        rarity: rarityFilter || undefined,
        type: typeFilter || undefined,
        isOwned: ownedOnly ? true : undefined,
      };
      const data = await fetchPets(filters);
      setPets(data);
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
    } catch (err) {
      setError('Error loading pets');
    } finally {
      setIsSearching(false);
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPets();
  }, [debouncedSearchName, rarityFilter, typeFilter, ownedOnly]);

  if (loading) {
    return <div className="text-center py-8">Loading...</div>;
  }

  if (error) {
    return <div className="text-center py-8 text-red-500">{error}</div>;
  }

  return (
    <div className="pixel-background font-pixel text-white mt-12 min-h-screen">
      <div className="container mx-auto p-4 w-7/12 ">
        <div className="mb-8">
          <div className="relative mb-4">
            <input
              type="text"
              value={searchName}
              onChange={(e) => setSearchName(e.target.value)}
              placeholder="Tìm kiếm pet theo tên..."
              className="w-full p-2 border background-color rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {isSearching && (
              <div className="absolute right-2 top-1/2 transform -translate-y-1/2 ">
                <svg className="animate-spin h-5 w-5 text-blue-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                </svg>
              </div>
            )}
          </div>

          <div className="flex items-center mb-4">
            <label className="flex items-center cursor-pointer">
              <div className="relative">
                <input
                  type="checkbox"
                  checked={ownedOnly}
                  onChange={(e) => setOwnedOnly(e.target.checked)}
                  className="sr-only"
                />
                <div className={`w-10 h-4 bg-gray-400 rounded-full shadow-inner ${ownedOnly ? 'bg-blue-500' : ''}`}></div>
                <div className={`dot absolute w-6 h-6 bg-white rounded-full shadow -left-1 -top-1 transition ${ownedOnly ? 'transform translate-x-full' : ''}`}></div>
              </div>
              <span className="ml-2 text-black">Chỉ hiển thị pet đã sở hữu</span>
            </label>
          </div>

          <div className="mb-4 ">
            <select
              value={rarityFilter}
              onChange={(e) => setRarityFilter(e.target.value)}
              className="p-2 border rounded-md w-full background-color"
            >
              <option value="">Chọn Rarity</option>
              {rarityOptions.map((rarity) => (
                <option key={rarity} value={rarity}>
                  {rarity}
                </option>
              ))}
            </select>
          </div>

          <div>
            <select
              value={typeFilter}
              onChange={(e) => setTypeFilter(e.target.value)}
              className="p-2 border rounded-md w-full background-color"
            >
              <option value="">Chọn Type</option>
              {typeOptions.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-5 gap-4">
          {pets.map((pet) => (
            <PetCard key={pet.id} pet={pet} />
          ))}
        </div>
      </div>
    </div>

  );
};

export default Pets;