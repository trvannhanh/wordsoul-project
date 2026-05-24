import * as SecureStore from 'expo-secure-store';
import axios from 'axios';

export const ACCESS_TOKEN_KEY = 'wordsoul_access_token';
export const REFRESH_TOKEN_KEY = 'wordsoul_refresh_token';
export const USER_ID_KEY = 'wordsoul_user_id';

export const setToken = async (key: string, value: string): Promise<void> => {
  await SecureStore.setItemAsync(key, value);
};

export const getToken = (key: string): string | null => {
  // SecureStore is async - use synchronous version via getItemAsync cached value
  // For request interceptor we use the cached value stored in memory
  return tokenCache[key] ?? null;
};

export const getTokenAsync = async (key: string): Promise<string | null> => {
  return SecureStore.getItemAsync(key);
};

export const clearToken = async (): Promise<void> => {
  await SecureStore.deleteItemAsync(ACCESS_TOKEN_KEY);
  await SecureStore.deleteItemAsync(REFRESH_TOKEN_KEY);
  await SecureStore.deleteItemAsync(USER_ID_KEY);
  tokenCache[ACCESS_TOKEN_KEY] = null;
  tokenCache[REFRESH_TOKEN_KEY] = null;
};

// In-memory cache for synchronous access in request interceptors
const tokenCache: Record<string, string | null> = {
  [ACCESS_TOKEN_KEY]: null,
  [REFRESH_TOKEN_KEY]: null,
};

// Call this at app startup to warm the cache
export const initTokenCache = async (): Promise<void> => {
  tokenCache[ACCESS_TOKEN_KEY] = await SecureStore.getItemAsync(ACCESS_TOKEN_KEY);
  tokenCache[REFRESH_TOKEN_KEY] = await SecureStore.getItemAsync(REFRESH_TOKEN_KEY);
};

export const updateTokenCache = (key: string, value: string | null): void => {
  tokenCache[key] = value;
};

export const isTokenExpired = (token: string): boolean => {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.exp * 1000 < Date.now();
  } catch {
    return true;
  }
};

export const parseAxiosError = (error: unknown): string => {
  if (axios.isAxiosError(error)) {
    return error.response?.data?.message || error.message || 'Đã có lỗi xảy ra';
  }
  return 'Đã có lỗi xảy ra';
};
