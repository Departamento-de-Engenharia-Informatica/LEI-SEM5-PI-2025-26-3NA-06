import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { DailySchedule } from '../../../app/logistic-operator/daily-schedule/daily-schedule';

describe('DailySchedule', () => {
  let component: DailySchedule;
  let fixture: ComponentFixture<DailySchedule>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DailySchedule, HttpClientTestingModule],
    }).compileComponents();

    fixture = TestBed.createComponent(DailySchedule);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize component', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
