import { inject, Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseHttpService } from '../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../shared/models/api-response.model';
import { PagedResult } from '../../../../shared/models/paged-result.model';

export interface OrganizerResponse {
  id: number;
  name: string;
  email: string;
}

export interface UserListResponse {
  id: number;
  name: string;
  email: string;
  phone: string | null;
  isActive: boolean;
  roles: string[];
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class AdminUserService {
  private http = inject(BaseHttpService);

  getAllUsers(pageNumber: number, pageSize: number, role?: string): Observable<ApiResponse<PagedResult<UserListResponse>>> {
    let params = new HttpParams()
      .set('PageNumber', pageNumber)
      .set('PageSize', pageSize);

    if (role) {
      params = params.set('role', role);
    }

    return this.http.get<PagedResult<UserListResponse>>('users', { params });
  }

  getOrganizers(): Observable<ApiResponse<OrganizerResponse[]>> {
    return this.http.get<OrganizerResponse[]>('users/organizers');
  }

  deleteUser(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<null>(`users/${id}`);
  }
}
