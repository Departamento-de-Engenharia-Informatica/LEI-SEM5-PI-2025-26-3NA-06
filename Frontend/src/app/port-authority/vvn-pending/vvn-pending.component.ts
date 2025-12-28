import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { timeout, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Component({
  selector: 'app-vvn-pending',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vvn-pending.component.html',
  styleUrls: ['./vvn-pending.component.css'],
})
export class VvnPendingComponent implements OnInit {
  pending: any[] = [];
  filteredPending: any[] = [];
  vessels: any[] = [];
  docks: any[] = [];
  containers: any[] = [];
  storageAreas: any[] = [];
  searchTerm: string = '';
  isLoading: boolean = true;
  message: string = '';
  successMessage: string = '';
  errorMessage: string = '';

  // Modal state
  showReviewModal: boolean = false;
  selectedVvn: any = null;
  selectedDockId: string = '';
  rejectionReason: string = '';
  isApproving: boolean = false;

  constructor(
    private http: HttpClient,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit() {
    this.loadVessels();
    this.loadDocks();
    this.loadContainers();
    this.loadStorageAreas();
    this.loadPending();
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

  loadContainers() {
    this.http
      .get<any[]>('http://localhost:5218/api/Container')
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
            this.containers = data;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Failed to load containers', err);
        },
      });
  }

  loadStorageAreas() {
    this.http
      .get<any[]>('http://localhost:5218/api/StorageArea')
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
            this.storageAreas = data;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Failed to load storage areas', err);
        },
      });
  }

  getVesselDisplay(imo: string): string {
    const vessel = this.vessels.find((v) => v.imo === imo);
    return vessel ? `${vessel.vesselName} (${imo})` : imo;
  }

  getContainerDisplay(containerId: string): string {
    const container = this.containers.find((c) => c.id === containerId);
    if (container) {
      const hazardBadge = container.isHazardous ? ' ⚠️ HAZMAT' : '';
      return `${container.isoCode} - ${container.cargoType}${hazardBadge}`;
    }
    return containerId;
  }

  getStorageAreaDisplay(areaId: string): string {
    const area = this.storageAreas.find((a) => a.id === areaId);
    return area ? area.areaName : areaId;
  }

  loadPending() {
    this.isLoading = true;
    this.message = '';

    this.http
      .get<any[]>('http://localhost:5218/api/VesselVisitNotification/pending')
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
            this.pending = data;
            this.filteredPending = data;
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          this.ngZone.run(() => {
            this.isLoading = false;
            this.message = 'Failed to load pending VVNs';
            console.error('Failed to load pending VVNs', err);
            this.cdr.detectChanges();
          });
        },
      });
  }

  search() {
    const term = this.searchTerm.toLowerCase();
    this.filteredPending = this.pending.filter(
      (vvn) =>
        vvn.referredVesselId.toLowerCase().includes(term) ||
        this.getVesselDisplay(vvn.referredVesselId).toLowerCase().includes(term)
    );
  }

  openReviewModal(vvn: any) {
    this.selectedVvn = vvn;
    this.selectedDockId = '';
    this.rejectionReason = '';
    this.isApproving = false;
    this.errorMessage = '';
    this.showReviewModal = true;
  }

  closeReviewModal() {
    this.showReviewModal = false;
    this.selectedVvn = null;
    this.selectedDockId = '';
    this.rejectionReason = '';
    this.errorMessage = '';
  }

  setApproving(value: boolean) {
    this.isApproving = value;
    this.errorMessage = '';
  }

  approveVvn(confirmConflict: boolean = false) {
    if (!this.selectedDockId) {
      this.errorMessage = 'Please select a dock for approval.';
      return;
    }

    this.http
      .post(`http://localhost:5218/api/VesselVisitNotification/${this.selectedVvn.id}/approve`, {
        tempAssignedDockId: this.selectedDockId,
        confirmDockConflict: confirmConflict,
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
            this.successMessage = 'VVN approved successfully!';
            this.closeReviewModal();
            this.loadPending();
            setTimeout(() => (this.successMessage = ''), 3000);
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          this.ngZone.run(() => {
            // Check if this is a dock conflict error (HTTP 409)
            if (err.status === 409 && err.error?.requiresConfirmation) {
              const conflictMessage = err.error.message;
              const confirmed = confirm(
                `⚠️ DOCK CONFLICT WARNING ⚠️\n\n${conflictMessage}\n\nDo you want to approve this VVN anyway?`
              );

              if (confirmed) {
                // Retry with confirmation flag
                this.approveVvn(true);
              }
            } else {
              this.errorMessage = err.error?.message || 'Failed to approve VVN';
            }
            this.cdr.detectChanges();
          });
        },
      });
  }

  rejectVvn() {
    if (!this.rejectionReason.trim()) {
      this.errorMessage = 'Please provide a rejection reason.';
      return;
    }

    this.http
      .post(`http://localhost:5218/api/VesselVisitNotification/${this.selectedVvn.id}/reject`, {
        rejectionReason: this.rejectionReason,
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
            this.successMessage = 'VVN rejected successfully!';
            this.closeReviewModal();
            this.loadPending();
            setTimeout(() => (this.successMessage = ''), 3000);
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          this.ngZone.run(() => {
            this.errorMessage = err.error?.message || 'Failed to reject VVN';
            this.cdr.detectChanges();
          });
        },
      });
  }

  getManifestCount(vvn: any): number {
    let count = 0;
    if (vvn.loadingManifest) count++;
    if (vvn.unloadingManifest) count++;
    return count;
  }

  isHazardous(vvn: any): boolean {
    // Use the VVN's isHazardous property directly from backend
    return vvn.isHazardous === true;
  }

  goBack() {
    this.router.navigate(['/port-authority']);
  }
}
