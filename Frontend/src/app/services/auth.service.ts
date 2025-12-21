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

    // Use sessionStorage instead of localStorage - clears when browser closes
    sessionStorage.setItem(this.tokenKey, response.accessToken);
    sessionStorage.setItem(this.tokenExpiryKey, expiryDate.toISOString());
    sessionStorage.setItem(this.userKey, JSON.stringify(response.user));


    this.currentUserSubject.next(response.user);
  }

  getToken(): string | null {
    
    const token = sessionStorage.getItem(this.tokenKey);
    

    if (!token) {
      return null;
    }

    const expired = this.isTokenExpired();
   

    if (token && !expired) {
      
      return token;
    }

    if (token && expired) {

      this.clearAuthData();
    }

    return null;
  }

  getUser(): User | null {
    
    const user = this.getStoredUser();
    
    return user;
  }

  private getStoredUser(): User | null {
    const userJson = sessionStorage.getItem(this.userKey);
    if (userJson) {
      try {
        const user = JSON.parse(userJson);
        return user;
      } catch {
        console.error('[AuthService] Failed to parse user JSON');
        return null;
      }
    }
    return null;
  }

  isTokenExpired(): boolean {
    const expiry = sessionStorage.getItem(this.tokenExpiryKey);
    

    if (!expiry) {
      
      return true;
    }

    const expiryDate = new Date(expiry);
    const now = new Date();
    const isExpired = expiryDate <= now;


    return isExpired;
  }

  isAuthenticated(): boolean {
   
    const authenticated = this.getToken() !== null;
    
    return authenticated;
  }

  logout(): void {
    this.clearAuthData();
    this.currentUserSubject.next(null);
  }

  private clearAuthData(): void {
    sessionStorage.removeItem(this.tokenKey);
    sessionStorage.removeItem(this.tokenExpiryKey);
    sessionStorage.removeItem(this.userKey);
  }
}
