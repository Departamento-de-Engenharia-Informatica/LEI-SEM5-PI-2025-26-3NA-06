import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  email: string = '';
  username: string = '';
  role: string = '';
  message: string = '';
  isLoading: boolean = false;
  isSuccess: boolean = false;

  constructor(private http: HttpClient, private router: Router) {}

  ngOnInit() {
    // Get email from cookie if available
    this.getEmailFromBackend();
  }

  getEmailFromBackend() {
    this.http
      .get('http://localhost:5218/api/Registration/get-pending-email', { withCredentials: true })
      .subscribe({
        next: (response: any) => {
          if (response && response.email) {
            this.email = response.email;
          }
        },
        error: (error) => {
          console.error('Error getting email:', error);
        },
      });
  }

  onSubmit() {
    this.isLoading = true;
    this.message = '';

    const registrationData = {
      email: this.email,
      username: this.username,
      role: this.role,
    };

    this.http
      .post('http://localhost:5218/api/Registration/self-register', registrationData, {
        withCredentials: true,
      })
      .subscribe({
        next: (response: any) => {
          this.isSuccess = true;
          this.message =
            response.message ||
            'Account has been registered. Please wait for an administrator to activate your account.';
          this.isLoading = false;

          // Show alert to ensure user sees the message
          alert(this.message);

          // Redirect to login after alert is dismissed
          this.router.navigate(['/login']);
        },
        error: (error) => {
          this.isSuccess = false;
          this.message = error.error?.message || 'Registration failed. Please try again.';
          this.isLoading = false;
        },
      });
  }
}
