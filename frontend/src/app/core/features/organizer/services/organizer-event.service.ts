import { inject, Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseHttpService } from '../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../shared/models/api-response.model';
import { PagedResult } from '../../../../shared/models/paged-result.model';
import { EventListResponse, EventDetailResponse } from '../../attendee/home/models/event.models';
import { EventAttendee, CheckInResponse } from '../models/attendee.models';
import { CreateEventRequest, UpdateEventRequest } from '../models/event.models';

@Injectable({ providedIn: 'root' })
export class OrganizerEventService {
  private http = inject(BaseHttpService);

  getMyEvents(pageNumber: number, pageSize: number): Observable<ApiResponse<PagedResult<EventListResponse>>> {
    const params = new HttpParams()
      .set('PageNumber', pageNumber)
      .set('PageSize', pageSize);

    return this.http.get<PagedResult<EventListResponse>>('events/my-events', { params });
  }

  getEventById(id: number): Observable<ApiResponse<EventDetailResponse>> {
    return this.http.get<EventDetailResponse>(`events/${id}`);
  }

  createEvent(formData: FormData): Observable<ApiResponse<EventDetailResponse>> {
    return this.http.post<EventDetailResponse>('events', formData);
  }

  updateEvent(id: number, formData: FormData): Observable<ApiResponse<EventDetailResponse>> {
    return this.http.put<EventDetailResponse>(`events/${id}`, formData);
  }

  deleteEvent(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<null>(`events/${id}`);
  }

  getAttendees(): Observable<ApiResponse<EventAttendee[]>> {
    return this.http.get<EventAttendee[]>('events/attendees');
  }

  checkIn(ticketCode: string): Observable<ApiResponse<CheckInResponse>> {
    return this.http.post<CheckInResponse>('tickets/check-in', { ticketCode });
  }
}
