import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseHttpService } from '../../../../../shared/services/base-http.service';
import { ApiResponse } from '../../../../../shared/models/api-response.model';
import { Country, State, City } from '../models/location.models';

@Injectable({ providedIn: 'root' })
export class LocationService {
  private http = inject(BaseHttpService);

  getCountries(): Observable<ApiResponse<Country[]>> {
    return this.http.get<Country[]>('locations/countries');
  }

  getStates(countryId: number): Observable<ApiResponse<State[]>> {
    return this.http.get<State[]>(`locations/countries/${countryId}/states`);
  }

  getCities(stateId: number): Observable<ApiResponse<City[]>> {
    return this.http.get<City[]>(`locations/states/${stateId}/cities`);
  }

  getCityById(cityId: number): Observable<ApiResponse<City>> {
    return this.http.get<City>(`locations/cities/${cityId}`);
  }
}
