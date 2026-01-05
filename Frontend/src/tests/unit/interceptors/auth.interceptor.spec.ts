import { TestBed } from '@angular/core/testing';
import {
  HttpInterceptorFn,
  HttpRequest,
  HttpHandler,
  HttpErrorResponse,
} from '@angular/common/http';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { authInterceptor } from '../../../app/interceptors/auth.interceptor';
import { AuthService } from '../../../app/services/auth.service';

describe('authInterceptor', () => {
  let mockRouter: jasmine.SpyObj<Router>;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockAuthService = jasmine.createSpyObj('AuthService', ['getToken', 'logout']);

    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: AuthService, useValue: mockAuthService },
      ],
    });
  });

  it('should add Authorization header when token exists', (done) => {
    mockAuthService.getToken.and.returnValue('mock-token');

    const mockRequest = new HttpRequest('GET', '/api/test');
    const mockHandler = jasmine.createSpy('HttpHandler').and.returnValue(of({}));

    TestBed.runInInjectionContext(() => {
      authInterceptor(mockRequest, mockHandler).subscribe(() => {
        expect(mockHandler).toHaveBeenCalled();
        const modifiedRequest = mockHandler.calls.mostRecent().args[0] as HttpRequest<any>;
        expect(modifiedRequest.headers.get('Authorization')).toBe('Bearer mock-token');
        done();
      });
    });
  });

  it('should not add Authorization header when token does not exist', (done) => {
    mockAuthService.getToken.and.returnValue(null);

    const mockRequest = new HttpRequest('GET', '/api/test');
    const mockHandler = jasmine.createSpy('HttpHandler').and.returnValue(of({}));

    TestBed.runInInjectionContext(() => {
      authInterceptor(mockRequest, mockHandler).subscribe(() => {
        expect(mockHandler).toHaveBeenCalled();
        const modifiedRequest = mockHandler.calls.mostRecent().args[0] as HttpRequest<any>;
        expect(modifiedRequest.headers.has('Authorization')).toBe(false);
        done();
      });
    });
  });

  it('should redirect to login on 401 error', (done) => {
    mockAuthService.getToken.and.returnValue('mock-token');

    const mockRequest = new HttpRequest('GET', '/api/test');
    const error = new HttpErrorResponse({ status: 401 });
    const mockHandler = jasmine.createSpy('HttpHandler').and.returnValue(throwError(() => error));

    TestBed.runInInjectionContext(() => {
      authInterceptor(mockRequest, mockHandler).subscribe({
        error: (err) => {
          expect(mockAuthService.logout).toHaveBeenCalled();
          expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
          done();
        },
      });
    });
  });

  it('should redirect to unauthorized on 403 error', (done) => {
    mockAuthService.getToken.and.returnValue('mock-token');

    const mockRequest = new HttpRequest('GET', '/api/test');
    const error = new HttpErrorResponse({ status: 403 });
    const mockHandler = jasmine.createSpy('HttpHandler').and.returnValue(throwError(() => error));

    TestBed.runInInjectionContext(() => {
      authInterceptor(mockRequest, mockHandler).subscribe({
        error: (err) => {
          expect(mockRouter.navigate).toHaveBeenCalledWith(['/unauthorized']);
          done();
        },
      });
    });
  });
});
