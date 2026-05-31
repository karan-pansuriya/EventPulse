import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GridComponent, GridColumn } from '../../../../shared/components/grid/grid.component';
import { OrganizerEventService } from '../services/organizer-event.service';
import { EventAttendee } from '../models/attendee.models';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-organizer-attendees',
  standalone: true,
  imports: [CommonModule, GridComponent],
  templateUrl: './attendees.component.html',
})
export class AttendeesComponent implements OnInit {
  private eventService = inject(OrganizerEventService);
  private toastService = inject(ToastService);
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

    this.eventService.getAttendees().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const data = res.data;
          this.totalRecords = data.length;
          this.gridData = data.map((a: EventAttendee, i: number) => ({
            index: i + 1,
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
        this.toastService.error('Failed to load attendees.');
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  trackByBookingId(index: number, item: Record<string, unknown>): unknown {
    return item['bookingId'];
  }
}
