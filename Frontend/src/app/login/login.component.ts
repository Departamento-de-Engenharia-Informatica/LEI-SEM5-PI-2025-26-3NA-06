import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  apiUrl = 'http://localhost:5218';

  ngOnInit(): void {
    // Clear any existing role from localStorage when visiting login page
    localStorage.removeItem('userRole');
  }

  loginWithGoogle() {
    // Redirect to backend Google OAuth endpoint
    window.location.href = `${this.apiUrl}/api/login`;
  }
}
