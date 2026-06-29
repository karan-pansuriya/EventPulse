import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { GridComponent, GridColumn } from '../../../../shared/components/grid/grid.component';
import { AdminBookingService } from '../layout/admin-layout/services/admin-booking.service';
import { AdminBookingResponse } from '../layout/admin-layout/models/admin-booking.model';

@Component({
  selector: 'app-admin-bookings',
  standalone: true,
  imports: [CommonModule, GridComponent],
  templateUrl: './bookings.component.html',
  styleUrl: './bookings.component.css',
})
export class BookingsComponent implements OnInit, OnDestroy {
  private bookingService = inject(AdminBookingService);
  private cdr = inject(ChangeDetectorRef);
  private zone = inject(NgZone);
  private destroy$ = new Subject<void>();

  bookings: AdminBookingResponse[] = [];
  totalRecords = 0;
  currentPage = 1;
  pageSize = 10;
  loading = false;
  error: string | null = null;

  columns: GridColumn[] = [
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
      .getAllBookings(this.currentPage, this.pageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.zone.run(() => {
            if (res.data) {
              this.bookings = res.data.items;
              this.totalRecords = res.data.totalCount;
            }
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

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadBookings();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadBookings();
  }
}
