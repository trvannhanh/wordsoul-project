
import type { PetDetailDto, PetDto, UpgradePetResponseDto } from "../types/PetDto";
import { authApi, endpoints } from "./api";

export const fetchPets = async (filters: { name?: string; rarity?: string; type?: string; isOwned?: boolean; vocabularySetId?: number }): Promise<PetDto[]> => {
  try {
    const response = await authApi.get<PetDto[]>(endpoints.pets ,{
      params: filters,
    });
    return response.data as PetDto[];
  } catch (error) {
    console.error('Error fetching pets:', error);
    return [];
  }
};

export const fetchPetById = async (id: number): Promise<PetDto> => {
  const response = await authApi.get<PetDto>(endpoints.pet(id));
  return response.data;
}

export const fetchPetDetailById = async (id: number): Promise<PetDetailDto> => {
  const response = await authApi.get<PetDetailDto>(endpoints.petDetail(id));
  return response.data;
}

export const getAllPets = async (): Promise<PetDto[]> => {
  const response = await authApi.get<PetDto[]>(endpoints.pets);
  return response.data;
};

export const createPet = async (formData: FormData): Promise<PetDto> => {
  const response = await authApi.post<PetDto>(endpoints.pets, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return response.data;
};

export const createPetsBulk = async (data: { pets: FormData[] }): Promise<PetDto[]> => {
  const response = await authApi.post<PetDto[]>(endpoints.petBulk, data);
  return response.data;
};

export const updatePet = async (id: number, formData: FormData): Promise<PetDto> => {
  const response = await authApi.put<PetDto>(endpoints.pet(id), formData, {
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

export const upgradePet = async (petId: number): Promise<UpgradePetResponseDto> => {
  const response = await authApi.post<UpgradePetResponseDto>(endpoints.upgradePet(petId));
  return response.data;
};

export const activePet = async (petId: number): Promise<PetDetailDto> => {
  const response = await authApi.put<PetDetailDto>(endpoints.petActive(petId));
  return response.data;
};