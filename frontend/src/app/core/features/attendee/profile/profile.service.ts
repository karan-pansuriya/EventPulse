import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { ApiResponse } from '../../../../shared/models/api-response.model';

export interface UserDto {
  id: number;
  name: string;
  email: string;
  phone: string | null;
  createdAt: string;
}

export interface UpdateUserRequest {
  name: string;
  phone: string | null;
}

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/profile`;

  getProfile(): Observable<UserDto> {
    return this.http.get<ApiResponse<UserDto>>(this.apiUrl).pipe(map(res => res.data!));
  }

  updateProfile(request: UpdateUserRequest): Observable<UserDto> {
    return this.http.put<ApiResponse<UserDto>>(this.apiUrl, request).pipe(map(res => res.data!));
  }
}
