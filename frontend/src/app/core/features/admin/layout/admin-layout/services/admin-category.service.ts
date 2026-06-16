import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpService } from '../../../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../../../shared/models/api-response.model';
import { Category, CreateCategoryRequest, UpdateCategoryRequest } from '../models/category.models';

@Injectable({ providedIn: 'root' })
export class AdminCategoryService {
  private http = inject(BaseHttpService);

  getAllCategoris(): Observable<ApiResponse<Category[]>> {
    return this.http.get<Category[]>('categories');
  }

  createCategory(data: CreateCategoryRequest): Observable<ApiResponse<Category>> {
    return this.http.post<Category>('categories', data);
  }

  updateCategory(id: number, data: UpdateCategoryRequest): Observable<ApiResponse<Category>> {
    return this.http.put<Category>(`categories/${id}`, data);
  }

  deleteCategory(id: number): Observable<ApiResponse<null>> {
    return this.http.delete<null>(`categories/${id}`);
  }
}
