import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface AuthResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
  user: {
    userId: string;
    email: string;
    role: string;
    name: string;
  };
}

export interface User {
  userId: string;
  email: string;
  role: string;
  name: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private authApiUrl = 'http://localhost:5001/auth';
  private tokenKey = 'auth_token';
  private tokenExpiryKey = 'auth_token_expiry';
  private userKey = 'auth_user';

  private currentUserSubject = new BehaviorSubject<User | null>(this.getStoredUser());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  authenticateWithGoogle(idToken: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.authApiUrl}/google`, { idToken }).pipe(
      tap((response) => {
        this.storeAuthData(response);
      })
    );
  }

  private storeAuthData(response: AuthResponse): void {
    const expiryDate = new Date();
    const expiresIn = response.expiresIn || 3600; // Default 1 hour if not provided
    expiryDate.setSeconds(expiryDate.getSeconds() + expiresIn);

    localStorage.setItem(this.tokenKey, response.accessToken);
    localStorage.setItem(this.tokenExpiryKey, expiryDate.toISOString());
    localStorage.setItem(this.userKey, JSON.stringify(response.user));

    console.log('Auth data stored. User role:', response.user.role);
    this.currentUserSubject.next(response.user);
  }

  getToken(): string | null {
    const token = localStorage.getItem(this.tokenKey);
    if (token && !this.isTokenExpired()) {
      console.log('Token retrieved successfully');
      return token;
    }
    if (token && this.isTokenExpired()) {
      console.warn('Token expired, clearing auth data');
    }
    this.clearAuthData();
    return null;
  }

  getUser(): User | null {
    return this.getStoredUser();
  }

  private getStoredUser(): User | null {
    const userJson = localStorage.getItem(this.userKey);
    if (userJson) {
      try {
        return JSON.parse(userJson);
      } catch {
        return null;
      }
    }
    return null;
  }

  isTokenExpired(): boolean {
    const expiry = localStorage.getItem(this.tokenExpiryKey);
    if (!expiry) {
      return true;
    }
    const expiryDate = new Date(expiry);
    return expiryDate <= new Date();
  }

  isAuthenticated(): boolean {
    return this.getToken() !== null;
  }

  logout(): void {
    this.clearAuthData();
    this.currentUserSubject.next(null);
  }

  private clearAuthData(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.tokenExpiryKey);
    localStorage.removeItem(this.userKey);
  }
}
