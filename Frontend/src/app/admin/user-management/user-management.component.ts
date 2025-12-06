import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

interface User {
  id: string;
  username: string;
  email: string;
  role: string;
  isActive: boolean;
  confirmationToken: string;
}

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css'],
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  filteredUsers: User[] = [];
  showInactiveOnly: boolean = true;
  selectedRole: { [userId: string]: string } = {};
  availableRoles = [
    'Admin',
    'PortAuthorityOfficer',
    'LogisticOperator',
    'ShippingAgentRepresentative',
  ];

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    const endpoint = this.showInactiveOnly
      ? 'http://localhost:5218/api/UserManagement/inactive-users'
      : 'http://localhost:5218/api/UserManagement/all-users';

    console.log('Loading users from:', endpoint);

    this.http.get<User[]>(endpoint, { withCredentials: true }).subscribe({
      next: (users) => {
        this.users = users;
        this.filteredUsers = [...users]; // Create new array reference

        // Initialize selected role for each user
        users.forEach((user) => {
          this.selectedRole[user.id] = user.role;
        });
      },
      error: (error) => {
        console.error('Error loading users:', error);
        alert('Failed to load users. Please ensure you are logged in as Admin.');
      },
    });
  }

  toggleFilter() {
    // Toggle the flag first to determine which endpoint to call
    const newFilterState = !this.showInactiveOnly;

    const endpoint = newFilterState
      ? 'http://localhost:5218/api/UserManagement/inactive-users'
      : 'http://localhost:5218/api/UserManagement/all-users';

    console.log('Toggling filter to:', newFilterState ? 'inactive only' : 'all users');

    this.http.get<User[]>(endpoint, { withCredentials: true }).subscribe({
      next: (users) => {
        // Only update the state after data is successfully loaded
        this.showInactiveOnly = newFilterState;
        this.users = users;
        this.filteredUsers = [...users];

        // Initialize selected role for each user
        users.forEach((user) => {
          this.selectedRole[user.id] = user.role;
        });
      },
      error: (error) => {
        console.error('Error loading users:', error);
        alert('Failed to load users. Please ensure you are logged in as Admin.');
      },
    });
  }

  assignRoleAndSendEmail(userId: string) {
    const role = this.selectedRole[userId];
    if (!role) {
      alert('Please select a role first');
      return;
    }

    if (!confirm(`Assign role "${role}" and send activation email to this user?`)) {
      return;
    }

    this.http
      .put(
        `http://localhost:5218/api/UserManagement/${userId}/assign-role`,
        { role },
        { withCredentials: true }
      )
      .subscribe({
        next: (response: any) => {
          alert(response.message || 'Role assigned and activation email sent successfully');
          this.loadUsers();
        },
        error: (error) => {
          console.error('Error assigning role:', error);
          alert('Failed to assign role: ' + (error.error?.message || error.message));
        },
      });
  }

  toggleUserActive(userId: string, currentStatus: boolean) {
    const action = currentStatus ? 'deactivate' : 'activate';
    if (!confirm(`Are you sure you want to ${action} this user?`)) {
      return;
    }

    this.http
      .put(
        `http://localhost:5218/api/UserManagement/${userId}/toggle-active`,
        {},
        { withCredentials: true }
      )
      .subscribe({
        next: (response: any) => {
          alert(response.message || `User ${action}d successfully`);
          this.loadUsers();
        },
        error: (error) => {
          console.error('Error toggling user status:', error);
          alert('Failed to toggle user status: ' + (error.error?.message || error.message));
        },
      });
  }
}
