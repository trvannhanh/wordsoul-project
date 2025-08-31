import type { LoginDto, RefreshTokenRequestDto, RegisterDto, TokenResponseDto, UserDto } from "../types/Dto";
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
  const response = await api.post<TokenResponseDto>(endpoints['refresh-token'], refreshTokenDto);
  return response.data;
};