import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { ApiResponse, MyTicketResponse } from '../models/ticket.model';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private http   = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/tickets`;

  getMyTickets(bookingId: number): Observable<ApiResponse<MyTicketResponse[]>> {
    return this.http.get<ApiResponse<MyTicketResponse[]>>(`${this.apiUrl}/my-tickets/${bookingId}`);
  }

  getAllMyTickets(): Observable<ApiResponse<MyTicketResponse[]>> {
    return this.http.get<ApiResponse<MyTicketResponse[]>>(`${this.apiUrl}/my-tickets`);
  }

  getDownloadUrl(ticketId: number): string {
    return `${this.apiUrl}/download/${ticketId}`;
  }
}
