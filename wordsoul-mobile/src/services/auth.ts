import api, { authApi, endpoints } from './api';
import type {
  LoginDto,
  RegisterDto,
  TokenResponseDto,
  UserDto,
} from '../types/UserDto';

export const login = async (loginDto: LoginDto): Promise<TokenResponseDto> => {
  const response = await api.post<TokenResponseDto>(endpoints.login, loginDto);
  return response.data;
};

export const register = async (registerDto: RegisterDto): Promise<UserDto> => {
  const response = await api.post<UserDto>(endpoints.register, registerDto);
  return response.data;
};

export const refreshToken = async (
  id: number,
  refreshToken: string,
): Promise<TokenResponseDto> => {
  const response = await api.post<TokenResponseDto>(endpoints.refreshToken, {
    id,
    refreshToken,
  });
  return response.data;
};

export const getCurrentUser = async (): Promise<UserDto> => {
  const response = await authApi.get<UserDto>(endpoints.currentUser);
  return response.data;
};
