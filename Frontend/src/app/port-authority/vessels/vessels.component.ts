import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { timeout, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Component({
  selector: 'app-vessels',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vessels.component.html',
  styleUrls: ['./vessels.component.css'],
})
export class VesselsComponent implements OnInit {
  vessels: any[] = [];
  filteredVessels: any[] = [];
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
    this.loadVessels();
  }

  loadVessels() {
    this.isLoading = true;
    this.message = '';

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
          console.log('Vessels loaded:', data);
          this.ngZone.run(() => {
            this.vessels = data;
            this.filteredVessels = data;
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Failed to load vessels', err);

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
              this.message = 'You do not have permission to view vessels.';
            } else {
              this.message =
                err.error?.message || `Failed to load vessels. Error: ${err.status || 'Unknown'}`;
            }

            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  createNew() {
    this.router.navigate(['/port-authority/create-vessel']);
  }

  editVessel(imo: string) {
    this.router.navigate(['/port-authority/edit-vessel', imo]);
  }

  goBack() {
    this.router.navigate(['/port-authority']);
  }

  filterVessels() {
    const search = this.searchTerm.toLowerCase().trim();

    if (!search) {
      this.filteredVessels = this.vessels;
    } else {
      this.filteredVessels = this.vessels.filter(
        (vessel) =>
          vessel.vesselName?.toLowerCase().includes(search) ||
          vessel.imo?.toLowerCase().includes(search)
      );
    }
    this.cdr.detectChanges();
  }
}
