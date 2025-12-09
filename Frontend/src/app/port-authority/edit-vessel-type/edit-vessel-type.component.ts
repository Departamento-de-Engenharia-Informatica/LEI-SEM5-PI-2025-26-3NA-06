import { Component, OnInit, NgZone, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { timeout, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Component({
  selector: 'app-edit-vessel-type',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './edit-vessel-type.component.html',
  styleUrls: ['./edit-vessel-type.component.css'],
})
export class EditVesselTypeComponent implements OnInit {
  vesselTypeId: string = '';
  vesselType = {
    typeName: '',
    typeDescription: '',
    typeCapacity: 0,
    maxRows: 0,
    maxBays: 0,
    maxTiers: 0,
  };
  isLoading: boolean = true;
  isSuccess: boolean = false;
  message: string = '';

  constructor(
    private http: HttpClient,
    private router: Router,
    private route: ActivatedRoute,
    private ngZone: NgZone,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.vesselTypeId = this.route.snapshot.params['id'];
    if (this.vesselTypeId) {
      this.loadVesselType();
    } else {
      this.message = 'Invalid vessel type ID';
      this.isLoading = false;
    }
  }

  loadVesselType() {
    this.isLoading = true;
    this.message = '';

    this.http
      .get<any>(`http://localhost:5218/api/VesselType/${this.vesselTypeId}`, {
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
            this.vesselType = {
              typeName: data.typeName,
              typeDescription: data.typeDescription,
              typeCapacity: data.typeCapacity,
              maxRows: data.maxRows,
              maxBays: data.maxBays,
              maxTiers: data.maxTiers,
            };
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          this.ngZone.run(() => {
            if (err.status === 404) {
              this.message = 'Vessel type not found';
            } else if (err.status === 401) {
              this.message = 'Authentication required. Please log in again.';
              setTimeout(() => this.router.navigate(['/login']), 2000);
            } else if (err.status === 403) {
              this.message = 'You do not have permission to edit vessel types.';
            } else {
              this.message = err.error?.message || 'Failed to load vessel type';
            }
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  onSubmit() {
    if (this.validateForm()) {
      this.message = '';
      this.isSuccess = false;

      this.http
        .put(`http://localhost:5218/api/VesselType/${this.vesselTypeId}`, this.vesselType, {
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
          next: (response) => {
            this.ngZone.run(() => {
              this.isSuccess = true;
              this.message = 'Vessel type updated successfully!';
              this.cdr.detectChanges();

              setTimeout(() => {
                this.router.navigate(['/port-authority/vessel-types']);
              }, 2000);
            });
          },
          error: (err) => {
            this.ngZone.run(() => {
              this.isSuccess = false;
              this.message =
                err.error?.message ||
                err.error?.Message ||
                'Failed to update vessel type. Please try again.';
              this.cdr.detectChanges();
            });
          },
        });
    }
  }

  validateForm(): boolean {
    if (!this.vesselType.typeName.trim()) {
      this.message = 'Type name is required';
      this.isSuccess = false;
      return false;
    }

    if (!this.vesselType.typeDescription.trim()) {
      this.message = 'Type description is required';
      this.isSuccess = false;
      return false;
    }

    if (this.vesselType.typeCapacity <= 0) {
      this.message = 'Capacity must be a positive number';
      this.isSuccess = false;
      return false;
    }

    if (this.vesselType.maxRows <= 0) {
      this.message = 'Max rows must be a positive number';
      this.isSuccess = false;
      return false;
    }

    if (this.vesselType.maxBays <= 0) {
      this.message = 'Max bays must be a positive number';
      this.isSuccess = false;
      return false;
    }

    if (this.vesselType.maxTiers <= 0) {
      this.message = 'Max tiers must be a positive number';
      this.isSuccess = false;
      return false;
    }

    return true;
  }

  cancel() {
    this.router.navigate(['/port-authority/vessel-types']);
  }
}
