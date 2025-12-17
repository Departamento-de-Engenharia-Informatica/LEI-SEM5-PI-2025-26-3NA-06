import { Component, OnInit, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

declare const google: any;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  errorMessage = '';
  isLoading = false;

  constructor(private authService: AuthService, private router: Router, private ngZone: NgZone) {}

  ngOnInit(): void {
    // Check if already authenticated
    if (this.authService.isAuthenticated()) {
      const user = this.authService.getUser();
      if (user) {
        // Only redirect if we have a valid user
        this.redirectToDashboard();
        return;
      }
    }

    // Initialize Google Sign-In
    this.initializeGoogleSignIn();
  }

  private initializeGoogleSignIn(): void {
    // Load the Google Sign-In script
    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    document.body.appendChild(script);

    script.onload = () => {
      google.accounts.id.initialize({
        client_id: '326129425022-nikum4l0daqmlmr4oe4ksu99q395viig.apps.googleusercontent.com',
        callback: this.handleCredentialResponse.bind(this),
      });

      google.accounts.id.renderButton(document.getElementById('google-signin-button'), {
        theme: 'outline',
        size: 'large',
        text: 'signin_with',
        shape: 'rectangular',
      });
    };
  }

  private handleCredentialResponse(response: any): void {
    console.log('Google credential response received:', response);
    this.ngZone.run(() => {
      this.isLoading = true;
      this.errorMessage = '';

      console.log('Calling AuthAPI at http://localhost:5001/auth/google');
      this.authService.authenticateWithGoogle(response.credential).subscribe({
        next: (authResponse) => {
          console.log('Authentication successful:', authResponse);
          console.log('User role:', authResponse.user.role);
          console.log('Redirecting to dashboard...');
          this.redirectToDashboard();
        },
        error: (error) => {
          console.error('Authentication error:', error);
          console.error('Error status:', error.status);
          console.error('Error details:', error.error);
          this.isLoading = false;

          if (error.error?.requiresRegistration) {
            // New user registration - redirect to register page with email and name
            this.errorMessage = error.error.message || 'Please complete your registration.';

            // Show brief alert then redirect
            alert('Welcome! Please complete your registration to create your account.');

            // Redirect to register page with email and name as query params
            setTimeout(() => {
              this.router.navigate(['/register'], {
                queryParams: {
                  email: error.error.email,
                  name: error.error.name,
                },
              });
            }, 500);
          } else if (error.error?.message?.includes('inactive')) {
            // Existing user with inactive account
            this.errorMessage =
              'Your account is pending approval. Please wait for an administrator to activate your account.';
          } else if (error.error?.message) {
            this.errorMessage = error.error.message;
          } else {
            this.errorMessage = 'Authentication failed. Please try again.';
          }
        },
      });
    });
  }

  private redirectToDashboard(): void {
    const user = this.authService.getUser();
    if (!user) {
      // User data is missing, clear auth and stay on login page
      this.authService.logout();
      this.errorMessage = 'Authentication data is invalid. Please login again.';
      return;
    }

    switch (user.role) {
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

  loginWithGoogle() {
    // This method is no longer used, but kept for backward compatibility
    // The actual login is handled by the Google Sign-In button
  }
}
