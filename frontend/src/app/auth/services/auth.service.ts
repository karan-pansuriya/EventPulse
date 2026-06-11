import { Injectable, inject, signal } from '@angular/core';
import { map, tap, Observable, throwError, BehaviorSubject, filter, take, catchError } from 'rxjs';
import { LoginRequest, RegisterRequest, TokenResponse, UserInfo, RoleResponse } from '../models/auth.models';
import { BaseHttpService } from '../../shared/services/base-http.service';
import {
  decodeToken,
  getAccessToken,
  getRefreshToken,
  saveTokens,
  clearTokens,
} from '../utils/jwt.utils';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(BaseHttpService);
  private baseUrl = 'auth';

  user = signal<UserInfo | null>(null);
  isAuthenticated = signal(false);

  private refreshing = false;
  private refreshSubject = new BehaviorSubject<string | null>(null);

  constructor() {
    const token = getAccessToken();
    if (token) {
      const user = decodeToken(token);
      if (user) {
        this.user.set(user);
        this.isAuthenticated.set(true);
      }
    }
  }

  login(request: LoginRequest): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.baseUrl}/login`, request).pipe(
      map((res) => res.data!),
      tap((tokens) => this.handleTokens(tokens)),
    );
  }

  register(request: RegisterRequest): Observable<TokenResponse> {
    const body = {
      name: request.name,
      email: request.email,
      password: request.password,
      phone: request.phone,
      roleId: request.roleId,
    };
    return this.http.post<TokenResponse>(`${this.baseUrl}/register`, body).pipe(
      map((res) => res.data!),
    );
  }

  refreshToken(): Observable<string | null> {
    if (this.refreshing) {
      return this.refreshSubject.pipe(
        filter((val) => val !== null),
        take(1),
      );
    }

    this.refreshing = true;
    const accessToken = getAccessToken();
    const refreshToken = getRefreshToken();

    if (!accessToken || !refreshToken) {
      this.logout();
      return throwError(() => new Error('No tokens available'));
    }

    return this.http
      .post<TokenResponse>(`${this.baseUrl}/refresh`, {
        accessToken,
        refreshToken,
      })
      .pipe(
        map((res) => res.data!),
        tap((tokens) => {
          this.handleTokens(tokens);
          this.refreshing = false;
          this.refreshSubject.next(tokens.accessToken);
        }),
        map((tokens) => tokens.accessToken),
        catchError((err: unknown) => {
          this.refreshing = false;
          this.refreshSubject.next(null);
          this.logout();
          return throwError(() => err);
        }),
      );
  }

  getRoles(): Observable<RoleResponse[]> {
    return this.http.get<RoleResponse[]>(`${this.baseUrl}/roles`).pipe(
      map((res) => res.data ?? []),
    );
  }

  logout(): void {
    // Clear local tokens first for instant UI responsiveness
    clearTokens();
    this.user.set(null);
    this.isAuthenticated.set(false);

    this.http.post<void>(`${this.baseUrl}/logout`, {}).subscribe({
      error: (err) => console.error('Failed to clear cookies from backend on logout:', err)
    });
  }

  getAccessToken(): string | null {
    return getAccessToken();
  }

  private handleTokens(tokens: TokenResponse): void {
    saveTokens(tokens.accessToken, tokens.refreshToken, tokens.expiresIn, tokens.refreshExpiresIn);
    const user = decodeToken(tokens.accessToken);
    if (user) {
      this.user.set(user);
      this.isAuthenticated.set(true);
    }
  }
}
