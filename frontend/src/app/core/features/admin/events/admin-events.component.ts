import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { NgZone } from '@angular/core';
import { GridComponent, GridColumn, GridActionItem } from '../../../../shared/components/grid/grid.component';
import { AdminEventService } from '../services/admin-event.service';
import { EventListResponse } from '../../attendee/home/models/event.models';

@Component({
  selector: 'app-admin-events',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, GridComponent],
  template: `
    <div class="admin-page">
      <div class="page-header">
        <h1>All Events</h1>
        <a class="btn btn-primary create-btn" routerLink="/organizer/events/create">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
            <line x1="12" y1="5" x2="12" y2="19" />
            <line x1="5" y1="12" x2="19" y2="12" />
          </svg>
          Create Event
        </a>
      </div>

      <app-grid
        [columns]="columns"
        [data]="events"
        [totalRecords]="totalRecords"
        [currentPage]="currentPage"
        [pageSize]="pageSize"
        [trackByField]="'id'"
        [isLoading]="loading"
        [errorMessage]="error"
        [actionItems]="getRowActionItems"
        (pageChange)="onPageChange($event)"
        (toggle)="onToggleVerify($event)"
        (delete)="onDelete($event)"
      />
    </div>
  `,
  styles: [`
    .admin-page {
      max-width: 1400px;
      margin: 0 auto;
    }
    .page-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 24px;
    }
    .page-header h1 {
      font-size: 1.5rem;
      font-weight: 700;
      color: #111827;
      margin: 0;
    }
    .btn-primary {
      background: #7c3aed;
      border-color: #7c3aed;
    }
    .btn-primary:hover {
      background: #6d28d9;
      border-color: #6d28d9;
    }
    .create-btn {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      white-space: nowrap;
    }
    @media (max-width: 575.98px) {
      .page-header {
        flex-direction: row;
        flex-wrap: wrap;
        gap: 8px;
      }
      .page-header h1 {
        font-size: 1.25rem;
      }
      .create-btn {
        padding: 6px 14px;
        font-size: 0.875rem;
      }
    }
  `]
})
export class AdminEventsComponent implements OnInit {
  private adminEventService = inject(AdminEventService);
  private cdr = inject(ChangeDetectorRef);
  private zone = inject(NgZone);
  private destroy$ = new Subject<void>();

  events: EventListResponse[] = [];
  totalRecords = 0;
  currentPage = 1;
  pageSize = 10;
  loading = false;
  error: string | null = null;

  columns: GridColumn[] = [
    { header: 'ID', field: 'id', width: '60px' },
    { header: 'Title', field: 'title', type: 'truncate', width: '200px' },
    { header: 'Category', field: 'categoryName' },
    { header: 'Venue', field: 'venueName', type: 'truncate' },
    { header: 'Date', field: 'eventDate', type: 'date' },
    { header: 'Price', field: 'price', type: 'currency' },
    { header: 'Verified', field: 'isVerified', type: 'toggle' },
    { header: 'Actions', field: 'id', type: 'action' },
  ];

  getRowActionItems: (row: EventListResponse) => GridActionItem[] = () => [
    { label: 'Delete', icon: 'bi-trash', emit: 'delete' },
  ];

  ngOnInit(): void {
    this.loadEvents();
  }

  loadEvents(): void {
    this.loading = true;
    this.error = null;

    this.adminEventService
      .getAllEvents(this.currentPage, this.pageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.zone.run(() => {
            if (res.data) {
              this.events = res.data.items;
              this.totalRecords = res.data.totalCount;
            }
            this.loading = false;
            this.cdr.detectChanges();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.error = 'Failed to load events.';
            this.loading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadEvents();
  }

  onToggleVerify(event: EventListResponse): void {
    this.adminEventService
      .toggleVerification(event.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.zone.run(() => {
          this.loadEvents();
          this.cdr.detectChanges();
        });
      });
  }

  onDelete(event: EventListResponse): void {
    if (!confirm('Are you sure you want to delete this event?')) return;

    this.adminEventService
      .deleteEvent(event.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.zone.run(() => {
          this.loadEvents();
          this.cdr.detectChanges();
        });
      });
  }
}
