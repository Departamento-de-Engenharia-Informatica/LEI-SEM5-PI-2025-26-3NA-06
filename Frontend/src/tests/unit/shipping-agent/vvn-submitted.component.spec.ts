import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { VvnSubmittedComponent } from '../../../app/shipping-agent/vvn-submitted/vvn-submitted.component';

describe('VvnSubmittedComponent', () => {
  let component: VvnSubmittedComponent;
  let fixture: ComponentFixture<VvnSubmittedComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VvnSubmittedComponent, HttpClientTestingModule],
    }).compileComponents();

    fixture = TestBed.createComponent(VvnSubmittedComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
