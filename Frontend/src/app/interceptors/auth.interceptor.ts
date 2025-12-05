import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  // Clone the request to ensure credentials (cookies) are included
  const authReq = req.clone({
    withCredentials: true,
  });

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // Unauthorized - session expired or invalid
        console.warn('Session expired or unauthorized. Redirecting to login...');
        localStorage.removeItem('userRole');
        router.navigate(['/login']);
      } else if (error.status === 403) {
        // Forbidden - user doesn't have permission
        console.warn('Access forbidden. User lacks required permissions.');
        router.navigate(['/unauthorized']);
      }

      return throwError(() => error);
    })
  );
};
