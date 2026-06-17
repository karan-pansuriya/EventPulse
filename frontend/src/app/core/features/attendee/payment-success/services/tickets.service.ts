import { Injectable, inject } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { BaseHttpService } from '../../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../../shared/models/api-response.model';
import { PagedResult } from '../../../../../shared/models/paged-result.model';
import { MyTicketResponse } from '../models/ticket.model';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private http = inject(BaseHttpService);
  private apiUrl = environment.apiUrl;

  getMyTickets(bookingId: number): Observable<ApiResponse<MyTicketResponse[]>> {
    return this.http.get<MyTicketResponse[]>(`tickets/my-tickets/${bookingId}`);
  }

  getAllMyTickets(pageNumber: number, pageSize: number): Observable<ApiResponse<PagedResult<MyTicketResponse>>> {
    const params = new HttpParams().set('PageNumber', pageNumber).set('PageSize', pageSize);
    return this.http.get<PagedResult<MyTicketResponse>>('tickets/my-tickets', { params });
  }

  getDownloadUrl(ticketId: number): string {
    return `${this.apiUrl}/tickets/download/${ticketId}`;
  }
}
