import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './confirm-email.component.html',
  styleUrls: ['./confirm-email.component.css'],
})
export class ConfirmEmailComponent implements OnInit {
  message = 'Processing your activation...';
  success = false;
  showLoginLink = false;

  constructor(private route: ActivatedRoute, private http: HttpClient, private router: Router) {}

  ngOnInit(): void {
    // Check if we're returning from backend with status
    const status = this.route.snapshot.queryParamMap.get('status');
    const errorMessage = this.route.snapshot.queryParamMap.get('message');

    if (status === 'success') {
      this.success = true;
      this.showLoginLink = true;
      this.message =
        'Email confirmed successfully! Your account is now active. You can now log in.';
    } else if (status === 'error') {
      this.success = false;
      this.showLoginLink = true;
      this.message = errorMessage
        ? decodeURIComponent(errorMessage)
        : 'Failed to confirm email. Please try again or contact support.';
    } else {
      // If no status, we have a token and need to redirect to backend
      const token = this.route.snapshot.queryParamMap.get('token');

      if (token) {
        this.message = 'Activating your account...';
        // Redirect to backend which will process and redirect back
        window.location.href = `http://localhost:5218/api/User/confirm-email?token=${token}`;
      } else {
        this.message = 'Invalid activation link. No token provided.';
        this.success = false;
        this.showLoginLink = true;
      }
    }
  }
}
