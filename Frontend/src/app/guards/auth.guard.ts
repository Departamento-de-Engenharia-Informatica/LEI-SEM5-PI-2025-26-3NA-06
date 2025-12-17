import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  // Check if user is authenticated
  if (!authService.isAuthenticated()) {
    router.navigate(['/login']);
    return false;
  }

  // Get user from AuthService
  const user = authService.getUser();
  if (!user) {
    router.navigate(['/login']);
    return false;
  }

  if (!user.role) {
    router.navigate(['/login']);
    return false;
  }

  // Check role-based access
  const requiredRole = route.data['role'];
  if (requiredRole && user.role !== requiredRole) {
    console.warn(`Access denied. Required role: ${requiredRole}, User role: ${user.role}`);
    router.navigate(['/unauthorized']);
    return false;
  }

  return true;
};
