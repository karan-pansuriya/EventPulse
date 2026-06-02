import { ChangeDetectorRef, Component, OnInit, inject, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { Subject, debounceTime, takeUntil, Subscription } from 'rxjs';
import { EventService } from './services/event.service';
import { CategoryService } from './services/category.service';
import { CityService } from './services/city.service';
import { EventListResponse, EventFilterRequest } from './models/event.models';
import { Category } from './models/category.models';
import { PagedResult } from '../../../../shared/models/paged-result.model';
import { ApiResponse } from '../../../../shared/models/api-response.model';
import { environment } from '../../../../../environments/environment';
import { EventCardComponent } from '../../../../shared/components/event-card/event-card.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [FormsModule, EventCardComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent implements OnInit, OnDestroy {
  private eventService = inject(EventService);
  private categoryService = inject(CategoryService);
  private cityService = inject(CityService);
  private cdr = inject(ChangeDetectorRef);
  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<string>();

  private eventSub: Subscription | null = null;

  result: PagedResult<EventListResponse> = { items: [], totalCount: 0 };
  loading = false;
  error: string | null = null;

  searchQuery = '';
  selectedCategory = '';
  selectedCity = '';
  dateFrom = '';
  dateTo = '';
  currentPage = 1;

  categories: Category[] = [];
  categoriesLoading = true;

  cities: string[] = [];
  citiesLoading = false;
  citySearchText = '';

  showCategoryDropdown = false;
  showCityDropdown = false;

  ngOnInit(): void {
    this.searchSubject.pipe(debounceTime(400), takeUntil(this.destroy$)).subscribe(() => {
      this.currentPage = 1;
      this.loadEvents();
    });

    this.categoryService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.data) this.categories = res.data;
          this.categoriesLoading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.categoriesLoading = false;
          this.cdr.detectChanges();
        },
      });

    this.loadCities();
    this.loadEvents();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.eventSub?.unsubscribe();
  }

  readonly imageBaseUrl = environment.apiUrl.replace('/api', '');

  onSearchInput(value: string): void {
    this.searchQuery = value;
    this.searchSubject.next(value);
  }

  // ── Category dropdown ──────────────────────────────────────
  get selectedCategoryName(): string {
    if (!this.selectedCategory) return 'All Categories';
    const cat = this.categories.find((c) => c.id === Number(this.selectedCategory));
    return cat ? cat.name : 'All Categories';
  }

  isCategorySelected(id: number): boolean {
    return this.selectedCategory === String(id);
  }

  catId(id: number): string {
    return String(id);
  }

  selectCategory(value: string): void {
    this.selectedCategory = value;
    this.showCategoryDropdown = false;
    this.applyFilters();
  }

  toggleCategoryDropdown(): void {
    this.showCategoryDropdown = !this.showCategoryDropdown;
    this.showCityDropdown = false;
  }

  // ── City dropdown ──────────────────────────────────────────
  get filteredCities(): string[] {
    if (!this.citySearchText) return this.cities;
    const q = this.citySearchText.toLowerCase();
    return this.cities.filter((c) => c.toLowerCase().includes(q));
  }

  toggleCityDropdown(): void {
    this.showCityDropdown = !this.showCityDropdown;
    this.showCategoryDropdown = false;
    if (this.showCityDropdown) this.citySearchText = '';
  }

  onCitySearchInput(value: string): void {
    this.citySearchText = value;
  }

  selectCityOption(city: string): void {
    this.selectedCity = city;
    this.showCityDropdown = false;
    this.citySearchText = '';
    this.applyFilters();
  }

  // ── Shared ─────────────────────────────────────────────────
  onBackdropClick(): void {
    this.showCategoryDropdown = false;
    this.showCityDropdown = false;
  }

  loadCities(): void {
    if (this.citiesLoading) return;
    this.citiesLoading = true;
    this.cityService
      .getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.data) this.cities = res.data;
          this.citiesLoading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.citiesLoading = false;
          this.cdr.detectChanges();
        },
      });
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadEvents();
  }

  clearFilters(): void {
    this.searchQuery = '';
    this.selectedCategory = '';
    this.selectedCity = '';
    this.dateFrom = '';
    this.dateTo = '';
    this.currentPage = 1;
    this.loadEvents();
  }

  retry(): void {
    this.loadEvents();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.currentPage) return;
    this.currentPage = page;
    this.loadEvents();
  }

  readonly pageSize = 12;

  get currentPageCount(): number {
    return this.result.items.length;
  }

  get totalPages(): number {
    return Math.ceil(this.result.totalCount / this.pageSize);
  }

  get pages(): number[] {
    const total = this.totalPages;
    const current = this.currentPage;
    const pages: number[] = [];
    const start = Math.max(1, current - 2);
    const end = Math.min(total, current + 2);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  }

  private loadEvents(): void {
    this.eventSub?.unsubscribe();
    this.loading = true;
    this.error = null;

    this.eventSub = this.eventService
      .getEvents(this.buildFilters())
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res: ApiResponse<PagedResult<EventListResponse>>) => {
          if (res.data) this.result = res.data;
          this.loading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.error = 'Failed to load events. Please try again.';
          this.loading = false;
          this.cdr.detectChanges();
        },
      });
  }

  private buildFilters(): EventFilterRequest {
    return {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortBy: 'eventDate',
      sortDirection: 'asc',
      search: this.searchQuery || undefined,
      categoryId: this.selectedCategory ? Number(this.selectedCategory) : undefined,
      city: this.selectedCity || undefined,
      dateFrom: this.dateFrom || undefined,
      dateTo: this.dateTo || undefined,
    };
  }
}