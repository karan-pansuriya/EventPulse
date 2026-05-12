import { HttpClient, HttpContext, HttpHeaders ,HttpParams} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApiResponse } from '../models/api-response.model';
import { environment } from '../../../environments/environment';
import { SHOW_ERROR, SHOW_SUCCESS } from '../tokens/http-context.tokens';

export interface ApiRequestOptions {
  params?: HttpParams | { [param: string]: string | number | boolean };
  headers?: HttpHeaders;
  showSuccess?: boolean;
  showError?: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class BaseHttpService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  private createContext(options?: ApiRequestOptions): HttpContext {
    let context = new HttpContext();

    if (options?.showSuccess !== undefined) {
      context = context.set(SHOW_SUCCESS, options.showSuccess);
    }

    if (options?.showError !== undefined) {
      context = context.set(SHOW_ERROR, options.showError);
    }

    return context;
  }

  get<T>(endpoint: string, options?: ApiRequestOptions) {
    return this.http.get<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, {
      params: options?.params,
      headers: options?.headers,
      context: this.createContext(options),
    });
  }

  post<T>(endpoint: string, data: any, options?: ApiRequestOptions) {
    return this.http.post<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, data, {
      headers: options?.headers,
      context: this.createContext(options),
    });
  }

  put<T>(endpoint: string, data: any, options?: ApiRequestOptions) {
    return this.http.put<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, data, {
      headers: options?.headers,
      context: this.createContext(options),
    });
  }

  patch<T>(endpoint: string, data: any, options?: ApiRequestOptions) {
    return this.http.patch<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, data, {
      headers: options?.headers,
      context: this.createContext(options),
    });
  }

  delete<T>(endpoint: string, options?: ApiRequestOptions) {
    return this.http.delete<ApiResponse<T>>(`${this.baseUrl}/${endpoint}`, {
      headers: options?.headers,
      context: this.createContext(options),
    });
  }
}
