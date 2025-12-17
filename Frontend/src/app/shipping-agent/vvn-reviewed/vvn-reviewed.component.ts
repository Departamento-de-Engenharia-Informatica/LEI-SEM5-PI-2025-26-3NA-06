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
  searchTerm: string = '';
  filterStatus: string = 'all';
  isLoading: boolean = true;
  message: string = '';

  constructor(
    private http: HttpClient,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit() {
    this.loadReviewed();
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
}
