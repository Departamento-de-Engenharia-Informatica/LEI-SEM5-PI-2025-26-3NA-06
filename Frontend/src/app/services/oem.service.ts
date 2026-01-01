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

export interface SaveOperationPlanRequest {
  planDate: string;
  isFeasible: boolean;
  warnings: string[];
  algorithmUsed?: string;
  vesselVisitNotifications: any[];
}

export interface SaveOperationPlanResponse {
  success: boolean;
  data?: {
    id: string;
    planDate: string;
    algorithmUsed: string;
    createdBy: string;
    createdAt: string;
    status: string;
    isFeasible: boolean;
    conflicts: any[];
    vesselVisitNotifications: any[];
    totalVessels: number;
  };
  message?: string;
  error?: string;
}

export interface VesselVisitExecution {
  id: string;
  vvnId: string;
  operationPlanId: string;
  vveDate: string;
  plannedDockId: string;
  plannedArrivalTime: string;
  plannedDepartureTime: string;
  actualDockId: string | null;
  actualArrivalTime: string | null;
  actualDepartureTime: string | null;
  status: 'NotStarted' | 'InProgress' | 'Delayed' | 'Completed';
  createdAt: string;
  updatedAt: string | null;
  // Enriched data (added by backend)
  vesselName?: string;
  vesselImo?: string;
  plannedDockName?: string;
  actualDockName?: string;
}

export interface PrepareTodaysVVEsResponse {
  success: boolean;
  message?: string;
  operationPlanId?: string;
  date?: string;
  created?: number;
  skipped?: number;
  errors?: number;
  details?: {
    createdVves: Array<{ id: string; vvnId: string; status: string }>;
    skippedVvns: Array<{ vvnId: string; reason: string }>;
    failedVves: Array<{ success: boolean; vvnId: string; error: string }>;
  };
  error?: string;
}

export interface VVEResponse {
  success: boolean;
  data?: VesselVisitExecution | VesselVisitExecution[];
  total?: number;
  message?: string;
  warnings?: string[];
  error?: string;
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

  saveOperationPlan(request: SaveOperationPlanRequest): Observable<SaveOperationPlanResponse> {
    return this.http.post<SaveOperationPlanResponse>(`${this.oemApiUrl}/operation-plans`, request, {
      headers: this.getHeaders(),
    });
  }

  replaceOperationPlan(request: SaveOperationPlanRequest): Observable<SaveOperationPlanResponse> {
    return this.http.post<SaveOperationPlanResponse>(
      `${this.oemApiUrl}/operation-plans/replace`,
      request,
      {
        headers: this.getHeaders(),
      }
    );
  }

  validateFeasibility(request: SaveOperationPlanRequest): Observable<any> {
    return this.http.post<any>(`${this.oemApiUrl}/operation-plans/validate`, request, {
      headers: this.getHeaders(),
    });
  }

  getOperationPlanByDate(date: string): Observable<SaveOperationPlanResponse> {
    return this.http.get<SaveOperationPlanResponse>(
      `${this.oemApiUrl}/operation-plans/date/${date}`,
      { headers: this.getHeaders() }
    );
  }

  searchOperationPlans(params: {
    startDate?: string;
    endDate?: string;
    vesselIMO?: string;
  }): Observable<any> {
    const queryParams = new URLSearchParams();
    if (params.startDate) queryParams.append('startDate', params.startDate);
    if (params.endDate) queryParams.append('endDate', params.endDate);
    if (params.vesselIMO) queryParams.append('vesselIMO', params.vesselIMO);

    const url = `${this.oemApiUrl}/operation-plans${
      queryParams.toString() ? '?' + queryParams.toString() : ''
    }`;
    return this.http.get<any>(url, { headers: this.getHeaders() });
  }

  getCargoManifests(vvnId: string): Observable<any> {
    return this.http.get<any>(`${this.oemApiUrl}/operation-plans/vvn/${vvnId}/cargo-manifests`, {
      headers: this.getHeaders(),
    });
  }

  // VVE Methods
  prepareTodaysVVEs(): Observable<PrepareTodaysVVEsResponse> {
    return this.http.post<PrepareTodaysVVEsResponse>(
      `${this.oemApiUrl}/vessel-visit-executions/prepare-today`,
      {},
      { headers: this.getHeaders() }
    );
  }

  getVVEsByDate(date: string): Observable<VVEResponse> {
    return this.http.get<VVEResponse>(`${this.oemApiUrl}/vessel-visit-executions/date/${date}`, {
      headers: this.getHeaders(),
    });
  }

  getVVEById(id: string): Observable<VVEResponse> {
    return this.http.get<VVEResponse>(`${this.oemApiUrl}/vessel-visit-executions/${id}`, {
      headers: this.getHeaders(),
    });
  }

  updateVVEBerth(
    id: string,
    data: {
      actualArrivalTime?: string;
      actualDockId?: string;
      actualDepartureTime?: string;
    }
  ): Observable<VVEResponse> {
    return this.http.patch<VVEResponse>(
      `${this.oemApiUrl}/vessel-visit-executions/${id}/berth`,
      data,
      { headers: this.getHeaders() }
    );
  }

  updateVVEStatus(id: string, status: string): Observable<VVEResponse> {
    return this.http.patch<VVEResponse>(
      `${this.oemApiUrl}/vessel-visit-executions/${id}/status`,
      { status },
      { headers: this.getHeaders() }
    );
  }
}
