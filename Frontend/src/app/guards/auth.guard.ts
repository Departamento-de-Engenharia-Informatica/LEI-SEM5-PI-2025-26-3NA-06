import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

export const authGuard: CanActivateFn = async (route, state) => {
  const router = inject(Router);
  const authService = inject(AuthService);
  const http = inject(HttpClient);


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

  // Check role-based access - look at current route data
  const requiredRole = route.data['role'];

  // If no role required on this route, allow access
  if (!requiredRole) {
    return true;
  }


  if (user.role !== requiredRole) {
    // Log unauthorized access attempt to backend
    const logData = {
      email: user.email || 'Unknown',
      role: user.role || 'None',
      attemptedPath: state.url,
      requiredRole: requiredRole,
      timestamp: new Date().toISOString(),
    };

    try {
      console.log('[AuthGuard] Sending unauthorized access log to backend...');
      const response = await firstValueFrom(
        http.post('http://localhost:5218/api/audit/unauthorized-access', logData)
      );
      console.log('[AuthGuard] ✓ Backend logged successfully:', response);
    } catch (err: any) {
      console.error('[AuthGuard] ✗ Failed to log to backend:', err);
      console.error('[AuthGuard] Error details:', err.message || err);
      console.error('[AuthGuard] Is backend running on http://localhost:5218?');
    }
    router.navigate(['/unauthorized']);
    return false;
  }

  
  return true;
};
