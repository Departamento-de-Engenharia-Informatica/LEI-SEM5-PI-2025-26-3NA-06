import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent implements OnInit {
  userRole: string = '';
  userName: string = '';

  constructor(private router: Router) {}

  ngOnInit(): void {
    // TODO: Get user role from authentication service
    // For now, we'll use mock data
    this.userRole = 'Admin'; // This should come from auth service
    this.userName = 'User'; // This should come from auth service
  }

  logout(): void {
    // TODO: Implement logout logic
    this.router.navigate(['/login']);
  }
}
