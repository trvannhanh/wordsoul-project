import type { Pet } from "../types/Dto";
import { authApi, endpoints } from "./api";

export const fetchPets = async (filters: { name?: string; rarity?: string; type?: string; isOwned?: boolean }): Promise<Pet[]> => {
  try {
    const response = await authApi.get<Pet[]>(endpoints.pets ,{
      params: filters,
    });
    return response.data as Pet[];
  } catch (error) {
    console.error('Error fetching pets:', error);
    return [];
  }
};

export const getAllPets = async (): Promise<Pet[]> => {
  const response = await authApi.get<Pet[]>(endpoints.pets);
  return response.data;
};

export const createPet = async (formData: FormData): Promise<Pet> => {
  const response = await authApi.post<Pet>(endpoints.pets, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return response.data;
};

export const createPetsBulk = async (data: { pets: FormData[] }): Promise<Pet[]> => {
  const response = await authApi.post<Pet[]>(endpoints.petBulk, data);
  return response.data;
};

export const updatePet = async (id: number, formData: FormData): Promise<Pet> => {
  const response = await authApi.put<Pet>(endpoints.pet(id), formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return response.data;
};

export const deletePet = async (id: number): Promise<void> => {
  await authApi.delete(endpoints.pet(id));
};

export const assignPetToUser = async (userId: number, petId: number, data: object): Promise<void> => {
  await authApi.post(endpoints.userOwnedPet(userId, petId), data);
};

export const removePetFromUser = async (userId: number, petId: number): Promise<void> => {
  await authApi.delete(endpoints.userOwnedPet(userId, petId));
};

export const evolvePet = async (petId: number, data: object): Promise<void> => {
  await authApi.post(endpoints.evolvePet(petId), data);
};