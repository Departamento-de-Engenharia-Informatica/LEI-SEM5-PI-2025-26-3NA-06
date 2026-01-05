import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { ConfirmEmailComponent } from '../../../app/confirm-email/confirm-email.component';

describe('ConfirmEmailComponent', () => {
  let component: ConfirmEmailComponent;
  let fixture: ComponentFixture<ConfirmEmailComponent>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockActivatedRoute: any;

  beforeEach(async () => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);
    mockActivatedRoute = {
      snapshot: {
        queryParamMap: {
          get: jasmine.createSpy('get'),
        },
      },
    };

    await TestBed.configureTestingModule({
      imports: [ConfirmEmailComponent, HttpClientTestingModule],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfirmEmailComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with default message', () => {
    expect(component.message).toBe('Processing your activation...');
    expect(component.success).toBe(false);
    expect(component.showLoginLink).toBe(false);
  });

  it('should handle success status from query params', () => {
    mockActivatedRoute.snapshot.queryParamMap.get.and.callFake((key: string) => {
      if (key === 'status') return 'success';
      return null;
    });

    component.ngOnInit();

    expect(component.success).toBe(true);
    expect(component.showLoginLink).toBe(true);
    expect(component.message).toContain('Email confirmed successfully');
  });

  it('should handle error status from query params', () => {
    mockActivatedRoute.snapshot.queryParamMap.get.and.callFake((key: string) => {
      if (key === 'status') return 'error';
      if (key === 'message') return 'Invalid token';
      return null;
    });

    component.ngOnInit();

    expect(component.success).toBe(false);
    expect(component.showLoginLink).toBe(true);
    expect(component.message).toBe('Invalid token');
  });

  it('should handle missing token', () => {
    mockActivatedRoute.snapshot.queryParamMap.get.and.returnValue(null);

    component.ngOnInit();

    expect(component.message).toBe('Invalid activation link. No token provided.');
    expect(component.success).toBe(false);
    expect(component.showLoginLink).toBe(true);
  });
});
