import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { OperationPlansListComponent } from '../../../app/logistic-operator/operation-plans-list/operation-plans-list';

describe('OperationPlansListComponent', () => {
  let component: OperationPlansListComponent;
  let fixture: ComponentFixture<OperationPlansListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OperationPlansListComponent, HttpClientTestingModule],
    }).compileComponents();

    fixture = TestBed.createComponent(OperationPlansListComponent);
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
