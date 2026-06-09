import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { NgZone } from '@angular/core';
import {
  GridComponent,
  GridColumn,
} from '../../../../shared/components/grid/grid.component';
import { AdminEventService } from '../layout/admin-layout/services/admin-event.service';
import { EventListResponse } from '../layout/admin-layout/models/event.models';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';

@Component({
  selector: 'app-admin-events',
  standalone: true,
  imports: [CommonModule, FormsModule, GridComponent, ConfirmationModalComponent],
  templateUrl: './admin-events.component.html',
  styleUrl: './admin-events.component.css',
})
export class AdminEventsComponent implements OnInit {
  private adminEventService = inject(AdminEventService);
  private cdr = inject(ChangeDetectorRef);
  private zone = inject(NgZone);
  private router = inject(Router);
  private destroy$ = new Subject<void>();

  events: EventListResponse[] = [];
  totalRecords = 0;
  currentPage = 1;
  pageSize = 10;
  loading = false;
  error: string | null = null;

  confirmEvent: EventListResponse | null = null;

  columns: GridColumn[] = [
    { header: 'ID', field: 'id', width: '60px' },
    { header: 'Title', field: 'title', type: 'truncate', width: '200px' },
    { header: 'Category', field: 'categoryName' },
    { header: 'Venue', field: 'venueName', type: 'truncate' },
    { header: 'Date', field: 'eventDate', type: 'date' },
    { header: 'Price', field: 'price', type: 'currency' },
    { header: 'Verified', field: 'isVerified', type: 'toggle' },
    { header: 'Edit', field: 'id', type: 'edit', width: '40px' },
    { header: 'Delete', field: 'id', type: 'delete', width: '40px' },
  ];

  ngOnInit(): void {
    this.loadEvents();
  }

  loadEvents(): void {
    this.loading = true;
    this.error = null;

    this.adminEventService
      .getAllEvents(this.currentPage, this.pageSize, 'EventDate', 'desc')
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

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadEvents();
  }

  createEvent(): void {
    this.router.navigate(['/admin/events/create']);
  }

  onEdit(event: EventListResponse): void {
    this.router.navigate(['/admin/events/edit', event.id]);
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

  promptDelete(event: EventListResponse): void {
    this.confirmEvent = event;
  }

  onDeleteConfirmed(): void {
    if (!this.confirmEvent) return;
    const event = this.confirmEvent;
    this.confirmEvent = null;

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

  onDeleteCancelled(): void {
    this.confirmEvent = null;
  }
}
