import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { ConfirmEmailComponent } from './confirm-email/confirm-email.component';
import { AdminDashboardComponent } from './admin/admin-dashboard.component';
import { UserManagementComponent } from './admin/user-management/user-management.component';
import { PortAuthorityDashboardComponent } from './port-authority/port-authority-dashboard.component';
import { LayoutComponent } from './layout/layout.component';
import { AccessDeniedComponent } from './access-denied/access-denied.component';
import { UnauthorizedComponent } from './unauthorized/unauthorized.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'confirm-email', component: ConfirmEmailComponent },
  { path: 'access-denied', component: AccessDeniedComponent },
  { path: 'unauthorized', component: UnauthorizedComponent },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'admin',
        data: { role: 'Admin' },
        canActivate: [authGuard],
        children: [
          { path: '', component: AdminDashboardComponent },
          { path: 'user-management', component: UserManagementComponent },
          // Add more admin child routes here
        ],
      },
      {
        path: 'port-authority',
        data: { role: 'PortAuthorityOfficer' },
        canActivate: [authGuard],
        children: [
          { path: '', component: PortAuthorityDashboardComponent },
          // Add more port authority child routes here
        ],
      },
      {
        path: 'logistic-operator',
        data: { role: 'LogisticOperator' },
        canActivate: [authGuard],
        children: [
          { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
          // Add logistic operator routes here
        ],
      },
      {
        path: 'shipping-agent',
        data: { role: 'ShippingAgentRepresentative' },
        canActivate: [authGuard],
        children: [
          { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
          // Add shipping agent routes here
        ],
      },
    ],
  },
];
