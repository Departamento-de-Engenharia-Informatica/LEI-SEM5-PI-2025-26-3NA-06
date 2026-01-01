import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { ConfirmEmailComponent } from './confirm-email/confirm-email.component';
import { AdminDashboardComponent } from './admin/admin-dashboard.component';
import { UserManagementComponent } from './admin/user-management/user-management.component';
import { PortAuthorityDashboardComponent } from './port-authority/port-authority-dashboard.component';
import { CreateVesselComponent } from './port-authority/create-vessel/create-vessel.component';
import { EditVesselComponent } from './port-authority/edit-vessel/edit-vessel.component';
import { VesselsComponent } from './port-authority/vessels/vessels.component';
import { CreateVesselTypeComponent } from './port-authority/create-vessel-type/create-vessel-type.component';
import { EditVesselTypeComponent } from './port-authority/edit-vessel-type/edit-vessel-type.component';
import { VesselTypesComponent } from './port-authority/vessel-types/vessel-types.component';
import { DocksComponent } from './port-authority/docks/docks.component';
import { CreateDockComponent } from './port-authority/create-dock/create-dock.component';
import { EditDockComponent } from './port-authority/edit-dock/edit-dock.component';
import { LayoutComponent } from './layout/layout.component';
import { AccessDeniedComponent } from './access-denied/access-denied.component';
import { UnauthorizedComponent } from './unauthorized/unauthorized.component';
import { authGuard } from './guards/auth.guard';
import { StorageAreasComponent } from './port-authority/storage-areas/storage-areas.component';
import { CreateStorageAreaComponent } from './port-authority/create-storage-area/create-storage-area.component';
import { EditStorageAreaComponent } from './port-authority/edit-storage-area/edit-storage-area.component';
import { VvnPendingComponent } from './port-authority/vvn-pending/vvn-pending.component';
import { ShippingAgentDashboardComponent } from './shipping-agent/shipping-agent-dashboard.component';
import { CreateVvnComponent } from './shipping-agent/create-vvn/create-vvn.component';
import { VvnDraftsComponent } from './shipping-agent/vvn-drafts/vvn-drafts.component';
import { VvnSubmittedComponent } from './shipping-agent/vvn-submitted/vvn-submitted.component';
import { VvnReviewedComponent } from './shipping-agent/vvn-reviewed/vvn-reviewed.component';
import { ContainerManagementComponent } from './shipping-agent/container-management/container-management.component';
import { LogisticOperatorDashboard } from './logistic-operator/logistic-operator-dashboard/logistic-operator-dashboard';
import { DailySchedule } from './logistic-operator/daily-schedule/daily-schedule';
import { OperationPlansListComponent } from './logistic-operator/operation-plans-list/operation-plans-list';
import { VveManagementComponent } from './logistic-operator/vve-management/vve-management';

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
          { path: 'create-vessel', component: CreateVesselComponent },
          { path: 'vessels', component: VesselsComponent },
          { path: 'edit-vessel/:imo', component: EditVesselComponent },
          { path: 'create-vessel-type', component: CreateVesselTypeComponent },
          { path: 'vessel-types', component: VesselTypesComponent },
          { path: 'edit-vessel-type/:id', component: EditVesselTypeComponent },
          { path: 'docks', component: DocksComponent },
          { path: 'create-dock', component: CreateDockComponent },
          { path: 'edit-dock/:id', component: EditDockComponent },
          { path: 'storage-areas', component: StorageAreasComponent },
          { path: 'create-storage-area', component: CreateStorageAreaComponent },
          { path: 'edit-storage-area/:id', component: EditStorageAreaComponent },
          { path: 'vvn-pending', component: VvnPendingComponent },
          // Add more port authority child routes here
        ],
      },
      {
        path: 'logistic-operator',
        data: { role: 'LogisticOperator' },
        canActivate: [authGuard],
        children: [
          { path: '', component: LogisticOperatorDashboard },
          { path: 'dashboard', component: LogisticOperatorDashboard },
          { path: 'daily-schedule', component: DailySchedule },
          { path: 'operation-plans', component: OperationPlansListComponent },
          { path: 'vve-management', component: VveManagementComponent },
        ],
      },
      {
        path: 'shipping-agent',
        data: { role: 'ShippingAgentRepresentative' },
        canActivate: [authGuard],
        children: [
          { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
          { path: 'dashboard', component: ShippingAgentDashboardComponent },
          { path: 'create-vvn', component: CreateVvnComponent },
          { path: 'edit-vvn/:id', component: CreateVvnComponent },
          { path: 'vvn-drafts', component: VvnDraftsComponent },
          { path: 'vvn-submitted', component: VvnSubmittedComponent },
          { path: 'vvn-reviewed', component: VvnReviewedComponent },
          { path: 'containers', component: ContainerManagementComponent },
        ],
      },
    ],
  },
];
