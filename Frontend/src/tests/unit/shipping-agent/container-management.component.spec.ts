import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ContainerManagementComponent } from '../../../app/shipping-agent/container-management/container-management.component';

describe('ContainerManagementComponent', () => {
  let component: ContainerManagementComponent;
  let fixture: ComponentFixture<ContainerManagementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContainerManagementComponent, HttpClientTestingModule],
    }).compileComponents();

    fixture = TestBed.createComponent(ContainerManagementComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
