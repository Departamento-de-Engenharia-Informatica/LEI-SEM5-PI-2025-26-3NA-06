import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { PortAuthorityDashboardComponent } from '../../../app/port-authority/port-authority-dashboard.component';

describe('PortAuthorityDashboardComponent', () => {
  let component: PortAuthorityDashboardComponent;
  let fixture: ComponentFixture<PortAuthorityDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PortAuthorityDashboardComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(PortAuthorityDashboardComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render dashboard', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement).toBeTruthy();
  });
});
