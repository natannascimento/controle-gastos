import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from "react";
import { configureApiClientAuth } from "../services/apiClient";
import * as authService from "../services/authService";
import type { AuthResponse, AuthUser } from "../types/auth";
import { AuthContext, type AuthContextValue, type AuthStatus } from "./authContextStore";

function applySession(
  session: AuthResponse,
  setStatus: (status: AuthStatus) => void,
  setAccessToken: (token: string | null) => void,
  setUser: (user: AuthUser | null) => void
): void {
  setAccessToken(session.accessToken);
  setUser(session.user);
  setStatus("authenticated");
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [status, setStatus] = useState<AuthStatus>("loading");
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [user, setUser] = useState<AuthUser | null>(null);
  const hasInitialized = useRef(false);
  const accessTokenRef = useRef<string | null>(null);

  useEffect(() => {
    accessTokenRef.current = accessToken;
  }, [accessToken]);

  const clearSession = useCallback(() => {
    setAccessToken(null);
    setUser(null);
    setStatus("unauthenticated");
  }, []);

  const refreshSession = useCallback(async (): Promise<string | null> => {
    try {
      const session = await authService.refresh();
      applySession(session, setStatus, setAccessToken, setUser);
      return session.accessToken;
    } catch {
      clearSession();
      return null;
    }
  }, [clearSession]);

  const login = useCallback(async (email: string, password: string) => {
    const session = await authService.login({ email, password });
    applySession(session, setStatus, setAccessToken, setUser);
  }, []);

  const register = useCallback(async (name: string, email: string, password: string) => {
    const session = await authService.register({ name, email, password });
    applySession(session, setStatus, setAccessToken, setUser);
  }, []);

  const loginWithGoogle = useCallback(async (idToken: string) => {
    const session = await authService.loginWithGoogle({ idToken });
    applySession(session, setStatus, setAccessToken, setUser);
  }, []);

  const logout = useCallback(async () => {
    try {
      await authService.logout();
    } finally {
      clearSession();
    }
  }, [clearSession]);

  useEffect(() => {
    configureApiClientAuth({
      getAccessToken: () => accessTokenRef.current,
      refreshAuthSession: refreshSession,
      onSessionExpired: clearSession,
    });
  }, [refreshSession, clearSession]);

  useEffect(() => {
    if (hasInitialized.current) {
      return;
    }

    hasInitialized.current = true;

    void (async () => {
      const token = await refreshSession();
      if (!token) {
        setStatus("unauthenticated");
      }
    })();
  }, [refreshSession]);

  const value = useMemo<AuthContextValue>(
    () => ({
      status,
      accessToken,
      user,
      login,
      register,
      loginWithGoogle,
      refresh: refreshSession,
      logout,
    }),
    [status, accessToken, user, login, register, loginWithGoogle, refreshSession, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
