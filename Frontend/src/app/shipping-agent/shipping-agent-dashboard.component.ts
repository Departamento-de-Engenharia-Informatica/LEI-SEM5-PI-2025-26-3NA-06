import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-shipping-agent-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './shipping-agent-dashboard.component.html',
  styleUrls: ['./shipping-agent-dashboard.component.css'],
})
export class ShippingAgentDashboardComponent {
  constructor(private router: Router) {}

  navigateTo(route: string) {
    this.router.navigate([`/shipping-agent/${route}`]);
  }
}
