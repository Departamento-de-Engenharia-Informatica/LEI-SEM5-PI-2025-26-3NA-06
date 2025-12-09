import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-create-dock',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-dock.component.html',
  styleUrls: ['./create-dock.component.css'],
})
export class CreateDockComponent implements OnInit {
  dock = {
    dockName: '',
    locationDescription: '',
    length: 0,
    depth: 0,
    maxDraft: 0,
    allowedVesselTypeIds: [] as string[],
  };

  vesselTypes: any[] = [];
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
    this.loadVesselTypes();
  }

  loadVesselTypes() {
    this.http
      .get<any[]>('http://localhost:5218/api/VesselType', { withCredentials: true })
      .subscribe({
        next: (data) => {
          this.ngZone.run(() => {
            this.vesselTypes = data;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Failed to load vessel types', err);
          this.message = 'Failed to load vessel types';
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
      .post('http://localhost:5218/api/Dock', this.dock, { withCredentials: true })
      .subscribe({
        next: (response: any) => {
          this.ngZone.run(() => {
            this.isSuccess = true;
            this.message = 'Dock created successfully!';
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
              'Failed to create dock. Please try again.';
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
