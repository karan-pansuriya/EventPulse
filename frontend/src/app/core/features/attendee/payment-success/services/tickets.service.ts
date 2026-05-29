import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { ApiResponse, MyTicketResponse } from '../models/ticket.model';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private http   = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/tickets`;

  /**
   * GET /api/tickets/my-tickets/{bookingId}
   * Returns a specific booking (with nested tickets) for the logged-in customer.
   */
  getMyTickets(bookingId: number): Observable<ApiResponse<MyTicketResponse[]>> {
    return this.http.get<ApiResponse<MyTicketResponse[]>>(`${this.apiUrl}/my-tickets/${bookingId}`);
  }

  /**
   * GET /api/tickets/my-tickets
   * Returns ALL bookings (with nested tickets) for the logged-in customer,
   * sorted by CreatedAt descending.
   */
  getAllMyTickets(): Observable<ApiResponse<MyTicketResponse[]>> {
    return this.http.get<ApiResponse<MyTicketResponse[]>>(`${this.apiUrl}/my-tickets`);
  }

  /**
   * GET /api/tickets/download/{ticketId}
   * Returns a direct URL for downloading the ticket PDF.
   * Used as an <a href> target — no Observable needed.
   */
  getDownloadUrl(ticketId: number): string {
    return `${this.apiUrl}/download/${ticketId}`;
  }
}