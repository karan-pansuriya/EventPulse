import { HttpContextToken } from '@angular/common/http';

export const SHOW_SUCCESS = new HttpContextToken<boolean>(() => true);
export const SHOW_ERROR = new HttpContextToken<boolean>(() => true);