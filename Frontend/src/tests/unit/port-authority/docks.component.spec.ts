import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { DocksComponent } from '../../../app/port-authority/docks/docks.component';

describe('DocksComponent', () => {
  let component: DocksComponent;
  let fixture: ComponentFixture<DocksComponent>;
  let httpMock: HttpTestingController;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockDocks = [
    { id: '1', name: 'Dock A', capacity: 10, vesselTypeIds: ['type1'] },
    { id: '2', name: 'Dock B', capacity: 15, vesselTypeIds: ['type2'] },
  ];

  const mockVesselTypes = [
    { id: 'type1', name: 'Container Ship' },
    { id: 'type2', name: 'Cargo Ship' },
  ];

  beforeEach(async () => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [DocksComponent, HttpClientTestingModule],
      providers: [{ provide: Router, useValue: mockRouter }],
    }).compileComponents();

    fixture = TestBed.createComponent(DocksComponent);
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
    expect(component.docks).toEqual([]);
    expect(component.filteredDocks).toEqual([]);
    expect(component.vesselTypes).toEqual([]);
    expect(component.searchTerm).toBe('');
    expect(component.isLoading).toBe(true);
  });

  it('should load docks and vessel types on init', () => {
    fixture.detectChanges();

    const vesselTypesReq = httpMock.expectOne('http://localhost:5218/api/VesselType');
    const docksReq = httpMock.expectOne('http://localhost:5218/api/Dock');

    vesselTypesReq.flush(mockVesselTypes);
    docksReq.flush(mockDocks);

    expect(component.docks.length).toBe(2);
    expect(component.vesselTypes.length).toBe(2);
    expect(component.isLoading).toBe(false);
  });

  it('should filter docks by search term', () => {
    component.docks = [
      {
        id: '1',
        dockName: 'Dock A',
        locationDescription: 'North Harbor',
        capacity: 100,
        allowedVesselTypeIds: [],
      } as any,
      {
        id: '2',
        dockName: 'Dock B',
        locationDescription: 'South Harbor',
        capacity: 200,
        allowedVesselTypeIds: [],
      } as any,
    ];
    component.filteredDocks = [...component.docks];
    component.searchTerm = 'Dock A';

    component.filterDocks();

    expect(component.filteredDocks.length).toBe(1);
    expect(component.filteredDocks[0].dockName).toBe('Dock A');
  });
});
