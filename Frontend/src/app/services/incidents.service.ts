import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Incident {
  id: string;
  incidentTypeId: string;
  incidentTypeCode: string;
  incidentTypeName: string;
  severity: 'Minor' | 'Major' | 'Critical';
  date: string;
  startTime: string;
  endTime: string | null;
  description: string;
  affectsAllVVEs: boolean;
  affectedVVEIds: string[];
  isActive: boolean;
  status: 'Active' | 'Inactive';
  durationMinutes: number | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateIncidentDto {
  incidentTypeId: string;
  startTime: string;
  endTime?: string | null;
  description: string;
  affectsAllVVEs: boolean;
  affectedVVEIds: string[];
}

export interface UpdateIncidentDto {
  startTime?: string;
  endTime?: string | null;
  description?: string;
  affectsAllVVEs?: boolean;
  affectedVVEIds?: string[];
}

export interface IncidentFilters {
  date?: string;
  status?: 'active' | 'inactive';
  incidentTypeId?: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: string;
  message?: string;
}

@Injectable({
  providedIn: 'root',
})
export class IncidentsService {
  private apiUrl = 'http://localhost:5004/api/oem/incidents';

  constructor(private http: HttpClient) {}

  /**
   * Create a new incident
   */
  createIncident(dto: CreateIncidentDto): Observable<ApiResponse<Incident>> {
    return this.http.post<ApiResponse<Incident>>(this.apiUrl, dto);
  }

  /**
   * Get all incidents with optional filters
   */
  getAllIncidents(filters?: IncidentFilters): Observable<ApiResponse<Incident[]>> {
    let params = new HttpParams();

    if (filters?.date) {
      params = params.set('date', filters.date);
    }
    if (filters?.status) {
      params = params.set('status', filters.status);
    }
    if (filters?.incidentTypeId) {
      params = params.set('incidentTypeId', filters.incidentTypeId);
    }

    return this.http.get<ApiResponse<Incident[]>>(this.apiUrl, { params });
  }

  /**
   * Get today's active incidents
   */
  getTodaysActiveIncidents(): Observable<ApiResponse<Incident[]>> {
    return this.http.get<ApiResponse<Incident[]>>(`${this.apiUrl}/active`);
  }

  /**
   * Get incident by ID
   */
  getIncidentById(id: string): Observable<ApiResponse<Incident>> {
    return this.http.get<ApiResponse<Incident>>(`${this.apiUrl}/${id}`);
  }

  /**
   * Update incident
   */
  updateIncident(id: string, dto: UpdateIncidentDto): Observable<ApiResponse<Incident>> {
    return this.http.put<ApiResponse<Incident>>(`${this.apiUrl}/${id}`, dto);
  }

  /**
   * Delete incident (soft delete)
   */
  deleteIncident(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }
}
