import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpService } from '../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../shared/models/api-response.model';

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

  getAllBookings(): Observable<ApiResponse<AdminBookingResponse[]>> {
    return this.http.get<AdminBookingResponse[]>('bookings/admin/all');
  }
}
