import { HttpContextToken } from '@angular/common/http';

export const SHOW_SUCCESS = new HttpContextToken<boolean>(() => false);
export const SHOW_ERROR = new HttpContextToken<boolean>(() => true);