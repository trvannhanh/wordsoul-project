import { createContext } from "react";
import type { UserDto } from "../types/UserDto";

export interface AuthContextType {
  user: UserDto | null;
  setUser: (user: UserDto | null) => void;
  loading: boolean;
  login: (username: string, password: string) => Promise<void>;
  register: (username: string, email: string, password: string) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);