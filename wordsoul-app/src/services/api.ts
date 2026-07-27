import axios, {
  type AxiosError,
  type AxiosRequestConfig,
  type InternalAxiosRequestConfig,
} from 'axios';
import { extractApiError, type AppError } from '../shared/errors';
import { queueToastAfterNavigation, toast } from '../shared/toast';
// Trong development: Vite proxy forward /api → https://localhost:63982/api (tránh CORS)
// Trong production: dùng biến môi trường VITE_API_URL
export const BASE_URL = import.meta.env.VITE_API_URL || '/api';


// ---- Endpoints ----
export const endpoints = {
  // Auth
  login: '/auth/login',
  register: '/auth/register',
  refreshToken: '/auth/refresh-token',
  googleLogin: '/auth/google-login',    // Redirect đến Google Consent Screen

  // User
  users: '/users', // Tất cả user
  currentUser: '/users/me', // Thông tin cá nhân User
  user: (userId: number) => `/users/${userId}`, // User chỉ định
  userRole: (userId: number) => `/users/${userId}/role`, // Role của user chỉ định
  userProgress: '/users/progress', // Thông tin tiến độ của User
  leaderBoard: '/users/leaderboard', // Bảng xếp hạng
  AllUserActivities: '/users/activities', // Tất cả activity của user
  userActivities: (userId: number) => `/users/${userId}/activities`, // Activity của user chỉ định
  consumeHint: '/users/me/hints/consume', // Tiêu thụ Hint
  uploadAvatar: '/users/me/avatar', // Tải lên ảnh đại diện
  userVocabularySets: (vocabularySetId: number) => `/users/vocabulary-sets/${vocabularySetId}`, // Danh sách bộ từ vựng của user
  userOwnedPet: (userId: number, petId: number) => `/users/${userId}/pets/${petId}`, // Pet của user chỉ định

  // VocabularySet
  vocabularySets: '/vocabulary-sets', // Tất cả bộ từ vựng
  aiCreateVocabularySet: '/vocabulary-sets/ai-create', // Tạo bộ từ vựng bằng AI
  aiPreviewVocabularySet: '/vocabulary-sets/ai-preview', // Preview bộ từ vựng bằng AI
  vocabularySet: (vocabularySetId: number) => `/vocabulary-sets/${vocabularySetId}`, // Bộ từ vựng chỉ định
  fetchGroupedVocabularySets: '/vocabulary-sets/grouped', // Gom nhóm bộ từ vựng
  vocabularySetDetail: (vocabularySetId: number) => `/vocabulary-sets/${vocabularySetId}/details`, // Chi tiết bộ từ vựng
  vocabularySetPublish: (id: number) => `/vocabulary-sets/${id}/publish`, // Publish bộ private → public
  vocabularySetVocabOverride: (setId: number, vocabId: number) => `/vocabulary-sets/${setId}/vocabularies/${vocabId}`, // Override từ trong bộ
  vocabularySetMyProgress: (id: number) => `/vocabulary-sets/${id}/my-progress`, // Tiến trình học của user
  vocabularySetUnregister: (id: number) => `/vocabulary-sets/${id}/user`, // Hủy đăng ký bộ từ vựng

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
  petActive: (petId: number) => `/pets/${petId}/active`, // Pet Active
  petBulk: '/pets/bulk', // Xử lý hàng loạt pet
  upgradePet: (petId: number) => `/pets/${petId}/upgrade`, // Nâng cấp pet

  // Notification
  notification: '/notifications', // Tất cả thông báo
  markReadAllNotification: '/notifications/read-all', // Đánh dấu tất cả thông báo đã đọc
  markReadNotification: (notificationId: number) => `/notifications/${notificationId}/read`, // Đánh dấu thông báo chỉ định đã đọc
  deleteNotification: (notificationId: number) => `/notifications/${notificationId}`, // Xóa thông báo chỉ định
  notificationHub: '/notificationHub',

  // Daily Quest
  todayQuests: '/daily-quests/today', // Lấy danh sách quest hôm nay
  claimQuestReward: (questId: number) => `/daily-quests/${questId}/claim`, // Nhận phần thưởng quest

  // Achievement
  myAchievements: '/achievements/me', // Danh sách thành tựu của user
  claimAchievement: (achievementId: number) => `/achievements/${achievementId}/claim`, // Nhận phần thưởng thành tựu

  // Gym Leader & Battle
  gyms: '/gym',                                                             // GET: danh sách 8 gym
  gymDetail: (gymId: number) => `/gym/${gymId}`,                            // GET: chi tiết 1 gym
  startGymBattle: (gymId: number) => `/gym/${gymId}/battle/start`,          // POST: bắt đầu battle
  submitBattle: (sessionId: number) => `/gym/battle/${sessionId}/submit`,   // POST: submit kết quả

  // Pronunciation Practice
  pronunciationWords: '/pronunciation-attempts/practice-vocabularies',                      // GET: danh sách từ đã học để luyện phát âm
  pronunciationAssess: '/pronunciation-attempts',                                           // POST: đánh giá phát âm
  pronunciationHistory: (vocabId: number) => `/pronunciation-attempts/vocabularies/${vocabId}`, // GET: lịch sử phát âm 1 từ
  pronunciationStats: '/pronunciation-attempts/stats',                                      // GET: thống kê tổng quan
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

interface TokenResponse {
  accessToken: string;
  refreshToken?: string;
}

// ---- API instances ----
const api = axios.create({
  baseURL: BASE_URL,
  withCredentials: false,
});

export const authApi = axios.create({
  baseURL: BASE_URL,
  withCredentials: true,
});

const notifyApiError = (error: AppError, config?: AxiosRequestConfig) => {
  if (config?.errorHandling?.suppressToast || error.kind === 'cancelled') {
    return;
  }

  const shouldNotify =
    Boolean(config?.errorHandling?.toastMessage) ||
    error.kind === 'network' ||
    error.kind === 'timeout' ||
    error.status === 403 ||
    error.status === 429 ||
    (error.status !== undefined && error.status >= 500);

  if (!shouldNotify) {
    return;
  }

  const retryMessage =
    error.status === 429 && error.retryAfterSeconds !== undefined
      ? ` Vui lòng thử lại sau ${error.retryAfterSeconds} giây.`
      : '';

  toast.error(
    `${config?.errorHandling?.toastMessage ?? error.message}${retryMessage}`,
    {
      id: `api-error:${error.code}`,
      description: error.traceId ? `Mã đối soát: ${error.traceId}` : undefined,
    },
  );
};

const rejectNormalizedError = (
  error: unknown,
  config?: AxiosRequestConfig,
) => {
  const appError = extractApiError(error);
  notifyApiError(appError, config);
  return Promise.reject(appError);
};

api.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => rejectNormalizedError(error, error.config),
);

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

