import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-storage-areas',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './storage-areas.component.html',
  styleUrls: ['./storage-areas.component.css'],
})
export class StorageAreasComponent implements OnInit {
  storageAreas: any[] = [];
  isLoading: boolean = true;
  message: string = '';

  constructor(
    private http: HttpClient,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit() {
    this.loadStorageAreas();
  }

  loadStorageAreas() {
    this.isLoading = true;
    this.http
      .get<any[]>('http://localhost:5218/api/StorageArea', { withCredentials: true })
      .subscribe({
        next: (data) => {
          this.ngZone.run(() => {
            this.storageAreas = data;
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          this.ngZone.run(() => {
            this.message = 'Failed to load storage areas';
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  createStorageArea() {
    this.router.navigate(['/port-authority/create-storage-area']);
  }

  editStorageArea(id: string) {
    this.router.navigate(['/port-authority/edit-storage-area', id]);
  }
}
