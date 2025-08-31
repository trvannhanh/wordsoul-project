import type { UserDto } from "../types/Dto";


interface AuthState {
  user: UserDto | null;
}

interface AuthAction {
  type: 'login' | 'logout';
  payload?: UserDto | null; // Sử dụng UserDto | null thay vì UserDto
}

export const reducer = (state: AuthState, action: AuthAction): AuthState => {
  switch (action.type) {
    case 'login':
      return { ...state, user: action.payload ?? null }; // Xử lý trường hợp payload là undefined
    case 'logout':
      document.cookie = 'token=; Max-Age=0; path=/';
      return { ...state, user: null };
    default:
      return state;
  }
};