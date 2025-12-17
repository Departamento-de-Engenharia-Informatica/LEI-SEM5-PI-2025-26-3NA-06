import { Component, OnInit, NgZone, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { timeout, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Component({
  selector: 'app-edit-storage-area',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './edit-storage-area.component.html',
  styleUrls: ['./edit-storage-area.component.css'],
})
export class EditStorageAreaComponent implements OnInit {
  storageAreaId: string = '';
  storageArea = {
    areaName: '',
    areaType: '',
    location: '',
    maxCapacity: 0,
    servesEntirePort: false,
    servedDockIds: [] as string[],
  };
  docks: any[] = [];
  areaTypes: string[] = ['Yard', 'Warehouse'];
  isLoading: boolean = true;
  isLoadingDocks: boolean = true;
  message: string = '';
  isSuccess: boolean = false;

  constructor(
    private http: HttpClient,
    private router: Router,
    private route: ActivatedRoute,
    private ngZone: NgZone,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.storageAreaId = this.route.snapshot.params['id'];
    if (this.storageAreaId) {
      this.loadDocks();
      this.loadStorageArea();
    } else {
      this.message = 'Invalid storage area ID';
      this.isLoading = false;
    }
  }

  loadDocks() {
    this.isLoadingDocks = true;

    this.http
      .get<any[]>('http://localhost:5218/api/Dock')
      .pipe(
        timeout(10000),
        catchError((err) => {
          console.error('HTTP error:', err);
          return throwError(() => err);
        })
      )
      .subscribe({
        next: (data) => {
          this.ngZone.run(() => {
            this.docks = data;
            this.isLoadingDocks = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Failed to load docks', err);
          this.ngZone.run(() => {
            this.message = 'Failed to load docks';
            this.isLoadingDocks = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  loadStorageArea() {
    this.isLoading = true;
    this.message = '';

    this.http
      .get<any>(`http://localhost:5218/api/StorageArea/${this.storageAreaId}`)
      .pipe(
        timeout(20000),
        catchError((err) => {
          console.error('HTTP error:', err);
          return throwError(() => err);
        })
      )
      .subscribe({
        next: (data) => {
          this.ngZone.run(() => {
            this.storageArea = {
              areaName: data.areaName || '',
              areaType: data.areaType || '',
              location: data.location || '',
              maxCapacity: data.maxCapacity || 0,
              servesEntirePort: data.servesEntirePort ?? false,
              servedDockIds: data.servedDockIds || [],
            };
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          this.ngZone.run(() => {
            if (err.status === 404) {
              this.message = 'Storage area not found';
            } else if (err.status === 401) {
              this.message = 'Authentication required. Please log in again.';
              setTimeout(() => {
                this.router.navigate(['/login']);
              }, 2000);
            } else if (err.status === 403) {
              this.message = 'You do not have permission to view this storage area.';
            } else {
              this.message = `Failed to load storage area. Error: ${err.status || 'Unknown'}`;
            }
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  toggleDock(dockId: string) {
    const index = this.storageArea.servedDockIds.indexOf(dockId);
    if (index > -1) {
      this.storageArea.servedDockIds.splice(index, 1);
    } else {
      this.storageArea.servedDockIds.push(dockId);
    }
  }

  isDockSelected(dockId: string): boolean {
    return this.storageArea.servedDockIds.includes(dockId);
  }

  onSubmit() {
    if (!this.validateForm()) {
      return;
    }

    // If servesEntirePort is true, clear servedDockIds (do not persist any dock IDs)
    if (this.storageArea.servesEntirePort) {
      this.storageArea.servedDockIds = [];
    }

    this.isLoading = true;
    this.message = '';

    this.http
      .put(`http://localhost:5218/api/StorageArea/${this.storageAreaId}`, this.storageArea)
      .subscribe({
        next: (response: any) => {
          this.ngZone.run(() => {
            this.isSuccess = true;
            this.message = 'Storage area updated successfully!';
            this.isLoading = false;
            this.cdr.detectChanges();

            setTimeout(() => {
              this.router.navigate(['/port-authority/storage-areas']);
            }, 2000);
          });
        },
        error: (error) => {
          this.ngZone.run(() => {
            this.isSuccess = false;
            this.message =
              error.error.message ||
              error.error.Message ||
              'Failed to update storage area. Please try again.';
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  validateForm(): boolean {
    if (!this.storageArea.areaName || this.storageArea.areaName.trim() === '') {
      this.message = 'Area name is required';
      this.cdr.detectChanges();
      return false;
    }

    if (!this.storageArea.areaType || this.storageArea.areaType === '') {
      this.message = 'Area type is required';
      this.cdr.detectChanges();
      return false;
    }

    if (!this.storageArea.location || this.storageArea.location.trim() === '') {
      this.message = 'Location is required';
      this.cdr.detectChanges();
      return false;
    }

    if (this.storageArea.maxCapacity <= 0) {
      this.message = 'Max capacity must be greater than 0';
      this.cdr.detectChanges();
      return false;
    }

    if (typeof this.storageArea.servesEntirePort !== 'boolean') {
      this.message = 'Please specify if this area serves the entire port.';
      this.cdr.detectChanges();
      return false;
    }

    return true;
  }

  goBack() {
    this.router.navigate(['/port-authority/storage-areas']);
  }
}
