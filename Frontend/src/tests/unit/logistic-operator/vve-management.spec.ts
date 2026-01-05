import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { VveManagementComponent } from '../../../app/logistic-operator/vve-management/vve-management';

describe('VveManagementComponent', () => {
  let component: VveManagementComponent;
  let fixture: ComponentFixture<VveManagementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VveManagementComponent, HttpClientTestingModule],
    }).compileComponents();

    fixture = TestBed.createComponent(VveManagementComponent);
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
