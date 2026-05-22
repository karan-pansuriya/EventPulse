import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  const requiredRoles = route.data?.['roles'] as string[] | undefined;
  if (requiredRoles?.length) {
    const userRoles = authService.user()?.roles || [];
    const hasRole = requiredRoles.some(r => userRoles.includes(r));
    if (!hasRole) {
      router.navigate(['/']);
      return false;
    }
  }

  return true;
};
