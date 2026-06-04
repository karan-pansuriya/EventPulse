import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpService } from '../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../shared/models/api-response.model';
import { OrganizerDashboardData, MonthlyRevenue } from '../models/dashboard.models';

@Injectable({ providedIn: 'root' })
export class OrganizerDashboardService {
  private http = inject(BaseHttpService);

  getDashboard(): Observable<ApiResponse<OrganizerDashboardData>> {
    return this.http.get<OrganizerDashboardData>('events/dashboard');
  }

  getRevenueTrend(period: string = 'year'): Observable<ApiResponse<MonthlyRevenue[]>> {
    return this.http.get<MonthlyRevenue[]>(`events/dashboard/revenue-trend?period=${period}`);
  }
}
