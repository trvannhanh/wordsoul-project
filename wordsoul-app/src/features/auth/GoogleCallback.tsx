import React, { useEffect, useRef } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { authApi, endpoints } from "../../services/api";
import { setToken, ACCESS_TOKEN_KEY, REFRESH_TOKEN_KEY } from "../../helpers/authHelpers";
import { useAuth } from "../../hooks/Auth/useAuth";

/**
 * GoogleCallback — Trang xử lý redirect từ backend sau khi đăng nhập Google thành công.
 * Backend redirect về: /auth/callback?accessToken=...&refreshToken=...
 * Trang này lưu token và fetch thông tin user, sau đó redirect /home.
 */
const GoogleCallback: React.FC = () => {
  const [searchParams] = useSearchParams();
  const { setUser } = useAuth();
  const navigate = useNavigate();
  const handledRef = useRef(false); // tránh double-run do StrictMode

  useEffect(() => {
    if (handledRef.current) return;
    handledRef.current = true;

    const handleCallback = async () => {
      const accessToken = searchParams.get("accessToken");
      const refreshToken = searchParams.get("refreshToken");
      const error = searchParams.get("error");

      if (error || !accessToken || !refreshToken) {
        const msg = error === "google_denied"
          ? "Bạn đã hủy đăng nhập bằng Google."
          : "Đăng nhập Google thất bại. Vui lòng thử lại.";
        navigate(`/login?googleError=${encodeURIComponent(msg)}`, { replace: true });
        return;
      }

      try {
        // Lưu token vào cookie
        setToken(ACCESS_TOKEN_KEY, accessToken);
        setToken(REFRESH_TOKEN_KEY, refreshToken);

        // Lấy thông tin user
        const res = await authApi.get(endpoints.currentUser, {
          headers: { Authorization: `Bearer ${accessToken}` },
        });
        setUser(res.data);

        navigate("/home", { replace: true });
      } catch {
        navigate("/login?googleError=Đã xảy ra lỗi khi lấy thông tin tài khoản.", { replace: true });
      }
    };

    handleCallback();
  }, []);

  return (
    <div className="w-full h-screen flex items-center justify-center bg-gradient-to-b from-[rgb(3,7,33)] to-[rgb(5,11,75)]">
      <div className="flex flex-col items-center gap-4 text-white">
        {/* Spinner */}
        <div className="relative w-16 h-16">
          <div className="absolute inset-0 rounded-full border-4 border-blue-500/30" />
          <div className="absolute inset-0 rounded-full border-4 border-transparent border-t-blue-400 animate-spin" />
        </div>
        <p className="text-lg font-medium tracking-wide">Đang xác thực tài khoản Google...</p>
        <p className="text-sm text-blue-300/70">Vui lòng chờ trong giây lát</p>
      </div>
    </div>
  );
};

export default GoogleCallback;
