import { inject, Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseHttpService } from '../../../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../../../shared/models/api-response.model';
import { PagedResult } from '../../../../../../shared/models/paged-result.model';
import { EventListResponse } from '../models/event.models';

@Injectable({ providedIn: 'root' })
export class AdminEventService {
  private http = inject(BaseHttpService);

  getAllEvents(
    pageNumber: number,
    pageSize: number,
    sortBy?: string,
    sortDirection?: string,
  ): Observable<ApiResponse<PagedResult<EventListResponse>>> {
    let params = new HttpParams().set('PageNumber', pageNumber).set('PageSize', pageSize);

    if (sortBy) params = params.set('SortBy', sortBy);
    if (sortDirection) params = params.set('SortDirection', sortDirection);

    return this.http.get<PagedResult<EventListResponse>>('events/admin/all', { params });
  }

  toggleVerification(id: number): Observable<ApiResponse<null>> {
    return this.http.put<null>(`events/${id}/verify`, {});
  }

  deleteEvent(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<null>(`events/${id}`);
  }
}
