import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface OemHealthResponse {
  service: string;
  status: string;
  utcNow: string;
  user: {
    id: string;
    username: string;
    email: string;
    roles: string[];
  };
}

@Injectable({
  providedIn: 'root',
})
export class OemService {
  private oemApiUrl = 'http://localhost:5004/api/oem'; // Node.js OEM API

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    });
  }

  getHealth(): Observable<OemHealthResponse> {
    return this.http.get<OemHealthResponse>(`${this.oemApiUrl}/health`, {
      headers: this.getHeaders(),
    });
  }
}
