'use client';

import React, { createContext, useContext, useState, useEffect } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import { jwtDecode } from 'jwt-decode';

interface User {
  id: string;
  name: string;
  email: string;
  role: string;
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (token: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    // Check token on initial load
    const token = getCookie('adminAccessToken');
    if (token) {
      try {
        const decoded: any = jwtDecode(token);
        const userRole = decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

        // Only allow Admin or SuperAdmin
        if (userRole === 'Admin' || userRole === 'SuperAdmin') {
          setUser({
            id: decoded.nameid || decoded.sub || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
            name: decoded.unique_name || decoded.name || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
            email: decoded.email,
            role: userRole,
          });
        } else {
          // Unauthorized role
          clearCookies();
        }
      } catch (error) {
        console.error('Invalid token');
        clearCookies();
      }
    }
    setIsLoading(false);
  }, []);

  useEffect(() => {
    if (!isLoading && !user && pathname !== '/login') {
      router.push('/login');
    }
  }, [isLoading, user, pathname, router]);

  const login = (token: string) => {
    const decoded: any = jwtDecode(token);
    const userRole = decoded.role || decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

    if (userRole === 'Admin' || userRole === 'SuperAdmin') {
      setUser({
        id: decoded.nameid || decoded.sub || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
        name: decoded.unique_name || decoded.name || decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
        email: decoded.email,
        role: userRole,
      });
      // Set cookie in browser
      document.cookie = `adminAccessToken=${token};path=/;max-age=86400`;
      router.push('/dashboard');
    } else {
      throw new Error('Unauthorized role. Only Admins can access this dashboard.');
    }
  };

  const logout = () => {
    setUser(null);
    clearCookies();
    router.push('/login');
  };

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

// Next.js client-side cookie helper
function getCookie(name: string) {
  if (typeof document === 'undefined') return null;
  const value = `; ${document.cookie}`;
  const parts = value.split(`; ${name}=`);
  if (parts.length === 2) return parts.pop()?.split(';').shift() || null;
  return null;
}

function clearCookies() {
  if (typeof document === 'undefined') return;
  document.cookie = `adminAccessToken=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/`;
  document.cookie = `adminRefreshToken=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/`;
}
