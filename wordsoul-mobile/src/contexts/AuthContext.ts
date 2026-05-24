import { createContext, useContext } from 'react';
import type { UserDto } from '../types/UserDto';

export interface AuthContextType {
  user: UserDto | null;
  setUser: (user: UserDto | null) => void;
  loading: boolean;
  login: (username: string, password: string) => Promise<void>;
  register: (
    username: string,
    email: string,
    password: string,
    starterPetId?: number,
  ) => Promise<void>;
  logout: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextType | null>(null);

export const useAuth = (): AuthContextType => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
};
