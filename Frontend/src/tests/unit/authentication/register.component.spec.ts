import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router, ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { RegisterComponent } from '../../../app/register/register.component';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let httpMock: HttpTestingController;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockActivatedRoute: any;

  beforeEach(async () => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockActivatedRoute = {
      queryParams: of({ email: 'test@example.com', name: 'Test User' }),
    };

    await TestBed.configureTestingModule({
      imports: [RegisterComponent, HttpClientTestingModule],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with empty values', () => {
    expect(component.email).toBe('');
    expect(component.username).toBe('');
    expect(component.role).toBe('');
    expect(component.message).toBe('');
    expect(component.isLoading).toBe(false);
    expect(component.isSuccess).toBe(false);
  });

  it('should populate email and username from query params', () => {
    fixture.detectChanges();

    expect(component.email).toBe('test@example.com');
    expect(component.username).toBe('TestUser');
  });

  it('should set loading state on form submit', () => {
    component.email = 'test@example.com';
    component.username = 'testuser';
    component.role = 'Admin';

    component.onSubmit();

    const req = httpMock.expectOne('http://localhost:5218/api/User/register');
    expect(component.isLoading).toBe(true);
    req.flush({ message: 'Success' });
  });

  it('should send registration request with correct data', () => {
    component.email = 'test@example.com';
    component.username = 'testuser';
    component.role = 'PortAuthority';

    component.onSubmit();

    const req = httpMock.expectOne('http://localhost:5218/api/User/register');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      email: 'test@example.com',
      username: 'testuser',
      role: 'PortAuthority',
    });

    req.flush({ message: 'Success' });
  });

  it('should handle successful registration', () => {
    component.email = 'test@example.com';
    component.username = 'testuser';
    component.role = 'ShippingAgent';

    component.onSubmit();

    const req = httpMock.expectOne('http://localhost:5218/api/User/register');
    req.flush({ message: 'Registration successful' });

    expect(component.isSuccess).toBe(true);
    expect(component.isLoading).toBe(false);
  });
});
