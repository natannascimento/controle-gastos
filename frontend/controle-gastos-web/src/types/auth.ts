export const AuthProvider = {
  Email: 1,
  Google: 2,
} as const;

export type AuthProvider = (typeof AuthProvider)[keyof typeof AuthProvider];

export interface AuthUser {
  id: string;
  email: string;
  name: string;
  authProvider: AuthProvider;
  personId: string | null;
}

export interface AuthResponse {
  accessToken: string;
  expiresIn: number;
  user: AuthUser;
}

export interface AuthLoginDto {
  email: string;
  password: string;
}

export interface AuthRegisterDto {
  name: string;
  email: string;
  password: string;
}

export interface GoogleAuthDto {
  idToken: string;
}
