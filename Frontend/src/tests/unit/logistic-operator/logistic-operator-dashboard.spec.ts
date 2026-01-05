import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { LogisticOperatorDashboard } from '../../../app/logistic-operator/logistic-operator-dashboard/logistic-operator-dashboard';

describe('LogisticOperatorDashboard', () => {
  let component: LogisticOperatorDashboard;
  let fixture: ComponentFixture<LogisticOperatorDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LogisticOperatorDashboard, HttpClientTestingModule],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(LogisticOperatorDashboard);
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
