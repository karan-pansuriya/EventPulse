import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { EventService } from '../home/services/event.service';
import { BookingService, CreateBookingRequest } from '../home/services/booking.service';
import { EventDetailResponse } from '../home/models/event.models';
import { ToastService } from '../../../../shared/services/toast.service';
import { environment } from '../../../../../environments/environment';
import { take } from 'rxjs';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [RouterLink, DatePipe, CurrencyPipe],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css',
})
export class CheckoutComponent implements OnInit {
  private cdr = inject(ChangeDetectorRef);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private eventService = inject(EventService);
  private bookingService = inject(BookingService);
  private toast = inject(ToastService);

  event: EventDetailResponse | null = null;
  quantity = 1;
  loading = true;
  processing = false;
  error: string | null = null;

  cardNumber = '';
  cardExpiry = '';
  cardCvv = '';

  ngOnInit(): void {
    const eventId = Number(this.route.snapshot.paramMap.get('eventId'));
    if (!eventId) {
      this.error = 'Invalid event.';
      this.loading = false;
      return;
    }

    const q = Number(this.route.snapshot.queryParamMap.get('qty'));
    if (q > 0) this.quantity = q;

    this.eventService.getEventById(eventId).pipe(take(1)).subscribe({
      next: (res) => {
        if (res.data) this.event = res.data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.error = 'Failed to load event details.';
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  get totalPrice(): number {
    return this.event ? this.event.price * this.quantity : 0;
  }

  onCardNumberInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    let val = input.value.replace(/\D/g, '').slice(0, 16);
    this.cardNumber = val.replace(/(\d{4})(?=\d)/g, '$1 ');
  }

  onExpiryInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    let val = input.value.replace(/\D/g, '').slice(0, 4);
    if (val.length > 2) val = val.slice(0, 2) + '/' + val.slice(2);
    this.cardExpiry = val;
  }

  onCvvInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.cardCvv = input.value.replace(/\D/g, '').slice(0, 3);
  }

  private readonly imageBaseUrl = '';

  getPosterUrl(url: string | null): string {
    return url ? `${environment.apiUrl.replace('/api', '')}/${url}` : '';
  }

  pay(): void {
    if (!this.event || this.processing) return;

    if (this.cardNumber.replace(/\s/g, '').length < 13) {
      this.toast.error('Please enter a valid card number.', 'Invalid Card');
      return;
    }
    if (this.cardExpiry.length < 5) {
      this.toast.error('Please enter a valid expiry date (MM/YY).', 'Invalid Expiry');
      return;
    }
    if (this.cardCvv.length < 3) {
      this.toast.error('Please enter a valid CVV.', 'Invalid CVV');
      return;
    }

    this.processing = true;

    const data: CreateBookingRequest = {
      eventId: this.event.id,
      quantity: this.quantity,
    };

    this.bookingService.createBooking(data).pipe(take(1)).subscribe({
      next: (res) => {
        this.processing = false;
        if (res.data) {
          this.router.navigate(['/attendee/payment-success'], {
            queryParams: { code: res.data.uniqueCode },
          });
        }
      },
      error: () => {
        this.processing = false;
        this.cdr.detectChanges();
        this.toast.error('Payment failed. Please try again.', 'Error');
      },
    });
  }
}
