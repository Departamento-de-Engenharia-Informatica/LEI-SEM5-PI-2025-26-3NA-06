import { Component, OnInit, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  IncidentTypesService,
  IncidentType,
  CreateIncidentTypeDto,
  UpdateIncidentTypeDto,
} from '../../services/incident-types.service';

@Component({
  selector: 'app-incident-types',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './incident-types.component.html',
  styleUrls: ['./incident-types.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IncidentTypesComponent implements OnInit {
  incidentTypes: IncidentType[] = [];
  hierarchyData: IncidentType[] = [];
  inactiveTypes: IncidentType[] = [];
  loading = false;
  error: string | null = null;
  successMessage: string | null = null;

  // View mode: 'list', 'hierarchy', or 'inactive'
  viewMode: 'list' | 'hierarchy' | 'inactive' = 'hierarchy';

  // Modal state
  showCreateModal = false;
  showEditModal = false;
  showDeleteModal = false;
  selectedType: IncidentType | null = null;

  // Form data
  formData: CreateIncidentTypeDto = {
    code: '',
    name: '',
    description: '',
    severity: 'Minor',
    parentId: null,
  };

  editFormData: UpdateIncidentTypeDto = {};

  // Filters
  filterSeverity: string = '';
  searchText: string = '';

  constructor(private incidentTypesService: IncidentTypesService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadIncidentTypes();
  }

  loadIncidentTypes() {
    this.loading = true;
    this.error = null;

    if (this.viewMode === 'hierarchy') {
      this.incidentTypesService.getIncidentTypesHierarchy().subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.hierarchyData = response.data;
          } else {
            this.error = response.error || 'Failed to load incident types';
          }
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load incident types';
          this.loading = false;
          this.cdr.markForCheck();
        },
      });
    } else if (this.viewMode === 'inactive') {
      this.incidentTypesService.getAllIncidentTypes(true).subscribe({
        next: (response) => {
          if (response.success && response.data) {
            // Backend now returns only inactive types, no need to filter
            this.inactiveTypes = response.data;
          } else {
            this.error = response.error || 'Failed to load inactive incident types';
          }
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load inactive incident types';
          this.loading = false;
          this.cdr.markForCheck();
        },
      });
    } else {
      this.incidentTypesService.getAllIncidentTypes(false).subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.incidentTypes = response.data;
          } else {
            this.error = response.error || 'Failed to load incident types';
          }
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to load incident types';
          this.loading = false;
          this.cdr.markForCheck();
        },
      });
    }
  }

  switchViewMode(mode: 'list' | 'hierarchy' | 'inactive') {
    this.viewMode = mode;
    this.loadIncidentTypes();
  }

  getFilteredTypes(): IncidentType[] {
    let filtered = this.incidentTypes;

    if (this.filterSeverity) {
      filtered = filtered.filter((t) => t.severity === this.filterSeverity);
    }

    if (this.searchText) {
      const search = this.searchText.toLowerCase();
      filtered = filtered.filter(
        (t) =>
          t.code.toLowerCase().includes(search) ||
          t.name.toLowerCase().includes(search) ||
          t.description?.toLowerCase().includes(search)
      );
    }

    return filtered;
  }

  openCreateModal() {
    this.formData = {
      code: '',
      name: '',
      description: '',
      severity: 'Minor',
      parentId: null,
    };
    this.showCreateModal = true;
    this.error = null;
    this.successMessage = null;

    // Load active types for parent dropdown if not already loaded
    if (this.incidentTypes.length === 0) {
      this.incidentTypesService.getAllIncidentTypes(false).subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.incidentTypes = response.data;
            this.cdr.markForCheck();
          }
        },
      });
    }
  }

  closeCreateModal() {
    this.showCreateModal = false;
    this.formData = {
      code: '',
      name: '',
      description: '',
      severity: 'Minor',
      parentId: null,
    };
  }

  createIncidentType() {
    this.error = null;
    this.successMessage = null;

    // Validation
    if (!this.formData.code || !this.formData.name || !this.formData.severity) {
      this.error = 'Code, Name, and Severity are required';
      this.cdr.markForCheck();
      return;
    }

    this.loading = true;
    this.incidentTypesService.createIncidentType(this.formData).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = 'Incident type created successfully';
          this.closeCreateModal();
          this.loadIncidentTypes();
        } else {
          this.error = response.error || 'Failed to create incident type';
        }
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = err.error?.error || 'Failed to create incident type';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  openEditModal(type: IncidentType) {
    this.selectedType = type;
    this.editFormData = {
      name: type.name,
      description: type.description || '',
      severity: type.severity,
      parentId: type.parentId,
      isActive: type.isActive,
    };
    this.showEditModal = true;
    this.error = null;
    this.successMessage = null;

    // Load active types for parent dropdown if not already loaded
    if (this.incidentTypes.length === 0) {
      this.incidentTypesService.getAllIncidentTypes(false).subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.incidentTypes = response.data;
            this.cdr.markForCheck();
          }
        },
      });
    }
  }

  closeEditModal() {
    this.showEditModal = false;
    this.selectedType = null;
    this.editFormData = {};
  }

  updateIncidentType() {
    if (!this.selectedType) return;

    this.error = null;
    this.successMessage = null;

    this.loading = true;
    this.incidentTypesService
      .updateIncidentType(this.selectedType.id, this.editFormData)
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.successMessage = 'Incident type updated successfully';
            this.closeEditModal();
            this.loadIncidentTypes();
          } else {
            this.error = response.error || 'Failed to update incident type';
          }
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.error = err.error?.error || 'Failed to update incident type';
          this.loading = false;
          this.cdr.markForCheck();
        },
      });
  }

  openDeleteModal(type: IncidentType) {
    this.selectedType = type;
    this.showDeleteModal = true;
    this.error = null;
    this.successMessage = null;
  }

  closeDeleteModal() {
    this.showDeleteModal = false;
    this.selectedType = null;
  }

  deleteIncidentType() {
    if (!this.selectedType) return;

    this.loading = true;
    this.incidentTypesService.deleteIncidentType(this.selectedType.id).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = response.message || 'Incident type deleted successfully';
          this.closeDeleteModal();
          this.loadIncidentTypes();
        } else {
          this.error = response.error || 'Failed to delete incident type';
        }
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = err.error?.error || 'Failed to delete incident type';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  activateIncidentType(type: IncidentType) {
    this.loading = true;
    this.error = null;
    this.incidentTypesService.activateIncidentType(type.id).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = response.message || 'Incident type activated successfully';
          this.loadIncidentTypes();
        } else {
          this.error = response.error || 'Failed to activate incident type';
        }
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = err.error?.error || 'Failed to activate incident type';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  getSeverityBadgeClass(severity: string): string {
    switch (severity) {
      case 'Minor':
        return 'badge badge-info';
      case 'Major':
        return 'badge badge-warning';
      case 'Critical':
        return 'badge badge-danger';
      default:
        return 'badge badge-secondary';
    }
  }

  getAllTypesFlat(): IncidentType[] {
    const flatten = (types: IncidentType[]): IncidentType[] => {
      let result: IncidentType[] = [];
      for (const type of types) {
        result.push(type);
        if (type.children && type.children.length > 0) {
          result = result.concat(flatten(type.children));
        }
      }
      return result;
    };

    // Flatten both hierarchyData and incidentTypes (for list view)
    if (this.viewMode === 'hierarchy' && this.hierarchyData.length > 0) {
      return flatten(this.hierarchyData);
    } else if (this.incidentTypes.length > 0) {
      return this.incidentTypes;
    }

    return [];
  }

  clearMessages() {
    this.error = null;
    this.successMessage = null;
    this.cdr.markForCheck();
  }
}
