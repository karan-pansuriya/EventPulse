import { HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpService } from '../../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../../shared/models/api-response.model';
import { PagedResult } from '../../../../../shared/models/paged-result.model';
import { OrganizerDashboardData, MonthlyRevenue, TopBookedEvent } from '../models/dashboard.models';

@Injectable({ providedIn: 'root' })
export class OrganizerDashboardService {
  private http = inject(BaseHttpService);

  getDashboard(): Observable<ApiResponse<OrganizerDashboardData>> {
    return this.http.get<OrganizerDashboardData>('events/dashboard');
  }

  getRevenueTrend(period: string = 'year'): Observable<ApiResponse<MonthlyRevenue[]>> {
    return this.http.get<MonthlyRevenue[]>(`events/dashboard/revenue-trend?period=${period}`);
  }

  getTopBookedEvents(
    pageNumber: number,
    pageSize: number,
  ): Observable<ApiResponse<PagedResult<TopBookedEvent>>> {
    const params = new HttpParams()
      .set('PageNumber', pageNumber)
      .set('PageSize', pageSize);
    return this.http.get<PagedResult<TopBookedEvent>>('events/top-booked', { params });
  }
}
