const ACCESS_TOKEN_KEY = "accessToken";
const REFRESH_TOKEN_KEY = "refreshToken";

export const getToken = (key: string): string | null => {
  const cookies = document.cookie.split("; ").find((row) => row.startsWith(`${key}=`));
  return cookies ? cookies.split("=")[1] : null;
};

export const setToken = (key: string, value: string, days = 7) => {
  document.cookie = `${key}=${value}; path=/; max-age=${days * 24 * 60 * 60}`;
};

export const clearToken = () => {
  document.cookie = `${ACCESS_TOKEN_KEY}=; Max-Age=0; path=/`;
  document.cookie = `${REFRESH_TOKEN_KEY}=; Max-Age=0; path=/`;
};

export { ACCESS_TOKEN_KEY, REFRESH_TOKEN_KEY };