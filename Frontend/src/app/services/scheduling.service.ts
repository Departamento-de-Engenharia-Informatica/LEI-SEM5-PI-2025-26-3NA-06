import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
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

export interface DailyDockSchedule {
  dockId: string;
  dockName: string;
  assignments: DockAssignment[];
}

export interface DailyScheduleResult {
  date: string;
  isFeasible: boolean;
  warnings: string[];
  dockSchedules: DailyDockSchedule[];
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
    ).pipe(
      tap(operationPlan => {
        console.log('%c═══════════════════════════════════════════════════════', 'color: #4CAF50; font-weight: bold');
        console.log('%cOPERATION PLAN - ' + operationPlan.date, 'color: #4CAF50; font-weight: bold; font-size: 16px');
        console.log('%c═══════════════════════════════════════════════════════', 'color: #4CAF50; font-weight: bold');
        console.log('%c✓ Feasible:', operationPlan.isFeasible ? 'color: green; font-weight: bold' : 'color: red; font-weight: bold', operationPlan.isFeasible);
        
        const totalVvns = operationPlan.dockSchedules.reduce((sum, dock) => sum + dock.assignments.length, 0);
        console.log('%c🚢 Docks with VVNs:', 'color: #2196F3; font-weight: bold', operationPlan.dockSchedules.length);
        console.log('%c📋 Total VVNs:', 'color: #2196F3; font-weight: bold', totalVvns);
        console.log('%c⚠️  Warnings:', 'color: #FF9800; font-weight: bold', operationPlan.warnings.length);
        
        if (operationPlan.warnings.length > 0) {
          console.group('%cWarnings:', 'color: #FF9800; font-weight: bold');
          operationPlan.warnings.forEach(warning => console.warn(warning));
          console.groupEnd();
        }
        
        if (operationPlan.dockSchedules.length > 0) {
          console.log('%cDock Schedules:', 'color: #2196F3; font-weight: bold; font-size: 14px');
          operationPlan.dockSchedules.forEach(dockSchedule => {
            console.group(`%c🏗️  ${dockSchedule.dockName} (${dockSchedule.assignments.length} VVN(s))`, 'color: #673AB7; font-weight: bold');
            if (dockSchedule.assignments.length > 0) {
              console.table(dockSchedule.assignments.map(a => ({
                'VVN ID': a.vvnId,
                'Vessel': `${a.vesselName} (${a.vesselImo})`,
                'ETA': new Date(a.eta).toLocaleString(),
                'ETD': new Date(a.etd).toLocaleString(),
                'TEU': a.estimatedTeu
              })));
            }
            console.groupEnd();
          });
        }
        
        console.log('%cFull Operation Plan Object:', 'color: #9C27B0; font-weight: bold');
        console.log(operationPlan);
        console.log('%c═══════════════════════════════════════════════════════', 'color: #4CAF50; font-weight: bold');
      })
    );
  }

  checkHealth(): Observable<any> {
    return this.http.get(`${this.schedulingApiUrl}/health`);
  }
}
