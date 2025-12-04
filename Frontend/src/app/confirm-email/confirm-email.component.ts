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
  message = 'Confirming your email...';
  success = false;

  constructor(private route: ActivatedRoute, private http: HttpClient, private router: Router) {}

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (token) {
      this.http
        .get(`http://localhost:5218/api/registration/confirm-email?token=${token}`, {
          responseType: 'text',
        })
        .subscribe({
          next: (response) => {
            this.message = 'Email confirmed successfully! Redirecting to dashboard...';
            this.success = true;

            // Redirect to dashboard after 2 seconds
            setTimeout(() => {
              this.router.navigate(['/dashboard']);
            }, 2000);
          },
          error: (err) => {
            console.error('Confirmation error:', err);
            this.message =
              'Email confirmation failed: ' + (err.error || err.message || 'Unknown error.');
            this.success = false;
          },
        });
    } else {
      this.message = 'Invalid confirmation link.';
      this.success = false;
    }
  }
}
