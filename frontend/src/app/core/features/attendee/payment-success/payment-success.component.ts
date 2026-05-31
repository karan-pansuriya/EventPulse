import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TicketService } from './services/tickets.service';
import { MyTicketResponse } from './models/ticket.model';

@Component({
  selector: 'app-payment-success',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './payment-success.component.html',
  styleUrl: './payment-success.component.css',
})
export class PaymentSuccessComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private ticketService = inject(TicketService);
  private cdr = inject(ChangeDetectorRef);

  bookingId: string | null = null;
  booking: MyTicketResponse | null = null;
  isLoading = true;
  hasError = false;

  ngOnInit(): void {
    this.bookingId = this.route.snapshot.queryParamMap.get('code');

    if (this.bookingId) {
      this.fetchBooking(Number.parseInt(this.bookingId));
    } else {
      this.isLoading = false;
    }
  }

  private fetchBooking(bookingId: number): void {
    this.ticketService.getMyTickets(bookingId).subscribe({
      next: (res) => {
        if (res.success && res.data?.length) {
          // Match the booking that was just paid for
          this.booking = res.data.find((b) => b.bookingId === bookingId) || null;
        }
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.hasError = true;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  getDownloadUrl(ticketId: number): string {
    return this.ticketService.getDownloadUrl(ticketId);
  }

  /** Formats "2025-07-20" → "July 20, 2025" */
  formatDate(dateStr: string | null): string {
    if (!dateStr) return '';
    return new Date(dateStr).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  }
}
