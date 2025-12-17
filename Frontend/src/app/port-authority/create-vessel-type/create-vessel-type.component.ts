import { Component, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-create-vessel-type',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-vessel-type.component.html',
  styleUrls: ['./create-vessel-type.component.css'],
})
export class CreateVesselTypeComponent {
  vesselType = {
    typeName: '',
    typeDescription: '',
    typeCapacity: 0,
    maxRows: 0,
    maxBays: 0,
    maxTiers: 0,
  };

  message: string = '';
  isLoading: boolean = false;
  isSuccess: boolean = false;

  constructor(
    private http: HttpClient,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  onSubmit() {
    this.isLoading = true;
    this.message = '';

    this.http.post('http://localhost:5218/api/VesselType', this.vesselType).subscribe({
      next: (response: any) => {
        this.ngZone.run(() => {
          this.isSuccess = true;
          this.message = 'Vessel type created successfully!';
          this.isLoading = false;
          this.cdr.detectChanges();

          setTimeout(() => {
            this.router.navigate(['/port-authority']);
          }, 2000);
        });
      },
      error: (error) => {
        this.ngZone.run(() => {
          this.isSuccess = false;
          this.message = error.error?.message || 'Failed to create vessel type. Please try again.';
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      },
    });
  }

  cancel() {
    this.router.navigate(['/port-authority']);
  }
}
