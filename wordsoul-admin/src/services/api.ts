import axios, { AxiosError } from 'axios';

// Get base URL from env, default to local backend
export const BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:63982/api';

export const endpoints = {
  // Auth
  login: '/auth/login',
  refreshToken: '/auth/refresh-token',

  // Dashboard & Analytics
  dashboard: '/admin/dashboard', // To be implemented in Backend

  // System Configuration (SuperAdmin)
  systemConfig: '/admin/configurations',
  systemHealth: '/admin/health',

  // Users
  users: '/users',
  userDetail: (id: number) => `/users/${id}`,
  userStatus: (id: number) => `/users/${id}/status`,
  userActivities: (id: number) => `/users/${id}/activities`,

  // PvP
  pvpLeaderboard: '/admin/pvp/leaderboard',

  // Vocabulary Sets
  vocabularySets: '/vocabulary-sets',
  vocabularySetDetail: (id: number) => `/vocabulary-sets/${id}/details`,
  vocabularySetUpdate: (id: number) => `/vocabulary-sets/${id}`,
  vocabularySetDelete: (id: number) => `/vocabulary-sets/${id}`,
  aiPreview: '/vocabulary-sets/ai-preview',
  aiCreate: '/vocabulary-sets/ai-create',
  vocabularies: '/vocabularies',

  // Quests & Achievements
  adminQuests: '/admin/quests-achievements/quests',
  adminAchievements: '/admin/quests-achievements/achievements',

  // Gyms
  adminGyms: '/admin/gyms',

  // Maintenance
  redisFlush: '/admin/maintenance/redis-flush',
  dbCleanup: '/admin/maintenance/db-cleanup',

  // User Groups
  adminGroups: '/admin/groups',
  adminGroupDetail: (id: number) => `/admin/groups/${id}`,
  adminGroupMembers: (id: number) => `/admin/groups/${id}/members`,
  adminGroupMember: (groupId: number, userId: number) => `/admin/groups/${groupId}/members/${userId}`,

  // Settings — Logs
  adminLogs: '/admin/logs',

  // Pets
  pets: '/pets',
  petDetail: (id: number) => `/pets/${id}`,

  // User Balance Adjustment (SuperAdmin)
  userBalance: (id: number) => `/admin/users/${id}/balance`,

  // Notification Broadcast (SuperAdmin)
  notificationBroadcast: '/admin/notifications/broadcast',
};

const ACCESS_TOKEN_KEY = 'adminAccessToken';
const REFRESH_TOKEN_KEY = 'adminRefreshToken';

// Next.js client-side cookie helper
const getCookie = (name: string) => {
  if (typeof document === 'undefined') return null;
  const value = `; ${document.cookie}`;
  const parts = value.split(`; ${name}=`);
  if (parts.length === 2) return parts.pop()?.split(';').shift() || null;
  return null;
};

const setCookie = (name: string, value: string, days = 7) => {
  if (typeof document === 'undefined') return;
  const expires = new Date();
  expires.setTime(expires.getTime() + days * 24 * 60 * 60 * 1000);
  document.cookie = `${name}=${value};expires=${expires.toUTCString()};path=/`;
};

const clearCookies = () => {
  if (typeof document === 'undefined') return;
  document.cookie = `${ACCESS_TOKEN_KEY}=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/`;
  document.cookie = `${REFRESH_TOKEN_KEY}=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/`;
};

export const api = axios.create({
  baseURL: BASE_URL,
  withCredentials: false,
});

export const authApi = axios.create({
  baseURL: BASE_URL,
  withCredentials: true,
});

authApi.interceptors.request.use(
  (config) => {
    const accessToken = getCookie(ACCESS_TOKEN_KEY);
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

authApi.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as typeof error.config & { _retry?: boolean };

    if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = getCookie(REFRESH_TOKEN_KEY);
        if (!refreshToken) {
          clearCookies();
          if (typeof window !== 'undefined') window.location.href = '/login';
          return Promise.reject(error);
        }

        const response = await api.post(endpoints.refreshToken, { refreshToken });
        const { accessToken, refreshToken: newRefreshToken } = response.data as {
          accessToken: string;
          refreshToken: string;
        };

        setCookie(ACCESS_TOKEN_KEY, accessToken);
        if (newRefreshToken) setCookie(REFRESH_TOKEN_KEY, newRefreshToken);

        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        }

        return authApi(originalRequest);
      } catch (refreshError) {
        clearCookies();
        if (typeof window !== 'undefined') window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);
