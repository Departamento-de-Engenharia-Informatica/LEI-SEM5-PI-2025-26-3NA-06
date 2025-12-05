import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-port-authority-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './port-authority-dashboard.component.html',
  styleUrls: ['./port-authority-dashboard.component.css'],
})
export class PortAuthorityDashboardComponent {}
