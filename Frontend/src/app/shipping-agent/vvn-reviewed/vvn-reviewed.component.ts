import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { timeout, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Component({
  selector: 'app-vvn-reviewed',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vvn-reviewed.component.html',
  styleUrls: ['./vvn-reviewed.component.css'],
})
export class VvnReviewedComponent implements OnInit {
  reviewed: any[] = [];
  filteredReviewed: any[] = [];
  vessels: any[] = [];
  docks: any[] = [];
  searchTerm: string = '';
  filterStatus: string = 'all';
  filterTimeType: string = 'all'; // 'all', 'arrival', 'departure'
  filterStartDate: string = '';
  filterEndDate: string = '';
  isLoading: boolean = true;
  message: string = '';
  successMessage: string = '';

  constructor(
    private http: HttpClient,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit() {
    this.loadVessels();
    this.loadDocks();
    this.loadReviewed();
  }

  loadVessels() {
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
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Failed to load vessels', err);
        },
      });
  }

  getVesselDisplay(imo: string): string {
    const vessel = this.vessels.find((v) => v.imo === imo);
    return vessel ? `${vessel.vesselName} (${imo})` : imo;
  }

  loadDocks() {
    this.http
      .get<any[]>('http://localhost:5218/api/Dock')
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
            this.docks = data;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Failed to load docks', err);
        },
      });
  }

  getDockDisplay(dockId: string): string {
    const dock = this.docks.find((d) => d.id === dockId);
    return dock ? dock.dockName : dockId;
  }

  loadReviewed() {
    this.isLoading = true;
    this.message = '';

    this.http
      .get<any[]>('http://localhost:5218/api/VesselVisitNotification/reviewed')
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
            this.reviewed = data;
            this.applyFilters();
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Failed to load reviewed VVNs', err);
          this.ngZone.run(() => {
            if (err.status === 401) {
              this.message = 'Authentication required. Please log in again.';
              setTimeout(() => {
                this.router.navigate(['/login']);
              }, 2000);
            } else {
              this.message =
                err.error?.message ||
                `Failed to load reviewed notifications. Error: ${err.status || 'Unknown'}`;
            }
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  goBack() {
    this.router.navigate(['/shipping-agent']);
  }

  applyFilters() {
    let filtered = this.reviewed;

    // Filter by status
    if (this.filterStatus !== 'all') {
      filtered = filtered.filter((vvn) => vvn.status?.toLowerCase() === this.filterStatus);
    }

    // Filter by search term
    const search = this.searchTerm.toLowerCase().trim();
    if (search) {
      filtered = filtered.filter(
        (vvn) =>
          vvn.referredVesselId?.toLowerCase().includes(search) ||
          vvn.id?.toLowerCase().includes(search)
      );
    }

    // Filter by time (arrival or departure)
    if (this.filterTimeType !== 'all' && (this.filterStartDate || this.filterEndDate)) {
      filtered = filtered.filter((vvn) => {
        const dateToCheck = this.filterTimeType === 'arrival' ? vvn.arrivalDate : vvn.departureDate;

        if (!dateToCheck) return false;

        const vvnDate = new Date(dateToCheck);

        if (this.filterStartDate && this.filterEndDate) {
          const start = new Date(this.filterStartDate);
          const end = new Date(this.filterEndDate);
          end.setHours(23, 59, 59, 999); // Include full end day
          return vvnDate >= start && vvnDate <= end;
        } else if (this.filterStartDate) {
          const start = new Date(this.filterStartDate);
          return vvnDate >= start;
        } else if (this.filterEndDate) {
          const end = new Date(this.filterEndDate);
          end.setHours(23, 59, 59, 999);
          return vvnDate <= end;
        }

        return true;
      });
    }

    this.filteredReviewed = filtered;
  }

  onFilterChange() {
    this.applyFilters();
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleString();
  }

  getStatusClass(status: string): string {
    return status?.toLowerCase() || '';
  }

  resubmit(vvnId: string) {
    // Navigate to edit form so user can make changes before resubmitting
    this.router.navigate(['/shipping-agent/edit-vvn', vvnId]);
  }
}
