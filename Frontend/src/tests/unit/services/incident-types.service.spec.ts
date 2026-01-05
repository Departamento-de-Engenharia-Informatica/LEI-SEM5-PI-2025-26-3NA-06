import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { IncidentTypesService } from '../../../app/services/incident-types.service';
import { AuthService } from '../../../app/services/auth.service';

describe('IncidentTypesService', () => {
  let service: IncidentTypesService;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    mockAuthService = jasmine.createSpyObj('AuthService', ['getToken']);
    mockAuthService.getToken.and.returnValue('mock-token');

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [IncidentTypesService, { provide: AuthService, useValue: mockAuthService }],
    });

    service = TestBed.inject(IncidentTypesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
