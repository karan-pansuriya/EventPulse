import { inject, Injectable } from '@angular/core';
import { Observable, shareReplay } from 'rxjs';
import { BaseHttpService } from '../../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../../shared/models/api-response.model';

@Injectable({ providedIn: 'root' })
export class CityService {
  private http = inject(BaseHttpService);
  private cache$: Observable<ApiResponse<string[]>> | null = null;

  getAll(): Observable<ApiResponse<string[]>> {
    if (!this.cache$) {
      this.cache$ = this.http.get<string[]>('venues/cities').pipe(shareReplay(1));
    }
    return this.cache$;
  }
}
