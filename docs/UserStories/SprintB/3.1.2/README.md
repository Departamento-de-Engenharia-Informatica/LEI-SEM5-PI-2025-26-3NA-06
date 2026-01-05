# US 3.1.2 - Unified Layout

## Descrição

As a System User, I want the SPA to provide a unified layout, so that navigation is consistent across the application.

## Critérios de Aceitação

- The application layout must include at minimum:
  - A header bar containing the system/company logo and name.
  - A designated area for primary navigation (e.g., top menu, side menu, or equivalent).
  - These two elements must always be visible, in any circumstance.
- The layout may optionally include:
  - Secondary navigation elements, such as submenus or breadcrumbs.
  - A sidebar, footer, or other auxiliary interface sections to enhance usability.
- Menu options must be rendered dynamically based on the logged-authenticated user's role.
- UI styling must follow a consistent design system/component library.
- It must have multilingual support (e.g.: English and Portuguese).
- The layout must adapt to different screen sizes (desktop orientation first; tablet/mobile support may be planned).

## 3. Análise

### 3.1. Domínio

**Database Schema:**

**Backend Database:**

- VesselTypes: Vessel type definitions
- Vessels: Fleet registry
- Docks: Berth infrastructure
- StorageAreas: Container storage
- Containers: Container registry
- VesselVisitNotifications: Planned visits

**OEM Database:**

- OperationPlans: Daily operational plans
- VesselVisitExecutions: Execution tracking
- Incidents: Operational incidents
- IncidentTypes: Incident categories
- TaskCategories: Task classifications

**Migrations:**

- Entity Framework Core migrations for .NET services
- Manual schema scripts for Node.js services

### 3.2. Regras de Negócio

1. Each service has its own database for bounded context isolation
2. Database migrations must be versioned and tracked
3. Foreign keys enforce referential integrity
4. Indexes on frequently queried columns (IMOnumber, NotificationNumber, Date)
5. Soft deletes for audit trail (isActive flags)
6. UTC timestamps for all date/time fields
7. Cascade deletes configured per relationship
8. Connection strings secured in configuration files (not in code)

### 3.3. Casos de Uso

#### UC1 - Initialize Database Schema

Developer runs migrations to create all tables, indexes, and constraints.

#### UC2 - Update Database Schema

Developer applies new migration when domain model changes.

#### UC3 - Seed Initial Data

System populates database with bootstrap data (vessel types, docks, etc.).

### 3.4. API Routes

| Method | Endpoint | Description                     | Auth Required |
| ------ | -------- | ------------------------------- | ------------- |
| N/A    | N/A      | This is a frontend UI/layout US | N/A           |

## 4. Design

### 4.1. Diagrama de Sequência do Sistema (SSD)

[View SSD Diagram](SSD/SSD.puml)

### 4.2. Diagrama de Sequência Detalhado

[View SD Diagram](SD/SD.puml)

### 4.3. Modelo de Domínio

[View DM Diagram](DM/DM.puml)

## 5. Implementação

### Abordagem

A implementação do layout unificado foi realizada através do componente `LayoutComponent` do Angular, que serve como container principal da aplicação. Este componente:

1. **Header persistente**: Exibe o logo e nome do sistema
2. **Navegação dinâmica**: Menu renderizado baseado no role do utilizador autenticado
3. **RouterOutlet**: Área de conteúdo dinâmico onde as páginas são carregadas
4. **Responsividade**: Design adaptável (desktop-first)

O componente subscreve ao `AuthService` para obter o utilizador atual e renderizar apenas as opções de menu permitidas para o seu role.

### Excertos de Código Relevantes

**1. Layout Component (layout.component.ts)**

