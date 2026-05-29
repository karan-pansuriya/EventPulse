import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { getAccessToken } from '../utils/jwt.utils';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const isRefresh = req.url.includes('/auth/refresh');
  const isAuthAction = req.url.includes('/auth/login') || req.url.includes('/auth/register');
  const token = getAccessToken();

  let authReq = req;

  if (token && !isRefresh) {
    authReq = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }

  return next(authReq).pipe(
    catchError(err => {
      if (err instanceof HttpErrorResponse && err.status === 401 && !isRefresh && !isAuthAction) {
        return authService.refreshToken().pipe(
          switchMap(() => next(req.clone({
            setHeaders: { Authorization: `Bearer ${getAccessToken()}` },
          }))),
          catchError(() => {
            router.navigate(['/login']);
            return throwError(() => err);
          }),
        );
      }
      return throwError(() => err);
    }),
  );
};
