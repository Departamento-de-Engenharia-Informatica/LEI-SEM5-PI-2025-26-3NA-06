import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService, AuthResponse } from '../../../app/services/auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  const mockAuthResponse: AuthResponse = {
    accessToken: 'mock-token',
    tokenType: 'Bearer',
    expiresIn: 3600,
    user: {
      userId: '123',
      email: 'test@example.com',
      role: 'Admin',
      name: 'Test User',
    },
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService],
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);

    // Clear sessionStorage before each test
    sessionStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should authenticate with Google', () => {
    const idToken = 'google-id-token';

    service.authenticateWithGoogle(idToken).subscribe((response) => {
      expect(response).toEqual(mockAuthResponse);
      expect(service.isAuthenticated()).toBe(true);
    });

    const req = httpMock.expectOne('http://localhost:5001/auth/google');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ idToken });
    req.flush(mockAuthResponse);
  });

  it('should store auth data in sessionStorage', () => {
    service.authenticateWithGoogle('test-token').subscribe();

    const req = httpMock.expectOne('http://localhost:5001/auth/google');
    req.flush(mockAuthResponse);

    expect(sessionStorage.getItem('auth_token')).toBe('mock-token');
    expect(sessionStorage.getItem('auth_user')).toBeTruthy();
  });

  it('should return token when authenticated', () => {
    // Store token in sessionStorage before getting it
    sessionStorage.setItem('auth_token', 'mock-token');
    const expiryDate = new Date();
    expiryDate.setSeconds(expiryDate.getSeconds() + 3600); // 1 hour from now
    sessionStorage.setItem('auth_token_expiry', expiryDate.toISOString());

    const token = service.getToken();

    expect(token).toBe('mock-token');
  });

  it('should return null when not authenticated', () => {
    const token = service.getToken();
    expect(token).toBeNull();
  });

  it('should check if user is authenticated', () => {
    expect(service.isAuthenticated()).toBe(false);

    sessionStorage.setItem('auth_token', 'mock-token');
    const futureDate = new Date();
    futureDate.setHours(futureDate.getHours() + 1);
    sessionStorage.setItem('auth_token_expiry', futureDate.toISOString());

    expect(service.isAuthenticated()).toBe(true);
  });

  it('should return false for expired token', () => {
    sessionStorage.setItem('auth_token', 'mock-token');
    const pastDate = new Date();
    pastDate.setHours(pastDate.getHours() - 1);
    sessionStorage.setItem('auth_token_expiry', pastDate.toISOString());

    expect(service.isAuthenticated()).toBe(false);
  });

  it('should get user from storage', () => {
    sessionStorage.setItem('auth_user', JSON.stringify(mockAuthResponse.user));

    const user = service.getUser();
    expect(user).toEqual(mockAuthResponse.user);
  });

  it('should logout and clear storage', () => {
    sessionStorage.setItem('auth_token', 'mock-token');
    sessionStorage.setItem('auth_user', JSON.stringify(mockAuthResponse.user));

    service.logout();

    expect(sessionStorage.getItem('auth_token')).toBeNull();
    expect(sessionStorage.getItem('auth_user')).toBeNull();
  });

  it('should emit user changes via observable', (done) => {
    service.currentUser$.subscribe((user) => {
      if (user) {
        expect(user).toEqual(mockAuthResponse.user);
        done();
      }
    });

    service.authenticateWithGoogle('test-token').subscribe();

    const req = httpMock.expectOne('http://localhost:5001/auth/google');
    req.flush(mockAuthResponse);
  });
});
