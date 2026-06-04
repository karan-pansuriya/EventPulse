import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  inject,
  NgZone,
  OnInit,
  PLATFORM_ID,
} from '@angular/core';
import { isPlatformBrowser, CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';
import { Chart, registerables } from 'chart.js';
import { OrganizerDashboardService } from '../services/organizer-dashboard.service';
import { OrganizerDashboardData } from '../models/dashboard.models';
import { ToastService } from '../../../../shared/services/toast.service';

Chart.register(...registerables);

@Component({
  selector: 'app-organizer-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class OrganizerDashboard implements OnInit, AfterViewInit {
  private dashboardService = inject(OrganizerDashboardService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);
  private zone = inject(NgZone);
  private platformId = inject(PLATFORM_ID);

  data: OrganizerDashboardData | null = null;
  isLoading = true;
  selectedPeriod: 'week' | 'month' | 'year' = 'year';

  private revenueChart: Chart | null = null;
  private categoryChart: Chart | null = null;

  ngOnInit(): void {
    this.loadDashboard();
  }

  ngAfterViewInit(): void {
    this.renderCharts();
  }

  setPeriod(period: 'week' | 'month' | 'year'): void {
    this.selectedPeriod = period;
    this.destroyRevenueChart();
    this.loadRevenueTrend(period);
  }

  private loadDashboard(period: string = 'year'): void {
    forkJoin({
      dashboard: this.dashboardService.getDashboard(),
      revenue: this.dashboardService.getRevenueTrend(period),
    }).subscribe({
      next: (res) => {
        this.data = res.dashboard.data ?? null;
        if (this.data && res.revenue.data) {
          this.data.monthlyRevenue = res.revenue.data;
        }
        this.isLoading = false;
        this.cdr.detectChanges();
        this.renderCharts();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
        this.toast.error('Failed to load dashboard data.', 'Error');
      },
    });
  }

  private loadRevenueTrend(period: string = 'year'): void {
    this.dashboardService.getRevenueTrend(period).subscribe({
      next: (res) => {
        if (this.data && res.data) {
          this.data.monthlyRevenue = res.data;
        }
        this.cdr.detectChanges();
        this.createRevenueChart();
      },
      error: () => {
        this.toast.error('Failed to load revenue trend.', 'Error');
      },
    });
  }

  private renderCharts(): void {
    if (!this.data || !isPlatformBrowser(this.platformId)) return;

    this.zone.runOutsideAngular(() => {
      setTimeout(() => {
        this.destroyCharts();
        this.createRevenueChart();
        this.createCategoryChart();
      });
    });
  }

  private destroyCharts(): void {
    this.revenueChart?.destroy();
    this.categoryChart?.destroy();
    this.revenueChart = null;
    this.categoryChart = null;
  }

  private destroyRevenueChart(): void {
    this.revenueChart?.destroy();
    this.revenueChart = null;
  }

  private createRevenueChart(): void {
    const canvas = document.getElementById('revenueChart') as HTMLCanvasElement | null;
    if (!canvas || !this.data) return;

    const months = this.data.monthlyRevenue.map((m) => {
      if (/^\d{4}-\d{2}$/.test(m.month)) {
        const [, month] = m.month.split('-');
        const names = [
          'Jan',
          'Feb',
          'Mar',
          'Apr',
          'May',
          'Jun',
          'Jul',
          'Aug',
          'Sep',
          'Oct',
          'Nov',
          'Dec',
        ];
        return names[parseInt(month) - 1] || m.month;
      }
      return m.month;
    });
    const revenues = this.data.monthlyRevenue.map((m) => Number(m.revenue));
    const bookings = this.data.monthlyRevenue.map((m) => m.bookings);

    this.revenueChart = new Chart(canvas, {
      type: 'bar',
      data: {
        labels: months,
        datasets: [
          {
            label: 'Revenue (₹)',
            data: revenues,
            backgroundColor: 'rgba(225, 29, 72, 0.7)',
            borderRadius: 0,
            borderSkipped: false,
            yAxisID: 'y',
          },
          {
            label: 'Bookings',
            data: bookings,
            backgroundColor: 'rgba(139, 92, 246, 0.65)',
            borderRadius: 0,
            borderSkipped: false,
            yAxisID: 'y1',
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: {
          duration: 800,
          easing: 'easeOutQuart',
        },
        interaction: {
          mode: 'index',
          intersect: false,
        },
        plugins: {
          legend: {
            position: 'top',
            labels: { boxWidth: 12, padding: 12, font: { size: 11 } },
          },
          tooltip: {
            backgroundColor: 'rgba(17,24,39,0.9)',
            titleFont: { size: 12 },
            bodyFont: { size: 11 },
            padding: 10,
            cornerRadius: 8,
          },
        },
        scales: {
          y: {
            beginAtZero: true,
            position: 'left',
            ticks: { font: { size: 10 } },
            grid: { color: 'rgba(0,0,0,0.03)' },
          },
          y1: {
            beginAtZero: true,
            position: 'right',
            ticks: { font: { size: 10 } },
            grid: { display: false },
          },
        },
      },
    });
  }

  private createCategoryChart(): void {
    const canvas = document.getElementById('categoryChart') as HTMLCanvasElement | null;
    if (!canvas || !this.data) return;

    const labels = this.data.eventsByCategory.map((c) => c.categoryName);
    const counts = this.data.eventsByCategory.map((c) => c.count);
    const colors = [
      '#f43f5e',
      '#f59e0b',
      '#10b981',
      '#8b5cf6',
      '#06b6d4',
      '#ec4899',
      '#f97316',
      '#14b8a6',
    ];

    this.categoryChart = new Chart(canvas, {
      type: 'doughnut',
      data: {
        labels,
        datasets: [
          {
            data: counts,
            backgroundColor: colors.slice(0, labels.length),
            borderWidth: 1,
            borderColor: '#fff',
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'bottom',
            labels: { boxWidth: 12, padding: 10, font: { size: 10 } },
          },
        },
        cutout: '55%',
      },
    });
  }

  abs(value: number): number {
    return Math.abs(value);
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(value);
  }
}
