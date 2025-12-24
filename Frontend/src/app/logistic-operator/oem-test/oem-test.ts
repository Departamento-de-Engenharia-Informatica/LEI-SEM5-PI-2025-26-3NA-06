import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OemService, OemHealthResponse } from '../../services/oem.service';

@Component({
  selector: 'app-oem-test',
  imports: [CommonModule],
  templateUrl: './oem-test.html',
  styleUrl: './oem-test.css',
  standalone: true,
})
export class OemTest implements OnInit {
  healthStatus: OemHealthResponse | null = null;
  loading: boolean = false;
  error: string | null = null;

  constructor(
    private oemService: OemService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit() {
    this.checkHealth();
  }

  checkHealth() {
    this.loading = true;
    this.error = null;
    this.healthStatus = null;

    this.oemService.getHealth().subscribe({
      next: (response) => {
        this.ngZone.run(() => {
          this.healthStatus = response;
          this.loading = false;
          console.log('OEM Health Response:', response);
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.ngZone.run(() => {
          console.error('OEM Health Check Failed:', err);
          this.error =
            err.error?.message ||
            'Failed to connect to OEM API. Please ensure the service is running on port 5003.';
          this.loading = false;
          this.cdr.detectChanges();
        });
      },
    });
  }
}
