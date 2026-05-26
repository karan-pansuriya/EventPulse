export interface LoginRequest {
  email: string;
  password: string;
  role: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  phone: string;
  role: string;
}

export interface RoleResponse {
  id: number;
  name: string;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  refreshExpiresIn: number;
}

export interface UserInfo {
  id: number;
  email: string;
  name: string;
  roles: string[];
}
