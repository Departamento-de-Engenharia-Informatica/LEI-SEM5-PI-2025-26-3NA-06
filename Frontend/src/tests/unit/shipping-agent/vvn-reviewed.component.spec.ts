import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { VvnReviewedComponent } from '../../../app/shipping-agent/vvn-reviewed/vvn-reviewed.component';

describe('VvnReviewedComponent', () => {
  let component: VvnReviewedComponent;
  let fixture: ComponentFixture<VvnReviewedComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VvnReviewedComponent, HttpClientTestingModule],
    }).compileComponents();

    fixture = TestBed.createComponent(VvnReviewedComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
