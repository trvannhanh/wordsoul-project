import React, { createContext, useContext, useEffect, useState } from "react";
import api, { authApi, endpoints } from "../services/api";


// Kiểu dữ liệu cho User
interface User {
  id: number;
  username: string;
  email: string;
  // thêm các field khác từ backend
}

interface AuthContextType {
  user: User | null;
  loading: boolean;
  login: (username: string, password: string) => Promise<void>;
  register: (username: string, email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const ACCESS_TOKEN_KEY = "accessToken";
const REFRESH_TOKEN_KEY = "refreshToken";

// Helpers
const getToken = (key: string) => {
  const cookies = document.cookie.split("; ").find((row) => row.startsWith(`${key}=`));
  return cookies ? cookies.split("=")[1] : null;
};

const setToken = (key: string, value: string, days = 7) => {
  document.cookie = `${key}=${value}; path=/; max-age=${days * 24 * 60 * 60}`;
};

const clearToken = () => {
  document.cookie = `${ACCESS_TOKEN_KEY}=; Max-Age=0; path=/`;
  document.cookie = `${REFRESH_TOKEN_KEY}=; Max-Age=0; path=/`;
};

// ---- Provider ----
export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  // Gọi API lấy thông tin user nếu có token
  useEffect(() => {
    const initAuth = async () => {
      try {
        const accessToken = getToken(ACCESS_TOKEN_KEY);
        if (accessToken) {
          const res = await authApi.get(endpoints.currentUser);
          setUser(res.data);
        }
      } catch (err) {
        console.error("Không thể lấy user:", err);
        setUser(null);
      } finally {
        setLoading(false);
      }
    };

    initAuth();
  }, []);

  // ---- Đăng nhập ----
  const login = async (username: string, password: string) => {
    try {
      const res = await api.post(endpoints.login, { username, password });
      const { accessToken, refreshToken } = res.data;

      // lưu token
      setToken(ACCESS_TOKEN_KEY, accessToken);
      setToken(REFRESH_TOKEN_KEY, refreshToken);

      console.log("BASE_URL", authApi.defaults.baseURL);
      console.log("endpoints.currentUser", endpoints.currentUser);
      console.log("endpoints", endpoints);

      // lấy thông tin user
      const me = await authApi.get(endpoints.currentUser, {
        headers: { Authorization: `Bearer ${accessToken}` }
      });
      setUser(me.data);

      // redirect về home
      window.location.href = "/home";
    } catch (err) {
      console.error("Login thất bại:", err);
      throw err;
    }
  };

  // ---- Đăng ký ----
  const register = async (username: string, email: string, password: string) => {
    try {
      await api.post(endpoints.register, { username, email, password });
      // Có thể login luôn sau khi đăng ký
      await login(username, password);
    } catch (err) {
      console.error("Register thất bại:", err);
      throw err;
    }
  };

  // ---- Đăng xuất ----
  const logout = () => {
    clearToken();
    setUser(null);
    window.location.href = "/login";
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

// Hook tiện dụng
// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth phải dùng trong <AuthProvider>");
  return ctx;
};
