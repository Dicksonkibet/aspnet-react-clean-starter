import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

interface AuthUser {
  email: string;
  fullName: string;
}

interface AuthState {
  user: AuthUser | null;
  token: string | null;
  isAuthenticated: boolean;
  login: (user: AuthUser, token: string) => void;
  logout: () => void;
}

// localStorage keeps the session across tabs and browser restarts. Swap for
// sessionStorage if you'd rather the session end when the tab closes.
export const AUTH_STORE_KEY = "cleanstart:auth";

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      isAuthenticated: false,
      login: (user, token) => set({ user, token, isAuthenticated: true }),
      logout: () => set({ user: null, token: null, isAuthenticated: false }),
    }),
    {
      name: AUTH_STORE_KEY,
      storage: createJSONStorage(() => localStorage),
    },
  ),
);
