import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { GridComponent, GridColumn } from '../../../../shared/components/grid/grid.component';
import { AdminBookingService, AdminBookingResponse } from '../services/admin-booking.service';

@Component({
  selector: 'app-admin-bookings',
  standalone: true,
  imports: [CommonModule, GridComponent],
  template: `
    <div class="admin-page">
      <div class="page-header">
        <h1>All Bookings</h1>
      </div>

      <app-grid
        [columns]="columns"
        [data]="bookings"
        [totalRecords]="0"
        [trackByField]="'id'"
        [isLoading]="loading"
        [errorMessage]="error"
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
    @media (max-width: 575.98px) {
      .page-header {
        flex-direction: row;
        flex-wrap: wrap;
        gap: 8px;
      }
      .page-header h1 {
        font-size: 1.25rem;
      }
    }
  `],
})
export class BookingsComponent implements OnInit, OnDestroy {
  private bookingService = inject(AdminBookingService);
  private cdr = inject(ChangeDetectorRef);
  private zone = inject(NgZone);
  private destroy$ = new Subject<void>();

  bookings: AdminBookingResponse[] = [];
  loading = false;
  error: string | null = null;

  columns: GridColumn[] = [
    { header: 'ID', field: 'id', width: '60px' },
    { header: 'Customer', field: 'customerName' },
    { header: 'Email', field: 'customerEmail' },
    { header: 'Event', field: 'eventTitle', type: 'truncate', width: '200px' },
    { header: 'Venue', field: 'venueName', type: 'truncate' },
    { header: 'Qty', field: 'quantity', width: '60px' },
    { header: 'Total', field: 'totalAmount', type: 'currency' },
    { header: 'Status', field: 'paymentStatus' },
    { header: 'Booked', field: 'createdAt', type: 'date' },
  ];

  ngOnInit(): void {
    this.loadBookings();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadBookings(): void {
    this.loading = true;
    this.error = null;

    this.bookingService
      .getAllBookings()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.zone.run(() => {
            if (res.data) this.bookings = res.data;
            this.loading = false;
            this.cdr.detectChanges();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.error = 'Failed to load bookings.';
            this.loading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }
}