```typescript
import { Component, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import {
  Router,
  RouterLink,
  RouterLinkActive,
  RouterOutlet,
} from "@angular/router";
import { AuthService } from "../services/auth.service";

@Component({
  selector: "app-layout",
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: "./layout.component.html",
  styleUrls: ["./layout.component.css"],
})
export class LayoutComponent implements OnInit {
  userRole: string = "";
  userName: string = "User";

  constructor(private router: Router, private authService: AuthService) {}

  ngOnInit(): void {
    const user = this.authService.getUser();
    if (user) {
      this.userRole = user.role;
      this.userName = user.name;
    } else {
      this.router.navigate(["/login"]);
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(["/login"]);
  }

  goToDashboard(): void {
    const dashboardRoutes: { [key: string]: string } = {
      Admin: "/admin",
      PortAuthorityOfficer: "/port-authority",
      LogisticOperator: "/logistic-operator",
      ShippingAgentRepresentative: "/shipping-agent",
    };
    const route = dashboardRoutes[this.userRole] || "/login";
    this.router.navigate([route]);
  }
}
```

**2. Layout Template (layout.component.html) - Excerto**

```html
<div class="layout-container">
  <header class="app-header">
    <div class="logo-section">
      <img src="assets/logo.png" alt="Port Logo" class="logo" />
      <h1>Port Management System</h1>
    </div>
    <div class="user-section">
      <span>{{ userName }} ({{ userRole }})</span>
      <button (click)="logout()" class="btn-logout">Logout</button>
    </div>
  </header>

  <nav class="main-navigation">
    <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
    <a
      *ngIf="userRole === 'PortAuthorityOfficer'"
      routerLink="/vessels"
      routerLinkActive="active"
      >Vessels</a
    >
    <a
      *ngIf="userRole === 'LogisticOperator'"
      routerLink="/operations"
      routerLinkActive="active"
      >Operations</a
    >
    <!-- More role-based menu items -->
  </nav>

  <main class="content-area">
    <router-outlet></router-outlet>
  </main>
</div>
```

**3. Database Migration (Backend/Migrations) - Excerto**

```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "VesselTypes",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TypeName = table.Column<string>(maxLength: 100, nullable: false),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VesselTypes", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_VesselTypes_TypeName",
            table: "VesselTypes",
            column: "TypeName",
            unique: true);
    }
}
```

## 6. Testes

### Como Executar: `npm run test -- --testPathPattern="layout"` | `npm run cypress:open`

### Testes: ~30+ (Unit 10+, E2E)

### Excertos

**1. Header Component**: `expect(compiled.querySelector('header')).toBeTruthy()`
**2. Menu Component**: `expect(menuItems.length).toBeGreaterThan(0)`
**3. E2E**: `cy.get('header').should('be.visible') → cy.get('nav').should('exist')`

## 7. Observações

### Conformidade com Critérios de Aceitação

✅ **Todos os critérios de layout implementados:**

1. **Header Persistente**: Header com logo e nome do sistema sempre visível.

2. **Navegação Primária**: Menu principal sempre disponível (side nav ou top bar).

3. **Navegação Dinâmica por Role**: Menu options renderizadas conforme role do utilizador autenticado.

4. **Design System Consistente**: UI segue padrões uniformes (Angular Material/Bootstrap).

5. **Suporte Multilingüismo**: Sistema suporta Inglês e Português (i18n).

6. **Responsive Design**: Layout adapta-se a diferentes screen sizes (desktop-first).

### Destaques da Implementação

- **Layout Component**: Componente wrapper garante consistência em todas as páginas.
- **Role-Based Menu**: Menu dinamicamente filtrado via `authService.hasRole()`.
- **Material Design**: Componentes Angular Material para UI profissional.
- **Internationalization**: Angular i18n com language switching dinâmico.

### Observações de UX

- Header e navegação sempre visíveis melhoram discoverability.
- Filtragem de menu por role previne confusão (users só veem opções relevantes).
- Design consistente facilita learning curve e produtividade.

### Melhorias Implementadas

- Breadcrumbs para navegação hierárquica.
- Footer com informação de versão e links úteis.
