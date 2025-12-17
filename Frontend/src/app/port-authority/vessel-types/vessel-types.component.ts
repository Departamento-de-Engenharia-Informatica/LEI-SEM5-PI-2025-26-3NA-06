import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { timeout, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Component({
  selector: 'app-vessel-types',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vessel-types.component.html',
  styleUrls: ['./vessel-types.component.css'],
})
export class VesselTypesComponent implements OnInit {
  vesselTypes: any[] = [];
  filteredVesselTypes: any[] = [];
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
    this.loadVesselTypes();
  }

  loadVesselTypes() {
    this.isLoading = true;
    this.message = '';

    this.http
      .get<any[]>('http://localhost:5218/api/VesselType')
      .pipe(
        timeout(20000), // 20 second timeout
        catchError((err) => {
          console.error('HTTP error:', err);
          return throwError(() => err);
        })
      )
      .subscribe({
        next: (data) => {
          console.log('Vessel types loaded:', data);
          this.ngZone.run(() => {
            this.vesselTypes = data;
            this.filteredVesselTypes = data;
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Failed to load vessel types', err);

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
              this.message = 'You do not have permission to view vessel types.';
            } else {
              this.message =
                err.error?.message ||
                `Failed to load vessel types. Error: ${err.status || 'Unknown'}`;
            }

            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  createNew() {
    this.router.navigate(['/port-authority/create-vessel-type']);
  }

  editVesselType(id: string) {
    this.router.navigate(['/port-authority/edit-vessel-type', id]);
  }

  goBack() {
    this.router.navigate(['/port-authority']);
  }

  filterVesselTypes() {
    const search = this.searchTerm.toLowerCase().trim();

    if (!search) {
      this.filteredVesselTypes = this.vesselTypes;
    } else {
      this.filteredVesselTypes = this.vesselTypes.filter(
        (type) =>
          type.typeName.toLowerCase().includes(search) ||
          (type.typeDescription && type.typeDescription.toLowerCase().includes(search))
      );
    }
    this.cdr.detectChanges();
  }
}
