# Frontend Unit Tests

This directory contains unit tests for all Frontend components, services, guards, and interceptors, organized by functional area.

## 📁 Test Structure

```
src/tests/unit/
├── authentication/          # Authentication & Authorization tests
│   ├── login.component.spec.ts
│   ├── register.component.spec.ts
│   ├── confirm-email.component.spec.ts
│   ├── access-denied.component.spec.ts
│   └── unauthorized.component.spec.ts
│
├── admin/                   # Admin area tests
│   ├── admin-dashboard.component.spec.ts
│   └── user-management.component.spec.ts
│
├── port-authority/          # Port Authority tests
│   ├── port-authority-dashboard.component.spec.ts
│   ├── vessels.component.spec.ts
│   ├── docks.component.spec.ts
│   ├── storage-areas.component.spec.ts
│   ├── vessel-types.component.spec.ts
│   ├── vvn-pending.component.spec.ts
│   └── incident-types.component.spec.ts
│
├── shipping-agent/          # Shipping Agent tests
│   ├── shipping-agent-dashboard.component.spec.ts
│   ├── create-vvn.component.spec.ts
│   ├── vvn-drafts.component.spec.ts
│   ├── vvn-submitted.component.spec.ts
│   ├── vvn-reviewed.component.spec.ts
│   └── container-management.component.spec.ts
│
├── logistic-operator/       # Logistic Operator tests
│   ├── logistic-operator-dashboard.spec.ts
│   ├── daily-schedule.spec.ts
│   ├── operation-plans-list.spec.ts
│   ├── vve-management.spec.ts
│   └── incidents.component.spec.ts
│
├── services/                # Service layer tests
│   ├── auth.service.spec.ts
│   ├── vessel.service.spec.ts
│   ├── container.service.spec.ts
│   ├── oem.service.spec.ts
│   ├── scheduling.service.spec.ts
│   ├── incidents.service.spec.ts
│   └── incident-types.service.spec.ts
│
├── guards/                  # Route guard tests
│   └── auth.guard.spec.ts
│
├── interceptors/            # HTTP interceptor tests
│   └── auth.interceptor.spec.ts
│
└── shared/                  # Shared component tests
    └── layout.component.spec.ts
```

## 🎯 Test Coverage by User Story

### Sprint A - Infrastructure & Setup

- **US 2.1.1**: Project setup and infrastructure
  - All test files configured with Jasmine/Karma

### Sprint B - Core Features

#### Authentication (US 3.2.x)

- **Login Component**: Google OAuth integration, user authentication
- **Register Component**: User registration with role selection
- **Confirm Email**: Email confirmation flow
- **Auth Service**: Token management, session handling
- **Auth Guard**: Route protection, role-based access control
- **Auth Interceptor**: JWT token injection, error handling

#### Admin Features (US 3.3.x)

- **Admin Dashboard**: Admin landing page
- **User Management**: User activation, role assignment

#### Port Authority Features (US 3.1.x)

- **Dashboard**: Port Authority landing page
- **Vessels Management**: CRUD operations for vessels
- **Docks Management**: CRUD operations for docks
- **Storage Areas**: Container storage management
- **Vessel Types**: Vessel type definitions
- **VVN Pending**: Vessel visit notification review
- **Incident Types**: Incident type management

#### Shipping Agent Features (US 3.4.x)

- **Dashboard**: Shipping Agent landing page
- **Create VVN**: New vessel visit notifications
- **VVN Drafts**: Draft notifications management
- **VVN Submitted**: Submitted notifications tracking
- **VVN Reviewed**: Reviewed notifications
- **Container Management**: Container operations

### Sprint C - OEM Integration (US 4.1.x)

#### Logistic Operator Features

- **Dashboard**: Logistic Operator landing page
- **Daily Schedule**: Daily operation schedule
- **Operation Plans**: Operation plan management
- **VVE Management**: Vessel visit execution tracking
- **Incidents**: Incident reporting and management

#### OEM Services

- **OEM Service**: Operations & Execution Management integration
- **Scheduling Service**: Schedule optimization

## 🧪 Test Types

### Component Tests

- **Creation**: Verify component instantiation
- **Initialization**: Check default values and setup
- **User Interactions**: Button clicks, form submissions
- **HTTP Requests**: API calls and responses
- **Error Handling**: Validation and error states
- **Navigation**: Route navigation logic

