import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css'],
})
export class LayoutComponent implements OnInit {
  userRole: string = '';
  userName: string = 'User';

  constructor(private router: Router, private authService: AuthService) {}

  ngOnInit(): void {
    // Get user from AuthService
    const user = this.authService.getUser();
    if (user) {
      this.userRole = user.role;
      this.userName = user.name;
    } else {
      // User is not authenticated, redirect will be handled by auth guard
      this.router.navigate(['/login']);
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  goToDashboard(): void {
    const dashboardRoutes: { [key: string]: string } = {
      Admin: '/admin',
      PortAuthorityOfficer: '/port-authority',
      LogisticOperator: '/logistic-operator',
      ShippingAgentRepresentative: '/shipping-agent',
    };

    const route = dashboardRoutes[this.userRole] || '/login';
    this.router.navigate([route]);
  }
}
