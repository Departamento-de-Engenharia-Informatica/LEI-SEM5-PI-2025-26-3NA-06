import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { timeout, catchError } from 'rxjs/operators';
import { throwError, forkJoin } from 'rxjs';

@Component({
  selector: 'app-docks',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './docks.component.html',
  styleUrls: ['./docks.component.css'],
})
export class DocksComponent implements OnInit {
  docks: any[] = [];
  filteredDocks: any[] = [];
  vesselTypes: any[] = [];
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
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    this.message = '';

    forkJoin({
      vesselTypes: this.http.get<any[]>('http://localhost:5218/api/VesselType', {
        withCredentials: true,
      }),
      docks: this.http.get<any[]>('http://localhost:5218/api/Dock', { withCredentials: true }),
    })
      .pipe(
        timeout(20000),
        catchError((err) => {
          console.error('HTTP error:', err);
          return throwError(() => err);
        })
      )
      .subscribe({
        next: ({ vesselTypes, docks }) => {
          this.ngZone.run(() => {
            this.vesselTypes = vesselTypes;
            this.docks = docks;
            this.filteredDocks = docks;
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Failed to load data', err);

          this.ngZone.run(() => {
            if (err.name === 'TimeoutError') {
              this.message = 'Request timed out. Please check if the backend is running.';
            } else if (err.status === 0) {
              this.message =
                'Cannot connect to server. Please check if the backend is running on port 5218.';
            } else if (err.status === 401) {
              this.message = 'Authentication required. Please log in again.';
              setTimeout(() => {
                this.router.navigate(['/login']);
              }, 2000);
            } else if (err.status === 403) {
              this.message = 'You do not have permission to view docks.';
            } else {
              this.message =
                err.error?.message || `Failed to load docks. Error: ${err.status || 'Unknown'}`;
            }

            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  filterDocks() {
    if (!this.searchTerm.trim()) {
      this.filteredDocks = this.docks;
      return;
    }

    const searchLower = this.searchTerm.toLowerCase();

    this.filteredDocks = this.docks.filter((dock) => {
      // Search by dock name
      if (dock.dockName.toLowerCase().includes(searchLower)) {
        return true;
      }

      // Search by location description
      if (dock.locationDescription.toLowerCase().includes(searchLower)) {
        return true;
      }

      // Search by allowed vessel type names
      if (dock.allowedVesselTypeIds && dock.allowedVesselTypeIds.length > 0) {
        const dockVesselTypes = this.vesselTypes.filter((vt) =>
          dock.allowedVesselTypeIds.includes(vt.id)
        );
        return dockVesselTypes.some((vt) => vt.typeName.toLowerCase().includes(searchLower));
      }

      return false;
    });
  }

  goBack() {
    this.router.navigate(['/port-authority']);
  }

  editDock(id: string) {
    this.router.navigate(['/port-authority/edit-dock', id]);
  }

  createDock() {
    this.router.navigate(['/port-authority/create-dock']);
  }

  getVesselTypeNames(vesselTypeIds: string[]): string {
    if (!vesselTypeIds || vesselTypeIds.length === 0) {
      return 'None';
    }
    const names = vesselTypeIds
      .map((id) => {
        const type = this.vesselTypes.find((vt) => vt.id === id);
        return type ? type.typeName : null;
      })
      .filter((name) => name !== null);
    return names.length > 0 ? names.join(', ') : 'None';
  }
}
