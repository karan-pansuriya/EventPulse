import { Component, Input, Output, EventEmitter, booleanAttribute } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { EventListResponse } from '../../../core/features/attendee/home/models/event.models';
import { formatTime } from '../../utils/format-utils';

@Component({
  selector: 'app-event-card',
  standalone: true,
  imports: [RouterLink, DatePipe, CurrencyPipe],
  templateUrl: './event-card.component.html',
  styleUrl: './event-card.component.css',
})
export class EventCardComponent {
  @Input({ required: true }) event!: EventListResponse;
  @Input() imageBaseUrl = '';
  @Input() link: any[] | null = null;

  @Output() clicked = new EventEmitter<number>();

  readonly formatTime = formatTime;

  get posterSrc(): string {
    return this.event.posterUrl ? `${this.imageBaseUrl}/${this.event.posterUrl}` : '/images/default_event_image.png';
  }

  get hasPoster(): boolean {
    return !!this.event.posterUrl;
  }
}
