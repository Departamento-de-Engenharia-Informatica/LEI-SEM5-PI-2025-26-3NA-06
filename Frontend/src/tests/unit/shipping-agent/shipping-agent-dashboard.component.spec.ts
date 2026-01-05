import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, RouterLink } from '@angular/router';
import { ShippingAgentDashboardComponent } from '../../../app/shipping-agent/shipping-agent-dashboard.component';

describe('ShippingAgentDashboardComponent', () => {
  let component: ShippingAgentDashboardComponent;
  let fixture: ComponentFixture<ShippingAgentDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShippingAgentDashboardComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ShippingAgentDashboardComponent);
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
