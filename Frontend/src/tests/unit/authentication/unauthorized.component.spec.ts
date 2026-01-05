import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, Navigation } from '@angular/router';
import { provideRouter } from '@angular/router';
import { UnauthorizedComponent } from '../../../app/unauthorized/unauthorized.component';

describe('UnauthorizedComponent', () => {
  let component: UnauthorizedComponent;
  let fixture: ComponentFixture<UnauthorizedComponent>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate', 'getCurrentNavigation']);
    mockRouter.getCurrentNavigation.and.returnValue({
      id: 1,
      initialUrl: {} as any,
      extractedUrl: {} as any,
      trigger: 'imperative' as any,
      previousNavigation: null,
      extras: { state: { message: 'Test message' } },
    } as any);

    await TestBed.configureTestingModule({
      imports: [UnauthorizedComponent],
      providers: [{ provide: Router, useValue: mockRouter }, provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(UnauthorizedComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display unauthorized message', () => {
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Unauthorized');
  });
});
