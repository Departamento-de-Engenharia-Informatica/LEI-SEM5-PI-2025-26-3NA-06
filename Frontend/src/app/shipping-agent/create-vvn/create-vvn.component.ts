import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { timeout, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

interface ManifestEntry {
  containerId: string;
  sourceStorageAreaId?: string;
  targetStorageAreaId?: string;
}

interface CargoManifest {
  manifestType: string;
  entries: ManifestEntry[];
}

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

  // Manifest data
  loadingEntries: ManifestEntry[] = [];
  unloadingEntries: ManifestEntry[] = [];

  // For adding new entries
  newLoadEntry = { containerId: '', sourceStorageAreaId: '' };
  newUnloadEntry = { containerId: '', targetStorageAreaId: '' };

  vessels: any[] = [];
  containers: any[] = [];
  storageAreas: any[] = [];
  message: string = '';
  isLoading: boolean = false;
  isLoadingVessels: boolean = true;
  isLoadingContainers: boolean = true;
  isLoadingStorageAreas: boolean = true;
  isSuccess: boolean = false;
  isDraft: boolean = false;
  isEditMode: boolean = false;
  draftId: string | null = null;
  isEditingRejectedVVN: boolean = false;

  // UI state for manifest sections
  showLoadingManifest: boolean = false;
  showUnloadingManifest: boolean = false;

  constructor(
    private http: HttpClient,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit() {
    // Check if we're in edit mode
    this.route.params.subscribe((params) => {
      if (params['id']) {
        this.isEditMode = true;
        this.draftId = params['id'];
        if (this.draftId) {
          this.loadDraft(this.draftId);
        }
      }
    });

    this.loadVessels();
    this.loadContainers();
    this.loadStorageAreas();
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

  loadContainers() {
    this.isLoadingContainers = true;
    const token = localStorage.getItem('token');
    this.http
      .get<any[]>('http://localhost:5218/api/Container', {
        headers: { Authorization: `Bearer ${token}` },
      })
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
            this.isLoadingContainers = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Failed to load containers', err);
          this.ngZone.run(() => {
            this.isLoadingContainers = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  loadDraft(id: string) {
    this.isLoading = true;
    // Try loading as draft first
    this.http
      .get<any>(`http://localhost:5218/api/VesselVisitNotification/drafts/${id}`)
      .pipe(
        timeout(20000),
        catchError((err) => {
          // If 404, try loading as reviewed VVN (for rejected VVNs being edited)
          if (err.status === 404) {
            console.log('Draft not found, trying reviewed endpoint...');
            this.isEditingRejectedVVN = true; // Mark that we're editing a rejected VVN
            return this.http
              .get<any>(`http://localhost:5218/api/VesselVisitNotification/reviewed/${id}`)
              .pipe(
                timeout(20000),
                catchError((reviewedErr) => {
                  console.error('HTTP error loading reviewed VVN:', reviewedErr);
                  return throwError(() => reviewedErr);
                })
              );
          }
          console.error('HTTP error:', err);
          return throwError(() => err);
        })
      )
      .subscribe({
        next: (data) => {
          this.ngZone.run(() => {
            console.log('Loaded VVN data:', data);
            this.vvn.referredVesselId = data.referredVesselId || data.ReferredVesselId;

            // Handle both camelCase and PascalCase property names from backend
            const arrivalDate = data.arrivalDate || data.ArrivalDate;
            const departureDate = data.departureDate || data.DepartureDate;

            console.log('Raw arrivalDate:', arrivalDate);
            console.log('Raw departureDate:', departureDate);

            const formattedArrival = arrivalDate ? this.formatDateForInput(arrivalDate) : '';
            const formattedDeparture = departureDate ? this.formatDateForInput(departureDate) : '';

            console.log('Formatted arrivalDate:', formattedArrival);
            console.log('Formatted departureDate:', formattedDeparture);

            this.vvn.arrivalDate = formattedArrival;
            this.vvn.departureDate = formattedDeparture;

            console.log('vvn.arrivalDate assigned:', this.vvn.arrivalDate);
            console.log('vvn.departureDate assigned:', this.vvn.departureDate);

            // Load manifests if they exist
            const loadingManifest = data.loadingManifest || data.LoadingManifest;
            const unloadingManifest = data.unloadingManifest || data.UnloadingManifest;

            if (loadingManifest && loadingManifest.entries) {
              this.loadingEntries = loadingManifest.entries;
              this.showLoadingManifest = true;
            }
            if (unloadingManifest && unloadingManifest.entries) {
              this.unloadingEntries = unloadingManifest.entries;
              this.showUnloadingManifest = true;
            }

            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          this.ngZone.run(() => {
            this.message = 'Failed to load VVN. Please try again.';
            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  formatDateForInput(dateString: string): string {
    // Convert ISO date to datetime-local format (YYYY-MM-DDTHH:mm)
    if (!dateString) return '';

    const date = new Date(dateString);
    const year = date.getFullYear();

    // HTML5 datetime-local inputs don't support years < 1000
    // Also treat year 1 (0001) as invalid since it's C#'s default DateTime
    if (isNaN(date.getTime()) || year < 1000) {
      console.log('Invalid or default date detected, returning empty string');
      return '';
    }

    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  loadStorageAreas() {
    this.isLoadingStorageAreas = true;
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
            this.isLoadingStorageAreas = false;
            this.cdr.detectChanges();
          });
        },
        error: (err) => {
          console.error('Failed to load storage areas', err);
          this.ngZone.run(() => {
            this.isLoadingStorageAreas = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  // Loading manifest management
  addLoadEntry() {
    if (!this.newLoadEntry.containerId || !this.newLoadEntry.sourceStorageAreaId) {
      this.message = 'Container and source storage area are required for loading entries';
      return;
    }

    // Check for duplicates
    if (this.loadingEntries.some((e) => e.containerId === this.newLoadEntry.containerId)) {
      this.message = 'Container already exists in loading manifest';
      return;
    }
    if (this.unloadingEntries.some((e) => e.containerId === this.newLoadEntry.containerId)) {
      this.message =
        'Container already exists in unloading manifest. Same container cannot be in both.';
      return;
    }

    this.loadingEntries.push({
      containerId: this.newLoadEntry.containerId,
      sourceStorageAreaId: this.newLoadEntry.sourceStorageAreaId,
    });

    this.newLoadEntry = { containerId: '', sourceStorageAreaId: '' };
    this.message = '';
  }

  removeLoadEntry(index: number) {
    this.loadingEntries.splice(index, 1);
  }

  // Unloading manifest management
  addUnloadEntry() {
    if (!this.newUnloadEntry.containerId || !this.newUnloadEntry.targetStorageAreaId) {
      this.message = 'Container and target storage area are required for unloading entries';
      return;
    }

    // Check for duplicates
    if (this.unloadingEntries.some((e) => e.containerId === this.newUnloadEntry.containerId)) {
      this.message = 'Container already exists in unloading manifest';
      return;
    }
    if (this.loadingEntries.some((e) => e.containerId === this.newUnloadEntry.containerId)) {
      this.message =
        'Container already exists in loading manifest. Same container cannot be in both.';
      return;
    }

    this.unloadingEntries.push({
      containerId: this.newUnloadEntry.containerId,
      targetStorageAreaId: this.newUnloadEntry.targetStorageAreaId,
    });

    this.newUnloadEntry = { containerId: '', targetStorageAreaId: '' };
    this.message = '';
  }

  removeUnloadEntry(index: number) {
    this.unloadingEntries.splice(index, 1);
  }

  getContainerDisplay(containerId: string): string {
    const container = this.containers.find((c) => c.id === containerId);
    return container ? `${container.isoCode} (${container.cargoType})` : containerId;
  }

  getStorageAreaDisplay(areaId: string): string {
    const area = this.storageAreas.find((a) => a.id === areaId);
    return area ? area.areaName : areaId;
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

    let endpoint: string;
    let httpMethod: any;

    if (this.isDraft && this.isEditMode && this.draftId && !this.isEditingRejectedVVN) {
      // Update existing draft (only if it's actually a draft, not a rejected VVN)
      endpoint = `http://localhost:5218/api/VesselVisitNotification/drafts/${this.draftId}`;
      httpMethod = this.http.put(endpoint, this.buildPayload());
    } else if (this.isDraft && !this.isEditMode) {
      // Create new draft
      endpoint = 'http://localhost:5218/api/VesselVisitNotification/draft';
      httpMethod = this.http.post(endpoint, this.buildPayload());
    } else if (this.isDraft && this.isEditingRejectedVVN) {
      // Convert rejected VVN back to draft status (not creating a new one)
      endpoint = `http://localhost:5218/api/VesselVisitNotification/${this.draftId}/convert-to-draft`;
      httpMethod = this.http.put(endpoint, this.buildPayload());
    } else if (this.isEditMode && this.draftId && !this.isEditingRejectedVVN) {
      // Submit existing draft for review - first update it with current form data
      const payload = this.buildPayload();
      this.http
        .put(`http://localhost:5218/api/VesselVisitNotification/drafts/${this.draftId}`, payload)
        .pipe(
          timeout(20000),
          catchError((err) => {
            console.error('HTTP error updating draft:', err);
            return throwError(() => err);
          })
        )
        .subscribe({
          next: () => {
            // Draft updated, now submit it
            endpoint = `http://localhost:5218/api/VesselVisitNotification/drafts/${this.draftId}/submit`;
            httpMethod = this.http.post(endpoint, {});
            this.executeSubmit(httpMethod);
          },
          error: (error: any) => {
            this.ngZone.run(() => {
              this.isLoading = false;
              this.isSuccess = false;
              this.message = error.error?.message || 'Failed to update draft before submission';
              this.cdr.detectChanges();
            });
          },
        });
      return; // Exit early since we're handling the submission in the callback
    } else if (this.isEditingRejectedVVN) {
      // Update and resubmit rejected VVN (changes status back to Submitted)
      endpoint = `http://localhost:5218/api/VesselVisitNotification/${this.draftId}/resubmit`;
      httpMethod = this.http.put(endpoint, this.buildPayload());
    } else {
      // Submit new VVN directly
      endpoint = 'http://localhost:5218/api/VesselVisitNotification/submit';
      httpMethod = this.http.post(endpoint, this.buildPayload());
    }

    this.executeSubmit(httpMethod);
  }

  executeSubmit(httpMethod: any) {
    httpMethod
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
            this.message =
              this.isEditMode && this.isDraft
                ? 'Draft updated successfully!'
                : this.isEditMode && !this.isDraft
                ? 'Vessel Visit Notification submitted successfully!'
                : this.isDraft
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
        error: (error: any) => {
          this.ngZone.run(() => {
            console.error('Failed to save VVN:', error);
            console.error('Error details:', error.error);

            this.isSuccess = false;

            // Extract validation errors if present
            if (error.error?.errors) {
              const errorMessages = Object.entries(error.error.errors)
                .map(([field, messages]: [string, any]) => {
                  const msgArray = Array.isArray(messages) ? messages : [messages];
                  return `${field}: ${msgArray.join(', ')}`;
                })
                .join('\n');
              this.message = `Validation errors:\n${errorMessages}`;
            } else {
              this.message =
                error.error?.message ||
                error.error?.Message ||
                error.error?.title ||
                'Failed to save notification. Please try again.';
            }

            this.isLoading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  buildPayload(): any {
    // Helper to safely convert date string to ISO or null
    const toISODateOrNull = (dateStr: string) => {
      if (!dateStr || dateStr.trim() === '') return null;
      const date = new Date(dateStr);
      return isNaN(date.getTime()) ? null : date.toISOString();
    };

    const payload: any = {
      referredVesselId: this.vvn.referredVesselId,
      arrivalDate: toISODateOrNull(this.vvn.arrivalDate),
      departureDate: toISODateOrNull(this.vvn.departureDate),
    };

    // Add manifests if they have entries
    if (this.loadingEntries.length > 0) {
      payload.loadingManifest = {
        manifestType: 'Load',
        entries: this.loadingEntries,
      };
    }

    if (this.unloadingEntries.length > 0) {
      payload.unloadingManifest = {
        manifestType: 'Unload',
        entries: this.unloadingEntries,
      };
    }

    return payload;
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
