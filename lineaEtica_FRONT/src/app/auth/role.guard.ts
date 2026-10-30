import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const roleGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const rolesPermitidos = route.data['roles'] as string[] | undefined;
  const rolActual = authService.getCurrentUserRole();

  if (rolesPermitidos && !rolesPermitidos.includes(rolActual)) {
    return router.createUrlTree(['/pages/inicio']);
  }

  return true;
};
