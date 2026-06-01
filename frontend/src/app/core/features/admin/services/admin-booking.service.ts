import { inject, Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseHttpService } from '../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../shared/models/api-response.model';
import { PagedResult } from '../../../../shared/models/paged-result.model';

export interface AdminBookingResponse {
  id: number;
  userId: number;
  customerName: string;
  customerEmail: string;
  eventId: number;
  eventTitle: string;
  venueName: string;
  quantity: number;
  totalAmount: number;
  paymentStatus: string;
  bookingStatus: string;
  uniqueCode: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class AdminBookingService {
  private http = inject(BaseHttpService);

  getAllBookings(pageNumber: number, pageSize: number): Observable<ApiResponse<PagedResult<AdminBookingResponse>>> {
    const params = new HttpParams()
      .set('PageNumber', pageNumber)
      .set('PageSize', pageSize);

    return this.http.get<PagedResult<AdminBookingResponse>>('bookings', { params });
  }
}
