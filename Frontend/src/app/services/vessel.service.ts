import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface Vessel {
  id: string;
  imo: string;
  name: string;
  vesselTypeId: string;
  capacity: number;
}

export interface Dock {
  id: string;
  name: string;
  status: string;
}

export interface VesselVisitNotification {
  id: string;
  referredVesselId: string;
  expectedArrivalTime: string;
  expectedDepartureTime: string;
  status: string;
}

@Injectable({
  providedIn: 'root',
})
export class VesselService {
  private apiUrl = 'http://localhost:5218/api';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    });
  }

  getAllVessels(): Observable<Vessel[]> {
    return this.http.get<Vessel[]>(`${this.apiUrl}/Vessel`, {
      headers: this.getHeaders(),
    });
  }

  getVesselById(id: string): Observable<Vessel> {
    return this.http.get<Vessel>(`${this.apiUrl}/Vessel/${id}`, {
      headers: this.getHeaders(),
    });
  }

  getAllDocks(): Observable<Dock[]> {
    return this.http.get<Dock[]>(`${this.apiUrl}/Dock`, {
      headers: this.getHeaders(),
    });
  }

  getDockById(id: string): Observable<Dock> {
    return this.http.get<Dock>(`${this.apiUrl}/Dock/${id}`, {
      headers: this.getHeaders(),
    });
  }

  getVesselVisitNotificationById(id: string): Observable<VesselVisitNotification> {
    return this.http.get<VesselVisitNotification>(
      `${this.apiUrl}/VesselVisitNotification/submitted/${id}`,
      {
        headers: this.getHeaders(),
      }
    );
  }
}
