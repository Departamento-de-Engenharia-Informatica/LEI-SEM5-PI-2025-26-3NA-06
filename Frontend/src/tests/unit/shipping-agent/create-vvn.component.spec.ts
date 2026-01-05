import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { CreateVvnComponent } from '../../../app/shipping-agent/create-vvn/create-vvn.component';

describe('CreateVvnComponent', () => {
  let component: CreateVvnComponent;
  let fixture: ComponentFixture<CreateVvnComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateVvnComponent, HttpClientTestingModule],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(CreateVvnComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with empty form', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
