import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { BaseHttpService } from '../../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../../shared/models/api-response.model';
import { BookingResponse } from './booking.service';

export interface CreatePaymentIntentRequest {
  eventId: number;
  quantity: number;
}

export interface PaymentIntentResponse {
  clientSecret: string;
  paymentIntentId: string;
  bookingId: number;
}

export interface ConfirmPaymentRequest {
  paymentIntentId: string;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private http = inject(BaseHttpService);

  createPaymentIntent(data: CreatePaymentIntentRequest): Observable<PaymentIntentResponse> {
    return this.http.post<PaymentIntentResponse>('payments/create-intent', data).pipe(
      map((res) => res.data!),
    );
  }

  confirmPayment(data: ConfirmPaymentRequest): Observable<BookingResponse> {
    return this.http.post<BookingResponse>('payments/confirm', data).pipe(
      map((res) => res.data!),
    );
  }

  getPaymentStatus(paymentIntentId: string): Observable<{ status: string; bookingId: number; uniqueCode: string }> {
    return this.http.get<{ status: string; bookingId: number; uniqueCode: string }>(`payments/status/${paymentIntentId}`).pipe(
      map((res) => res.data!),
    );
  }
}
