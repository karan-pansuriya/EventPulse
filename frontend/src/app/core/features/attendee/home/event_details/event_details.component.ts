import { Component, OnInit, inject, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { EventService } from '../services/event.service';
import { EventDetailResponse } from '../models/event.models';
import { environment } from '../../../../../../environments/environment';
import { SignalRService } from '../../../../../shared/services/signalr.service';

@Component({
  selector: 'app-event-details',
  standalone: true,
  imports: [RouterLink, DatePipe, CurrencyPipe],
  templateUrl: './event_details.component.html',
  styleUrl: './event_details.component.css',
})
export class EventDetailsComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private eventService = inject(EventService);
  private signalR = inject(SignalRService);
  private cdr = inject(ChangeDetectorRef);
  private destroy$ = new Subject<void>();

  private readonly imageBaseUrl = environment.apiUrl.replace('/api', '');
  private eventId = 0;

  event: EventDetailResponse | null = null;
  loading = true;
  error: string | null = null;
  selectedQuantity = 1;
  signalRConnected = false;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.error = 'Invalid event ID.';
      this.loading = false;
      return;
    }

    this.eventId = id;
    this.fetchEvent(id);
    this.listenForSeatUpdates();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.signalR.leaveEventGroup(this.eventId);
  }

  private async listenForSeatUpdates(): Promise<void> {
    await this.signalR.joinEventGroup(this.eventId);
    this.signalRConnected = true;
    this.cdr.detectChanges();

    this.signalR.seatUpdates$
      .pipe(takeUntil(this.destroy$))
      .subscribe((update) => {
        if (update.eventId === this.eventId && this.event) {
          this.event.totalSeats = update.remainingSeats;
          this.cdr.detectChanges();
        }
      });
  }

  private fetchEvent(id: number): void {
    this.loading = true;
    this.error = null;

    this.eventService.getEventById(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.data) {
            this.event = res.data;
          }
          this.loading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.error = 'Failed to load event details. Please try again.';
          this.loading = false;
          this.cdr.detectChanges();
        },
      });
  }

  getPosterUrl(url: string | null): string {
    return url ? `${this.imageBaseUrl}/${url}` : '';
  }

  retry(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) this.fetchEvent(id);
  }

  get availableSeats(): number {
    return this.event ? this.event.totalSeats : 0;
  }

  bookNow(): void {
    if (!this.event) return;
    this.router.navigate(['/attendee/checkout', this.event.id], {
      queryParams: { qty: this.selectedQuantity },
    });
  }
}
