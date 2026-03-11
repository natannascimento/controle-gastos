import { apiClient } from "./apiClient";
import type {
  AuthLoginDto,
  AuthRegisterDto,
  AuthResponse,
  AuthUser,
  GoogleAuthDto,
} from "../types/auth";

const AUTH_ENDPOINT = "/auth";
const USERS_ENDPOINT = "/users";

export async function login(dto: AuthLoginDto): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>(`${AUTH_ENDPOINT}/login`, dto);
  return response.data;
}

export async function register(dto: AuthRegisterDto): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>(`${AUTH_ENDPOINT}/register`, dto);
  return response.data;
}

export async function loginWithGoogle(dto: GoogleAuthDto): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>(`${AUTH_ENDPOINT}/google`, dto);
  return response.data;
}

export async function refresh(): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>(`${AUTH_ENDPOINT}/refresh`);
  return response.data;
}

export async function logout(): Promise<void> {
  await apiClient.post(`${AUTH_ENDPOINT}/logout`);
}

export async function getMe(): Promise<AuthUser> {
  const response = await apiClient.get<AuthUser>(`${USERS_ENDPOINT}/me`);
  return response.data;
}
