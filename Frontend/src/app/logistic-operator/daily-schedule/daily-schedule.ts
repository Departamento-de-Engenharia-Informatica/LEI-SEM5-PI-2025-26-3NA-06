import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  SchedulingService,
  DailyScheduleResult,
  DockAssignment,
} from '../../services/scheduling.service';

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
  error: string | null = null;

  constructor(
    private schedulingService: SchedulingService,
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

  formatDateTime(dateStr: string): string {
    return new Date(dateStr).toLocaleString();
  }
}
