import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpService } from '../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../shared/models/api-response.model';
import { OrganizerDashboardData } from '../models/dashboard.models';

@Injectable({ providedIn: 'root' })
export class OrganizerDashboardService {
  private http = inject(BaseHttpService);

  getDashboard(period: string = 'year'): Observable<ApiResponse<OrganizerDashboardData>> {
    return this.http.get<OrganizerDashboardData>(`events/dashboard?period=${period}`);
  }
}
