import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { IncidentTypesComponent } from '../../../app/port-authority/incident-types/incident-types.component';

describe('IncidentTypesComponent', () => {
  let component: IncidentTypesComponent;
  let fixture: ComponentFixture<IncidentTypesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IncidentTypesComponent, HttpClientTestingModule],
    }).compileComponents();

    fixture = TestBed.createComponent(IncidentTypesComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
