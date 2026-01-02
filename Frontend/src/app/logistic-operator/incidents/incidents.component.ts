import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  IncidentsService,
  Incident,
  CreateIncidentDto,
  UpdateIncidentDto,
  IncidentFilters,
} from '../../services/incidents.service';
import { IncidentTypesService, IncidentType } from '../../services/incident-types.service';

@Component({
  selector: 'app-incidents',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './incidents.component.html',
  styleUrls: ['./incidents.component.css'],
})
export class IncidentsComponent implements OnInit {
  incidents: Incident[] = [];
  incidentTypes: IncidentType[] = [];
  loading = false;
  error: string | null = null;
  successMessage: string | null = null;

  // Modal state
  showCreateModal = false;
  showEditModal = false;
  showDeleteModal = false;
  selectedIncident: Incident | null = null;

  // Form data
  formData: CreateIncidentDto = {
    incidentTypeId: '',
    startTime: '',
    endTime: null,
    description: '',
    affectsAllVVEs: false,
    affectedVVEIds: [],
  };

  editFormData: UpdateIncidentDto = {};

  // Filters
  filters: IncidentFilters = {
    date: new Date().toISOString().split('T')[0], // Today by default
    status: undefined,
    incidentTypeId: undefined,
  };

  // For affected VVEs input
  affectedVVEInput: string = '';

  constructor(
    private incidentsService: IncidentsService,
    private incidentTypesService: IncidentTypesService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.loadIncidents();
    // Load incident types for filters, but don't block if it fails
    this.loadIncidentTypes();
  }

