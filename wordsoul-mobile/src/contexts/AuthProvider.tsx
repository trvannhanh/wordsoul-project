import React, { useCallback, useEffect, useState } from 'react';
import { AuthContext } from './AuthContext';
import type { UserDto } from '../types/UserDto';
import { login as loginService } from '../services/auth';
import { getCurrentUser } from '../services/user';
import {
  ACCESS_TOKEN_KEY,
  REFRESH_TOKEN_KEY,
  USER_ID_KEY,
  clearToken,
  initTokenCache,
  setToken,
  updateTokenCache,
} from '../helpers/authHelpers';
import * as SecureStore from 'expo-secure-store';
import { authApi } from '../services/api';

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [user, setUser] = useState<UserDto | null>(null);
  const [loading, setLoading] = useState(true);

  // Init: kiểm tra token đã lưu, load user nếu còn hợp lệ
  useEffect(() => {
    const initAuth = async () => {
      try {
        await initTokenCache();
        const accessToken = await SecureStore.getItemAsync(ACCESS_TOKEN_KEY);
        if (accessToken) {
          authApi.defaults.headers.common.Authorization = `Bearer ${accessToken}`;
          const me = await getCurrentUser();
          setUser(me);
        }
      } catch {
        setUser(null);
        await clearToken();
      } finally {
        setLoading(false);
      }
    };
    initAuth();
  }, []);

  const login = useCallback(
    async (username: string, password: string) => {
      const tokenRes = await loginService({ username, password });
      await setToken(ACCESS_TOKEN_KEY, tokenRes.accessToken);
      await setToken(REFRESH_TOKEN_KEY, tokenRes.refreshToken);
      updateTokenCache(ACCESS_TOKEN_KEY, tokenRes.accessToken);

      authApi.defaults.headers.common.Authorization = `Bearer ${tokenRes.accessToken}`;
      const me = await getCurrentUser();

      // Lưu userId để refresh token
      await SecureStore.setItemAsync(USER_ID_KEY, String(me.id));
      setUser(me);
    },
    [],
  );

  const register = useCallback(
    async (
      username: string,
      email: string,
      password: string,
      starterPetId?: number,
    ) => {
      const api = (await import('../services/api')).default;
      const endpoints = (await import('../services/api')).endpoints;
      await api.post(endpoints.register, {
        username,
        email,
        password,
        starterPetId,
      });
      await login(username, password);
    },
    [login],
  );

  const logout = useCallback(async () => {
    await clearToken();
    delete authApi.defaults.headers.common.Authorization;
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider value={{ user, setUser, loading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
};
