
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