import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-create-vessel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-vessel.component.html',
  styleUrls: ['./create-vessel.component.css'],
})
export class CreateVesselComponent implements OnInit {
  vessel = {
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
    this.http.get<any[]>('http://localhost:5218/api/VesselType').subscribe({
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

  onSubmit() {
    // Validate form before submitting
    if (!this.validateForm()) {
      return;
    }

    this.isLoading = true;
    this.message = '';

    this.http.post('http://localhost:5218/api/Vessel', this.vessel).subscribe({
      next: (response: any) => {
        this.isSuccess = true;
        this.message = 'Vessel created successfully!';
        this.isLoading = false;

        setTimeout(() => {
          this.router.navigate(['/port-authority/vessels']);
        }, 2000);
      },
      error: (error) => {
        this.isSuccess = false;
        this.message =
          error.error.message ||
          error.error.Message ||
          'Failed to create vessel. Please try again.';
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  validateForm(): boolean {
    if (!this.vessel.imo || this.vessel.imo.trim() === '') {
      this.message = 'IMO number is required';
      this.cdr.detectChanges();
      return false;
    }

    if (!this.vessel.vesselName || this.vessel.vesselName.trim() === '') {
      this.message = 'Vessel name is required';
      this.cdr.detectChanges();
      return false;
    }

    if (!this.vessel.capacity) {
      this.message = 'Capacity is required';
      this.cdr.detectChanges();
      return false;
    }

    if (!this.vessel.rows) {
      this.message = 'Rows is required';
      this.cdr.detectChanges();
      return false;
    }

    if (!this.vessel.bays) {
      this.message = 'Bays is required';
      this.cdr.detectChanges();
      return false;
    }

    if (!this.vessel.tiers) {
      this.message = 'Tiers is required';
      this.cdr.detectChanges();
      return false;
    }

    if (!this.vessel.length) {
      this.message = 'Length is required';
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
    this.router.navigate(['/port-authority']);
  }
}
