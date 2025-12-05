import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);

  // Get current role from localStorage
  let userRole = localStorage.getItem('userRole');

  // Check for role in query params (from OAuth callback)
  const roleFromQuery = route.queryParams['role'];
  let shouldCleanUrl = false;

  // Only accept role from query params if it matches the user's actual role
  // This prevents role manipulation via URL
  if (roleFromQuery && roleFromQuery === userRole) {
    // Valid OAuth callback - role matches stored role
    shouldCleanUrl = true;
  } else if (roleFromQuery && !userRole) {
    // First time login - no stored role yet, accept from OAuth
    localStorage.setItem('userRole', roleFromQuery);
    userRole = roleFromQuery;
    shouldCleanUrl = true;
  } else if (roleFromQuery && roleFromQuery !== userRole) {
    // Someone is trying to manipulate the role via query param
    // Ignore it and use the stored role
  }

  if (!userRole) {
    router.navigate(['/login']);
    return false;
  }

  // Check if route requires specific role
  const requiredRole = route.data['role'] as string;

  if (requiredRole && userRole !== requiredRole) {
    // Navigate to unauthorized page with state
    router.navigate(['/unauthorized'], {
      state: { attemptedRoute: state.url },
    });
    return false;
  }

  // Clean URL by removing query parameters after successful authentication
  if (shouldCleanUrl && roleFromQuery) {
    setTimeout(() => {
      router.navigate([state.url.split('?')[0]], {
        replaceUrl: true,
      });
    }, 0);
  }

  return true;
};
