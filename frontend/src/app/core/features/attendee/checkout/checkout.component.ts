import { Component, OnInit, ChangeDetectorRef, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { take } from 'rxjs';
import { loadStripe, Stripe, StripeElements, StripeCardNumberElement, StripeCardExpiryElement, StripeCardCvcElement } from '@stripe/stripe-js';
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
  private cardNumber: StripeCardNumberElement | null = null;
  private cardExpiry: StripeCardExpiryElement | null = null;
  private cardCvc: StripeCardCvcElement | null = null;
  stripeReady = false;
  paymentIntent: PaymentIntentResponse | null = null;

  cardNumberError = '';
  cardExpiryError = '';
  cardCvcError = '';
  private cardNumberComplete = false;
  private cardExpiryComplete = false;
  private cardCvcComplete = false;

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

    const numberEl = document.getElementById('stripe-card-number');
    const expiryEl = document.getElementById('stripe-card-expiry');
    const cvcEl = document.getElementById('stripe-card-cvc');

    if (!numberEl || !expiryEl || !cvcEl) {
      this.error = 'Card elements not found in DOM.';
      return;
    }

    try {
      this.stripe = await loadStripe(environment.stripePublishableKey);
      if (!this.stripe) {
        this.error = 'Failed to load payment system (Stripe.js did not initialize).';
        return;
      }

      this.elements = this.stripe.elements({ locale: 'en' });

      const style = {
        base: {
          fontSize: '16px',
          color: '#1f2937',
          '::placeholder': { color: '#9ca3af' },
        },
        invalid: { color: '#dc2626' },
      };

      this.cardNumber = this.elements.create('cardNumber', { style, placeholder: '1234 5678 9012 3456' });
      this.cardNumber.mount('#stripe-card-number');
      this.cardNumber.on('change', (e) => {
        this.cardNumberError = e.error?.message ?? '';
        this.cardNumberComplete = e.complete;
        this.cdr.detectChanges();
      });

      this.cardExpiry = this.elements.create('cardExpiry', { style, placeholder: 'MM / YY' });
      this.cardExpiry.mount('#stripe-card-expiry');
      this.cardExpiry.on('change', (e) => {
        this.cardExpiryError = e.error?.message ?? '';
        this.cardExpiryComplete = e.complete;
        this.cdr.detectChanges();
      });

      this.cardCvc = this.elements.create('cardCvc', { style, placeholder: '123' });
      this.cardCvc.mount('#stripe-card-cvc');
      this.cardCvc.on('change', (e) => {
        this.cardCvcError = e.error?.message ?? '';
        this.cardCvcComplete = e.complete;
        this.cdr.detectChanges();
      });

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

  private validateFields(): boolean {
    if (!this.cardNumberComplete) this.cardNumberError = 'Card number is incomplete.';
    if (!this.cardExpiryComplete) this.cardExpiryError = 'Expiry date is incomplete.';
    if (!this.cardCvcComplete) this.cardCvcError = 'CVC is incomplete.';

    const isValid = this.cardNumberComplete && this.cardExpiryComplete && this.cardCvcComplete;

    if (!isValid) {
      this.cdr.detectChanges();
      return false;
    }

    return true;
  }

  async pay(): Promise<void> {
    if (!this.event || this.processing || !this.stripe || !this.cardNumber) return;

    if (!this.validateFields()) return;

    this.processing = true;

    this.paymentService.createPaymentIntent({
      eventId: this.event.id,
      quantity: this.quantity,
    }).pipe(take(1)).subscribe({
      next: async (pi) => {
        this.paymentIntent = pi;

        const { error, paymentIntent } = await this.stripe!.confirmCardPayment(pi.clientSecret, {
          payment_method: { card: this.cardNumber! },
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
                  queryParams: { code: booking.id },
                });
              },
              error: () => {
                this.processing = false;
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
        this.cdr.detectChanges();
      },
    });
  }
}
