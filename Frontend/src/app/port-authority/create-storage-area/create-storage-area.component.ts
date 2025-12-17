import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-create-storage-area',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-storage-area.component.html',
  styleUrls: ['./create-storage-area.component.css'],
})
export class CreateStorageAreaComponent implements OnInit {
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
  message: string = '';
  isLoading: boolean = false;
  isSuccess: boolean = false;

  constructor(
    private http: HttpClient,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit() {
    this.loadDocks();
  }

  loadDocks() {
    this.http.get<any[]>('http://localhost:5218/api/Dock').subscribe({
      next: (data) => {
        this.ngZone.run(() => {
          this.docks = data;
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        console.error('Failed to load docks', err);
        this.message = 'Failed to load docks';
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

    this.http.post('http://localhost:5218/api/StorageArea', this.storageArea).subscribe({
      next: () => {
        this.ngZone.run(() => {
          this.isSuccess = true;
          this.message = 'Storage area created successfully!';
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
            'Failed to create storage area. Please try again.';
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
