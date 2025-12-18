import { Component, OnInit, NgZone, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { timeout, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Component({
  selector: 'app-edit-vessel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './edit-vessel.component.html',
  styleUrls: ['./edit-vessel.component.css'],
})
export class EditVesselComponent implements OnInit {
  vesselImo: string = '';
  vesselId: string = '';
  vessel = {
    id: '',
    imo: '',
    vesselName: '',
    capacity: 0,
    rows: 0,
    bays: 0,
    tiers: 0,
    length: 0,
    vesselTypeId: '',
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
    this.vesselImo = this.route.snapshot.params['imo'];
    this.vesselId = this.vesselImo; // Use IMO as ID
    if (this.vesselImo) {
      this.loadVesselTypes();
      this.loadVessel();
    } else {
      this.message = 'Invalid vessel IMO';
      this.isLoading = false;
    }
  }

  loadVesselTypes() {
    this.isLoadingTypes = true;

    this.http
      .get<any[]>('http://localhost:5218/api/VesselType')
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

  loadVessel() {
    this.isLoading = true;
    this.message = '';

    this.http
      .get<any>(`http://localhost:5218/api/Vessel/imo/${this.vesselImo}`)
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
            this.vessel = {
              id: data.id || '',
              imo: data.imo || this.vesselImo,
              vesselName: data.vesselName || '',
              capacity: data.capacity || 0,
              rows: data.rows || 0,
              bays: data.bays || 0,
              tiers: data.tiers || 0,
              length: data.length || 0,
              vesselTypeId: data.vesselTypeId || '',
            };
            this.vesselId = data.id; // Store the actual vessel ID for updates
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          this.ngZone.run(() => {
            if (err.status === 404) {
              this.message = 'Vessel not found';
            } else if (err.status === 401) {
              this.message = 'Authentication required. Please log in again.';
              setTimeout(() => this.router.navigate(['/login']), 2000);
            } else if (err.status === 403) {
              this.message = 'You do not have permission to edit vessels.';
            } else {
              this.message = err.error?.message || 'Failed to load vessel';
            }
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  onSubmit() {
    if (!this.validateForm()) {
      return;
    }

    this.message = '';
    this.isSuccess = false;

    this.http
      .put(`http://localhost:5218/api/Vessel/${this.vesselId}`, this.vessel)
      .pipe(
        timeout(20000),
        catchError((err) => {
          console.error('HTTP error:', err);
          return throwError(() => err);
        })
      )
      .subscribe({
        next: (response) => {
          this.ngZone.run(() => {
            this.isSuccess = true;
            this.message = 'Vessel updated successfully!';
            this.cdr.detectChanges();

            setTimeout(() => {
              this.router.navigate(['/port-authority/vessels']);
            }, 2000);
          });
        },
        error: (err) => {
          this.ngZone.run(() => {
            this.isSuccess = false;
            this.message =
              err.error?.message ||
              err.error?.Message ||
              'Failed to update vessel. Please try again.';
            this.cdr.detectChanges();
          });
        },
      });
  }

  validateForm(): boolean {
    if (!this.vessel.vesselName.trim()) {
      this.message = 'Vessel name is required';
      this.cdr.detectChanges();
      return false;
    }

    if (!this.vessel.imo.trim()) {
      this.message = 'IMO number is required';
      this.cdr.detectChanges();
      return false;
    }

    if (this.vessel.capacity <= 0) {
      this.message = 'Capacity must be greater than 0';
      this.cdr.detectChanges();
      return false;
    }

    if (this.vessel.rows <= 0) {
      this.message = 'Rows must be greater than 0';
      this.cdr.detectChanges();
      return false;
    }

    if (this.vessel.bays <= 0) {
      this.message = 'Bays must be greater than 0';
      this.cdr.detectChanges();
      return false;
    }

    if (this.vessel.tiers <= 0) {
      this.message = 'Tiers must be greater than 0';
      this.cdr.detectChanges();
      return false;
    }

    if (this.vessel.length <= 0) {
      this.message = 'Length must be greater than 0';
      this.cdr.detectChanges();
      return false;
    }

    if (!this.vessel.vesselTypeId) {
      this.message = 'Vessel type is required';
      this.cdr.detectChanges();
      return false;
    }

    return true;
  }

  cancel() {
    this.router.navigate(['/port-authority/vessels']);
  }
}
