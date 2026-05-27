import { UserInfo } from '../models/auth.models';

const ACCESS_KEY = 'access_token';
const REFRESH_KEY = 'refresh_token';

function getCookie(name: string): string | null {
  const match = document.cookie.match(new RegExp(`(?:^|;\\s*)${name}=([^;]*)`));
  return match ? decodeURIComponent(match[1]) : null;
}

function setCookie(name: string, value: string, maxAgeSec: number): void {
  document.cookie = `${name}=${encodeURIComponent(value)}; path=/; max-age=${maxAgeSec}; SameSite=Lax`;
}

function removeCookie(name: string): void {
  document.cookie = `${name}=; path=/; max-age=0; SameSite=Lax`;
}

export function decodeToken(token: string): UserInfo | null {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const roleIds: number[] = [];
    const roleIdClaim = payload.role_id;
    if (Array.isArray(roleIdClaim)) roleIds.push(...roleIdClaim.map(Number));
    else if (roleIdClaim != null) roleIds.push(Number(roleIdClaim));

    return {
      id: parseInt(payload.sub, 10) || 0,
      email: payload.email || '',
      name: payload.name || payload.unique_name || '',
      roleIds,
    };
  } catch {
    return null;
  }
}

export function getAccessToken(): string | null {
  return getCookie(ACCESS_KEY);
}

export function getRefreshToken(): string | null {
  return getCookie(REFRESH_KEY);
}

export function saveTokens(accessToken: string, refreshToken: string): void {
  setCookie(ACCESS_KEY, accessToken, 900);
  setCookie(REFRESH_KEY, refreshToken, 604800);
}

export function clearTokens(): void {
  removeCookie(ACCESS_KEY);
  removeCookie(REFRESH_KEY);
}
