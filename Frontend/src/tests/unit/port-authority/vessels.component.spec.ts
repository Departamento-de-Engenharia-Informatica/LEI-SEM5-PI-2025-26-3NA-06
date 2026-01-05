import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { VesselsComponent } from '../../../app/port-authority/vessels/vessels.component';

describe('VesselsComponent', () => {
  let component: VesselsComponent;
  let fixture: ComponentFixture<VesselsComponent>;
  let httpMock: HttpTestingController;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockVessels = [
    {
      id: '1',
      name: 'Vessel One',
      imo: 'IMO1234567',
      capacity: 5000,
      vesselTypeId: 'type1',
    },
    {
      id: '2',
      name: 'Vessel Two',
      imo: 'IMO7654321',
      capacity: 3000,
      vesselTypeId: 'type2',
    },
  ];

  beforeEach(async () => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [VesselsComponent, HttpClientTestingModule],
      providers: [{ provide: Router, useValue: mockRouter }],
    }).compileComponents();

    fixture = TestBed.createComponent(VesselsComponent);
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
    expect(component.vessels).toEqual([]);
    expect(component.filteredVessels).toEqual([]);
    expect(component.searchTerm).toBe('');
    expect(component.isLoading).toBe(true);
  });

  it('should load vessels on init', () => {
    fixture.detectChanges();

    const req = httpMock.expectOne('http://localhost:5218/api/Vessel');
    expect(req.request.method).toBe('GET');
    req.flush(mockVessels);

    expect(component.vessels.length).toBe(2);
    expect(component.isLoading).toBe(false);
  });

  it('should filter vessels by search term', () => {
    component.vessels = [
      {
        id: '1',
        vesselName: 'Vessel One',
        imo: 'IMO1234567',
        capacity: 5000,
        vesselTypeId: 'type1',
      } as any,
      {
        id: '2',
        vesselName: 'Vessel Two',
        imo: 'IMO7654321',
        capacity: 3000,
        vesselTypeId: 'type2',
      } as any,
    ];
    component.filteredVessels = [...component.vessels];
    component.searchTerm = 'Vessel One';

    component.filterVessels();

    expect(component.filteredVessels.length).toBe(1);
    expect(component.filteredVessels[0].vesselName).toBe('Vessel One');
  });

  it('should handle error when loading vessels', () => {
    fixture.detectChanges();

    const req = httpMock.expectOne('http://localhost:5218/api/Vessel');
    req.error(new ProgressEvent('error'));

    expect(component.isLoading).toBe(false);
    expect(component.message).toBeTruthy();
  });
});
