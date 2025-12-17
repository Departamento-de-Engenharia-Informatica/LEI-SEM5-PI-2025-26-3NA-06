import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { timeout, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Component({
  selector: 'app-create-vvn',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-vvn.component.html',
  styleUrls: ['./create-vvn.component.css'],
})
export class CreateVvnComponent implements OnInit {
  vvn = {
    referredVesselId: '',
    arrivalDate: '',
    departureDate: '',
  };

  vessels: any[] = [];
  message: string = '';
  isLoading: boolean = false;
  isLoadingVessels: boolean = true;
  isSuccess: boolean = false;
  isDraft: boolean = false;

  constructor(
    private http: HttpClient,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit() {
    this.loadVessels();
  }

  loadVessels() {
    this.isLoadingVessels = true;
    this.http
      .get<any[]>('http://localhost:5218/api/Vessel')
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
            this.vessels = data;
            this.isLoadingVessels = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Failed to load vessels', err);
          this.ngZone.run(() => {
            this.message = 'Failed to load vessels. Please try again.';
            this.isLoadingVessels = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  saveDraft() {
    this.isDraft = true;
    this.onSubmit();
  }

  submitVVN() {
    this.isDraft = false;
    if (!this.validateForm(false)) {
      return;
    }
    this.onSubmit();
  }

  onSubmit() {
    if (this.isDraft && !this.validateForm(true)) {
      return;
    }

    this.isLoading = true;
    this.message = '';

    const endpoint = this.isDraft
      ? 'http://localhost:5218/api/VesselVisitNotification/draft'
      : 'http://localhost:5218/api/VesselVisitNotification/submit';

    const payload = {
      referredVesselId: this.vvn.referredVesselId,
      arrivalDate: this.vvn.arrivalDate || null,
      departureDate: this.vvn.departureDate || null,
    };

    this.http
      .post(endpoint, payload)
      .pipe(
        timeout(20000),
        catchError((err) => {
          console.error('HTTP error:', err);
          return throwError(() => err);
        })
      )
      .subscribe({
        next: (response: any) => {
          this.ngZone.run(() => {
            this.isSuccess = true;
            this.message = this.isDraft
              ? 'Vessel Visit Notification draft saved successfully!'
              : 'Vessel Visit Notification submitted successfully!';
            this.isLoading = false;
            this.cdr.detectChanges();

            setTimeout(() => {
              this.router.navigate([
                this.isDraft ? '/shipping-agent/vvn-drafts' : '/shipping-agent/vvn-submitted',
              ]);
            }, 2000);
          });
        },
        error: (error) => {
          this.ngZone.run(() => {
            this.isSuccess = false;
            this.message =
              error.error?.message ||
              error.error?.Message ||
              'Failed to save notification. Please try again.';
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  validateForm(isDraft: boolean): boolean {
    if (!this.vvn.referredVesselId || this.vvn.referredVesselId.trim() === '') {
      this.message = 'Vessel is required';
      this.cdr.detectChanges();
      return false;
    }

    if (!isDraft) {
      if (!this.vvn.arrivalDate) {
        this.message = 'Arrival date is required for submission';
        this.cdr.detectChanges();
        return false;
      }

      if (!this.vvn.departureDate) {
        this.message = 'Departure date is required for submission';
        this.cdr.detectChanges();
        return false;
      }

      if (new Date(this.vvn.arrivalDate) >= new Date(this.vvn.departureDate)) {
        this.message = 'Departure date must be after arrival date';
        this.cdr.detectChanges();
        return false;
      }
    }

    return true;
  }

  cancel() {
    this.router.navigate(['/shipping-agent']);
  }
}
