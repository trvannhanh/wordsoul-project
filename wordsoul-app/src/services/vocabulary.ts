import type { SearchVocabularyDto, Vocabulary } from "../types/Dto";
import { authApi, endpoints } from "./api";

export const searchVocabularies = async (searchDto: SearchVocabularyDto): Promise<Vocabulary[]> => {
    const response = await authApi.post<Vocabulary[]>(endpoints.searchVocabularies, searchDto);
    return response.data;
};