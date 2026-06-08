import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { OrganizerEventService } from '../layout/services/organizer-event.service';
import { ConfirmationModalComponent } from '../../../../shared/components/confirmation-modal/confirmation-modal.component';
import {
  GridComponent,
  GridColumn,
  GridActionItem,
} from '../../../../shared/components/grid/grid.component';
import { EventListResponse } from '../../attendee/home/models/event.models';
import { environment } from '../../../../../environments/environment';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-organizer-events',
  standalone: true,
  imports: [CommonModule, GridComponent, ConfirmationModalComponent],
  templateUrl: './events.component.html',
  styleUrl: './events.component.css',
})
export class EventsComponent implements OnInit {
  private eventService = inject(OrganizerEventService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  private toastService = inject(ToastService);

  private imageBaseUrl = environment.apiUrl.replace('/api', '');

  events: EventListResponse[] = [];
  totalCount = 0;
  pageNumber = 1;
  pageSize = 10;
  isLoading = true;
  hasError = false;
  showDeleteModal = false;
  deletingEvent: EventListResponse | null = null;

  /** Cast to Record<string, unknown>[] for GridComponent typings */
  get gridData(): Record<string, unknown>[] {
    return this.events as unknown as Record<string, unknown>[];
  }

  columns: GridColumn[] = [
    { header: 'Event', field: 'title', type: 'text' },
    { header: 'Category', field: 'categoryName' },
    { header: 'Venue', field: 'venueName' },
    { header: 'Date', field: 'eventDate', type: 'date' },
    { header: 'Time', field: 'startTime', formatter: (v) => this.formatTime(v as string) },
    { header: 'Price', field: 'price', type: 'currency' },
    { header: 'Seats', field: 'totalSeats' },
    {
      header: 'Status',
      field: 'isVerified',
      formatter: (v) =>
        v
          ? '<span class="badge bg-success">Verified</span>'
          : '<span class="badge bg-danger">Not Verified</span>',
    },
    { header: '', field: 'actions', type: 'action', width: '60px' },
  ];

  getRowActionItems = (row: Record<string, unknown>): GridActionItem[] => {
    const r = row as unknown as EventListResponse;
    const isPast = this.isPastEvent(r.eventDate);
    const items: GridActionItem[] = [];
    if (!r.isVerified && !isPast) {
      items.push({ label: 'Edit', icon: 'assets/icons/Edit.svg', emit: 'edit' });
    }
    if (!r.isVerified && !isPast) {
      items.push({ label: 'Delete', icon: 'assets/icons/Delete.svg', emit: 'delete' });
    }
    return items;
  };

  ngOnInit(): void {
    this.loadEvents();
  }

  private loadEvents(): void {
    this.isLoading = true;
    this.hasError = false;

    this.eventService.getMyEvents(this.pageNumber, this.pageSize).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.events = res.data.items;
          this.totalCount = res.data.totalCount;
        }
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.hasError = true;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  onPageChange(page: number): void {
    this.pageNumber = page;
    this.loadEvents();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.pageNumber = 1;
    this.loadEvents();
  }

  createEvent(): void {
    this.router.navigate(['/organizer/events/create']);
  }

  onEdit(event: Record<string, unknown>): void {
    const ev = event as unknown as EventListResponse;
    this.router.navigate(['/organizer/events/edit', ev.id]);
  }

  onDelete(event: Record<string, unknown>): void {
    this.deletingEvent = event as unknown as EventListResponse;
    this.showDeleteModal = true;
  }

  confirmDelete(): void {
    if (!this.deletingEvent) return;

    this.eventService.deleteEvent(this.deletingEvent.id).subscribe({
      next: () => {
        this.showDeleteModal = false;
        this.deletingEvent = null;
        if (this.events.length === 1 && this.pageNumber > 1) {
          this.pageNumber--;
        }
        this.loadEvents();
      },
      error: (err) => {
        this.showDeleteModal = false;
        this.deletingEvent = null;
        const message = err?.error?.message || 'Failed to delete event.';
        this.toastService.error(message);
        this.cdr.detectChanges();
      },
    });
  }

  cancelDelete(): void {
    this.showDeleteModal = false;
    this.deletingEvent = null;
  }

  get deleteMessage(): string {
    const title = this.deletingEvent?.title || '';
    return `Are you sure you want to delete "${title}"? This action cannot be undone.`;
  }

  private isPastEvent(eventDate: string): boolean {
    const today = new Date().toISOString().slice(0, 10);
    return eventDate < today;
  }

  private formatTime(timeStr: string): string {
    const [h, m] = timeStr.split(':');
    const hour = parseInt(h, 10);
    const ampm = hour >= 12 ? 'PM' : 'AM';
    const hour12 = hour % 12 || 12;
    return `${hour12}:${m} ${ampm}`;
  }
}
