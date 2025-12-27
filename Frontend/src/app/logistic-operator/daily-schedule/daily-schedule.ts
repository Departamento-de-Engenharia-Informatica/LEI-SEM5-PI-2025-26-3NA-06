import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  SchedulingService,
  DailyScheduleResult,
  DailyDockSchedule,
  DockAssignment,
} from '../../services/scheduling.service';
import { OemService, SaveOperationPlanRequest } from '../../services/oem.service';

@Component({
  selector: 'app-daily-schedule',
  imports: [CommonModule, FormsModule],
  templateUrl: './daily-schedule.html',
  styleUrl: './daily-schedule.css',
  standalone: true,
})
export class DailySchedule implements OnInit {
  selectedDate: string = '';
  schedule: DailyScheduleResult | null = null;
  loading: boolean = false;
  saving: boolean = false;
  error: string | null = null;
  saveSuccess: string | null = null;

  constructor(
    private schedulingService: SchedulingService,
    private oemService: OemService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit() {
    // Set default date to today
    const today = new Date();
    this.selectedDate = today.toISOString().split('T')[0];
  }

  generateSchedule() {
    if (!this.selectedDate) {
      this.error = 'Please select a date';
      return;
    }

    this.loading = true;
    this.error = null;
    this.saveSuccess = null;
    this.schedule = null;

    this.schedulingService.generateDailySchedule(this.selectedDate).subscribe({
      next: (result) => {
        this.ngZone.run(() => {
          this.schedule = result;
          this.loading = false;
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.ngZone.run(() => {
          console.error('Error generating schedule:', err);
          this.error = err.error?.message || 'Failed to generate schedule. Please try again.';
          this.loading = false;
          this.cdr.detectChanges();
        });
      },
    });
  }

  saveOperationPlan() {
    if (!this.schedule || !this.selectedDate) {
      this.error = 'No schedule to save';
      return;
    }

    this.saving = true;
    this.error = null;
    this.saveSuccess = null;

    // Prepare VVN data from dockSchedules
    const vesselVisitNotifications = this.schedule.dockSchedules.flatMap((dock: any) =>
      dock.assignments.map((assignment: any) => ({
        id: assignment.vvnId,
        vesselName: assignment.vesselName,
        vesselImo: assignment.vesselImo,
        eta: assignment.eta,
        etd: assignment.etd,
        assignedDockId: dock.dockId,
        dockName: dock.dockName,
      }))
    );

    const request: SaveOperationPlanRequest = {
      planDate: this.selectedDate,
      isFeasible: this.schedule.isFeasible,
      warnings: this.schedule.warnings || [],
      vesselVisitNotifications: vesselVisitNotifications,
    };

    this.oemService.saveOperationPlan(request).subscribe({
      next: (response) => {
        this.ngZone.run(() => {
          if (response.success) {
            this.saveSuccess = `Operation Plan saved successfully!`;
            console.log('Operation Plan saved:', response.data);
          } else {
            this.error = response.error || 'Failed to save operation plan';
          }
          this.saving = false;
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.ngZone.run(() => {
          console.error('Error saving operation plan:', err);
          const errorMsg =
            err.error?.error || err.error?.message || 'Failed to save operation plan';

          // Check if plan already exists
          if (errorMsg.includes('already exists')) {
            // Show confirmation dialog
            const confirmReplace = confirm(
              `An operation plan for ${this.selectedDate} already exists. Are you sure you want to replace it with the current one?`
            );

            if (confirmReplace) {
              // User confirmed, replace the plan
              this.replaceOperationPlan(request);
              return;
            }
          }

          this.error = errorMsg;
          this.saving = false;
          this.cdr.detectChanges();
        });
      },
    });
  }

  replaceOperationPlan(request: SaveOperationPlanRequest) {
    this.oemService.replaceOperationPlan(request).subscribe({
      next: (response) => {
        this.ngZone.run(() => {
          if (response.success) {
            this.saveSuccess = `Operation Plan replaced successfully!`;
            console.log('Operation Plan replaced:', response.data);
          } else {
            this.error = response.error || 'Failed to replace operation plan';
          }
          this.saving = false;
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.ngZone.run(() => {
          console.error('Error replacing operation plan:', err);
          console.error('Full error object:', JSON.stringify(err.error, null, 2));
          this.error = err.error?.error || err.error?.message || 'Failed to replace operation plan';
          this.saving = false;
          this.cdr.detectChanges();
        });
      },
    });
  }

  canSaveOperationPlan(): boolean {
    return !!(this.schedule && this.schedule.dockSchedules.length > 0 && !this.saving);
  }

  formatDateTime(dateStr: string): string {
    return new Date(dateStr).toLocaleString();
  }

  formatWarning(warning: string): string {
    // Replace [Dock X] with styled badge
    return warning.replace(/\[Dock ([^\]]+)\]:/g, '<strong class="dock-badge">Dock $1</strong>:');
  }
}
