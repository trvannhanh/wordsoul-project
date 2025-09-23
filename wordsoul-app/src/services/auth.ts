
import type { LoginDto, RefreshTokenRequestDto, RegisterDto, TokenResponseDto, UserDto } from "../types/UserDto";
import api, { endpoints } from "./api";



export const login = async (loginDto: LoginDto): Promise<TokenResponseDto> => {
  const response = await api.post<TokenResponseDto>(endpoints['login'], loginDto);
  return response.data;
};

export const register = async (registerDto: RegisterDto): Promise<UserDto> => {
  const response = await api.post<UserDto>(endpoints['register'], registerDto);
  return response.data;
};

export const refreshToken = async (refreshTokenDto: RefreshTokenRequestDto): Promise<TokenResponseDto> => {
  const response = await api.post<TokenResponseDto>(endpoints['refreshToken'], refreshTokenDto);
  return response.data;
};