import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ChangeDetectorRef, NgZone } from '@angular/core';
import { UserManagementComponent } from '../../../app/admin/user-management/user-management.component';

describe('UserManagementComponent', () => {
  let component: UserManagementComponent;
  let fixture: ComponentFixture<UserManagementComponent>;
  let httpMock: HttpTestingController;

  const mockUsers = [
    {
      id: '1',
      username: 'testuser',
      email: 'test@example.com',
      role: 'PortAuthorityOfficer',
      isActive: false,
      confirmationToken: 'token123',
    },
    {
      id: '2',
      username: 'activeuser',
      email: 'active@example.com',
      role: 'ShippingAgentRepresentative',
      isActive: true,
      confirmationToken: '',
    },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserManagementComponent, HttpClientTestingModule],
    }).compileComponents();

    fixture = TestBed.createComponent(UserManagementComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with default values', () => {
    expect(component.users).toEqual([]);
    expect(component.filteredUsers).toEqual([]);
    expect(component.showInactiveOnly).toBe(true);
    expect(component.availableRoles).toEqual([
      'Admin',
      'PortAuthorityOfficer',
      'LogisticOperator',
      'ShippingAgentRepresentative',
    ]);
  });

  it('should load inactive users on init', () => {
    fixture.detectChanges();

    const req = httpMock.expectOne('http://localhost:5218/api/User/inactive-users');
    expect(req.request.method).toBe('GET');
    req.flush(mockUsers);

    expect(component.users.length).toBe(2);
    expect(component.filteredUsers.length).toBe(2);
  });

  it('should load all users when filter is toggled', () => {
    component.showInactiveOnly = false;
    component.toggleFilter();

    const req = httpMock.expectOne('http://localhost:5218/api/User/inactive-users');
    req.flush(mockUsers);
  });

  it('should initialize selected roles for users', () => {
    fixture.detectChanges();

    const req = httpMock.expectOne('http://localhost:5218/api/User/inactive-users');
    req.flush(mockUsers);

    expect(component.selectedRole['1']).toBe('PortAuthorityOfficer');
    expect(component.selectedRole['2']).toBe('ShippingAgentRepresentative');
  });

  it('should handle error when loading users', () => {
    spyOn(window, 'alert');
    fixture.detectChanges();

    const req = httpMock.expectOne('http://localhost:5218/api/User/inactive-users');
    req.error(new ProgressEvent('error'));

    expect(window.alert).toHaveBeenCalledWith(
      'Failed to load users. Please ensure you are logged in as Admin.'
    );
  });
});
