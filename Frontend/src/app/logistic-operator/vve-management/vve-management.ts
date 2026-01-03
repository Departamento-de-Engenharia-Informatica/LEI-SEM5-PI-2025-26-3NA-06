import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import {
  OemService,
  VesselVisitExecution,
  PrepareTodaysVVEsResponse,
} from '../../services/oem.service';
import { VesselService } from '../../services/vessel.service';
import { IncidentsService, CreateIncidentDto } from '../../services/incidents.service';
import { IncidentTypesService, IncidentType } from '../../services/incident-types.service';

@Component({
  selector: 'app-vve-management',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './vve-management.html',
  styleUrls: ['./vve-management.css'],
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VveManagementComponent implements OnInit {
  today: string = '';
  loading: boolean = false;
  hasOperationPlan: boolean = false;
  vves: VesselVisitExecution[] = [];
  selectedVve: VesselVisitExecution | null = null;
  showDetailModal: boolean = false;
  showUpdateModal: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';
  prepareResult: PrepareTodaysVVEsResponse | null = null;
  operationPlanData: any = null; // Cache operation plan data
  dataReady: boolean = false; // Flag to control card display

  // For update form
  updateForm = {
    actualArrivalTime: '',
    actualDockId: '',
    actualDepartureTime: '',
    status: '',
  };

  docks: any[] = [];
  availableStatuses: string[] = [];

  // For incident recording
  showRecordIncidentModal: boolean = false;
  incidentSelectionMode: boolean = false;
  selectedVveIds: string[] = [];
  incidentTypes: IncidentType[] = [];
  todaysIncidents: any[] = []; // Store today's incidents
  incidentErrorMessage: string = '';
  incidentForm: CreateIncidentDto = {
    incidentTypeId: '',
    startTime: '',
    endTime: null,
    description: '',
    affectsAllVVEs: false,
    affectedVVEIds: [],
  };

  constructor(
    private oemService: OemService,
    private vesselService: VesselService,
    private incidentsService: IncidentsService,
    private incidentTypesService: IncidentTypesService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.today = new Date().toISOString().split('T')[0];
    // Load all data sequentially to ensure proper enrichment
    this.loadAllData();
    this.loadTodaysIncidents();
  }

  loadAllData(): void {
    console.log('🚀 Starting data load sequence...');
    this.loading = true;
    this.dataReady = false;
    this.errorMessage = '';

    // Step 1: Load docks first
    this.vesselService.getAllDocks().subscribe({
      next: (docks: any) => {
        this.docks = Array.isArray(docks) ? docks : [];
        console.log('✅ Step 1: Docks loaded:', this.docks.length);
        console.log('Dock sample:', this.docks.slice(0, 2));

        // Step 2: Check operation plan and load VVEs
        this.oemService.getOperationPlanByDate(this.today).subscribe({
          next: (planResponse) => {
            console.log('📅 Step 2: Operation plan response:', planResponse);
            this.hasOperationPlan = planResponse.success;

            if (planResponse.success && planResponse.data) {
              this.operationPlanData = planResponse.data;
              console.log('✅ Operation plan loaded:', {
                planId: this.operationPlanData.id,
                assignmentsCount: this.operationPlanData.assignments?.length || 0,
                sampleAssignment: this.operationPlanData.assignments?.[0],
              });

              // Step 3: Load VVEs
              this.oemService.getVVEsByDate(this.today).subscribe({
                next: (vveResponse) => {
                  console.log('📦 Step 3: VVE response:', vveResponse);

                  if (vveResponse.success && Array.isArray(vveResponse.data)) {
                    this.vves = vveResponse.data;
                    console.log('✅ VVEs loaded:', this.vves.length);
                    console.log('VVE sample:', this.vves.slice(0, 2));

                    // Step 4: Enrich VVEs with all available data
                    this.enrichVVEsSync();
                  } else {
                    console.log('ℹ️ No VVEs found or invalid response');
                    this.vves = [];
                    this.dataReady = true;
                  }
                  this.loading = false;
                  this.cdr.markForCheck();
                },
                error: (err) => {
                  console.error('❌ Error loading VVEs:', err);
                  this.vves = [];
                  this.loading = false;
                  this.dataReady = true;
                  this.cdr.markForCheck();
                },
              });
            } else {
              console.log('ℹ️ No operation plan for today');
              this.loading = false;
              this.dataReady = true;
              this.cdr.markForCheck();
            }
          },
          error: (err) => {
            console.error('❌ Error loading operation plan:', err);
            this.hasOperationPlan = false;
            this.loading = false;
            this.dataReady = true;
            this.cdr.markForCheck();
          },
        });
      },
      error: (err: any) => {
        console.error('Error loading docks:', err);
        this.loading = false;
        this.dataReady = true;
        this.cdr.markForCheck();
      },
    });
  }

  enrichVVEsSync(): void {
    if (!this.operationPlanData || !this.docks.length) {
      console.error('❌ Cannot enrich VVEs - missing data', {
        hasOperationPlan: !!this.operationPlanData,
        operationPlanAssignments: this.operationPlanData?.assignments?.length || 0,
        docksCount: this.docks.length,
      });
      // Don't set dataReady if we can't enrich
      this.dataReady = false;
      return;
    }

    console.log('🔄 Starting VVE enrichment...', {
      vvesCount: this.vves.length,
      assignmentsCount: this.operationPlanData.assignments?.length || 0,
      docksCount: this.docks.length,
    });

    let enrichedCount = 0;

    this.vves.forEach((vve) => {
      console.log(`📋 Processing VVE ${vve.id}:`, {
        vvnId: vve.vvnId,
        plannedDockId: vve.plannedDockId,
        status: vve.status,
        hasVesselName: !!vve.vesselName,
        hasPlannedDockName: !!vve.plannedDockName,
      });

      // Only enrich if data is not already present (backend now provides enriched data)
      if (!vve.vesselName || !vve.plannedDockName) {
        // Get vessel info from operation plan by matching vvnId (case-insensitive)
        const assignment = this.operationPlanData.assignments?.find(
          (a: any) => a.vvnId.toLowerCase() === vve.vvnId.toLowerCase()
        );

        if (assignment) {
          if (!vve.vesselName) vve.vesselName = assignment.vesselName;
          if (!vve.vesselImo) vve.vesselImo = assignment.vesselImo;
          if (!vve.plannedDockName) vve.plannedDockName = assignment.dockName;

          console.log(`✅ VVE ${vve.id} enriched from assignment:`, {
            vesselName: vve.vesselName,
            vesselImo: vve.vesselImo,
            plannedDockName: vve.plannedDockName,
          });
          enrichedCount++;
        } else {
          console.error(`❌ VVE ${vve.id}: No assignment found for vvnId ${vve.vvnId}`);
          console.log(
            'Available assignments:',
            this.operationPlanData.assignments?.map((a: any) => ({
              vvnId: a.vvnId,
              vesselName: a.vesselName,
            }))
          );
        }
      } else {
        console.log(`✅ VVE ${vve.id} already enriched by backend`);
        enrichedCount++;
      }

      // Get actual dock name if actualDockId is set (only during updates)
      if (vve.actualDockId) {
        const actualDock = this.docks.find(
          (d: any) => d.id.toLowerCase() === vve.actualDockId!.toLowerCase()
        );
        if (actualDock) {
          vve.actualDockName = actualDock.dockName;
          console.log(`🔄 VVE ${vve.id}: Actual dock ${vve.actualDockName}`);
        }
      }
    });

    console.log(`✅ VVE enrichment complete: ${enrichedCount}/${this.vves.length} enriched`);

    // Only set dataReady if ALL VVEs have been properly enriched
    if (enrichedCount === this.vves.length) {
      console.log('✅ All VVEs enriched successfully - showing cards');
      this.dataReady = true;
    } else {
      console.error(
        `❌ Only ${enrichedCount}/${this.vves.length} VVEs enriched - NOT showing cards`
      );
      this.dataReady = false;
    }

    this.cdr.markForCheck();
  }

  prepareTodaysVVEs(): void {
    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.oemService.prepareTodaysVVEs().subscribe({
      next: (response) => {
        this.prepareResult = response;
        if (response.success) {
          this.successMessage = response.message || 'VVEs prepared successfully';
          // Reload VVEs
          this.loadAllData();
        } else {
          this.errorMessage = response.error || 'Failed to prepare VVEs';
        }
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error preparing VVEs:', err);
        this.errorMessage = err.error?.error || 'Error preparing VVEs';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  openDetailModal(vve: VesselVisitExecution): void {
    this.selectedVve = vve;
    this.showDetailModal = true;
    this.cdr.markForCheck();
  }

  closeDetailModal(): void {
    this.showDetailModal = false;
    // Don't null selectedVve here to allow transition to update modal
    // selectedVve will be updated when opening update modal or closing update modal
    this.cdr.markForCheck();
  }

  openUpdateModal(vve: VesselVisitExecution | null): void {
    // Use provided vve or fallback to selectedVve (for modal transitions)
    const targetVve = vve || this.selectedVve;

    if (!targetVve) {
      console.error('No VVE selected for update');
      return;
    }

    this.selectedVve = targetVve;

    // Find the matching dock ID (case-insensitive) for pre-filling
    const dockIdToUse = targetVve.actualDockId || targetVve.plannedDockId;
    const matchingDock = this.docks.find(
      (d: any) => d.id.toLowerCase() === dockIdToUse.toLowerCase()
    );

    this.updateForm = {
      // Pre-fill with actual values if they exist, otherwise use planned values as defaults
      actualArrivalTime: targetVve.actualArrivalTime
        ? new Date(targetVve.actualArrivalTime).toISOString().slice(0, 16)
        : new Date(targetVve.plannedArrivalTime).toISOString().slice(0, 16),
      actualDockId: matchingDock
        ? matchingDock.id
        : targetVve.actualDockId || targetVve.plannedDockId,
      actualDepartureTime: targetVve.actualDepartureTime
        ? new Date(targetVve.actualDepartureTime).toISOString().slice(0, 16)
        : new Date(targetVve.plannedDepartureTime).toISOString().slice(0, 16),
      status: '', // Status will be auto-inferred
    };

    this.showUpdateModal = true;
    this.cdr.markForCheck();
  }

  closeUpdateModal(): void {
    this.showUpdateModal = false;
    this.selectedVve = null; // Now it's safe to null it
    this.updateForm = {
      actualArrivalTime: '',
      actualDockId: '',
      actualDepartureTime: '',
      status: '',
    };
    this.cdr.markForCheck();
  }

  getAvailableStatuses(currentStatus: string): string[] {
    const transitions: { [key: string]: string[] } = {
      NotStarted: ['InProgress', 'Delayed'],
      InProgress: ['Delayed', 'Completed'],
      Delayed: ['InProgress', 'Completed'],
      Completed: [],
    };
    return transitions[currentStatus] || [];
  }

  updateVVE(): void {
    if (!this.selectedVve) return;

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    // Validate times are in the past
    const now = new Date();

    if (this.updateForm.actualArrivalTime) {
      const actualArrival = new Date(this.updateForm.actualArrivalTime);
      if (actualArrival > now) {
        this.errorMessage =
          'O horário introduzido em Arrival é inválido porque ainda não aconteceu';
        this.loading = false;
        this.cdr.markForCheck();
        return;
      }
    }

    if (this.updateForm.actualDepartureTime) {
      const actualDeparture = new Date(this.updateForm.actualDepartureTime);
      if (actualDeparture > now) {
        this.errorMessage =
          'O horário introduzido em Departure é inválido porque ainda não aconteceu';
        this.loading = false;
        this.cdr.markForCheck();
        return;
      }
    }

    // Validate arrival is before departure
    if (this.updateForm.actualArrivalTime && this.updateForm.actualDepartureTime) {
      const actualArrival = new Date(this.updateForm.actualArrivalTime);
      const actualDeparture = new Date(this.updateForm.actualDepartureTime);
      if (actualArrival >= actualDeparture) {
        this.errorMessage = 'O horário de Arrival deve ser antes do horário de Departure';
        this.loading = false;
        this.cdr.markForCheck();
        return;
      }
    }

    // Update berth info if changed
    const berthData: any = {};
    let needsExplicitStatusUpdate = false;
    let explicitStatus = '';

    if (this.updateForm.actualArrivalTime) {
      const actualArrival = new Date(this.updateForm.actualArrivalTime);
      const plannedArrival = new Date(this.selectedVve.plannedArrivalTime);

      berthData.actualArrivalTime = actualArrival.toISOString();

      // Check if we need explicit Delayed status (backend auto-handles NotStarted->InProgress)
      if (actualArrival > plannedArrival && this.selectedVve.status !== 'Delayed') {
        needsExplicitStatusUpdate = true;
        explicitStatus = 'Delayed';
      }
    }

    if (this.updateForm.actualDockId) {
      berthData.actualDockId = this.updateForm.actualDockId;
    }

    if (this.updateForm.actualDepartureTime) {
      berthData.actualDepartureTime = new Date(this.updateForm.actualDepartureTime).toISOString();
      // Departure time requires Completed status
      if (this.selectedVve.status !== 'Completed') {
        needsExplicitStatusUpdate = true;
        explicitStatus = 'Completed';
      }
    }

    const updates: any[] = [];

    // Update berth if data provided
    if (Object.keys(berthData).length > 0) {
      updates.push(this.oemService.updateVVEBerth(this.selectedVve.id, berthData));
    }

    // Only update status explicitly when needed (Delayed or Completed)
    // Backend auto-handles NotStarted->InProgress transition
    if (needsExplicitStatusUpdate) {
      updates.push(this.oemService.updateVVEStatus(this.selectedVve.id, explicitStatus));
    }

    if (updates.length === 0) {
      this.errorMessage = 'No changes to update';
      this.loading = false;
      this.cdr.markForCheck();
      return;
    }

    // Execute updates sequentially
    this.executeUpdatesSequentially(updates, 0);
  }

  executeUpdatesSequentially(updates: any[], index: number): void {
    if (index >= updates.length) {
      // All updates done - close modals first, then show success
      this.loading = false;
      this.closeUpdateModal();
      this.closeDetailModal(); // Also close detail modal to return to cards
      this.successMessage = 'VVE updated successfully';
      this.loadAllData();
      this.cdr.markForCheck();
      return;
    }

    updates[index].subscribe({
      next: (response: any) => {
        if (response.warnings && response.warnings.length > 0) {
          console.warn('Warnings:', response.warnings);
        }
        // Continue with next update
        this.executeUpdatesSequentially(updates, index + 1);
      },
      error: (err: any) => {
        console.error('Error updating VVE:', err);
        this.errorMessage = err.error?.error || 'Error updating VVE';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  getStatusBadgeClass(status: string): string {
    const classes: { [key: string]: string } = {
      NotStarted: 'badge-secondary',
      InProgress: 'badge-primary',
      Delayed: 'badge-warning',
      Completed: 'badge-success',
    };
    return classes[status] || 'badge-secondary';
  }

  formatDateTime(dateString: string | null): string {
    if (!dateString) return 'Not set';
    const date = new Date(dateString);
    return date.toLocaleString('en-GB', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  isDockChanged(vve: VesselVisitExecution): boolean {
    return !!(vve.actualDockId && vve.actualDockId !== vve.plannedDockId);
  }

  isDelayed(vve: VesselVisitExecution): boolean {
    if (!vve.actualArrivalTime) return false;
    return new Date(vve.actualArrivalTime) > new Date(vve.plannedArrivalTime);
  }

  isDepartureChanged(vve: VesselVisitExecution): boolean {
    if (!vve.actualDepartureTime) return false;
    const planned = new Date(vve.plannedDepartureTime).getTime();
    const actual = new Date(vve.actualDepartureTime).getTime();
    return actual > planned;
  }

  // Incident Recording Methods
  startIncidentSelection(): void {
    this.incidentSelectionMode = true;
    this.selectedVveIds = [];
    this.cdr.markForCheck();
  }

  cancelIncidentSelection(): void {
    this.incidentSelectionMode = false;
    this.selectedVveIds = [];
    this.cdr.markForCheck();
  }

  toggleVveSelection(vveId: string): void {
    // Find the VVE to check its status
    const vve = this.vves.find((v) => v.id === vveId);
    if (!vve || vve.status === 'Completed') {
      // Don't allow selection of completed VVEs
      return;
    }

    const index = this.selectedVveIds.indexOf(vveId);
    if (index > -1) {
      this.selectedVveIds.splice(index, 1);
    } else {
      this.selectedVveIds.push(vveId);
    }
    this.cdr.markForCheck();
  }

  isVveSelected(vveId: string): boolean {
    return this.selectedVveIds.includes(vveId);
  }

  canSelectVve(vve: VesselVisitExecution): boolean {
    return vve.status !== 'Completed';
  }

  selectAllVVEs(): void {
    // Only select VVEs that are not completed
    this.selectedVveIds = this.vves
      .filter((vve) => vve.status !== 'Completed')
      .map((vve) => vve.id);
    this.cdr.markForCheck();
  }

  deselectAllVVEs(): void {
    this.selectedVveIds = [];
    this.cdr.markForCheck();
  }

  getSelectedVves(): VesselVisitExecution[] {
    return this.vves.filter((vve) => this.selectedVveIds.includes(vve.id));
  }

  proceedToIncidentForm(): void {
    if (this.selectedVveIds.length === 0) return;

    // Exit selection mode
    this.incidentSelectionMode = false;
    this.loading = true;

    // Load incident types first, then show modal
    this.incidentTypesService.getAllIncidentTypes(false).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.incidentTypes = response.data;

          // Initialize form with current time and selected VVEs
          const now = new Date();
          const timeStr = now.toTimeString().slice(0, 5); // HH:MM format

          this.incidentForm = {
            incidentTypeId: '',
            startTime: timeStr,
            endTime: null,
            description: '',
            affectsAllVVEs: false,
            affectedVVEIds: [...this.selectedVveIds],
          };

          // Show modal only after types are loaded
          this.showRecordIncidentModal = true;
          this.incidentErrorMessage = '';
          this.loading = false;
          this.cdr.markForCheck();
        }
      },
      error: (err) => {
        console.error('Failed to load incident types:', err);
        this.errorMessage = 'Failed to load incident types';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  closeRecordIncidentModal(): void {
    this.showRecordIncidentModal = false;
    this.selectedVveIds = [];
    this.incidentErrorMessage = '';
    this.incidentForm = {
      incidentTypeId: '',
      startTime: '',
      endTime: null,
      description: '',
      affectsAllVVEs: false,
      affectedVVEIds: [],
    };
    this.cdr.markForCheck();
  }

  submitIncident(): void {
    if (this.selectedVveIds.length === 0) return;

    this.loading = true;
    this.incidentErrorMessage = '';

    // Prepare the incident data - convert times to datetime
    const today = new Date().toISOString().split('T')[0];
    const incidentData: CreateIncidentDto = {
      ...this.incidentForm,
      startTime: `${today}T${this.incidentForm.startTime}:00`,
      endTime: this.incidentForm.endTime ? `${today}T${this.incidentForm.endTime}:00` : null,
      affectedVVEIds: this.incidentForm.affectedVVEIds,
    };

    this.incidentsService.createIncident(incidentData).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = 'Incident recorded successfully!';
          this.closeRecordIncidentModal();
          // Reload incidents to update the counts
          this.loadTodaysIncidents();
          setTimeout(() => {
            this.successMessage = '';
            this.cdr.markForCheck();
          }, 3000);
        } else {
          this.incidentErrorMessage = response.error || 'Failed to record incident';
        }
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.incidentErrorMessage = err.error?.error || 'Failed to record incident';
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  getSelectedIncidentType(): IncidentType | undefined {
    return this.incidentTypes.find((type) => type.id === this.incidentForm.incidentTypeId);
  }

  getSeverityBadgeClass(severity: string): string {
    const classes: { [key: string]: string } = {
      Minor: 'badge-success',
      Major: 'badge-warning',
      Critical: 'badge-danger',
    };
    return classes[severity] || 'badge-secondary';
  }

  // Load today's incidents
  loadTodaysIncidents(): void {
    const filters = {
      date: this.today,
    };

    this.incidentsService.getAllIncidents(filters).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.todaysIncidents = response.data;
          this.cdr.markForCheck();
        }
      },
      error: (err) => {
        console.error('Failed to load incidents:', err);
      },
    });
  }

  // Get incident count for a specific VVE
  getVveIncidentCount(vveId: string): number {
    return this.todaysIncidents.filter(
      (incident) => incident.affectsAllVVEs || incident.affectedVVEIds.includes(vveId)
    ).length;
  }

  // Get incidents affecting a specific VVE (sorted by start time, earliest first)
  getVveIncidents(vveId: string): any[] {
    return this.todaysIncidents
      .filter((incident) => incident.affectsAllVVEs || incident.affectedVVEIds.includes(vveId))
      .sort((a, b) => {
        const timeA = new Date(a.startTime).getTime();
        const timeB = new Date(b.startTime).getTime();
        return timeA - timeB;
      });
  }

  // Navigate to incidents management page
  navigateToIncidents(): void {
    this.router.navigate(['/logistic-operator/incidents']);
  }
}
