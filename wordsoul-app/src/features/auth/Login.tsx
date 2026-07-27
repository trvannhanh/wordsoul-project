import React, { useState } from "react";
import { useNavigate, Link, useSearchParams } from "react-router-dom";
import { useAuth } from "../../hooks/Auth/useAuth";
import { BASE_URL } from "../../services/api";
import { extractApiError } from "../../shared/errors";

/** Icon Google SVG (official colors) */
const GoogleIcon: React.FC = () => (
  <svg width="20" height="20" viewBox="0 0 48 48" aria-hidden="true">
    <path fill="#EA4335" d="M24 9.5c3.54 0 6.71 1.22 9.21 3.6l6.85-6.85C35.9 2.38 30.47 0 24 0 14.62 0 6.51 5.38 2.56 13.22l7.98 6.19C12.43 13.72 17.74 9.5 24 9.5z"/>
    <path fill="#4285F4" d="M46.98 24.55c0-1.57-.15-3.09-.38-4.55H24v9.02h12.94c-.58 2.96-2.26 5.48-4.78 7.18l7.73 6c4.51-4.18 7.09-10.36 7.09-17.65z"/>
    <path fill="#FBBC05" d="M10.53 28.59c-.48-1.45-.76-2.99-.76-4.59s.27-3.14.76-4.59l-7.98-6.19C.92 16.46 0 20.12 0 24c0 3.88.92 7.54 2.56 10.78l7.97-6.19z"/>
    <path fill="#34A853" d="M24 48c6.48 0 11.93-2.13 15.89-5.81l-7.73-6c-2.15 1.45-4.92 2.3-8.16 2.3-6.26 0-11.57-4.22-13.47-9.91l-7.98 6.19C6.51 42.62 14.62 48 24 48z"/>
    <path fill="none" d="M0 0h48v48H0z"/>
  </svg>
);

const Login: React.FC = () => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  // Lỗi từ Google OAuth callback (nếu có)
  const googleError = searchParams.get("googleError");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);
    try {
      await login(username, password);
      navigate("/home");
    } catch (err: unknown) {
      setError(extractApiError(err).message);
    } finally {
      setIsLoading(false);
    }
  };

  /** Redirect sang backend /api/auth/google-login → Google Consent Screen */
  const handleGoogleLogin = () => {
    const apiBase = BASE_URL.startsWith("http")
      ? BASE_URL
      : `${window.location.origin}${BASE_URL}`;
    const starterPetId = localStorage.getItem('onboarding_starter_pet_id');
    const stateParam = starterPetId ? `starterPetId=${starterPetId}` : '';
    const url = `${apiBase}/auth/google-login${stateParam ? `?state=${encodeURIComponent(stateParam)}` : ''}`;
    window.location.href = url;
  };

  return (
    <div className="w-full h-screen bg-[url('https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756565536/dark-cloud_rzn2xf.webp'),linear-gradient(to_bottom,rgb(3,7,33),rgb(5,11,75))] bg-cover bg-center flex items-center justify-center">
      <div className="w-2/3 h-2/3 bg-[url('https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756565537/sky-stars_xnmjpc.png')] bg-cover bg-center rounded-2xl flex items-center justify-center">
        <div className="bg-white p-8 rounded-lg shadow-lg w-full max-w-md">
          <h2 className="text-2xl font-bold mb-6 text-center text-gray-800">
            Đăng nhập
          </h2>

          {/* Lỗi Google OAuth */}
          {googleError && (
            <p className="text-red-500 text-sm mb-4 text-center bg-red-50 border border-red-200 rounded px-3 py-2">
              {decodeURIComponent(googleError)}
            </p>
          )}

          {error && (
            <p className="text-red-500 text-sm mb-4 text-center">{error}</p>
          )}

          <form onSubmit={handleSubmit}>
            <div className="mb-4">
              <label
                className="block text-sm font-medium text-gray-700 mb-2"
                htmlFor="username"
              >
                Tên đăng nhập
              </label>
              <input
                className="border rounded w-full py-2 px-3 text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
                id="username"
                type="text"
                placeholder="Tên đăng nhập"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                disabled={isLoading}
                required
              />
            </div>
            <div className="mb-6">
              <label
                className="block text-sm font-medium text-gray-700 mb-2"
                htmlFor="password"
              >
                Mật khẩu
              </label>
              <input
                className="border rounded w-full py-2 px-3 text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
                id="password"
                type="password"
                placeholder="Mật khẩu"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                disabled={isLoading}
                required
              />
            </div>
            <button
              className="bg-blue-500 hover:bg-blue-600 text-white font-semibold py-2 px-4 rounded w-full disabled:bg-blue-300 transition-colors"
              type="submit"
              id="login-submit-btn"
              disabled={isLoading}
            >
              {isLoading ? "Đang đăng nhập..." : "Đăng nhập"}
            </button>
          </form>

          {/* Divider */}
          <div className="flex items-center my-4 gap-3">
            <div className="flex-1 h-px bg-gray-200" />
            <span className="text-xs text-gray-400 whitespace-nowrap">hoặc tiếp tục với</span>
            <div className="flex-1 h-px bg-gray-200" />
          </div>

          {/* Google Login Button */}
          <button
            id="google-login-btn"
            onClick={handleGoogleLogin}
            disabled={isLoading}
            className="w-full flex items-center justify-center gap-3 border border-gray-300 rounded py-2 px-4 text-gray-700 font-medium hover:bg-gray-50 hover:border-gray-400 active:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-150 shadow-sm"
          >
            <GoogleIcon />
            <span>Đăng nhập bằng Google</span>
          </button>

          <p className="text-center mt-4 text-sm text-gray-600">
            Chưa có tài khoản?{" "}
            <Link to="/register" className="text-blue-500 hover:underline">
              Đăng ký
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default Login;
