import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { NgZone } from '@angular/core';
import { LoginComponent } from '../../../app/login/login.component';
import { AuthService } from '../../../app/services/auth.service';
import { of } from 'rxjs';

describe('LoginComponent', () => {
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockNgZone: jasmine.SpyObj<NgZone>;

  beforeEach(() => {
    mockAuthService = jasmine.createSpyObj('AuthService', [
      'isAuthenticated',
      'getUser',
      'login',
      'authenticateWithGoogle',
    ]);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockNgZone = jasmine.createSpyObj('NgZone', ['run']);

    // Default mock returns - not authenticated
    mockAuthService.isAuthenticated.and.returnValue(false);
    const mockAuthResponse = {
      accessToken: 'mock-token',
      tokenType: 'Bearer',
      expiresIn: 3600,
      user: { userId: '123', email: 'test@example.com', name: 'Test User', role: 'Admin' },
    };
    mockAuthService.authenticateWithGoogle.and.returnValue(of(mockAuthResponse));
  });

  it('should create component instance directly', () => {
    const component = new LoginComponent(mockAuthService, mockRouter, mockNgZone);
    expect(component).toBeTruthy();
  });

  it('should initialize with default values', () => {
    const component = new LoginComponent(mockAuthService, mockRouter, mockNgZone);
    expect(component.errorMessage).toBe('');
    expect(component.isLoading).toBe(false);
  });

  it('should check authentication on init', () => {
    const component = new LoginComponent(mockAuthService, mockRouter, mockNgZone);
    // Mock document.body.appendChild to prevent Google API script from loading
    spyOn(document.body, 'appendChild');
    mockAuthService.isAuthenticated.and.returnValue(false);

    component.ngOnInit();

    expect(mockAuthService.isAuthenticated).toHaveBeenCalled();
  });

  it('should set error message', () => {
    const component = new LoginComponent(mockAuthService, mockRouter, mockNgZone);
    component.errorMessage = 'Login failed';

    expect(component.errorMessage).toBe('Login failed');
  });

  it('should set loading state', () => {
    const component = new LoginComponent(mockAuthService, mockRouter, mockNgZone);
    component.isLoading = true;

    expect(component.isLoading).toBe(true);
  });
});
