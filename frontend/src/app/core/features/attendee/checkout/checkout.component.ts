import { Component, OnInit, ChangeDetectorRef, inject, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { take } from 'rxjs';
import { loadStripe, Stripe, StripeElements, StripeCardElement } from '@stripe/stripe-js';
import { EventService } from '../home/services/event.service';
import { PaymentService, PaymentIntentResponse } from '../home/services/payment.service';
import { EventDetailResponse } from '../home/models/event.models';
import { ToastService } from '../../../../shared/services/toast.service';
import { environment } from '../../../../../environments/environment';
import { formatTime } from '../../../../shared/utils/format-utils';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [RouterLink, DatePipe, CurrencyPipe],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css',
})
export class CheckoutComponent implements OnInit {
  @ViewChild('cardElement') cardElementRef!: ElementRef;

  private cdr = inject(ChangeDetectorRef);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private eventService = inject(EventService);
  private paymentService = inject(PaymentService);
  private toast = inject(ToastService);

  event: EventDetailResponse | null = null;
  readonly formatTime = formatTime;
  quantity = 1;
  loading = true;
  processing = false;
  error: string | null = null;

  private stripe: Stripe | null = null;
  private elements: StripeElements | null = null;
  private card: StripeCardElement | null = null;
  cardElementId = 'stripe-card-element';
  stripeReady = false;
  paymentIntent: PaymentIntentResponse | null = null;

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
        if (res.data) {
          this.event = res.data;
        }
        this.loading = false;
        this.cdr.detectChanges();
        setTimeout(() => this.initStripe(), 0);
      },
      error: () => {
        this.error = 'Failed to load event details.';
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  private async initStripe(): Promise<void> {
    if (this.stripeReady) return;

    const el = document.getElementById(this.cardElementId);
    if (!el) {
      this.error = 'Card element not found in DOM.';
      return;
    }

    try {
      this.stripe = await loadStripe(environment.stripePublishableKey);
      if (!this.stripe) {
        this.error = 'Failed to load payment system (Stripe.js did not initialize).';
        return;
      }

      this.elements = this.stripe.elements({ locale: 'en' });
      this.card = this.elements.create('card', {
        style: {
          base: {
            fontSize: '16px',
            color: '#1f2937',
            '::placeholder': { color: '#9ca3af' },
          },
        },
      });
      this.card.mount(`#${this.cardElementId}`);
      this.stripeReady = true;
      this.cdr.detectChanges();
    } catch (err) {
      this.error = `Stripe init error: ${err instanceof Error ? err.message : 'Unknown'}`;
      this.cdr.detectChanges();
    }
  }

  get totalPrice(): number {
    return this.event ? this.event.price * this.quantity : 0;
  }

  private readonly imageBaseUrl = '';

  getPosterStyle(url: string | null): string {
    const fullUrl = url ? `${environment.apiUrl.replace('/api', '')}/${url}` : '';
    return `url(${fullUrl})`;
  }

  async pay(): Promise<void> {
    if (!this.event || this.processing || !this.stripe || !this.card) return;

    this.processing = true;

    this.paymentService.createPaymentIntent({
      eventId: this.event.id,
      quantity: this.quantity,
    }).pipe(take(1)).subscribe({
      next: async (pi) => {
        this.paymentIntent = pi;

        const { error, paymentIntent } = await this.stripe!.confirmCardPayment(pi.clientSecret, {
          payment_method: { card: this.card! },
        });

        if (error) {
          this.processing = false;
          this.toast.error(error.message || 'Payment failed.', 'Error');
          this.cdr.detectChanges();
          return;
        }

        if (paymentIntent?.status === 'succeeded') {
          this.paymentService.confirmPayment({ paymentIntentId: pi.paymentIntentId })
            .pipe(take(1))
            .subscribe({
              next: (booking) => {
                this.processing = false;
                this.router.navigate(['/attendee/payment-success'], {
                  queryParams: { code: booking.uniqueCode },
                });
              },
              error: () => {
                this.processing = false;
                this.toast.error('Failed to finalize booking.', 'Error');
                this.cdr.detectChanges();
              },
            });
        } else {
          this.processing = false;
          this.toast.error('Payment was not completed.', 'Error');
          this.cdr.detectChanges();
        }
      },
      error: () => {
        this.processing = false;
        this.toast.error('Failed to initiate payment.', 'Error');
        this.cdr.detectChanges();
      },
    });
  }
}
