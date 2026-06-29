import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { BaseHttpService } from '../../services/base-http.service';

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
  private http = inject(BaseHttpService);
  private apiUrl = 'profile';

  getProfile(): Observable<UserDto> {
    return this.http.get<UserDto>(this.apiUrl).pipe(map((res) => res.data!));
  }

  updateProfile(request: UpdateUserRequest): Observable<UserDto> {
    return this.http.put<UserDto>(this.apiUrl, request).pipe(map((res) => res.data!));
  }
}