  loadIncidents() {
    this.loading = true;
    this.error = null;

    this.incidentsService.getAllIncidents(this.filters).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.incidents = response.data;
        } else {
          this.error = response.error || 'Failed to load incidents';
        }
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = err.error?.error || 'Failed to load incidents';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  loadIncidentTypes() {
    this.incidentTypesService.getAllIncidentTypes(false).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.incidentTypes = response.data;
          this.cdr.markForCheck();
        }
      },
      error: (err) => {
        console.error('Failed to load incident types:', err);
        // Don't show error to user since this is just for filters
        // The filter will just be empty if types fail to load
      },
    });
  }

  applyFilters() {
    this.loadIncidents();
  }

  clearFilters() {
    this.filters = {
      date: new Date().toISOString().split('T')[0],
      status: undefined,
      incidentTypeId: undefined,
    };
    this.loadIncidents();
  }

  openCreateModal() {
    this.loadIncidentTypes();

    // Set default values
    const now = new Date();
    const timeStr = now.toTimeString().split(' ')[0].substring(0, 5); // HH:MM format

    this.formData = {
      incidentTypeId: '',
      startTime: `${new Date().toISOString().split('T')[0]}T${timeStr}`,
      endTime: null,
      description: '',
      affectsAllVVEs: false,
      affectedVVEIds: [],
    };
    this.affectedVVEInput = '';
    this.showCreateModal = true;
    this.error = null;
    this.successMessage = null;
  }

  closeCreateModal() {
    this.showCreateModal = false;
    this.formData = {
      incidentTypeId: '',
      startTime: '',
      endTime: null,
      description: '',
      affectsAllVVEs: false,
      affectedVVEIds: [],
    };
    this.affectedVVEInput = '';
  }

  createIncident() {
    // Parse affected VVEs from input
    if (!this.formData.affectsAllVVEs && this.affectedVVEInput.trim()) {
      this.formData.affectedVVEIds = this.affectedVVEInput
        .split(',')
        .map((id) => id.trim())
        .filter((id) => id.length > 0);
    }

    this.loading = true;
    this.error = null;

    this.incidentsService.createIncident(this.formData).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = 'Incident created successfully!';
          this.closeCreateModal();
          this.loadIncidents();
          setTimeout(() => {
            this.successMessage = null;
            this.cdr.markForCheck();
          }, 3000);
        } else {
          this.error = response.error || 'Failed to create incident';
        }
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = err.error?.error || 'Failed to create incident';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  openEditModal(incident: Incident) {
    this.selectedIncident = incident;

    // Extract time from datetime for time inputs
    const startTime = incident.startTime
      ? new Date(incident.startTime).toTimeString().slice(0, 5)
      : '';
    const endTime = incident.endTime ? new Date(incident.endTime).toTimeString().slice(0, 5) : null;

    this.editFormData = {
      startTime: startTime,
      endTime: endTime,
      description: incident.description,
      affectsAllVVEs: incident.affectsAllVVEs,
      affectedVVEIds: [...incident.affectedVVEIds],
    };
    this.affectedVVEInput = incident.affectedVVEIds.join(', ');
    this.showEditModal = true;
    this.error = null;
    this.successMessage = null;
  }

  closeEditModal() {
    this.showEditModal = false;
    this.selectedIncident = null;
    this.editFormData = {};
    this.affectedVVEInput = '';
  }

  updateIncident() {
    if (!this.selectedIncident) return;

    this.loading = true;
    this.error = null;

    // Convert time inputs to full datetime using the incident's date
    const incidentDate = this.selectedIncident.date.split('T')[0];
    const updateData: any = {
      startTime: `${incidentDate}T${this.editFormData.startTime}:00`,
      endTime: this.editFormData.endTime ? `${incidentDate}T${this.editFormData.endTime}:00` : null,
      description: this.editFormData.description,
      affectsAllVVEs: this.editFormData.affectsAllVVEs,
      affectedVVEIds: this.editFormData.affectedVVEIds,
    };

    this.incidentsService.updateIncident(this.selectedIncident.id, updateData).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = 'Incident updated successfully!';
          this.closeEditModal();
          this.loadIncidents();
          setTimeout(() => {
            this.successMessage = null;
            this.cdr.markForCheck();
          }, 3000);
        } else {
          this.error = response.error || 'Failed to update incident';
        }
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = err.error?.error || 'Failed to update incident';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  openDeleteModal(incident: Incident) {
    this.selectedIncident = incident;
    this.showDeleteModal = true;
    this.error = null;
  }

  closeDeleteModal() {
    this.showDeleteModal = false;
    this.selectedIncident = null;
  }

  deleteIncident() {
    if (!this.selectedIncident) return;

    this.loading = true;
    this.error = null;

    this.incidentsService.deleteIncident(this.selectedIncident.id).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = 'Incident deleted successfully!';
          this.closeDeleteModal();
          this.loadIncidents();
          setTimeout(() => {
            this.successMessage = null;
            this.cdr.markForCheck();
          }, 3000);
        } else {
          this.error = response.error || 'Failed to delete incident';
        }
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = err.error?.error || 'Failed to delete incident';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  getSelectedIncidentType(): IncidentType | undefined {
    return this.incidentTypes.find((type) => type.id === this.formData.incidentTypeId);
  }

  getSeverityClass(severity: string): string {
    switch (severity) {
      case 'Minor':
        return 'severity-minor';
      case 'Major':
        return 'severity-major';
      case 'Critical':
        return 'severity-critical';
      default:
        return '';
    }
  }

  getStatusClass(status: string | boolean): string {
    // Handle both status string and isActive boolean
    const isActive = typeof status === 'boolean' ? status : status === 'Active';
    return isActive ? 'status-active' : 'status-inactive';
  }

  getStatusText(incident: any): string {
    // Check isActive field if status is not properly set
    if (incident.status && incident.status !== 'Unknown') {
      return incident.status;
    }
    return incident.isActive ? 'Active' : 'Inactive';
  }

  formatDateTime(datetime: string): string {
    return new Date(datetime).toLocaleString();
  }

  formatTime(datetime: string): string {
    return new Date(datetime).toLocaleTimeString();
  }

  formatDate(dateString: string): string {
    if (!dateString) return '';
    // Extract just YYYY-MM-DD from ISO string
    return dateString.split('T')[0];
  }
}
