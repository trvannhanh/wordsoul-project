import React, { useEffect, useState } from "react";
import api, { authApi, endpoints } from "../services/api";
import type { UserDto } from "../types/UserDto";
import { AuthContext } from "./AuthContext";
import { ACCESS_TOKEN_KEY, clearToken, getToken, REFRESH_TOKEN_KEY, setToken } from "../helpers/authHelpers";
import { useLocation, useNavigate } from "react-router-dom";


export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [user, setUser] = useState<UserDto | null>(null);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();
    const location = useLocation();

    useEffect(() => {
        const initAuth = async () => {
            try {
                const accessToken = getToken(ACCESS_TOKEN_KEY);
                if (accessToken) {
                    const res = await authApi.get(endpoints.currentUser);
                    setUser(res.data);
                    const authEntryPaths = new Set(["/", "/login", "/register"]);
                    if (authEntryPaths.has(location.pathname)) {
                        navigate("/home", { replace: true });
                    }                }
            } catch (err) {
                console.error("Không thể lấy user:", err);
                setUser(null);
            } finally {
                setLoading(false);
            }
        };

        initAuth();
    }, [location.pathname, navigate]);

    const login = async (username: string, password: string) => {
        try {
            const res = await api.post(endpoints.login, { username, password });
            const { accessToken, refreshToken } = res.data;

            setToken(ACCESS_TOKEN_KEY, accessToken);
            setToken(REFRESH_TOKEN_KEY, refreshToken);

            const me = await authApi.get(endpoints.currentUser, {
                headers: { Authorization: `Bearer ${accessToken}` },
            });
            setUser(me.data);

            navigate("/home", { replace: true });
        } catch (err) {
            console.error("Login thất bại:", err);
            throw err;
        }
    };

    const register = async (username: string, email: string, password: string, starterPetId?: number) => {
        try {
            await api.post(endpoints.register, { username, email, password, starterPetId });
            await login(username, password);
        } catch (err) {
            console.error("Register thất bại:", err);
            throw err;
        }
    };

    const logout = () => {
        clearToken();
        setUser(null);
        navigate("/login", { replace: true });
    };

    return (
        <AuthContext.Provider value={{ user, setUser, loading, login, register, logout }}>
            {children}
        </AuthContext.Provider>
    );
};