import { Component, Input, Output, EventEmitter, HostListener, ElementRef, ViewChild, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

@Component({
  selector: 'app-searchable-select',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './searchable-select.component.html',
  styleUrl: './searchable-select.component.css',
})
export class SearchableSelectComponent implements OnInit, OnDestroy {
  @Input() items: string[] = [];
  @Input() loading = false;
  @Input() label = '';
  @Input() placeholder = 'Type to search...';
  @Input() allLabel = 'All';
  @Input() value = '';

  @Output() valueChange = new EventEmitter<string>();

  @ViewChild('searchInput') searchInput: ElementRef<HTMLInputElement> | null = null;

  private elementRef = inject(ElementRef);
  private destroy$ = new Subject<void>();

  isOpen = false;
  searchText = '';
  highlightedIndex = -1;

  private searchSubject = new Subject<string>();

  constructor() {
    this.searchSubject
      .pipe(debounceTime(200), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(() => {
        this.highlightedIndex = -1;
      });
  }

  ngOnInit(): void {
    this.searchText = this.value;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get displayText(): string {
    return this.value || this.allLabel;
  }

  get filteredItems(): string[] {
    if (!this.searchText) return this.items;
    const q = this.searchText.toLowerCase();
    return this.items.filter((item) => item.toLowerCase().includes(q));
  }

  toggle(): void {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.searchText = '';
      this.highlightedIndex = -1;
      setTimeout(() => this.searchInput?.nativeElement.focus());
    }
  }

  open(): void {
    this.isOpen = true;
    this.searchText = '';
    this.highlightedIndex = -1;
    setTimeout(() => this.searchInput?.nativeElement.focus());
  }

  close(): void {
    this.isOpen = false;
    this.searchText = this.value;
  }

  select(item: string): void {
    this.value = item;
    this.valueChange.emit(item);
    this.searchText = item;
    this.isOpen = false;
  }

  selectAll(): void {
    this.value = '';
    this.valueChange.emit('');
    this.searchText = '';
    this.isOpen = false;
  }

  onSearchInput(value: string): void {
    this.searchText = value;
    this.searchSubject.next(value);
    this.highlightedIndex = -1;
  }

  onKeydown(event: KeyboardEvent): void {
    if (!this.isOpen) return;

    const list = this.filteredItems;

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.highlightedIndex = Math.min(this.highlightedIndex + 1, list.length - 1);
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.highlightedIndex = Math.max(this.highlightedIndex - 1, 0);
        break;
      case 'Enter':
        event.preventDefault();
        if (this.highlightedIndex >= 0 && this.highlightedIndex < list.length) {
          this.select(list[this.highlightedIndex]);
        }
        break;
      case 'Escape':
        event.preventDefault();
        this.close();
        break;
    }
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.isOpen = false;
      this.searchText = this.value;
    }
  }
}
