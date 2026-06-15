import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TicketService } from '../payment-success/services/tickets.service';
import { MyTicketResponse } from '../payment-success/models/ticket.model';

@Component({
  selector: 'app-my-tickets',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './my-tickets.component.html',
  styleUrl: './my-tickets.component.css',
})
export class MyTicketsComponent implements OnInit {
  private ticketService = inject(TicketService);
  private cdr = inject(ChangeDetectorRef);

  bookings: MyTicketResponse[] = [];
  isLoading = true;
  hasError = false;
  activeTab: 'upcoming' | 'completed' = 'upcoming';

  get filteredBookings(): MyTicketResponse[] {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return this.bookings.filter((b) => {
      const allUsed = b.tickets.length > 0 && b.tickets.every((t) => t.isUsed);
      const eventDate = b.eventDate ? new Date(b.eventDate) : null;
      const eventPassed = eventDate !== null && eventDate < today;
      if (this.activeTab === 'upcoming') {
        return !eventPassed && !allUsed;
      } else {
        return eventPassed || allUsed;
      }
    });
  }

  ngOnInit(): void {
    this.fetchTickets();
  }

  private fetchTickets(): void {
    this.ticketService.getAllMyTickets().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.bookings = res.data;
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

  setActiveTab(tab: 'upcoming' | 'completed'): void {
    this.activeTab = tab;
  }

  getDownloadUrl(ticketId: number): string {
    return this.ticketService.getDownloadUrl(ticketId);
  }

  formatDate(dateStr: string | null): string {
    if (!dateStr) return '';
    return new Date(dateStr).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  }

  formatDateTime(dateStr: string): string {
    const d = new Date(dateStr);
    return d.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }
}
