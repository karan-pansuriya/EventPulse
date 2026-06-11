import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { SHOW_ERROR } from '../../core/tokens/http-context.tokens';
import { ToastService } from '../services/toast.service';

const extractErrorMessage = (err: HttpErrorResponse): string => {
  const payload = err?.error;

  if (payload && typeof payload === 'object') {
    const message = (payload as { message?: string }).message;
    if (message) return message;
  }

  if (typeof payload === 'string' && payload.trim()) return payload;

  return err?.message || 'Something went wrong.';
};

export const errorToastInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  const showError = req.context.get(SHOW_ERROR);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      if (showError) {
        const message = extractErrorMessage(err);
        toast.error(message, 'Request failed');
      }
      return throwError(() => err);
    }),
  );
};
