import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Container {
  id: string;
  isoCode: string;
  isHazardous: boolean;
  cargoType: string;
  description: string;
}

export interface UpsertContainer {
  isoCode: string;
  isHazardous: boolean;
  cargoType: string;
  description: string;
}

@Injectable({
  providedIn: 'root',
})
export class ContainerService {
  private apiUrl = 'http://localhost:5218/api/Container';

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    });
  }

  getAll(): Observable<Container[]> {
    return this.http.get<Container[]>(this.apiUrl, { headers: this.getHeaders() });
  }

  getById(id: string): Observable<Container> {
    return this.http.get<Container>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }

  getByIsoCode(isoCode: string): Observable<Container> {
    return this.http.get<Container>(`${this.apiUrl}/iso/${isoCode}`, {
      headers: this.getHeaders(),
    });
  }

  create(container: UpsertContainer): Observable<Container> {
    return this.http.post<Container>(this.apiUrl, container, { headers: this.getHeaders() });
  }

  update(id: string, container: UpsertContainer): Observable<Container> {
    return this.http.put<Container>(`${this.apiUrl}/${id}`, container, {
      headers: this.getHeaders(),
    });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }
}
