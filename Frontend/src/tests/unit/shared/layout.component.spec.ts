import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { LayoutComponent } from '../../../app/layout/layout.component';
import { AuthService } from '../../../app/services/auth.service';

describe('LayoutComponent', () => {
  let component: LayoutComponent;
  let fixture: ComponentFixture<LayoutComponent>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockAuthService = jasmine.createSpyObj('AuthService', ['getUser', 'logout']);

    await TestBed.configureTestingModule({
      imports: [LayoutComponent],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: AuthService, useValue: mockAuthService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LayoutComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with user data on init', () => {
    mockAuthService.getUser.and.returnValue({
      userId: '1',
      email: 'test@example.com',
      role: 'Admin',
      name: 'Test User',
    });

    component.ngOnInit();

    expect(component.userRole).toBe('Admin');
    expect(component.userName).toBe('Test User');
  });

  it('should redirect to login if user is not authenticated', () => {
    mockAuthService.getUser.and.returnValue(null);

    component.ngOnInit();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should logout and redirect to login', () => {
    component.logout();

    expect(mockAuthService.logout).toHaveBeenCalled();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should navigate to admin dashboard for Admin role', () => {
    component.userRole = 'Admin';

    component.goToDashboard();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/admin']);
  });

  it('should navigate to port authority dashboard for PortAuthorityOfficer role', () => {
    component.userRole = 'PortAuthorityOfficer';

    component.goToDashboard();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/port-authority']);
  });

  it('should navigate to logistic operator dashboard for LogisticOperator role', () => {
    component.userRole = 'LogisticOperator';

    component.goToDashboard();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/logistic-operator']);
  });

  it('should navigate to shipping agent dashboard for ShippingAgentRepresentative role', () => {
    component.userRole = 'ShippingAgentRepresentative';

    component.goToDashboard();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/shipping-agent']);
  });

  it('should redirect to login for unknown role', () => {
    component.userRole = 'UnknownRole';

    component.goToDashboard();

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
  });
});
