import {
  Component,
  OnInit,
  ChangeDetectorRef,
  ChangeDetectionStrategy,
  NgZone,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OemService } from '../../services/oem.service';
import { timeout, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';

@Component({
  selector: 'app-operation-plans-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './operation-plans-list.html',
  styleUrls: ['./operation-plans-list.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OperationPlansListComponent implements OnInit {
  operationPlans: any[] = [];
  loading = false;
  error: string | null = null;
  searched = false;
  selectedPlan: any = null;

  // Cargo manifest properties
  selectedVvnId: string | null = null;
  cargoManifests: any = null;
  loadingManifests = false;
  manifestsError: string | null = null;

  filters = {
    startDate: '',
    endDate: '',
  };

  sortField: string = 'planDate';
  sortDirection: 'asc' | 'desc' = 'desc';

  constructor(
    private oemService: OemService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit() {
    // Load today's plans by default
    const today = new Date().toISOString().split('T')[0];
    this.filters.startDate = today;
    this.filters.endDate = today;
    this.searchPlans();
  }

  searchPlans() {
    this.loading = true;
    this.error = null;
    this.searched = true;

    const params: any = {};

    if (this.filters.startDate) {
      params.startDate = this.filters.startDate;
    }
    if (this.filters.endDate) {
      params.endDate = this.filters.endDate;
    }

    this.oemService
      .searchOperationPlans(params)
      .pipe(
        timeout(20000),
        catchError((err) => {
          console.error('HTTP error:', err);
          return throwError(() => err);
        })
      )
      .subscribe({
        next: (response) => {
          console.log('Operation plans loaded:', response);
          if (response.success) {
            this.operationPlans = (response.data || []).map((plan: any) => ({
              ...plan,
              assignmentsCount: plan.assignments?.length || 0,
            }));
            this.sortPlans();
          } else {
            this.error = response.error || 'Failed to load operation plans';
            this.operationPlans = [];
          }
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (err) => {
          console.error('Error loading operation plans:', err);
          this.error = err.error?.error || err.message || 'Failed to load operation plans';
          this.operationPlans = [];
          this.loading = false;
          this.cdr.markForCheck();
        },
      });
  }

  clearFilters() {
    this.filters = {
      startDate: '',
      endDate: '',
    };
    this.operationPlans = [];
    this.searched = false;
    this.error = null;
    this.cdr.markForCheck();
  }

  sortBy(field: string) {
    if (this.sortField === field) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortField = field;
      this.sortDirection = 'asc';
    }
    this.sortPlans();
    this.cdr.markForCheck();
  }

  sortPlans() {
    this.operationPlans.sort((a, b) => {
      let aValue = a[this.sortField];
      let bValue = b[this.sortField];

      // Handle null/undefined values
      if (aValue === null || aValue === undefined) aValue = '';
      if (bValue === null || bValue === undefined) bValue = '';

      // Convert to comparable format
      if (typeof aValue === 'string') aValue = aValue.toLowerCase();
      if (typeof bValue === 'string') bValue = bValue.toLowerCase();

      // Handle dates
      if (this.sortField === 'planDate' || this.sortField === 'creationDate') {
        aValue = new Date(aValue).getTime();
        bValue = new Date(bValue).getTime();
      }

      // Handle booleans
      if (this.sortField === 'isFeasible') {
        aValue = aValue ? 1 : 0;
        bValue = bValue ? 1 : 0;
      }

      let comparison = 0;
      if (aValue < bValue) comparison = -1;
      if (aValue > bValue) comparison = 1;

      return this.sortDirection === 'asc' ? comparison : -comparison;
    });
  }

  getSortIcon(field: string): string {
    if (this.sortField !== field) return 'fa-sort';
    return this.sortDirection === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
  }

  viewDetails(plan: any) {
    this.selectedPlan = plan;
  }

  closeDetails() {
    this.selectedPlan = null;
  }

  formatWarning(warning: string): string {
    // Replace [Dock X] with styled badge
    return warning.replace(/\[Dock ([^\]]+)\]:/g, '<strong class="dock-badge">Dock $1</strong>:');
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'NotStarted':
        return 'badge badge-secondary';
      case 'InProgress':
        return 'badge badge-primary';
      case 'Finished':
        return 'badge badge-success';
      default:
        return 'badge badge-secondary';
    }
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'NotStarted':
        return '⏳ Not Started';
      case 'InProgress':
        return '🔄 In Progress';
      case 'Finished':
        return '✓ Finished';
      default:
        return '❓ Unknown';
    }
  }

  getDockGroups(): any[] {
    if (!this.selectedPlan || !this.selectedPlan.assignments) {
      return [];
    }

    // Group assignments by dockId
    const dockMap = new Map<string, any>();

    this.selectedPlan.assignments.forEach((assignment: any) => {
      const dockId = assignment.dockId;
      if (!dockMap.has(dockId)) {
        dockMap.set(dockId, {
          dockId: dockId,
          dockName: assignment.dockName || `Dock ${dockId.substring(0, 8)}...`,
          assignments: [],
        });
      }
      dockMap.get(dockId).assignments.push(assignment);
    });

    // Sort assignments within each dock by ETA
    Array.from(dockMap.values()).forEach((dock) => {
      dock.assignments.sort((a: any, b: any) => {
        const etaA = new Date(a.eta).getTime();
        const etaB = new Date(b.eta).getTime();
        return etaA - etaB;
      });
    });

    return Array.from(dockMap.values());
  }

  viewCargoManifests(vvnId: string) {
    this.selectedVvnId = vvnId;
    this.loadingManifests = true;
    this.manifestsError = null;
    this.cargoManifests = null;

    this.oemService
      .getCargoManifests(vvnId)
      .pipe(
        timeout(10000),
        catchError((err) => {
          console.error('Error loading cargo manifests:', err);
          return throwError(() => err);
        })
      )
      .subscribe({
        next: (response) => {
          this.ngZone.run(() => {
            if (response.success) {
              this.cargoManifests = response.data;
            } else {
              this.manifestsError = response.error || 'Failed to load cargo manifests';
            }
            this.loadingManifests = false;
            this.cdr.markForCheck();
          });
        },
        error: (err) => {
          this.ngZone.run(() => {
            console.error('Error in cargo manifests request:', err);
            this.manifestsError =
              err.error?.error || err.message || 'Failed to load cargo manifests';
            this.loadingManifests = false;
            this.cdr.markForCheck();
          });
        },
      });
  }

  closeCargoManifests() {
    this.selectedVvnId = null;
    this.cargoManifests = null;
    this.loadingManifests = false;
    this.manifestsError = null;
    this.cdr.markForCheck();
  }
}
