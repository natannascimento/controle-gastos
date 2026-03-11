import axios from "axios";

const DEFAULT_API_BASE_URL = "http://localhost:5034/api";

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? DEFAULT_API_BASE_URL,
  withCredentials: true,
  headers: {
    "Content-Type": "application/json",
  },
});

declare module "axios" {
  export interface InternalAxiosRequestConfig {
    _retry?: boolean;
  }
}

let getAccessToken: () => string | null = () => null;
let refreshAuthSession: (() => Promise<string | null>) | null = null;
let onSessionExpired: (() => void) | null = null;
let refreshInFlight: Promise<string | null> | null = null;

export function configureApiClientAuth(options: {
  getAccessToken: () => string | null;
  refreshAuthSession: () => Promise<string | null>;
  onSessionExpired: () => void;
}): void {
  getAccessToken = options.getAccessToken;
  refreshAuthSession = options.refreshAuthSession;
  onSessionExpired = options.onSessionExpired;
}

apiClient.interceptors.request.use((config) => {
  const accessToken = getAccessToken();

  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }

  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (!axios.isAxiosError(error)) {
      return Promise.reject(error);
    }

    const originalRequest = error.config;
    const statusCode = error.response?.status;
    const requestUrl = originalRequest?.url ?? "";
    const isRefreshRequest = requestUrl.includes("/auth/refresh");

    if (
      statusCode !== 401 ||
      !originalRequest ||
      originalRequest._retry ||
      isRefreshRequest ||
      !refreshAuthSession
    ) {
      return Promise.reject(error);
    }

    originalRequest._retry = true;

    if (!refreshInFlight) {
      refreshInFlight = refreshAuthSession().finally(() => {
        refreshInFlight = null;
      });
    }

    const refreshedAccessToken = await refreshInFlight;

    if (!refreshedAccessToken) {
      onSessionExpired?.();
      return Promise.reject(error);
    }

    originalRequest.headers.Authorization = `Bearer ${refreshedAccessToken}`;
    return apiClient(originalRequest);
  }
);
