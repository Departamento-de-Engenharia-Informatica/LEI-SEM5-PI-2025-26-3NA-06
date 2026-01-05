import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { VesselTypesComponent } from '../../../app/port-authority/vessel-types/vessel-types.component';

describe('VesselTypesComponent', () => {
  let component: VesselTypesComponent;
  let fixture: ComponentFixture<VesselTypesComponent>;
  let httpMock: HttpTestingController;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockVesselTypes = [
    { id: '1', name: 'Container Ship', description: 'Cargo containers' },
    { id: '2', name: 'Tanker', description: 'Liquid cargo' },
  ];

  beforeEach(async () => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [VesselTypesComponent, HttpClientTestingModule],
      providers: [{ provide: Router, useValue: mockRouter }],
    }).compileComponents();

    fixture = TestBed.createComponent(VesselTypesComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load vessel types on init', () => {
    fixture.detectChanges();

    const req = httpMock.expectOne('http://localhost:5218/api/VesselType');
    expect(req.request.method).toBe('GET');
    req.flush(mockVesselTypes);

    expect(component.vesselTypes.length).toBe(2);
  });
});