let refreshPromise: Promise<TokenResponse> | null = null;
let isRedirectingToLogin = false;

const refreshAccessToken = () => {
  if (refreshPromise) {
    return refreshPromise;
  }

  const refreshToken = getToken(REFRESH_TOKEN_KEY);
  if (!refreshToken) {
    return Promise.reject(new Error('Refresh token is not available.'));
  }

  refreshPromise = api
    .post<TokenResponse>(
      endpoints.refreshToken,
      { refreshToken },
      { errorHandling: { suppressToast: true } },
    )
    .then((response) => response.data)
    .finally(() => {
      refreshPromise = null;
    });

  return refreshPromise;
};

const redirectToLogin = () => {
  clearToken();

  if (isRedirectingToLogin || window.location.pathname === '/login') {
    return;
  }

  isRedirectingToLogin = true;
  queueToastAfterNavigation(
    'warning',
    'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.',
    { id: 'session-expired' },
  );
  window.location.assign('/login');
};

// ---- Response interceptor ----
authApi.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as
      | InternalAxiosRequestConfig
      | undefined;

    if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const {
          accessToken,
          refreshToken: newRefreshToken,
        } = await refreshAccessToken();

        setToken(ACCESS_TOKEN_KEY, accessToken);
        if (newRefreshToken) setToken(REFRESH_TOKEN_KEY, newRefreshToken);

        originalRequest.headers.Authorization = `Bearer ${accessToken}`;

        return authApi(originalRequest);
      } catch (refreshError) {
        redirectToLogin();
        return Promise.reject(extractApiError(refreshError));
      }
    }

    return rejectNormalizedError(error, originalRequest);
  }
);

export default api;
