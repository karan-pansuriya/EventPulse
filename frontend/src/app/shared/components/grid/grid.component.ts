import { Component, Input, Output, EventEmitter, HostListener, TemplateRef, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, NgTemplateOutlet } from '@angular/common';
import { DatePipe } from '@angular/common';

export interface GridColumn {
  header: string;
  field: string;
  sortable?: boolean;
  type?: 'text' | 'truncate' | 'action' | 'toggle' | 'currency' | 'percent' | 'date' | 'edit' | 'delete';
  width?: string;
  formatter?: (value: unknown) => string;
}

export interface GridActionItem {
  label: string;
  icon: string;
  emit: 'edit' | 'delete';
}

const DEFAULT_ACTIONS: GridActionItem[] = [
  { label: 'Edit', icon: 'assets/icons/Edit.svg', emit: 'edit' },
  { label: 'Delete', icon: 'assets/icons/Delete.svg', emit: 'delete' },
];

@Component({
  selector: 'app-grid',
  standalone: true,
  imports: [CommonModule, NgTemplateOutlet, DatePipe],
  templateUrl: './grid.component.html',
  styleUrls: ['./grid.component.css'],
})
export class GridComponent<T> implements OnInit, OnDestroy {
  @Input() columns: GridColumn[] = [];
  @Input() data: T[] = [];
  @Input() totalRecords = 0;
  @Input() currentPage = 1;
  @Input() trackByField!: string;
  @Input() pageSize: number = 10;
  @Input() actionItems: GridActionItem[] | ((row: T) => GridActionItem[]) = DEFAULT_ACTIONS;
  @Input() actionTemplate?: TemplateRef<{ $implicit: T }>;

  /** Show spinner overlay; mirrors master-data's ds.isLoading */
  @Input() isLoading = false;

  @Input() errorMessage: string | null = null;

  getRowActionItems(row: T): GridActionItem[] {
    return typeof this.actionItems === 'function' ? this.actionItems(row) : this.actionItems;
  }

  @Output() pageChange = new EventEmitter<number>();
  @Output() pageSizeChange = new EventEmitter<number>();
  @Output() sortChange = new EventEmitter<{ column: string; direction: string }>();
  @Output() edit = new EventEmitter<T>();
  @Output() delete = new EventEmitter<T>();
  @Output() toggle = new EventEmitter<T>();
  @Output() rowClick = new EventEmitter<T>();

  @HostListener('document:click')
  onDocumentClick(): void {
    if (this.activeDropdownRow !== null) {
      this.activeDropdownRow = null;
    }
  }

  private scrollHandler = () => {
    this.activeDropdownRow = null;
  };

  ngOnInit(): void {
    document.addEventListener('scroll', this.scrollHandler, true);
  }

  ngOnDestroy(): void {
    document.removeEventListener('scroll', this.scrollHandler, true);
  }

  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  activeDropdownRow: T | null = null;
  dropdownTop = 0;
  dropdownLeft = 0;

  sort(field: string) {
    if (this.sortColumn === field) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = field;
      this.sortDirection = 'asc';
    }
    this.sortChange.emit({ column: this.sortColumn, direction: this.sortDirection });
  }

  trackByFn = (index: number, item: T) => {
    return this.trackByField ? (item as any)[this.trackByField] : index;
  };

  get totalPages(): number {
    return Math.ceil(this.totalRecords / this.pageSize);
  }

  changePage(page: number) {
    if (page < 1 || page > this.totalPages) return;
    this.pageChange.emit(page);
  }

  changePageSize(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.pageSizeChange.emit(+value);
  }

  getStartRecord() {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  getEndRecord() {
    return Math.min(this.currentPage * this.pageSize, this.totalRecords);
  }

  toggleDropdown(event: MouseEvent, row: T): void {
    event.stopPropagation();
    if (this.activeDropdownRow === row) {
      this.activeDropdownRow = null;
      return;
    }
    const btn = event.currentTarget as HTMLElement;
    const rect = btn.getBoundingClientRect();
    this.dropdownTop = rect.bottom + 4;
    this.dropdownLeft = rect.right - 150;
    this.activeDropdownRow = row;
  }

  closeAllDropdowns(): void {
    this.activeDropdownRow = null;
  }

  handleAction(action: GridActionItem, row: T): void {
    this.closeAllDropdowns();
    if (action.emit === 'edit') {
      this.edit.emit(row);
    } else {
      this.delete.emit(row);
    }
  }

  asDate(value: unknown): string | number | Date | null {
    return value as string | number | Date | null;
  }

  getFieldValue(row: T, field: string): unknown {
    return (row as any)[field];
  }

  displayValue(value: unknown): string {
    if (value === null || value === undefined || value === '') return '-';
    return String(value);
  }
}