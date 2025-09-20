import axios, { AxiosError } from 'axios';
const BASE_URL = 'https://localhost:7272/api';

// ---- Endpoints ----
export const endpoints = {
  // Auth
  login: '/auth/login',
  register: '/auth/register',
  refreshToken: '/auth/refresh-token',

  // User
  users: '/users', // Tất cả user
  currentUser: '/users/me', // Thông tin cá nhân User
  user: (userId: number) => `/users/${userId}`, // User chỉ định
  userRole: (userId: number) => `/users/${userId}/role`, // Role của user chỉ định
  userProgress: '/users/progress', // Thông tin tiến độ của User
  leaderBoard: '/users/leaderboard', // Bảng xếp hạng
  AllUserActivities: '/users/activities', // Tất cả activity của user
  userActivities: (userId: number) => `/users/${userId}/activities`, // Activity của user chỉ định
  userVocabularySets: (vocabularySetId: number) => `/users/vocabulary-sets/${vocabularySetId}`, // Danh sách bộ từ vựng của user
  userOwnedPet: (userId: number, petId: number) => `/users/${userId}/pets/${petId}`, // Pet của user chỉ định

  // VocabularySet
  vocabularySets: '/vocabulary-sets', // Tất cả bộ từ vựng
  vocabularySet: (vocabularySetId: number) => `/vocabulary-sets/${vocabularySetId}`, // Bộ từ vựng chỉ định
  vocabularySetDetail: (vocabularySetId: number) => `/vocabulary-sets/${vocabularySetId}/details`, // Chi tiết bộ từ vựng

  // Vocabulary
  vocabularies: '/vocabularies', // Tất cả từ vựng
  vocabulary: (vocabularyId: number) => `/vocabularies/${vocabularyId}`, // Từ vựng chỉ định
  searchVocabularies: '/vocabularies/search', // Tìm kiếm từ vựng
  setVocabulary: (vocabularySetId: number) => `/vocabularies/${vocabularySetId}/vocabularies`, // Từ vựng trong bộ chỉ định
  deleteSetVocabulary: (vocabularySetId: number, vocabularyId: number) => `/vocabularies/${vocabularySetId}/vocabularies/${vocabularyId}`, // Xóa từ vựng trong bộ

  // LearningSession
  reviewSession: '/learning-sessions', // Phiên ôn tập
  learningSession: (vocabSetId: number) => `/learning-sessions/${vocabSetId}`, // Phiên học cho bộ từ vựng
  quizQuestions: (sessionId: number) => `/learning-sessions/${sessionId}/questions`, // Câu hỏi quiz trong phiên
  answerRecord: (sessionId: number) => `/learning-sessions/${sessionId}/answers`, // Ghi lại câu trả lời trong phiên
  learningProgress: (sessionId: number, vocabId: number) => `/learning-sessions/${sessionId}/progress/${vocabId}`, // Tiến độ học trong phiên
  completeLearningSession: (sessionId: number) => `/learning-sessions/${sessionId}/learning-completion`, // Hoàn thành phiên học
  completeReviewSession: (sessionId: number) => `/learning-sessions/${sessionId}/review-completion`, // Hoàn thành phiên ôn tập

  // Pets
  pets: '/pets', // Tất cả pet
  pet: (petId: number) => `/pets/${petId}`, // Pet chỉ định
  petDetail: (petId: number) => `/pets/${petId}/details`, // Chi tiết pet
  petBulk: '/pets/bulk', // Xử lý hàng loạt pet
  upgradePet: (petId: number) => `/pets/${petId}/upgrade`, // Nâng cấp pet

  // Notification
  notification: '/notifications', // Tất cả thông báo
  markReadAllNotification: '/notifications/read-all', // Đánh dấu tất cả thông báo đã đọc
  markReadNotification: (notificationId: number) => `/notifications/${notificationId}/read`, // Đánh dấu thông báo chỉ định đã đọc
  deleteNotification: (notificationId: number) => `/notifications/${notificationId}`, // Xóa thông báo chỉ định
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