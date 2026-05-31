import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { GridComponent, GridColumn, GridActionItem } from '../../../../shared/components/grid/grid.component';
import { AdminCategoryService, CreateCategoryRequest, UpdateCategoryRequest } from '../services/admin-category.service';
import { Category } from '../../attendee/home/models/category.models';

@Component({
  selector: 'app-admin-categories',
  standalone: true,
  imports: [CommonModule, FormsModule, GridComponent],
  templateUrl: './categories.component.html',
  styleUrls: ['./categories.component.css'],
})
export class CategoriesComponent implements OnInit, OnDestroy {
  private categoryService = inject(AdminCategoryService);
  private cdr = inject(ChangeDetectorRef);
  private zone = inject(NgZone);
  private destroy$ = new Subject<void>();

  categories: Category[] = [];
  loading = false;
  error: string | null = null;

  showModal = false;
  modalMode: 'add' | 'edit' = 'add';
  editId: number | null = null;
  formName = '';
  saving = false;

  columns: GridColumn[] = [
    { header: 'ID', field: 'id', width: '60px' },
    { header: 'Name', field: 'name' },
    { header: 'Actions', field: 'id', type: 'action' },
  ];

  getRowActionItems: (row: Category) => GridActionItem[] = () => [
    { label: 'Edit', icon: 'bi-pencil', emit: 'edit' },
    { label: 'Delete', icon: 'bi-trash', emit: 'delete' },
  ];

  ngOnInit(): void {
    this.loadCategories();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCategories(): void {
    this.loading = true;
    this.error = null;

    this.categoryService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.zone.run(() => {
            if (res.data) this.categories = res.data;
            this.loading = false;
            this.cdr.detectChanges();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.error = 'Failed to load categories.';
            this.loading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  onEdit(cat: Category): void {
    this.zone.run(() => {
      this.modalMode = 'edit';
      this.editId = cat.id;
      this.formName = cat.name;
      this.showModal = true;
      this.cdr.detectChanges();
    });
  }

  openAddModal(): void {
    this.modalMode = 'add';
    this.editId = null;
    this.formName = '';
    this.showModal = true;
  }

  onDelete(cat: Category): void {
    if (!confirm(`Delete category "${cat.name}"?`)) return;

    this.categoryService
      .delete(cat.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.zone.run(() => {
          this.loadCategories();
          this.cdr.detectChanges();
        });
      });
  }

  save(): void {
    const name = this.formName.trim();
    if (!name) return;

    this.saving = true;

    const obs$ = this.modalMode === 'add'
      ? this.categoryService.create({ name } as CreateCategoryRequest)
      : this.categoryService.update(this.editId!, { name } as UpdateCategoryRequest);

    obs$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.zone.run(() => {
            this.saving = false;
            this.showModal = false;
            this.loadCategories();
            this.cdr.detectChanges();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.saving = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  cancelModal(): void {
    this.zone.run(() => {
      this.showModal = false;
      this.cdr.detectChanges();
    });
  }
}
