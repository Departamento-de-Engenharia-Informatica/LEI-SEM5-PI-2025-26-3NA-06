import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface DockAssignment {
  vvnId: string;
  vesselId: string;
  vesselImo?: string;
  vesselName?: string;
  dockId: string;
  dockName?: string;
  eta: string;
  etd: string;
  estimatedTeu: number;
}

export interface DailyScheduleResult {
  date: string;
  isFeasible: boolean;
  warnings: string[];
  assignments: DockAssignment[];
}

@Injectable({
  providedIn: 'root',
})
export class SchedulingService {
  private schedulingApiUrl = 'http://localhost:5002/api/Scheduling';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    });
  }

  generateDailySchedule(date: string): Observable<DailyScheduleResult> {
    return this.http.post<DailyScheduleResult>(
      `${this.schedulingApiUrl}/daily?date=${date}`,
      null,
      { headers: this.getHeaders() }
    );
  }

  checkHealth(): Observable<any> {
    return this.http.get(`${this.schedulingApiUrl}/health`);
  }
}
