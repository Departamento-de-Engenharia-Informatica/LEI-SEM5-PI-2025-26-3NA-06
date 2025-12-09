import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { HttpClient } from '@angular/common/http';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const http = inject(HttpClient);

  // Get current role from localStorage
  let userRole = localStorage.getItem('userRole');
  let userEmail = localStorage.getItem('userEmail');
  let userName = localStorage.getItem('userName');

  // Check for role, email, and name in query params (from OAuth callback)
  // Try getting from route first, then from URL search params
  let roleFromQuery = route.queryParams['role'];
  let emailFromQuery = route.queryParams['email'];
  let nameFromQuery = route.queryParams['name'];

  // If not in route params, parse from URL directly
  if (!roleFromQuery) {
    const urlParams = new URLSearchParams(window.location.search);
    roleFromQuery = urlParams.get('role');
    emailFromQuery = urlParams.get('email');
    nameFromQuery = urlParams.get('name');
  }

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
    if (emailFromQuery) {
      localStorage.setItem('userEmail', emailFromQuery);
      userEmail = emailFromQuery;
    }
    if (nameFromQuery) {
      localStorage.setItem('userName', nameFromQuery);
      userName = nameFromQuery;
    }
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
    // Log unauthorized access attempt to backend
    http
      .post(
        'http://localhost:5218/api/AuditLog/unauthorized-access',
        {
          name: userName,
          email: userEmail,
          role: userRole,
          attemptedRoute: state.url,
        },
        { withCredentials: true }
      )
      .subscribe();

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