### Service Tests

- **HTTP Methods**: GET, POST, PUT, DELETE operations
- **Headers**: Authorization token injection
- **Response Handling**: Success and error scenarios
- **State Management**: Observable subscriptions
- **Storage**: sessionStorage/localStorage interactions

### Guard Tests

- **Authentication Check**: Verify user is logged in
- **Role-Based Access**: Check user has required role
- **Redirection**: Navigate to login/unauthorized pages
- **Route Data**: Extract and validate route requirements

### Interceptor Tests

- **Token Injection**: Add JWT to request headers
- **401 Handling**: Redirect to login on unauthorized
- **403 Handling**: Redirect to access denied page
- **Error Propagation**: Pass through other errors

## 📊 Running Tests

### Run All Tests

```powershell
npm test
```

### Run Tests with Coverage

```powershell
npm run test:coverage
```

### Run Tests in Headless Mode (CI/CD)

```powershell
npm run test:headless
```

### Run Specific Test Suite

```powershell
npm test -- --include="**/authentication/**"
```

### Using PowerShell Script

```powershell
# Run all unit tests
.\run-tests.ps1 -Unit

# Run with coverage
.\run-tests.ps1 -Coverage

# Run in headless mode
.\run-tests.ps1 -Headless

# Watch mode for development
.\run-tests.ps1 -Watch
```

## 🔍 Test Patterns

### Component Test Pattern

```typescript
describe('ComponentName', () => {
  let component: ComponentName;
  let fixture: ComponentFixture<ComponentName>;
  let mockService: jasmine.SpyObj<ServiceName>;

  beforeEach(async () => {
    mockService = jasmine.createSpyObj('ServiceName', ['method1', 'method2']);

    await TestBed.configureTestingModule({
      imports: [ComponentName, HttpClientTestingModule],
      providers: [{ provide: ServiceName, useValue: mockService }],
    }).compileComponents();

    fixture = TestBed.createComponent(ComponentName);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
```

### Service Test Pattern

```typescript
describe('ServiceName', () => {
  let service: ServiceName;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ServiceName],
    });

    service = TestBed.inject(ServiceName);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should make HTTP request', () => {
    service.getData().subscribe();
    const req = httpMock.expectOne('http://api.url');
    req.flush({ data: 'test' });
  });
});
```

### Guard Test Pattern

```typescript
describe('guardName', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: AuthService, useValue: mockAuthService },
      ],
    });
  });

  it('should allow access', async () => {
    mockAuthService.isAuthenticated.and.returnValue(true);
    const result = await TestBed.runInInjectionContext(() => guardName(mockRoute, mockState));
    expect(result).toBe(true);
  });
});
```

## 📈 Coverage Goals

- **Statements**: > 80%
- **Branches**: > 75%
- **Functions**: > 80%
- **Lines**: > 80%

## 🐛 Common Issues

### Issue: "Can't resolve 'zone.js/testing'"

**Solution**: Install zone.js

```powershell
npm install zone.js
```

### Issue: Tests fail with "No provider for HttpClient"

**Solution**: Import HttpClientTestingModule

```typescript
imports: [ComponentName, HttpClientTestingModule];
```

### Issue: "NG0203: inject() must be called from an injection context"

**Solution**: Use TestBed.runInInjectionContext for functional guards/interceptors

```typescript
const result = await TestBed.runInInjectionContext(() => guardName(route, state));
```

## 📝 Writing New Tests

1. **Create test file**: Name it `component-name.spec.ts`
2. **Import dependencies**: Component, TestBed, services
3. **Set up TestBed**: Configure testing module
4. **Write test cases**: Cover all scenarios
5. **Run tests**: Verify all pass
6. **Check coverage**: Ensure coverage goals met

## 🔗 Related Documentation

- [Jasmine Documentation](https://jasmine.github.io/)
- [Angular Testing Guide](https://angular.io/guide/testing)
- [Karma Configuration](https://karma-runner.github.io/latest/config/configuration-file.html)
- [Frontend Testing Guide](../TESTING.md)

## 📞 Support

For issues or questions about tests:

1. Check existing test files for patterns
2. Review Angular testing documentation
3. Check coverage report: `coverage/proj-arqsi-frontend/index.html`
