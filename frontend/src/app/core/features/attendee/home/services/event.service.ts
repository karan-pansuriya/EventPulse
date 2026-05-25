import { inject, Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseHttpService } from '../../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../../shared/models/api-response.model';
import { EventFilterRequest, EventListResponse } from '../models/event.models';
import { PagedResult } from '../../../../../shared/models/paged-result.model';

@Injectable({ providedIn: 'root' })
export class EventService {
  private http = inject(BaseHttpService);

  getEvents(filters: EventFilterRequest): Observable<ApiResponse<PagedResult<EventListResponse>>> {
    let params = new HttpParams()
      .set('PageNumber', filters.pageNumber)
      .set('PageSize', filters.pageSize);

    if (filters.search) params = params.set('Search', filters.search);
    if (filters.categoryId) params = params.set('CategoryId', filters.categoryId);
    if (filters.city) params = params.set('City', filters.city);
    if (filters.dateFrom) params = params.set('DateFrom', filters.dateFrom);
    if (filters.dateTo) params = params.set('DateTo', filters.dateTo);
    if (filters.sortBy) params = params.set('SortBy', filters.sortBy);
    if (filters.sortDirection) params = params.set('SortDirection', filters.sortDirection);

    return this.http.get<PagedResult<EventListResponse>>('events', { params });
  }
}
