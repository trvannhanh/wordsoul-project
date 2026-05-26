import axios, { AxiosError } from 'axios';
import Constants from 'expo-constants';
import {
  ACCESS_TOKEN_KEY,
  REFRESH_TOKEN_KEY,
  USER_ID_KEY,
  clearToken,
  getToken,
  setToken,
  updateTokenCache,
} from '../helpers/authHelpers';

// Base URL: đọc từ env variable (10.0.2.2 = localhost trên Android Emulator)
export const BASE_URL =
  Constants.expoConfig?.extra?.apiUrl ||
  process.env.EXPO_PUBLIC_API_URL ||
  'http://10.0.2.2:63982/api';

// ---- Endpoints ----
export const endpoints = {
  // Auth
  login: '/auth/login',
  register: '/auth/register',
  refreshToken: '/auth/refresh-token',
  googleLogin: '/auth/google-login',

  // User
  users: '/users',
  currentUser: '/users/me',
  user: (userId: number) => `/users/${userId}`,
  userRole: (userId: number) => `/users/${userId}/role`,
  userProgress: '/users/progress',
  leaderBoard: '/users/leaderboard',
  AllUserActivities: '/users/activities',
  userActivities: (userId: number) => `/users/${userId}/activities`,
  consumeHint: '/users/me/hints/consume',
  userVocabularySets: (vocabularySetId: number) =>
    `/users/vocabulary-sets/${vocabularySetId}`,
  userOwnedPet: (userId: number, petId: number) =>
    `/users/${userId}/pets/${petId}`,

  // VocabularySet
  vocabularySets: '/vocabulary-sets',
  aiCreateVocabularySet: '/vocabulary-sets/ai-create',
  aiPreviewVocabularySet: '/vocabulary-sets/ai-preview',
  vocabularySet: (vocabularySetId: number) =>
    `/vocabulary-sets/${vocabularySetId}`,
  fetchGroupedVocabularySets: '/vocabulary-sets/grouped',
  vocabularySetDetail: (vocabularySetId: number) =>
    `/vocabulary-sets/${vocabularySetId}/details`,
  vocabularySetPublish: (id: number) => `/vocabulary-sets/${id}/publish`,
  vocabularySetVocabOverride: (setId: number, vocabId: number) =>
    `/vocabulary-sets/${setId}/vocabularies/${vocabId}`,
  vocabularySetMyProgress: (id: number) =>
    `/vocabulary-sets/${id}/my-progress`,
  vocabularySetUnregister: (id: number) => `/vocabulary-sets/${id}/user`,

  // Vocabulary
  vocabularies: '/vocabularies',
  vocabulary: (vocabularyId: number) => `/vocabularies/${vocabularyId}`,
  searchVocabularies: '/vocabularies/search',
  setVocabulary: (vocabularySetId: number) =>
    `/vocabularies/${vocabularySetId}/vocabularies`,

  // LearningSession
  reviewSession: '/learning-sessions',
  learningSession: (vocabSetId: number) => `/learning-sessions/${vocabSetId}`,
  quizQuestions: (sessionId: number) =>
    `/learning-sessions/${sessionId}/questions`,
  answerRecord: (sessionId: number) =>
    `/learning-sessions/${sessionId}/answers`,
  learningProgress: (sessionId: number, vocabId: number) =>
    `/learning-sessions/${sessionId}/progress/${vocabId}`,
  completeLearningSession: (sessionId: number) =>
    `/learning-sessions/${sessionId}/learning-completion`,
  completeReviewSession: (sessionId: number) =>
    `/learning-sessions/${sessionId}/review-completion`,

  // Pets
  pets: '/pets',
  pet: (petId: number) => `/pets/${petId}`,
  petDetail: (petId: number) => `/pets/${petId}/details`,
  petActive: (petId: number) => `/pets/${petId}/active`,
  petBulk: '/pets/bulk',
  upgradePet: (petId: number) => `/pets/${petId}/upgrade`,

  // Notification
  notification: '/notifications',
  markReadAllNotification: '/notifications/read-all',
  markReadNotification: (notificationId: number) =>
    `/notifications/${notificationId}/read`,
  deleteNotification: (notificationId: number) =>
    `/notifications/${notificationId}`,
  notificationHub: '/notificationHub',

  // Daily Quest
  todayQuests: '/daily-quests/today',
  claimQuestReward: (questId: number) => `/daily-quests/${questId}/claim`,

  // Achievement
  myAchievements: '/achievements/me',
  claimAchievement: (achievementId: number) =>
    `/achievements/${achievementId}/claim`,

  // Gym Leader & Battle
  gyms: '/gym',
  gymDetail: (gymId: number) => `/gym/${gymId}`,
  startGymBattle: (gymId: number) => `/gym/${gymId}/battle`,
  gymBattleQuestion: (battleId: number) =>
    `/gym/battles/${battleId}/question`,
  gymBattleAnswer: (battleId: number) => `/gym/battles/${battleId}/answer`,
  gymBattleComplete: (battleId: number) =>
    `/gym/battles/${battleId}/complete`,

  // PvP
  pvpHub: '/pvpHub',

  // Push token (mobile-specific)
  pushToken: '/users/me/push-token',
};

// ---- Public API (không cần auth) ----
const api = axios.create({
  baseURL: BASE_URL,
  withCredentials: false,
  timeout: 15000,
});

// ---- Authenticated API ----
export const authApi = axios.create({
  baseURL: BASE_URL,
  withCredentials: false,
  timeout: 15000,
});

// ---- Request interceptor: gắn Bearer token ----
authApi.interceptors.request.use(
  (config) => {
    const accessToken = getToken(ACCESS_TOKEN_KEY);
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    return config;
  },
  (error) => Promise.reject(error),
);

// ---- Response interceptor: auto refresh token khi 401 ----
authApi.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as typeof error.config & {
      _retry?: boolean;
    };

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshTokenValue = await import('expo-secure-store').then((m) =>
          m.getItemAsync(REFRESH_TOKEN_KEY),
        );
        const userIdStr = await import('expo-secure-store').then((m) =>
          m.getItemAsync(USER_ID_KEY),
        );

        if (!refreshTokenValue || !userIdStr) {
          await clearToken();
          return Promise.reject(error);
        }

        const res = await api.post(endpoints.refreshToken, {
          id: parseInt(userIdStr, 10),
          refreshToken: refreshTokenValue,
        });

        const { accessToken, refreshToken: newRefresh } = res.data;
        await setToken(ACCESS_TOKEN_KEY, accessToken);
        await setToken(REFRESH_TOKEN_KEY, newRefresh);
        updateTokenCache(ACCESS_TOKEN_KEY, accessToken);

        originalRequest.headers = originalRequest.headers || {};
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return authApi(originalRequest);
      } catch {
        await clearToken();
        return Promise.reject(error);
      }
    }

    return Promise.reject(error);
  },
);

export default api;
