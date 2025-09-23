import { useState, useEffect } from 'react';

import { assignPetToUser, createPet, createPetsBulk, deletePet, getAllPets, updatePet } from '../../services/pet';
import { getAllUsers } from '../../services/user';
import type { PetDto } from '../../types/PetDto';
import type { UserDto } from '../../types/UserDto';


const PetList = () => {
  const [pets, setPets] = useState<PetDto[]>([]);
  const [selectedPet, setSelectedPet] = useState<PetDto | null>(null);
  const [formData, setFormData] = useState({ name: '', description: '', imageFile: null as File | null, rarity: '', type: '', baseFormId: null, nextEvolutionId: null, requiredLevel: null });
  const [bulkPets, setBulkPets] = useState<File[]>([]); // For bulk create (list files or DTOs, adjust if needed)
  const [users, setUsers] = useState<UserDto[]>([]); // List users for assign
  const [assignForm, setAssignForm] = useState({ userId: 0, initialLevel: 1, initialExperience: 0 });
  const [evolveForm, setEvolveForm] = useState({ userId: 0, experienceToAdd: 0 });
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [petToDelete, setPetToDelete] = useState<number | null>(null);

  useEffect(() => {
    const fetchPets = async () => {
      try {
        const data = await getAllPets();
        setPets(data);
      } catch (error) {
        console.error('Error fetching pets:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchPets();

    const fetchUsers = async () => {
      try {
        const data = await getAllUsers();
        setUsers(data);
      } catch (error) {
        console.error('Error fetching users:', error);
      }
    };
    fetchUsers();
  }, []);

  const handleCreatePet = async () => {
    const fd = new FormData();
    fd.append('name', formData.name);
    fd.append('description', formData.description);
    if (formData.imageFile) fd.append('imageFile', formData.imageFile);
    fd.append('rarity', formData.rarity);
    fd.append('type', formData.type);
    if (formData.baseFormId) fd.append('baseFormId', formData.baseFormId);
    if (formData.nextEvolutionId) fd.append('nextEvolutionId', formData.nextEvolutionId);
    if (formData.requiredLevel) fd.append('requiredLevel', formData.requiredLevel);

    try {
      const newPet = await createPet(fd);
      setPets([...pets, newPet]);
      setFormData({ name: '', description: '', imageFile: null, rarity: '', type: '', baseFormId: null, nextEvolutionId: null, requiredLevel: null }); // Reset form
    } catch (error) {
      console.error('Error creating pet:', error);
    }
  };

  const handleCreateBulkPets = async () => {
    const data = { pets: bulkPets.map((file) => {
      const fd = new FormData();
      fd.append('imageFile', file);
      return fd;  // Adjust based on bulk DTO if needed
    }) };
    try {
      const newPets = await createPetsBulk(data);
      setPets([...pets, ...newPets]);
      setBulkPets([]); // Reset
    } catch (error) {
      console.error('Error creating bulk pets:', error);
    }
  };

  const handleSelectPet = async (id: number) => {
    try {
      const pet = await getAllPets().then(pets => pets.find(p => p.id === id)); // Giả sử getAllPets trả list, hoặc thêm getPetById nếu có
      setSelectedPet(pet || null);
    } catch (error) {
      console.error('Error fetching pet detail:', error);
    }
  };

  const handleUpdatePet = async () => {
    if (selectedPet) {
      const fd = new FormData();
      fd.append('name', selectedPet.name);
      fd.append('description', selectedPet.description);
      if (formData.imageFile) fd.append('imageFile', formData.imageFile);
      fd.append('rarity', selectedPet.rarity);
      fd.append('type', selectedPet.type);
      // Add other fields as needed

      try {
        const updatedPet = await updatePet(selectedPet.id, fd);
        setPets(pets.map(p => p.id === updatedPet.id ? updatedPet : p));
        setSelectedPet(updatedPet);
      } catch (error) {
        console.error('Error updating pet:', error);
      }
    }
  };

  const handleDeletePet = async (id: number) => {
    try {
      await deletePet(id);
      setPets(pets.filter((pet) => pet.id !== id));
      setSelectedPet(null);
      setIsModalOpen(false);
    } catch (error) {
      console.error('Error deleting pet:', error);
    }
  };

  const handleAssignPet = async () => {
    if (selectedPet) {
      try {
        await assignPetToUser(assignForm.userId, selectedPet.id, { initialLevel: assignForm.initialLevel, initialExperience: assignForm.initialExperience });
        alert('Pet assigned successfully');
      } catch (error) {
        console.error('Error assigning pet:', error);
      }
    }
  };

  // const handleRemovePet = async (userId: number) => {
  //   if (selectedPet) {
  //     await removePetFromUser(userId, selectedPet.id);
  //     alert('Pet removed successfully');
  //   }
  // };

  // const handleEvolvePet = async () => {
  //   if (selectedPet) {
  //     try {
  //       await evolvePet(selectedPet.id, { userId: evolveForm.userId, experienceToAdd: evolveForm.experienceToAdd });
  //       alert('Pet evolved successfully');
  //     } catch (error) {
  //       console.error('Error evolving pet:', error);
  //     }
  //   }
  // };

  if (loading) return <div>Loading...</div>;

  return (
    <div className="bg-white p-6 rounded-lg shadow">
      <h2 className="text-2xl font-semibold mb-6">Pets</h2>
      {/* Form create pet */}
      <div className="mb-6">
        <input placeholder="Name" onChange={(e) => setFormData({ ...formData, name: e.target.value })} className="border p-2 mr-2" />
        <input placeholder="Description" onChange={(e) => setFormData({ ...formData, description: e.target.value })} className="border p-2 mr-2" />
        <input type="file" onChange={(e) => setFormData({ ...formData, imageFile: e.target.files?.[0] || null })} />
        <button onClick={handleCreatePet} className="bg-blue-500 text-white p-2 rounded ml-2">Create Pet</button>
      </div>
      {/* Bulk create */}
      <div className="mb-6">
        <input type="file" multiple onChange={(e) => setBulkPets(Array.from(e.target.files || []))} />
        <button onClick={handleCreateBulkPets} className="bg-purple-500 text-white p-2 rounded ml-2">Create Bulk Pets</button>
      </div>
      <table className="min-w-full divide-y divide-gray-200">
        <tbody>
          {pets.map((pet) => (
            <tr key={pet.id}>
              <td>{pet.name}</td>
              <td>
                <button onClick={() => handleSelectPet(pet.id)} className="text-blue-500">View Details</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {selectedPet && (
        <div className="mt-6">
          <h3 className="text-xl font-semibold mb-4">Details for {selectedPet.name}</h3>
          {/* Form assign pet */}
          <div className="mb-4">
            <select onChange={(e) => setAssignForm({ ...assignForm, userId: Number(e.target.value) })} className="border p-2 mr-2">
              <option>Select User</option>
              {users.map((user) => (
                <option key={user.id} value={user.id}>
                  {user.username}
                </option>
              ))}
            </select>
            <input placeholder="Initial Level" onChange={(e) => setAssignForm({ ...assignForm, initialLevel: Number(e.target.value) })} className="border p-2 mr-2" />
            <input placeholder="Initial Experience" onChange={(e) => setAssignForm({ ...assignForm, initialExperience: Number(e.target.value) })} className="border p-2 mr-2" />
            <button onClick={handleAssignPet} className="bg-green-500 text-white p-2 rounded ml-2">Assign Pet</button>
          </div>
          {/* Form evolve pet */}
          <div className="mb-4">
            <select onChange={(e) => setEvolveForm({ ...evolveForm, userId: Number(e.target.value) })} className="border p-2 mr-2">
              <option>Select User</option>
              {users.map((user) => (
                <option key={user.id} value={user.id}>
                  {user.username}
                </option>
              ))}
            </select>
            <input placeholder="Experience to Add" onChange={(e) => setEvolveForm({ ...evolveForm, experienceToAdd: Number(e.target.value) })} className="border p-2 mr-2" />
            {/* <button onClick={handleEvolvePet} className="bg-yellow-500 text-white p-2 rounded ml-2">Evolve Pet</button> */}
          </div>
          <button onClick={handleUpdatePet} className="bg-blue-500 text-white p-2 rounded">Update Pet</button>
          <button
            onClick={() => {
              setPetToDelete(selectedPet.id);
              setIsModalOpen(true);
            }}
            className="bg-red-500 text-white p-2 rounded ml-2"
          >
            Delete Pet
          </button>
        </div>
      )}

      {/* Modal Delete Confirmation */}
      {isModalOpen && petToDelete && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded-lg shadow-lg">
            <h3 className="text-lg font-semibold mb-4">Confirm Delete</h3>
            <p>Are you sure you want to delete this pet?</p>
            <div className="mt-4 flex justify-end gap-2">
              <button
                onClick={() => setIsModalOpen(false)}
                className="bg-gray-500 text-white p-2 rounded-md hover:bg-gray-600"
              >
                Cancel
              </button>
              <button
                onClick={() => handleDeletePet(petToDelete)}
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

export default PetList;