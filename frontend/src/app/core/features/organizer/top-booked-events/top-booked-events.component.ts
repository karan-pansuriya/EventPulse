import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { GridComponent, GridColumn } from '../../../../shared/components/grid/grid.component';
import { OrganizerDashboardService } from '../layout/services/organizer-dashboard.service';
import { TopBookedEvent } from '../layout/models/dashboard.models';

@Component({
  selector: 'app-top-booked-events',
  standalone: true,
  imports: [CommonModule, RouterLink, GridComponent],
  templateUrl: './top-booked-events.component.html',
  styleUrl: './top-booked-events.component.css',
})
export class TopBookedEventsComponent implements OnInit {
  private dashboardService = inject(OrganizerDashboardService);
  private cdr = inject(ChangeDetectorRef);

  events: TopBookedEvent[] = [];
  totalRecords = 0;
  currentPage = 1;
  pageSize = 10;
  loading = false;

  columns: GridColumn[] = [
    { header: 'Title', field: 'title', type: 'truncate', width: '250px' },
    { header: 'Bookings', field: 'totalBookings' },
    { header: 'Revenue', field: 'revenueGenerated', type: 'currency' },
    { header: 'Date', field: 'eventDate', type: 'date' },
    { header: 'Organizer', field: 'organizerName', type: 'truncate' },
  ];

  ngOnInit(): void {
    this.loadEvents();
  }

  loadEvents(): void {
    this.loading = true;
    this.dashboardService.getTopBookedEvents(this.currentPage, this.pageSize).subscribe({
      next: (res) => {
        if (res.data) {
          this.events = res.data.items;
          this.totalRecords = res.data.totalCount;
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadEvents();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadEvents();
  }
}
