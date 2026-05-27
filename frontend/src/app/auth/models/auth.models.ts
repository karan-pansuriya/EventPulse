export const RoleId = {
  Admin: 1,
  Organizer: 2,
  Customer: 3,
} as const;

export interface LoginRequest {
  email: string;
  password: string;
  roleId: number;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  phone: string;
  roleId: number;
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
  roleIds: number[];
}
