import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { authGuard } from '../../../app/guards/auth.guard';
import { AuthService } from '../../../app/services/auth.service';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

describe('authGuard', () => {
  let mockRouter: jasmine.SpyObj<Router>;
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockHttpClient: jasmine.SpyObj<HttpClient>;
  let mockRoute: ActivatedRouteSnapshot;
  let mockState: RouterStateSnapshot;

  beforeEach(() => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockAuthService = jasmine.createSpyObj('AuthService', ['isAuthenticated', 'getUser']);
    mockHttpClient = jasmine.createSpyObj('HttpClient', ['post']);

    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: AuthService, useValue: mockAuthService },
        { provide: HttpClient, useValue: mockHttpClient },
      ],
    });

    mockRoute = {
      data: {},
    } as any;

    mockState = {
      url: '/test-url',
    } as RouterStateSnapshot;
  });

  it('should allow access when authenticated and no role required', async () => {
    mockAuthService.isAuthenticated.and.returnValue(true);
    mockAuthService.getUser.and.returnValue({
      userId: '1',
      email: 'test@example.com',
      role: 'Admin',
      name: 'Test User',
    });

    const result = await TestBed.runInInjectionContext(() => authGuard(mockRoute, mockState));

    expect(result).toBe(true);
  });

  it('should redirect to login when not authenticated', async () => {
    mockAuthService.isAuthenticated.and.returnValue(false);

    const result = await TestBed.runInInjectionContext(() => authGuard(mockRoute, mockState));

    expect(result).toBe(false);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should redirect to login when user is null', async () => {
    mockAuthService.isAuthenticated.and.returnValue(true);
    mockAuthService.getUser.and.returnValue(null);

    const result = await TestBed.runInInjectionContext(() => authGuard(mockRoute, mockState));

    expect(result).toBe(false);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should redirect to login when user has no role', async () => {
    mockAuthService.isAuthenticated.and.returnValue(true);
    mockAuthService.getUser.and.returnValue({
      userId: '1',
      email: 'test@example.com',
      role: '',
      name: 'Test User',
    });

    const result = await TestBed.runInInjectionContext(() => authGuard(mockRoute, mockState));

    expect(result).toBe(false);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should allow access when user has required role', async () => {
    mockRoute.data = { role: 'Admin' };
    mockAuthService.isAuthenticated.and.returnValue(true);
    mockAuthService.getUser.and.returnValue({
      userId: '1',
      email: 'test@example.com',
      role: 'Admin',
      name: 'Test User',
    });

    const result = await TestBed.runInInjectionContext(() => authGuard(mockRoute, mockState));

    expect(result).toBe(true);
  });
});
