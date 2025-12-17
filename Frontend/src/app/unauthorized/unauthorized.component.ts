import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './unauthorized.component.html',
  styleUrls: ['./unauthorized.component.css'],
})
export class UnauthorizedComponent implements OnInit {
  userRole: string = '';
  attemptedRoute: string = '';

  constructor(private router: Router, private authService: AuthService) {}

  ngOnInit(): void {
    const user = this.authService.getUser();
    this.userRole = user?.role || 'Unknown';

    // Get the attempted route from navigation state
    const navigation = this.router.getCurrentNavigation();
    this.attemptedRoute = navigation?.extras?.state?.['attemptedRoute'] || 'Unknown';
  }

  goToDashboard(): void {
    const user = this.authService.getUser();
    const userRole = user?.role;
    switch (userRole) {
      case 'Admin':
        this.router.navigate(['/admin']);
        break;
      case 'PortAuthorityOfficer':
        this.router.navigate(['/port-authority']);
        break;
      case 'LogisticOperator':
        this.router.navigate(['/logistic-operator']);
        break;
      case 'ShippingAgentRepresentative':
        this.router.navigate(['/shipping-agent']);
        break;
      default:
        this.router.navigate(['/login']);
    }
  }
}
