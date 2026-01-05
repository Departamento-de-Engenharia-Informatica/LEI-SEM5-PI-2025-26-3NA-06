import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { VesselService } from '../../../app/services/vessel.service';
import { AuthService } from '../../../app/services/auth.service';

describe('VesselService', () => {
  let service: VesselService;
  let httpMock: HttpTestingController;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    mockAuthService = jasmine.createSpyObj('AuthService', ['getToken']);
    mockAuthService.getToken.and.returnValue('mock-token');

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [VesselService, { provide: AuthService, useValue: mockAuthService }],
    });

    service = TestBed.inject(VesselService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should include authorization header in requests', () => {
    service.getAllVessels().subscribe();

    const req = httpMock.expectOne('http://localhost:5218/api/Vessel');
    expect(req.request.headers.get('Authorization')).toBe('Bearer mock-token');
    req.flush([]);
  });
});
