import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpService } from '../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../shared/models/api-response.model';
import { MonthlyRevenue, OrganizerDashboardData } from '../../organizer/models/dashboard.models';

@Injectable({ providedIn: 'root' })
export class AdminDashboardService {
  private http = inject(BaseHttpService);

  getDashboard(organizerId?: number): Observable<ApiResponse<OrganizerDashboardData>> {
    let endpoint = 'events/admin/dashboard';
    if (organizerId) {
      endpoint += `?organizerId=${organizerId}`;
    }
    return this.http.get<OrganizerDashboardData>(endpoint);
  }

  getRevenueTrend(period: string = 'year', organizerId?: number): Observable<ApiResponse<MonthlyRevenue[]>> {
    let endpoint = `events/admin/dashboard/revenue-trend?period=${period}`;
    if (organizerId) {
      endpoint += `&organizerId=${organizerId}`;
    }
    return this.http.get<MonthlyRevenue[]>(endpoint);
  }
}