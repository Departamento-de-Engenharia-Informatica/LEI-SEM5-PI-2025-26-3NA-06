import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ContainerService, Container, UpsertContainer } from '../../services/container.service';

@Component({
  selector: 'app-container-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './container-management.component.html',
  styleUrls: ['./container-management.component.css'],
})
export class ContainerManagementComponent implements OnInit {
  containers: Container[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';
  showForm = false;
  editingContainer: Container | null = null;

  containerForm: UpsertContainer = {
    isoCode: '',
    isHazardous: false,
    cargoType: '',
    description: '',
  };

  constructor(
    private containerService: ContainerService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit() {
    this.loadContainers();
  }

  loadContainers() {
    this.loading = true;
    this.errorMessage = '';
    this.containerService.getAll().subscribe({
      next: (data) => {
        this.ngZone.run(() => {
          this.containers = data;
          this.loading = false;
          this.cdr.detectChanges();
        });
      },
      error: (error) => {
        this.ngZone.run(() => {
          this.errorMessage = error.error?.message || 'Failed to load containers';
          this.loading = false;
          this.cdr.detectChanges();
        });
      },
    });
  }

  openCreateForm() {
    this.showForm = true;
    this.editingContainer = null;
    this.containerForm = {
      isoCode: '',
      isHazardous: false,
      cargoType: '',
      description: '',
    };
  }

  openEditForm(container: Container) {
    this.showForm = true;
    this.editingContainer = container;
    this.containerForm = {
      isoCode: container.isoCode,
      isHazardous: container.isHazardous,
      cargoType: container.cargoType,
      description: container.description,
    };
  }

  closeForm() {
    this.showForm = false;
    this.editingContainer = null;
    this.errorMessage = '';
    this.successMessage = '';
  }

  saveContainer() {
    this.errorMessage = '';
    this.successMessage = '';
    this.loading = true;

    if (this.editingContainer) {
      // Update existing container
      this.containerService.update(this.editingContainer.id, this.containerForm).subscribe({
        next: () => {
          this.ngZone.run(() => {
            this.successMessage = 'Container updated successfully';
            this.loading = false;
            this.loadContainers();
            setTimeout(() => this.closeForm(), 1500);
            this.cdr.detectChanges();
          });
        },
        error: (error) => {
          this.ngZone.run(() => {
            this.errorMessage = error.error?.message || 'Failed to update container';
            this.loading = false;
            this.cdr.detectChanges();
          });
        },
      });
    } else {
      // Create new container
      this.containerService.create(this.containerForm).subscribe({
        next: () => {
          this.ngZone.run(() => {
            this.successMessage = 'Container created successfully!';
            this.loading = false;
            this.loadContainers();
            this.cdr.detectChanges();
            // Keep form open with success message for 2 seconds before closing
            setTimeout(() => {
              this.closeForm();
              this.cdr.detectChanges();
            }, 2000);
          });
        },
        error: (error) => {
          this.ngZone.run(() => {
            this.errorMessage = error.error?.message || 'Failed to create container';
            this.loading = false;
            this.cdr.detectChanges();
          });
        },
      });
    }
  }

  deleteContainer(container: Container) {
    if (!confirm(`Are you sure you want to delete container ${container.isoCode}?`)) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.containerService.delete(container.id).subscribe({
      next: () => {
        this.ngZone.run(() => {
          this.successMessage = 'Container deleted successfully';
          this.loadContainers();
          this.cdr.detectChanges();
          setTimeout(() => {
            this.successMessage = '';
            this.cdr.detectChanges();
          }, 3000);
        });
      },
      error: (error) => {
        this.ngZone.run(() => {
          this.errorMessage = error.error?.message || 'Failed to delete container';
          this.loading = false;
          this.cdr.detectChanges();
        });
      },
    });
  }

  formatIsoCode(isoCode: string): string {
    // Format as AAAU123456-C
    if (isoCode && isoCode.length === 11 && !isoCode.includes('-')) {
      return `${isoCode.substring(0, 10)}-${isoCode.substring(10)}`;
    }
    return isoCode;
  }
}
