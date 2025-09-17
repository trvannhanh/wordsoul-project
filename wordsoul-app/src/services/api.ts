// api.ts
import axios, { AxiosError } from 'axios';
const BASE_URL = 'https://localhost:7272/api';

// ---- Endpoints ----
export const endpoints = {
  // Auth
  login: '/auth/login',
  register: '/auth/register',
  refreshToken: '/auth/refresh-token',

  // User
  currentUser: '/users/me', //thông tin cá nhân User 
  userProgress: '/users/progress', //thông tin tiến độ của User

  users: '/users', //tất cả user
  user:(userId: number) => `/users/${userId}`, //user chỉ định
  userRole:(userId: number) => `/users/${userId}/role`, //Role user chỉ định
  userActivities:(userId: number) => `/users/${userId}/activities`, //Activity user chỉ định
  AllUserActivities: "/users/activities", //Activity User
  userVocabularySets: (vocabularySetId: number) => `/users/vocabulary-sets/${vocabularySetId}`, //Danh sách bộ từ vựng của user

  userOwnedPet: (userId: number , petId: number) => `/users/${userId}/pets/${petId}`,

  // VocabularySet
  vocabularySets: '/vocabulary-sets',
  vocabularySet: (vocabularySetId: number) => `/vocabulary-sets/${vocabularySetId}`, 
  vocabularySetDetail: (vocabularySetId: number) => `/vocabulary-sets/${vocabularySetId}/details`,
  
  //Vocabulary
  vocabularies:'/vocabularies',
  vocabulary: (vocabularyId: number) => `/vocabularies/${vocabularyId}`,
  searchVocabularies:'/vocabularies/search',


  setVocabulary: (vocabularySetId: number) => `/vocabularies/${vocabularySetId}/vocabularies`,
  deleteSetVocabulary: (vocabularySetId: number, vocabularyId: number) => `/vocabularies/${vocabularySetId}/vocabularies/${vocabularyId}`,


  //LearningSession
  learningSession: (vocabSetId: number) => `/learning-sessions/${vocabSetId}`,
  reviewSession:  '/learning-sessions',
  quizQuestions: (sessionId: number) => `/learning-sessions/${sessionId}/questions`,
  answerRecord: (sessionId : number) =>  `/learning-sessions/${sessionId}/answers`,
  learningProgress: (sessionId : number, vocabId : number) => `/learning-sessions/${sessionId}/progress/${vocabId}`,
  completeLearningSession: (sessionId : number) => `/learning-sessions/${sessionId}/learning-completion`,
  completeReviewSession: (sessionId : number) => `/learning-sessions/${sessionId}/review-completion`,

  //Pets
  pets: '/pets',

  pet: (petId: number) => `/pets/${petId}`,
  petDetail: (petId: number) => `/pets/${petId}/details`,
  petBulk: "/pets/bulk",
  upgradePet: (petId: number) => `/pets/${petId}/upgrade`,

  //Notification
  notification: '/notifications',
  markReadAllNotification: '/notifications/read-all',
  markReadNotification: (notificationId: number) => `/notifications/${notificationId}/read`,
  deleteNotification: (notificationId: number) => `/notifications/${notificationId}`,
};

// ---- Helpers ----
const ACCESS_TOKEN_KEY = 'accessToken';
const REFRESH_TOKEN_KEY = 'refreshToken';

// Lấy token từ cookie (hoặc localStorage)
const getToken = (key: string) => {
  const cookies = document.cookie.split('; ').find((row) => row.startsWith(`${key}=`));
  return cookies ? cookies.split('=')[1] : null;
};

const setToken = (key: string, value: string, days = 7) => {
  document.cookie = `${key}=${value}; path=/; max-age=${days * 24 * 60 * 60}`;
};

const clearToken = () => {
  document.cookie = `${ACCESS_TOKEN_KEY}=; Max-Age=0; path=/`;
  document.cookie = `${REFRESH_TOKEN_KEY}=; Max-Age=0; path=/`;
};

// ---- API instances ----
const api = axios.create({
  baseURL: BASE_URL,
  withCredentials: false,
});

export const authApi = axios.create({
  baseURL: BASE_URL,
  withCredentials: true,
});

// ---- Request interceptor ----
authApi.interceptors.request.use(
  (config) => {
    const accessToken = getToken(ACCESS_TOKEN_KEY);
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ---- Response interceptor ----
authApi.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as typeof error.config & { _retry?: boolean };

    if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = getToken(REFRESH_TOKEN_KEY);
        if (!refreshToken) {
          clearToken();
          window.location.href = '/login';
          return Promise.reject(error);
        }

        // gọi API refresh token
        const response = await api.post(endpoints.refreshToken, { refreshToken });
        const { accessToken, refreshToken: newRefreshToken } = response.data as {
          accessToken: string;
          refreshToken: string;
        };

        // lưu token mới
        setToken(ACCESS_TOKEN_KEY, accessToken);
        if (newRefreshToken) setToken(REFRESH_TOKEN_KEY, newRefreshToken);

        // gắn lại header cho request ban đầu
        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        }

        return authApi(originalRequest);
      } catch (refreshError) {
        clearToken();
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default api;