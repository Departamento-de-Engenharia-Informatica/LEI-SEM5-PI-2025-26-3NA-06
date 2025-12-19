import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { timeout, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Component({
  selector: 'app-vvn-drafts',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vvn-drafts.component.html',
  styleUrls: ['./vvn-drafts.component.css'],
})
export class VvnDraftsComponent implements OnInit {
  drafts: any[] = [];
  filteredDrafts: any[] = [];
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
    this.loadDrafts();
  }

  loadDrafts() {
    this.isLoading = true;
    this.message = '';

    this.http
      .get<any[]>('http://localhost:5218/api/VesselVisitNotification/drafts')
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
            this.drafts = data;
            this.filteredDrafts = data;
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Failed to load drafts', err);
          this.ngZone.run(() => {
            if (err.name === 'TimeoutError') {
              this.message = 'Request timed out. Please check if the backend is running.';
            } else if (err.status === 0) {
              this.message = 'Cannot connect to server. Please check if the backend is running.';
            } else if (err.status === 401) {
              this.message = 'Authentication required. Please log in again.';
              setTimeout(() => {
                this.router.navigate(['/login']);
              }, 2000);
            } else {
              this.message =
                err.error?.message || `Failed to load drafts. Error: ${err.status || 'Unknown'}`;
            }
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  createNew() {
    this.router.navigate(['/shipping-agent/create-vvn']);
  }

  editDraft(id: string) {
    this.router.navigate(['/shipping-agent/edit-vvn', id]);
  }

  deleteDraft(id: string) {
    if (!confirm('Are you sure you want to delete this draft?')) {
      return;
    }

    this.http
      .delete(`http://localhost:5218/api/VesselVisitNotification/drafts/${id}`, {
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
        next: () => {
          this.ngZone.run(() => {
            this.message = 'Draft deleted successfully';
            this.cdr.detectChanges();
            // Delay reload to allow user to see the success message
            setTimeout(() => {
              this.loadDrafts();
            }, 2000);
          });
        },
        error: (err) => {
          this.ngZone.run(() => {
            this.message = err.error?.message || 'Failed to delete draft';
            this.cdr.detectChanges();
          });
        },
      });
  }

  goBack() {
    this.router.navigate(['/shipping-agent']);
  }

  filterDrafts() {
    const search = this.searchTerm.toLowerCase().trim();

    if (!search) {
      this.filteredDrafts = this.drafts;
    } else {
      this.filteredDrafts = this.drafts.filter(
        (draft) =>
          draft.referredVesselId?.toLowerCase().includes(search) ||
          draft.id?.toLowerCase().includes(search)
      );
    }
  }

  formatDate(dateString: string | null): string {
    if (!dateString) return 'Not set';
    const date = new Date(dateString);
    return date.toLocaleString();
  }
}
