import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpService } from '../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../shared/models/api-response.model';
import { OrganizerDashboardData } from '../../organizer/models/dashboard.models';

@Injectable({ providedIn: 'root' })
export class AdminDashboardService {
  private http = inject(BaseHttpService);

  getDashboard(period: string = 'year', organizerId?: number): Observable<ApiResponse<OrganizerDashboardData>> {
    let endpoint = `events/admin/dashboard?period=${period}`;
    if (organizerId) {
      endpoint += `&organizerId=${organizerId}`;
    }
    return this.http.get<OrganizerDashboardData>(endpoint);
  }
}