import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';

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
  registrationData: any = {};

  constructor(private http: HttpClient, private router: Router, private route: ActivatedRoute) {}

  ngOnInit() {
    // Get email and name from query parameters (passed from login)
    this.route.queryParams.subscribe((params) => {
      if (params['email']) {
        this.email = params['email'];
      }
      if (params['name']) {
        // Pre-fill username with name from Google (user can change it)
        this.username = params['name'].replace(/\s+/g, '');
      }
    });
  }

  onSubmit() {
    this.isLoading = true;
    this.message = '';

    this.registrationData = {
      email: this.email,
      username: this.username,
      role: this.role,
    };

    this.http
      .post('http://localhost:5218/api/User/register', this.registrationData)
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
