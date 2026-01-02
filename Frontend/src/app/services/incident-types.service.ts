import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface IncidentType {
  id: string;
  code: string;
  name: string;
  description: string | null;
  severity: 'Minor' | 'Major' | 'Critical';
  parentId: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
  children?: IncidentType[];
  parentName?: string | null;
}

export interface CreateIncidentTypeDto {
  code: string;
  name: string;
  description?: string;
  severity: 'Minor' | 'Major' | 'Critical';
  parentId?: string | null;
}

export interface UpdateIncidentTypeDto {
  name?: string;
  description?: string;
  severity?: 'Minor' | 'Major' | 'Critical';
  parentId?: string | null;
  isActive?: boolean;
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
export class IncidentTypesService {
  private apiUrl = 'http://localhost:5004/api/oem/incident-types';

  constructor(private http: HttpClient) {}

  /**
   * Create a new incident type
   */
  createIncidentType(data: CreateIncidentTypeDto): Observable<ApiResponse<IncidentType>> {
    return this.http.post<ApiResponse<IncidentType>>(this.apiUrl, data);
  }

  /**
   * Get all incident types (flat list)
   */
  getAllIncidentTypes(includeInactive = false): Observable<ApiResponse<IncidentType[]>> {
    let params = new HttpParams();
    if (includeInactive) {
      params = params.set('includeInactive', 'true');
    }
    return this.http.get<ApiResponse<IncidentType[]>>(this.apiUrl, { params });
  }

  /**
   * Get incident types as hierarchical tree
   */
  getIncidentTypesHierarchy(): Observable<ApiResponse<IncidentType[]>> {
    return this.http.get<ApiResponse<IncidentType[]>>(`${this.apiUrl}/hierarchy`);
  }

  /**
   * Get incident type by ID
   */
  getIncidentTypeById(id: string): Observable<ApiResponse<IncidentType>> {
    return this.http.get<ApiResponse<IncidentType>>(`${this.apiUrl}/${id}`);
  }

  /**
   * Get incident type by code
   */
  getIncidentTypeByCode(code: string): Observable<ApiResponse<IncidentType>> {
    return this.http.get<ApiResponse<IncidentType>>(`${this.apiUrl}/code/${code}`);
  }

  /**
   * Update incident type (code is immutable)
   */
  updateIncidentType(
    id: string,
    data: UpdateIncidentTypeDto
  ): Observable<ApiResponse<IncidentType>> {
    return this.http.put<ApiResponse<IncidentType>>(`${this.apiUrl}/${id}`, data);
  }

  /**
   * Delete incident type (soft delete)
   */
  deleteIncidentType(id: string): Observable<ApiResponse<{ message: string }>> {
    return this.http.delete<ApiResponse<{ message: string }>>(`${this.apiUrl}/${id}`);
  }

  /**
   * Activate incident type
   */
  activateIncidentType(id: string): Observable<ApiResponse<IncidentType>> {
    return this.http.post<ApiResponse<IncidentType>>(`${this.apiUrl}/${id}/activate`, {});
  }
}
