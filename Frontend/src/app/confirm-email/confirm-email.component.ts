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
  message = 'Redirecting to authentication...';
  success = false;

  constructor(private route: ActivatedRoute, private http: HttpClient, private router: Router) {}

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    console.log('Activation token:', token);

    if (token) {
      // Redirect to backend which will initiate Google OAuth
      // After OAuth, backend will verify the user and activate the account
      this.message = 'Redirecting to Google for authentication...';
      window.location.href = `http://localhost:5218/api/User/confirm-email?token=${token}`;
    } else {
      this.message = 'Invalid activation link. No token provided.';
      this.success = false;
    }
  }
}
