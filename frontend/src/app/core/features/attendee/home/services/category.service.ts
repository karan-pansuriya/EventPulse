import { inject, Injectable } from '@angular/core';
import { Observable, shareReplay } from 'rxjs';
import { BaseHttpService } from '../../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../../shared/models/api-response.model';
import { Category } from '../models/category.models';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private http = inject(BaseHttpService);
  private cache$: Observable<ApiResponse<Category[]>> | null = null;

  getAll(): Observable<ApiResponse<Category[]>> {
    if (!this.cache$) {
      this.cache$ = this.http.get<Category[]>('categories').pipe(shareReplay(1));
    }
    return this.cache$;
  }
}
