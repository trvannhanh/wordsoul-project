import { authApi, endpoints } from './api';
import type { PetDetailDto, PetDto, UpgradePetResponseDto } from '../types/PetDto';

export const fetchMyPets = async (filters?: {
  name?: string;
  rarity?: string;
  type?: string;
  isOwned?: boolean;
}): Promise<PetDto[]> => {
  const response = await authApi.get<PetDto[]>(endpoints.pets, {
    params: { isOwned: true, ...filters },
  });
  return response.data;
};

export const fetchAllPets = async (): Promise<PetDto[]> => {
  const response = await authApi.get<PetDto[]>(endpoints.pets);
  return response.data;
};

export const fetchPetDetail = async (id: number): Promise<PetDetailDto> => {
  const response = await authApi.get<PetDetailDto>(endpoints.petDetail(id));
  return response.data;
};

export const setActivePet = async (petId: number): Promise<void> => {
  await authApi.post(endpoints.petActive(petId));
};

export const upgradePet = async (
  petId: number,
): Promise<UpgradePetResponseDto> => {
  const response = await authApi.post<UpgradePetResponseDto>(
    endpoints.upgradePet(petId),
  );
  return response.data;
};
