import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

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

  constructor(private router: Router) {}

  ngOnInit(): void {
    // Get role from localStorage
    this.userRole = localStorage.getItem('userRole') || '';
    if (!this.userRole) {
      this.router.navigate(['/login']);
    }
  }

  logout(): void {
    localStorage.removeItem('userRole');
    this.router.navigate(['/login']);
  }
}
