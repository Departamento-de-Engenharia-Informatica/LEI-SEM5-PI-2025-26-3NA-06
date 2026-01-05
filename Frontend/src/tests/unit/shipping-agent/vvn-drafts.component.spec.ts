import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { VvnDraftsComponent } from '../../../app/shipping-agent/vvn-drafts/vvn-drafts.component';

describe('VvnDraftsComponent', () => {
  let component: VvnDraftsComponent;
  let fixture: ComponentFixture<VvnDraftsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VvnDraftsComponent, HttpClientTestingModule],
    }).compileComponents();

    fixture = TestBed.createComponent(VvnDraftsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
