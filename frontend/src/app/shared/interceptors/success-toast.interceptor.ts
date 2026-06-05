import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { tap } from 'rxjs';
import { SHOW_SUCCESS } from '../../core/tokens/http-context.tokens';
import { ToastService } from '../services/toast.service';

export const successToastInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  const showSuccess = req.context.get(SHOW_SUCCESS);

  return next(req).pipe(
    tap({
      next: (event) => {
        if (showSuccess && event instanceof HttpResponse && event.ok && req.method !== 'GET') {
          const payload = event.body as { message?: string } | null;
          const message = payload?.message || 'Operation completed successfully.';
          toast.success(message, 'Success');
        }
      },
    }),
  );
};
