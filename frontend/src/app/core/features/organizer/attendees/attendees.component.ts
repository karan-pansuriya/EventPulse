import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GridComponent, GridColumn } from '../../../../shared/components/grid/grid.component';
import { OrganizerEventService } from '../layout/services/organizer-event.service';
import { EventAttendee } from '../layout/models/attendee.models';
import { PagedResult } from '../../../../shared/models/paged-result.model';

@Component({
  selector: 'app-organizer-attendees',
  standalone: true,
  imports: [CommonModule, GridComponent],
  templateUrl: './attendees.component.html',
})
export class AttendeesComponent implements OnInit {
  private eventService = inject(OrganizerEventService);
  private cdr = inject(ChangeDetectorRef);

  columns: GridColumn[] = [
    { header: '#', field: 'index' },
    { header: 'Customer', field: 'customerName' },
    { header: 'Email', field: 'customerEmail' },
    { header: 'Phone', field: 'customerPhone' },
    { header: 'Event', field: 'eventTitle' },
    { header: 'Qty', field: 'quantity', width: '60px' },
    { header: 'Amount', field: 'totalAmount', type: 'currency' },
    { header: 'Payment', field: 'paymentStatus' },
    { header: 'Booked', field: 'bookedAt', type: 'date' },
  ];

  gridData: Record<string, unknown>[] = [];
  totalRecords = 0;
  currentPage = 1;
  pageSize = 10;
  isLoading = true;
  errorMessage: string | null = null;

  ngOnInit(): void {
    this.loadAttendees();
  }

  private loadAttendees(): void {
    this.isLoading = true;
    this.errorMessage = null;
    this.cdr.detectChanges();

    this.eventService.getAttendees(this.currentPage, this.pageSize).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const paged = res.data as PagedResult<EventAttendee>;
          this.totalRecords = paged.totalCount;
          this.gridData = paged.items.map((a: EventAttendee, i: number) => ({
            index: (this.currentPage - 1) * this.pageSize + i + 1,
            bookingId: a.bookingId,
            customerName: a.customerName,
            customerEmail: a.customerEmail,
            customerPhone: a.customerPhone || '-',
            eventTitle: a.eventTitle,
            quantity: a.quantity,
            totalAmount: a.totalAmount,
            paymentStatus: a.paymentStatus,
            bookedAt: a.bookedAt,
          }));
        } else {
          this.errorMessage = res.message || 'Failed to load attendees.';
        }
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadAttendees();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadAttendees();
  }

  trackByBookingId(index: number, item: Record<string, unknown>): unknown {
    return item['bookingId'];
  }
}
