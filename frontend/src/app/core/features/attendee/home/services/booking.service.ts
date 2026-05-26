import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpService } from '../../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../../shared/models/api-response.model';

export interface CreateBookingRequest {
  eventId: number;
  quantity: number;
}

export interface BookingResponse {
  id: number;
  userId: number;
  eventId: number;
  eventTitle: string;
  uniqueCode: string;
  quantity: number;
  pricePerTicket: number;
  totalAmount: number;
  paymentStatus: string;
  bookingStatus: string;
  paymentRef: string | null;
  ticketCodes: string[];
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class BookingService {
  private http = inject(BaseHttpService);

  createBooking(data: CreateBookingRequest): Observable<ApiResponse<BookingResponse>> {
    return this.http.post<BookingResponse>('bookings', data);
  }
}
