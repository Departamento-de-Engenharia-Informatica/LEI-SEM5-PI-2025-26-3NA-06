import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { timeout, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Component({
  selector: 'app-vvn-submitted',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vvn-submitted.component.html',
  styleUrls: ['./vvn-submitted.component.css'],
})
export class VvnSubmittedComponent implements OnInit {
  submitted: any[] = [];
  filteredSubmitted: any[] = [];
  searchTerm: string = '';
  isLoading: boolean = true;
  message: string = '';

  constructor(
    private http: HttpClient,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit() {
    this.loadSubmitted();
  }

  loadSubmitted() {
    this.isLoading = true;
    this.message = '';

    this.http
      .get<any[]>('http://localhost:5218/api/VesselVisitNotification/submitted')
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
            this.submitted = data;
            this.filteredSubmitted = data;
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Failed to load submitted VVNs', err);
          this.ngZone.run(() => {
            if (err.status === 401) {
              this.message = 'Authentication required. Please log in again.';
              setTimeout(() => {
                this.router.navigate(['/login']);
              }, 2000);
            } else {
              this.message =
                err.error?.message ||
                `Failed to load submitted notifications. Error: ${err.status || 'Unknown'}`;
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

  filterSubmitted() {
    const search = this.searchTerm.toLowerCase().trim();

    if (!search) {
      this.filteredSubmitted = this.submitted;
    } else {
      this.filteredSubmitted = this.submitted.filter(
        (vvn) =>
          vvn.referredVesselId?.toLowerCase().includes(search) ||
          vvn.id?.toLowerCase().includes(search)
      );
    }
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleString();
  }
}
