import { Component, OnInit, NgZone, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { timeout, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Component({
  selector: 'app-edit-dock',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './edit-dock.component.html',
  styleUrls: ['./edit-dock.component.css'],
})
export class EditDockComponent implements OnInit {
  dockId: string = '';
  dock = {
    dockName: '',
    locationDescription: '',
    length: 0,
    depth: 0,
    maxDraft: 0,
    allowedVesselTypeIds: [] as string[],
  };
  vesselTypes: any[] = [];
  isLoading: boolean = true;
  isLoadingTypes: boolean = true;
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
    this.dockId = this.route.snapshot.params['id'];
    if (this.dockId) {
      this.loadVesselTypes();
      this.loadDock();
    } else {
      this.message = 'Invalid dock ID';
      this.isLoading = false;
    }
  }

  loadVesselTypes() {
    this.isLoadingTypes = true;

    this.http
      .get<any[]>('http://localhost:5218/api/VesselType', { withCredentials: true })
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
            this.vesselTypes = data;
            this.isLoadingTypes = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Failed to load vessel types', err);
          this.ngZone.run(() => {
            this.message = 'Failed to load vessel types';
            this.isLoadingTypes = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  loadDock() {
    this.isLoading = true;
    this.message = '';

    this.http
      .get<any>(`http://localhost:5218/api/Dock/${this.dockId}`, {
        withCredentials: true,
      })
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
            this.dock = {
              dockName: data.dockName || '',
              locationDescription: data.locationDescription || '',
              length: data.length || 0,
              depth: data.depth || 0,
              maxDraft: data.maxDraft || 0,
              allowedVesselTypeIds: data.allowedVesselTypeIds || [],
            };
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          this.ngZone.run(() => {
            if (err.status === 404) {
              this.message = 'Dock not found';
            } else if (err.status === 401) {
              this.message = 'Authentication required. Please log in again.';
              setTimeout(() => {
                this.router.navigate(['/login']);
              }, 2000);
            } else if (err.status === 403) {
              this.message = 'You do not have permission to view this dock.';
            } else {
              this.message = `Failed to load dock. Error: ${err.status || 'Unknown'}`;
            }
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  toggleVesselType(vesselTypeId: string) {
    const index = this.dock.allowedVesselTypeIds.indexOf(vesselTypeId);
    if (index > -1) {
      this.dock.allowedVesselTypeIds.splice(index, 1);
    } else {
      this.dock.allowedVesselTypeIds.push(vesselTypeId);
    }
  }

  isVesselTypeSelected(vesselTypeId: string): boolean {
    return this.dock.allowedVesselTypeIds.includes(vesselTypeId);
  }

  onSubmit() {
    if (!this.validateForm()) {
      return;
    }

    this.isLoading = true;
    this.message = '';

    this.http
      .put(`http://localhost:5218/api/Dock/${this.dockId}`, this.dock, { withCredentials: true })
      .subscribe({
        next: (response: any) => {
          this.ngZone.run(() => {
            this.isSuccess = true;
            this.message = 'Dock updated successfully!';
            this.isLoading = false;
            this.cdr.detectChanges();

            setTimeout(() => {
              this.router.navigate(['/port-authority/docks']);
            }, 2000);
          });
        },
        error: (error) => {
          this.ngZone.run(() => {
            this.isSuccess = false;
            this.message =
              error.error.message ||
              error.error.Message ||
              'Failed to update dock. Please try again.';
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  validateForm(): boolean {
    if (!this.dock.dockName || this.dock.dockName.trim() === '') {
      this.message = 'Dock name is required';
      this.cdr.detectChanges();
      return false;
    }

    if (!this.dock.locationDescription || this.dock.locationDescription.trim() === '') {
      this.message = 'Location description is required';
      this.cdr.detectChanges();
      return false;
    }

    if (this.dock.length <= 0) {
      this.message = 'Length must be greater than 0';
      this.cdr.detectChanges();
      return false;
    }

    if (this.dock.depth <= 0) {
      this.message = 'Depth must be greater than 0';
      this.cdr.detectChanges();
      return false;
    }

    if (this.dock.maxDraft <= 0) {
      this.message = 'Max draft must be greater than 0';
      this.cdr.detectChanges();
      return false;
    }

    return true;
  }

  goBack() {
    this.router.navigate(['/port-authority/docks']);
  }
}
