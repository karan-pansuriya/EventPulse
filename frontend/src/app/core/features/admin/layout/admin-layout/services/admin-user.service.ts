import { inject, Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseHttpService } from '../../../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../../../shared/models/api-response.model';
import { PagedResult } from '../../../../../../shared/models/paged-result.model';
import { OrganizerResponse, UserListResponse, RoleResponse } from '../models/adminuser.model';

@Injectable({ providedIn: 'root' })
export class AdminUserService {
  private http = inject(BaseHttpService);

  getAllUsers(
    pageNumber: number,
    pageSize: number,
    roleId?: number,
  ): Observable<ApiResponse<PagedResult<UserListResponse>>> {
    let params = new HttpParams().set('PageNumber', pageNumber).set('PageSize', pageSize);

    if (roleId) {
      params = params.set('roleId', roleId);
    }

    return this.http.get<PagedResult<UserListResponse>>('users', { params });
  }

  getOrganizers(): Observable<ApiResponse<OrganizerResponse[]>> {
    return this.http.get<OrganizerResponse[]>('users/organizers');
  }

  deleteUser(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<null>(`users/${id}`);
  }

  removeUserRoles(userId: number, roleIds: number[]): Observable<ApiResponse<null>> {
    return this.http.put<null>(`users/${userId}/remove-roles`, { roleIds });
  }

  getRoles(): Observable<ApiResponse<RoleResponse[]>> {
    return this.http.get<RoleResponse[]>('Auth/roles');
  }
}
