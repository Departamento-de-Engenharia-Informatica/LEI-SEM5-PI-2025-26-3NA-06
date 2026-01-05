import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { StorageAreasComponent } from '../../../app/port-authority/storage-areas/storage-areas.component';

describe('StorageAreasComponent', () => {
  let component: StorageAreasComponent;
  let fixture: ComponentFixture<StorageAreasComponent>;
  let httpMock: HttpTestingController;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockStorageAreas = [
    { id: '1', name: 'Storage A', capacity: 1000, currentOccupancy: 500 },
    { id: '2', name: 'Storage B', capacity: 2000, currentOccupancy: 1500 },
  ];

  beforeEach(async () => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [StorageAreasComponent, HttpClientTestingModule],
      providers: [{ provide: Router, useValue: mockRouter }],
    }).compileComponents();

    fixture = TestBed.createComponent(StorageAreasComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load storage areas on init', () => {
    fixture.detectChanges();

    const req = httpMock.expectOne('http://localhost:5218/api/StorageArea');
    expect(req.request.method).toBe('GET');
    req.flush(mockStorageAreas);

    expect(component.storageAreas.length).toBe(2);
  });
});
